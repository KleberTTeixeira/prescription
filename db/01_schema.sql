-- =============================================================
-- Medical prescription database - schema
-- Target: MySQL 8.0.16+ (CHECK constraints enforced)
-- =============================================================

-- Script file is UTF-8: the client must send it as utf8mb4
-- (otherwise accented text is stored double-encoded).
SET NAMES utf8mb4;

CREATE DATABASE IF NOT EXISTS prescription_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE prescription_db;

-- -------------------------------------------------------------
-- Lookup tables
-- -------------------------------------------------------------

CREATE TABLE pharmaceutical_forms (
    id    INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name  VARCHAR(60)  NOT NULL,
    CONSTRAINT pk_pharmaceutical_forms PRIMARY KEY (id),
    CONSTRAINT uq_pharmaceutical_forms_name UNIQUE (name)
) ENGINE = InnoDB;

CREATE TABLE administration_routes (
    id    INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name  VARCHAR(60)  NOT NULL,
    CONSTRAINT pk_administration_routes PRIMARY KEY (id),
    CONSTRAINT uq_administration_routes_name UNIQUE (name)
) ENGINE = InnoDB;

CREATE TABLE dose_units (
    id            INT UNSIGNED NOT NULL AUTO_INCREMENT,
    abbreviation  VARCHAR(20)  NOT NULL,
    name          VARCHAR(60)  NOT NULL,
    CONSTRAINT pk_dose_units PRIMARY KEY (id),
    CONSTRAINT uq_dose_units_abbreviation UNIQUE (abbreviation)
) ENGINE = InnoDB;

CREATE TABLE frequencies (
    id    INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name  VARCHAR(60)  NOT NULL,
    CONSTRAINT pk_frequencies PRIMARY KEY (id),
    CONSTRAINT uq_frequencies_name UNIQUE (name)
) ENGINE = InnoDB;

