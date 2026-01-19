using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace GastroDesk.Models
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        public int DishId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        [ForeignKey(nameof(OrderId))]
        [JsonIgnore]
        [XmlIgnore]
        public virtual Order? Order { get; set; }

        [ForeignKey(nameof(DishId))]
        public virtual Dish? Dish { get; set; }

        [NotMapped]
        public decimal TotalPrice => Price * Quantity;
    }
}
