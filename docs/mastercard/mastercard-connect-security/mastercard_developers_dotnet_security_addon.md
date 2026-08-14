# Mastercard Developers APIs — Security Add-on for .NET

## Purpose

This document extends the payment-platform architecture handbook with concrete Mastercard Developers API security requirements drawn from Mastercard’s public documentation:

- OAuth 1.0a request signing (primary Mastercard Developers authentication)
- Optional mutual TLS via Key Management Portal when a product requires it
- Payload encryption (Field Level Encryption and/or JWE) for sensitive fields
- Certificate and key lifecycle
- Sandbox / MTF / Production separation
- .NET integration patterns
- Monitoring, testing, and stage gates

> Mastercard Developers authentication is **not** Visa Two-Way SSL + MLE. Use OAuth 1.0a + Mastercard payload-encryption guidance unless a specific product document says otherwise.

## Official Mastercard links

| Topic | URL |
|-------|-----|
| OAuth 1.0a | https://developer.mastercard.com/platform/documentation/security-and-authentication/using-oauth-1a-to-access-mastercard-apis/ |
| Payload encryption | https://developer.mastercard.com/platform/documentation/security-and-authentication/securing-sensitive-data-using-payload-encryption/ |
| Quick start | https://developer.mastercard.com/platform/documentation/getting-started-with-mastercard-apis/quick-start-guide/ |
| Developers portal | https://developer.mastercard.com/ |
| Mastercard Connect | https://www.mastercardconnect.com/ |
| Transaction API for Acquirers | https://developer.mastercard.com/transaction-api-for-acquirers/documentation/ |
| Curated website index | [`../official-website-references.md`](../official-website-references.md) |

### Official libraries

| Library | URL |
|---------|-----|
| oauth1-signer-java | https://github.com/Mastercard/oauth1-signer-java |
| oauth1-signer-nodejs | https://github.com/Mastercard/oauth1-signer-nodejs |
| client-encryption-java | https://github.com/Mastercard/client-encryption-java |
| API client tutorial | https://github.com/Mastercard/mastercard-api-client-tutorial |
| Transaction API reference app | https://github.com/Mastercard/transaction-api-reference-app |

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

From Mastercard Developers docs: every API request must be signed with an RSA private key; Mastercard verifies using the public key registered for the project Consumer Key. Payload encryption protects PCI/PII fields when the product requires it (FLE and/or JWE).

## 2. OAuth 1.0a key model

Project setup on Mastercard Developers typically yields:

- **Consumer Key** (shown in the project dashboard)
- **Signing key** private key (often downloaded as a password-protected PKCS#12)
- Optional encryption certificates/keys when payload encryption is enabled

Extract the private signing key from PKCS#12 only into a vault/HSM workflow. Prefer Mastercard’s published signer libraries as the behavioral reference for Authorization header construction.

## 3. Payload encryption key model

When enabled for the API:

- Encrypt sensitive request fields with Mastercard’s public encryption certificate.
- Decrypt sensitive response fields with the client private decryption key.
- Confirm whether the product uses Mastercard Field Level Encryption, JWE, or both—paths and headers differ by product.

## 4. Optional mTLS (product-dependent)

Some acquiring products (for example Transaction API for Acquirers reference materials) obtain client certificates through **Key Management Portal (KMP)** inside [Mastercard Connect](https://www.mastercardconnect.com/). Treat mTLS as additive to OAuth when the product guide requires it—not as a replacement for OAuth 1.0a on Developers APIs.

## 5. Configuration example

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
    },
    "Mtls": {
      "Enabled": false,
      "ClientCertificateSecret": "mc-mtls-client-cert"
    }
  }
}
```

## 6. Recommended .NET structure

- `MastercardOAuthHandler` — DelegatingHandler that signs requests (align with official signer behavior)
- `MastercardPayloadEncryptionService` — encrypt/decrypt per product config
- `MastercardApiClient` — typed client via `IHttpClientFactory`
- Vault-backed secret provider
- OpenTelemetry spans: `mastercard.request`, `mastercard.response.mapping`

## 7. Environment separation

| Environment | Purpose |
|-------------|---------|
| Sandbox | Connectivity and functional development |
| MTF / Certification | Scheme or sponsor certification scenarios |
| Production | Live traffic after go-live approval |

Do not reuse sandbox signing keys in production.

## 8. Monitoring and stage gates

- Alert on OAuth signature failures, HTTP 401/403 auth errors, and encryption failures.
- Contract tests for signed requests and encrypted fields.
- Stage gates: sandbox connectivity → certification evidence → production credentials → controlled go-live.
- Never log PAN, Consumer Key secrets, or private keys.

## Related docs

- Curated website references: [`../official-website-references.md`](../official-website-references.md)
- Quick start: [`../quick-start/`](../quick-start/)
- NFR / security / ops: [`../nfr-security-operations/`](../nfr-security-operations/)
- Phase 09 certification: [`../../platform/09-mastercard-developer-certification/`](../../platform/09-mastercard-developer-certification/)
