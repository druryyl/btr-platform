package com.elsasa.bgud.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.elsasa.bgud.datastore.SessionPreferencesDataSource

/**
 * Factory for [SettingsViewModel], following the `BTrade3`
 * `…ViewModelFactory` per-screen convention (Architecture §19.3).
 */
class SettingsViewModelFactory(
    private val session: SessionPreferencesDataSource
) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {
        if (modelClass.isAssignableFrom(SettingsViewModel::class.java)) {
            @Suppress("UNCHECKED_CAST")
            return SettingsViewModel(session) as T
        }
        throw IllegalArgumentException("Unknown ViewModel class")
    }
}
