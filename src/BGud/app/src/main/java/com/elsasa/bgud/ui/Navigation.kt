package com.elsasa.bgud.ui

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import com.elsasa.bgud.database.AppDatabase

/**
 * Minimal navigation scaffold for S5.1.
 *
 * Full screen inventory (SCR-MOB-001..008) is owned by S5.4..S5.11.
 * This graph only proves the Navigation Compose wiring compiles and
 * resolves the start destination; each destination is a placeholder.
 */
@Composable
fun AppNavigation(
    navController: NavHostController,
    database: AppDatabase
) {
    NavHost(
        navController = navController,
        startDestination = "home"
    ) {
        composable("login") {
            PlaceholderScreen("Login (S5.4)")
        }
        composable("home") {
            PlaceholderScreen("Home (S5.5)")
        }
    }
}

@Composable
private fun PlaceholderScreen(label: String) {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        Text(text = label)
    }
}
