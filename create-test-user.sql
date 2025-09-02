-- Script para crear un usuario de prueba en PostgreSQL local
-- Password: 123456 (hash BCrypt)

-- Primero verificar si el usuario ya existe
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM users WHERE email = 'admin@websitebuilder.com') THEN
        INSERT INTO users (
            email,
            password_hash,
            first_name,
            last_name,
            is_active,
            email_confirmed,
            created_at,
            updated_at
        ) VALUES (
            'admin@websitebuilder.com',
            '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW', -- BCrypt hash de "123456"
            'Admin',
            'User',
            true,
            true,
            NOW(),
            NOW()
        );
        
        RAISE NOTICE 'Usuario admin@websitebuilder.com creado exitosamente';
    ELSE
        -- Si el usuario existe, actualizar el password
        UPDATE users 
        SET password_hash = '$2a$11$rBaVoWHpe/zLWPO.aJGiPOujFY6lxcp49obuPKBF8SySwqCJQgyTW',
            updated_at = NOW()
        WHERE email = 'admin@websitebuilder.com';
        
        RAISE NOTICE 'Password del usuario admin@websitebuilder.com actualizado';
    END IF;
END $$;

-- Asignar rol SuperAdmin al usuario
DO $$
DECLARE
    v_user_id INT;
    v_role_id INT;
BEGIN
    -- Obtener el ID del usuario
    SELECT id INTO v_user_id FROM users WHERE email = 'admin@websitebuilder.com';
    
    -- Obtener el ID del rol SuperAdmin (o crearlo si no existe)
    SELECT id INTO v_role_id FROM roles WHERE name = 'SuperAdmin';
    
    IF v_role_id IS NULL THEN
        INSERT INTO roles (name, description, is_system_role, created_at)
        VALUES ('SuperAdmin', 'Administrator with full access', true, NOW())
        RETURNING id INTO v_role_id;
    END IF;
    
    -- Asignar el rol al usuario si no lo tiene
    IF NOT EXISTS (SELECT 1 FROM user_roles WHERE user_id = v_user_id AND role_id = v_role_id) THEN
        INSERT INTO user_roles (user_id, role_id, assigned_at)
        VALUES (v_user_id, v_role_id, NOW());
        
        RAISE NOTICE 'Rol SuperAdmin asignado al usuario';
    END IF;
END $$;

SELECT 'Usuario de prueba configurado:' AS mensaje,
       email,
       first_name || ' ' || last_name AS nombre_completo,
       'Password: 123456' AS password
FROM users 
WHERE email = 'admin@websitebuilder.com';