using System.Collections.Generic;

namespace CsRopExample.Database
{
    /// <summary>
    /// This class represents a (in-memory) SQL database
    /// </summary>
    class SqlDatabase
    {
        readonly Dictionary<int, DbCustomer> _data = new Dictionary<int, DbCustomer>();

        public IEnumerable<DbCustomer> SelectAll()
        {
            return _data.Values;
        }

        public void Update(DbCustomer customer)
        {
            if (!_data.ContainsKey(customer.Id))
            {
                throw new SqlException("KeyDoesntExist");
            }
            _data[customer.Id] = customer;
        }

        public void Insert(DbCustomer customer)
        {
            if (_data.ContainsKey(customer.Id))
            {
                throw new SqlException("KeyExists");
            }
            _data[customer.Id] = customer;
        }

    }
}
