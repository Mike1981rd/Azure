using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Customers;
using WebsiteBuilderAPI.Models;
using BCrypt.Net;

namespace WebsiteBuilderAPI.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CustomerService> _logger;
        private readonly Random _random = new Random();

        public CustomerService(ApplicationDbContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Main CRUD operations
        public async Task<PagedResult<CustomerDto>> GetCustomersAsync(int companyId, CustomerFilterDto filter)
        {
            try
            {
                var query = _context.Customers
                    .Include(c => c.WishlistItems)
                    .Include(c => c.Coupons)
                    .Where(c => c.CompanyId == companyId && c.DeletedAt == null);

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var searchLower = filter.Search.ToLower();
                    query = query.Where(c => 
                        c.FirstName.ToLower().Contains(searchLower) ||
                        c.LastName.ToLower().Contains(searchLower) ||
                        c.Email.ToLower().Contains(searchLower) ||
                        c.Username.ToLower().Contains(searchLower) ||
                        c.CustomerId.ToLower().Contains(searchLower));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(c => c.Status == filter.Status);
                }

                if (!string.IsNullOrEmpty(filter.Country))
                {
                    query = query.Where(c => c.Country == filter.Country);
                }

                if (!string.IsNullOrEmpty(filter.LoyaltyTier))
                {
                    query = query.Where(c => c.LoyaltyTier == filter.LoyaltyTier);
                }

                if (filter.MinSpent.HasValue)
                {
                    query = query.Where(c => c.TotalSpent >= filter.MinSpent.Value);
                }

                if (filter.MaxSpent.HasValue)
                {
                    query = query.Where(c => c.TotalSpent <= filter.MaxSpent.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDescending 
                        ? query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName)
                        : query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
                    "email" => filter.SortDescending 
                        ? query.OrderByDescending(c => c.Email)
                        : query.OrderBy(c => c.Email),
                    "totalspent" => filter.SortDescending 
                        ? query.OrderByDescending(c => c.TotalSpent)
                        : query.OrderBy(c => c.TotalSpent),
                    "loyaltypoints" => filter.SortDescending 
                        ? query.OrderByDescending(c => c.LoyaltyPoints)
                        : query.OrderBy(c => c.LoyaltyPoints),
                    _ => filter.SortDescending 
                        ? query.OrderByDescending(c => c.CreatedAt)
                        : query.OrderBy(c => c.CreatedAt)
                };

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var customers = await query
                    .Skip(filter.Skip)
                    .Take(filter.Take)
                    .Select(c => new CustomerDto
                    {
                        Id = c.Id,
                        CustomerId = c.CustomerId,
                        FullName = c.FirstName + " " + c.LastName,
                        Username = c.Username,
                        Email = c.Email,
                        Avatar = c.Avatar,
                        Phone = c.Phone,
                        Country = c.Country,
                        Status = c.Status,
                        AccountBalance = c.AccountBalance,
                        TotalSpent = c.TotalSpent,
                        TotalOrders = c.TotalOrders,
                        LoyaltyPoints = c.LoyaltyPoints,
                        LoyaltyTier = c.LoyaltyTier,
                        TwoFactorEnabled = c.TwoFactorEnabled,
                        WishlistCount = c.WishlistItems.Count,
                        CouponsCount = c.Coupons.Count(cp => cp.IsActive && DateTime.UtcNow <= cp.ValidUntil),
                        CreatedAt = c.CreatedAt,
                        LastLoginAt = c.LastLoginAt
                    })
                    .ToListAsync();

                return new PagedResult<CustomerDto>
                {
                    Items = customers,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.Size,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.Size)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<CustomerDetailDto> GetCustomerByIdAsync(int companyId, int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Addresses)
                    .Include(c => c.PaymentMethods)
                    .Include(c => c.NotificationPreference)
                    .Include(c => c.Devices)
                    .Include(c => c.WishlistItems)
                        .ThenInclude(w => w.Product)
                    .Include(c => c.Coupons)
                    .Include(c => c.SecurityQuestions)
                    .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && c.DeletedAt == null);

                if (customer == null)
                    return null;

                return new CustomerDetailDto
                {
                    Id = customer.Id,
                    CustomerId = customer.CustomerId,
                    FullName = customer.FirstName + " " + customer.LastName,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Username = customer.Username,
                    Email = customer.Email,
                    Avatar = customer.Avatar,
                    Phone = customer.Phone,
                    Country = customer.Country,
                    Status = customer.Status,
                    AccountBalance = customer.AccountBalance,
                    TotalSpent = customer.TotalSpent,
                    TotalOrders = customer.TotalOrders,
                    LoyaltyPoints = customer.LoyaltyPoints,
                    LoyaltyTier = customer.LoyaltyTier,
                    TwoFactorEnabled = customer.TwoFactorEnabled,
                    TwoFactorPhone = customer.TwoFactorPhone,
                    RecoveryEmail = customer.RecoveryEmail,
                    SessionTimeoutMinutes = customer.SessionTimeoutMinutes,
                    BirthDate = customer.BirthDate?.ToString("yyyy-MM-dd"),
                    Gender = customer.Gender,
                    PreferredLanguage = customer.PreferredLanguage,
                    PreferredCurrency = customer.PreferredCurrency,
                    CompanyName = customer.CompanyName,
                    TaxId = customer.TaxId,
                    WishlistCount = customer.WishlistItems?.Count ?? 0,
                    CouponsCount = customer.Coupons?.Count(cp => cp.IsActive && DateTime.UtcNow <= cp.ValidUntil) ?? 0,
                    CreatedAt = customer.CreatedAt,
                    LastLoginAt = customer.LastLoginAt,
                    Addresses = customer.Addresses.Select(a => new CustomerAddressDto
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Label = a.Label,
                        Street = a.Street,
                        Apartment = a.Apartment,
                        City = a.City,
                        State = a.State,
                        Country = a.Country,
                        PostalCode = a.PostalCode,
                        IsDefault = a.IsDefault,
                        CreatedAt = a.CreatedAt
                    }).ToList(),
                    PaymentMethods = customer.PaymentMethods.Select(p => new CustomerPaymentMethodDto
                    {
                        Id = p.Id,
                        CardType = p.CardType,
                        CardholderName = p.CardholderName,
                        Last4Digits = p.Last4Digits,
                        ExpiryMonth = p.ExpiryMonth,
                        ExpiryYear = p.ExpiryYear,
                        BillingAddress = p.BillingAddress,
                        IsPrimary = p.IsPrimary,
                        CreatedAt = p.CreatedAt
                    }).ToList(),
                    NotificationPreferences = customer.NotificationPreference != null ? 
                        new CustomerNotificationPreferenceDto
                        {
                            Id = customer.NotificationPreference.Id,
                            CustomerId = customer.NotificationPreference.CustomerId,
                            EmailOrderUpdates = customer.NotificationPreference.EmailOrderUpdates,
                            EmailPromotions = customer.NotificationPreference.EmailPromotions,
                            EmailNewsletter = customer.NotificationPreference.EmailNewsletter,
                            EmailProductReviews = customer.NotificationPreference.EmailProductReviews,
                            EmailPriceAlerts = customer.NotificationPreference.EmailPriceAlerts,
                            SmsOrderUpdates = customer.NotificationPreference.SmsOrderUpdates,
                            SmsDeliveryAlerts = customer.NotificationPreference.SmsDeliveryAlerts,
                            SmsPromotions = customer.NotificationPreference.SmsPromotions,
                            PushEnabled = customer.NotificationPreference.PushEnabled,
                            PushSound = customer.NotificationPreference.PushSound,
                            PushVibration = customer.NotificationPreference.PushVibration,
                            DoNotDisturbStart = customer.NotificationPreference.DoNotDisturbStart,
                            DoNotDisturbEnd = customer.NotificationPreference.DoNotDisturbEnd,
                            Timezone = customer.NotificationPreference.Timezone,
                            CreatedAt = customer.NotificationPreference.CreatedAt,
                            UpdatedAt = customer.NotificationPreference.UpdatedAt
                        } : new CustomerNotificationPreferenceDto(),
                    RecentDevices = customer.Devices
                        .OrderByDescending(d => d.LastActivity)
                        .Take(10)
                        .Select(d => new CustomerDeviceDto
                        {
                            Id = d.Id,
                            Browser = d.Browser,
                            DeviceType = d.DeviceType,
                            DeviceName = d.DeviceName,
                            OperatingSystem = d.OperatingSystem,
                            IpAddress = d.IpAddress,
                            Location = d.Location,
                            LastActivity = d.LastActivity,
                            FirstSeen = d.FirstSeen,
                            IsTrusted = d.IsTrusted,
                            IsActive = d.IsActive
                        }).ToList(),
                    WishlistItems = customer.WishlistItems.Select(w => new CustomerWishlistItemDto
                    {
                        Id = w.Id,
                        ProductId = w.ProductId,
                        ProductVariantId = w.ProductVariantId,
                        ProductName = w.Product?.Name ?? "Unknown Product",
                        ProductPrice = w.Product?.BasePrice ?? 0,
                        InStock = w.Product?.Stock > 0,
                        AddedAt = w.AddedAt,
                        NotifyOnPriceChange = w.NotifyOnPriceChange,
                        NotifyOnBackInStock = w.NotifyOnBackInStock
                    }).ToList(),
                    Coupons = customer.Coupons.Select(c => new CustomerCouponDto
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Description = c.Description,
                        DiscountAmount = c.DiscountAmount,
                        DiscountType = c.DiscountType,
                        MinimumPurchase = c.MinimumPurchase,
                        ValidFrom = c.ValidFrom,
                        ValidUntil = c.ValidUntil,
                        IsActive = c.IsActive,
                        UsageCount = c.UsageCount,
                        MaxUsageCount = c.MaxUsageCount
                    }).ToList(),
                    SecurityQuestions = customer.SecurityQuestions.Select(sq => new CustomerSecurityQuestionDto
                    {
                        Id = sq.Id,
                        CustomerId = sq.CustomerId,
                        Question = sq.Question
                        // Never return AnswerHash
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer {Id} for company {CompanyId}", id, companyId);
                throw;
            }
        }

        public async Task<CustomerDto> CreateCustomerAsync(int companyId, CreateCustomerDto dto)
        {
            try
            {
                // Check if email or username already exists
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId && 
                        (c.Email == dto.Email || c.Username == dto.Username));

                if (existingCustomer != null)
                {
                    throw new InvalidOperationException(
                        existingCustomer.Email == dto.Email 
                            ? "A customer with this email already exists" 
                            : "A customer with this username already exists");
                }

                var customer = new Customer
                {
                    CompanyId = companyId,
                    CustomerId = await GenerateCustomerIdAsync(),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Phone = dto.Phone,
                    Country = dto.Country,
                    Status = dto.Status,
                    AccountBalance = dto.InitialBalance ?? 0,
                    LoyaltyPoints = dto.InitialLoyaltyPoints ?? 0,
                    BirthDate = dto.BirthDate?.ToUniversalTime(),
                    Gender = dto.Gender,
                    PreferredLanguage = dto.PreferredLanguage,
                    PreferredCurrency = dto.PreferredCurrency,
                    CompanyName = dto.CompanyName,
                    TaxId = dto.TaxId,
                    LoyaltyTier = dto.LoyaltyTier ?? "Bronze",
                    Avatar = dto.Avatar,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Update loyalty tier based on initial points
                customer.UpdateLoyaltyTier();

                // Add default notification preference (single record per customer)
                var defaultNotificationPreference = new CustomerNotificationPreference
                {
                    Customer = customer,
                    // Email defaults
                    EmailOrderUpdates = true,
                    EmailPromotions = false,
                    EmailNewsletter = false,
                    EmailProductReviews = true,
                    EmailPriceAlerts = false,
                    // SMS defaults
                    SmsOrderUpdates = false,
                    SmsDeliveryAlerts = false,
                    SmsPromotions = false,
                    // Push defaults
                    PushEnabled = false,
                    PushSound = true,
                    PushVibration = true,
                    // Schedule defaults
                    DoNotDisturbStart = "22:00",
                    DoNotDisturbEnd = "08:00",
                    Timezone = "America/Santo_Domingo"
                };

                _context.CustomerNotificationPreferences.Add(defaultNotificationPreference);

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new customer {CustomerId} for company {CompanyId}", 
                    customer.CustomerId, companyId);

                return new CustomerDto
                {
                    Id = customer.Id,
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Username = customer.Username,
                    Email = customer.Email,
                    Avatar = customer.Avatar,
                    Phone = customer.Phone,
                    Country = customer.Country,
                    Status = customer.Status,
                    AccountBalance = customer.AccountBalance,
                    TotalSpent = customer.TotalSpent,
                    TotalOrders = customer.TotalOrders,
                    LoyaltyPoints = customer.LoyaltyPoints,
                    LoyaltyTier = customer.LoyaltyTier,
                    TwoFactorEnabled = customer.TwoFactorEnabled,
                    WishlistCount = 0,
                    CouponsCount = 0,
                    CreatedAt = customer.CreatedAt,
                    LastLoginAt = customer.LastLoginAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer for company {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int companyId, int id, UpdateCustomerDto dto)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.WishlistItems)
                    .Include(c => c.Coupons)
                    .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && c.DeletedAt == null);

                if (customer == null)
                    return null;

                // Update only provided fields (partial update per Guardado.md)
                // FirstName and LastName are required fields, never set to null
                if (!string.IsNullOrEmpty(dto.FirstName))
                    customer.FirstName = dto.FirstName;

                if (!string.IsNullOrEmpty(dto.LastName))
                    customer.LastName = dto.LastName;

                if (!string.IsNullOrEmpty(dto.Username))
                {
                    // Check if username is unique
                    var existingUsername = await _context.Customers
                        .AnyAsync(c => c.CompanyId == companyId && c.Username == dto.Username && c.Id != id);
                    
                    if (!existingUsername)
                        customer.Username = dto.Username;
                    else
                        throw new InvalidOperationException("Username already exists");
                }

                if (!string.IsNullOrEmpty(dto.Email))
                {
                    // Check if email is unique
                    var existingEmail = await _context.Customers
                        .AnyAsync(c => c.CompanyId == companyId && c.Email == dto.Email && c.Id != id);
                    
                    if (!existingEmail)
                        customer.Email = dto.Email;
                    else
                        throw new InvalidOperationException("Email already exists");
                }

                if (dto.Phone != null && dto.Phone != "")
                    customer.Phone = dto.Phone;
                else if (dto.Phone == "")
                    customer.Phone = null;

                if (!string.IsNullOrEmpty(dto.Country))
                    customer.Country = dto.Country;

                if (!string.IsNullOrEmpty(dto.Status))
                    customer.Status = dto.Status;

                if (!string.IsNullOrEmpty(dto.Avatar))
                    customer.Avatar = dto.Avatar;

                if (dto.AccountBalance.HasValue)
                    customer.AccountBalance = dto.AccountBalance.Value;

                if (dto.LoyaltyPoints.HasValue)
                {
                    customer.LoyaltyPoints = dto.LoyaltyPoints.Value;
                    customer.UpdateLoyaltyTier();
                }

                if (dto.TwoFactorEnabled.HasValue)
                    customer.TwoFactorEnabled = dto.TwoFactorEnabled.Value;

                if (!string.IsNullOrEmpty(dto.TwoFactorPhone))
                    customer.TwoFactorPhone = dto.TwoFactorPhone;

                // Handle additional fields
                if (dto.BirthDate.HasValue)
                    customer.BirthDate = dto.BirthDate.Value.ToUniversalTime();
                    
                if (!string.IsNullOrEmpty(dto.Gender))
                    customer.Gender = dto.Gender;
                    
                if (!string.IsNullOrEmpty(dto.PreferredLanguage))
                    customer.PreferredLanguage = dto.PreferredLanguage;
                    
                if (!string.IsNullOrEmpty(dto.PreferredCurrency))
                    customer.PreferredCurrency = dto.PreferredCurrency;
                    
                if (!string.IsNullOrEmpty(dto.CompanyName))
                    customer.CompanyName = dto.CompanyName;
                    
                if (!string.IsNullOrEmpty(dto.TaxId))
                    customer.TaxId = dto.TaxId;
                    
                if (!string.IsNullOrEmpty(dto.LoyaltyTier))
                    customer.LoyaltyTier = dto.LoyaltyTier;

                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated customer {CustomerId} for company {CompanyId}", 
                    customer.CustomerId, companyId);

                return new CustomerDto
                {
                    Id = customer.Id,
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Username = customer.Username,
                    Email = customer.Email,
                    Avatar = customer.Avatar,
                    Phone = customer.Phone,
                    Country = customer.Country,
                    Status = customer.Status,
                    AccountBalance = customer.AccountBalance,
                    TotalSpent = customer.TotalSpent,
                    TotalOrders = customer.TotalOrders,
                    LoyaltyPoints = customer.LoyaltyPoints,
                    LoyaltyTier = customer.LoyaltyTier,
                    TwoFactorEnabled = customer.TwoFactorEnabled,
                    WishlistCount = customer.WishlistItems.Count,
                    CouponsCount = customer.Coupons.Count(c => c.IsActive && DateTime.UtcNow <= c.ValidUntil),
                    CreatedAt = customer.CreatedAt,
                    LastLoginAt = customer.LastLoginAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {Id} for company {CompanyId}", id, companyId);
                throw;
            }
        }

        public async Task DeleteCustomerAsync(int companyId, int id)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && c.DeletedAt == null);

                if (customer == null)
                    return;

                // Soft delete
                customer.DeletedAt = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted customer {CustomerId} for company {CompanyId}", 
                    customer.CustomerId, companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {Id} for company {CompanyId}", id, companyId);
                throw;
            }
        }

        public async Task<CustomerDto> UpdateAvatarAsync(int companyId, int id, string avatarUrl)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && c.DeletedAt == null);

                if (customer == null)
                    return null;

                customer.Avatar = avatarUrl;
                customer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated avatar for customer {CustomerId}", customer.CustomerId);

                return new CustomerDto
                {
                    Id = customer.Id,
                    CustomerId = customer.CustomerId,
                    FullName = customer.FullName,
                    Username = customer.Username,
                    Email = customer.Email,
                    Avatar = customer.Avatar,
                    Phone = customer.Phone,
                    Country = customer.Country,
                    Status = customer.Status,
                    CreatedAt = customer.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating avatar for customer {Id}", id);
                throw;
            }
        }

        // Authentication methods

        public async Task<bool> EnableTwoFactorAsync(int companyId, int id, string phoneNumber)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && c.DeletedAt == null);

                if (customer == null)
                    return false;

                customer.TwoFactorEnabled = true;
                customer.TwoFactorPhone = phoneNumber;
                customer.TwoFactorSecret = Guid.NewGuid().ToString(); // In production, use proper 2FA library
                customer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Enabled 2FA for customer {CustomerId}", customer.CustomerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA for customer {Id}", id);
                throw;
            }
        }

        public async Task<bool> DisableTwoFactorAsync(int companyId, int id)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId && c.DeletedAt == null);

                if (customer == null)
                    return false;

                customer.TwoFactorEnabled = false;
                customer.TwoFactorSecret = null;
                customer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Disabled 2FA for customer {CustomerId}", customer.CustomerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA for customer {Id}", id);
                throw;
            }
        }

        // Helper method to generate unique customer ID
        public async Task<string> GenerateCustomerIdAsync()
        {
            string customerId;
            bool exists;
            
            do
            {
                customerId = "#" + _random.Next(100000, 999999).ToString();
                exists = await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
            } while (exists);

            return customerId;
        }

        // Address management methods
        public async Task<List<CustomerAddressDto>> GetAddressesAsync(int customerId)
        {
            try
            {
                var addresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .Select(a => new CustomerAddressDto
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Label = a.Label,
                        Street = a.Street,
                        Apartment = a.Apartment,
                        City = a.City,
                        State = a.State,
                        Country = a.Country,
                        PostalCode = a.PostalCode,
                        IsDefault = a.IsDefault,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                return addresses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddressDto> AddAddressAsync(int customerId, AddAddressDto dto)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                    throw new InvalidOperationException("Customer not found");

                var address = new CustomerAddress
                {
                    CustomerId = customerId,
                    Type = dto.Type ?? "Home",
                    Label = dto.Label ?? dto.Type ?? "Address",
                    Street = dto.Street,
                    Apartment = dto.Apartment,
                    City = dto.City,
                    State = dto.State,
                    Country = dto.Country,
                    PostalCode = dto.PostalCode,
                    IsDefault = dto.IsDefault,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // If this is the default address, unset other defaults
                if (address.IsDefault)
                {
                    var existingDefaults = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == customerId && a.IsDefault)
                        .ToListAsync();
                    
                    foreach (var existing in existingDefaults)
                    {
                        existing.IsDefault = false;
                    }
                }

                _context.CustomerAddresses.Add(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added address for customer {CustomerId}", customerId);

                return new CustomerAddressDto
                {
                    Id = address.Id,
                    Type = address.Type,
                    Label = address.Label,
                    Street = address.Street,
                    Apartment = address.Apartment,
                    City = address.City,
                    State = address.State,
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault,
                    CreatedAt = address.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerAddressDto> UpdateAddressAsync(int customerId, int addressId, AddAddressDto dto)
        {
            try
            {
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

                if (address == null)
                    throw new InvalidOperationException("Address not found");

                // Update fields
                if (!string.IsNullOrEmpty(dto.Type))
                    address.Type = dto.Type;
                if (!string.IsNullOrEmpty(dto.Label))
                    address.Label = dto.Label;
                if (!string.IsNullOrEmpty(dto.Street))
                    address.Street = dto.Street;
                
                address.Apartment = dto.Apartment; // Can be null
                
                if (!string.IsNullOrEmpty(dto.City))
                    address.City = dto.City;
                    
                address.State = dto.State; // Can be null
                
                if (!string.IsNullOrEmpty(dto.Country))
                    address.Country = dto.Country;
                    
                address.PostalCode = dto.PostalCode; // Can be null

                // Handle default flag - IsDefault is not nullable in DTO
                address.IsDefault = dto.IsDefault;
                
                // If setting as default, unset other defaults
                if (address.IsDefault)
                {
                    var otherAddresses = await _context.CustomerAddresses
                        .Where(a => a.CustomerId == customerId && a.Id != addressId && a.IsDefault)
                        .ToListAsync();
                    
                    foreach (var other in otherAddresses)
                    {
                        other.IsDefault = false;
                    }
                }

                address.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated address {AddressId} for customer {CustomerId}", addressId, customerId);

                return new CustomerAddressDto
                {
                    Id = address.Id,
                    Type = address.Type,
                    Label = address.Label,
                    Street = address.Street,
                    Apartment = address.Apartment,
                    City = address.City,
                    State = address.State,
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault,
                    CreatedAt = address.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for customer {CustomerId}", addressId, customerId);
                throw;
            }
        }

        public async Task DeleteAddressAsync(int customerId, int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

                if (address == null)
                    return;

                _context.CustomerAddresses.Remove(address);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted address {AddressId} for customer {CustomerId}", addressId, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for customer {CustomerId}", addressId, customerId);
                throw;
            }
        }

        public async Task<bool> SetDefaultAddressAsync(int customerId, int addressId)
        {
            try
            {
                var address = await _context.CustomerAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId);

                if (address == null)
                    return false;

                // Unset all other defaults
                var otherAddresses = await _context.CustomerAddresses
                    .Where(a => a.CustomerId == customerId && a.Id != addressId && a.IsDefault)
                    .ToListAsync();
                
                foreach (var other in otherAddresses)
                {
                    other.IsDefault = false;
                }

                address.IsDefault = true;
                address.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Set default address {AddressId} for customer {CustomerId}", addressId, customerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId} for customer {CustomerId}", addressId, customerId);
                throw;
            }
        }

        public Task<List<CustomerPaymentMethodDto>> GetPaymentMethodsAsync(int customerId) => 
            throw new NotImplementedException("Implement payment method retrieval");

        public Task<CustomerPaymentMethodDto> AddPaymentMethodAsync(int customerId, AddPaymentMethodDto dto) => 
            throw new NotImplementedException("Implement payment method addition");

        public Task DeletePaymentMethodAsync(int customerId, int methodId) => 
            throw new NotImplementedException("Implement payment method deletion");

        public Task<bool> SetPrimaryPaymentMethodAsync(int customerId, int methodId) => 
            throw new NotImplementedException("Implement primary payment method setting");

        public Task<List<CustomerNotificationPreferenceDto>> GetNotificationPreferencesAsync(int customerId) => 
            throw new NotImplementedException("Implement notification preference retrieval");

        public async Task UpdateNotificationPreferencesAsync(int customerId, UpdateNotificationPreferencesDto dto)
        {
            try
            {
                // Get existing preference or create new one
                var preference = await _context.CustomerNotificationPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (preference == null)
                {
                    // Create new preference if doesn't exist
                    preference = new CustomerNotificationPreference
                    {
                        CustomerId = customerId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.CustomerNotificationPreferences.Add(preference);
                }
                
                // Update Email Notifications
                if (dto.EmailNotifications != null)
                {
                    preference.EmailOrderUpdates = dto.EmailNotifications.OrderUpdates;
                    preference.EmailPromotions = dto.EmailNotifications.Promotions;
                    preference.EmailNewsletter = dto.EmailNotifications.Newsletter;
                    preference.EmailProductReviews = dto.EmailNotifications.ProductReviews;
                    preference.EmailPriceAlerts = dto.EmailNotifications.PriceAlerts;
                }
                
                // Update SMS Notifications
                if (dto.SmsNotifications != null)
                {
                    preference.SmsOrderUpdates = dto.SmsNotifications.OrderUpdates;
                    preference.SmsDeliveryAlerts = dto.SmsNotifications.DeliveryAlerts;
                    preference.SmsPromotions = dto.SmsNotifications.Promotions;
                }
                
                // Update Push Notifications
                if (dto.PushNotifications != null)
                {
                    preference.PushEnabled = dto.PushNotifications.Enabled;
                    preference.PushSound = dto.PushNotifications.Sound;
                    preference.PushVibration = dto.PushNotifications.Vibration;
                }
                
                // Update Notification Schedule
                if (dto.NotificationSchedule != null)
                {
                    preference.DoNotDisturbStart = dto.NotificationSchedule.DoNotDisturbStart;
                    preference.DoNotDisturbEnd = dto.NotificationSchedule.DoNotDisturbEnd;
                    preference.Timezone = dto.NotificationSchedule.Timezone;
                }
                
                preference.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated notification preferences for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public Task<List<CustomerDeviceDto>> GetDevicesAsync(int customerId) => 
            throw new NotImplementedException("Implement device retrieval");

        public Task DeleteDeviceAsync(int customerId, int deviceId) => 
            throw new NotImplementedException("Implement device deletion");

        public Task<bool> TrustDeviceAsync(int customerId, int deviceId) => 
            throw new NotImplementedException("Implement device trust");

        public Task<List<CustomerWishlistItemDto>> GetWishlistAsync(int customerId) => 
            throw new NotImplementedException("Implement wishlist retrieval");

        public Task<CustomerWishlistItemDto> AddToWishlistAsync(int customerId, int productId, int? variantId = null) => 
            throw new NotImplementedException("Implement wishlist addition");

        public Task RemoveFromWishlistAsync(int customerId, int wishlistItemId) => 
            throw new NotImplementedException("Implement wishlist removal");

        public Task<List<CustomerCouponDto>> GetCouponsAsync(int customerId) => 
            throw new NotImplementedException("Implement coupon retrieval");

        public Task<CustomerCouponDto> AssignCouponAsync(int customerId, string couponCode) => 
            throw new NotImplementedException("Implement coupon assignment");

        public Task<bool> UseCouponAsync(int customerId, int couponId) => 
            throw new NotImplementedException("Implement coupon usage");

        public Task UpdateCustomerMetricsAsync(int customerId) => 
            throw new NotImplementedException("Implement metrics update");

        public async Task UpdateSecuritySettingsAsync(int customerId, UpdateSecuritySettingsDto dto)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.SecurityQuestions)
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                
                if (customer == null)
                    throw new Exception($"Customer {customerId} not found");
                
                // Update Two-Factor settings
                if (dto.TwoFactorEnabled.HasValue)
                    customer.TwoFactorEnabled = dto.TwoFactorEnabled.Value;
                
                if (!string.IsNullOrEmpty(dto.TwoFactorPhone))
                    customer.TwoFactorPhone = dto.TwoFactorPhone;
                
                // Update recovery email
                if (!string.IsNullOrEmpty(dto.RecoveryEmail))
                    customer.RecoveryEmail = dto.RecoveryEmail;
                
                // Update session timeout
                if (dto.SessionTimeoutMinutes.HasValue)
                    customer.SessionTimeoutMinutes = dto.SessionTimeoutMinutes.Value;
                
                // Handle security questions
                if (dto.SecurityQuestions != null)
                {
                    foreach (var questionDto in dto.SecurityQuestions)
                    {
                        if (questionDto.Delete == true && questionDto.Id.HasValue)
                        {
                            // Delete existing question
                            var existing = customer.SecurityQuestions.FirstOrDefault(q => q.Id == questionDto.Id.Value);
                            if (existing != null)
                                _context.CustomerSecurityQuestions.Remove(existing);
                        }
                        else if (questionDto.Id.HasValue)
                        {
                            // Update existing question
                            var existing = customer.SecurityQuestions.FirstOrDefault(q => q.Id == questionDto.Id.Value);
                            if (existing != null)
                            {
                                existing.Question = questionDto.Question;
                                if (!string.IsNullOrEmpty(questionDto.Answer))
                                    existing.AnswerHash = BCrypt.Net.BCrypt.HashPassword(questionDto.Answer);
                                existing.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                        else if (!string.IsNullOrEmpty(questionDto.Question) && !string.IsNullOrEmpty(questionDto.Answer))
                        {
                            // Add new question
                            var newQuestion = new CustomerSecurityQuestion
                            {
                                CustomerId = customerId,
                                Question = questionDto.Question,
                                AnswerHash = BCrypt.Net.BCrypt.HashPassword(questionDto.Answer),
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            customer.SecurityQuestions.Add(newQuestion);
                        }
                    }
                }
                
                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated security settings for customer {CustomerId}", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security settings for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int customerId, CustomerPasswordChangeDto dto)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                
                if (customer == null)
                    return false;
                
                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, customer.PasswordHash))
                {
                    _logger.LogWarning("Invalid current password for customer {CustomerId}", customerId);
                    return false;
                }
                
                // Update password
                customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                customer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Password changed for customer {CustomerId}", customerId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<List<CustomerSecurityQuestionDto>> GetSecurityQuestionsAsync(int customerId)
        {
            try
            {
                var questions = await _context.CustomerSecurityQuestions
                    .Where(q => q.CustomerId == customerId)
                    .Select(q => new CustomerSecurityQuestionDto
                    {
                        Id = q.Id,
                        CustomerId = q.CustomerId,
                        Question = q.Question
                        // Never return AnswerHash
                    })
                    .ToListAsync();
                
                return questions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security questions for customer {CustomerId}", customerId);
                throw;
            }
        }
        
        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(int customerId, ResetPasswordDto dto)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                
                if (customer == null)
                {
                    return new ResetPasswordResponseDto
                    {
                        Success = false,
                        Message = "Customer not found"
                    };
                }
                
                // Generate a temporary password if not provided
                string temporaryPassword = dto.TemporaryPassword;
                if (string.IsNullOrEmpty(temporaryPassword))
                {
                    // Generate a secure random password
                    temporaryPassword = GenerateSecurePassword();
                }
                
                // Hash and save the new password
                customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);
                customer.ForcePasswordChange = dto.ForcePasswordChange;
                customer.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                // TODO: Send email notification if requested
                bool emailSent = false;
                if (dto.SendEmail && !string.IsNullOrEmpty(customer.Email))
                {
                    // Email sending logic would go here
                    // For now, we'll just log it
                    _logger.LogInformation("Password reset email should be sent to {Email} for customer {CustomerId}", 
                        customer.Email, customerId);
                    emailSent = false; // Set to true when email service is implemented
                }
                
                _logger.LogInformation("Password reset for customer {CustomerId} by admin", customerId);
                
                return new ResetPasswordResponseDto
                {
                    Success = true,
                    TemporaryPassword = temporaryPassword,
                    Message = "Password has been reset successfully",
                    EmailSent = emailSent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for customer {CustomerId}", customerId);
                throw;
            }
        }
        
        private string GenerateSecurePassword()
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string special = "!@#$%^&*";
            const string allChars = uppercase + lowercase + numbers + special;
            
            var random = new Random();
            var password = new char[12];
            
            // Ensure at least one of each type
            password[0] = uppercase[random.Next(uppercase.Length)];
            password[1] = lowercase[random.Next(lowercase.Length)];
            password[2] = numbers[random.Next(numbers.Length)];
            password[3] = special[random.Next(special.Length)];
            
            // Fill the rest randomly
            for (int i = 4; i < password.Length; i++)
            {
                password[i] = allChars[random.Next(allChars.Length)];
            }
            
            // Shuffle the password
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
    }
}