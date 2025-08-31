using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderAPI.Data;
using WebsiteBuilderAPI.DTOs.Policies;
using WebsiteBuilderAPI.Models;

namespace WebsiteBuilderAPI.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly ApplicationDbContext _context;

        public PolicyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PolicyDto>> GetAllByCompanyAsync(int companyId)
        {
            // First ensure all policies exist for this company
            await InitializePoliciesForCompanyAsync(companyId);

            var policies = await _context.Policies
                .Where(p => p.CompanyId == companyId)
                .ToListAsync();

            // Map to DTOs and sort in memory
            var policyDtos = policies.Select(p => new PolicyDto
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                Type = p.Type,
                Title = p.Title,
                Content = p.Content ?? string.Empty,
                IsRequired = p.IsRequired,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Icon = GetPolicyIcon(p.Type),
                Description = GetPolicyDescription(p.Type)
            })
            .OrderBy(p => GetPolicyOrder(p.Type))
            .ToList();

            return policyDtos;
        }

        public async Task<PolicyDto> GetByTypeAsync(int companyId, string type)
        {
            var policy = await _context.Policies
                .Where(p => p.CompanyId == companyId && p.Type == type)
                .FirstOrDefaultAsync();

            if (policy == null)
            {
                // Create the policy if it doesn't exist
                await InitializeSinglePolicyAsync(companyId, type);
                policy = await _context.Policies
                    .Where(p => p.CompanyId == companyId && p.Type == type)
                    .FirstOrDefaultAsync();
            }

            return new PolicyDto
            {
                Id = policy.Id,
                CompanyId = policy.CompanyId,
                Type = policy.Type,
                Title = policy.Title,
                Content = policy.Content ?? string.Empty,
                IsRequired = policy.IsRequired,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt,
                Icon = GetPolicyIcon(policy.Type),
                Description = GetPolicyDescription(policy.Type)
            };
        }

        public async Task<PolicyDto> UpdateAsync(int companyId, string type, UpdatePolicyDto dto)
        {
            var policy = await _context.Policies
                .Where(p => p.CompanyId == companyId && p.Type == type)
                .FirstOrDefaultAsync();

            if (policy == null)
            {
                throw new InvalidOperationException($"Policy of type '{type}' not found for company {companyId}");
            }

            policy.Title = dto.Title;
            policy.Content = dto.Content;
            policy.IsRequired = dto.IsRequired;
            policy.IsActive = dto.IsActive;
            policy.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PolicyDto
            {
                Id = policy.Id,
                CompanyId = policy.CompanyId,
                Type = policy.Type,
                Title = policy.Title,
                Content = policy.Content ?? string.Empty,
                IsRequired = policy.IsRequired,
                IsActive = policy.IsActive,
                CreatedAt = policy.CreatedAt,
                UpdatedAt = policy.UpdatedAt,
                Icon = GetPolicyIcon(policy.Type),
                Description = GetPolicyDescription(policy.Type)
            };
        }

        public async Task InitializePoliciesForCompanyAsync(int companyId)
        {
            var existingTypes = await _context.Policies
                .Where(p => p.CompanyId == companyId)
                .Select(p => p.Type)
                .ToListAsync();

            var missingTypes = PolicyTypes.AllTypes.Except(existingTypes).ToList();

            if (missingTypes.Any())
            {
                foreach (var type in missingTypes)
                {
                    await InitializeSinglePolicyAsync(companyId, type);
                }
            }
        }

        public async Task<bool> PolicyExistsAsync(int companyId, string type)
        {
            return await _context.Policies
                .AnyAsync(p => p.CompanyId == companyId && p.Type == type);
        }

        private async Task InitializeSinglePolicyAsync(int companyId, string type)
        {
            var policy = new Policy
            {
                CompanyId = companyId,
                Type = type,
                Title = PolicyTypes.DefaultTitles.ContainsKey(type) 
                    ? PolicyTypes.DefaultTitles[type] 
                    : type,
                Content = GetDefaultContent(type),
                IsRequired = type == PolicyTypes.Terms || type == PolicyTypes.Privacy,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
        }

        private string GetDefaultContent(string type)
        {
            return type switch
            {
                PolicyTypes.Returns => @"<p>All orders can be canceled until they are shipped. If your order has been paid and you need to change or cancel it, please contact us within 12 hours of placing it. We have a 14-day return policy, which means you have 14 days after receiving your item to request a return.</p>
<p>To be eligible for a return, your item must be in the same condition that you received it, unworn or unused, with tags, and in its original packaging. You'll also need the receipt or proof of purchase.</p>
<p>To start a return, you can contact us at&nbsp;support@purrteam.com&nbsp;If your return is accepted, we'll send you a return shipping label, as well as instructions on how and where to send your package. Items sent back to us without first requesting a return will not be accepted.</p>
<h3>Damages and issues</h3>
<p>Please inspect your order upon reception and contact us immediately if the item is defective, damaged or if you receive the wrong item, so that we can evaluate the issue and make it right.</p>
<h3>Exceptions / non-returnable items</h3>
<p>Certain types of items cannot be returned, like perishable goods (such as food, flowers, or plants), custom products (such as special orders or personalized items), and personal care goods (such as beauty products). We also do not accept returns for hazardous materials, flammable liquids, or gases. Please get in touch if you have questions or concerns about your specific item.</p>
<p>Unfortunately, we cannot accept returns on sale items or gift cards.</p>
<h3>Exchanges</h3>
<p>The fastest way to ensure you get what you want is to return the item you have, and once the return is accepted, make a separate purchase for the new item.</p>
<h3>Refunds</h3>
<p>We will notify you once we've received and inspected your return, and let you know if the refund was approved or not. If approved, you'll be automatically refunded on your original payment method. Please remember it can take some time for your bank or credit card company to process and post the refund too.</p>",
                PolicyTypes.Privacy => "<p>Esta política de privacidad describe cómo recopilamos, usamos y protegemos su información personal.</p>",
                PolicyTypes.Terms => "<p>Estos términos y condiciones rigen el uso de nuestro sitio web y servicios.</p>",
                PolicyTypes.Shipping => "<p>Información sobre nuestros métodos de envío, tiempos de entrega y costos.</p>",
                PolicyTypes.Contact => "<p>Puede contactarnos a través de los siguientes medios:</p><ul><li>Email: info@empresa.com</li><li>Teléfono: +1 234 567 8900</li><li>Dirección: Calle Principal 123</li></ul>",
                _ => "<p>Contenido de política por defecto.</p>"
            };
        }

        private string GetPolicyIcon(string type)
        {
            return type switch
            {
                PolicyTypes.Returns => "📦",
                PolicyTypes.Privacy => "🔒",
                PolicyTypes.Terms => "📄",
                PolicyTypes.Shipping => "🚚",
                PolicyTypes.Contact => "📧",
                _ => "📄"
            };
        }

        private string GetPolicyDescription(string type)
        {
            return type switch
            {
                PolicyTypes.Returns => "Define las condiciones para devoluciones y reembolsos",
                PolicyTypes.Privacy => "Explica cómo se maneja la información personal",
                PolicyTypes.Terms => "Términos y condiciones del servicio",
                PolicyTypes.Shipping => "Información sobre envíos y entregas",
                PolicyTypes.Contact => "Información de contacto de la empresa",
                _ => ""
            };
        }

        private int GetPolicyOrder(string type)
        {
            return type switch
            {
                PolicyTypes.Returns => 1,
                PolicyTypes.Privacy => 2,
                PolicyTypes.Terms => 3,
                PolicyTypes.Shipping => 4,
                PolicyTypes.Contact => 5,
                _ => 99
            };
        }
    }
}