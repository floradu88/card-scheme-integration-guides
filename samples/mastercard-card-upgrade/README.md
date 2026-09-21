# Mastercard Card Product Upgrade – .NET 8 sample

End-to-end Product Graduation Plus flow: **create a card → register the PAN in ACS → upgrade the product on the same PAN/BIN**.

Default `AlmMode` is `Local` (in-process ACS using the official 3.1.0 field names) so the full flow runs without issuer encryption keys. Set `AlmMode` to `Mastercard` to call `BaseUrl` + ACS paths.

Contract: [ACS API 3.1.0 swagger](https://static.developer.mastercard.com/content/account-catalog-services/swagger/acs-api-swagger.yaml)

Companion docs: [`docs/mastercard/alm-product-graduation/`](../../docs/mastercard/alm-product-graduation/). Original ZIP: [`archives/mastercard-card-upgrade-net8.zip`](../../archives/mastercard-card-upgrade-net8.zip).

## Run the whole flow (no Mastercard keys)

```powershell
cd samples/mastercard-card-upgrade
dotnet run --project src/MastercardCardUpgrade.Api --launch-profile Sandbox
```

Swagger: http://localhost:5088/swagger

```http
POST /api/demo/e2e
{ "sourceProductCode": "MCG", "targetProductCode": "MWE", "pan": "5555555555554444" }
```

That will:

1. Create an issuer card (MCG) with a generated Mastercard-range PAN
2. `POST /asc/acs-api/account-registrations` — Product Graduation Plus register
3. `PUT /asc/acs-api/account-registrations` — update `productGraduationProductCode` to MWE
4. `GET /asc/acs-api/account-registrations?correlation_id=` — INTERIM stays Submitted; only FINAL activates the product
5. Assert same PAN and same BIN, and issuer product matches ACS (`GET /api/cards/{id}/treatment`)

Timeouts and HTTP 408 become `Unknown`: local product is not changed, the same request id is not retried, and `POST /api/cards/{id}/upgrades/{migrationId}/reconcile` (or `POST /api/migrations/reconcile`) GETs ACS by `correlation_id`.

Step-by-step instead of the demo:

```http
POST /api/cards                  { "productCode": "MCG" }
POST /api/cards/{id}/register
POST /api/cards/{id}/upgrades    { "targetProductCode": "MWE" }
GET  /api/cards/{id}/treatment
POST /api/cards/{id}/upgrades/{migrationId}/rollback
POST /api/cards/{id}/close
```

Allowed line-of-business moves (same PAN): `MCG ↔ MCW ↔ MWE`.

## Postman

Import both files from `postman/`:

1. `Mastercard.Card.Upgrade.postman_collection.json`
2. `Mastercard.Card.Upgrade.local.postman_environment.json`

Select environment **Mastercard Card Upgrade Local**. Start the API on http://localhost:5088, then either:

- **Collection Runner** of the whole collection (starts with a unique PAN so it does not collide with `5555555555554444`)
- Folders in order: Setup → Products → Cards → Register → Upgrade → Treatment → Reconcile → Rollback and close
- **08 Demo** for the one-shot official Postman PAN (restart the API if that PAN already exists)

| Folder | What it covers |
|--------|----------------|
| 00 Setup | Unique PAN, sandbox status + `testData`, swagger.json |
| 01 Products | `GET /api/products` (MCG, MCW, MWE, MCO) |
| 02 Cards | Create MCG / MCW / MCO, generated PAN, list, get, 404 |
| 03 Register | Unique id, idempotent replay, swagger request id |
| 04 Upgrade | MCG→MCW→MWE, list migrations, legacy `/api/mastercard/upgrade` (`00616` is ALM) |
| 05 Treatment | MATCH / UNVERIFIED / MISMATCH |
| 06 Reconcile | One migration, all open, 404 |
| 07 Rollback and close | Product Graduation rollback, ACS delete-registrations |
| 08 Demo | Official PAN, unique PAN, empty body defaults |
| 09 Mastercard sandbox | BIN Lookup `585240844`, ALM status |
| 10 Negative examples | 422 / 404 / 409 |

Official sandbox examples used as variables:

| Source | Value |
|--------|--------|
| BIN Lookup Postman `accountRange` | `585240844` (native product `MCO`) |
| Card Upgrade Postman PAN | `5555555555554444` |
| ACS swagger `productGraduationProductCode` | `MCW` |
| ACS swagger request id | `21ad4488-615b-4004-6158-ec5abff7d58f` |
| ACS swagger expiry | `1223` |
| ACS `almServiceCode` | `00616` |

`GET /api/mastercard/sandbox/status` returns these under `testData`. Saved Postman examples on the main requests show the expected JSON (masked PAN, never the clear 16-digit PAN).

## Live Mastercard sandbox

| Call | Live sandbox | Notes |
|------|----------------|--------|
| BIN Lookup | Yes, with token or `.p12` | `BaseUrl` + `Paths.BinLookup` |
| ACS register / upgrade / status | When `AlmMode=Mastercard` | `BaseUrl` + `Paths.AcsRegistrations` (JWE required) |
| ACS delete / close | When `AlmMode=Mastercard` | `BaseUrl` + `Paths.AcsDeleteRegistrations` |

### Mastercard Developers project

1. Sign in at [developer.mastercard.com](https://developer.mastercard.com/).
2. Add **BIN Lookup** and **Account Catalog Services**.
3. Download sandbox `.p12`, Consumer Key, encryption certificate, and decryption key.

```json
{
  "Mastercard": {
    "BaseUrl": "https://sandbox.api.mastercard.com",
    "AuthMode": "OAuth1",
    "Token": "",
    "AlmMode": "Mastercard",
    "WritesEnabled": true,
    "CardStorePath": "App_Data/cards.json",
    "ReconcileIntervalSeconds": 15,
    "ConsumerKey": "YOUR_SANDBOX_CONSUMER_KEY",
    "SigningKeyP12Path": "C:\\secure\\mastercard-sandbox.p12",
    "SigningKeyAlias": "keyalias",
    "SigningKeyPassword": "YOUR_KEYSTORE_PASSWORD",
    "EncryptionCertificatePath": "C:\\secure\\mastercard-encryption.pem",
    "DecryptionKeyPath": "C:\\secure\\mastercard-decryption.pem",
    "Paths": {
      "BinLookup": "/bin-ranges/account-searches",
      "AcsRegistrations": "/asc/acs-api/account-registrations",
      "AcsDeleteRegistrations": "/asc/acs-api/account-registrations/delete-registrations"
    }
  }
}
```

Every Mastercard call is `BaseUrl` + a path from `Paths`. For a gateway that issues a bearer token, set `"AuthMode": "Bearer"` and `"Token": "..."`.

Copy `src/MastercardCardUpgrade.Api/appsettings.Local.json.example` to `appsettings.Local.json` (gitignored). Environment variables work the same way (`Mastercard__BaseUrl`, `Mastercard__Token`, `Mastercard__Paths__BinLookup`).

Official: [Quick start](https://developer.mastercard.com/platform/documentation/getting-started-with-mastercard-apis/quick-start-guide/) · [OAuth 1.0a](https://developer.mastercard.com/platform/documentation/security-and-authentication/using-oauth-1a-to-access-mastercard-apis/) · [ACS](https://developer.mastercard.com/account-catalog-services/documentation/) · [Product graduating a PAN](https://developer.mastercard.com/account-catalog-services/documentation/use-cases/pan-registration/product_graduating_pan/)

```powershell
dotnet test tests/MastercardCardUpgrade.UnitTests/MastercardCardUpgrade.UnitTests.csproj
dotnet test tests/MastercardCardUpgrade.SandboxTests/MastercardCardUpgrade.SandboxTests.csproj
dotnet test tests/MastercardCardUpgrade.E2ETests/MastercardCardUpgrade.E2ETests.csproj
```

### HTTP end-to-end suite (local machine + sandbox)

`MastercardCardUpgrade.E2ETests` hits the real HTTP surface (create → register → upgrade → treatment → rollback → close, plus kill switch / timeout / BIN Lookup).

**Local machine (default):** in-process `WebApplicationFactory`, Sandbox environment, Local ACS. No running server required.

```powershell
cd samples/mastercard-card-upgrade
dotnet test tests/MastercardCardUpgrade.E2ETests/MastercardCardUpgrade.E2ETests.csproj
# or
.\tests\MastercardCardUpgrade.E2ETests\run-e2e.ps1
```

**Against a running Sandbox API** (start the API first):

```powershell
dotnet run --project src/MastercardCardUpgrade.Api --launch-profile Sandbox
# other terminal:
$env:CARD_UPGRADE_E2E_BASE_URL = "http://localhost:5088"
dotnet test tests/MastercardCardUpgrade.E2ETests/MastercardCardUpgrade.E2ETests.csproj
# or
.\tests\MastercardCardUpgrade.E2ETests\run-e2e.ps1 -AgainstRunningApi
```

Config-override cases (kill switch, ACS timeout, account-range allow-list) only run in-process; they skip when `CARD_UPGRADE_E2E_BASE_URL` is set. Tests use unique PANs so a shared in-memory store does not collide.

**Live Mastercard sandbox**

| What | When it runs |
|------|----------------|
| BIN Lookup `585240844` | Token or `.p12` is configured (`credentialsConfigured`) |
| ACS Product Graduation writes | `AlmMode=Mastercard`, JWE ready, **and** `CARD_UPGRADE_E2E_LIVE_ACS=1` |

```powershell
.\tests\MastercardCardUpgrade.E2ETests\run-e2e.ps1 -AgainstRunningApi -LiveAcs
```

Never commit Mastercard keys, PAN data, secrets, or production credentials.
