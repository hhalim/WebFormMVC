using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class User
    {
        public int? UserID { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        public string LastName { get; set; }
    }
}
