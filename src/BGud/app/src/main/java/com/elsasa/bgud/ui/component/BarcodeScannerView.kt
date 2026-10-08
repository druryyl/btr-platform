package com.elsasa.bgud.ui.component

import androidx.camera.core.Camera
import androidx.camera.core.CameraSelector
import androidx.camera.core.FocusMeteringAction
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.compose.animation.core.FastOutSlowInEasing
import androidx.compose.animation.core.LinearEasing
import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.animateFloatAsState
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectTapGestures
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.CornerRadius
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Path
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.drawscope.Stroke
import androidx.compose.ui.hapticfeedback.HapticFeedbackType
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalHapticFeedback
import androidx.compose.ui.unit.dp
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.content.ContextCompat
import androidx.lifecycle.compose.LocalLifecycleOwner
import com.elsasa.bgud.ui.theme.BrandGreen
import com.elsasa.bgud.ui.theme.BrandGreenDark
import com.elsasa.bgud.ui.theme.BrandWarmCream
import com.google.mlkit.vision.barcode.Barcode
import com.google.mlkit.vision.barcode.BarcodeScannerOptions
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.common.InputImage
import kotlinx.coroutines.delay
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit

/**
 * Camera acquisition component (SCR-MOB-003 viewfinder, Architecture §20).
 *
 * Camera acquisition is isolated behind this single component so the domain
 * stays scanner-agnostic (P-10): callers receive a plain barcode string via
 * [onBarcode] and never touch CameraX or ML Kit types.
 *
 * Stack: CameraX preview + ML Kit Barcode Scanning (TQ-7).
 * - Target resolution: HD 720p (1280x720) for optimal 1D barcode edge density.
 * - Formats restricted to retail & warehouse standards (EAN, UPC, Code 128, Code 39, QR).
 * - Interactive tap-to-focus with visual ring indicator.
 * - Torch (flashlight) toggle control for low-light warehouse environments.
 * - Viewfinder reticle overlay with animated scanline guide.
 * - Haptic confirmation upon successful scan.
 * - 7-second liveness timeout callback [onScanTimeout] to trigger operational guidance.
 *
 * Frames are analyzed with `STRATEGY_KEEP_ONLY_LATEST`; analysis runs only while
 * [enabled] (single-scan mode — the caller disables it after resolution
 * and re-enables it to re-arm, §20). The ML Kit client and the camera
 * executors are released when the composition leaves.
 */
