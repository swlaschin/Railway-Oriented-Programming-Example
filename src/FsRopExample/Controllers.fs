namespace FsRopExample.Controllers

open System
open Owin 
open Microsoft.Owin
open System.Web.Http 
open System.Web.Http.Dispatcher
open System.Net
open System.Net.Http.Formatting
open System.Web.Http.Results

open FsRopExample
open FsRopExample.Rop
open FsRopExample.Dtos
open FsRopExample.DomainModel


/// A set of helper functions for the CustomersController
module CustomersControllerHelper = 

    /// This type represents a reduced set of choices.
    /// Each DomainMessage is classified into one of these groups
    /// and converted into strings if appropriate.
    /// They are ordered from most important to least, so that 
    /// a NotFound beats a BadRequest, 
    /// a BadRequest beats a InternalServerError, etc. 
    type ResponseMessage = 
        | NotFound
        | BadRequest of string 
        | InternalServerError of string 
        | DomainEvent of DomainMessage 

    // classify a domain message into one of the standard responses
    let classify msg = 
        match msg with 
        | CustomerIsRequired
        | CustomerIdMustBePositive
        | FirstNameIsRequired
        | FirstNameIsTooLong
        | LastNameIsRequired
        | LastNameIsTooLong
        | EmailIsRequired
        | EmailIsNotAValidFormat
        | EmailIsTooLong -> 
            BadRequest (sprintf "%A" msg)

        // Events
        | EmailAddressChanged _ ->
            DomainEvent msg

        // Exposed errors
        | CustomerNotFound -> 
            NotFound 

        // Internal errors
        | SqlCustomerIsInvalid
        | DatabaseTimeout
        | DatabaseError _ ->
            InternalServerError (sprintf "%A" msg)

    // return the most important response
    // in the list of messages
    let primaryResponse msgs = 
        msgs 
        |> List.map classify
        |> List.sort 
        |> List.head  // we can assume at least one        

    // return all the BadRequests in the list of messages as string
    let badRequestsToStr msgs = 
        msgs 
        |> List.map classify
        |> List.choose (function BadRequest s -> Some s |_ ->  None)
        |> List.map (sprintf "ValidationError: %s; ")
        |> List.reduce (+)

    // return all the DomainEvents in the list of messages as string
    let domainEventsToStr msgs = 
        msgs 
        |> List.map classify
        |> List.choose (function DomainEvent s -> Some s |_ ->  None)
        |> List.map (sprintf "DomainEvent: %A; ")
        |> List.reduce (+)

    // return an overall IHttpActionResult for a set of messages
    let toHttpResult (controller:ApiController) msgs :IHttpActionResult = 
        match primaryResponse msgs with
        | NotFound -> 
            upcast NotFoundResult(controller) 
        | BadRequest _ -> 
            // find all events and accumulate them
            let validationMsg = badRequestsToStr msgs
            upcast NegotiatedContentResult(HttpStatusCode.BadRequest,validationMsg,controller) 
        | InternalServerError msg -> 
            upcast NegotiatedContentResult(HttpStatusCode.InternalServerError, msg,controller) 
        | DomainEvent _ -> 
            // find all events and accumulate them
            let eventsMsg = domainEventsToStr msgs
            upcast NegotiatedContentResult(HttpStatusCode.OK, eventsMsg,controller) 

