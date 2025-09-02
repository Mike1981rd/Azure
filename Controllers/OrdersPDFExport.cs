using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebsiteBuilderAPI.DTOs;
using WebsiteBuilderAPI.DTOs.Company;

namespace WebsiteBuilderAPI.Controllers
{
    public partial class OrdersController
    {
        /// <summary>
        /// Exporta una orden a PDF con diseño profesional
        /// </summary>
        [HttpGet("{id}/export-pdf")]
        public async Task<IActionResult> ExportToPdf(int id)
        {
            try
            {
                // Configure QuestPDF license and enable debugging
                QuestPDF.Settings.License = LicenseType.Community;
                QuestPDF.Settings.EnableDebugging = true;

                var companyIdClaim = User.FindFirst("companyId")?.Value;
                int companyId = string.IsNullOrEmpty(companyIdClaim) ? 1 : int.Parse(companyIdClaim);

                // Get order and company data
                var order = await _orderService.GetByIdAsync(companyId, id);
                if (order == null)
                {
                    return NotFound(new { error = $"Orden {id} no encontrada" });
                }

                var company = await _companyService.GetCurrentCompanyAsync();
                
                // Use actual company colors or defaults
                var primaryColorHex = !string.IsNullOrEmpty(company?.PrimaryColor) ? company.PrimaryColor : "#3b82f6";
                var secondaryColorHex = !string.IsNullOrEmpty(company?.SecondaryColor) ? company.SecondaryColor : "#64748b";
                
                var primaryColor = ParseHexColor(primaryColorHex);
                var secondaryColor = ParseHexColor(secondaryColorHex);
                
                _logger.LogInformation($"PDF Generation - Company: {company?.Name}, Logo: {company?.Logo}, Primary: {primaryColorHex}, Secondary: {secondaryColorHex}");

                // Number formatter for currency
                var numberFormat = new NumberFormatInfo
                {
                    NumberDecimalSeparator = ".",
                    NumberGroupSeparator = ",",
                    CurrencyDecimalDigits = 2
                };

                // Generate PDF document
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken4));

