# Sandbox testing against Mastercard Developers

## End-to-end product upgrade (default, no keys)

`AlmMode` defaults to `Local`. This uses the official ACS 3.1.0 JSON field names (`accountIdentifier`, `productGraduationProductCode`, `Universal-Spec-Api-Request-Id`) against an in-process simulator.

```powershell
dotnet run --project src/MastercardCardUpgrade.Api --launch-profile Sandbox
```

```http
POST http://localhost:5088/api/demo/e2e
Content-Type: application/json

{ "sourceProductCode": "MCG", "targetProductCode": "MWE" }
```

Expected: a generated PAN, PGP registration, upgrade MCG → MWE, `samePan` and `sameBin` true, status `Active`.

## Live Mastercard calls

| Call | Mastercard sandbox | Notes |
|------|--------------------|--------|
| `GET /api/mastercard/sandbox/status` | No | Local config check |
| `POST /api/mastercard/sandbox/bin-lookup` | Yes, with `.p12` | `https://sandbox.api.mastercard.com/bin-ranges/account-searches` |
| `POST /api/demo/e2e` with `AlmMode=Mastercard` | Yes, ACS + JWE | `https://sandbox.api.mastercard.com/asc/acs-api/account-registrations` |

ACS payloads are marked `x-mastercard-api-encrypted`. Live mode needs the project encryption certificate and decryption key as well as the OAuth `.p12`.

## Credentials

Store in gitignored `src/MastercardCardUpgrade.Api/appsettings.Local.json`. See the sample README.
