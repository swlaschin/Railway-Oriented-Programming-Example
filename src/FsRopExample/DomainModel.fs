module DomainModel

open CommonTypes

// ============================== 
// Exposed models
// ============================== 

[<CLIMutableAttribute>]
type CustomerDto = {
    FirstName: string
    LastName: string
    Email: string
}

// ============================== 
// Internal models
// ============================== 

type PersonalName = {
    FirstName: String10 
    LastName: String10 
}

type Customer = {
    Name: PersonalName
    Email: EmailAddress
}


