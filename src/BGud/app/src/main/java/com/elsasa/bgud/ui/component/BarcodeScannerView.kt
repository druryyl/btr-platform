package com.elsasa.bgud.ui.component

import androidx.camera.core.CameraSelector
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.compose.LocalLifecycleOwner
import androidx.compose.ui.viewinterop.AndroidView
import androidx.core.content.ContextCompat
import com.google.mlkit.vision.barcode.Barcode
import com.google.mlkit.vision.barcode.BarcodeScannerOptions
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.common.InputImage
import java.util.concurrent.Executors

/**
 * Camera acquisition component (SCR-MOB-003 viewfinder, Architecture §20).
 *
 * Camera acquisition is isolated behind this single component so the domain
 * stays scanner-agnostic (P-10): callers receive a plain barcode string via
 * [onBarcode] and never touch CameraX or ML Kit types.
 *
 * Stack: CameraX preview + ML Kit Barcode Scanning (TQ-7). Frames are
 * analyzed with `STRATEGY_KEEP_ONLY_LATEST`; analysis runs only while
 * [enabled] (single-scan mode — the caller disables it after resolution
 * and re-enables it to re-arm, §20). The ML Kit client and the camera
 * executors are released when the composition leaves.
 */
@Composable
fun BarcodeScannerView(
    enabled: Boolean,
    onBarcode: (String) -> Unit,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current

    // Latest callback wins; the analyzer reads the current value per frame.
    val latestOnBarcode = remember { Ref<(String) -> Unit>(onBarcode) }
    latestOnBarcode.value = onBarcode
    val latestEnabled = remember { Ref(enabled) }
    latestEnabled.value = enabled

    val scanner = remember {
        BarcodeScanning.getClient(
            BarcodeScannerOptions.Builder()
                .setBarcodeFormats(Barcode.FORMAT_ALL_FORMATS)
                .build()
        )
    }

    AndroidView(
        modifier = modifier,
        factory = { factoryContext ->
            val previewView = PreviewView(factoryContext)
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
                                            latestOnBarcode.value(rawValue)
                                        }
                                    }
                                    .addOnCompleteListener { imageProxy.close() }
                            }
                        }
                    try {
                        cameraProvider.unbindAll()
                        cameraProvider.bindToLifecycle(
                            lifecycleOwner,
                            CameraSelector.DEFAULT_BACK_CAMERA,
                            preview,
                            imageAnalysis
                        )
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
}

/** Mutable holder so long-lived analyzer callbacks see the latest values. */
private class Ref<T>(var value: T)
