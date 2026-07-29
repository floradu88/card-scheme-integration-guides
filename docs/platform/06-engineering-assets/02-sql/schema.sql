CREATE TABLE payments (
    id uuid PRIMARY KEY,
    merchant_id text NOT NULL,
    amount_minor bigint NOT NULL,
    currency char(3) NOT NULL,
    status text NOT NULL,
    created_at timestamptz NOT NULL
);

CREATE TABLE interchange_decisions (
    id uuid PRIMARY KEY,
    payment_id uuid NOT NULL,
    config_version text NOT NULL,
    rule_id text,
    program_id text,
    amount_minor bigint,
    explanation jsonb NOT NULL,
    created_at timestamptz NOT NULL
);
