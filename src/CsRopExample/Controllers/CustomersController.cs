using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using CsRopExample.DataAccessLayer;
using CsRopExample.DomainModels;
using CsRopExample.Dtos;

namespace CsRopExample.Controllers
{
    /// <summary>
    /// This is an example of a simple Controller for managing customers
    /// </summary>
    /// <remarks>
    /// There are two primary actions
    /// * GET retrieves a customer 
    /// * POST inserts or updates a customer
    /// 
    /// For each action there are TWO versions
    /// * one with no error handling
    /// * one with better error handling
    /// 
    /// As you can see, adding error handling makes the code much more complex.
    /// I have deliberately put all the code in the controller, both to keep it together in one place,
    /// and to show what it looks like when it is not refactored into other classes.
    /// 
    /// Compare this code with the F# equivalent to see the railway-oriented approach to handling errors.
    /// </remarks>
    public class CustomersController : ApiController
    {

        readonly ICustomerDao _dao;

        /// <summary>
        /// We inject a DAO object into the object via the constructor
        /// </summary>
        public CustomersController(ICustomerDao dao)
        {
            _dao = dao;
        }

        //==============================================
        // Get a customer, with and without error handling
        //==============================================

        /// <summary>
        /// Get one customer, without error handling
        /// </summary>
        [Route("customers/{customerId}")]
        [HttpGet]
        public IHttpActionResult Get(int customerId)
        {
            var custId = CustomerId.Create(customerId);
            var cust = _dao.GetById(custId);
            var dto = DtoConverter.CustomerToDto(cust);
            return Ok(dto);
        }


        /// <summary>
        /// Get one customer, with error handling
        /// </summary>
        /// <remarks>
        /// Extra features added:
        /// * logging
        /// * validate the id
        /// * handle customer not found
        /// * trap exceptions coming from the database
        /// </remarks>
        [Route("customersE/{customerId}")]
        [HttpGet]
        public IHttpActionResult GetWithErrorHandling(int customerId)
        {
            Log("GetWithErrorHandling {0}", customerId);

            // first create the customer id
            // it might be null, so handle that case
            var custId = CustomerId.Create(customerId);
            if (custId == null)
            {
                Log("CustomerId is not valid");
                return BadRequest("CustomerId is not valid");
            }

            try
            {
                // look up the customer in the database
                // it might be null, so handle that case
                var cust = _dao.GetById(custId);
                if (cust == null)
                {
                    Log("Customer not found");
                    return NotFound();
                }

                // this should always succeed
                var dto = DtoConverter.CustomerToDto(cust);

                // return
                return Ok(dto);
            }
            catch (Exception ex)
            {
                // handle database errors
                Log("Exception: {0}", ex.Message);
                return this.InternalServerError(ex);
            }
        }

        //==============================================
        // Post a customer, with and without error handling
        //==============================================

        /// <summary>
        /// Upsert a customer, without error handling
        /// </summary>
        [Route("customers/{customerId}")]
        [HttpPost]
        public IHttpActionResult Post(int customerId, [FromBody] CustomerDto dto)
        {
            dto.Id = customerId;
            var cust = DtoConverter.DtoToCustomer(dto);
            _dao.Upsert(cust);
            return this.Ok();
        }

        /// <summary>
        /// Upsert a customer, with error handling
        /// </summary>
        /// <remarks>
        /// Extra features added:
        /// * logging
        /// * validate the id
        /// * validate the Dto
        /// * handle case when domain Customer could not be created from the DTO
        /// * handle the EmailAddressChanged event
        /// * trap exceptions coming from the database
        /// </remarks>
        [Route("customersE/{customerId}")]
        [HttpPost]
        public IHttpActionResult PostWithErrorHandling(int customerId, [FromBody] CustomerDto dto)
        {
            dto.Id = customerId;

            Log("POST with {0}", customerId);

            // handle validation errors at DTO level
            if (!this.ModelState.IsValid)
            {
                Log("Model State invalid");
                return this.BadRequest(this.ModelState);
            }

            // handle customer creation errors 
            var cust = DtoConverter.DtoToCustomer(dto);
            if (cust == null)
            {
                Log("Customer could not be created from DTO");
                return this.BadRequest("Customer could not be created from DTO");
            }

            // hook into the domain event
            DomainEvents.EmailAddressChanged += NotifyCustomerWhenEmailChanged;

            try
            {
                _dao.Upsert(cust);
                return this.Ok();
            }
            catch (Exception ex)
            {
                // handle database errors
                Log("Exception: {0}", ex.Message);
                return this.InternalServerError(ex);
            }
            finally
            {
                // unhook from the domain event no matter what happens
                DomainEvents.EmailAddressChanged -= NotifyCustomerWhenEmailChanged;
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
        public IHttpActionResult GetAll()
        {
            try
            {
                var dtos = _dao.GetAll().Select(DtoConverter.CustomerToDto);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Handler for the EmailAddressChanged event
        /// </summary>
        private void NotifyCustomerWhenEmailChanged(object sender, EmailAddressChangedEventArgs args)
        {
            // just log it for now.
            // a real version would put a message on a queue, for example
            Log("Email Address Changed from : {0} to {1}", args.OldAddress.Email, args.NewAddress.Email);
        }

        /// <summary>
        ///  A crude logger
        /// </summary>
        private static void Log(string format, params object[] objs)
        {
            Debug.WriteLine("[LOG]" + format, objs);
        }
    }
}
