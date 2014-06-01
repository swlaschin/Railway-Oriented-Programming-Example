module FsRopExample.DataAccessLayer

open System
open FsRopExample.Rop
open FsRopExample.DomainPrimitiveTypes
open FsRopExample.DomainModel
open FsRopExample.SqlDatabase    


/// This is a data access wrapper around a SQL database
type ICustomerDao =

    /// Return all customers
    abstract GetAll : unit -> RopResult<Customer seq,DomainMessage>
    // Note that the F# version traps sql exceptions and will return a DatabaseError

    /// Return the customer with the given CustomerId, or CustomerNotFound error if not found
    abstract GetById : CustomerId.T -> RopResult<Customer,DomainMessage>
    // Note that the F# version allows errors and messages to be passed back.
    // Compare with the C# version, where null is used to indicate a missing customer

    /// Insert/update the customer 
    /// If it already exists, update it, otherwise insert it.
    /// If the email address has changed, add a EmailAddressChanged event to the result
    abstract Upsert : Customer -> RopResult<unit,DomainMessage>
    // Note that the F# version allows events to be passed back along with a value.
    // There is nothing to return, so we use "unit" (aka "void")
    // In this, we can add an "EmailAddressChanged" event to the result if needed.
    // Compare with the C# version, where we have to use a global DomainEvents to signal that something happened

/// Convert a dbCustomer into a domain Customer with possible errors.
/// We MUST handle the possibility of one or more errors
/// because the Customer type has stricter constraints than dbCustomer 
/// and the conversion might fail.
let fromDbCustomer (dbCustomer:DbCustomer) = 
    if dbCustomer = null then 
        fail SqlCustomerIsInvalid
    else
        // This is an example of the power of composition!
        // Each step returns a value OR an error.
        // These are then gradually combined to make bigger things, all the while preserving any errors 
        // that happen.

        // if the id is not valid, the createCustomerId function will return a Failure
        // hover over idOrError and you can see it has type RopResult<CustomerId,DomainEvent> rather than just CustomerId
        let idOrError = createCustomerId dbCustomer.Id

        // similarly for first and last name
        let firstNameOrError = createFirstName dbCustomer.FirstName
        let lastNameOrError = createLastName dbCustomer.LastName

        // the "createPersonalName" functions takes normal inputs, not inputs with errors, 
        // but we can use the "lift" function to convert it into one that does handle error input
        // the output has also changed from a normal name to one with errors
        let personalNameOrError = lift2R createPersonalName firstNameOrError lastNameOrError 

        // similarly try to make an email
        let emailOrError = createEmail dbCustomer.Email

        // finally add them all together to make a customers
        // the "createCustomer" takes three params, so use lift3 to convert it
        let customerOrError = lift3R createCustomer idOrError personalNameOrError emailOrError
        customerOrError 

// The code above is very explicit and was designed for beginners to understand.
// Below is a more idiomatic version which uses the <!> and <*> operators rather than "lift".
//
// The <!> and <*> operators make it look complicated, but in fact it is always the same pattern.
//  <!> is used for the first param
//  <*> is used for the subsequent params
//
// so for example:
//   existingFunction <!> firstParam <*> secondParam <*> thirdParam
let fromDbCustomerIdiomatic (dbCustomer:DbCustomer) = 
    if dbCustomer = null then 
        fail SqlCustomerIsInvalid
    else
        let nameOrError = 
            createPersonalName
            <!> createFirstName dbCustomer.FirstName
            <*> createLastName dbCustomer.LastName

        createCustomer 
        <!> createCustomerId dbCustomer.Id
        <*> nameOrError
        <*> createEmail dbCustomer.Email

