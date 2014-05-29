using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsRopExample.Models
{
    static class DtoConverter
    {

        /// <summary>
        /// Create a customer object from a DTO or null if not valid.
        /// </summary>
        public static Customer FromDto(CustomerDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            var name = PersonalName.Create(dto.FirstName, dto.LastName);
            var email = EmailAddress.Create(dto.Email);
            var cust = Customer.Create(name, email);
            return cust;
        }


        public static CustomerDto ToDto(Customer customer)
        {
            return new CustomerDto
            {
                FirstName = customer.Name.First,
                LastName = customer.Name.Last,
                Email = customer.EmailAddress.Email
            };
        }

    }
}
