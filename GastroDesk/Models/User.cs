using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using GastroDesk.Models.Enums;

namespace GastroDesk.Models
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Waiter;

        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        [XmlIgnore]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
