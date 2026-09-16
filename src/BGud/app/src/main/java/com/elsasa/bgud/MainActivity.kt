package com.elsasa.bgud

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.platform.LocalContext
import androidx.navigation.compose.rememberNavController
import com.elsasa.bgud.database.AppDatabase
import com.elsasa.bgud.ui.AppNavigation
import com.elsasa.bgud.ui.theme.BGudTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            BGudTheme {
                Surface(
                    color = MaterialTheme.colorScheme.background
                ) {
                    BGudApp()
                }
            }
        }
    }
}

@Composable
fun BGudApp() {
    val navController = rememberNavController()
    val database = AppDatabase.getDatabase(LocalContext.current)

    AppNavigation(navController, database)
}
