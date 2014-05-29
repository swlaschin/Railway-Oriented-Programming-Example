namespace Controllers

open System
open Owin 
open Microsoft.Owin
open System.Web.Http 
open System.Web.Http.Dispatcher
open System.Net.Http.Formatting

type CustomerController() =
    inherit ApiController()

    // GET api/values 
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

