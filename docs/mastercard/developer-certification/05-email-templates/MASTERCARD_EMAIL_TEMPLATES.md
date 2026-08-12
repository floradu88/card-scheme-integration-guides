# Mastercard / Acquirer Communication Templates

Use the assigned Mastercard representative or Implementation Manager once one exists. Mastercard Developers' published general support address is `developer@mastercard.com`.

Do not email secrets, passwords, private keys, PAN, CVV, raw production payloads, or unredacted cardholder data.

## 1 — Initial implementation / access

**To:** developer@mastercard.com  
**Subject:** Mastercard Developers APIs — implementation and onboarding request — [Project/Organization]

Hello Mastercard Developers team,

We are preparing an implementation of Mastercard Developers APIs for [generic use-case description].

Target region(s): [regions]  
Target APIs: [Authorization/Capture/Sale/Refund/Void/Verification]  
Target environments: Sandbox → Certification → Production  
Sponsor/acquirer: [name or status]  
Expected volume/TPS: [estimate]  
Target certification window: [date range]  
Target production window: [date range]

Please confirm the appropriate onboarding path, required approvals, product access, certification prerequisites, and the correct product/implementation contact.

Regards,  
[Name / role / organization]

## 2 — Sandbox readiness

**Subject:** Sandbox implementation ready for onboarding/certification planning — [Project]

Hello [Implementation Manager / Mastercard Developers team],

Our Sandbox implementation has reached the certification-planning gate.

Completed:
- authentication/connectivity
- MLE where required
- core API flows
- idempotency/correlation
- negative/error scenarios
- observability
- security review

Open items: [list]  
Evidence pack: [approved secure location/reference]  
Requested next step: confirm certification scope, test cases, required forms, credentials/test data, and scheduling.

Regards,
[...]

## 3 — Certification scheduling

**Subject:** Request to schedule certification — [Project] — [Preferred window]

Hello [Implementation Manager],

We are ready to schedule certification.

Preferred window: [dates/times/time zone]  
Engineering contacts: [...]  
Security contact: [...]  
Operations contact: [...]  
Acquirer/sponsor contact: [...]

Preconditions completed: [summary]
Known deviations/open risks: [summary]

Please confirm the test plan, participants, environment prerequisites, evidence format, and available certification slots.

Regards,
[...]

## 4 — Certification defect follow-up

**Subject:** Certification retest request — [Project] — [Case IDs]

Hello [Implementation Manager/Test Team],

The following certification findings have been addressed:
- [case] — [resolution]
- [case] — [resolution]

Regression evidence: [secure reference]
Proposed retest window: [...]
No sensitive data is attached to this email.

Please confirm the retest plan.

Regards,
[...]

## 5 — Production readiness

**Subject:** Production readiness / activation request — [Project]

Hello [Implementation Manager],

Certification is complete and our production readiness gate is [GO/CONDITIONAL GO].

Completed:
- production configuration
- production-specific credentials/certificates
- MLE validation
- PCI/compliance evidence
- monitoring/alerts
- incident runbooks
- rollback/DR
- approved smoke-test plan

Requested activation window: [...]
Expected initial volume: [...]
On-call contacts: [...]

Please confirm remaining Mastercard actions and final production activation steps.

Regards,
[...]

## 6 — Certificate/key rotation

**Subject:** Credential rotation coordination — [Project] — expires [date]

Hello [Implementation Manager / Mastercard Developers Support],

We are preparing rotation of [mTLS/MLE/X-Pay credential].

Environment: [...]
Current expiry: [...]
Planned change window: [...]
Rollback window: [...]

Please confirm any Mastercard-side actions, sequencing constraints, and validation steps required.

Regards,
[...]

## 7 — Production incident escalation

**Subject:** [SEV] Mastercard API production issue — [Project] — [UTC timestamp]

Hello [assigned support/Implementation Manager],

We are investigating a production issue affecting [operations].

Start time UTC: [...]
Environment: Production
Impact: [...]
HTTP/error category: [...]
Correlation/request IDs: [non-sensitive IDs]
Current mitigation: [...]
Assistance requested: [...]

No PAN, CVV, keys, passwords, or raw sensitive payloads are included.

Regards,
[Incident Commander]
