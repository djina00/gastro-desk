using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using GastroDesk.Models.Enums;

namespace GastroDesk.Models
{
    public class Order : BaseEntity
    {
        [Required]
        public int TableNumber { get; set; }

        [Required]
        public DateTime OrderDateTime { get; set; } = DateTime.Now;

        public OrderStatus Status { get; set; } = OrderStatus.Active;

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [JsonIgnore]
        [XmlIgnore]
        public virtual User? User { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        [NotMapped]
        public decimal TotalPrice => Items?.Sum(s => s.TotalPrice) ?? 0;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
