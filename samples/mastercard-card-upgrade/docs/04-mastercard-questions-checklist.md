# Questions for Mastercard before implementation/certification

## Product / eligibility

- Is the issuer ICA enabled for Account Level Management?
- Which Product Graduation service applies: Plus, Select, or another variant?
- Which source and target Product Codes are allowed?
- Can the exact source -> target transition be performed with the same PAN?
- Is the BIN/account range configured for the target?
- Are there regional/product restrictions?

## API

- What is the canonical Universal Specification Submission endpoint?
- What is the API version?
- What authentication/signature profile is required?
- Is the payload JSON, XML, fixed specification format, or API-specific DTO?
- What field represents Product Graduation action?
- What field represents Registered Product Code / target product?
- Which service code is required?
- How is effective date represented?
- What provides idempotency?
- What correlation/reference is returned?
- What is the final-status API / Detailed Response endpoint?

## Processing

- Is the operation synchronous or asynchronous?
- What is the expected processing lifecycle?
- What statuses are possible?
- What rejection/error codes are possible?
- What retry rules are permitted?
- How are reversals/downgrades performed?

## Certification

- Which test PANs are supplied?
- Which upgrade/downgrade test cases are mandatory?
- Is authorization validation required?
- Is clearing validation required?
- Is settlement/reconciliation validation required?
- What evidence must be submitted?
- Who signs off production enablement?
