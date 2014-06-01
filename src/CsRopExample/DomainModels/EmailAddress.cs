using System;

namespace CsRopExample.DomainModels
{
    /// <summary>
    /// Represents a EmailAddress in the domain. 
    /// A special class is used to avoid primitive obsession and to ensure valid data
    /// </summary>
    /// <remarks>
    /// I have just used a private ctor and a static method to create an instance.
    /// 
    /// You could also provide implicit or explicit operators as documented here:
    /// http://lostechies.com/jimmybogard/2007/12/03/dealing-with-primitive-obsession/
    /// </remarks>
    public class EmailAddress : IEquatable<EmailAddress>
    {
        // private constructor to force use of static
        private EmailAddress()
        {
        }

        /// <summary>
        /// Create a new EmailAddress from a string. If not valid, return null
        /// </summary>
        public static EmailAddress Create(string email)
        {
            // Do validation. Note that validation occurs both here and in the DTO, 
            // and so regex, length, etc, must be synchronized.
            //
            // Compare this with the F# version, where the domain object
            // contains its own validation as part of its definition,
            // and there is no validation on the DTO itself.
            if (string.IsNullOrEmpty(email)) { return null; }
            if (!email.Contains("@")) { return null; }
            if (email.Length > 20) return null;  // make short for testing!

            return new EmailAddress { Email = email };
        }

        /// <summary>
        /// Email property
        /// </summary>
        public string Email { get; private set; }

        public override string ToString()
        {
            return string.Format("EmailAddress {0}", this.Email);
        }

        // =====================================
        // code for equality
        // =====================================

        public override int GetHashCode()
        {
            return this.Email.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as EmailAddress;
            return this.Equals(other);
        }

        public bool Equals(EmailAddress other)
        {
            if (other == null)
            {
                return false;
            }
            return this.Email.Equals(other.Email);
        }

    }
}
