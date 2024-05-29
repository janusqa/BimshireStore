using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BimshireStore.Services.ShoppingCartAPI.Models.Dto;

namespace BimshireStore.Services.ShoppingCartAPI.Models
{
    public class CartDetail
    {
        [Key]
        public int CartDetailId { get; set; }
        public int CartHeaderId { get; set; }
        [ForeignKey("CartHeaderId")]
        [NotMapped]
        public CartHeader? CartHeader { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [NotMapped]
        public ProductDto? Product { get; set; }
        public int Count { get; set; }
    }
}