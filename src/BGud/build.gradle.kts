// Top-level build file where you can add configuration options common to all sub-projects/modules.
plugins {
    alias(libs.plugins.android.application) apply false
    alias(libs.plugins.kotlin.android) apply false
    alias(libs.plugins.kotlin.compose) apply false
}
buildscript {
    dependencies {
        // Google Services plugin for Google Sign-In (TD-01/TD-07), same version as BTrade3.
        classpath("com.google.gms:google-services:4.3.15")
    }
}
