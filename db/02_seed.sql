-- =============================================================
-- Medical prescription database - initial data
-- All people and institutions below are fictitious.
-- =============================================================

-- Script file is UTF-8: the client must send it as utf8mb4
-- (otherwise accented text is stored double-encoded).
SET NAMES utf8mb4;

USE prescription_db;

INSERT INTO pharmaceutical_forms (id, name) VALUES
    (1, 'Comprimido'),
    (2, 'Cápsula'),
    (3, 'Solução oral'),
    (4, 'Suspensão oral'),
    (5, 'Solução injetável'),
    (6, 'Creme'),
    (7, 'Pomada'),
    (8, 'Gotas'),
    (9, 'Xarope'),
    (10, 'Aerossol');

INSERT INTO administration_routes (id, name) VALUES
    (1, 'Oral'),
    (2, 'Sublingual'),
    (3, 'Intravenosa'),
    (4, 'Intramuscular'),
    (5, 'Subcutânea'),
    (6, 'Tópica'),
    (7, 'Inalatória'),
    (8, 'Oftálmica'),
    (9, 'Nasal'),
    (10, 'Retal');

INSERT INTO dose_units (id, abbreviation, name) VALUES
    (1, 'mg', 'Miligrama'),
    (2, 'g', 'Grama'),
    (3, 'mcg', 'Micrograma'),
    (4, 'mL', 'Mililitro'),
    (5, 'gota', 'Gota'),
    (6, 'comp', 'Comprimido'),
    (7, 'cáps', 'Cápsula'),
    (8, 'UI', 'Unidade internacional'),
    (9, 'jato', 'Jato'),
    (10, 'aplic', 'Aplicação');

INSERT INTO frequencies (id, name) VALUES
    (1, '1 vez ao dia (24 em 24 horas)'),
    (2, '2 vezes ao dia (12 em 12 horas)'),
    (3, '3 vezes ao dia (8 em 8 horas)'),
    (4, '4 vezes ao dia (6 em 6 horas)'),
    (5, '6 vezes ao dia (4 em 4 horas)'),
    (6, '1 vez por semana'),
    (7, 'Dose única'),
    (8, 'Se necessário');

INSERT INTO validity_periods (id, name, days) VALUES
    (1, 'Sem prazo', NULL),
    (2, 'Antimicrobiano - 10 dias', 10),
    (3, 'Antimicrobiano - tratamento prolongado - 90 dias', 90),
    (4, 'Controle especial - 30 dias', 30);

INSERT INTO institutions
    (id, name, cnes, address_street, address_number, address_complement,
     address_district, address_city, address_state, address_zip_code, phone)
VALUES
    (1, 'Clínica Vida Plena', '1234567', 'Rua das Acácias', '120', NULL,
     'Jardim Paulista', 'São Paulo', 'SP', '01415000', '(11) 3000-1001'),
    (2, 'Hospital Santa Clara', '2345678', 'Avenida Central', '1500', 'Bloco B',
     'Centro', 'Campinas', 'SP', '13010000', '(19) 3200-2002'),
    (3, 'Centro Médico Horizonte', '3456789', 'Rua dos Ipês', '45', 'Sala 302',
     'Savassi', 'Belo Horizonte', 'MG', '30140000', '(31) 3300-3003'),
    (4, 'Policlínica Bem-Estar', '4567890', 'Avenida Atlântica', '800', NULL,
     'Copacabana', 'Rio de Janeiro', 'RJ', '22021000', '(21) 3400-4004');

INSERT INTO professionals (id, full_name, council, council_number, council_state) VALUES
    (1, 'Ana Beatriz Moreira', 'CRM', '123456', 'SP'),
    (2, 'Carlos Eduardo Lima', 'CRM', '234567', 'SP'),
    (3, 'Fernanda Souza Ramos', 'CRM', '345678', 'MG'),
    (4, 'Ricardo Alves Pinto', 'CRO', '456789', 'RJ');

INSERT INTO clients
    (id, full_name, birth_date, sex, address_street, address_number, address_complement,
     address_district, address_city, address_state, address_zip_code)
VALUES
    (1, 'João Pedro Silva', '1980-03-15', 'M', 'Rua Augusta', '500', 'Apto 12',
     'Consolação', 'São Paulo', 'SP', '01304000'),
    (2, 'Maria Clara Oliveira', '1992-07-22', 'F', 'Rua Barão de Jaguara', '210', NULL,
     'Centro', 'Campinas', 'SP', '13015000'),
    (3, 'Lucas Gabriel Santos', '2015-11-02', 'M', 'Rua Pernambuco', '88', NULL,
     'Funcionários', 'Belo Horizonte', 'MG', '30130150'),
    (4, 'Helena Martins Costa', '1955-01-30', 'F', 'Rua Barata Ribeiro', '330', 'Apto 801',
     'Copacabana', 'Rio de Janeiro', 'RJ', '22040000'),
    (5, 'Rafael Almeida Rocha', '1973-09-09', 'M', 'Avenida Paulista', '1000', 'Conj. 45',
     'Bela Vista', 'São Paulo', 'SP', '01310100'),
    (6, 'Beatriz Fernandes Dias', '2001-05-18', 'F', 'Rua Gonçalves Dias', '75', NULL,
     'Lourdes', 'Belo Horizonte', 'MG', '30140090');

INSERT INTO medications
    (id, name, active_ingredient, concentration, pharmaceutical_form_id,
     validity_period_id, is_controlled, is_antimicrobial, is_active)