-- Prescription validity (ECF.10.03 "prazo de utilização").
-- days NULL = no validity limit.
CREATE TABLE validity_periods (
    id    INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name  VARCHAR(80)  NOT NULL,
    days  INT UNSIGNED NULL,
    CONSTRAINT pk_validity_periods PRIMARY KEY (id),
    CONSTRAINT uq_validity_periods_name UNIQUE (name),
    CONSTRAINT ck_validity_periods_days CHECK (days IS NULL OR days > 0)
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Institutions (place of care) - ECF.10.04
-- -------------------------------------------------------------

CREATE TABLE institutions (
    id                  INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name                VARCHAR(150) NOT NULL,
    cnes                CHAR(7)      NOT NULL,
    address_street      VARCHAR(150) NOT NULL,
    address_number      VARCHAR(10)  NOT NULL,
    address_complement  VARCHAR(60)  NULL,
    address_district    VARCHAR(80)  NOT NULL,
    address_city        VARCHAR(80)  NOT NULL,
    address_state       CHAR(2)      NOT NULL,
    address_zip_code    CHAR(8)      NOT NULL,
    phone               VARCHAR(20)  NOT NULL,
    CONSTRAINT pk_institutions PRIMARY KEY (id),
    CONSTRAINT uq_institutions_cnes UNIQUE (cnes),
    CONSTRAINT ck_institutions_cnes CHECK (cnes REGEXP '^[0-9]{7}$'),
    CONSTRAINT ck_institutions_zip_code CHECK (address_zip_code REGEXP '^[0-9]{8}$')
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Professionals (prescribers) - ECF.10.04
-- -------------------------------------------------------------

CREATE TABLE professionals (
    id              INT UNSIGNED NOT NULL AUTO_INCREMENT,
    full_name       VARCHAR(150) NOT NULL,
    council         VARCHAR(10)  NOT NULL,
    council_number  VARCHAR(20)  NOT NULL,
    council_state   CHAR(2)      NOT NULL,
    CONSTRAINT pk_professionals PRIMARY KEY (id),
    CONSTRAINT uq_professionals_council UNIQUE (council, council_number, council_state)
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Clients - ECF.10.04
-- -------------------------------------------------------------

CREATE TABLE clients (
    id                  INT UNSIGNED NOT NULL AUTO_INCREMENT,
    full_name           VARCHAR(150) NOT NULL,
    birth_date          DATE         NOT NULL,
    sex                 CHAR(1)      NOT NULL,
    address_street      VARCHAR(150) NOT NULL,
    address_number      VARCHAR(10)  NOT NULL,
    address_complement  VARCHAR(60)  NULL,
    address_district    VARCHAR(80)  NOT NULL,
    address_city        VARCHAR(80)  NOT NULL,
    address_state       CHAR(2)      NOT NULL,
    address_zip_code    CHAR(8)      NOT NULL,
    CONSTRAINT pk_clients PRIMARY KEY (id),
    CONSTRAINT ck_clients_sex CHECK (sex IN ('M', 'F')),
    CONSTRAINT ck_clients_zip_code CHECK (address_zip_code REGEXP '^[0-9]{8}$')
) ENGINE = InnoDB;

CREATE INDEX ix_clients_full_name ON clients (full_name);

-- -------------------------------------------------------------
-- Medications catalog - ECF.10.03 (b)
-- -------------------------------------------------------------

CREATE TABLE medications (
    id                      INT UNSIGNED NOT NULL AUTO_INCREMENT,
    name                    VARCHAR(150) NOT NULL,
    active_ingredient       VARCHAR(150) NOT NULL,
    concentration           VARCHAR(60)  NULL,
    pharmaceutical_form_id  INT UNSIGNED NOT NULL,
    validity_period_id      INT UNSIGNED NOT NULL,
    is_controlled          BOOLEAN      NOT NULL DEFAULT FALSE,
    is_antimicrobial        BOOLEAN      NOT NULL DEFAULT FALSE,
    is_active               BOOLEAN      NOT NULL DEFAULT TRUE,
    CONSTRAINT pk_medications PRIMARY KEY (id),
    CONSTRAINT uq_medications_name_form UNIQUE (name, concentration, pharmaceutical_form_id),
    CONSTRAINT fk_medications_pharmaceutical_form
        FOREIGN KEY (pharmaceutical_form_id) REFERENCES pharmaceutical_forms (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT fk_medications_validity_period
        FOREIGN KEY (validity_period_id) REFERENCES validity_periods (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT
) ENGINE = InnoDB;

CREATE INDEX ix_medications_name ON medications (name);
CREATE INDEX ix_medications_active_ingredient ON medications (active_ingredient);

-- -------------------------------------------------------------
-- Favorite medications per professional - ECF.10.10
-- -------------------------------------------------------------

CREATE TABLE favorite_medications (
    professional_id  INT UNSIGNED NOT NULL,
    medication_id    INT UNSIGNED NOT NULL,
    created_at       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_favorite_medications PRIMARY KEY (professional_id, medication_id),
    CONSTRAINT fk_favorite_medications_professional
        FOREIGN KEY (professional_id) REFERENCES professionals (id)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT fk_favorite_medications_medication
        FOREIGN KEY (medication_id) REFERENCES medications (id)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Prescription templates (standard texts) - ECF.10.02
-- -------------------------------------------------------------

CREATE TABLE prescription_templates (
    id               INT UNSIGNED NOT NULL AUTO_INCREMENT,
    professional_id  INT UNSIGNED NOT NULL,
    title            VARCHAR(100) NOT NULL,
    content          TEXT         NOT NULL,
    created_at       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT pk_prescription_templates PRIMARY KEY (id),
    CONSTRAINT uq_prescription_templates_title UNIQUE (professional_id, title),
    CONSTRAINT fk_prescription_templates_professional
        FOREIGN KEY (professional_id) REFERENCES professionals (id)
        ON UPDATE RESTRICT ON DELETE CASCADE
) ENGINE = InnoDB;

-- -------------------------------------------------------------
-- Prescriptions - ECF.10.01 (unstructured) / ECF.10.03 (structured)
-- -------------------------------------------------------------

CREATE TABLE prescriptions (
    id               INT UNSIGNED NOT NULL AUTO_INCREMENT,
    client_id        INT UNSIGNED NOT NULL,
    professional_id  INT UNSIGNED NOT NULL,
    institution_id   INT UNSIGNED NOT NULL,
    format           VARCHAR(12)  NOT NULL,
    category         VARCHAR(16)  NOT NULL DEFAULT 'COMMON',
    free_text        TEXT         NULL,
    issued_at        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- printed_at: first confirmed print (prescription issued).
    -- NULL = not issued, editable; filled = issued, locked.
    printed_at       DATETIME     NULL,
    created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at       DATETIME     NULL ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT pk_prescriptions PRIMARY KEY (id),
    CONSTRAINT ck_prescriptions_format CHECK (format IN ('UNSTRUCTURED', 'STRUCTURED')),
    CONSTRAINT ck_prescriptions_category CHECK (category IN ('COMMON', 'SPECIAL_CONTROL', 'ANTIMICROBIAL')),
    CONSTRAINT ck_prescriptions_free_text CHECK (format <> 'UNSTRUCTURED' OR free_text IS NOT NULL),
    CONSTRAINT fk_prescriptions_client
        FOREIGN KEY (client_id) REFERENCES clients (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT fk_prescriptions_professional
        FOREIGN KEY (professional_id) REFERENCES professionals (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT fk_prescriptions_institution
        FOREIGN KEY (institution_id) REFERENCES institutions (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT
) ENGINE = InnoDB;

CREATE INDEX ix_prescriptions_issued_at ON prescriptions (issued_at);

-- -------------------------------------------------------------
-- Prescription items (structured prescription) - ECF.10.03 (c, d)
-- medication_id is NULL for medications not in the catalog
-- (e.g. compounded); medication_name is always stored.
-- usage_deadline: usage deadline date of the item. Auto-filled by
-- the system as issue date + validity_periods.days of the
-- medication; blank and optional when the medication has no
-- validity period or is not in the catalog. Stores the date
-- left by the user.
-- -------------------------------------------------------------

CREATE TABLE prescription_items (
    id                      INT UNSIGNED  NOT NULL AUTO_INCREMENT,
    prescription_id         INT UNSIGNED  NOT NULL,
    medication_id           INT UNSIGNED  NULL,
    medication_name         VARCHAR(150)  NOT NULL,
    pharmaceutical_form_id  INT UNSIGNED  NOT NULL,
    dose                    DECIMAL(10,3) NOT NULL,
    dose_unit_id            INT UNSIGNED  NOT NULL,
    frequency_id            INT UNSIGNED  NOT NULL,
    start_date              DATE          NOT NULL,
    duration_days           INT UNSIGNED  NULL,
    is_continuous_use       BOOLEAN       NOT NULL DEFAULT FALSE,
    administration_route_id INT UNSIGNED  NOT NULL,
    usage_deadline          DATE          NULL,
    notes                   TEXT          NULL,
    CONSTRAINT pk_prescription_items PRIMARY KEY (id),
    CONSTRAINT ck_prescription_items_dose CHECK (dose > 0),
    CONSTRAINT ck_prescription_items_duration CHECK (duration_days IS NULL OR duration_days > 0),
    CONSTRAINT fk_prescription_items_prescription
        FOREIGN KEY (prescription_id) REFERENCES prescriptions (id)
        ON UPDATE RESTRICT ON DELETE CASCADE,
    CONSTRAINT fk_prescription_items_medication
        FOREIGN KEY (medication_id) REFERENCES medications (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT fk_prescription_items_pharmaceutical_form
        FOREIGN KEY (pharmaceutical_form_id) REFERENCES pharmaceutical_forms (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT fk_prescription_items_dose_unit
        FOREIGN KEY (dose_unit_id) REFERENCES dose_units (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT fk_prescription_items_frequency
        FOREIGN KEY (frequency_id) REFERENCES frequencies (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT,
    CONSTRAINT fk_prescription_items_administration_route
        FOREIGN KEY (administration_route_id) REFERENCES administration_routes (id)
        ON UPDATE RESTRICT ON DELETE RESTRICT
) ENGINE = InnoDB;
