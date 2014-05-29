using System.Collections.Generic;
using CsRopExample.Models;

namespace CsRopExample.Database
{
    public interface ICustomerRepository
    {
        IEnumerable<Customer> GetAll();
        Customer GetById(int id);
        void Add(int id, Customer customer);
    }
}