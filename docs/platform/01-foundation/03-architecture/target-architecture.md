# Target Architecture

```mermaid
flowchart LR
    Merchant --> Gateway
    Gateway --> Orchestrator
    Orchestrator --> VisaAdapter
    Orchestrator --> MastercardAdapter
    Orchestrator --> PaymentStore
    Orchestrator --> InterchangeEngine
    InterchangeEngine --> DecisionStore
    Clearing --> Reconciliation
    Reconciliation --> Finance
```
