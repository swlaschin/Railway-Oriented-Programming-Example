using CsRopExample.DomainModels;

namespace CsRopExample.SqlDatabase
{
    /// <summary>
    /// Represents a customer in a SQL database
    /// </summary>
    public class DbCustomer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
