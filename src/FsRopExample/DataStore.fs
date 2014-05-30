module DataStore

open Rop
open DomainModel
open System.Collections.Generic

exception DataStoreException of string

type ICustomerRepository =
    abstract GetAll : unit -> Customer seq
    abstract GetById : int -> Customer
    abstract Add : int * Customer -> unit

type CustomerRepository() =

    let _data = new Dictionary<int, Customer>()

    interface ICustomerRepository with
        member this.GetAll() = 
            _data.Values |> Seq.cast
       
        member this.GetById id = 
            let found,cust = _data.TryGetValue(id)
            if found then 
                cust
            else 
                raise (DataStoreException "Customer not found")

        member this.Add (id,cust) = 
            _data.Add(id,cust)
    
    