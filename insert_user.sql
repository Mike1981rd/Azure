INSERT INTO public."Users" (
    "Email", 
    "PasswordHash", 
    "Name", 
    "RoleId", 
    "IsActive", 
    "CreatedAt", 
    "UpdatedAt", 
    "CompanyId"
) VALUES (
    'support@purrteam.com',
    '$2a$11$rBPq7yPpmCMwRoUa5BQVFO0BjQQEYh6OnzqUVRxWNj5OYug77o5Gu',
    'Support Team',
    1,
    true,
    NOW(),
    NOW(),
    1
) ON CONFLICT ("Email") DO UPDATE SET 
    "PasswordHash" = EXCLUDED."PasswordHash",
    "UpdatedAt" = NOW();