@Composable
fun BarcodeScannerView(
    enabled: Boolean,
    onBarcode: (String) -> Unit,
    modifier: Modifier = Modifier,
    onScanTimeout: (() -> Unit)? = null
) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current
    val hapticFeedback = LocalHapticFeedback.current

    // Camera reference and PreviewView state for tap-to-focus and torch controls
    var cameraInstance by remember { mutableStateOf<Camera?>(null) }
    var previewViewInstance by remember { mutableStateOf<PreviewView?>(null) }
    var isTorchOn by rememberSaveable { mutableStateOf(false) }
    var focusPoint by remember { mutableStateOf<Offset?>(null) }
    var scanResetKey by remember { mutableIntStateOf(0) }

    // Latest callback wins; the analyzer reads the current value per frame.
    val latestOnBarcode = remember { Ref<(String) -> Unit>(onBarcode) }
    latestOnBarcode.value = onBarcode
    val latestEnabled = remember { Ref(enabled) }
    latestEnabled.value = enabled
    val latestOnScanTimeout = rememberUpdatedState(onScanTimeout)

    // Optimized ML Kit barcode detector: restricted to retail & warehouse formats
    val scanner = remember {
        BarcodeScanning.getClient(
            BarcodeScannerOptions.Builder()
                .setBarcodeFormats(
                    Barcode.FORMAT_EAN_13,
                    Barcode.FORMAT_EAN_8,
                    Barcode.FORMAT_UPC_A,
                    Barcode.FORMAT_UPC_E,
                    Barcode.FORMAT_CODE_128,
                    Barcode.FORMAT_CODE_39,
                    Barcode.FORMAT_QR_CODE
                )
                .build()
        )
    }

    // Sync torch state with CameraControl
    LaunchedEffect(isTorchOn, cameraInstance) {
        cameraInstance?.let { cam ->
            try {
                if (cam.cameraInfo.hasFlashUnit()) {
                    cam.cameraControl.enableTorch(isTorchOn)
                }
            } catch (_: Exception) {
                // Flashlight hardware not supported or unavailable
            }
        }
    }

    // Automatically turn torch off when scanner is disabled
    LaunchedEffect(enabled) {
        if (!enabled && isTorchOn) {
            isTorchOn = false
            try {
                cameraInstance?.cameraControl?.enableTorch(false)
            } catch (_: Exception) {
            }
        }
    }

    // Clean up torch when leaving composition
    DisposableEffect(cameraInstance) {
        onDispose {
            try {
                cameraInstance?.cameraControl?.enableTorch(false)
            } catch (_: Exception) {
            }
        }
    }

    // Clear focus ring after ~1 second
    LaunchedEffect(focusPoint) {
        if (focusPoint != null) {
            delay(1000L)
            focusPoint = null
        }
    }

    // 7-second liveness timeout: triggers onScanTimeout if no barcode is detected within 7s
    LaunchedEffect(enabled, scanResetKey) {
        if (enabled && latestOnScanTimeout.value != null) {
            delay(7000L)
            latestOnScanTimeout.value?.invoke()
        }
    }

    Box(modifier = modifier) {
        AndroidView(
            modifier = Modifier.fillMaxSize(),
            factory = { factoryContext ->
                val previewView = PreviewView(factoryContext).also {
                    previewViewInstance = it
                }
                val cameraExecutor = Executors.newSingleThreadExecutor()
                val mainExecutor = ContextCompat.getMainExecutor(factoryContext)
                val cameraProviderFuture = ProcessCameraProvider.getInstance(factoryContext)

                cameraProviderFuture.addListener(
                    {
                        val cameraProvider = cameraProviderFuture.get()
                        val preview = Preview.Builder().build().also {
                            it.setSurfaceProvider(previewView.surfaceProvider)
                        }
                        val imageAnalysis = ImageAnalysis.Builder()
                            .setTargetResolution(android.util.Size(1280, 720))
                            .setBackpressureStrategy(ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST)
                            .build()
                            .also { analysis ->
                                analysis.setAnalyzer(cameraExecutor) { imageProxy ->
                                    if (!latestEnabled.value) {
                                        imageProxy.close()
                                        return@setAnalyzer
                                    }
                                    val mediaImage = imageProxy.image
                                    if (mediaImage == null) {
                                        imageProxy.close()
                                        return@setAnalyzer
                                    }
                                    val inputImage = InputImage.fromMediaImage(
                                        mediaImage,
                                        imageProxy.imageInfo.rotationDegrees
                                    )
                                    scanner.process(inputImage)
                                        .addOnSuccessListener(mainExecutor) { barcodes ->
                                            val rawValue = barcodes
                                                .firstOrNull { !it.rawValue.isNullOrEmpty() }
                                                ?.rawValue
                                            if (rawValue != null && latestEnabled.value) {
                                                try {
                                                    hapticFeedback.performHapticFeedback(HapticFeedbackType.LongPress)
                                                } catch (_: Exception) {
                                                    // Best-effort haptic feedback
                                                }
                                                scanResetKey++
                                                latestOnBarcode.value(rawValue)
                                            }
                                        }
                                        .addOnCompleteListener { imageProxy.close() }
                                }
                            }
                        try {
                            cameraProvider.unbindAll()
                            val cam = cameraProvider.bindToLifecycle(
                                lifecycleOwner,
                                CameraSelector.DEFAULT_BACK_CAMERA,
                                preview,
                                imageAnalysis
                            )
                            cameraInstance = cam
                        } catch (_: Exception) {
                            // Binding failures surface as an absent preview; the
                            // manual entry fallback stays available (§12.5).
                        }
                    },
                    mainExecutor
                )

                previewView.apply {
                    tag = cameraExecutor
                }
            },
            onRelease = { previewView ->
                try {
                    cameraInstance?.cameraControl?.enableTorch(false)
                } catch (_: Exception) {
                }
                cameraInstance = null
                previewViewInstance = null
                try {
                    val providerFuture = ProcessCameraProvider.getInstance(context)
                    // Never block dispose on the camera provider; unbind only
                    // when it is already available.
                    if (providerFuture.isDone) providerFuture.get().unbindAll()
                } catch (_: Exception) {
                    // Best-effort release on dispose.
                }
                (previewView.tag as? java.util.concurrent.ExecutorService)?.shutdown()
                scanner.close()
            }
        )

        // Viewfinder reticle overlay & tap-to-focus gesture handler
        Box(
            modifier = Modifier
                .fillMaxSize()
                .pointerInput(cameraInstance, previewViewInstance, enabled) {
                    detectTapGestures { offset ->
                        val cam = cameraInstance
                        val pv = previewViewInstance
                        if (enabled && cam != null && pv != null) {
                            focusPoint = offset
                            try {
                                val factory = pv.meteringPointFactory
                                val point = factory.createPoint(offset.x, offset.y)
                                val action = FocusMeteringAction.Builder(point, FocusMeteringAction.FLAG_AF)
                                    .setAutoCancelDuration(3, TimeUnit.SECONDS)
                                    .build()
                                cam.cameraControl.startFocusAndMetering(action)
                            } catch (_: Exception) {
                                // Best-effort focus and metering
                            }
                        }
                    }
                }
        ) {
            // Viewfinder reticle corners and animated scan line
            ScannerReticleOverlay(isScanning = enabled)

            // Animated focus ring at tap location
            focusPoint?.let { pt ->
                FocusRingIndicator(center = pt)
            }
        }

        // Torch (flashlight) toggle button
        TorchToggleButton(
            isTorchOn = isTorchOn,
            onToggle = { isTorchOn = !isTorchOn },
            modifier = Modifier
                .align(Alignment.TopEnd)
                .padding(10.dp)
        )
    }
}

