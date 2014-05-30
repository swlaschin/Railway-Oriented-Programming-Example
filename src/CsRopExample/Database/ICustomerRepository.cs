using System.Collections.Generic;
using CsRopExample.DomainModels;

namespace CsRopExample.Database
{
    public interface ICustomerRepository
    {
        /// <summary>
        /// Return all customers
        /// </summary>
        IEnumerable<Customer> GetAll();

        /// <summary>
        /// Return the customer with the given CustomerId, or null if not found
        /// </summary>
        Customer GetById(CustomerId id);

        /// <summary>
        /// Add the customer to the collection. If it already exists, update it
        /// </summary>
        void Add(Customer customer);
    }
}