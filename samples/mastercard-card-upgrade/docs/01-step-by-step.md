# Step-by-step card product upgrade

Live Mastercard sandbox walkthrough: [`05-sandbox-testing.md`](05-sandbox-testing.md).

## Step 1 — Authenticate

Create a Mastercard Developers project and provision the API products you need.

For the public Postman collections:

1. Copy the Mastercard `consumerKey`.
2. Download the Mastercard `.p12`.
3. Convert the private key to PEM/RSA as described in Mastercard's Postman
   workspace.
4. Configure OAuth 1.0.

Example key conversion from Mastercard's Postman instructions:

```bash
openssl pkcs12 -in mykey.p12 | openssl rsa -out myrsa.key
```

## Step 2 — Identify current account range/product

Public Mastercard BIN Lookup request:

```http
POST {{baseUrl}}/bin-ranges/account-searches
Content-Type: application/json
```

Example:

```json
{
  "accountRange": 585240844
}
```

Relevant response fields include:

- `lowAccountRange`
- `highAccountRange`
- `ica`
- `productCode`
- `productDescription`
- `programName`
- `fundingSource`
- `consumerType`

## Step 3 — Check ALM / Product Graduation eligibility

Before submitting a PAN-level change, validate that:

- the issuer is provisioned for Mastercard ALM;
- the source product is eligible;
- the target product is configured;
- the PAN/account is eligible;
- the required Product Graduation service is active;
- the required effective-date rules are satisfied.

## Step 4 — Submit the upgrade

Use Mastercard ALM / Universal Specification Submission with the issuer-provisioned
Product Graduation operation.

Application-level information normally required by the orchestration layer:

- PAN
- ICA
- target product
- Product Graduation service/action
- effective date
- correlation/reference data

The exact wire schema must come from Mastercard's ALM/USS specification.

## Step 5 — Treat submission and final status separately

Do not equate HTTP 200/202 with "card upgraded".

Recommended state model:

```text
CREATED
 -> SUBMITTED
 -> ACCEPTED / REJECTED
 -> ACTIVE
```

Use the Mastercard Detailed Response/status mechanism supplied during onboarding.

## Step 6 — Verify

Verify on both sides:

1. issuer/card platform reflects the desired product;
2. Mastercard ALM registration is accepted/active;
3. authorization tests show expected product treatment;
4. clearing/reconciliation shows expected ALM/product information.

## Step 7 — Rollback

Define a controlled downgrade/reversal path using the Mastercard-supported Product
Graduation operation rather than directly modifying network state.