/**
 * Modern industrial reticle frame with corner brackets and animated sweep line.
 */
@Composable
private fun ScannerReticleOverlay(isScanning: Boolean) {
    val infiniteTransition = rememberInfiniteTransition(label = "scan_line")
    val scanLineProgress by infiniteTransition.animateFloat(
        initialValue = 0f,
        targetValue = 1f,
        animationSpec = infiniteRepeatable(
            animation = tween(durationMillis = 2000, easing = LinearEasing),
            repeatMode = RepeatMode.Reverse
        ),
        label = "scan_line_progress"
    )

    Canvas(modifier = Modifier.fillMaxSize()) {
        val w = size.width
        val h = size.height

        val boxWidth = (w * 0.72f).coerceIn(160.dp.toPx(), 280.dp.toPx()).coerceAtMost(w - 24.dp.toPx())
        val boxHeight = (h * 0.58f).coerceIn(90.dp.toPx(), 160.dp.toPx()).coerceAtMost(h - 24.dp.toPx())
        val left = (w - boxWidth) / 2f
        val top = (h - boxHeight) / 2f
        val right = left + boxWidth
        val bottom = top + boxHeight

        val cornerLength = 20.dp.toPx()
        val cornerStroke = 3.dp.toPx()
        val cornerShadowStroke = 4.5.dp.toPx()
        val cornerColor = BrandGreen
        val shadowColor = Color.Black.copy(alpha = 0.35f)

        // Draw drop-shadow for corner brackets to ensure high contrast
        // Top-Left corner
        drawLine(shadowColor, Offset(left, top + cornerLength), Offset(left, top), cornerShadowStroke)
        drawLine(shadowColor, Offset(left, top), Offset(left + cornerLength, top), cornerShadowStroke)

        // Top-Right corner
        drawLine(shadowColor, Offset(right - cornerLength, top), Offset(right, top), cornerShadowStroke)
        drawLine(shadowColor, Offset(right, top), Offset(right, top + cornerLength), cornerShadowStroke)

        // Bottom-Left corner
        drawLine(shadowColor, Offset(left, bottom - cornerLength), Offset(left, bottom), cornerShadowStroke)
        drawLine(shadowColor, Offset(left, bottom), Offset(left + cornerLength, bottom), cornerShadowStroke)

        // Bottom-Right corner
        drawLine(shadowColor, Offset(right - cornerLength, bottom), Offset(right, bottom), cornerShadowStroke)
        drawLine(shadowColor, Offset(right, bottom), Offset(right, bottom - cornerLength), cornerShadowStroke)

        // Foreground BrandGreen corner brackets
        // Top-Left corner
        drawLine(cornerColor, Offset(left, top + cornerLength), Offset(left, top), cornerStroke)
        drawLine(cornerColor, Offset(left, top), Offset(left + cornerLength, top), cornerStroke)

        // Top-Right corner
        drawLine(cornerColor, Offset(right - cornerLength, top), Offset(right, top), cornerStroke)
        drawLine(cornerColor, Offset(right, top), Offset(right, top + cornerLength), cornerStroke)

        // Bottom-Left corner
        drawLine(cornerColor, Offset(left, bottom - cornerLength), Offset(left, bottom), cornerStroke)
        drawLine(cornerColor, Offset(left, bottom), Offset(left + cornerLength, bottom), cornerStroke)

        // Bottom-Right corner
        drawLine(cornerColor, Offset(right - cornerLength, bottom), Offset(right, bottom), cornerStroke)
        drawLine(cornerColor, Offset(right, bottom), Offset(right, bottom - cornerLength), cornerStroke)

        // Subtle center guideline
        drawLine(
            color = Color.White.copy(alpha = 0.3f),
            start = Offset(left + 14.dp.toPx(), (top + bottom) / 2f),
            end = Offset(right - 14.dp.toPx(), (top + bottom) / 2f),
            strokeWidth = 1.dp.toPx()
        )

        // Animated horizontal scanline
        if (isScanning) {
            val scanY = top + 6.dp.toPx() + (boxHeight - 12.dp.toPx()) * scanLineProgress
            drawLine(
                color = BrandGreen.copy(alpha = 0.85f),
                start = Offset(left + 8.dp.toPx(), scanY),
                end = Offset(right - 8.dp.toPx(), scanY),
                strokeWidth = 2.dp.toPx(),
                cap = StrokeCap.Round
            )
        }
    }
}

