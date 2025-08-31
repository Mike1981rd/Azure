-- Script para crear las tablas básicas necesarias para el login

-- Tabla Users
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    email_confirmed BOOLEAN DEFAULT false,
    phone_number VARCHAR(20),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabla Roles
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    description TEXT,
    is_system_role BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabla UserRoles
CREATE TABLE IF NOT EXISTS user_roles (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id INTEGER NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, role_id)
);

-- Tabla Permissions
CREATE TABLE IF NOT EXISTS permissions (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    category VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tabla RolePermissions
CREATE TABLE IF NOT EXISTS role_permissions (
    id SERIAL PRIMARY KEY,
    role_id INTEGER NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_id INTEGER NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    UNIQUE(role_id, permission_id)
);

-- Tabla Companies (necesaria para algunas funciones)
CREATE TABLE IF NOT EXISTS companies (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(20),
    address TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insertar roles básicos
INSERT INTO roles (name, description, is_system_role) VALUES 
    ('SuperAdmin', 'Administrator with full access', true),
    ('Admin', 'Administrator', true),
    ('Editor', 'Content Editor', true),
    ('Viewer', 'Read-only access', true)
ON CONFLICT (name) DO NOTHING;

-- Crear usuario de prueba
INSERT INTO users (email, password_hash, first_name, last_name, is_active, email_confirmed) VALUES 
    ('test@websitebuilder.com', '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW', 'Test', 'User', true, true)
ON CONFLICT (email) DO UPDATE SET password_hash = '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW';

-- Asignar rol SuperAdmin al usuario de prueba
INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id 
FROM users u, roles r 
WHERE u.email = 'test@websitebuilder.com' AND r.name = 'SuperAdmin'
ON CONFLICT DO NOTHING;

-- Verificar que todo esté creado
SELECT 'Tables created:' as status;
SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';

SELECT 'Test user:' as status;
SELECT u.id, u.email, u.first_name, u.last_name, r.name as role
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.user_id
LEFT JOIN roles r ON ur.role_id = r.id
WHERE u.email = 'test@websitebuilder.com';