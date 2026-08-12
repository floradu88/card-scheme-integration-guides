# Mastercard Developers APIs — Security Add-on for .NET

## Purpose

This document extends the payment-platform architecture handbook with concrete Mastercard Developers API security requirements for:

- OAuth 1.0a request signing (primary Mastercard Developers authentication)
- Optional mutual TLS where product/environment requires it
- Payload encryption for sensitive fields
- Certificate and key lifecycle
- Sandbox, Certification, and Production separation
- .NET integration patterns
- Monitoring, testing, and stage gates

> Note: Mastercard Developers authentication is **not identical** to Visa Two-Way SSL + MLE. Prefer OAuth 1.0a signing keys and Mastercard payload-encryption guidance for API products. Confirm product-specific requirements in the Mastercard Developers project dashboard.

## Official Mastercard links

- OAuth 1.0a authentication  
  https://developer.mastercard.com/platform/documentation/security-and-authentication/using-oauth-1a-to-access-mastercard-apis/

- Payload encryption  
  https://developer.mastercard.com/platform/documentation/security-and-authentication/securing-sensitive-data-using-payload-encryption/

- Getting started  
  https://developer.mastercard.com/platform/documentation/getting-started-with-mastercard-apis/quick-start-guide/

- Mastercard Developers portal  
  https://developer.mastercard.com/

- Mastercard Rules hub  
  https://www.mastercard.com/us/en/business/support/rules.html

## 1. Security architecture

OAuth signing and payload encryption are separate controls:

```text
Application
   |
   | JSON payload
   v
Payload encryption (when required)
   |
   | encrypted fields / payload
   v
HttpClient + OAuth 1.0a signer
   |
   | Authorization header + optional mTLS
   v
Mastercard Developers API
   |
   v
HTTP response
   |
   | encrypted fields (if any)
   v
Payload decryption
   |
   v
Application
```

OAuth 1.0a authenticates the client and protects request integrity.

Payload encryption protects sensitive message content (for example PAN, PII, account data) when the selected API requires it.

## 2. OAuth 1.0a key model

Mastercard Developers projects typically issue:

- Consumer Key
- Signing key (private key) held by the client
- Mastercard encryption keys when payload encryption is enabled

### Request signing

The client signs each request with its private signing key. Mastercard validates the signature using the registered consumer/project credentials.

### Payload encryption (when enabled)

Encrypt sensitive request fields using the Mastercard encryption public key. Decrypt response fields with the client private decryption key when responses are encrypted.

Verify the exact algorithms and field paths required by the active API/project before implementation.

## 3. Key identifiers and configuration

Consumer keys, key aliases, and encryption key IDs must be external configuration because they differ by environment and rotate over time.

Example:

```json
{
  "Mastercard": {
    "Environment": "Sandbox",
    "BaseUrl": "https://sandbox.api.mastercard.com",
    "ConsumerKey": "FROM-MC-PROJECT",
    "SigningKeySecret": "mc-signing-key-sandbox",
    "PayloadEncryption": {
      "Enabled": true,
      "EncryptionCertificateSecret": "mc-encryption-public-cert",
      "DecryptionKeySecret": "mc-decryption-private-key"
    }
  }
}
```

## 4. Recommended .NET structure

- `MastercardAuthHandler` — OAuth 1.0a signing DelegatingHandler
- `MastercardPayloadEncryptionService` — field/payload encrypt/decrypt
- `MastercardApiClient` — typed HTTP client via `IHttpClientFactory`
- Secret/vault provider for keys (never commit private keys)
- Options pattern for environment-specific endpoints and key IDs
- OpenTelemetry spans: `mastercard.request`, `mastercard.response.mapping`

## 5. Environment separation

Maintain distinct projects/credentials for:

| Environment | Purpose |
|-------------|---------|
| Sandbox | Connectivity and functional development |
| Certification / MTF | Scheme or sponsor certification scenarios |
| Production | Live traffic only after go-live approval |

Do not reuse sandbox signing keys in production.

## 6. Certificate and key lifecycle

1. Generate or obtain signing/encryption keys per Mastercard Developers project guidance.
2. Register public material in the project dashboard.
3. Store private material in a vault with rotation runbooks.
4. Monitor expiry and rotation windows.
5. Dual-run old/new keys during rotation when the product supports it.
6. Revoke compromised keys immediately and open a security incident.

## 7. Monitoring, testing, and stage gates

- Alert on auth failures, signature validation errors, and encryption failures.
- Contract tests for signed requests and encrypted fields.
- Stage gates: sandbox connectivity → certification evidence → production credentials → controlled go-live.
- Never log PAN, full OAuth secrets, or private keys.

## 8. Stage-gate checklist (summary)

- [ ] Sandbox project created; consumer key and signing key validated
- [ ] Payload encryption validated if required by the API
- [ ] Negative auth/encryption tests recorded
- [ ] Certification scenarios executed and evidence archived
- [ ] Production credentials issued and vaulted
- [ ] Rollback and key-revocation runbooks approved

## Related docs

- Mastercard quick start: [`../quick-start/`](../quick-start/)
- NFR / security / ops: [`../nfr-security-operations/`](../nfr-security-operations/)
- Phase 09 certification pack: [`../../platform/09-mastercard-developer-certification/`](../../platform/09-mastercard-developer-certification/)
- Official link index: [`../nfr-security-operations/13-official-references/mastercard-links.md`](../nfr-security-operations/13-official-references/mastercard-links.md)
