using System;
using CsRopExample.DomainModels;

namespace CsRopExample.Dtos
{
    static class DtoConverter
    {

        /// <summary>
        /// Create a domain customer from a DTO or null if not valid.
        /// </summary>
        public static Customer FromDto(CustomerDto dto)
        {
            if (dto == null)
            {
                // dto can be null if deserialization fails
                return null;
            }

            var id = CustomerId.Create(dto.Id);
            var name = PersonalName.Create(dto.FirstName, dto.LastName);
            var email = EmailAddress.Create(dto.Email);
            var cust = Customer.Create(id, name, email);
            return cust;
        }


        /// <summary>
        /// Create a DTO from a domain customer 
        /// </summary>
        public static CustomerDto ToDto(Customer customer)
        {
            // we should never try to convert a null customer
            if (customer == null) { throw new ArgumentNullException("customer"); }

            return new CustomerDto
            {
                Id = customer.Id.Id,
                FirstName = customer.Name.First,
                LastName = customer.Name.Last,
                Email = customer.EmailAddress.Email
            };
        }

    }
}
