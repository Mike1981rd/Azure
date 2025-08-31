using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebsiteBuilderAPI.DTOs.Collections
{
    public class ManageCollectionProductsDto
    {
        [Required]
        public List<int> ProductIds { get; set; } = new List<int>();
    }
    
    public class UpdateProductPositionDto
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int Position { get; set; }
    }
    
    public class BulkUpdateProductPositionsDto
    {
        [Required]
        public List<UpdateProductPositionDto> Positions { get; set; } = new List<UpdateProductPositionDto>();
    }
}