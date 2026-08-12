# Emergency Configuration Rollback

1. Declare incident.
2. Freeze new configuration releases.
3. Identify current active checksum.
4. Identify last-known-good approved package.
5. Preserve current evidence.
6. Preload rollback package on all nodes.
7. Validate checksum and smoke tests.
8. Atomically activate rollback.
9. Confirm node consistency.
10. Monitor fallback, unmatched, latency, and variance.
11. Replay affected transactions.
12. quantify exposure.
13. create corrected immutable package.
14. complete root-cause analysis.

Never directly edit active production rules.
