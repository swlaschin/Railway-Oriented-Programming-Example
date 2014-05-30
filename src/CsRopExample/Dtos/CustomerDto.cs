using System.ComponentModel.DataAnnotations;

namespace CsRopExample.Dtos
{
    /// <summary>
    /// A Data Transfer Object to serialize/deserialize Customers 
    /// </summary>
    public class CustomerDto
    {
        /// <summary>
        /// Id is not required as it is often set via the URL parameter
        /// </summary>
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(10)]
        public string LastName { get; set; }

        [RegularExpression(".*@.*")] // crude validation
        [Required]
        [StringLength(20)]
        public string Email { get; set; }
    }
}
