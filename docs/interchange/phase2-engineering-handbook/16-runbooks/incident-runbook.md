# Incident Runbook

## Trigger examples

- unexpected fallback spike;
- financial variance threshold exceeded;
- wrong package activated;
- node version drift;
- corrupt import;
- actual feed missing.

## Response

1. Stop further activation.
2. Identify active checksum and affected scope.
3. Preserve decision and source evidence.
4. Decide whether to continue last-known-good, roll back or isolate scope.
5. Atomically activate approved rollback.
6. Replay affected transactions.
7. quantify financial exposure.
8. create corrected immutable package.
9. complete root-cause analysis.

## Never

- edit active rules directly;
- delete decision history;
- overwrite estimated result with actual;
- expose sensitive payment data in incident channels.
