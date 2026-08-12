# Visa Developer Toolbox

## Portal / project tools
- Visa Developer account and Project Dashboard
- Product Catalog and regional availability
- Request Access for restricted products
- Onboarding Dashboard
- Sandbox project view
- Certification and Production credential views
- Assets / Asset Management
- API Reference / Code Explorer
- API Explorer
- supplied sandbox test data
- product implementation guides and restricted project assets

## Test and troubleshooting tools
- VDC Playground (Windows) for Visa API testing/troubleshooting
- authentication support including Two-Way SSL and MLE
- JKS/P12 keystore generation support in VDC Playground
- exportable diagnostic details
- Postman or equivalent REST client for local diagnostics
- OpenSSL for CSR, key, certificate and PKCS#12 workflows
- Java Keytool where JKS is required
- curl for TLS/connectivity diagnostics
- .NET integration/contract test harness
- packet/TLS diagnostics in approved non-production environments

## .NET developer stack
- .NET 8+
- IHttpClientFactory / SocketsHttpHandler
- System.Security.Cryptography
- X509Certificate2
- Options pattern
- OpenTelemetry
- Polly/resilience pipeline only with operation-safe retry rules
- JSON serialization
- JOSE/JWE library selected through security review for MLE
- secret/vault provider
- contract/integration test project
- certification test runner
- sanitized request/response diagnostic capture

## CI/CD tools
- SAST
- SCA/dependency scanning
- secret scanning
- SBOM generation
- artifact signing
- IaC scanning
- integration-test stage
- certification-test stage
- environment approval gates
- production smoke tests

## Operations tools
- metrics/dashboard platform
- distributed tracing
- centralized sanitized logs
- SIEM
- certificate expiry monitoring
- vault/HSM/KMS
- incident management
- evidence repository

## Required developer deliverables
- Visa adapter
- authentication handler
- MLE handler
- certificate/key provider
- correlation/idempotency implementation
- HAL/HATEOAS support where used
- callback receiver where product requires callbacks
- error mapping
- observability
- configuration validation
- sandbox test suite
- certification test suite
- production smoke suite
