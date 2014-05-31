namespace CsRopExample.DomainModels
{
    /// <summary>
    /// Represents a PersonalName in the domain. 
    /// </summary>
    public class PersonalName
    {
        // private constructor to force use of static
        private PersonalName()
        {
        }

        /// <summary>
        /// Create a new name from a first and last strings. If null or too long, return null
        /// </summary>
        public static PersonalName Create(string first, string last)
        {
            // Do validation. Note that validation occurs both here and in the DTO, 
            // and so lengths, etc, must be synchronized.

            // Compare this with the F# version, where the domain object
            // contains its own validation as part of its definition,
            // and there is no validation on the DTO itself.

            if (string.IsNullOrEmpty(first)) { return null; }
            if (first.Length > 10) return null;  // make them short for testing!

            if (string.IsNullOrEmpty(last)) { return null; }
            if (last.Length > 10) return null;


            return new PersonalName { First = first, Last = last };

        }

        /// <summary>
        /// First name property
        /// </summary>
        public string First { get; private set; }

        /// <summary>
        /// Last name property
        /// </summary>
        public string Last { get; private set; }
    }
}
