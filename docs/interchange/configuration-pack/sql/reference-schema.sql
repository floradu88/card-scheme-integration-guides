-- Vendor-neutral reference schema; adapt types and conventions to your database.

create table configuration_package (
    id varchar(100) primary key,
    semantic_version varchar(30) not null,
    network varchar(30) not null,
    region varchar(50) not null,
    status varchar(20) not null,
    effective_from timestamp not null,
    effective_to timestamp null,
    checksum varchar(128) not null,
    source_metadata_json text not null,
    created_at timestamp not null,
    created_by varchar(200) not null,
    approved_at timestamp null,
    approved_by varchar(200) null,
    activated_at timestamp null,
    activated_by varchar(200) null,
    unique(network, region, semantic_version)
);

create table interchange_program (
    id varchar(150) primary key,
    package_id varchar(100) not null references configuration_package(id),
    network_code varchar(100) null,
    name varchar(300) not null,
    description text null,
    fallback_flag boolean not null,
    effective_from timestamp not null,
    effective_to timestamp null
);

create table interchange_rule (
    id varchar(150) primary key,
    package_id varchar(100) not null references configuration_package(id),
    program_id varchar(150) not null references interchange_program(id),
    priority integer not null,
    specificity integer not null,
    partition_key varchar(500) not null,
    conditions_json text not null,
    rate_json text not null,
    source_reference text not null,
    effective_from timestamp not null,
    effective_to timestamp null
);

create index ix_rule_partition
on interchange_rule(package_id, partition_key, priority desc, specificity desc);

create table interchange_decision (
    id varchar(100) primary key,
    transaction_id varchar(150) not null,
    event_timestamp timestamp not null,
    configuration_package_id varchar(100) not null,
    engine_version varchar(50) not null,
    derivation_version varchar(50) not null,
    program_id varchar(150) null,
    rule_id varchar(150) null,
    fee_minor_units bigint null,
    fee_currency char(3) null,
    fallback_flag boolean not null,
    explanation_json text not null,
    normalized_input_hash varchar(128) not null,
    created_at timestamp not null
);

create index ix_decision_transaction on interchange_decision(transaction_id);
create index ix_decision_version on interchange_decision(configuration_package_id);

create table configuration_audit_event (
    id varchar(100) primary key,
    package_id varchar(100) not null,
    event_type varchar(50) not null,
    actor varchar(200) not null,
    event_timestamp timestamp not null,
    details_json text not null
);
