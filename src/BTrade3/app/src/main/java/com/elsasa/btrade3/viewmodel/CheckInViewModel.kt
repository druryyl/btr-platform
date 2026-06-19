package com.elsasa.btrade3.viewmodel

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.location.Location
import androidx.core.app.ActivityCompat
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.elsasa.btrade3.model.CheckIn
import com.elsasa.btrade3.model.Customer
import com.elsasa.btrade3.repository.CheckInRepository
import com.elsasa.btrade3.repository.CheckInSyncRepository
import com.elsasa.btrade3.repository.CustomerRepository
import com.elsasa.btrade3.util.LocationHelper
import com.elsasa.btrade3.util.LocationStatus
import com.elsasa.btrade3.util.LocationUtils
import com.elsasa.btrade3.util.ReverseGeocodingHelper
import com.elsasa.btrade3.util.UlidHelper
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

class CheckInViewModel(
    private val context: Context,
    private val checkInRepository: CheckInRepository,
    private val customerRepository: CustomerRepository,
    private val checkInSyncRepository: CheckInSyncRepository
) : ViewModel() {

    private val _locationStatus = MutableStateFlow(LocationStatus.NO_SIGNAL)
    val locationStatus: StateFlow<LocationStatus> = _locationStatus.asStateFlow()

    private val _currentLocation = MutableStateFlow<Location?>(null)
    val currentLocation: StateFlow<Location?> = _currentLocation.asStateFlow()

    private val _accuracy = MutableStateFlow(0f)
    val accuracy: StateFlow<Float> = _accuracy.asStateFlow()

    private val _isLoading = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = _isLoading.asStateFlow()

    private val _currentAddress = MutableStateFlow<String?>(null)
    val currentAddress: StateFlow<String?> = _currentAddress.asStateFlow()

    private val _nearbyCustomers = MutableStateFlow<List<Customer>>(emptyList())
    val nearbyCustomers: StateFlow<List<Customer>> = _nearbyCustomers.asStateFlow()

    private val _selectedCustomer = MutableStateFlow<Customer?>(null)
    val selectedCustomer: StateFlow<Customer?> = _selectedCustomer.asStateFlow()

    private val _allCustomers = MutableStateFlow<List<Customer>>(emptyList())

    private val _selectedCustomerWilayah: MutableStateFlow<String> = MutableStateFlow("")
    private val _selectedCustomerId: MutableStateFlow<String> = MutableStateFlow("")

    private val locationHelper = LocationHelper(context)
    private val reverseGeocodingHelper = ReverseGeocodingHelper(context)

    override fun onCleared() {
        locationHelper.stopLocationUpdates()
        super.onCleared()
    }

    fun checkLocationPermission(): Boolean {
        return ActivityCompat.checkSelfPermission(
            context,
            Manifest.permission.ACCESS_FINE_LOCATION
        ) == PackageManager.PERMISSION_GRANTED
    }

    fun loadNearbyCustomers() {
        viewModelScope.launch {
            _allCustomers.value = customerRepository.getAllCustomer().first()
            updateNearbyCustomers()
        }
    }

    fun startLocationCapture() {
        if (!checkLocationPermission()) {
            _locationStatus.value = LocationStatus.NO_PERMISSION
            return
        }

        _isLoading.value = true
        _locationStatus.value = LocationStatus.ACQUIRING
        _currentAddress.value = null

        locationHelper.acquireHighAccuracyLocation(
            onLocationUpdate = { location ->
                _currentLocation.value = location
                _accuracy.value = location.accuracy
                _locationStatus.value = LocationStatus.LOCKED
                updateNearbyCustomers()
            },
            onComplete = { location ->
                _isLoading.value = false
                location?.let { loc ->
                    _currentLocation.value = loc
                    _accuracy.value = loc.accuracy
                    _locationStatus.value = LocationStatus.LOCKED
                    viewModelScope.launch {
                        _currentAddress.value = reverseGeocodingHelper.getAddressFromLocation(loc)
                    }
                    updateNearbyCustomers()
                } ?: run {
                    if (_currentLocation.value == null) {
                        _locationStatus.value = LocationStatus.NO_SIGNAL
                    }
                }
            },
            onError = {
                _locationStatus.value = LocationStatus.NO_SIGNAL
                _isLoading.value = false
            }
        )
    }

    private fun updateNearbyCustomers() {
        try {
            val currentLocation = _currentLocation.value

            if (currentLocation != null) {
                if (currentLocation.latitude.isNaN() || currentLocation.longitude.isNaN()) {
                    _nearbyCustomers.value = emptyList()
                    return
                }

                viewModelScope.launch {
                    val customersToProcess = if (_selectedCustomerWilayah.value.isNotEmpty()) {
                        _allCustomers.value.filter { customer ->
                            customer.wilayah == _selectedCustomerWilayah.value &&
                                customer.customerId != _selectedCustomerId.value
                        }
                    } else {
                        _allCustomers.value.filter { customer ->
                            customer.customerId != _selectedCustomerId.value
                        }
                    }

                    _nearbyCustomers.value = LocationUtils.getCustomersWithinRadius(
                        currentLocation.latitude,
                        currentLocation.longitude,
                        customersToProcess,
                        100f
                    )
                }
            } else {
                _nearbyCustomers.value = emptyList()
            }
        } catch (e: Exception) {
            _nearbyCustomers.value = emptyList()
        }
    }

    fun selectCustomer(customer: Customer) {
        _selectedCustomerId.value = customer.customerId
        _selectedCustomerWilayah.value = customer.wilayah
        _selectedCustomer.value = customer
        updateNearbyCustomers()
    }

    fun unselectCustomer() {
        _selectedCustomer.value = null
        _selectedCustomerId.value = ""
        _selectedCustomerWilayah.value = ""
        updateNearbyCustomers()
    }

    fun checkIn(userEmail: String) {
        val currentLoc = _currentLocation.value
        val selectedCust = _selectedCustomer.value

        if (currentLoc != null && selectedCust != null) {
            viewModelScope.launch {
                val checkInDate = SimpleDateFormat("yyyy-MM-dd", Locale.getDefault()).format(Date())
                val checkInTime = SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(Date())

                val autoClosed = checkInRepository.autoCloseOpenVisit(
                    userEmail = userEmail,
                    checkOutTime = checkInTime,
                    checkOutLatitude = currentLoc.latitude,
                    checkOutLongitude = currentLoc.longitude,
                    checkOutAccuracy = currentLoc.accuracy
                )
                autoClosed?.let { checkInSyncRepository.uploadCheckIn(it.checkInId, context) }

                val checkIn = CheckIn(
                    checkInId = UlidHelper.generate(),
                    checkInDate = checkInDate,
                    checkInTime = checkInTime,
                    userEmail = userEmail,
                    checkInLatitude = currentLoc.latitude,
                    checkInLongitude = currentLoc.longitude,
                    accuracy = currentLoc.accuracy,
                    customerId = selectedCust.customerId,
                    customerCode = selectedCust.customerCode,
                    customerName = selectedCust.customerName,
                    customerAddress = selectedCust.alamat,
                    customerLatitude = selectedCust.latitude,
                    customerLongitude = selectedCust.longitude,
                    statusSync = "DRAFT",
                    isExplicitlyOpen = true
                )

                checkInRepository.insertCheckIn(checkIn)
                checkInSyncRepository.uploadCheckIn(checkIn.checkInId, context)
                _selectedCustomer.value = null
            }
        }
    }

    fun refreshCurrentAddress() {
        val currentLoc = _currentLocation.value
        if (currentLoc != null) {
            viewModelScope.launch {
                _currentAddress.value = reverseGeocodingHelper.getAddressFromLocation(currentLoc)
            }
        }
    }

    fun resetLocation() {
        locationHelper.stopLocationUpdates()
        _currentLocation.value = null
        _accuracy.value = 0f
        _currentAddress.value = null
        _locationStatus.value = LocationStatus.NO_SIGNAL
        _selectedCustomer.value = null
        _nearbyCustomers.value = emptyList()
    }
}
