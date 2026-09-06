-- Optional reference schema. The application uses EF Core migrations/model creation.
CREATE TABLE IF NOT EXISTS users (
    id uuid PRIMARY KEY,
    email varchar(320) NOT NULL UNIQUE,
    role varchar(32) NOT NULL
);

CREATE TABLE IF NOT EXISTS projects (
    id uuid PRIMARY KEY,
    name varchar(200) NOT NULL,
    created_at_utc timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS project_assignments (
    project_id uuid NOT NULL REFERENCES projects(id),
    user_id uuid NOT NULL REFERENCES users(id),
    is_project_manager boolean NOT NULL,
    PRIMARY KEY(project_id, user_id)
);

CREATE INDEX IF NOT EXISTS ix_project_assignments_user_id
    ON project_assignments(user_id);

CREATE TABLE IF NOT EXISTS catalogue_items (
    id uuid PRIMARY KEY,
    code varchar(50) NOT NULL UNIQUE,
    description varchar(500) NOT NULL,
    unit_of_measure varchar(30) NOT NULL
);

CREATE TABLE IF NOT EXISTS catalogue_rates (
    id uuid PRIMARY KEY,
    catalogue_item_id uuid NOT NULL REFERENCES catalogue_items(id),
    effective_from date NOT NULL,
    rate numeric(18,2) NOT NULL,
    labour_cost numeric(18,2) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_catalogue_rates_item_date
    ON catalogue_rates(catalogue_item_id, effective_from);

CREATE TABLE IF NOT EXISTS estimates (
    id uuid PRIMARY KEY,
    project_id uuid NOT NULL REFERENCES projects(id),
    description varchar(1000) NOT NULL,
    pricing_date date NOT NULL,
    status varchar(32) NOT NULL,
    created_by_user_id uuid NOT NULL,
    created_at_utc timestamptz NOT NULL,
    updated_at_utc timestamptz NOT NULL,
    version bigint NOT NULL
);

CREATE TABLE IF NOT EXISTS estimate_lines (
    id uuid PRIMARY KEY,
    estimate_id uuid NOT NULL REFERENCES estimates(id),
    catalogue_item_id uuid NOT NULL REFERENCES catalogue_items(id),
    description varchar(500) NOT NULL,
    unit_of_measure varchar(30) NOT NULL,
    quantity numeric(18,3) NOT NULL,
    markup_percentage numeric(5,2) NOT NULL,
    rate numeric(18,2) NOT NULL,
    labour_cost numeric(18,2) NOT NULL,
    net_amount numeric(18,2) NOT NULL,
    markup_amount numeric(18,2) NOT NULL,
    total_amount numeric(18,2) NOT NULL,
    UNIQUE(estimate_id, catalogue_item_id)
);

CREATE INDEX IF NOT EXISTS ix_estimate_lines_estimate_id
    ON estimate_lines(estimate_id);

CREATE TABLE IF NOT EXISTS estimate_audit_events (
    id uuid PRIMARY KEY,
    estimate_id uuid NOT NULL REFERENCES estimates(id),
    from_status varchar(32) NOT NULL,
    to_status varchar(32) NOT NULL,
    changed_by_user_id uuid NOT NULL,
    changed_at_utc timestamptz NOT NULL
);
