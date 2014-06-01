module FsRopExample.DomainModel

open System
open Rop
open DomainPrimitiveTypes

// ============================== 
// Domain models
// ============================== 


type PersonalName = {
    FirstName: String10.T 
    LastName: String10.T 
}

type Customer = {
    Id: CustomerId.T 
    Name: PersonalName
    Email: EmailAddress.T 
}

// All possible things that can happen in the use-cases
type DomainMessage =

    // Validation errors
    // Note: I deliberate choose extremelt specific ones to 
    // show how easy it is to have fine detail in the errors
    | CustomerIsRequired
    | CustomerIdMustBePositive
    | FirstNameIsRequired
    | FirstNameMustNotBeMoreThan10Chars
    | LastNameIsRequired
    | LastNameMustNotBeMoreThan10Chars
    | EmailIsRequired
    | EmailMustNotBeMoreThan20Chars
    | EmailMustContainAtSign

    // Events
    | EmailAddressChanged of string * string

    // Exposed errors
    | CustomerNotFound

    // Internal errors
    | SqlCustomerIsInvalid
    | DatabaseTimeout
    | DatabaseError of string

// ============================== 
// Utility functions
// ============================== 

let createFirstName firstName = 
    let map = function
        | StringError.Missing -> FirstNameIsRequired
        | MustNotBeLongerThan _ -> FirstNameMustNotBeMoreThan10Chars
        | DoesntMatchPattern _ -> failwithf "not expecting DoesntMatchPattern for firstName" 

    // create the string and convert the messages into the ones
    // appropriate for the domain
    String10.create firstName
    |> mapMessagesR map

let createLastName lastName = 
    let map = function
        | StringError.Missing -> LastNameIsRequired
        | MustNotBeLongerThan _ -> LastNameMustNotBeMoreThan10Chars
        | DoesntMatchPattern _ -> failwithf "not expecting DoesntMatchPattern for lastName" 

    // create the string and convert the messages into the ones
    // appropriate for the domain
    String10.create lastName 
    |> mapMessagesR map


let createEmail email = 
    let map = function
        | StringError.Missing -> EmailIsRequired
        | MustNotBeLongerThan _ -> EmailMustNotBeMoreThan20Chars
        | DoesntMatchPattern _ -> EmailMustContainAtSign

    // create the EmailAddress and convert the mes sages into the ones
    // appropriate for the domain
    EmailAddress.create email
    |> mapMessagesR map

let createCustomerId customerId = 
    let map = function
        | IntegerError.Missing -> CustomerIsRequired
        | MustBePositiveInteger _ -> CustomerIdMustBePositive

    // create the CustomerId and convert the messages into the ones
    // appropriate for the domain
    CustomerId.create customerId
    |> mapMessagesR map

let createPersonalName firstName lastName = 
    {FirstName = firstName; LastName = lastName}

let createCustomer custId name email = 
    {Id = custId; Name = name; Email = email}