/// This is an example of a simple Controller for managing customers
///
/// There are two primary actions
/// * GET retrieves a customer 
/// * POST inserts or updates a customer
/// 
/// For each action there are TWO versions
/// * one with no error handling
/// * one with better error handling
/// 
/// As you can see, adding error handling in F# does NOT make the code more complex.
/// 
/// Compare this code with the C# equivalent to see the traditional approach to handling errors.
type CustomersController (csDao:CsRopExample.DataAccessLayer.ICustomerDao, fsDao:DataAccessLayer.ICustomerDao) as this =
    // We inject both the C# and F# DAO objects into the object via the constructor
    inherit ApiController()

    // =================================
    // Helper code
    // 
    // Note: In F#, private code comes ABOVE public code, 
    // so look below for the controller interface
    // =================================

    // create local copies of some C# functions
    let csGetById = csDao.GetById
    let csCreateCustomerId = CsRopExample.DomainModels.CustomerId.Create
    let csCustomerToDto = CsRopExample.Dtos.DtoConverter.CustomerToDto
    let csDtoToCustomer = CsRopExample.Dtos.DtoConverter.DtoToCustomer

    // create two track versions of some F# functions
    let getByIdR = bindR fsDao.GetById
    let customerToDtoR = mapR DtoConverter.customerToDto
    let dtoToCustomerR = bindR DtoConverter.dtoToCustomer
    let upsertCustomerR = bindR fsDao.Upsert 
    let createCustomerIdR = bindR createCustomerId

    // =================================
    // IHttpActionResults 
    // =================================
    // return OK
    let ok content = 
        if content = box () then
            // avoid converting unit to null!
            OkResult(this) :> IHttpActionResult 
        else
            NegotiatedContentResult(HttpStatusCode.OK, content,this) :> IHttpActionResult 

    // create a two track version of Ok
    let okR result = mapR ok result

    let toHttpResult result = 
        result 
        |> valueOrDefault (CustomersControllerHelper.toHttpResult this)

    // =================================
    // logging
    // =================================

    let log format (objs:obj[]) = 
        System.Diagnostics.Debug.WriteLine("[LOG]" + format,objs)

    // log values on the success path
    let logSuccessR format result = 
        // helper to log the value
        let logSuccess obj = log format [|obj|]

        result 
        |> successTee logSuccess 

    // log errors on the failure path    
    let logFailureR result = 
        // helper to log one error
        let logError err = log "Error: {0}" [| sprintf "%A" err |]
        
        result 
        |> failureTee (Seq.iter logError) // loop through all errors

    //==============================================
    // event handlers 
    //
    // In the F# version, the events are added to the result,
    // which means they can be processed or not later in the 
    // data flow. As a result, there a no globals used and all
    // events are tracable.
    //
    // Compare this with the C# version which uses a global DomainEvents
    // and the event handler is decoupled.
    //==============================================

    let notifyCustomerWhenEmailChangedR = 
        // a helper function to filter events
        let detectEvent = function 
            | EmailAddressChanged (oldEmail,newEmail) -> Some (oldEmail,newEmail) 
            | _ -> None

        // this function does the notification
        // a real version would put a message on a queue, for example
        let notifyCustomer (oldEmail,newEmail) =
            log "Email changed from {0} to {1}" [|oldEmail;newEmail|]

        successTee (fun (_,msgs) ->
            msgs
            |> List.choose detectEvent 
            |> List.iter notifyCustomer
            )

    //==============================================
    // Get a customer, with and without error handling
    //==============================================


    /// Get one customer, without error handling
    [<Route("customers/{customerId}")>]
    [<HttpGet>]
    member this.Get(customerId:int) : IHttpActionResult =
        customerId
        |> csCreateCustomerId 
        |> csGetById 
        |> csCustomerToDto 
        |> ok

    /// Get one customer, with error handling
    ///
    /// Extra features added:
    /// * logging
    /// * validate the id
    /// * handle customer not found
    /// * trap exceptions coming from the database
    [<Route("customersE/{customerId}")>]
    [<HttpGet>]
    member this.GetWithErrorHandling(customerId:int) : IHttpActionResult =
        succeed customerId
        |> logSuccessR "GetWithErrorHandling {0}" 
        |> createCustomerIdR 
        |> getByIdR 
        |> customerToDtoR
        |> logFailureR
        |> okR
        |> toHttpResult


    //==============================================
    // Post a customer, with and without error handling
    //==============================================

    /// <summary>
    /// Upsert a customer, without error handling
    /// </summary>
    [<Route("customers/{customerId}")>]
    [<HttpPost>]
    member this.Post(customerId:int, [<FromBody>] dto:CsRopExample.Dtos.CustomerDto) :IHttpActionResult  =
        dto.Id <- customerId
        let cust = csDtoToCustomer dto
        csDao.Upsert(cust)
        ok()
       
    /// <summary>
    /// Upsert a customer, with error handling
    /// </summary>
    /// <remarks>
    /// Extra features added:
    /// * logging
    /// * validate the id
    /// * handle case when domain Customer could not be created from the DTO
    /// * handle the EmailAddressChanged event
    /// * trap exceptions coming from the database
    /// </remarks>
    [<Route("customersE/{customerId}")>]
    [<HttpPost>]
    member this.PostWithErrorHandling(customerId:int, [<FromBody>] dto:CustomerDto) :IHttpActionResult  =
        
        dto.Id <- customerId

        succeed dto
        |> logSuccessR "POST with {0}"
        |> dtoToCustomerR
        |> upsertCustomerR
        |> logFailureR
        |> notifyCustomerWhenEmailChangedR
        |> okR
        |> toHttpResult

    // =========================================
    // Debugging and helpers
    // =========================================

    /// Return an example DTO to model a POST on
    [<Route("example")>]
    [<HttpGet>]
    member this.GetExample() : IHttpActionResult = 
        let dto = new CustomerDto()
        dto.FirstName <- "Alice"
        dto.LastName <- "Adams"
        dto.Email <- "alice@example.com"
        ok dto

    /// Return all customers in the database
    [<Route("customers/")>]
    [<HttpGet>]
    member this.GetAll() : IHttpActionResult = 
        fsDao.GetAll()
        |> mapR (Seq.map DtoConverter.customerToDto)
        |> okR
        |> toHttpResult
