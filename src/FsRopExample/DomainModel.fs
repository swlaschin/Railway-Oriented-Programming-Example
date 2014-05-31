module FsRopExample.DomainModel

open System
open Rop
open CommonTypes

// ============================== 
// Domain models
// ============================== 

type CustomerId = CustomerId of int

type PersonalName = {
    FirstName: String10.T 
    LastName: String10.T 
}

type Customer = {
    Id: CustomerId 
    Name: PersonalName
    Email: EmailAddress.T 
}

// All possible things that can happen in the use-cases
type DomainMessage =

    // Validation errors
    | CustomerIsRequired
    | CustomerIdMustBePositive
    | FirstNameIsRequired
    | FirstNameIsTooLong
    | LastNameIsRequired
    | LastNameIsTooLong
    | EmailIsRequired
    | EmailIsNotAValidFormat
    | EmailIsTooLong

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

let createCustomerId custId = 
    if custId < 1 then
        fail CustomerIdMustBePositive
    else
        succeed (CustomerId custId)

let createFirstName firstName = 
    if String.IsNullOrWhiteSpace(firstName) then
        fail FirstNameIsRequired
    else
        String10.create firstName
        |> failIfNone FirstNameIsTooLong

let createLastName lastName = 
    if String.IsNullOrWhiteSpace(lastName) then
        fail LastNameIsRequired
    else
        String10.create lastName
        |> failIfNone LastNameIsTooLong

let createEmail email = 
    if String.IsNullOrWhiteSpace(email) then
        fail EmailIsRequired
    else
        EmailAddress.create email
        |> failIfNone EmailIsNotAValidFormat

let createPersonalName firstName lastName = 
    {FirstName = firstName; LastName = lastName}

let createCustomer custId name email = 
    {Id = custId; Name = name; Email = email}


