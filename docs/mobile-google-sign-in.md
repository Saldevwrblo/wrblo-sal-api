# Mobile Google sign-in

The API uses a Google ID-token exchange. iOS and Android must use their native Google Sign-In SDKs to obtain an **ID token**, then send that token to the API.

## API request

```http
POST /api/auth/google
Content-Type: application/json

{
  "idToken": "<Google ID token>"
}
```

The API validates the token signature, expiry, email verification, and audience. On success it returns the normal `LoginResponse` containing the application access token and refresh token. Store the application tokens, not the Google ID token, for API authorization.

## OAuth client configuration

Create separate OAuth client IDs in Google Cloud Console as appropriate:

- Android OAuth client: package name and SHA-1/SHA-256 signing certificate fingerprints.
- iOS OAuth client: bundle ID.
- Web OAuth client: only if a web client also uses the same API.

Configure all accepted client IDs in the API. The environment-variable form is:

```text
Google__ClientId=<primary-client-id>
Google__ClientIds__0=<android-client-id>
Google__ClientIds__1=<ios-client-id>
Google__ClientIds__2=<web-client-id>
```

`Google__ClientId` is retained for backward compatibility. Every client ID that can issue an ID token for this API must be listed.

## Android

Use the native Google Sign-In SDK and request an ID token using the **server/web client ID** when the SDK requires a server client ID. The returned ID token's `aud` claim must match one of the configured API audiences. Send the token as `idToken` in the request above.

Do not send an access token in place of the ID token.

## iOS

Configure the iOS client ID and reversed client ID in the native Google Sign-In SDK. Request the user's ID token, then send it as `idToken` to the API. The API does not require an OAuth redirect or browser callback.

## Local development

The mobile application can call the API using the HTTPS URL exposed by the active Visual Studio profile, for example:

```text
https://localhost:44398/api/auth/google
```

A physical device cannot resolve the host machine's `localhost`. Use the host machine's LAN IP with a trusted development certificate, or use an HTTPS tunnel. Android emulators commonly reach the host through `10.0.2.2`, but the API must listen on an accessible interface and the certificate must be trusted by the emulator.

Mobile applications do not need the browser CORS origins configured for the web frontend because native HTTP clients are not subject to browser CORS enforcement.
