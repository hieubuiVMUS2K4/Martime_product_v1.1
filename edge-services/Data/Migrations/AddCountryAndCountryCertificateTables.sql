-- Migration: Add Country and CountryCertificate Tables
-- Date: 2026-01-29
-- Purpose: Add tables to manage country-certificate relationships (many-to-many)

-- ========== CREATE COUNTRIES TABLE ==========
CREATE TABLE IF NOT EXISTS public.countries (
    id SERIAL PRIMARY KEY,
    country_code VARCHAR(3) NOT NULL,
    country_name VARCHAR(100) NOT NULL,
    country_code_alpha2 VARCHAR(2),
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT idx_country_code_unique UNIQUE (country_code)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_country_code_alpha2 ON public.countries (country_code_alpha2);
CREATE INDEX IF NOT EXISTS idx_country_active ON public.countries (is_active) WHERE is_active = true;

COMMENT ON TABLE public.countries IS 'List of countries - ISO 3166-1 standard';
COMMENT ON COLUMN public.countries.country_code IS 'ISO 3166-1 alpha-3 code (e.g., USA, GBR, VNM)';
COMMENT ON COLUMN public.countries.country_code_alpha2 IS 'ISO 3166-1 alpha-2 code (e.g., US, GB, VN)';

-- ========== CREATE COUNTRY_CERTIFICATES TABLE ==========
CREATE TABLE IF NOT EXISTS public.country_certificates (
    id SERIAL PRIMARY KEY,
    country_id INTEGER NOT NULL,
    certificate_id INTEGER NOT NULL,
    is_recognized BOOLEAN NOT NULL DEFAULT true,
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_country_cert_country FOREIGN KEY (country_id) 
        REFERENCES public.countries (id) ON DELETE CASCADE,
    CONSTRAINT fk_country_cert_certificate FOREIGN KEY (certificate_id) 
        REFERENCES public.certificates (id) ON DELETE CASCADE,
    CONSTRAINT idx_country_cert_unique UNIQUE (country_id, certificate_id)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_country_cert_country_id ON public.country_certificates (country_id);
CREATE INDEX IF NOT EXISTS idx_country_cert_certificate_id ON public.country_certificates (certificate_id);
CREATE INDEX IF NOT EXISTS idx_country_cert_recognized ON public.country_certificates (is_recognized) WHERE is_recognized = true;

COMMENT ON TABLE public.country_certificates IS 'Many-to-many relationship between countries and certificates';
COMMENT ON COLUMN public.country_certificates.is_recognized IS 'Whether this country recognizes this certificate type';
COMMENT ON COLUMN public.country_certificates.notes IS 'Additional notes about recognition conditions or requirements';

-- ========== SEED SOME COMMON COUNTRIES ==========
INSERT INTO public.countries (country_code, country_name, country_code_alpha2, is_active) VALUES
('USA', 'United States', 'US', true),
('GBR', 'United Kingdom', 'GB', true),
('VNM', 'Vietnam', 'VN', true),
('PHL', 'Philippines', 'PH', true),
('JPN', 'Japan', 'JP', true),
('KOR', 'South Korea', 'KR', true),
('CHN', 'China', 'CN', true),
('SGP', 'Singapore', 'SG', true),
('IND', 'India', 'IN', true),
('MLT', 'Malta', 'MT', true),
('PAN', 'Panama', 'PA', true),
('LBR', 'Liberia', 'LR', true),
('NOR', 'Norway', 'NO', true),
('DNK', 'Denmark', 'DK', true),
('NLD', 'Netherlands', 'NL', true),
('DEU', 'Germany', 'DE', true),
('FRA', 'France', 'FR', true),
('ITA', 'Italy', 'IT', true),
('ESP', 'Spain', 'ES', true),
('GRC', 'Greece', 'GR', true)
ON CONFLICT (country_code) DO NOTHING;

COMMENT ON SCHEMA public IS 'Maritime Edge Database Schema';

-- Show summary
SELECT 'Countries table created with ' || COUNT(*) || ' countries' AS summary 
FROM public.countries;
