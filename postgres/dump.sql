-- ============================================================================
-- Dump do banco de dados — repositorycontrol
-- Estrutura atual (schema) + usuário admin padrão.
--
-- Gerado a partir do banco de desenvolvimento em 2026-08-19.
-- Aplicado na inicialização do postgres de produção
-- (/docker-entrypoint-initdb.d).
--
-- Usuário admin padrão:
--   email: admin@ong.org
--   senha: admin
-- ============================================================================







SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;


-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -


CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;



-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: -


COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';



-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -


CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;



-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: -


COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';



-- Name: movement_type; Type: TYPE; Schema: public; Owner: -


CREATE TYPE public.movement_type AS ENUM (
    'entry',
    'exit'
);



-- Name: process_status; Type: TYPE; Schema: public; Owner: -


CREATE TYPE public.process_status AS ENUM (
    'active',
    'paused',
    'completed',
    'cancelled'
);



-- Name: update_updated_at(); Type: FUNCTION; Schema: public; Owner: -


CREATE FUNCTION public.update_updated_at() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$;


SET default_tablespace = '';

SET default_table_access_method = heap;


-- Name: categories; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.categories (
    id integer NOT NULL,
    name character varying(100) NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    unit character varying(50) DEFAULT 'unidades'::character varying NOT NULL
);



-- Name: categories_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.categories_id_seq OWNED BY public.categories.id;



-- Name: inbound_items; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.inbound_items (
    id integer NOT NULL,
    process_id integer NOT NULL,
    item_id integer,
    name character varying(255) NOT NULL,
    quantity integer DEFAULT 0 NOT NULL,
    unit character varying(50) DEFAULT 'unidades'::character varying NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    product_type_id integer,
    expiry_date date,
    CONSTRAINT inbound_items_quantity_check CHECK ((quantity >= 0))
);



-- Name: inbound_items_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.inbound_items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: inbound_items_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.inbound_items_id_seq OWNED BY public.inbound_items.id;



-- Name: inbound_processes; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.inbound_processes (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    description text DEFAULT ''::text NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    status public.process_status DEFAULT 'active'::public.process_status NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    type character varying(10) DEFAULT 'entry'::character varying NOT NULL
);



-- Name: inbound_processes_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.inbound_processes_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: inbound_processes_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.inbound_processes_id_seq OWNED BY public.inbound_processes.id;



-- Name: items; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.items (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    description text DEFAULT ''::text NOT NULL,
    category character varying(100) NOT NULL,
    quantity integer DEFAULT 0 NOT NULL,
    unit character varying(50) DEFAULT 'unidades'::character varying NOT NULL,
    min_quantity integer DEFAULT 0 NOT NULL,
    donor character varying(255) DEFAULT ''::character varying NOT NULL,
    entry_date date DEFAULT CURRENT_DATE NOT NULL,
    expiry_date date,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    product_type_id integer NOT NULL,
    CONSTRAINT items_min_quantity_check CHECK ((min_quantity >= 0)),
    CONSTRAINT items_quantity_check CHECK ((quantity >= 0))
);



-- Name: items_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.items_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: items_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.items_id_seq OWNED BY public.items.id;



-- Name: movements; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.movements (
    id integer NOT NULL,
    item_id integer NOT NULL,
    item_name character varying(255) NOT NULL,
    type public.movement_type NOT NULL,
    quantity integer NOT NULL,
    date timestamp with time zone DEFAULT now() NOT NULL,
    description text DEFAULT ''::text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    source text DEFAULT 'item'::text NOT NULL,
    CONSTRAINT movements_quantity_check CHECK ((quantity > 0))
);



-- Name: movements_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.movements_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: movements_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.movements_id_seq OWNED BY public.movements.id;



-- Name: notification_emails; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.notification_emails (
    id integer NOT NULL,
    email character varying(255) NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);



-- Name: notification_emails_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.notification_emails_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: notification_emails_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.notification_emails_id_seq OWNED BY public.notification_emails.id;



-- Name: notification_events; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.notification_events (
    id integer NOT NULL,
    event_key character varying(50) NOT NULL,
    enabled boolean DEFAULT true NOT NULL,
    label character varying(100) NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);



-- Name: notification_events_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.notification_events_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: notification_events_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.notification_events_id_seq OWNED BY public.notification_events.id;



-- Name: product_types; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.product_types (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    category character varying(100) DEFAULT 'Outros'::character varying NOT NULL
);



-- Name: product_types_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.product_types_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: product_types_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.product_types_id_seq OWNED BY public.product_types.id;



-- Name: users; Type: TABLE; Schema: public; Owner: -


CREATE TABLE public.users (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    email character varying(255) NOT NULL,
    password_hash character varying(512) NOT NULL,
    password_salt character varying(128) DEFAULT 'ong-salt-2026'::character varying NOT NULL,
    role character varying(50) DEFAULT 'admin'::character varying NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);



-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: -


CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;



-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -


ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;



-- Name: categories id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.categories ALTER COLUMN id SET DEFAULT nextval('public.categories_id_seq'::regclass);



