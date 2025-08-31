using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebsiteBuilderAPI.DTOs.Customers;
using WebsiteBuilderAPI.Services;

namespace WebsiteBuilderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] int page = 1, 
            [FromQuery] int size = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = "Active",
            [FromQuery] string? country = null,
            [FromQuery] string? loyaltyTier = null,
            [FromQuery] decimal? minSpent = null,
            [FromQuery] decimal? maxSpent = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                // CRITICAL: Use lowercase "companyId" per Guardado.md
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId;
                
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out companyId))
                {
                    companyId = 1; // Fallback for single-tenant
                }

                var filter = new CustomerFilterDto
                {
                    Page = page,
                    Size = size,
                    Search = search,
                    Status = status,
                    Country = country,
                    LoyaltyTier = loyaltyTier,
                    MinSpent = minSpent,
                    MaxSpent = maxSpent,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _customerService.GetCustomersAsync(companyId, filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers");
                return StatusCode(500, new { error = "An error occurred while retrieving customers", details = ex.Message });
            }
        }

        // GET: api/customers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var customer = await _customerService.GetCustomerByIdAsync(companyId, id);
                
                if (customer == null)
                    return NotFound(new { error = "Customer not found" });
                
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the customer", details = ex.Message });
            }
        }

        // POST: api/customers
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var customer = await _customerService.CreateCustomerAsync(companyId, dto);
                
                return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, new { error = "An error occurred while creating the customer", details = ex.Message });
            }
        }

        // PUT: api/customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var customer = await _customerService.UpdateCustomerAsync(companyId, id, dto);
                
                if (customer == null)
                    return NotFound(new { error = "Customer not found" });
                
                return Ok(customer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while updating the customer", details = ex.Message });
            }
        }

        // DELETE: api/customers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                await _customerService.DeleteCustomerAsync(companyId, id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the customer", details = ex.Message });
            }
        }

        // POST: api/customers/{id}/avatar
        [HttpPost("{id}/avatar")]
        public async Task<IActionResult> UpdateAvatar(int id, [FromBody] UpdateAvatarDto dto)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var customer = await _customerService.UpdateAvatarAsync(companyId, id, dto.AvatarUrl);
                
                if (customer == null)
                    return NotFound(new { error = "Customer not found" });
                
                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating avatar for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while updating the avatar", details = ex.Message });
            }
        }


        // POST: api/customers/{id}/two-factor/enable
        [HttpPost("{id}/two-factor/enable")]
        public async Task<IActionResult> EnableTwoFactor(int id, [FromBody] EnableTwoFactorDto dto)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _customerService.EnableTwoFactorAsync(companyId, id, dto.PhoneNumber);
                
                if (!result)
                    return NotFound(new { error = "Customer not found" });
                
                return Ok(new { message = "Two-factor authentication enabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while enabling two-factor authentication", details = ex.Message });
            }
        }

        // POST: api/customers/{id}/two-factor/disable
        [HttpPost("{id}/two-factor/disable")]
        public async Task<IActionResult> DisableTwoFactor(int id)
        {
            try
            {
                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                var result = await _customerService.DisableTwoFactorAsync(companyId, id);
                
                if (!result)
                    return NotFound(new { error = "Customer not found" });
                
                return Ok(new { message = "Two-factor authentication disabled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while disabling two-factor authentication", details = ex.Message });
            }
        }

        // GET: api/customers/{id}/addresses
        [HttpGet("{id}/addresses")]
        public async Task<IActionResult> GetAddresses(int id)
        {
            try
            {
                var addresses = await _customerService.GetAddressesAsync(id);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting addresses for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving addresses", details = ex.Message });
            }
        }

        // POST: api/customers/{id}/addresses
        [HttpPost("{id}/addresses")]
        public async Task<IActionResult> AddAddress(int id, [FromBody] AddAddressDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var address = await _customerService.AddAddressAsync(id, dto);
                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while adding the address", details = ex.Message });
            }
        }

        // PUT: api/customers/{id}/addresses/{addressId}
        [HttpPut("{id}/addresses/{addressId}")]
        public async Task<IActionResult> UpdateAddress(int id, int addressId, [FromBody] AddAddressDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var address = await _customerService.UpdateAddressAsync(id, addressId, dto);
                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId} for customer {Id}", addressId, id);
                return StatusCode(500, new { error = "An error occurred while updating the address", details = ex.Message });
            }
        }

        // DELETE: api/customers/{id}/addresses/{addressId}
        [HttpDelete("{id}/addresses/{addressId}")]
        public async Task<IActionResult> DeleteAddress(int id, int addressId)
        {
            try
            {
                await _customerService.DeleteAddressAsync(id, addressId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId} for customer {Id}", addressId, id);
                return StatusCode(500, new { error = "An error occurred while deleting the address", details = ex.Message });
            }
        }

        // GET: api/customers/{id}/payment-methods
        [HttpGet("{id}/payment-methods")]
        public async Task<IActionResult> GetPaymentMethods(int id)
        {
            try
            {
                var methods = await _customerService.GetPaymentMethodsAsync(id);
                return Ok(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving payment methods", details = ex.Message });
            }
        }

        // POST: api/customers/{id}/payment-methods
        [HttpPost("{id}/payment-methods")]
        public async Task<IActionResult> AddPaymentMethod(int id, [FromBody] AddPaymentMethodDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var method = await _customerService.AddPaymentMethodAsync(id, dto);
                return Ok(method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while adding the payment method", details = ex.Message });
            }
        }

        // DELETE: api/customers/{id}/payment-methods/{methodId}
        [HttpDelete("{id}/payment-methods/{methodId}")]
        public async Task<IActionResult> DeletePaymentMethod(int id, int methodId)
        {
            try
            {
                await _customerService.DeletePaymentMethodAsync(id, methodId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment method {MethodId} for customer {Id}", methodId, id);
                return StatusCode(500, new { error = "An error occurred while deleting the payment method", details = ex.Message });
            }
        }

        // GET: api/customers/{id}/notifications
        [HttpGet("{id}/notifications")]
        public async Task<IActionResult> GetNotificationPreferences(int id)
        {
            try
            {
                var preferences = await _customerService.GetNotificationPreferencesAsync(id);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving notification preferences", details = ex.Message });
            }
        }

        // PUT: api/customers/{id}/notifications
        [HttpPut("{id}/notifications")]
        public async Task<IActionResult> UpdateNotificationPreferences(int id, [FromBody] UpdateNotificationPreferencesDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _customerService.UpdateNotificationPreferencesAsync(id, dto);
                return Ok(new { message = "Notification preferences updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while updating notification preferences", details = ex.Message });
            }
        }

        // GET: api/customers/{id}/devices
        [HttpGet("{id}/devices")]
        public async Task<IActionResult> GetDevices(int id)
        {
            try
            {
                var devices = await _customerService.GetDevicesAsync(id);
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting devices for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving devices", details = ex.Message });
            }
        }

        // DELETE: api/customers/{id}/devices/{deviceId}
        [HttpDelete("{id}/devices/{deviceId}")]
        public async Task<IActionResult> DeleteDevice(int id, int deviceId)
        {
            try
            {
                await _customerService.DeleteDeviceAsync(id, deviceId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting device {DeviceId} for customer {Id}", deviceId, id);
                return StatusCode(500, new { error = "An error occurred while deleting the device", details = ex.Message });
            }
        }

        // GET: api/customers/{id}/wishlist
        [HttpGet("{id}/wishlist")]
        public async Task<IActionResult> GetWishlist(int id)
        {
            try
            {
                var wishlist = await _customerService.GetWishlistAsync(id);
                return Ok(wishlist);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wishlist for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the wishlist", details = ex.Message });
            }
        }

        // GET: api/customers/{id}/coupons
        [HttpGet("{id}/coupons")]
        public async Task<IActionResult> GetCoupons(int id)
        {
            try
            {
                var coupons = await _customerService.GetCouponsAsync(id);
                return Ok(coupons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coupons for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving coupons", details = ex.Message });
            }
        }

        // PUT: api/customers/{id}/security
        [HttpPut("{id}/security")]
        public async Task<IActionResult> UpdateSecuritySettings(int id, [FromBody] UpdateSecuritySettingsDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _customerService.UpdateSecuritySettingsAsync(id, dto);
                return Ok(new { message = "Security settings updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security settings for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while updating security settings", details = ex.Message });
            }
        }

        // POST: api/customers/{id}/change-password
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] CustomerPasswordChangeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var success = await _customerService.ChangePasswordAsync(id, dto);
                
                if (!success)
                    return BadRequest(new { error = "Current password is incorrect" });
                
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while changing password", details = ex.Message });
            }
        }

        // GET: api/customers/{id}/security-questions
        [HttpGet("{id}/security-questions")]
        public async Task<IActionResult> GetSecurityQuestions(int id)
        {
            try
            {
                var questions = await _customerService.GetSecurityQuestionsAsync(id);
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security questions for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving security questions", details = ex.Message });
            }
        }
        
        // POST: api/customers/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Admin,SuperAdmin")] // Only admins can reset passwords
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                var result = await _customerService.ResetPasswordAsync(id, dto);
                
                if (!result.Success)
                    return BadRequest(new { error = result.Message });
                
                // Return the temporary password ONLY ONCE
                return Ok(new 
                { 
                    message = result.Message,
                    temporaryPassword = result.TemporaryPassword,
                    emailSent = result.EmailSent,
                    warning = "This temporary password is shown only once. Make sure to save it or communicate it securely to the customer."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for customer {Id}", id);
                return StatusCode(500, new { error = "An error occurred while resetting the password", details = ex.Message });
            }
        }
    }
}