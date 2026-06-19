package com.elsasa.btrade3.util

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.location.Location
import android.os.Handler
import android.os.Looper
import androidx.annotation.RequiresPermission
import androidx.core.app.ActivityCompat
import com.google.android.gms.location.LocationAvailability
import com.google.android.gms.location.LocationCallback
import com.google.android.gms.location.LocationRequest
import com.google.android.gms.location.LocationResult
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority

class LocationHelper(private val context: Context) {

    private val fusedLocationClient =
        LocationServices.getFusedLocationProviderClient(context)

    private var activeCallback: LocationCallback? = null
    private val mainHandler = Handler(Looper.getMainLooper())
    private var timeoutRunnable: Runnable? = null

    @RequiresPermission(allOf = [Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission.ACCESS_COARSE_LOCATION])
    fun getCurrentLocation(
        onLocationResult: (LocationResult) -> Unit,
        onError: (Exception) -> Unit
    ) {
        acquireHighAccuracyLocation(
            onLocationUpdate = { location ->
                onLocationResult(LocationResult.create(listOf(location)))
            },
            onComplete = { location ->
                if (location != null) {
                    onLocationResult(LocationResult.create(listOf(location)))
                } else {
                    onError(Exception("Location not available"))
                }
            },
            onError = onError
        )
    }

    /**
     * Requests continuous high-accuracy GPS updates until target accuracy is reached,
     * timeout occurs, or updates are stopped. Avoids stale cached network fixes.
     */
    @RequiresPermission(allOf = [Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission.ACCESS_COARSE_LOCATION])
    fun acquireHighAccuracyLocation(
        targetAccuracyMeters: Float = DEFAULT_TARGET_ACCURACY_METERS,
        timeoutMs: Long = DEFAULT_TIMEOUT_MS,
        maxLocationAgeMs: Long = DEFAULT_MAX_LOCATION_AGE_MS,
        onLocationUpdate: (Location) -> Unit,
        onComplete: (Location?) -> Unit,
        onError: (Exception) -> Unit
    ) {
        if (!hasLocationPermission()) {
            onError(SecurityException("Location permission not granted"))
            return
        }

        stopLocationUpdates()

        var bestLocation: Location? = null
        var completed = false

        fun finish(location: Location?) {
            if (completed) return
            completed = true
            stopLocationUpdates()
            onComplete(location)
        }

        val locationRequest = LocationRequest.Builder(Priority.PRIORITY_HIGH_ACCURACY, UPDATE_INTERVAL_MS)
            .setMinUpdateIntervalMillis(FASTEST_INTERVAL_MS)
            .setWaitForAccurateLocation(true)
            .setMaxUpdateDelayMillis(UPDATE_INTERVAL_MS * 2)
            .build()

        val callback = object : LocationCallback() {
            override fun onLocationResult(locationResult: LocationResult) {
                for (location in locationResult.locations) {
                    if (!location.hasAccuracy()) continue

                    onLocationUpdate(location)

                    if (bestLocation == null || location.accuracy < bestLocation!!.accuracy) {
                        bestLocation = location
                    }

                    if (LocationAcquisitionLogic.shouldAcceptLocation(
                            location,
                            targetAccuracyMeters,
                            maxLocationAgeMs
                        )
                    ) {
                        finish(location)
                        return
                    }
                }
            }

            override fun onLocationAvailability(locationAvailability: LocationAvailability) {
                if (!locationAvailability.isLocationAvailable && bestLocation == null) {
                    // Keep waiting until timeout; GPS may become available shortly.
                }
            }
        }

        activeCallback = callback

        timeoutRunnable = Runnable {
            finish(bestLocation)
        }.also {
            mainHandler.postDelayed(it, timeoutMs)
        }

        try {
            fusedLocationClient.requestLocationUpdates(
                locationRequest,
                callback,
                Looper.getMainLooper()
            )
        } catch (e: SecurityException) {
            stopLocationUpdates()
            onError(e)
        } catch (e: Exception) {
            stopLocationUpdates()
            onError(e)
        }
    }

    fun stopLocationUpdates() {
        timeoutRunnable?.let { mainHandler.removeCallbacks(it) }
        timeoutRunnable = null

        activeCallback?.let { callback ->
            fusedLocationClient.removeLocationUpdates(callback)
        }
        activeCallback = null
    }

    private fun hasLocationPermission(): Boolean {
        return ActivityCompat.checkSelfPermission(
            context,
            Manifest.permission.ACCESS_FINE_LOCATION
        ) == PackageManager.PERMISSION_GRANTED
    }

    companion object {
        const val DEFAULT_TARGET_ACCURACY_METERS = 30f
        const val DEFAULT_TIMEOUT_MS = 30_000L
        const val DEFAULT_MAX_LOCATION_AGE_MS = 5_000L
        private const val UPDATE_INTERVAL_MS = 1_000L
        private const val FASTEST_INTERVAL_MS = 500L
    }
}
