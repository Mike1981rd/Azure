# 🗄️ MIGRATION REQUIRED - Customer Module

## Migration Name: `AddCustomerModels`
## Context: `ApplicationDbContext`

## Changes Included:
- ✅ Table `Customers` with all customer information fields
- ✅ Table `CustomerAddresses` for multiple addresses per customer
- ✅ Table `CustomerPaymentMethods` for payment cards
- ✅ Table `CustomerNotificationPreferences` for notification settings
- ✅ Table `CustomerDevices` for device tracking
- ✅ Table `CustomerWishlistItems` for wishlist
- ✅ Table `CustomerCoupons` for customer-specific coupons
- ✅ All foreign key relationships configured
- ✅ Unique indices on Email, Username, and CustomerId
- ✅ Soft delete support with DeletedAt field
- ✅ Added `Stock` field to Products table

## Commands to Execute:

### Option 1: Package Manager Console (Visual Studio)
```powershell
# 1. Create migration
Add-Migration AddCustomerModels -Context ApplicationDbContext

# 2. Review the generated migration file

# 3. Apply migration to database
Update-Database -Context ApplicationDbContext
```

### Option 2: .NET CLI (Terminal)
```bash
# 1. Create migration
dotnet ef migrations add AddCustomerModels --context ApplicationDbContext

# 2. Apply migration
dotnet ef database update --context ApplicationDbContext
```

## Post-Migration Steps:
1. Verify all tables were created correctly
2. Check that indices are in place
3. Confirm foreign key relationships
4. Test with sample data insertion

## Important Notes:
- This migration creates 7 new tables for the Customer module
- All tables include proper audit fields (CreatedAt, UpdatedAt)
- Customer passwords are stored as hashed values
- Payment card numbers are NOT stored - only last 4 digits
- Soft delete is implemented via DeletedAt field

## Verification Query:
```sql
-- Check if tables were created
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_name LIKE 'Customer%';
```

Please run the migration commands in Visual Studio Package Manager Console or via .NET CLI and confirm when completed.