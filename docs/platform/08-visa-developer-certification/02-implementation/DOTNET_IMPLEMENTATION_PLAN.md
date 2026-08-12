# .NET Implementation Plan

## Solution boundary

```text
Domain/Application
  -> Network-neutral interchange decision
  -> Visa adapter
       -> request mapper
       -> correlatnId/idempotency
       -> MLE/JWE handler
       -> authentication
       -> HttpClient/mTLS
       -> Visa API
       <- HAL/response mapper
  -> audit/telemetry
```

## Work packages

### WP1 Configuration
Create strongly typed environment configuration for:
- Base URL
- API/resource paths
- timeout budget
- authentication mode
- client certificate reference
- User ID/password reference where required
- MLE enabled/key ID/key references
- callback settings
- feature flags

### WP2 Authentication
For VisaNet Connect – Acceptance implement Two-Way SSL. Keep authentication replaceable for other Visa APIs that may use other VDP mechanisms.

### WP3 MLE
Implement request encryption and response decryption as a dedicated boundary. Never log plaintext sensitive payloads.

### WP4 REST/HAL
Support:
- POST resources
- `application/hal+json` where required
- follow-on action links
- Visa HTTP/error mapping

### WP5 Idempotency
Generate and persist `correlatnId`.
For VisaNet Connect – Acceptance, preserve the same ID for a safe retry of the same request. Implement retention consistent with the product rules.

### WP6 Unknown outcomes
Timeout != decline. Persist pending/unknown state and execute the operation-specific recovery/void/reconciliation workflow.

### WP7 Callbacks
Where applicable:
- HTTPS
- mutual-auth requirements
- schema validation
- deduplication
- durable queue
- replay
- sanitized logs

### WP8 Observability
Record:
- operation
- correlation ID
- Visa request/reference identifiers
- HTTP status
- latency
- duplicate response indicator
- active credential/key metadata (non-secret)
- trace ID

### WP9 Security
- secrets outside code/config files
- key/cert access least privilege
- no PAN/CVV/raw sensitive payload logs
- certificate validation enabled
- key rotation
- PCI evidence

### WP10 Automated testing
- unit
- mapping/schema
- cryptographic component
- TLS
- integration
- negative/error
- idempotency
- timeout/unknown outcome
- performance
- security
- certification
- production smoke
