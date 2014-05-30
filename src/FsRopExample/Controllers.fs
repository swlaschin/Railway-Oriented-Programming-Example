namespace Controllers

open System
open Owin 
open Microsoft.Owin
open System.Web.Http 
open System.Web.Http.Dispatcher
open System.Net.Http.Formatting

open Rop
open DomainModel
open DataStore

type CustomerController(repository:ICustomerRepository) =
    inherit ApiController()

    // wrap the repo in a safe handler
    let safeGetAll() =
        try
            repository.GetAll() |> succeed
        with
        | DataStoreException msg ->
            fail (DatabaseError msg)

    // wrap the repo in a safe handler
    let safeGetId id =
        try
            repository.GetById id |> succeed
        with
        | DataStoreException msg ->
            fail (DatabaseError msg)


    [<Route("customers/example")>]
    [<HttpGet>]
    member this.GetExample() : IHttpActionResult = 
        let dto = {CustomerDto.FirstName = "Alice"; LastName = "Adams"; Email = "alice@example.com"}
        this.Ok(dto) |> upcast

    [<Route("customers/")>]
    [<HttpGet>]
    member this.Get() : IHttpActionResult = 
    {
        safeGetAll
        >> map (fun cust )
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

    member this.Get()  =
        ["value1";"value2"]

    // GET api/values/5 
    member this.Get id = 
        sprintf "id is %i" id 

    // POST api/values 
    member this.Post ([<FromBody>]value:string) = 
        ()

    // PUT api/values/5 
    member this.Put(id:int, [<FromBody>]value:string) =
        ()

