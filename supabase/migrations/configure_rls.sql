-- Configure Row Level Security (RLS) for main tables

-- Enable RLS on main tables
ALTER TABLE companies ENABLE ROW LEVEL SECURITY;
ALTER TABLE orders ENABLE ROW LEVEL SECURITY;
ALTER TABLE global_theme_configs ENABLE ROW LEVEL SECURITY;
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE customers ENABLE ROW LEVEL SECURITY;
ALTER TABLE products ENABLE ROW LEVEL SECURITY;
ALTER TABLE collections ENABLE ROW LEVEL SECURITY;

-- Companies: Users can only see and update their own company
CREATE POLICY "Users can view own company"
ON companies FOR SELECT
USING (
  auth.uid()::text = user_id::text 
  OR auth.uid() IN (
    SELECT user_id::uuid FROM users 
    WHERE company_id = companies.id
  )
);

CREATE POLICY "Users can update own company"
ON companies FOR UPDATE
USING (
  auth.uid()::text = user_id::text 
  OR auth.uid() IN (
    SELECT user_id::uuid FROM users 
    WHERE company_id = companies.id 
    AND role_id IN (
      SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
    )
  )
);

-- Orders: Users can view orders from their company
CREATE POLICY "Users can view company orders"
ON orders FOR SELECT
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can create orders for their company"
ON orders FOR INSERT
WITH CHECK (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can update their company orders"
ON orders FOR UPDATE
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

-- Global Theme Configs: Users can manage their company's theme
CREATE POLICY "Users can view company theme"
ON global_theme_configs FOR SELECT
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can update company theme"
ON global_theme_configs FOR UPDATE
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
      AND role_id IN (
        SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
      )
  )
);

CREATE POLICY "Users can create company theme"
ON global_theme_configs FOR INSERT
WITH CHECK (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
      AND role_id IN (
        SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
      )
  )
);

-- Users table: Users can view users from their company
CREATE POLICY "Users can view company users"
ON users FOR SELECT
USING (
  company_id IN (
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can update own profile"
ON users FOR UPDATE
USING (
  user_id::uuid = auth.uid()
);

-- Customers: Users can manage their company's customers
CREATE POLICY "Users can view company customers"
ON customers FOR SELECT
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can create company customers"
ON customers FOR INSERT
WITH CHECK (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can update company customers"
ON customers FOR UPDATE
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

-- Products: Users can manage their company's products
CREATE POLICY "Users can view company products"
ON products FOR SELECT
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can create company products"
ON products FOR INSERT
WITH CHECK (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
      AND role_id IN (
        SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
      )
  )
);

CREATE POLICY "Users can update company products"
ON products FOR UPDATE
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
      AND role_id IN (
        SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
      )
  )
);

-- Collections: Users can manage their company's collections
CREATE POLICY "Users can view company collections"
ON collections FOR SELECT
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
  )
);

CREATE POLICY "Users can create company collections"
ON collections FOR INSERT
WITH CHECK (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
      AND role_id IN (
        SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
      )
  )
);

CREATE POLICY "Users can update company collections"
ON collections FOR UPDATE
USING (
  company_id IN (
    SELECT id FROM companies 
    WHERE user_id::text = auth.uid()::text
    UNION
    SELECT company_id FROM users 
    WHERE user_id::uuid = auth.uid()
      AND role_id IN (
        SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
      )
  )
);

-- Create payment_logs table if it doesn't exist
CREATE TABLE IF NOT EXISTS payment_logs (
  id SERIAL PRIMARY KEY,
  event_type VARCHAR(100) NOT NULL,
  event_data JSONB NOT NULL,
  created_at TIMESTAMP DEFAULT NOW()
);

-- Enable RLS on payment_logs
ALTER TABLE payment_logs ENABLE ROW LEVEL SECURITY;

-- Payment logs: Only service role can write, admins can read
CREATE POLICY "Admins can view payment logs"
ON payment_logs FOR SELECT
USING (
  auth.uid() IN (
    SELECT user_id::uuid FROM users 
    WHERE role_id IN (
      SELECT id FROM roles WHERE name IN ('SuperAdmin', 'Admin')
    )
  )
);