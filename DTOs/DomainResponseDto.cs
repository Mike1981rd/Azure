using System;
using System.Collections.Generic;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.DTOs
{
    public class DomainResponseDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public string? SubDomain { get; set; }
        public DomainStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsPrimary { get; set; }
        
        // SSL Information
        public bool HasSsl { get; set; }
        public DateTime? SslExpiryDate { get; set; }
        public string? SslProvider { get; set; }
        public int? SslDaysUntilExpiry { get; set; }
        
        // Verification
        public bool IsVerified { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }
        
        // Provider Information
        public string? Provider { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public string? Notes { get; set; }
        
        // DNS Records
        public List<DnsRecordResponseDto> DnsRecords { get; set; } = new();
        
        // Audit Information
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Additional Information
        public string FullUrl => $"https://{DomainName}";
        public bool RequiresAction { get; set; }
        public List<string> PendingActions { get; set; } = new();
    }
    
    public class DnsRecordResponseDto
    {
        public int Id { get; set; }
        public DnsRecordType Type { get; set; }
        public string TypeDisplay { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int TTL { get; set; }
        public string TTLDisplay { get; set; } = string.Empty;
        public int? Priority { get; set; }
        public int? Port { get; set; }
        public int? Weight { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    // Response wrapper for API operations
    public class DomainOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DomainResponseDto? Data { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
        public string? ErrorCode { get; set; }
        public DomainOperationDetails? Details { get; set; }
    }
    
    public class DomainOperationDetails
    {
        public int? DnsRecordsCreated { get; set; }
        public int? DnsRecordsUpdated { get; set; }
        public int? DnsRecordsDeleted { get; set; }
        public bool? VerificationPending { get; set; }
        public string? EstimatedPropagationTime { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }
}