-- Name: inbound_items id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.inbound_items ALTER COLUMN id SET DEFAULT nextval('public.inbound_items_id_seq'::regclass);



-- Name: inbound_processes id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.inbound_processes ALTER COLUMN id SET DEFAULT nextval('public.inbound_processes_id_seq'::regclass);



-- Name: items id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.items ALTER COLUMN id SET DEFAULT nextval('public.items_id_seq'::regclass);



-- Name: movements id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.movements ALTER COLUMN id SET DEFAULT nextval('public.movements_id_seq'::regclass);



-- Name: notification_emails id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.notification_emails ALTER COLUMN id SET DEFAULT nextval('public.notification_emails_id_seq'::regclass);



-- Name: notification_events id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.notification_events ALTER COLUMN id SET DEFAULT nextval('public.notification_events_id_seq'::regclass);



-- Name: product_types id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.product_types ALTER COLUMN id SET DEFAULT nextval('public.product_types_id_seq'::regclass);



-- Name: users id; Type: DEFAULT; Schema: public; Owner: -


ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);



-- Name: categories categories_name_key; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_name_key UNIQUE (name);



-- Name: categories categories_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.categories
    ADD CONSTRAINT categories_pkey PRIMARY KEY (id);



-- Name: inbound_items inbound_items_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.inbound_items
    ADD CONSTRAINT inbound_items_pkey PRIMARY KEY (id);



-- Name: inbound_processes inbound_processes_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.inbound_processes
    ADD CONSTRAINT inbound_processes_pkey PRIMARY KEY (id);



-- Name: items items_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_pkey PRIMARY KEY (id);



-- Name: movements movements_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.movements
    ADD CONSTRAINT movements_pkey PRIMARY KEY (id);



-- Name: notification_emails notification_emails_email_key; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.notification_emails
    ADD CONSTRAINT notification_emails_email_key UNIQUE (email);



-- Name: notification_emails notification_emails_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.notification_emails
    ADD CONSTRAINT notification_emails_pkey PRIMARY KEY (id);



-- Name: notification_events notification_events_event_key_key; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.notification_events
    ADD CONSTRAINT notification_events_event_key_key UNIQUE (event_key);



-- Name: notification_events notification_events_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.notification_events
    ADD CONSTRAINT notification_events_pkey PRIMARY KEY (id);



-- Name: product_types product_types_name_key; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.product_types
    ADD CONSTRAINT product_types_name_key UNIQUE (name);



-- Name: product_types product_types_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.product_types
    ADD CONSTRAINT product_types_pkey PRIMARY KEY (id);



-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);



-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);



-- Name: idx_inbound_items_process; Type: INDEX; Schema: public; Owner: -


CREATE INDEX idx_inbound_items_process ON public.inbound_items USING btree (process_id);



-- Name: idx_movements_date; Type: INDEX; Schema: public; Owner: -


CREATE INDEX idx_movements_date ON public.movements USING btree (date);



-- Name: idx_movements_item_id; Type: INDEX; Schema: public; Owner: -


CREATE INDEX idx_movements_item_id ON public.movements USING btree (item_id);



-- Name: idx_movements_type; Type: INDEX; Schema: public; Owner: -


CREATE INDEX idx_movements_type ON public.movements USING btree (type);



-- Name: items trg_items_updated_at; Type: TRIGGER; Schema: public; Owner: -


CREATE TRIGGER trg_items_updated_at BEFORE UPDATE ON public.items FOR EACH ROW EXECUTE FUNCTION public.update_updated_at();



-- Name: inbound_items inbound_items_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.inbound_items
    ADD CONSTRAINT inbound_items_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.items(id) ON DELETE SET NULL;



-- Name: inbound_items inbound_items_process_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.inbound_items
    ADD CONSTRAINT inbound_items_process_id_fkey FOREIGN KEY (process_id) REFERENCES public.inbound_processes(id) ON DELETE CASCADE;



-- Name: inbound_items inbound_items_product_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.inbound_items
    ADD CONSTRAINT inbound_items_product_type_id_fkey FOREIGN KEY (product_type_id) REFERENCES public.product_types(id) ON DELETE SET NULL;



-- Name: items items_product_type_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.items
    ADD CONSTRAINT items_product_type_id_fkey FOREIGN KEY (product_type_id) REFERENCES public.product_types(id) ON DELETE SET NULL;



-- Name: movements movements_item_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -


ALTER TABLE ONLY public.movements
    ADD CONSTRAINT movements_item_id_fkey FOREIGN KEY (item_id) REFERENCES public.items(id) ON DELETE CASCADE;







-- ============================================================
-- users - usuário admin padrão
-- ============================================================
INSERT INTO public.users (name, email, password_hash, password_salt, role) VALUES
    ('Administrador', 'admin@ong.org', encode(public.digest('admin' || 'ong-salt-2026', 'sha256'), 'base64'), 'ong-salt-2026', 'admin')
ON CONFLICT (email) DO NOTHING;
