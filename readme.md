# Railway Oriented Programming example

This repository contains code that demonstrates the "Railway Oriented Programming" concept
for error handling in functional programming languages.

You can find out more about Railway Oriented Programming at http://fsharpforfunandprofit.com/rop,
where there are slides and videos that explain the concepts in more detail. 

## The example scenarios 

There are two scenarios that I will use in this example:

**Get a customer**. The steps are:

* Validate the input params.
* Fetch the customer from the database.
* Convert the customer into a DTO. 
* Return the DTO or the appropriate error.

**Add or update a customer**. The steps are:

* Validate the input params.
* Convert the DTO into customer.
* Insert or update the customer in the database.
* If the customer's email has changed, send them a notification.
* Return OK or the appropriate error.
  
Each scenario is implemented twice, in both C# and F#.
The first implementation has no error handling, while the second implementation has complete error handling.

These implementations demonstrate that error handling is very complex in C#, while in F#, using the 
Railway Oriented Programming approach, the error handling code is just as simple as the non-error-handling code.

## Running the code

There are two projects, a C# example (`CsRopExample`) and an F# example (`FsRopExample`).
They both implement the same simple WebApi service which supports getting and posting customers.

Both projects are self-hosting OWIN web services. You can run the EXEs and interact with them through a browser.

* **To compile the code,** open up the solution file and compile in Visual Studio (sorry, no FAKE script yet!).
  The binaries are output to the `\bin` folder
* **To run the code,** just run the EXEs from each folder.
  The C# service runs on localhost:9000 and the F# service runs on localhost:9001
* **To interact with the services,** use a REST testing tool like [Postman](http://www.getpostman.com/).


The API is:

* `GET /example` shows an example DTO that you can use for POSTing
* `GET /customer/1` fetches the customer for that id. This is *without* error handling, so a missing customer
  will still return 200!
* `GET /customerE/1` fetches the customer for that id. This is *with* error handling, so a missing customer
  will now return 404.
* `POST /customer/1` adds or updates the customer for that id. This is *without* error handling, so an invalid customerDto
  will cause an exception to be thrown. Try setting the names to blank or removing the "@" from the email address.
* `POST /customer/1` adds or updates the customer for that id. This is *with* error handling, so an invalid customerDto
  will show a nice error message with a "BadRequest" response.

Bonus: Set the id to 42 when posting to emulate a database timeout exception. 


## About the code

The design of both projects is similar, and can be grouped into the following layers or subsystems:

* There is a domain model that contains the definition of a `Customer` object.
* There is an in-memory "SqlDatabase" that stores customers.
* There is a Data Access Layer that provides a wrapper on top of the sql database.
* There is a `CustomerDto` type that is a flattened, "dumb" version of the domain type.
* There is a `CustomersController` that provides the endpoint for the service.
* Finally, there is the infrastructure, setup and configuration.


## The domain model layer

*(The C# code is in the `DomainModels` folder. The F# code is in the `DomainModel.fs` and `DomainPrimitiveTypes.fs` files.)*

The domain model contains the definition of a `Customer` object and its subcomponents:

* `CustomerId`
* `PersonalName`
* `EmailAddress`

These are modelled as proper types rather than as [primitives such as `string`](http://sourcemaking.com/refactoring/primitive-obsession)
so that validation can be done at creation time; that way we can be sure that the instances are always valid.

For example:

* The `CustomerId` is constrained to contain a positive integer
* The first and last names in `PersonalName` are constrained to be non-null and be no longer than 10 chars.
  I set the length constraint very short for this example. You should set it to the field length of the backing database, if applicable.
* The `EmailAddress` is constrained to be non-null, no longer than 20 chars, and to contain a "@" sign.
  Obviously, you can get more sophisticated with the validation if you like.


**C# version**

In C# I use static methods to validate and create domain types. ([C# tips to avoid primitive obsession](http://lostechies.com/jimmybogard/2007/12/03/dealing-with-primitive-obsession/)).

Even so, there are some issues with these kinds of types in C#:

* 1) You have to add code for equality
* 2) They are allowed to be null, so when building a compound type (such as `Customer`) out of smaller types, 
  we still have to check for null.
* 3) In the constructor there is no nice way to handle failed creations. Typically you would return null
  and perhaps throw an exception such as `ArgumentException` or `InvalidCastException`.

