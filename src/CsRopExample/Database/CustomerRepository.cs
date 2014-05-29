using System.Collections.Generic;
using CsRopExample.Models;

namespace CsRopExample.Database
{
    class CustomerRepository : ICustomerRepository
    {
        readonly Dictionary<int,Customer> _data = new Dictionary<int, Customer>();

        public IEnumerable<Customer> GetAll()
        {
            return _data.Values;
        }

        public Customer GetById(int id)
        {
            if (_data.ContainsKey(id))
            {
                return _data[id];
            }

            throw new DataStoreException("Customer not found");
        }

        public void Add(int id, Customer customer)
        {
            _data[id] = customer;
        }

    }
}
