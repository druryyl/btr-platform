package com.elsasa.bgud.util

import android.content.Context
import android.content.Intent
import com.google.android.gms.auth.api.signin.GoogleSignIn
import com.google.android.gms.auth.api.signin.GoogleSignInAccount
import com.google.android.gms.auth.api.signin.GoogleSignInClient
import com.google.android.gms.auth.api.signin.GoogleSignInOptions
import com.google.android.gms.common.api.ApiException

/**
 * Google Sign-In gate for BGud, mirroring
 * `src/BTrade3/app/src/main/java/com/elsasa/btrade3/util/GoogleSignInHelper.kt`
 * (TD-01/TD-07): acquires the signed-in Google account (email identity only —
 * the id token is never sent to the Cloud) and exposes sign-out of the Google
 * account. Uses the shared web client id of Google project `btrade3-663be`;
 * there is no separate BGud OAuth registration (OQ-007).
 */
class GoogleSignInHelper(context: Context) {
    private val gso = GoogleSignInOptions.Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
        .requestEmail()
        // Shared web client id (client_type: 3) of project btrade3-663be, same as BTrade3 (TD-07)
        .requestIdToken("405920502340-odieer196drj8fd5jinppg7hnj8s8bpa.apps.googleusercontent.com")
        .build()

    private val googleSignInClient: GoogleSignInClient = GoogleSignIn.getClient(context, gso)

    fun getSignInIntent(): Intent = googleSignInClient.signInIntent

    fun signOut() {
        googleSignInClient.signOut()
    }

    fun getSignedInAccountFromIntent(data: Intent?): GoogleSignInAccount? {
        return try {
            val task = GoogleSignIn.getSignedInAccountFromIntent(data)
            task.getResult(ApiException::class.java)
        } catch (e: ApiException) {
            null
        }
    }

    /** The signed-in Google account email, or null when unavailable. */
    fun getSignedInEmail(data: Intent?): String? = getSignedInAccountFromIntent(data)?.email
}
