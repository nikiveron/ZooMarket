-- Создаем отдельные базы данных для каждого микросервиса
CREATE DATABASE catalog;
CREATE DATABASE "user";
CREATE DATABASE "order";
CREATE DATABASE review;
CREATE DATABASE payment;

-- Даем права пользователю postgres
GRANT ALL PRIVILEGES ON DATABASE catalog TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "user" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "order" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE review TO postgres;
GRANT ALL PRIVILEGES ON DATABASE payment TO postgres;