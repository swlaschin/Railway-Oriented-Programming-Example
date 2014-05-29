using System.ComponentModel.DataAnnotations;

namespace CsRopExample.Models
{
    public class CustomerDto
    {
        [Required]
        [StringLength(10)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(10)]
        public string LastName { get; set; }

        [RegularExpression(@".*@.*")]
        [Required]
        [StringLength(20)]
        public string Email { get; set; }
    }
}
