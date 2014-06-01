using System.Collections.Generic;
using CsRopExample.DomainModels;

namespace CsRopExample.DataAccessLayer
{
    /// <summary>
    /// This is a data access wrapper around a SQL database
    /// </summary>
    public interface ICustomerDao
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
        /// Insert/update the customer 
        /// If it already exists, update it, otherwise insert it.
        /// If the email address has changed, raise a EmailAddressChanged event on DomainEvents
        /// </summary>
        void Upsert(Customer customer);
    }
}