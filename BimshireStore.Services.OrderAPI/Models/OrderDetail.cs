using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BimshireStore.Services.OrderAPI.Models.Dto;

namespace BimshireStore.Services.OrderAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }
        public int OrderHeaderId { get; set; }
        [ForeignKey("OrderHeaderId")]
        [NotMapped]
        public OrderDetail? OrderHeader { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [NotMapped]
        public ProductDto? Product { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Count { get; set; }
    }
}