using System.Collections.Generic;

namespace CsRopExample.SqlDatabase
{
    /// <summary>
    /// This class represents a (in-memory) SQL database
    /// </summary>
    class DbContext
    {
        // in-memory collection
        static readonly Dictionary<int, DbCustomer> Data = new Dictionary<int, DbCustomer>();

        
        public IEnumerable<DbCustomer> Customers()
        {
            return Data.Values;
        }

        public void Update(DbCustomer customer)
        {
            if (!Data.ContainsKey(customer.Id))
            {
                // Emulate a SQL error
                throw new SqlException("KeyNotFound");
            }

            // use the customer id to trigger some special cases
            switch (customer.Id)
            {
                case 42:
                    // Emulate a SQL error
                    throw new SqlException("Timeout");
                default:
                    Data[customer.Id] = customer;
                    break;
            }
        }

        public void Insert(DbCustomer customer)
        {
            if (Data.ContainsKey(customer.Id))
            {
                // Emulate a SQL error
                throw new SqlException("DuplicateKey");
            }

            // use the customer id to trigger some special cases
            switch (customer.Id)
            {
                case 42:
                    // Emulate a SQL error
                    throw new SqlException("Timeout");
                default:
                    Data[customer.Id] = customer;
                    break;
            }

        }

    }
}