**F# version**

In F#, I have created primitive types for `String10`, `EmailAddress`, `CustomerId`, etc. 

The F# equivalent of a private constructor is to use signature files. With this technique, you can hide the internals of the
type completely (an opaque type) so that users of the type are forced to use the helper functions 
to create and access the data inside.

The compound types such as `PersonalName` and `Customer` are then built from the primitive types.

Compared with the C# code, the F# has a number of advantages

1) In F#, we did not need to define equality for the types. You get that for free.

2) In F# the primitive types cannot be null, so it is literally not possible to create an invalid `Customer` type.
Hence, the compound types do NOT need to be opaque or encapsulated -- anyone can create one directly.

3) When creating the primitive types, no exceptions or nulls are used! 
Instead, I have used the Success/Failure type (aka "Either", aka `RopResult`) to return what happened.

Callers then have to handle both cases explicitly, and convert the error codes from the primitive errors 
to the domain-level error codes as appropriate.  

For example, when creating a first name, the primitive-level error `Missing` 
is converted into the domain-level error `FirstNameIsRequired`.
See the `createEmail` function in `DomainModel.fs` for another example of this mapping.


## The "sql database"  layer

*(The C# code is in the `SqlDatabase` folder. The F# code is in the `SqlDatabase.fs` file.)*

The "sql database" is just an in-memory static dictionary. 

The entry point is a class called `DbContext`. It has the following methods:

* `Customers` returns a IEnumerable of customers.
* `Insert` adds a customer to the dictionary and throws a "DuplicateKey" exception if it already exists.
* `Update` updates a customer in the dictionary and throws a "KeyNotFound" exception if it does not already exist.

I have also added a special case -- if the customerId is 42, throw a "Timeout" exception.

The customer objects returned by the database are `DbCustomer` objects, which are not domain objects, so they
need to be mapped to the domain object on the way in or out.  This is done by the Data Access Layer (described next).

The C# code and the F# code are identical. This means that I had to add extra code in F# to
allow the `DbCustomer` class to be nullable, and for the properties to be nullable.  

In F# it is much easier to make a non-nullable immutable class than a nullable mutable class! In C# of course, it
is the other way round. 

Also of note, in F# you can define an exception class in one line. Compare the F# version of `SqlException` with the 
C# one.


## The data access layer

*(The C# code is in the `DataAccessLayer` folder. The F# code is in the `DataAccessLayer.fs` file.)*

The data access layer wraps the Sql database and converts the domain customers to the sql customers

There is a `ICustomerDao` which represents the interface for a data access object.
A `ICustomerDao` is injected into the controller to isolate it from the database.

The implementation of this interface is called `CustomerDao`.

The methods are:

* `GetAll` which returns all customers.
* `GetById` which returns one customer.
* `Upsert` which either inserts or updates the customer. 

The C# and F# versions of this DAO are very different.


**C# version**

Let's start with getting a customer.
For `GetById`, what should the implementation do if the customer is not found?

In this C# implementation, I just chose to return null.  You could throw an exception, but that 
seems like overkill for such a common case.

Also, what should happen if the database throws an exception? For example, as noted above, this
database will throw a timeout if the customer id is 42.

In this implementation, I chose to ignore exceptions and let the caller catch them.

Another problem that might arise is if the domain `Customer` cannot be reconstructed from a persisted `DbCustomer` object.
This could happen if the database has a null email, for example. What should happen in this case? 

In the C# code we just ignore the conversion error.

**F# version**

The F# version is very different from the C# version.

Because errors and messages can be passed back in the function result, the `ICustomerDao` interface
has Success/Failure results rather than Customers or voids.

So in the implementation of `GetById`, if the customer is not found, we return a `CustomerNotFound` code explicitly
in the result.

The same technique can be used to trap exceptions and turn them into clean error codes.
This means that, unlike the C# code, the clients of the F# implementation never have to trap exceptions at all.

So for example, if the Sql database throws a Timeout exception, the CustomerDao traps that and turns it into
a DatabaseTimeout error code.

Finally, what about the case when the domain `Customer` cannot be reconstructed from a `DbCustomer` object?

In the F# we can't ignore the errors by accident, as the type system won't let us!  In this case, I did choose to ignore it
(see line 140 in `DataAccessLayer.fs`) but we could choose to take other actions, such as logging them for manual correction.

**The EmailAddressChanged event**

One of the requirements in the "update" scenario is to send a notification to the customer if their email changes.

To do this, we need to load the existing record and compare it with the incoming record. But where to do this?

Rather than expose the details of the database to the caller, I put this test inside the `CustomerDao` itself.

In the C# version, there is no easy way to indicate to the caller what events happened, so I use a [static
`DomainEvents` class as a broker](http://lostechies.com/jimmybogard/2010/04/08/strengthening-your-domain-domain-events/).

The CustomerDao triggers the event on the DomainEvents, and any interested parties can hook into the event to handle it.

I'm not a fan of this model, because shared global events are too much like magic for me. Also,
they are hard to control. For example, let's say I am doing some admin work and I want to canonicalize some of the emails
in the database -- in that case, I don't want to trigger events for the customer. 

So now I want to turn off events for some callers but not for others. How can I do that in a clean way?

Much better if we can put the event in the data flow itself. This is exactly the approach that the F#
version uses. 

In F# the result can include events too, so we just add an `EmailAddressChanged` event to the result.
The caller can then decide to process that event or not, and there is no global magic anywhere.

## The DTOs

*(The C# code is in the `Dtos` folder. The F# code is in the `Dtos.fs` file.)*

There is a `CustomerDto` type that is a flattened, "dumb" version of the domain type. This is what is used on the wire
for serializing requests and responses.

Along with the DTO, we need some associated conversion functions which can convert from a DTO to a domain Customer and back.

Converting from a domain Customer to a DTO should always succeed, but what about converting from a DTO to a domain Customer?
The domain Customer is more restrictive than the DTO, so there are many ways in which this conversion might fail.

**C# version**

In the C# version, we attach validation attributes to the DTO properties, and the WebApi framework will
validate them for us. For simple properties this works, but as the validation gets more complicated,
this approach begins to break down.

A more fundamental problem with this approach is that the validation should really be part of the domain model, not the DTO.

That is, it should not be possible to instantiate an invalid `Customer` object by any means. Nothing to do with DTOs at all. 

So now we have to keep the domain-level validation rules synchronized with the DTO attributes, and we are doing validation
in two places.  It would be nice not to have to do that.

**F# version**

In the F# version, the validation is not a separate step, but a core part of the definition of the domain
object. We literally cannot create a customer with a missing first name, because the type system will not
allow it.

When this approach is used, the DTO does not need any special validation at all. The creation of the customer itself
will result in a list of errors if the input is not valid.

The `DtoConverter` module in the F# code demonstrates how this works.
It uses a scary sounding technique called "applicative functors" to turn "normal" functions into functions that return errors

In this case we start with a "normal" function such as `createCustomer` at the bottom of `DomainModel.fs`. The parameters
to this functions are just normal CustomerId, PersonalName and EmailAddress. And the function returns a normal Customer.

By using applicatives, we turn this function into a new function that takes "CustomerId or error", "PersonalName or error", and "EmailAddress or error",
and returns a "Customer or error".

In this way, creating a customer from possibly bad data will give you either a valid customer or a list of validation errors,
all in one step. 

Here's a code snippet using applicatives:

```fsharp
let customerIdOrError = 
    createCustomerId dto.Id

let nameOrError = 
    createPersonalName
    <!> createFirstName dto.FirstName
    <*> createLastName dto.LastName

createCustomer 
<!> customerIdOrError 
<*> nameOrError
<*> createEmail dto.Email //inline this one
```

The `<!>` and `<*>` symbols look scary, but they always follow the same pattern, and it's no more difficult to understand
than any OO pattern, such as visitor or MVC.


## The Controllers 

*(The C# code is in the `Controllers` folder. The F# code is in the `Controllers.fs` file.)*

Finally we come to the controllers.

They are standard controllers, using attributes to indicate the routes they handle.

As noted above, each scenario is implemented twice, once without error handling and once with.

**C# version**

The C# code is standard and straightforward.

For example, here is the C# implementation of "get customer" without error handling:

```csharp
[Route("customers/{customerId}")]
[HttpGet]
public IHttpActionResult Get(int customerId)
{
    var custId = CustomerId.Create(customerId);
    var cust = _dao.GetById(custId);
    var dto = DtoConverter.CustomerToDto(cust);
    return Ok(dto);
}
```

and here is the code with error handling:

``` csharp
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
```

**F# version**

The F# is also straightforward if you are familiar with the way that F# works!

If you are not familiar with F#, you only need to know that the `|>` operator is the "pipe" symbol and just
connects the output of one function to the input of the next.

Here is the F# code without error handling:

```fsharp
[<Route("customers/{customerId}")>]
[<HttpGet>]
member this.Get(customerId:int) : IHttpActionResult =
    customerId
    |> csCreateCustomerId   // convert the int into a CustomerId
    |> csGetById            // get the Customer for that CustomerId
    |> csCustomerToDto      // convert the Customer into a DTO
    |> ok                   // return OK -- no tests for errors 
```

In order to throw exceptions I had to use the C# code! So I referenced the C# project from the F# project and used the C#
database and DAO classes.

I used the "cs" prefix to indicate that these were functions from C#.  The C# functions are aliased with code like this:

```fsharp
// create local copies of some C# functions
let csGetById = csDao.GetById
let csCreateCustomerId = CsRopExample.DomainModels.CustomerId.Create
let csCustomerToDto = CsRopExample.Dtos.DtoConverter.CustomerToDto
```

Now here is the F# code version with error handling added:

```fsharp
[<Route("customersE/{customerId}")>]
[<HttpGet>]
member this.GetWithErrorHandling(customerId:int) : IHttpActionResult =
    succeed customerId      // start with a success 
    |> logSuccessR "GetWithErrorHandling {0}"  // log the success branch
    |> createCustomerIdR    // convert the int into a CustomerId
    |> getByIdR             // get the Customer for that CustomerId
    |> customerToDtoR       // convert the Customer into a DTO
    |> logFailureR          // log any errors
    |> okR                  // return OK on the happy path
    |> toHttpResult         // other errors returned as BadRequest, etc
```

I used the "R" suffix to indicate that these were normal functions that have been converted to "railway oriented" functions.

The conversions looked like this, using the `bindR` and `mapR` functions
that are part of the Railway Oriented Programming library (`Rop.fs`):

```fsharp
// create two track versions of some F# functions
let createCustomerIdR = bindR createCustomerId
let getByIdR = bindR fsDao.GetById
let customerToDtoR = mapR DtoConverter.customerToDto
let okR result = mapR ok result
```

As you can see, the F# with error handling is still quite simple.

## The Infrastructure

The rest of the code consists of infrastructure and configuration code, and is identical in the two projects.

* `DependencyResolver` is a little DI class that allows the controllers to be injected with a `ICustomerDao`.
* `MessageLoggingHandler` is a simple logger injected into the HTTP input and output.
* `Startup` is where the WebApi is configured.
* `Program` is, of course, the main entry point.

## Dependency order of files

One glaring difference between the C# and F# projects is that in the
C# project, the files are listed in alphabetical order, while in the F# project, the files are in dependency order. 
That is because in F# you cannot use forward references (code that hasn't been seen by the compiler yet).

Although you might think this is annoying at first, it is actually very helpful, because otherwise you might
create cyclic dependencies, and [cyclic dependencies are evil](http://fsharpforfunandprofit.com/posts/cyclic-dependencies/).

On a more practical level, it means that a F# project can always be read from top to bottom, with lower-level
layers at the top of the file list, and higher-level layers at the bottom.

Once you get used to it, this becomes a great aid to understanding unfamiliar code.
In a C# project it can be hard to know where to start sometimes.


## Summary

I hope you find this project useful as a practical demonstration of the Railway Oriented Programming concepts.

If you have any corrections or suggestions to improve this code, please create an issue.

Thanks!

-- Scott