                        // Professional Header
                        page.Header()
                            .MinHeight(90)
                            .Background(Colors.White)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(20)
                            .Row(row =>
                            {
                                // Company branding section
                                row.RelativeItem(2).Column(column =>
                                {
                                    // Try to display logo
                                    if (!string.IsNullOrEmpty(company?.Logo))
                                    {
                                        var logoFileName = Path.GetFileName(company.Logo);
                                        var logoPath = Path.Combine(_environment.WebRootPath, "uploads", "logos", logoFileName);
                                        
                                        _logger.LogInformation($"Looking for logo at: {logoPath}");
                                        
                                        if (System.IO.File.Exists(logoPath))
                                        {
                                            column.Item().MaxHeight(50).MaxWidth(200).Image(logoPath).FitArea();
                                        }
                                        else
                                        {
                                            // Fallback to text logo with company name
                                            column.Item().Text(company?.Name ?? "Company")
                                                .FontSize(24)
                                                .Bold()
                                                .FontColor(Colors.Black);
                                        }
                                    }
                                    else
                                    {
                                        column.Item().Text(company?.Name ?? "Company")
                                            .FontSize(24)
                                            .Bold()
                                            .FontColor(Colors.Black);
                                    }
                                    
                                    // Company details if available
                                    if (!string.IsNullOrEmpty(company?.ContactEmail))
                                    {
                                        column.Item().PaddingTop(2).Text(company.ContactEmail)
                                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                                    }
                                    if (!string.IsNullOrEmpty(company?.PhoneNumber))
                                    {
                                        column.Item().Text(company.PhoneNumber)
                                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                                    }
                                });

                                // Invoice details section
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().AlignRight().Text("INVOICE")
                                        .FontSize(28)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    column.Item().AlignRight().PaddingTop(5).Row(r =>
                                    {
                                        r.AutoItem().Text("Invoice #: ").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        r.AutoItem().Text(order.FormattedOrderNumber).FontSize(10).Bold().FontColor(Colors.Black);
                                    });
                                    
                                    column.Item().AlignRight().PaddingTop(2).Row(r =>
                                    {
                                        r.AutoItem().Text("Date: ").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        r.AutoItem().Text($"{order.OrderDate:MMMM dd, yyyy}").FontSize(10).FontColor(Colors.Black);
                                    });
                                    
                                    column.Item().AlignRight().PaddingTop(2).Row(r =>
                                    {
                                        r.AutoItem().Text("Status: ").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        r.AutoItem().Text(order.PaymentStatus).FontSize(10).Bold().FontColor(Colors.Black);
                                    });
                                });
                            });

                        // Content
                        page.Content()
                            .PaddingVertical(20)
                            .Column(column =>
                            {
                                column.Spacing(15);

                                // Customer and Order Info Row
                                column.Item().Row(row =>
                                {
                                    // Bill To
                                    row.RelativeItem().PaddingRight(15)
                                        .Column(col =>
                                        {
                                            col.Item().Text("BILL TO").FontSize(11).Bold().FontColor(Colors.Black);
                                            col.Item().PaddingTop(8).Text(order.CustomerName).FontSize(11).SemiBold();
                                            col.Item().PaddingTop(2).Text(order.CustomerEmail).FontSize(9);
                                            if (!string.IsNullOrEmpty(order.CustomerPhone))
                                                col.Item().PaddingTop(2).Text(order.CustomerPhone).FontSize(9);
                                            
                                            // Billing address if available
                                            if (order.BillingAddress != null)
                                            {
                                                col.Item().PaddingTop(8);
                                                col.Item().Text(order.BillingAddress.Street).FontSize(9);
                                                if (!string.IsNullOrEmpty(order.BillingAddress.Street2))
                                                    col.Item().Text(order.BillingAddress.Street2).FontSize(9);
                                                col.Item().Text($"{order.BillingAddress.City}, {order.BillingAddress.State} {order.BillingAddress.PostalCode}").FontSize(9);
                                                col.Item().Text(order.BillingAddress.Country).FontSize(9);
                                            }
                                        });

                                    row.ConstantItem(15); // Spacer

                                    // Ship To
                                    row.RelativeItem().PaddingRight(15)
                                        .Column(col =>
                                        {
                                            col.Item().Text("SHIP TO").FontSize(11).Bold().FontColor(Colors.Black);
                                            
                                            if (order.ShippingAddress != null)
                                            {
                                                col.Item().PaddingTop(8).Text(order.CustomerName).FontSize(11).SemiBold();
                                                col.Item().PaddingTop(8).Text(order.ShippingAddress.Street).FontSize(9);
                                                if (!string.IsNullOrEmpty(order.ShippingAddress.Street2))
                                                    col.Item().Text(order.ShippingAddress.Street2).FontSize(9);
                                                col.Item().Text($"{order.ShippingAddress.City}, {order.ShippingAddress.State} {order.ShippingAddress.PostalCode}").FontSize(9);
                                                col.Item().Text(order.ShippingAddress.Country).FontSize(9);
                                            }
                                            else
                                            {
                                                col.Item().PaddingTop(8).Text("Same as billing address").FontSize(9).Italic();
                                            }
                                        });

                                    row.ConstantItem(15); // Spacer

                                    // Order Details
                                    row.RelativeItem()
                                        .Column(col =>
                                        {
                                            col.Item().Text("ORDER DETAILS").FontSize(11).Bold().FontColor(Colors.Black);
                                            
                                            col.Item().PaddingTop(8).Row(r =>
                                            {
                                                r.RelativeItem().Text("Order Status:").FontSize(9);
                                                r.RelativeItem().AlignRight().Text(order.OrderStatus).FontSize(9).Bold();
                                            });
                                            
                                            col.Item().PaddingTop(4).Row(r =>
                                            {
                                                r.RelativeItem().Text("Payment Status:").FontSize(9);
                                                r.RelativeItem().AlignRight().Text(order.PaymentStatus).FontSize(9).Bold();
                                            });
                                            
                                            col.Item().PaddingTop(4).Row(r =>
                                            {
                                                r.RelativeItem().Text("Payment Method:").FontSize(9);
                                                r.RelativeItem().AlignRight().Text(order.PaymentMethod ?? "N/A").FontSize(9);
                                            });
                                            
                                            col.Item().PaddingTop(4).Row(r =>
                                            {
                                                r.RelativeItem().Text("Delivery Status:").FontSize(9);
                                                r.RelativeItem().AlignRight().Text(order.DeliveryStatus).FontSize(9).Bold();
                                            });
                                        });
                                });

                                // Order Items Table
                                column.Item().PaddingTop(10).Table(table =>
                                {
                                    // Define columns
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(50);  // Item #
                                        columns.RelativeColumn(3);   // Description
                                        columns.RelativeColumn();    // SKU
                                        columns.RelativeColumn();    // Qty
                                        columns.RelativeColumn();    // Unit Price
                                        columns.RelativeColumn();    // Total
                                    });

                                    // Corporate style header
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Darken2).Padding(10)
                                            .Text("#").FontSize(10).Bold().FontColor(Colors.Black);
                                        header.Cell().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Darken2).Padding(10)
                                            .Text("DESCRIPTION").FontSize(10).Bold().FontColor(Colors.Black);
                                        header.Cell().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Darken2).Padding(10)
                                            .Text("SKU").FontSize(10).Bold().FontColor(Colors.Black);
                                        header.Cell().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Darken2).Padding(10).AlignRight()
                                            .Text("QTY").FontSize(10).Bold().FontColor(Colors.Black);
                                        header.Cell().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Darken2).Padding(10).AlignRight()
                                            .Text("UNIT PRICE").FontSize(10).Bold().FontColor(Colors.Black);
                                        header.Cell().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Darken2).Padding(10).AlignRight()
                                            .Text("TOTAL").FontSize(10).Bold().FontColor(Colors.Black);
                                    });

                                    // Items
                                    var itemIndex = 1;
                                    foreach (var item in order.Items)
                                    {
                                        var bgColor = itemIndex % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                        
                                        table.Cell().Background(bgColor).Padding(8)
                                            .Text(itemIndex.ToString()).FontSize(9);
                                        table.Cell().Background(bgColor).Padding(8)
                                            .Text(item.ProductName).FontSize(9);
                                        table.Cell().Background(bgColor).Padding(8)
                                            .Text(item.ProductSku ?? "-").FontSize(9);
                                        table.Cell().Background(bgColor).Padding(8).AlignRight()
                                            .Text(item.Quantity.ToString()).FontSize(9);
                                        table.Cell().Background(bgColor).Padding(8).AlignRight()
                                            .Text(FormatCurrency(item.UnitPrice, order.Currency, numberFormat)).FontSize(9);
                                        table.Cell().Background(bgColor).Padding(8).AlignRight()
                                            .Text(FormatCurrency(item.TotalPrice, order.Currency, numberFormat)).FontSize(9).SemiBold();
                                        
                                        itemIndex++;
                                    }
                                });

                                // Professional Summary Section
                                column.Item().AlignRight().MaxWidth(250).Column(col =>
                                {
                                    // Subtotal row
                                    col.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("Subtotal").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        r.ConstantItem(120).AlignRight()
                                            .Text(FormatCurrency(order.SubTotal, order.Currency, numberFormat))
                                            .FontSize(10).FontColor(Colors.Black);
                                    });

                                    // Discount if exists
                                    if (order.DiscountAmount > 0)
                                    {
                                        col.Item().PaddingTop(4).Row(r =>
                                        {
                                            r.RelativeItem().Text("Discount").FontSize(10).FontColor(Colors.Grey.Darken2);
                                            r.ConstantItem(120).AlignRight()
                                                .Text($"({FormatCurrency(order.DiscountAmount, order.Currency, numberFormat)})")
                                                .FontSize(10).FontColor(Colors.Black);
                                        });
                                    }

                                    // Tax
                                    col.Item().PaddingTop(4).Row(r =>
                                    {
                                        r.RelativeItem().Text("Tax").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        r.ConstantItem(120).AlignRight()
                                            .Text(FormatCurrency(order.TaxAmount, order.Currency, numberFormat))
                                            .FontSize(10).FontColor(Colors.Black);
                                    });

                                    // Shipping
                                    col.Item().PaddingTop(4).Row(r =>
                                    {
                                        r.RelativeItem().Text("Shipping").FontSize(10).FontColor(Colors.Grey.Darken2);
                                        r.ConstantItem(120).AlignRight()
                                            .Text(FormatCurrency(order.ShippingAmount, order.Currency, numberFormat))
                                            .FontSize(10).FontColor(Colors.Black);
                                    });

                                    // Separator line
                                    col.Item().PaddingTop(8).BorderBottom(1).BorderColor(Colors.Grey.Darken2);

                                    // Total Amount
                                    col.Item().PaddingTop(8).Row(r =>
                                    {
                                        r.RelativeItem().Text("TOTAL AMOUNT").FontSize(12).Bold().FontColor(Colors.Black);
                                        r.ConstantItem(120).AlignRight()
                                            .Text(FormatCurrency(order.TotalAmount, order.Currency, numberFormat))
                                            .FontSize(12).Bold().FontColor(Colors.Black);
                                    });
                                    
                                    // Payment status
                                    if (order.PaymentStatus == "Paid")
                                    {
                                        col.Item().PaddingTop(4).Row(r =>
                                        {
                                            r.RelativeItem().Text("Payment Status").FontSize(9).FontColor(Colors.Grey.Darken2);
                                            r.ConstantItem(120).AlignRight()
                                                .Text("PAID").FontSize(9).Bold().FontColor(Colors.Black);
                                        });
                                    }
                                });

                                // Notes section if exists
                                if (!string.IsNullOrEmpty(order.Notes))
                                {
                                    column.Item().PaddingTop(20).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15)
                                        .Column(col =>
                                        {
                                            col.Item().Text("NOTES").FontSize(11).Bold().FontColor(primaryColor);
                                            col.Item().PaddingTop(5).Text(order.Notes).FontSize(9);
                                        });
                                }
                            });

                        // Professional Footer
                        page.Footer()
                            .MinHeight(50)
                            .BorderTop(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(15)
                            .Column(column =>
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem().AlignLeft().Text(text =>
                                    {
                                        text.Span("Thank you for your business!").FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
                                    });
                                    
                                    row.RelativeItem().AlignRight().Text(text =>
                                    {
                                        text.Span($"Page ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken2);
                                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken2);
                                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken2);
                                    });
                                });
                                
                                column.Item().PaddingTop(5).AlignCenter().Text(text =>
                                {
                                    text.Span($"© {DateTime.Now.Year} {company?.Name ?? "Company"} - Generated on {DateTime.Now:MMMM dd, yyyy}").FontSize(7).FontColor(Colors.Grey.Darken1);
                                });
                            });
                    });
                });

                var pdfBytes = pdf.GeneratePdf();

                _logger.LogInformation($"Professional PDF generated for order {id}");
                return File(pdfBytes, "application/pdf", $"invoice-{order.FormattedOrderNumber}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting order {id} to PDF");
                return StatusCode(500, new { error = "Error al exportar la orden a PDF", details = ex.Message });
            }
        }

        private string FormatCurrency(decimal amount, string currency, NumberFormatInfo format)
        {
            var symbol = currency switch
            {
                "USD" => "$",
                "EUR" => "€",
                "GBP" => "£",
                "DOP" => "RD$",
                _ => currency + " "
            };
            
            return $"{symbol}{amount.ToString("N2", format)}";
        }

        private Color ParseHexColor(string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length != 6) return Colors.Blue.Medium;
                
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);
                
                return Color.FromRGB(r, g, b);
            }
            catch
            {
                return Colors.Blue.Medium;
            }
        }
    }
}