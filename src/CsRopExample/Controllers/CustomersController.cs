using System;
using System.Linq;
using System.Web.Http;
using CsRopExample.Database;
using CsRopExample.DomainModels;
using CsRopExample.Dtos;

namespace CsRopExample.Controllers
{
    public class CustomersController : ApiController
    {
        readonly ICustomerRepository _repository;

        public CustomersController(ICustomerRepository repository)
        {
            _repository = repository;
        }

        //==============================================
        // Get a customer, with and without error handling
        //==============================================

        /// <summary>
        /// Get one customer, without error handling
        /// </summary>
        [Route("customers/{id}")]
        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            var custId = CustomerId.Create(id);
            var cust = _repository.GetById(custId);
            var dto = DtoConverter.ToDto(cust);
            return Ok(dto);
        }


        /// <summary>
        /// Get one customer, with error handling
        /// Yes this is deli
        /// </summary>
        [Route("customersE/{id}")]
        [HttpGet]
        public IHttpActionResult GetWithErrorHandling(int id)
        {
            Log("GetWithErrorHandling {0}", id);

            // first create the customer id
            // it might be null, so handle that case
            var custId = CustomerId.Create(id);
            if (custId == null)
            {
                Log("CustomerId is not valid");
                return BadRequest("CustomerId is not valid");
            }

            try
            {
                // look up the customer in the database
                // it might be null, so handle that case
                var cust = _repository.GetById(custId);
                if (cust == null)
                {
                    Log("Customer not found");
                    return BadRequest("Customer not found");
                }

                // this should always succeed
                var dto = DtoConverter.ToDto(cust);

                // return
                return Ok(dto);
            }
            catch (DataStoreException ex)
            {
                // handle database errors
                Log("Exception: {0}", ex.Message);
                return this.InternalServerError(ex);
            }
        }

        //==============================================
        // Post a customer, with and without error handling
        //==============================================

        [Route("customers/{customerId}")]
        [HttpPost]
        public IHttpActionResult Post(int customerId, [FromBody] CustomerDto dto)
        {
            dto.Id = customerId;
            var cust = DtoConverter.FromDto(dto);
            _repository.Add(cust);
            return this.Ok();
        }

        [Route("customersE/{customerId}")]
        [HttpPost]
        public IHttpActionResult PostWithErrorHandling(int customerId, [FromBody] CustomerDto dto)
        {
            Log("POST with {0}", customerId);

            // handle validation errors at DTO level
            if (!this.ModelState.IsValid)
            {
                Log("Model State invalid") ;
                return this.BadRequest(this.ModelState);
            }

            // handle customer creation errors 
            dto.Id = customerId;
            var cust = DtoConverter.FromDto(dto);
            if (cust == null)
            {
                Log("Customer could not be created from DTO");
                return this.BadRequest("Customer could not be created from DTO");
            }

            try
            {
                _repository.Add(cust);
                return this.Ok();
            }
            catch (Exception ex)
            {
                // handle database errors
                Log("Exception: {0}", ex.Message);
                return this.InternalServerError(ex);
            }

            
        }


        // =========================================
        // Debugging and helpers
        // =========================================

        /// <summary>
        /// Return an example DTO to model a POST on
        /// </summary>
        [Route("example")]
        [HttpGet]
        public IHttpActionResult GetExample()
        {
            var dto = new CustomerDto { FirstName = "Alice", LastName = "Adams", Email = "alice@example.com" };
            return this.Ok(dto);
        }

        /// <summary>
        /// Return all customers in the database
        /// </summary>
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


        private void Log(string format, params object[] objs)
        {
            Console.WriteLine(format, objs);
        }
    }
}
