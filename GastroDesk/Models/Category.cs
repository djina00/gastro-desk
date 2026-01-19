using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace GastroDesk.Models
{
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public virtual ICollection<Dish> Dishes { get; set; } = new List<Dish>();
    }
}
