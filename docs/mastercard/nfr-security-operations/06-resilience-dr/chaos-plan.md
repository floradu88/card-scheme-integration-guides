# Chaos Engineering Plan

Run only with controlled scope and approvals.

## Scenarios

- terminate one runtime node;
- restart all non-leader nodes;
- block Mastercard endpoint;
- inject Mastercard latency;
- inject DNS failure;
- expire or revoke test certificate;
- disable configuration service;
- corrupt a draft package;
- introduce node configuration drift;
- stop database primary;
- stop Redis/queue;
- delay settlement feed;
- produce malformed clearing record;
- exhaust thread pool;
- simulate disk pressure;
- fail one availability zone.

## Success criteria

- no duplicate financial processing;
- alerts fire;
- runbooks are usable;
- service degrades as designed;
- recovery occurs within target;
- audit trail remains complete;
- no sensitive data leaks.
