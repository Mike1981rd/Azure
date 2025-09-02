using System;

namespace WebsiteBuilderAPI.DTOs.Location
{
    public class LocationDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public bool FulfillOnlineOrders { get; set; }
        public string Address { get; set; }
        public string Apartment { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PinCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateLocationDto
    {
        public string Name { get; set; }
        public bool FulfillOnlineOrders { get; set; } = true;
        public string Address { get; set; }
        public string Apartment { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PinCode { get; set; }
    }

    public class UpdateLocationDto
    {
        public string Name { get; set; }
        public bool FulfillOnlineOrders { get; set; }
        public string Address { get; set; }
        public string Apartment { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PinCode { get; set; }
    }

    public class SetDefaultLocationDto
    {
        public int LocationId { get; set; }
    }
}