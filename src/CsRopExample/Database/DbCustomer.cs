using CsRopExample.DomainModels;

namespace CsRopExample.Database
{
    /// <summary>
    /// Represents a customer in the database
    /// </summary>
    public class DbCustomer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        /// <summary>
        /// Convert a DbCustomer into a domain Customer
        /// </summary>
        public static Customer FromDbCustomer(DbCustomer dbCustomer)
        {
            if (dbCustomer == null)
            {
                return null;
            }

            var id = CustomerId.Create(dbCustomer.Id);
            var name = PersonalName.Create(dbCustomer.FirstName, dbCustomer.LastName);
            var email = EmailAddress.Create(dbCustomer.Email);
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