/// Convert a domain Customer into a dbCustomer.
/// There is no possibility of an error 
/// because the Customer type has stricter constraints than dbCustomer.
let toDbCustomer(cust:Customer) =

    // extract the raw int id from the CustomerId wrapper
    let custIdInt = cust.Id |> CustomerId.apply id

    // create the object and set the properties
    let dbCustomer = DbCustomer()
    dbCustomer.Id <- custIdInt
    dbCustomer.FirstName <- cust.Name.FirstName |> String10.apply id
    dbCustomer.LastName <- cust.Name.LastName |> String10.apply id
    dbCustomer.Email <- cust.Email |> EmailAddress.apply id
    dbCustomer

/// A utility that matches SqlExceptions and turns them into something more useful,
/// a set of choices that that can be matched on later.
/// This uses the "active patterns" feature of F#.
let (|KeyNotFound|DuplicateKey|Timeout|Other|) (ex:SqlException) =
    match ex.Data0 with 
    | "KeyNotFound" -> KeyNotFound
    | "DuplicateKey" -> DuplicateKey
    | "Timeout" -> Timeout
    | _ -> Other

let failureFromException (ex:SqlException) =
    // This code handles exceptions coming from the database. 
    // It uses the utility function above to find out what kind of exception 
    // it is and and then return an appropriate error code
    match ex with
    | Timeout -> 
        fail DatabaseTimeout
    | _ -> 
        // treat all other errors the same
        fail (DatabaseError ex.Message)


/// An implementation of a ICustomerDao 
type CustomerDao() =

    interface ICustomerDao with

        member this.GetAll() = 
            let db = new DbContext()

            // set up helper functions for the Seq.choose below.
            let fSuccess (x,_) = Some x  // keep success
            let fFailure _ = None // discard failure

            try
                db.Customers() 
                |> Seq.map fromDbCustomer
                // At this point we have a sequence of rop results, one for each customer.
                // What should we do if some are failures? 
                // For now, we choose to ignore the failures and just return the successes
                |> Seq.choose (either fSuccess fFailure)
                |> succeed
            with
            | :? SqlException as ex -> failureFromException ex
                // This code (unlike the C# version) DOES trap exceptions coming from the database. 
                // It uses the utility function above to find out what kind of exception 
                // it is and and then return an appropriate error code

        member this.GetById customerId = 
            let db = new DbContext()

            // extract the raw int id from the CustomerId wrapper
            let custIdInt = customerId |> CustomerId.apply id    

            try
                db.Customers()
                |> Seq.tryFind (fun sql -> sql.Id = custIdInt)
                |> Option.map fromDbCustomer
                // At this point we might or might not have a customer
                // In the C# code, we just return null if missing.
                // In F#, we can do better and return an explicit error code "CustomerNotFound"
                |> failIfNoneR CustomerNotFound
            with
            | :? SqlException as ex -> failureFromException ex
                // This code (unlike the C# version) DOES trap exceptions coming from the database. 
                // It uses the utility function above to find out what kind of exception 
                // it is and and then return an appropriate error code

        member this.Upsert (customer:Customer) = 
            let db = new DbContext()

            try
                let newDbCust = toDbCustomer customer

                // extract the raw int id from the CustomerId wrapper
                let custIdInt = customer.Id |> CustomerId.apply id

                // Try to find an existing customer.
                // Return a optional DbCustomer            
                let existingDbCustOpt =
                    db.Customers()
                    |> Seq.tryFind (fun sql -> sql.Id = custIdInt)

                // do different actions depending on
                // whether the customer was found or not
                match existingDbCustOpt with
                | None ->
                    // insert
                    db.Insert(newDbCust)

                    // return a Success
                    succeed ()

                | Some existingDbCust -> 
                    // update
                    db.Update(newDbCust)

                    // check for changed email
                    if newDbCust.Email <> existingDbCust.Email then
                        // return a Success with the extra event
                        let event = EmailAddressChanged (existingDbCust.Email,newDbCust.Email)
                        succeedWithMsg () event 
                    else 
                        // return a Success with no extra event
                        succeed ()  

            with
            | :? SqlException as ex -> failureFromException ex
                // This code (unlike the C# version) DOES trap exceptions coming from the database. 
                // It uses the utility function above to find out what kind of exception 
                // it is and and then return an appropriate error code

    