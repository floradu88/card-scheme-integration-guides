create table configuration_package (
  package_id varchar(120) primary key,
  version varchar(30) not null,
  network varchar(20) not null,
  region varchar(50) not null,
  status varchar(20) not null,
  effective_from timestamptz not null,
  effective_to timestamptz null,
  checksum varchar(128) not null,
  canonical_json jsonb not null,
  created_at timestamptz not null,
  created_by varchar(200) not null,
  unique(network, region, version)
);

create table interchange_decision (
  decision_id uuid primary key,
  transaction_id varchar(160) not null,
  event_timestamp timestamptz not null,
  package_id varchar(120) not null,
  engine_version varchar(40) not null,
  derivation_version varchar(40) not null,
  bin_version varchar(40) null,
  program_id varchar(160) null,
  rule_id varchar(160) null,
  fee_minor_units bigint null,
  fee_currency char(3) null,
  fallback_flag boolean not null,
  reason_codes jsonb not null,
  input_hash varchar(128) not null,
  created_at timestamptz not null
);

create index ix_decision_tx on interchange_decision(transaction_id);
create index ix_decision_package on interchange_decision(package_id);
create index ix_decision_time on interchange_decision(event_timestamp);

create table reconciliation_result (
  reconciliation_id uuid primary key,
  decision_id uuid not null references interchange_decision(decision_id),
  actual_program_code varchar(160) null,
  actual_fee_minor_units bigint null,
  actual_fee_currency char(3) null,
  variance_minor_units bigint null,
  status varchar(30) not null,
  reason_code varchar(100) null,
  source_reference varchar(500) not null,
  reconciled_at timestamptz not null
);
