using System;
using CsRopExample.DomainModels;

namespace CsRopExample.Dtos
{
    public static class DtoConverter
    {

        /// <summary>
        /// Create a domain customer from a DTO or null if not valid.
        /// </summary>
        public static Customer DtoToCustomer(CustomerDto dto)
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
        /// Create a DTO from a domain customer or null if the customer is null
        /// </summary>
        public static CustomerDto CustomerToDto(Customer customer)
        {
            // we should never try to convert a null customer
            if (customer == null)
            {
                return null;
            }

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