VALUES
    (1,  'Dipirona Sódica', 'Dipirona sódica', '500 mg', 1, 1, FALSE, FALSE, TRUE),
    (2,  'Dipirona Sódica', 'Dipirona sódica', '500 mg/mL', 8, 1, FALSE, FALSE, TRUE),
    (3,  'Paracetamol', 'Paracetamol', '750 mg', 1, 1, FALSE, FALSE, TRUE),
    (4,  'Ibuprofeno', 'Ibuprofeno', '600 mg', 1, 1, FALSE, FALSE, TRUE),
    (5,  'Amoxicilina', 'Amoxicilina', '500 mg', 2, 2, FALSE, TRUE, TRUE),
    (6,  'Amoxicilina', 'Amoxicilina', '250 mg/5 mL', 4, 2, FALSE, TRUE, TRUE),
    (7,  'Azitromicina', 'Azitromicina', '500 mg', 1, 2, FALSE, TRUE, TRUE),
    (8,  'Cefalexina', 'Cefalexina', '500 mg', 2, 2, FALSE, TRUE, TRUE),
    (9,  'Losartana Potássica', 'Losartana potássica', '50 mg', 1, 1, FALSE, FALSE, TRUE),
    (10, 'Hidroclorotiazida', 'Hidroclorotiazida', '25 mg', 1, 1, FALSE, FALSE, TRUE),
    (11, 'Metformina', 'Cloridrato de metformina', '850 mg', 1, 1, FALSE, FALSE, TRUE),
    (12, 'Omeprazol', 'Omeprazol', '20 mg', 2, 1, FALSE, FALSE, TRUE),
    (13, 'Sinvastatina', 'Sinvastatina', '20 mg', 1, 1, FALSE, FALSE, TRUE),
    (14, 'Clonazepam', 'Clonazepam', '2 mg', 1, 4, TRUE, FALSE, TRUE),
    (15, 'Clonazepam', 'Clonazepam', '2,5 mg/mL', 8, 4, TRUE, FALSE, TRUE),
    (16, 'Sertralina', 'Cloridrato de sertralina', '50 mg', 1, 4, TRUE, FALSE, TRUE),
    (17, 'Salbutamol', 'Sulfato de salbutamol', '100 mcg/dose', 10, 1, FALSE, FALSE, TRUE),
    (18, 'Dexametasona', 'Dexametasona', '1 mg/g', 6, 1, FALSE, FALSE, TRUE),
    (19, 'Insulina NPH', 'Insulina humana NPH', '100 UI/mL', 5, 1, FALSE, FALSE, TRUE),
    (20, 'Loratadina', 'Loratadina', '1 mg/mL', 9, 1, FALSE, FALSE, TRUE);

INSERT INTO favorite_medications (professional_id, medication_id) VALUES
    (1, 1), (1, 5), (1, 9),
    (2, 3), (2, 7), (2, 14),
    (3, 6), (3, 17), (3, 20),
    (4, 4), (4, 5);

INSERT INTO prescription_templates (id, professional_id, title, content) VALUES
    (1, 1, 'Analgesia simples',
     'Dipirona sódica 500 mg - tomar 1 comprimido via oral de 6 em 6 horas, se dor ou febre, por até 3 dias.'),
    (2, 1, 'Orientações de hidratação',
     'Ingerir no mínimo 2 litros de água por dia. Retornar em caso de piora dos sintomas.'),
    (3, 2, 'Pós-procedimento',
     'Repouso relativo por 48 horas. Paracetamol 750 mg - tomar 1 comprimido via oral de 8 em 8 horas, se dor.'),
    (4, 4, 'Pós-extração dentária',
     'Ibuprofeno 600 mg - tomar 1 comprimido via oral de 8 em 8 horas por 3 dias. Compressa fria nas primeiras 24 horas.');

INSERT INTO prescriptions
    (id, client_id, professional_id, institution_id, format, category, free_text, issued_at, printed_at)
VALUES
    (1, 1, 1, 1, 'UNSTRUCTURED', 'COMMON',
     'Dipirona sódica 500 mg - tomar 1 comprimido via oral de 6 em 6 horas, se dor ou febre, por até 3 dias.',
     '2026-10-01 09:30:00', '2026-10-01 09:45:00'),
    (2, 2, 2, 2, 'STRUCTURED', 'ANTIMICROBIAL', NULL, '2026-10-02 14:10:00', '2026-10-02 14:25:00'),
    (3, 4, 3, 3, 'STRUCTURED', 'COMMON', NULL, '2026-10-03 10:45:00', NULL),
    (4, 5, 2, 2, 'STRUCTURED', 'SPECIAL_CONTROL', NULL, '2026-10-05 16:20:00', NULL);

INSERT INTO prescription_items
    (prescription_id, medication_id, medication_name, pharmaceutical_form_id, dose, dose_unit_id,
     frequency_id, start_date, duration_days, is_continuous_use, administration_route_id,
     usage_deadline, notes)
VALUES
    (2, 7, 'Azitromicina 500 mg', 1, 1, 6,
     1, '2026-10-02', 5, FALSE, 1, '2026-10-12','Tomar 1 hora antes ou 2 horas após as refeições.'),
    (3, 9, 'Losartana Potássica 50 mg', 1, 1, 6,
     1, '2026-10-03', NULL, TRUE, 1, NULL, NULL),
    (3, 10, 'Hidroclorotiazida 25 mg', 1, 1, 6,
     1, '2026-10-03', NULL, TRUE, 1, NULL, 'Tomar pela manhã.'),
    (3, NULL, 'Cápsula manipulada de vitamina D3 7.000 UI', 2, 1, 7,
     6, '2026-10-03', 56, FALSE, 1, NULL,'Medicamento manipulado.'),
    (4, 14, 'Clonazepam 2 mg', 1, 1, 6,
     1, '2026-10-05', 30, FALSE, 1, '2026-11-04','Tomar à noite.');
