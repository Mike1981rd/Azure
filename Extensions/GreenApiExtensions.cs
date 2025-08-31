using System.Text.RegularExpressions;

namespace WebsiteBuilderAPI.Extensions
{
    /// <summary>
    /// Extension methods for Green API WhatsApp integration
    /// </summary>
    public static class GreenApiExtensions
    {
        /// <summary>
        /// Converts a phone number to Green API chat ID format
        /// Green API format: [country code][number]@c.us
        /// Example: +1234567890 becomes 1234567890@c.us
        /// </summary>
        /// <param name="phoneNumber">Phone number with or without + prefix</param>
        /// <returns>Green API formatted chat ID</returns>
        public static string ToGreenApiChatId(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));
            }

            // Remove all non-numeric characters
            var cleanNumber = Regex.Replace(phoneNumber, @"[^\d]", "");

            // If the number doesn't start with a country code, add default (1 for US/Dominican Republic)
            // A valid international number should have at least 11 digits
            if (cleanNumber.Length == 10)
            {
                // Assume US/Dominican Republic if 10 digits
                cleanNumber = "1" + cleanNumber;
            }
            
            // For numbers that already include country code, use as-is
            // This handles cases like 18096374142 (Dominican Republic with country code)

            // Validate the number has at least 7 digits (minimum for international format)
            if (cleanNumber.Length < 7)
            {
                throw new ArgumentException($"Invalid phone number format: {phoneNumber}", nameof(phoneNumber));
            }

            return $"{cleanNumber}@c.us";
        }

        /// <summary>
        /// Converts Green API chat ID back to phone number format
        /// </summary>
        /// <param name="chatId">Green API chat ID (e.g., 1234567890@c.us)</param>
        /// <returns>Phone number in international format (+1234567890)</returns>
        public static string FromGreenApiChatId(this string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
            }

            // Remove @c.us suffix
            var phoneNumber = chatId.Replace("@c.us", "");

            // Add + prefix if not present
            if (!phoneNumber.StartsWith("+"))
            {
                phoneNumber = "+" + phoneNumber;
            }

            return phoneNumber;
        }

        /// <summary>
        /// Validates if a string is a valid Green API chat ID format
        /// </summary>
        /// <param name="chatId">Chat ID to validate</param>
        /// <returns>True if valid Green API format, false otherwise</returns>
        public static bool IsValidGreenApiChatId(this string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                return false;
            }

            // Pattern: digits@c.us
            var pattern = @"^\d{7,15}@c\.us$";
            return Regex.IsMatch(chatId, pattern);
        }

        /// <summary>
        /// Validates if a phone number is in a format that can be converted to Green API
        /// </summary>
        /// <param name="phoneNumber">Phone number to validate</param>
        /// <returns>True if can be converted, false otherwise</returns>
        public static bool CanConvertToGreenApiChatId(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            try
            {
                var _ = phoneNumber.ToGreenApiChatId();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Normalizes phone number for Green API usage
        /// Removes spaces, dashes, parentheses, and other formatting
        /// </summary>
        /// <param name="phoneNumber">Raw phone number input</param>
        /// <returns>Normalized phone number ready for Green API conversion</returns>
        public static string NormalizePhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            // Remove all characters except digits and +
            var normalized = Regex.Replace(phoneNumber, @"[^\d+]", "");

            // Ensure + is only at the beginning
            if (normalized.Contains('+'))
            {
                var plusIndex = normalized.IndexOf('+');
                if (plusIndex > 0)
                {
                    // Move + to the beginning if it's not there
                    normalized = "+" + normalized.Replace("+", "");
                }
            }

            return normalized.Trim();
        }
    }
}