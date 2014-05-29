namespace CsRopExample.Models
{
    public class EmailAddress
    {
        // private constructor to force use of static
        private EmailAddress()
        {
        }

        /// <summary>
        /// Create a new email address from a string. If not valid, return null
        /// </summary>
        public static EmailAddress Create(string email)
        {
            if (string.IsNullOrEmpty(email)) { return null; }
            if (!email.Contains("@")) { return null; }
            if (email.Length > 50) return null;

            return new EmailAddress { Email = email };
        }

        /// <summary>
        /// Email property
        /// </summary>
        public string Email { get; private set; }
    }
}
