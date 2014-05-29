namespace CsRopExample.Models
{
    public class Customer
    {
        // private constructor to force use of static
        private Customer()
        {
        }

        /// <summary>
        /// Create a new email address from a string. If not valid, return null
        /// </summary>
        public static Customer Create(PersonalName name, EmailAddress email)
        {
            if (name == null) { return null; }
            if (email == null) { return null; }

            return new Customer { Name = name, EmailAddress = email };
        }

        public PersonalName Name { get; private set; }
        public EmailAddress EmailAddress { get; private set; }
    }
}
