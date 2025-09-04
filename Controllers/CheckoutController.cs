using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Checkout;
using WebsiteBuilderAPI.DTOs.Customers;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.Models;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomerService _customerService;
        private readonly IReservationService _reservationService;
        private readonly IEmailService _emailService;
        private readonly IAvailabilityService _availabilityService;
        private readonly ICompanyService _companyService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ApplicationDbContext context,
            ICustomerService customerService,
            IReservationService reservationService,
            IEmailService emailService,
            IAvailabilityService availabilityService,
            ICompanyService companyService,
            ILogger<CheckoutController> logger)
        {
            _context = context;
            _customerService = customerService;
            _reservationService = reservationService;
            _emailService = emailService;
            _availabilityService = availabilityService;
            _companyService = companyService;
            _logger = logger;
        }

        /// <summary>
        /// Get checkout configuration settings
        /// </summary>
        [HttpGet("checkout-settings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCheckoutSettings()
        {
            try
            {
                var settings = await _companyService.GetCheckoutSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting checkout settings");
                // Return default settings if there's an error
                return Ok(new 
                {
                    phoneNumberField = "optional",
                    companyNameField = "optional",
                    apartmentSuiteField = "optional",
                    allowGuestCheckout = true,
                    showOrderComments = true,
                    showCompanyField = false
                });
            }
        }

        /// <summary>
        /// Process a room reservation from checkout page
        /// Creates/updates customer and creates reservation
        /// </summary>
        [HttpPost("process-room-reservation")]
        [AllowAnonymous]
        public async Task<ActionResult<ProcessRoomReservationResponseDto>> ProcessRoomReservation(
            [FromBody] ProcessRoomReservationDto dto)
        {
            // Log incoming data for debugging
            _logger.LogInformation("=== CHECKOUT REQUEST RECEIVED ===");
            _logger.LogInformation("Email: {Email}", dto?.Email ?? "NULL");
            _logger.LogInformation("FirstName: {FirstName}", dto?.FirstName ?? "NULL");
            _logger.LogInformation("LastName: {LastName}", dto?.LastName ?? "NULL");
            _logger.LogInformation("Country: {Country}", dto?.Country ?? "NULL");
            _logger.LogInformation("RoomId: {RoomId}", dto?.RoomId ?? 0);
            _logger.LogInformation("CheckInDate: {CheckInDate}", dto?.CheckInDate);
            _logger.LogInformation("CheckOutDate: {CheckOutDate}", dto?.CheckOutDate);
            _logger.LogInformation("Guests: {Guests}", dto?.NumberOfGuests ?? 0);
            _logger.LogInformation("TotalAmount: {TotalAmount}", dto?.TotalAmount ?? 0);
            _logger.LogInformation("PaymentMethod: {PaymentMethod}", dto?.PaymentMethod ?? "NULL");
            _logger.LogInformation("CardNumber present: {HasCard}", !string.IsNullOrEmpty(dto?.CardNumber));
            _logger.LogInformation("CVV present: {HasCVV}", !string.IsNullOrEmpty(dto?.CVV));
            _logger.LogInformation("=================================");

            ActionResult<ProcessRoomReservationResponseDto>? finalResult = null;
            try
            {
                if (!ModelState.IsValid)
                {
                    // Log all validation errors
                    _logger.LogError("VALIDATION FAILED - ModelState Errors:");
                    foreach (var modelError in ModelState)
                    {
                        foreach (var error in modelError.Value.Errors)
                        {
                            _logger.LogError("Field: {Field}, Error: {Error}", modelError.Key, error.ErrorMessage);
                        }
                    }
                    
                    var allErrors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    
                    return BadRequest(new ProcessRoomReservationResponseDto
                    {
                        Success = false,
                        Message = "Invalid data provided",
                        Error = new ProcessReservationErrorDto
                        {
                            Code = "VALIDATION_ERROR",
                            Description = allErrors.Length > 0 ? allErrors : "Validation failed"
                        }
                    });
                }

                // Get company ID from request or default
                var companyIdHeader = Request.Headers["X-Company-Id"].FirstOrDefault();
                int companyId = !string.IsNullOrEmpty(companyIdHeader) 
                    ? int.Parse(companyIdHeader) 
                    : 1; // Default company

                // Validate checkout settings requirements
                var checkoutSettings = await _companyService.GetCheckoutSettingsAsync();
                if (checkoutSettings != null)
                {
                    // Validate phone if required
                    if (checkoutSettings.PhoneNumberField == "required" && string.IsNullOrEmpty(dto.Phone))
                    {
                        return BadRequest(new ProcessRoomReservationResponseDto
                        {
                            Success = false,
                            Message = "Phone number is required",
                            Error = new ProcessReservationErrorDto
                            {
                                Code = "PHONE_REQUIRED",
                                Field = "phone",
                                Description = "Phone number is required based on checkout settings"
                            }
                        });
                    }

                    // Validate company name if required
                    if (checkoutSettings.CompanyNameField == "required" && string.IsNullOrEmpty(dto.CompanyName))
                    {
                        return BadRequest(new ProcessRoomReservationResponseDto
                        {
                            Success = false,
                            Message = "Company name is required",
                            Error = new ProcessReservationErrorDto
                            {
                                Code = "COMPANY_NAME_REQUIRED",
                                Field = "companyName",
                                Description = "Company name is required based on checkout settings"
                            }
                        });
                    }

                    // Check if guest checkout is allowed
                    if (!checkoutSettings.AllowGuestCheckout && !dto.CreateAccount)
                    {
                        return BadRequest(new ProcessRoomReservationResponseDto
                        {
                            Success = false,
                            Message = "Account creation is required",
                            Error = new ProcessReservationErrorDto
                            {
                                Code = "ACCOUNT_REQUIRED",
                                Description = "Guest checkout is not allowed. Please create an account."
                            }
                        });
                    }
                }

                // Validate password if creating account
                if (dto.CreateAccount && string.IsNullOrEmpty(dto.Password))
                {
                    return BadRequest(new ProcessRoomReservationResponseDto
                    {
                        Success = false,
                        Message = "Password is required when creating an account",
                        Error = new ProcessReservationErrorDto
                        {
                            Code = "PASSWORD_REQUIRED",
                            Field = "password",
                            Description = "Password is required when creating an account"
                        }
                    });
                }

                // Verify room availability
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(r => r.Id == dto.RoomId && r.CompanyId == companyId);

                if (room == null)
                {
                    return NotFound(new ProcessRoomReservationResponseDto
                    {
                        Success = false,
                        Message = "Room not found",
                        Error = new ProcessReservationErrorDto
                        {
                            Code = "ROOM_NOT_FOUND",
                            Description = "The selected room is not available"
                        }
                    });
                }

                // Check availability for the dates
                var isAvailable = await CheckRoomAvailability(dto.RoomId, dto.CheckInDate, dto.CheckOutDate);
                if (!isAvailable)
                {
                    return BadRequest(new ProcessRoomReservationResponseDto
                    {
                        Success = false,
                        Message = "Room is not available for selected dates",
                        Error = new ProcessReservationErrorDto
                        {
                            Code = "ROOM_NOT_AVAILABLE",
                            Description = "The room is not available for the selected dates"
                        }
                    });
                }

                // Ejecutar operaciones de BD bajo ExecutionStrategy + transacción para compatibilidad con EnableRetryOnFailure
                var executionStrategy = _context.Database.CreateExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                // Step 1: Create or update customer
                _logger.LogInformation("Processing customer for email: {Email}", dto.Email);
                
                // Find existing customer by email (case-insensitive). Fallback to phone if email missing.
                var normalizedEmail = dto.Email?.Trim().ToLower();
                Customer? customer = null;
                if (!string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Email.ToLower() == normalizedEmail);
                }
                if (customer == null && !string.IsNullOrWhiteSpace(dto.Phone))
                {
                    customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Phone == dto.Phone);
                }

                bool accountCreated = false;
                bool passwordEmailSent = false;
                string temporaryPassword = null;

                if (customer == null)
                {
                    // Create new customer
                    _logger.LogInformation("Creating new customer for email: {Email}", dto.Email);
                    
                    string password;
                    if (dto.CreateAccount)
                    {
                        // Customer chose to create account with their own password
                        password = dto.Password;
                        accountCreated = true;
                    }
                    else
                    {
                        // Generate temporary password for guest checkout
                        password = GenerateTemporaryPassword();
                        temporaryPassword = password;
                        passwordEmailSent = true;
                    }

                    // Resolve contact and billing address intent from DTO
                    var contactStreet = string.IsNullOrWhiteSpace(dto.ContactAddress) 
                        ? (dto.BillingDifferent ? null : dto.Address) 
                        : dto.ContactAddress;
                    var contactApartment = string.IsNullOrWhiteSpace(dto.ContactApartment) ? null : dto.ContactApartment;
                    var contactCity = string.IsNullOrWhiteSpace(dto.ContactCity) 
                        ? (dto.BillingDifferent ? null : dto.City) 
                        : dto.ContactCity;
                    var contactState = string.IsNullOrWhiteSpace(dto.ContactState) 
                        ? (dto.BillingDifferent ? null : dto.State) 
                        : dto.ContactState;
                    var contactPostalCode = string.IsNullOrWhiteSpace(dto.ContactPostalCode) 
                        ? (dto.BillingDifferent ? null : dto.PostalCode) 
                        : dto.ContactPostalCode;
                    var contactCountry = dto.Country; // Country already provided at top-level

                    var billingStreet = dto.BillingDifferent ? dto.Address : null;
                    var billingApartment = dto.BillingDifferent ? dto.Apartment : null;
                    var billingCity = dto.BillingDifferent ? dto.City : null;
                    var billingState = dto.BillingDifferent ? dto.State : null;
                    var billingPostalCode = dto.BillingDifferent ? dto.PostalCode : null;
                    var billingCountry = dto.BillingDifferent ? dto.Country : null;

                    customer = new Customer
                    {
                        Username = dto.Email, // Username IS the email for user-friendliness
                        Email = dto.Email,
                        PasswordHash = HashPassword(password),
                        FirstName = dto.FirstName,
                        LastName = dto.LastName ?? "",
                        Phone = dto.Phone,
                        Country = dto.Country,
                        CompanyName = dto.CompanyName,
                        TaxId = dto.TaxId,
                        Status = "Active",
                        CompanyId = companyId,
                        CustomerId = GenerateCustomerId(),
                        PreferredCurrency = dto.Currency,
                        // Persist billing fields ONLY if different billing address provided
                        BillingAddress = billingStreet,
                        BillingApartment = billingApartment,
                        BillingCity = billingCity,
                        BillingState = billingState,
                        BillingPostalCode = billingPostalCode,
                        BillingCountry = billingCountry,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Customer created with ID: {CustomerId}", customer.Id);

                    // Create default CONTACT address record if provided
                    if (!string.IsNullOrWhiteSpace(contactStreet))
                    {
                        var address = new CustomerAddress
                        {
                            CustomerId = customer.Id,
                            Type = "Home",
                            Label = $"{dto.FirstName} {dto.LastName} - Contact",
                            Street = contactStreet,
                            Apartment = contactApartment,
                            City = contactCity ?? string.Empty,
                            State = contactState,
                            PostalCode = contactPostalCode ?? string.Empty,
                            Country = contactCountry,
                            IsDefault = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CustomerAddresses.Add(address);
                        await _context.SaveChangesAsync();
                    }

                    // Create Billing address record if billingDifferent and provided
                    if (!string.IsNullOrWhiteSpace(billingStreet))
                    {
                        var address = new CustomerAddress
                        {
                            CustomerId = customer.Id,
                            Type = "Billing",
                            Label = $"{dto.FirstName} {dto.LastName} - Billing",
                            Street = billingStreet,
                            Apartment = billingApartment,
                            City = billingCity ?? "",
                            State = billingState,
                            PostalCode = billingPostalCode ?? "",
                            Country = billingCountry,
                            IsDefault = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CustomerAddresses.Add(address);
                        await _context.SaveChangesAsync();
                    }

                    // Send welcome email with credentials
                    if (!dto.CreateAccount && !string.IsNullOrEmpty(temporaryPassword))
                    {
                        await SendWelcomeEmailWithCredentials(dto.Email, temporaryPassword);
                    }
                    else if (dto.CreateAccount)
                    {
                        await SendWelcomeEmail(dto.Email, dto.FirstName);
                    }
                }
                else
                {
                    // Update existing customer data
                    _logger.LogInformation("Updating existing customer ID: {CustomerId}", customer.Id);
                    
                    customer.Phone = dto.Phone ?? customer.Phone;
                    customer.Country = dto.Country;
                    customer.FirstName = dto.FirstName;
                    customer.LastName = dto.LastName ?? customer.LastName;
                    customer.CompanyName = dto.CompanyName ?? customer.CompanyName;
                    customer.TaxId = dto.TaxId ?? customer.TaxId;
                    customer.UpdatedAt = DateTime.UtcNow;
                    
                    // Resolve contact and billing addresses from DTO
                    var updContactStreet = string.IsNullOrWhiteSpace(dto.ContactAddress) 
                        ? (dto.BillingDifferent ? null : dto.Address) 
                        : dto.ContactAddress;
                    var updContactApartment = string.IsNullOrWhiteSpace(dto.ContactApartment) ? null : dto.ContactApartment;
                    var updContactCity = string.IsNullOrWhiteSpace(dto.ContactCity) 
                        ? (dto.BillingDifferent ? null : dto.City) 
                        : dto.ContactCity;
                    var updContactState = string.IsNullOrWhiteSpace(dto.ContactState) 
                        ? (dto.BillingDifferent ? null : dto.State) 
                        : dto.ContactState;
                    var updContactPostalCode = string.IsNullOrWhiteSpace(dto.ContactPostalCode) 
                        ? (dto.BillingDifferent ? null : dto.PostalCode) 
                        : dto.ContactPostalCode;

                    var updBillingStreet = dto.BillingDifferent ? dto.Address : null;
                    var updBillingApartment = dto.BillingDifferent ? dto.Apartment : null;
                    var updBillingCity = dto.BillingDifferent ? dto.City : null;
                    var updBillingState = dto.BillingDifferent ? dto.State : null;
                    var updBillingPostalCode = dto.BillingDifferent ? dto.PostalCode : null;

                    // Update billing fields only if billingDifferent and values provided
                    if (!string.IsNullOrWhiteSpace(updBillingStreet)) customer.BillingAddress = updBillingStreet;
                    if (!string.IsNullOrWhiteSpace(updBillingApartment)) customer.BillingApartment = updBillingApartment;
                    if (!string.IsNullOrWhiteSpace(updBillingCity)) customer.BillingCity = updBillingCity;
                    if (!string.IsNullOrWhiteSpace(updBillingState)) customer.BillingState = updBillingState;
                    if (!string.IsNullOrWhiteSpace(updBillingPostalCode)) customer.BillingPostalCode = updBillingPostalCode;
                    if (!string.IsNullOrWhiteSpace(dto.Country) && dto.BillingDifferent) customer.BillingCountry = dto.Country;

                    await _context.SaveChangesAsync();

                    // Upsert CONTACT address record (Home)
                    if (!string.IsNullOrWhiteSpace(updContactStreet))
                    {
                        var existingHome = await _context.CustomerAddresses
                            .FirstOrDefaultAsync(a => a.CustomerId == customer.Id && a.Type == "Home" && a.IsDefault);

                        if (existingHome != null)
                        {
                            existingHome.Street = updContactStreet;
                            existingHome.Apartment = updContactApartment;
                            existingHome.City = updContactCity ?? existingHome.City;
                            existingHome.State = updContactState;
                            existingHome.PostalCode = updContactPostalCode ?? existingHome.PostalCode;
                            existingHome.Country = dto.Country;
                            existingHome.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var address = new CustomerAddress
                            {
                                CustomerId = customer.Id,
                                Type = "Home",
                                Label = $"{customer.FirstName} {customer.LastName} - Contact",
                                Street = updContactStreet,
                                Apartment = updContactApartment,
                                City = updContactCity ?? string.Empty,
                                State = updContactState,
                                PostalCode = updContactPostalCode ?? string.Empty,
                                Country = dto.Country,
                                IsDefault = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.CustomerAddresses.Add(address);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Upsert default Billing address record if user provided billing address
                    if (!string.IsNullOrWhiteSpace(updBillingStreet))
                    {
                        var existingBilling = await _context.CustomerAddresses
                            .FirstOrDefaultAsync(a => a.CustomerId == customer.Id && a.Type == "Billing" && a.IsDefault);

                        if (existingBilling != null)
                        {
                            existingBilling.Street = updBillingStreet;
                            existingBilling.Apartment = updBillingApartment;
                            existingBilling.City = updBillingCity ?? existingBilling.City;
                            existingBilling.State = updBillingState;
                            existingBilling.PostalCode = updBillingPostalCode ?? existingBilling.PostalCode;
                            existingBilling.Country = dto.Country;
                            existingBilling.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var address = new CustomerAddress
                            {
                                CustomerId = customer.Id,
                                Type = "Billing",
                                Label = $"{customer.FirstName} {customer.LastName} - Billing",
                                Street = updBillingStreet,
                                Apartment = updBillingApartment,
                                City = updBillingCity ?? "",
                                State = updBillingState,
                                PostalCode = updBillingPostalCode ?? "",
                                Country = dto.Country,
                                IsDefault = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.CustomerAddresses.Add(address);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // Step 2: Create reservation
                _logger.LogInformation("Creating reservation for customer ID: {CustomerId}", customer.Id);
                
                var reservation = new Reservation
                {
                    CompanyId = companyId,
                    CustomerId = customer.Id,
                    RoomId = dto.RoomId,
                    CheckInDate = dto.CheckInDate.Date,
                    CheckOutDate = dto.CheckOutDate.Date,
                    NumberOfGuests = dto.NumberOfGuests,
                    RoomRate = dto.RoomRate,
                    TotalAmount = dto.TotalAmount,
                    SpecialRequests = dto.SpecialRequests,
                    Status = "Pending", // Will be updated to Confirmed after payment
                    InternalNotes = $"Comprobante Fiscal: {GetComprobanteFiscalName(dto.TaxDocumentType)}" + 
                                   (dto.TaxId != null ? $" - RNC/Cédula: {dto.TaxId}" : ""),
                    CreatedAt = DateTime.UtcNow
                };

                // Calculate number of nights
                reservation.CalculateNights();

                // Add reservation custom billing snapshot if user provided billing address (regardless of tax document type)
                if (!string.IsNullOrWhiteSpace(dto.Address))
                {
                    reservation.CustomBillingAddress = dto.Address;
                    reservation.CustomBillingCity = dto.City;
                    reservation.CustomBillingState = dto.State;
                    reservation.CustomBillingPostalCode = dto.PostalCode;
                    reservation.CustomBillingCountry = dto.Country;
                }

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Reservation created with ID: {ReservationId}", reservation.Id);

                // Step 3: Process payment (simplified for now - in real implementation, integrate with payment gateway)
                bool paymentSuccess = await ProcessPayment(reservation.Id, dto);
                
                if (paymentSuccess)
                {
                    // Update reservation status to confirmed
                    reservation.Status = "Confirmed";
                    await _context.SaveChangesAsync();

                    // Update customer aggregate metrics
                    try
                    {
                        customer.TotalSpent = (customer.TotalSpent) + dto.TotalAmount;
                        customer.TotalOrders = (customer.TotalOrders) + 1;
                        customer.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update customer aggregates for {CustomerId}", customer.Id);
                    }

                    // Upsert customer payment method based on provided card details
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(dto.CardNumber) && !string.IsNullOrWhiteSpace(dto.ExpiryDate))
                        {
                            var last4 = dto.CardNumber.Length >= 4 ? dto.CardNumber[^4..] : dto.CardNumber;
                            string cardType = DetectCardType(dto.CardNumber);
                            // Parse MM/YY
                            var parts = dto.ExpiryDate.Split('/');
                            var mm = parts.Length > 0 ? parts[0] : "";
                            var yy = parts.Length > 1 ? (parts[1].Length == 2 ? $"20{parts[1]}" : parts[1]) : "";

                            // Find existing default or matching last4
                            var existing = await _context.CustomerPaymentMethods
                                .FirstOrDefaultAsync(p => p.CustomerId == customer.Id && p.Last4Digits == last4);

                            if (existing == null)
                            {
                                var method = new CustomerPaymentMethod
                                {
                                    CustomerId = customer.Id,
                                    CardType = cardType,
                                    CardholderName = dto.CardholderName ?? $"{customer.FirstName} {customer.LastName}".Trim(),
                                    Last4Digits = last4,
                                    ExpiryMonth = mm,
                                    ExpiryYear = yy,
                                    BillingAddress = BuildBillingAddressSnapshot(dto),
                                    IsPrimary = true
                                };
                                // If already has a primary, keep it and set this as non-primary
                                var hasPrimary = await _context.CustomerPaymentMethods.AnyAsync(p => p.CustomerId == customer.Id && p.IsPrimary);
                                if (hasPrimary) method.IsPrimary = false;
                                _context.CustomerPaymentMethods.Add(method);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                existing.CardType = cardType;
                                existing.CardholderName = dto.CardholderName ?? existing.CardholderName;
                                existing.ExpiryMonth = string.IsNullOrWhiteSpace(mm) ? existing.ExpiryMonth : mm;
                                existing.ExpiryYear = string.IsNullOrWhiteSpace(yy) ? existing.ExpiryYear : yy;
                                existing.BillingAddress = BuildBillingAddressSnapshot(dto) ?? existing.BillingAddress;
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to upsert customer payment method for customer {CustomerId}", customer.Id);
                    }

                    // Step 4: Update room availability calendar (sync with confirmed reservations)
                    try
                    {
                        await _availabilityService.SyncAvailabilityWithReservationsAsync(companyId);
                    }
                    catch (Exception syncEx)
                    {
                        _logger.LogWarning(syncEx, "Availability sync failed after confirming reservation {ReservationId}", reservation.Id);
                        // Non-blocking: don't fail the reservation if sync fails; widget will still fetch from Reservations
                    }

                    // Step 5: Send confirmation emails
                    await SendReservationConfirmation(customer.Email, reservation, room);

                    // Commit transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation("Reservation process completed successfully for reservation ID: {ReservationId}", reservation.Id);

                    finalResult = Ok(new ProcessRoomReservationResponseDto
                    {
                        Success = true,
                        Message = "Reservation confirmed successfully",
                        ReservationId = reservation.Id,
                        CustomerId = customer.Id,
                        ConfirmationNumber = $"RES{reservation.Id:D6}",
                        CustomerEmail = customer.Email,
                        AccountCreated = accountCreated,
                        PasswordEmailSent = passwordEmailSent
                    });
                    return; // finish lambda
                }
                else
                {
                    // Payment failed - rollback
                    await transaction.RollbackAsync();
                    
                    finalResult = BadRequest(new ProcessRoomReservationResponseDto
                    {
                        Success = false,
                        Message = "Payment processing failed",
                        Error = new ProcessReservationErrorDto
                        {
                            Code = "PAYMENT_FAILED",
                            Description = "Unable to process payment. Please check your card details and try again."
                        }
                    });
                    return; // finish lambda
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing room reservation");
                
                finalResult = StatusCode(500, new ProcessRoomReservationResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your reservation",
                    Error = new ProcessReservationErrorDto
                    {
                        Code = "INTERNAL_ERROR",
                        Description = "An unexpected error occurred. Please try again later."
                    }
                });
                return; // finish lambda
            }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing room reservation");
                return StatusCode(500, new ProcessRoomReservationResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your reservation",
                    Error = new ProcessReservationErrorDto
                    {
                        Code = "INTERNAL_ERROR",
                        Description = "An unexpected error occurred. Please try again later."
                    }
                });
            }

            if (finalResult == null)
            {
                _logger.LogError("finalResult was null after execution strategy block");
                return StatusCode(500, new ProcessRoomReservationResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your reservation",
                    Error = new ProcessReservationErrorDto
                    {
                        Code = "INTERNAL_ERROR",
                        Description = "Unexpected state after processing reservation."
                    }
                });
            }
            return finalResult;
        }

        private static string DetectCardType(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return "Card";
            var num = cardNumber.Replace(" ", "");
            if (System.Text.RegularExpressions.Regex.IsMatch(num, "^4[0-9]{6,}$")) return "Visa";
            if (System.Text.RegularExpressions.Regex.IsMatch(num, "^5[1-5][0-9]{5,}$")) return "Mastercard";
            if (System.Text.RegularExpressions.Regex.IsMatch(num, "^(34|37)[0-9]{5,}$")) return "AmericanExpress";
            if (System.Text.RegularExpressions.Regex.IsMatch(num, "^3(?:0[0-5]|[68][0-9])[0-9]{4,}$")) return "DinersClub";
            if (System.Text.RegularExpressions.Regex.IsMatch(num, "^6(?:011|5[0-9]{2})[0-9]{3,}$")) return "Discover";
            if (System.Text.RegularExpressions.Regex.IsMatch(num, "^(?:2131|1800|35[0-9]{3})[0-9]{3,}$")) return "JCB";
            return "Card";
        }

        private static string? BuildBillingAddressSnapshot(ProcessRoomReservationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Address)) return null;
            var parts = new[] { dto.Address, dto.Apartment, dto.City, dto.State, dto.PostalCode, dto.Country }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(", ", parts!);
        }

        private async Task<bool> CheckRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // Check if there are any confirmed reservations overlapping with these dates
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.RoomId == roomId 
                    && (r.Status == "Confirmed" || r.Status == "CheckedIn")
                    && r.CheckInDate < checkOut 
                    && r.CheckOutDate > checkIn);
                    
            return !hasConflict;
        }

        private async Task UpdateRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // Here you would update your availability calendar
            // This is a simplified version - in production you'd have a proper availability table
            _logger.LogInformation("Room {RoomId} blocked from {CheckIn} to {CheckOut}", 
                roomId, checkIn, checkOut);
            
            // TODO: Implement actual availability calendar update
            await Task.CompletedTask;
        }

        private async Task<bool> ProcessPayment(int reservationId, ProcessRoomReservationDto dto)
        {
            try
            {
                // This is a simplified payment processing
                // In production, you would integrate with a payment gateway like Stripe, PayPal, etc.
                
                _logger.LogInformation("Processing payment for reservation {ReservationId}", reservationId);
                
                // Get current user ID if authenticated
                var currentUserId = User?.Identity?.IsAuthenticated == true 
                    ? int.TryParse(User.FindFirst("userId")?.Value, out var id) ? id : (int?)null
                    : null;
                
                // Create payment record using only existing model properties
                var payment = new ReservationPayment
                {
                    ReservationId = reservationId,
                    Amount = dto.TotalAmount, // Using TotalAmount from dto
                    PaymentMethod = dto.PaymentMethod,
                    Status = "Completed", // In production, this would depend on actual payment processing
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Notes = $"Payment for reservation #{reservationId:D6}. Method: {dto.PaymentMethod}. " +
                           $"Card: ****{(dto.CardNumber?.Length >= 4 ? dto.CardNumber.Substring(dto.CardNumber.Length - 4) : "****")}",
                    ProcessedByUserId = currentUserId
                };
                
                _context.ReservationPayments.Add(payment);
                await _context.SaveChangesAsync();
                
                // Simulate payment processing delay
                await Task.Delay(500);
                
                // For testing, always return true
                // In production, this would depend on the payment gateway response
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for reservation {ReservationId}", reservationId);
                return false;
            }
        }

        private string GenerateCustomerId()
        {
            // Generate a unique customer ID like #895280
            var random = new Random();
            return $"#{random.Next(100000, 999999)}";
        }

        private string GenerateTemporaryPassword()
        {
            // Generate a secure temporary password
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#";
            var random = new Random();
            var password = new char[12];
            
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            
            // Ensure it has at least one uppercase, lowercase, number and special char
            password[0] = "ABCDEFGHJKLMNPQRSTUVWXYZ"[random.Next(25)];
            password[1] = "abcdefghijkmnpqrstuvwxyz"[random.Next(25)];
            password[2] = "23456789"[random.Next(8)];
            password[3] = "!@#"[random.Next(3)];
            
            return new string(password);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task SendWelcomeEmailWithCredentials(string email, string temporaryPassword)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    email,
                    "Reservation Confirmed - Your Account Details",
                    $@"
                    <h2>Your reservation has been confirmed!</h2>
                    <p>We've created an account for you to manage your reservation.</p>
                    <h3>Your login credentials:</h3>
                    <p><strong>Email:</strong> {email}</p>
                    <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                    <p>For security, please change your password after your first login.</p>
                    <p><a href='http://localhost:3000/login'>Login to your account</a></p>
                    ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            }
        }

        private async Task SendWelcomeEmail(string email, string firstName)
        {
            try
            {
                await _emailService.SendEmailAsync(
                    email,
                    "Welcome - Account Created Successfully",
                    $@"
                    <h2>Welcome {firstName}!</h2>
                    <p>Your account has been created successfully.</p>
                    <p>You can now login with your email and the password you chose.</p>
                    <p><a href='http://localhost:3000/login'>Login to your account</a></p>
                    ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            }
        }

        private async Task SendReservationConfirmation(string email, Reservation reservation, Room room)
        {
            try
            {
                var confirmationNumber = $"RES{reservation.Id:D6}";
                await _emailService.SendEmailAsync(
                    email,
                    $"Reservation Confirmed - {confirmationNumber}",
                    $@"
                    <h2>Reservation Confirmed!</h2>
                    <p><strong>Confirmation Number:</strong> {confirmationNumber}</p>
                    <h3>Reservation Details:</h3>
                    <p><strong>Room:</strong> {room.Name}</p>
                    <p><strong>Check-in:</strong> {reservation.CheckInDate:MMMM dd, yyyy}</p>
                    <p><strong>Check-out:</strong> {reservation.CheckOutDate:MMMM dd, yyyy}</p>
                    <p><strong>Guests:</strong> {reservation.NumberOfGuests}</p>
                    <p><strong>Total Amount:</strong> ${reservation.TotalAmount:F2}</p>
                    <p>We look forward to welcoming you!</p>
                    ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reservation confirmation to {Email}", email);
            }
        }

        private string GetComprobanteFiscalName(string taxDocumentType)
        {
            return taxDocumentType?.ToLower() switch
            {
                "consumidor_final" => "Consumidor Final",
                "credito_fiscal" => "Crédito Fiscal",
                "gubernamental" => "Gubernamental",
                "regimen_especial" => "Régimen Especial",
                _ => "Consumidor Final"
            };
        }
    }
}
