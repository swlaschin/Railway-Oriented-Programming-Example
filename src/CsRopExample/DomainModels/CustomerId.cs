namespace CsRopExample.DomainModels
{
    /// <summary>
    /// Represents a CustomerId in the domain. 
    /// A special class is used to avoid primitive obsession and to ensure valid data
    /// </summary>
    public class CustomerId
    {
        // private constructor to force use of static
        private CustomerId()
        {
        }

        /// <summary>
        /// Create a new CustomerId from an int. If not valid, return null
        /// </summary>
        public static CustomerId Create(int id)
        {
            if (id < 1) return null;

            return new CustomerId { Id= id};
        }

        /// <summary>
        /// Id property
        /// </summary>
        public int Id { get; private set; }
    }
}
