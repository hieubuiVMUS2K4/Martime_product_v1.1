-- Maritime Edge Database Initialization Script
-- PostgreSQL 15+

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For text search
CREATE EXTENSION IF NOT EXISTS "btree_gist"; -- For advanced indexing

-- Set timezone to UTC
SET TIME ZONE 'UTC';

-- Create schema (already public by default)
-- But we can add comments
COMMENT ON SCHEMA public IS 'Maritime Edge Server data storage';

-- Performance tuning for time-series data
-- These are applied at session level, DBA should set at database level
ALTER DATABASE maritime_edge SET timezone TO 'UTC';
ALTER DATABASE maritime_edge SET log_statement TO 'mod';

-- Create read-only user for monitoring
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'edge_readonly') THEN
        CREATE ROLE edge_readonly WITH LOGIN PASSWORD 'readonly_password';
    END IF;
END
$$;

-- Grant read-only access
GRANT CONNECT ON DATABASE maritime_edge TO edge_readonly;
GRANT USAGE ON SCHEMA public TO edge_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO edge_readonly;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO edge_readonly;

-- Create function for automatic timestamp updates
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Vacuum and analyze settings for optimal performance
ALTER SYSTEM SET autovacuum = on;
ALTER SYSTEM SET autovacuum_naptime = '1min';

-- Log completion
DO $$
BEGIN
    RAISE NOTICE 'Maritime Edge Database initialized successfully at %', NOW();
END
$$;
