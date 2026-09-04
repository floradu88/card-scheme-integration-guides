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
{ "sourceProductCode": "MCG", "targetProductCode": "MWE" }
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
dotnet test tests/MastercardCardUpgrade.SandboxTests/MastercardCardUpgrade.SandboxTests.csproj
```

Never commit Mastercard keys, PAN data, secrets, or production credentials.
