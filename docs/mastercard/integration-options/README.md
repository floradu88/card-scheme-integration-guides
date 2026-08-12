# Mastercard Integration Options

Choose one option folder below. These packs mirror the Visa A–F options for Mastercard, adapted in-repo (no separate A–F Mastercard ZIPs in [`../../../archives/`](../../../archives/)).

| Option | Path | Goal |
|--------|------|------|
| **A** — Authorization-only PoC | [`A-authorization-only-poc/`](A-authorization-only-poc/) | Prove sandbox connectivity and auth handling |
| **B** — Authorization, capture, clearing | [`B-authorization-capture-clearing/`](B-authorization-capture-clearing/) | Lifecycle through clearing |
| **C** — Full payment processing | [`C-full-payment-processing/`](C-full-payment-processing/) | Production-grade Mastercard acceptance |
| **D** — Interchange estimation only | [`D-interchange-estimation-only/`](D-interchange-estimation-only/) | Estimate interchange without direct processing |
| **E** — Clearing reconciliation only | [`E-clearing-reconciliation-only/`](E-clearing-reconciliation-only/) | Reconcile expected vs actual interchange |
| **F** — Processor connection replacement | [`F-processor-connection-replacement/`](F-processor-connection-replacement/) | Migrate off an existing processor connection |

## Related

- NFR / security / operations: [`../nfr-security-operations/`](../nfr-security-operations/)
- Mastercard Connect / Developers security: [`../mastercard-connect-security/`](../mastercard-connect-security/)
- Platform network phase: [`../../platform/02-network-integration/`](../../platform/02-network-integration/)
- Interchange (esp. options D/E): [`../../interchange/`](../../interchange/)
- Mastercard interchange fees / dynamic per-country updates: [`../interchange-fees/`](../interchange-fees/)
