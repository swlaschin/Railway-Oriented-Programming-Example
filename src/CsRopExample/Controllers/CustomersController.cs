using System;
using System.Linq;
using System.Web.Http;
using CsRopExample.Database;
using CsRopExample.Models;

namespace CsRopExample.Controllers
{
    public class CustomersController : ApiController
    {
        readonly ICustomerRepository _repository;

        public CustomersController(ICustomerRepository repository)
        {
            _repository = repository;
        }

        [Route("customers/example")]
        [HttpGet]
        public IHttpActionResult GetExample()
        {
            var dto = new CustomerDto {FirstName = "Alice", LastName = "Adams", Email = "alice@example.com"};
            return this.Ok(dto);
        }

        [Route("customers/")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                var custs = _repository.GetAll();
                var dtos = custs.Select(DtoConverter.ToDto);
                return Ok(dtos);
            }
            catch (DataStoreException ex)
            {
                return this.InternalServerError(ex);
            }
        }


        [Route("customers/{customerId}")]
        [HttpGet]
        public IHttpActionResult Get(int customerId)
        {
            try
            {
                var cust = _repository.GetById(customerId);
                var dto = DtoConverter.ToDto(cust);
                return Ok(dto);
            }
            catch (DataStoreException ex)
            {
                return this.InternalServerError(ex);
            }
        }


        [Route("customers/{customerId}")]
        [HttpPost]
        public IHttpActionResult Post(int customerId, [FromBody] CustomerDto dto)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            var cust = DtoConverter.FromDto(dto);
            if (cust == null)
            {
                return this.BadRequest("Customer could not be created from DTO");
            }

            try
            {
                _repository.Add(customerId, cust);
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }

            return this.Ok();
        }

    }
}
