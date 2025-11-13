-- Создаем отдельные базы данных для каждого микросервиса
CREATE DATABASE catalog_db;
CREATE DATABASE identity_db;
CREATE DATABASE ordering_db;
CREATE DATABASE review_db;
CREATE DATABASE payment_db;

-- Даем права пользователю postgres
GRANT ALL PRIVILEGES ON DATABASE catalog_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE identity_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE ordering_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE review_db TO postgres;
GRANT ALL PRIVILEGES ON DATABASE payment_db TO postgres;