/**
 * Visual focus indicator ring shown at touch coordinates.
 */
@Composable
private fun FocusRingIndicator(center: Offset) {
    var isExpanded by remember(center) { mutableStateOf(false) }
    LaunchedEffect(center) {
        isExpanded = true
    }
    val animatedScale by animateFloatAsState(
        targetValue = if (isExpanded) 1.0f else 1.4f,
        animationSpec = tween(durationMillis = 200, easing = FastOutSlowInEasing),
        label = "focus_scale"
    )

    Canvas(modifier = Modifier.fillMaxSize()) {
        val radius = 28.dp.toPx() * animatedScale
        // Outer contrast shadow ring
        drawCircle(
            color = Color.Black.copy(alpha = 0.35f),
            radius = radius,
            center = center,
            style = Stroke(width = 3.5.dp.toPx())
        )
        // Outer bright ring
        drawCircle(
            color = Color.White.copy(alpha = 0.95f),
            radius = radius,
            center = center,
            style = Stroke(width = 2.dp.toPx())
        )
        // Center crosshair dot
        drawCircle(
            color = BrandGreen,
            radius = 3.5.dp.toPx(),
            center = center
        )
    }
}

/**
 * Torch toggle button overlay for the scanner viewfinder.
 */
@Composable
private fun TorchToggleButton(
    isTorchOn: Boolean,
    onToggle: () -> Unit,
    modifier: Modifier = Modifier
) {
    Surface(
        onClick = onToggle,
        modifier = modifier.size(38.dp),
        shape = CircleShape,
        color = if (isTorchOn) BrandWarmCream else Color.Black.copy(alpha = 0.5f),
        border = BorderStroke(
            1.dp,
            if (isTorchOn) BrandGreen else Color.White.copy(alpha = 0.35f)
        ),
        shadowElevation = 2.dp
    ) {
        Box(
            modifier = Modifier.fillMaxSize(),
            contentAlignment = Alignment.Center
        ) {
            FlashlightIcon(
                isOn = isTorchOn,
                tint = if (isTorchOn) BrandGreenDark else Color.White,
                modifier = Modifier.size(20.dp)
            )
        }
    }
}

/**
 * Custom vector flashlight icon with active beam rays.
 */
@Composable
private fun FlashlightIcon(
    isOn: Boolean,
    tint: Color,
    modifier: Modifier = Modifier
) {
    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val sw = 1.6f * density

        // Bezel / Head (trapezoid at top)
        val headPath = Path().apply {
            moveTo(w * 0.30f, h * 0.40f)
            lineTo(w * 0.22f, h * 0.24f)
            lineTo(w * 0.78f, h * 0.24f)
            lineTo(w * 0.70f, h * 0.40f)
            close()
        }
        drawPath(headPath, color = tint, style = Stroke(width = sw))

        // Body / Handle
        drawRoundRect(
            color = tint,
            topLeft = Offset(w * 0.34f, h * 0.40f),
            size = Size(w * 0.32f, h * 0.42f),
            cornerRadius = CornerRadius(2f * density, 2f * density),
            style = Stroke(width = sw)
        )

        // Switch button
        drawLine(
            color = tint,
            start = Offset(w * 0.50f, h * 0.50f),
            end = Offset(w * 0.50f, h * 0.60f),
            strokeWidth = sw * 1.5f,
            cap = StrokeCap.Round
        )

        // Light beam rays when ON
        if (isOn) {
            drawLine(
                color = tint,
                start = Offset(w * 0.50f, h * 0.17f),
                end = Offset(w * 0.50f, h * 0.05f),
                strokeWidth = sw,
                cap = StrokeCap.Round
            )
            drawLine(
                color = tint,
                start = Offset(w * 0.26f, h * 0.18f),
                end = Offset(w * 0.14f, h * 0.08f),
                strokeWidth = sw,
                cap = StrokeCap.Round
            )
            drawLine(
                color = tint,
                start = Offset(w * 0.74f, h * 0.18f),
                end = Offset(w * 0.86f, h * 0.08f),
                strokeWidth = sw,
                cap = StrokeCap.Round
            )
        }
    }
}

/** Mutable holder so long-lived analyzer callbacks see the latest values. */
private class Ref<T>(var value: T)
