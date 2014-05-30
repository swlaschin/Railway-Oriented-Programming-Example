namespace CsRopExample.DomainModels
{
    /// <summary>
    /// Represents a Customer in the domain. 
    /// </summary>
    public class Customer
    {
        // private constructor to force use of static
        private Customer()
        {
        }

        /// <summary>
        /// Create a new customer from the parameters. If not valid, return null
        /// </summary>
        public static Customer Create(CustomerId id, PersonalName name, EmailAddress email)
        {
            if (id == null) { return null; }
            if (name == null) { return null; }
            if (email == null) { return null; }

            return new Customer { Id = id, Name = name, EmailAddress = email };
        }


        public CustomerId Id { get; private set; }
        public PersonalName Name { get; private set; }
        public EmailAddress EmailAddress { get; private set; }
    }
}
