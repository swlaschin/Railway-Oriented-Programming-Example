namespace FsRopExample.SqlDatabase

open FsRopExample.Rop
open FsRopExample.DomainPrimitiveTypes
open FsRopExample.DomainModel
open System.Collections.Generic

/// Represents a customer in a SQL database
/// This is a regular POCO class which can be null. 
/// To emulate the C# class, all the properties are initialized to null by default
///
/// Note that in F# you have to make quite an effort to create nullable classes with nullable fields
[<AllowNullLiteralAttribute>]
type DbCustomer() = 
    member val Id = 0 with get, set
    member val FirstName : string = null with get, set
    member val LastName : string = null with get, set
    member val Email : string  = null with get, set


/// This class represents an exception
exception SqlException of string

/// This class represents a (in-memory) SQL database
/// It throws exceptions just like the C# version
type internal DbContext() = 

    static let _data = new Dictionary<int, DbCustomer>()

    member this.Customers() : DbCustomer seq = 
        upcast _data.Values 

    member this.Update (customer:DbCustomer) = 
        if _data.ContainsKey(customer.Id) |> not then
            raise (SqlException "KeyNotFound")
        else 
            // use the customer id to trigger some special cases
            match customer.Id with
            | 42 -> 
                raise (SqlException "Timeout")
            | _ -> 
                _data.[customer.Id] <- customer

    member this.Insert (customer:DbCustomer) =
        if _data.ContainsKey(customer.Id) then
            raise (SqlException "DuplicateKey")
        else 
            // use the customer id to trigger some special cases
            match customer.Id with
            | 42 -> 
                raise (SqlException "Timeout")
            | _ -> 
                _data.[customer.Id] <- customer
