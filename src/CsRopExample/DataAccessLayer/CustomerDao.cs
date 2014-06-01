using System;
using System.Collections.Generic;
using System.Linq;
using CsRopExample.DomainModels;
using CsRopExample.SqlDatabase;

namespace CsRopExample.DataAccessLayer
{
    /// <summary>
    /// This is a data access wrapper around a SQL database
    /// </summary>
    public class CustomerDao : ICustomerDao
    {
        /// <summary>
        /// Return all customers
        /// </summary>
        public IEnumerable<Customer> GetAll()
        {
            var db = new DbContext();
            return db.Customers().Select(FromDbCustomer);
        }

        /// <summary>
        /// Return the customer with the given CustomerId, or null if not found
        /// </summary>
        public Customer GetById(CustomerId id)
        {
            var db = new DbContext();
            
            // Note that this code does not trap exceptions coming from the database. What would it do with them?
            // Compare with the F# version, where errors are returned along with the Customer
            return db.Customers().Where(c => c.Id == id.Id).Select(FromDbCustomer).FirstOrDefault();
        }

        /// <summary>
        /// Insert/update the customer 
        /// If it already exists, update it, otherwise insert it.
        /// If the email address has changed, raise a EmailAddressChanged event on DomainEvents
        /// </summary>
        public void Upsert(Customer customer)
        {
            if (customer == null) { throw new ArgumentNullException("customer"); }

            var db = new DbContext();
            var existingDbCust = GetById(customer.Id);
            var newDbCust = ToDbCustomer(customer);
            if (existingDbCust == null)
            {
                // insert
                db.Insert(newDbCust);

                // Note that this code does not trap exceptions coming from the database. What would it do with them?
                // Compare with the F# version, where errors are alway returned from the call 
            }
            else
            {
                // update
                db.Update(newDbCust);

                // check for changed email
                if (!customer.EmailAddress.Equals(existingDbCust.EmailAddress))
                {
                    // Generate a event
                    // Note that this code is buried deep in a class and is not obvious
                    // It is also hard to turn on and off (eg for batch updates) without adding extra complications
                    //
                    // Compare this with the F# version, when an event is returned from the call itself.
                    DomainEvents.OnEmailAddressChanged(existingDbCust.EmailAddress, customer.EmailAddress);
                }

            }
        }

        /// <summary>
        /// Convert a DbCustomer into a domain Customer
        /// </summary>
        public static Customer FromDbCustomer(DbCustomer sqlCustomer)
        {
            if (sqlCustomer == null)
            {
                return null;
            }

            var id = CustomerId.Create(sqlCustomer.Id);
            var name = PersonalName.Create(sqlCustomer.FirstName, sqlCustomer.LastName);
            var email = EmailAddress.Create(sqlCustomer.Email);
            var cust = Customer.Create(id, name, email);
            return cust;
        }


        /// <summary>
        /// Convert a domain Customer into a DbCustomer
        /// </summary>
        public static DbCustomer ToDbCustomer(Customer customer)
        {
            return new DbCustomer
            {
                Id = customer.Id.Id,
                FirstName = customer.Name.First,
                LastName = customer.Name.Last,
                Email = customer.EmailAddress.Email
            };
        }
    }
}
