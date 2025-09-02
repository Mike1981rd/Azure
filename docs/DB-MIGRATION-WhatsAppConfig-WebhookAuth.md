WhatsAppConfig — Webhook Authorization Migration

Summary
- Migration: AddWebhookAuthFieldsToWhatsAppConfig
- Date: [pending]
- Fields added to WhatsAppConfigs:
  - HeaderName: varchar(100), default "Authorization"
  - HeaderValueTemplate: varchar(200), default "Bearer {secret}"
  - WebhookSecret: varchar(500), encrypted at rest (app-level)
  - LastWebhookEventAt: timestamp with time zone, nullable

PowerShell (Visual Studio PMC)
- Set default project to WebsiteBuilderAPI
- Add-Migration AddWebhookAuthFieldsToWhatsAppConfig
- Update-Database -Verbose

dotnet-ef (CLI)
- $Env:ASPNETCORE_ENVIRONMENT = "Development"
- dotnet tool install -g dotnet-ef  # if needed
- dotnet ef migrations add AddWebhookAuthFieldsToWhatsAppConfig
- dotnet ef database update

Verification (psql)
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema='public' AND table_name='WhatsAppConfigs'
  AND column_name IN ('HeaderName','HeaderValueTemplate','WebhookSecret','LastWebhookEventAt')
ORDER BY column_name;

Notes
- EF in this project uses quoted PascalCase identifiers (e.g., "HeaderName"), not snake_case.
- Manual ALTER TABLE statements were removed from SetupController to avoid drift; EF Core migrations are the source of truth.

