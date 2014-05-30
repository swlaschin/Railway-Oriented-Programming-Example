using System;
using System.Collections.Generic;
using System.Linq;
using CsRopExample.DomainModels;

namespace CsRopExample.Database
{
    /// <summary>
    /// This is a repository wrapper around a SQL database
    /// </summary>
    class CustomerRepository : ICustomerRepository
    {

        /// <summary>
        /// Return all customers
        /// </summary>
        public IEnumerable<Customer> GetAll()
        {
            var db = new SqlDatabase();
            return db.SelectAll().Select(DbCustomer.FromDbCustomer);
        }

        /// <summary>
        /// Return the customer with the given CustomerId, or null if not found
        /// </summary>
        public Customer GetById(CustomerId id)
        {
            var db = new SqlDatabase();
            return db.SelectAll().Where(c => c.Id == id.Id).Select(DbCustomer.FromDbCustomer).FirstOrDefault();
        }

        /// <summary>
        /// Add the customer to the collection. If it already exists, update it
        /// </summary>
        public void Add(Customer customer)
        {
            if (customer == null) { throw new ArgumentNullException("customer"); }

            var db = new SqlDatabase();
            var existingCust = GetById(customer.Id);
            var dbCust = DbCustomer.ToDbCustomer(customer);
            if (existingCust == null)
            {
                db.Insert(dbCust);
            }
            else
            {
                if (!customer.EmailAddress.Equals(existingCust.EmailAddress))
                {
                    // Note that this code is buried deep in a class and is not obvious
                    DomainEvents.OnEmailAddressChanged(existingCust.EmailAddress, customer.EmailAddress);
                }

                db.Update(dbCust);
            }
        }

    }
}
