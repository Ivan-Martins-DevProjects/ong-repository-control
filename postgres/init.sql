CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================
-- users
-- ============================================================
CREATE TABLE IF NOT EXISTS users (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(255) NOT NULL,
    email           VARCHAR(255) NOT NULL UNIQUE,
    password_hash   VARCHAR(512) NOT NULL,
    password_salt   VARCHAR(128) NOT NULL DEFAULT 'ong-salt-2026',
    role            VARCHAR(50) NOT NULL DEFAULT 'admin',
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

INSERT INTO users (name, email, password_hash, password_salt, role) VALUES
    ('Administrador', 'admin@ong.org', encode(digest('admin' || 'ong-salt-2026', 'sha256'), 'base64'), 'ong-salt-2026', 'admin'),
    ('Maria Silva',    'maria@ong.org', encode(digest('maria123' || 'ong-salt-2026', 'sha256'), 'base64'), 'ong-salt-2026', 'viewer')
ON CONFLICT (email) DO NOTHING;

-- ============================================================
-- items
-- ============================================================
CREATE TABLE IF NOT EXISTS items (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(255) NOT NULL,
    product_type_id INTEGER NOT NULL REFERENCES product_types(id) ON DELETE CASCADE,
    description     TEXT NOT NULL DEFAULT '',
    category        VARCHAR(100) NOT NULL DEFAULT 'Outros',
    quantity        INTEGER NOT NULL DEFAULT 1 CHECK (quantity >= 0),
    unit            VARCHAR(50) NOT NULL DEFAULT 'unidades',
    min_quantity    INTEGER NOT NULL DEFAULT 0 CHECK (min_quantity >= 0),
    donor           VARCHAR(255) NOT NULL DEFAULT '',
    entry_date      DATE NOT NULL DEFAULT CURRENT_DATE,
    expiry_date     DATE,
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- ============================================================
-- movements (entrada/saída)
-- ============================================================
CREATE TYPE movement_type AS ENUM ('entry', 'exit');

CREATE TABLE IF NOT EXISTS movements (
    id              SERIAL PRIMARY KEY,
    item_id         INTEGER NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    item_name       VARCHAR(255) NOT NULL,
    type            movement_type NOT NULL,
    quantity        INTEGER NOT NULL CHECK (quantity > 0),
    date            TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    description     TEXT NOT NULL DEFAULT '',
    source          TEXT NOT NULL DEFAULT 'item',
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_movements_item_id ON movements(item_id);
CREATE INDEX idx_movements_date ON movements(date);
CREATE INDEX idx_movements_type ON movements(type);

-- ============================================================
-- categories
-- ============================================================
CREATE TABLE IF NOT EXISTS categories (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(100) NOT NULL UNIQUE,
    unit            VARCHAR(50) NOT NULL DEFAULT 'unidades',
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- seed default categories
INSERT INTO categories (name, unit) VALUES
    ('Alimentos', 'pacotes'),
    ('Higiene', 'unidades'),
    ('Vestuário', 'unidades'),
    ('Limpeza', 'unidades'),
    ('Outros', 'unidades')
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- notification_emails
-- ============================================================
CREATE TABLE IF NOT EXISTS notification_emails (
    id              SERIAL PRIMARY KEY,
    email           VARCHAR(255) NOT NULL UNIQUE,
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

INSERT INTO notification_emails (email) VALUES ('admin@ong.org')
ON CONFLICT (email) DO NOTHING;

-- ============================================================
-- notification_events (quais eventos disparam notificações)
-- ============================================================
CREATE TABLE IF NOT EXISTS notification_events (
    id              SERIAL PRIMARY KEY,
    event_key       VARCHAR(50) NOT NULL UNIQUE,
    enabled         BOOLEAN NOT NULL DEFAULT TRUE,
    label           VARCHAR(100) NOT NULL,
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

INSERT INTO notification_events (event_key, label) VALUES
    ('onEntry', 'Entrada de item'),
    ('onExit', 'Saída de item'),
    ('onExpiry', 'Vencimento de item')
ON CONFLICT (event_key) DO NOTHING;

-- ============================================================
-- product_types
-- ============================================================
CREATE TABLE IF NOT EXISTS product_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE,
    category VARCHAR(100) NOT NULL DEFAULT 'Outros',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

INSERT INTO product_types (name, category) VALUES
    ('Arroz', 'Alimentos'), ('Feijão', 'Alimentos'), ('Óleo de Soja', 'Alimentos'),
    ('Leite em Pó', 'Alimentos'), ('Fralda Infantil', 'Higiene'), ('Sabonete', 'Higiene'),
    ('Cobertor', 'Vestuário'), ('Macarrão', 'Alimentos'), ('Açúcar', 'Alimentos'),
    ('Café', 'Alimentos'), ('Farinha', 'Alimentos'), ('Biscoito', 'Alimentos'),
    ('Enlatado', 'Alimentos'), ('Suco', 'Alimentos'), ('Água Sanitária', 'Limpeza'),
    ('Detergente', 'Limpeza'), ('Sabão em Pó', 'Limpeza'), ('Escova Dental', 'Higiene'),
    ('Pasta Dental', 'Higiene'), ('Shampoo', 'Higiene')
ON CONFLICT (name) DO NOTHING;

-- ============================================================
-- seed movements for testing
-- ============================================================
INSERT INTO items (id, name, product_type_id, description, category, quantity, unit, min_quantity, donor, entry_date, expiry_date) VALUES
    -- Arroz
    (1,  'Arroz',       (SELECT id FROM product_types WHERE name='Arroz'),        'Solito 5Kg',       'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-06-01', '2027-06-01'),
    (2,  'Arroz',       (SELECT id FROM product_types WHERE name='Arroz'),        'Solito 5Kg',       'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-06-01', '2027-06-01'),
    (3,  'Arroz',       (SELECT id FROM product_types WHERE name='Arroz'),        'Namorado 5Kg',     'Alimentos', 1, 'unidades', 0, 'Campanha da Igreja', '2026-06-10', '2027-12-10'),
    (4,  'Arroz',       (SELECT id FROM product_types WHERE name='Arroz'),        'Namorado 5Kg',     'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-07-15', '2027-07-15'),
    (5,  'Arroz',       (SELECT id FROM product_types WHERE name='Arroz'),        'Bom Pastor',       'Alimentos', 1, 'unidades', 0, 'Supermercado Centro','2026-05-20', '2027-05-20'),
    (6,  'Arroz',       (SELECT id FROM product_types WHERE name='Arroz'),        'Tio João 5Kg',     'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-08-01', '2027-08-01'),
    (7,  'Arroz',       (SELECT id FROM product_types WHERE name='Arroz'),        'Prato Fino 5Kg',   'Alimentos', 1, 'unidades', 0, 'Campanha da Igreja', '2026-09-10', '2027-09-10'),
    -- Feijão
    (8,  'Feijão',      (SELECT id FROM product_types WHERE name='Feijão'),       'Carioca 1Kg',      'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-07-01', '2027-01-01'),
    (9,  'Feijão',      (SELECT id FROM product_types WHERE name='Feijão'),       'Preto 1Kg',        'Alimentos', 1, 'unidades', 0, 'Supermercado Centro','2026-07-05', '2027-03-01'),
    (10, 'Feijão',      (SELECT id FROM product_types WHERE name='Feijão'),       'Carioca 1Kg',      'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-08-15', '2027-05-15'),
    (11, 'Feijão',      (SELECT id FROM product_types WHERE name='Feijão'),       'Carioca 1Kg',      'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-09-01', '2027-03-01'),
    -- Óleo
    (12, 'Óleo de Soja',(SELECT id FROM product_types WHERE name='Óleo de Soja'), '900ml',            'Alimentos', 1, 'unidades', 0, 'Supermercado Centro','2026-05-20', '2027-05-20'),
    (13, 'Óleo de Soja',(SELECT id FROM product_types WHERE name='Óleo de Soja'), '900ml',            'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-06-15', '2027-06-15'),
    (14, 'Óleo de Soja',(SELECT id FROM product_types WHERE name='Óleo de Soja'), '900ml',            'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-08-10', '2027-08-10'),
    -- Leite
    (15, 'Leite em Pó', (SELECT id FROM product_types WHERE name='Leite em Pó'),  'Integral 400g',    'Alimentos', 1, 'unidades', 0, 'Farmácia Popular',   '2026-06-15', NULL),
    (16, 'Leite em Pó', (SELECT id FROM product_types WHERE name='Leite em Pó'),  'Integral 400g',    'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-07-20', NULL),
    (17, 'Leite em Pó', (SELECT id FROM product_types WHERE name='Leite em Pó'),  'Desnatado 400g',   'Alimentos', 1, 'unidades', 0, 'Farmácia Popular',   '2026-09-01', NULL),
    -- Macarrão
    (18, 'Macarrão',    (SELECT id FROM product_types WHERE name='Macarrão'),     'Espaguete 500g',   'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-07-10', '2028-01-10'),
    (19, 'Macarrão',    (SELECT id FROM product_types WHERE name='Macarrão'),     'Parafuso 500g',    'Alimentos', 1, 'unidades', 0, 'Supermercado Centro','2026-08-05', '2028-02-05'),
    (20, 'Macarrão',    (SELECT id FROM product_types WHERE name='Macarrão'),     'Espaguete 500g',   'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-09-10', '2028-03-10'),
    -- Cobertor
    (21, 'Cobertor',    (SELECT id FROM product_types WHERE name='Cobertor'),     'Casal',            'Vestuário', 1, 'unidades', 0, 'Campanha do Agasalho','2026-05-01', NULL),
    (22, 'Cobertor',    (SELECT id FROM product_types WHERE name='Cobertor'),     'Solteiro',         'Vestuário', 1, 'unidades', 0, 'Campanha do Agasalho','2026-05-01', NULL),
    (23, 'Cobertor',    (SELECT id FROM product_types WHERE name='Cobertor'),     'Casal',            'Vestuário', 1, 'unidades', 0, 'Doador Anônimo',     '2026-06-10', NULL),
    -- Fralda
    (24, 'Fralda Infantil',(SELECT id FROM product_types WHERE name='Fralda Infantil'),'Tamanho M',    'Higiene', 1, 'unidades', 0, 'Farmácia Popular',   '2026-06-15', NULL),
    (25, 'Fralda Infantil',(SELECT id FROM product_types WHERE name='Fralda Infantil'),'Tamanho G',    'Higiene', 1, 'unidades', 0, 'Doador Anônimo',     '2026-07-10', NULL),
    -- Sabonete
    (26, 'Sabonete',    (SELECT id FROM product_types WHERE name='Sabonete'),     'Líquido 200ml',    'Higiene', 1, 'unidades', 0, 'Doador Anônimo',     '2026-06-20', NULL),
    (27, 'Sabonete',    (SELECT id FROM product_types WHERE name='Sabonete'),     'Barra 90g',        'Higiene', 1, 'unidades', 0, 'Farmácia Popular',   '2026-08-10', NULL),
    -- Açúcar
    (28, 'Açúcar',      (SELECT id FROM product_types WHERE name='Açúcar'),       'Cristal 1Kg',      'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-07-15', '2027-07-15'),
    (29, 'Açúcar',      (SELECT id FROM product_types WHERE name='Açúcar'),       'Refinado 1Kg',     'Alimentos', 1, 'unidades', 0, 'Supermercado Centro','2026-08-20', '2027-08-20'),
    -- Café
    (30, 'Café',        (SELECT id FROM product_types WHERE name='Café'),         'Torrado 500g',     'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-08-01', '2027-02-01'),
    (31, 'Café',        (SELECT id FROM product_types WHERE name='Café'),         'Torrado 500g',     'Alimentos', 1, 'unidades', 0, 'Campanha da Igreja', '2026-09-05', '2027-03-05'),
    -- Farinha
    (32, 'Farinha',     (SELECT id FROM product_types WHERE name='Farinha'),      'Trigo 1Kg',        'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-07-01', '2027-01-01'),
    (33, 'Farinha',     (SELECT id FROM product_types WHERE name='Farinha'),      'Mandiooca 1Kg',    'Alimentos', 1, 'unidades', 0, 'Supermercado Centro','2026-08-15', '2027-02-15'),
    -- Biscoito
    (34, 'Biscoito',    (SELECT id FROM product_types WHERE name='Biscoito'),     'Maizena 200g',     'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-08-10', '2027-02-10'),
    (35, 'Biscoito',    (SELECT id FROM product_types WHERE name='Biscoito'),     'Recheado 150g',    'Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-09-05', '2027-03-05'),
    -- Enlatado
    (36, 'Enlatado',    (SELECT id FROM product_types WHERE name='Enlatado'),     'Sardinha 125g',    'Alimentos', 1, 'unidades', 0, 'Supermercado Centro','2026-07-20', '2028-07-20'),
    (37, 'Enlatado',    (SELECT id FROM product_types WHERE name='Enlatado'),     'Milho 200g',       'Alimentos', 1, 'unidades', 0, 'Doador Anônimo',     '2026-08-15', '2028-08-15'),
    (38, 'Enlatado',    (SELECT id FROM product_types WHERE name='Enlatado'),     'Molho Tomate 340g','Alimentos', 1, 'unidades', 0, 'Mercado Solidário',  '2026-09-01', '2028-03-01'),
    -- Limpeza
    (39, 'Água Sanitária',(SELECT id FROM product_types WHERE name='Água Sanitária'),'1L',           'Limpeza', 1, 'unidades', 0, 'Doador Anônimo',     '2026-07-01', '2027-07-01'),
    (40, 'Detergente',  (SELECT id FROM product_types WHERE name='Detergente'),   '500ml',            'Limpeza', 1, 'unidades', 0, 'Supermercado Centro','2026-08-01', '2027-08-01'),
    (41, 'Sabão em Pó', (SELECT id FROM product_types WHERE name='Sabão em Pó'),  '1Kg',              'Limpeza', 1, 'unidades', 0, 'Mercado Solidário',  '2026-08-15', '2027-08-15'),
    -- Higiene pessoal
    (42, 'Escova Dental',(SELECT id FROM product_types WHERE name='Escova Dental'),'Macia',           'Higiene', 1, 'unidades', 0, 'Farmácia Popular',   '2026-07-10', NULL),
    (43, 'Pasta Dental',(SELECT id FROM product_types WHERE name='Pasta Dental'), '90g',              'Higiene', 1, 'unidades', 0, 'Farmácia Popular',   '2026-07-10', '2027-07-10'),
    (44, 'Shampoo',     (SELECT id FROM product_types WHERE name='Shampoo'),      '200ml',            'Higiene', 1, 'unidades', 0, 'Doador Anônimo',     '2026-08-20', NULL)
ON CONFLICT (id) DO NOTHING;

INSERT INTO movements (item_id, item_name, type, quantity, date, description) VALUES
    -- Maio/2026
    (5,  'Arroz - Bom Pastor',      'entry', 15, '2026-05-10', 'Doação Supermercado Centro'),
    (12, 'Óleo de Soja - 900ml',    'entry', 20, '2026-05-20', 'Supermercado Centro'),
    (21, 'Cobertor - Casal',        'entry', 10, '2026-05-25', 'Campanha do Agasalho'),
    (22, 'Cobertor - Solteiro',     'entry', 15, '2026-05-25', 'Campanha do Agasalho'),
    -- Junho/2026
    (1,  'Arroz - Solito 5Kg',      'entry', 25, '2026-06-01', 'Doação Mercado Solidário'),
    (2,  'Arroz - Solito 5Kg',      'entry', 25, '2026-06-01', 'Doação Mercado Solidário'),
    (3,  'Arroz - Namorado 5Kg',    'entry', 10, '2026-06-10', 'Campanha da Igreja'),
    (7,  'Arroz - Prato Fino 5Kg',  'entry', 20, '2026-06-10', 'Campanha da Igreja'),
    (15, 'Leite em Pó - Integral',  'entry', 12, '2026-06-15', 'Farmácia Popular'),
    (24, 'Fralda Infantil - M',     'entry', 10, '2026-06-15', 'Farmácia Popular'),
    (26, 'Sabonete - Líquido',      'entry', 15, '2026-06-20', 'Doador Anônimo'),
    (1,  'Arroz - Solito 5Kg',      'exit',   8, '2026-06-15', 'Distribuição famílias cadastradas'),
    (5,  'Arroz - Bom Pastor',      'exit',   5, '2026-06-20', 'Cesta básica emergencial'),
    (8,  'Feijão - Carioca 1Kg',    'exit',  10, '2026-06-20', 'Cesta básica'),
    (12, 'Óleo de Soja - 900ml',    'exit',   6, '2026-06-25', 'Cesta básica'),
    -- Julho/2026
    (4,  'Arroz - Namorado 5Kg',    'entry', 10, '2026-07-15', 'Doador Anônimo'),
    (8,  'Feijão - Carioca 1Kg',    'entry', 15, '2026-07-01', 'Doador Anônimo'),
    (9,  'Feijão - Preto 1Kg',      'entry', 10, '2026-07-05', 'Supermercado Centro'),
    (13, 'Óleo de Soja - 900ml',    'entry', 15, '2026-06-15', 'Doador Anônimo'),
    (16, 'Leite em Pó - Integral',  'entry', 10, '2026-07-20', 'Doador Anônimo'),
    (18, 'Macarrão - Espaguete',    'entry', 30, '2026-07-10', 'Mercado Solidário'),
    (25, 'Fralda Infantil - G',     'entry',  8, '2026-07-10', 'Doador Anônimo'),
    (28, 'Açúcar - Cristal 1Kg',    'entry', 20, '2026-07-15', 'Mercado Solidário'),
    (32, 'Farinha - Trigo 1Kg',     'entry', 15, '2026-07-01', 'Mercado Solidário'),
    (36, 'Enlatado - Sardinha',     'entry', 12, '2026-07-20', 'Supermercado Centro'),
    (39, 'Água Sanitária - 1L',     'entry', 10, '2026-07-01', 'Doador Anônimo'),
    (42, 'Escova Dental',           'entry', 20, '2026-07-10', 'Farmácia Popular'),
    (43, 'Pasta Dental - 90g',      'entry', 20, '2026-07-10', 'Farmácia Popular'),
    (3,  'Arroz - Namorado 5Kg',    'exit',   5, '2026-07-10', 'Família Silva'),
    (8,  'Feijão - Carioca 1Kg',    'exit',   8, '2026-07-12', 'Distribuição semanal'),
    (15, 'Leite em Pó - Integral',  'exit',   4, '2026-07-15', 'Família com crianças'),
    (18, 'Macarrão - Espaguete',    'exit',  12, '2026-07-15', 'Distribuição semanal'),
    (26, 'Sabonete - Líquido',      'exit',   5, '2026-07-20', 'Kit higiene'),
    -- Agosto/2026
    (6,  'Arroz - Tio João 5Kg',    'entry', 30, '2026-08-01', 'Doador Anônimo'),
    (10, 'Feijão - Carioca 1Kg',    'entry', 20, '2026-08-15', 'Mercado Solidário'),
    (14, 'Óleo de Soja - 900ml',    'entry', 25, '2026-08-10', 'Mercado Solidário'),
    (19, 'Macarrão - Parafuso',     'entry', 20, '2026-08-05', 'Supermercado Centro'),
    (27, 'Sabonete - Barra 90g',    'entry', 30, '2026-08-10', 'Farmácia Popular'),
    (29, 'Açúcar - Refinado 1Kg',   'entry', 15, '2026-08-20', 'Supermercado Centro'),
    (30, 'Café - Torrado 500g',     'entry', 15, '2026-08-01', 'Doador Anônimo'),
    (33, 'Farinha - Mandioca 1Kg',  'entry', 10, '2026-08-15', 'Supermercado Centro'),
    (34, 'Biscoito - Maizena 200g', 'entry', 25, '2026-08-10', 'Doador Anônimo'),
    (37, 'Enlatado - Milho 200g',   'entry', 15, '2026-08-15', 'Doador Anônimo'),
    (40, 'Detergente - 500ml',      'entry', 20, '2026-08-01', 'Supermercado Centro'),
    (41, 'Sabão em Pó - 1Kg',       'entry', 15, '2026-08-15', 'Mercado Solidário'),
    (44, 'Shampoo - 200ml',         'entry', 10, '2026-08-20', 'Doador Anônimo'),
    (2,  'Arroz - Solito 5Kg',      'exit',  10, '2026-08-05', 'Cesta básica'),
    (9,  'Feijão - Preto 1Kg',      'exit',   5, '2026-08-10', 'Família Oliveira'),
    (13, 'Óleo de Soja - 900ml',    'exit',   8, '2026-08-15', 'Cesta básica'),
    (28, 'Açúcar - Cristal 1Kg',    'exit',   6, '2026-08-20', 'Distribuição mensal'),
    -- Setembro/2026
    (7,  'Arroz - Prato Fino 5Kg',  'entry', 15, '2026-09-10', 'Campanha da Igreja'),
    (11, 'Feijão - Carioca 1Kg',    'entry', 18, '2026-09-01', 'Doador Anônimo'),
    (17, 'Leite em Pó - Desnatado', 'entry', 10, '2026-09-01', 'Farmácia Popular'),
    (20, 'Macarrão - Espaguete',    'entry', 20, '2026-09-10', 'Doador Anônimo'),
    (31, 'Café - Torrado 500g',     'entry', 12, '2026-09-05', 'Campanha da Igreja'),
    (35, 'Biscoito - Recheado 150g','entry', 20, '2026-09-05', 'Mercado Solidário'),
    (38, 'Enlatado - Molho Tomate', 'entry', 18, '2026-09-01', 'Mercado Solidário'),
    (4,  'Arroz - Namorado 5Kg',    'exit',   3, '2026-09-05', 'Família Pereira'),
    (10, 'Feijão - Carioca 1Kg',    'exit',   6, '2026-09-15', 'Distribuição quinzenal'),
    (30, 'Café - Torrado 500g',     'exit',   4, '2026-09-20', 'Kit café da manhã'),
    (34, 'Biscoito - Maizena 200g', 'exit',   8, '2026-09-25', 'Lanches crianças'),
    -- Outubro/2026
    (6,  'Arroz - Tio João 5Kg',    'exit',   8, '2026-10-05', 'Distribuição outubro'),
    (11, 'Feijão - Carioca 1Kg',    'exit',   5, '2026-10-10', 'Cesta básica'),
    (16, 'Leite em Pó - Integral',  'exit',   3, '2026-10-15', 'Família crianças'),
    (27, 'Sabonete - Barra 90g',    'exit',  10, '2026-10-20', 'Kit higiene'),
    (32, 'Farinha - Trigo 1Kg',     'exit',   6, '2026-10-25', 'Distribuição mensal')
ON CONFLICT (id) DO NOTHING;

-- ============================================================
-- auto-update updated_at trigger
-- ============================================================
CREATE OR REPLACE FUNCTION update_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_items_updated_at ON items;
CREATE TRIGGER trg_items_updated_at
    BEFORE UPDATE ON items
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at();

-- ============================================================
-- inbound_processes (contagem de entrada/saída)
-- ============================================================
CREATE TYPE process_status AS ENUM ('active', 'paused', 'completed', 'cancelled');

CREATE TABLE IF NOT EXISTS inbound_processes (
    id              SERIAL PRIMARY KEY,
    name            VARCHAR(255) NOT NULL,
    description     TEXT NOT NULL DEFAULT '',
    start_date      DATE NOT NULL,
    end_date        DATE NOT NULL,
    status          process_status NOT NULL DEFAULT 'active',
    type            VARCHAR(10) NOT NULL DEFAULT 'entry',
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS inbound_items (
    id              SERIAL PRIMARY KEY,
    process_id      INTEGER NOT NULL REFERENCES inbound_processes(id) ON DELETE CASCADE,
    product_type_id INTEGER REFERENCES product_types(id) ON DELETE SET NULL,
    item_id         INTEGER REFERENCES items(id) ON DELETE SET NULL,
    name            VARCHAR(255) NOT NULL,
    quantity        INTEGER NOT NULL DEFAULT 0 CHECK (quantity >= 0),
    unit            VARCHAR(50) NOT NULL DEFAULT 'unidades',
    expiry_date     DATE,
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_inbound_items_process ON inbound_items(process_id);

-- ============================================================
-- seed inbound processes and items
-- ============================================================
INSERT INTO inbound_processes (id, name, description, start_date, end_date, status, type) VALUES
    (1, 'Doação Julho 2026',     'Campanha da igreja',              '2026-07-01', '2026-07-31', 'active',    'entry'),
    (2, 'Cestas Básicas Junho',  'Montagem de cestas para junho',   '2026-06-01', '2026-06-30', 'completed', 'entry'),
    (3, 'Arrecadação Escolas',   'Materiais e alimentos',           '2026-08-01', '2026-08-30', 'active',    'entry'),
    (4, 'Campanha do Agasalho',  'Roupas de inverno',               '2026-05-01', '2026-06-30', 'completed', 'entry'),
    (5, 'Saída Emergencial',     'Distribuição urgente para famílias','2026-07-18', '2026-07-25', 'paused',   'exit'),
    (6, 'Doação Supermercado',   'Parceria com o Supermercado',     '2026-08-15', '2026-09-15', 'active',    'entry'),
    (7, 'Entrega Famílias Julho','Cestas para 20 famílias',         '2026-07-10', '2026-07-20', 'completed', 'exit'),
    (8, 'Campanha Natal 2026',   'Arrecadação de fim de ano',       '2026-11-01', '2026-12-25', 'active',    'entry'),
    (9, 'Doação Agosto',         'Campanha mensal agosto',          '2026-08-01', '2026-08-31', 'active',    'entry'),
    (10, 'Kit Higiene Pessoal',  'Arrecadação de produtos de higiene','2026-09-01','2026-09-30', 'active',   'entry'),
    (11, 'Cestas Setembro',      'Distribuição de setembro',        '2026-09-01', '2026-09-15', 'paused',   'exit'),
    (12, 'Doação Farmácia',      'Parceria Farmácia Popular',       '2026-07-01', '2026-07-31', 'completed', 'entry')
ON CONFLICT (id) DO NOTHING;

INSERT INTO inbound_items (process_id, product_type_id, name, quantity, unit, expiry_date) VALUES
    -- Processo 1: Doação Julho 2026 (active, entry)
    (1, (SELECT id FROM product_types WHERE name='Arroz'),        'Arroz Solito 5Kg',        1, 'unidades', '2027-06-01'),
    (1, (SELECT id FROM product_types WHERE name='Arroz'),        'Arroz Solito 5Kg',        1, 'unidades', '2027-06-01'),
    (1, (SELECT id FROM product_types WHERE name='Feijão'),       'Feijão Carioca 1Kg',      1, 'unidades', '2027-01-01'),
    (1, (SELECT id FROM product_types WHERE name='Leite em Pó'),  'Leite Integral 400g',     1, 'unidades', NULL),
    (1, (SELECT id FROM product_types WHERE name='Óleo de Soja'), 'Óleo de Soja 900ml',      1, 'unidades', '2027-05-20'),
    (1, (SELECT id FROM product_types WHERE name='Açúcar'),       'Açúcar Cristal 1Kg',      1, 'unidades', '2027-07-15'),
    (1, (SELECT id FROM product_types WHERE name='Biscoito'),     'Biscoito Maizena 200g',   1, 'unidades', '2027-02-10'),
    -- Processo 2: Cestas Básicas Junho (completed, entry)
    (2, (SELECT id FROM product_types WHERE name='Arroz'),        'Arroz Solito 5Kg',        1, 'unidades', '2027-06-01'),
    (2, (SELECT id FROM product_types WHERE name='Feijão'),       'Feijão Carioca 1Kg',      1, 'unidades', '2027-01-01'),
    (2, (SELECT id FROM product_types WHERE name='Macarrão'),     'Macarrão Espaguete 500g', 1, 'unidades', '2028-01-10'),
    (2, (SELECT id FROM product_types WHERE name='Óleo de Soja'), 'Óleo de Soja 900ml',      1, 'unidades', '2027-05-20'),
    (2, (SELECT id FROM product_types WHERE name='Farinha'),      'Farinha Trigo 1Kg',       1, 'unidades', '2027-01-01'),
    -- Processo 3: Arrecadação Escolas (active, entry)
    (3, (SELECT id FROM product_types WHERE name='Arroz'),        'Arroz Solito 5Kg',        1, 'unidades', '2027-12-10'),
    (3, (SELECT id FROM product_types WHERE name='Macarrão'),     'Macarrão Espaguete 500g', 1, 'unidades', '2028-01-10'),
    (3, (SELECT id FROM product_types WHERE name='Feijão'),       'Feijão Preto 1Kg',        1, 'unidades', '2027-03-15'),
    (3, (SELECT id FROM product_types WHERE name='Sabonete'),     'Sabonete Barra 90g',      1, 'unidades', NULL),
    (3, (SELECT id FROM product_types WHERE name='Fralda Infantil'),'Fralda Tamanho M',      1, 'unidades', NULL),
    (3, (SELECT id FROM product_types WHERE name='Óleo de Soja'), 'Óleo de Soja 900ml',      1, 'unidades', '2027-08-10'),
    (3, (SELECT id FROM product_types WHERE name='Biscoito'),     'Biscoito Recheado 150g',  1, 'unidades', '2027-03-05'),
    (3, (SELECT id FROM product_types WHERE name='Enlatado'),     'Sardinha 125g',           1, 'unidades', '2028-07-20'),
    -- Processo 4: Campanha do Agasalho (completed, entry)
    (4, (SELECT id FROM product_types WHERE name='Cobertor'),     'Cobertor Casal',          1, 'unidades', NULL),
    (4, (SELECT id FROM product_types WHERE name='Cobertor'),     'Cobertor Casal',          1, 'unidades', NULL),
    (4, (SELECT id FROM product_types WHERE name='Cobertor'),     'Cobertor Solteiro',       1, 'unidades', NULL),
    (4, (SELECT id FROM product_types WHERE name='Cobertor'),     'Cobertor Solteiro',       1, 'unidades', NULL),
    -- Processo 6: Doação Supermercado (active, entry)
    (6, (SELECT id FROM product_types WHERE name='Arroz'),        'Arroz Namorado 5Kg',      1, 'unidades', '2027-12-10'),
    (6, (SELECT id FROM product_types WHERE name='Feijão'),       'Feijão Carioca 1Kg',      1, 'unidades', '2027-07-01'),
    (6, (SELECT id FROM product_types WHERE name='Leite em Pó'),  'Leite Integral 400g',     1, 'unidades', NULL),
    (6, (SELECT id FROM product_types WHERE name='Macarrão'),     'Macarrão Parafuso 500g',  1, 'unidades', '2028-06-15'),
    (6, (SELECT id FROM product_types WHERE name='Açúcar'),       'Açúcar Refinado 1Kg',     1, 'unidades', '2027-08-20'),
    (6, (SELECT id FROM product_types WHERE name='Detergente'),   'Detergente 500ml',        1, 'unidades', '2027-08-01'),
    (6, (SELECT id FROM product_types WHERE name='Sabão em Pó'),  'Sabão em Pó 1Kg',         1, 'unidades', '2027-08-15'),
    -- Processo 8: Campanha Natal 2026 (active, entry)
    (8, (SELECT id FROM product_types WHERE name='Arroz'),        'Arroz Bom Pastor',        1, 'unidades', '2027-05-20'),
    (8, (SELECT id FROM product_types WHERE name='Feijão'),       'Feijão Carioca 1Kg',      1, 'unidades', '2027-09-01'),
    (8, (SELECT id FROM product_types WHERE name='Óleo de Soja'), 'Óleo de Soja 900ml',      1, 'unidades', '2027-11-20'),
    (8, (SELECT id FROM product_types WHERE name='Cobertor'),     'Cobertor Casal',          1, 'unidades', NULL),
    (8, (SELECT id FROM product_types WHERE name='Sabonete'),     'Sabonete Líquido 200ml',  1, 'unidades', NULL),
    (8, (SELECT id FROM product_types WHERE name='Café'),         'Café Torrado 500g',       1, 'unidades', '2027-02-01'),
    (8, (SELECT id FROM product_types WHERE name='Enlatado'),     'Milho 200g',              1, 'unidades', '2028-08-15'),
    (8, (SELECT id FROM product_types WHERE name='Fralda Infantil'),'Fralda Tamanho G',      1, 'unidades', NULL),
    -- Processo 9: Doação Agosto (active, entry)
    (9, (SELECT id FROM product_types WHERE name='Arroz'),        'Arroz Tio João 5Kg',      1, 'unidades', '2027-08-01'),
    (9, (SELECT id FROM product_types WHERE name='Feijão'),       'Feijão Carioca 1Kg',      1, 'unidades', '2027-05-15'),
    (9, (SELECT id FROM product_types WHERE name='Macarrão'),     'Macarrão Espaguete 500g', 1, 'unidades', '2028-03-10'),
    (9, (SELECT id FROM product_types WHERE name='Óleo de Soja'), 'Óleo de Soja 900ml',      1, 'unidades', '2027-08-10'),
    (9, (SELECT id FROM product_types WHERE name='Farinha'),      'Farinha Mandioca 1Kg',    1, 'unidades', '2027-02-15'),
    (9, (SELECT id FROM product_types WHERE name='Sabão em Pó'),  'Sabão em Pó 1Kg',         1, 'unidades', '2027-08-15'),
    -- Processo 10: Kit Higiene Pessoal (active, entry)
    (10, (SELECT id FROM product_types WHERE name='Sabonete'),    'Sabonete Barra 90g',      1, 'unidades', NULL),
    (10, (SELECT id FROM product_types WHERE name='Sabonete'),    'Sabonete Barra 90g',      1, 'unidades', NULL),
    (10, (SELECT id FROM product_types WHERE name='Escova Dental'),'Escova Dental Macia',    1, 'unidades', NULL),
    (10, (SELECT id FROM product_types WHERE name='Pasta Dental'), 'Pasta Dental 90g',        1, 'unidades', '2027-07-10'),
    (10, (SELECT id FROM product_types WHERE name='Shampoo'),     'Shampoo 200ml',           1, 'unidades', NULL),
    (10, (SELECT id FROM product_types WHERE name='Fralda Infantil'),'Fralda Tamanho M',     1, 'unidades', NULL),
    (10, (SELECT id FROM product_types WHERE name='Sabonete'),    'Sabonete Líquido 200ml',  1, 'unidades', NULL),
    -- Processo 12: Doação Farmácia (completed, entry)
    (12, (SELECT id FROM product_types WHERE name='Leite em Pó'), 'Leite Integral 400g',     1, 'unidades', NULL),
    (12, (SELECT id FROM product_types WHERE name='Fralda Infantil'),'Fralda Tamanho M',     1, 'unidades', NULL),
    (12, (SELECT id FROM product_types WHERE name='Sabonete'),    'Sabonete Barra 90g',      1, 'unidades', NULL),
    (12, (SELECT id FROM product_types WHERE name='Escova Dental'),'Escova Dental Macia',    1, 'unidades', NULL),
    (12, (SELECT id FROM product_types WHERE name='Pasta Dental'), 'Pasta Dental 90g',        1, 'unidades', '2027-07-10')
ON CONFLICT DO NOTHING;