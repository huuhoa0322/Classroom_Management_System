# Architecture Decision Record (ADR) Template

> **What is an ADR?**
> An Architecture Decision Record (ADR) is a short text file in a format similar to Alexandrian patterns that captures an important architectural decision made along with its context and consequences. It is stored as part of the project documentation (Docs-as-code) directly in the Git repository.

---

## Title: [ADR-00X] Short noun phrase describing the decision
**Date:** YYYY-MM-DD
**Status:** [ Proposed | Accepted | Rejected | Deprecated | Superseded by ADR-XXX ]
**Author(s):** [Name of Architect / AI pairing]

---

### 1. Context
*Describe the circumstances and the problem you are trying to solve. What is the business requirement or technical challenge?*
*   **Requirement:** e.g., "The system needs to process 10,000 orders per minute without freezing the user's checkout screen."
*   **Initial AI Suggestion:** e.g., "AI suggested a Microservices architecture using Apache Kafka for data streaming."

### 2. Decision
*What is the change that we are agreeing to make? Be specific. If you rejected the AI's suggestion, state why and what you chose instead.*
*   "We have decided to use **RabbitMQ** instead of Kafka."
*   **Justification:** "While Kafka is proposed for high-throughput streaming, our current team (3 Junior backend developers) lacks the devOps capacity to maintain a Zookeeper/Kafka cluster. RabbitMQ handles our required payload easily and is much simpler to deploy via a single Docker image."

### 3. Consequences
*What becomes easier or more difficult because of this change? (List both pros and cons)*
*   **Positive (Pros):**
    *   Easier implementation and lower server costs.
    *   Fulfills the asynchronous processing constraint.
*   **Negative (Cons / Tech Debt):**
    *   If traffic exceeds 50,000 concurrent writes/sec in the future, we may need to migrate to Kafka, requiring a rewrite of the publisher/subscriber beans.

### 4. Compliance & Verification
*How do we verify this decision?*
*   **Action items:** DevOps team will spin up a RabbitMQ container in the staging environment. Backend team will write a Locust load-testing script to verify 10,000 messages/min.
