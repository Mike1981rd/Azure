# Shipping Zones and Rates Implementation

## Overview
- **Purpose**: Implement a comprehensive shipping management system with zones and rates
- **Scope**: Create, read, update, delete shipping zones with multiple rate types per zone
- **Dependencies**: Entity Framework Core, Npgsql, PostgreSQL JSONB support
- **Date Implemented**: 2025-08-07

## Architecture Decisions

### Pattern Used
- **Service Layer Pattern**: ShippingService handles all business logic
- **Repository Pattern**: Not used (direct DbContext access for simplicity)
- **DTO Pattern**: Separate DTOs for Create, Update, and Bulk operations

### Technology Choices
- **PostgreSQL JSONB**: Store countries list as JSON for flexibility
- **Npgsql Dynamic JSON**: Enable dynamic serialization for complex types
- **Bulk Update**: Single endpoint to update all zones and rates at once

### Security Considerations
- JWT Authentication required on all endpoints
- Company-scoped data access (single-tenant)
- Permission attributes ready but temporarily disabled

## Implementation Details

### Backend

#### Models Created
```csharp
// Models/ShippingZone.cs
public class ShippingZone
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; }
    public string ZoneType { get; set; } // domestic, international, custom
    public List<string> Countries { get; set; } // Stored as JSONB
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public List<ShippingRate> Rates { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Models/ShippingRate.cs
public class ShippingRate
{
    public int Id { get; set; }
    public int ShippingZoneId { get; set; }
    public string RateType { get; set; } // weight, vat, duty
    public string? Condition { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public ShippingZone ShippingZone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### API Endpoints
- `GET /api/shipping/zones` - Get all zones with rates
- `GET /api/shipping/zones/{id}` - Get specific zone
- `POST /api/shipping/zones` - Create new zone with default rates
- `PUT /api/shipping/zones/{id}` - Update zone
- `DELETE /api/shipping/zones/{id}` - Delete zone
- `POST /api/shipping/zones/{zoneId}/rates` - Add rate to zone
- `PUT /api/shipping/zones/{zoneId}/rates/{rateId}` - Update rate
- `DELETE /api/shipping/zones/{zoneId}/rates/{rateId}` - Delete rate
- `PUT /api/shipping/zones/bulk-update` - Bulk update all zones and rates
- `POST /api/shipping/zones/{id}/duplicate` - Duplicate a zone
- `PUT /api/shipping/zones/reorder` - Reorder zones

#### Service Implementation
```csharp
public class ShippingService : IShippingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ShippingService> _logger;

    // Key feature: Auto-create default rates when creating zone
    public async Task<ShippingZoneDto> CreateZoneAsync(CreateShippingZoneDto dto)
    {
        var zone = new ShippingZone
        {
            // ... zone properties
            Rates = new List<ShippingRate>
            {
                new ShippingRate { RateType = "weight", Price = 0, IsActive = true },
                new ShippingRate { RateType = "vat", Price = 0, IsActive = true },
                new ShippingRate { RateType = "duty", Price = 0, IsActive = true }
            }
        };
        // ... save logic
    }
}
```

#### Database Configuration
```csharp
// Data/ApplicationDbContext.cs
modelBuilder.Entity<ShippingZone>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    entity.Property(e => e.ZoneType).IsRequired().HasMaxLength(20);
    entity.Property(e => e.Countries).HasColumnType("jsonb"); // JSONB for flexibility
    entity.HasIndex(e => e.CompanyId);
    entity.HasIndex(e => e.IsActive);
    entity.HasIndex(e => e.DisplayOrder);
});
```

### Frontend

#### Components Created
- `ShippingConfiguration.tsx` - Main shipping management component
- `ShippingZoneCard.tsx` - Individual zone display card
- `CreateZoneModal.tsx` - Modal for creating new zones
- `ConfirmDialog.tsx` - Reusable confirmation dialog

#### State Management
- Local state with useState hooks
- Optimistic updates before API calls
- Separate tracking of original vs modified data for change detection

#### API Integration
```typescript
// Create zone locally first, then sync
const handleCreateZone = (newZone: Omit<ShippingZone, 'id'>) => {
    const zone: ShippingZone = {
        ...newZone,
        id: `temp-${Date.now()}`, // Temporary ID
        displayOrder: shippingZones.length + 1,
        rates: [/* default rates */]
    };
    setShippingZones([...shippingZones, zone]);
    setHasChanges(true);
};

// Bulk save all changes
const handleSave = async () => {
    // Separate new zones from existing
    const newZones = shippingZones.filter(z => z.id.startsWith('temp-'));
    
    // Create new zones first
    for (const zone of newZones) {
        const response = await api.post('/shipping/zones', createDto);
        zone.id = response.data.id; // Update with real ID
    }
    
    // Then bulk update all
    await api.put('/shipping/zones/bulk-update', { zones: zonesForBackend });
};
```

## Configuration

### Npgsql Dynamic JSON (Critical Configuration)
```csharp
// Program.cs - Enable dynamic JSON serialization
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson(); // Required for List<string> serialization
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));
```

### Database Migration
```bash
# Migration created
Add-Migration AddShippingZonesAndRates -Context ApplicationDbContext

# Applied to database
Update-Database -Context ApplicationDbContext
```

## Testing

### Manual Testing Checklist
- [x] Create new shipping zone
- [x] Add/edit/delete rates
- [x] Bulk save changes
- [x] Zone type validation (domestic/international/custom)
- [x] Rate type validation (weight/vat/duty)
- [x] Countries list persistence as JSONB

### Key Test Scenarios
1. Create zone with default rates
2. Edit multiple zones and bulk save
3. Delete zone and verify cascade delete of rates
4. Verify company isolation (single-tenant)

## Known Issues & Limitations

### Current Limitations
- No import/export functionality
- Countries are simple string list (no validation against country codes)
- No shipping cost calculation logic (just rate storage)
- No zone overlap detection

### Future Improvements
- Add postal code ranges support
- Implement shipping calculator service
- Add zone templates for common configurations
- Support for multiple currencies per zone

## Performance Considerations
- Bulk update reduces API calls from N to 1
- JSONB indexing could be added for country searches
- Consider caching zones for frequently accessed stores

## Troubleshooting
- **JSON Serialization Error**: See [Npgsql JSON Serialization Issues](/docs/troubleshooting/database/db-01-npgsql-json-serialization.md)
- **Bulk Update 400 Error**: See [DTO Validation Issues](/docs/troubleshooting/api/api-01-dto-validation.md)

## Debug Tips
- Check console logs for detailed error messages
- Verify Npgsql EnableDynamicJson() is configured
- Ensure migration was applied successfully
- Check for duplicate DTO definitions

## References
- [Npgsql JSON Documentation](https://www.npgsql.org/doc/types/json.html)
- [EF Core PostgreSQL Provider](https://www.npgsql.org/efcore/)
- Project Blueprint: `/blueprint2.md`
- CLAUDE.md Rules: `/CLAUDE.md`