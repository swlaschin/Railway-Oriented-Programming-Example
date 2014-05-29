namespace CsRopExample.Models
{
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
            if (string.IsNullOrEmpty(first)) { return null; }
            if (first.Length > 50) return null;

            if (string.IsNullOrEmpty(last)) { return null; }
            if (last.Length > 50) return null;


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
