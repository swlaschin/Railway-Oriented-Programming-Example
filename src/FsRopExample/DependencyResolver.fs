namespace FsRopExample

open System
open System.Collections.Generic
open System.Linq
open System.Web.Http.Dependencies

type TypeConstructor = unit -> obj

/// A simple implementation of DI for use in WebApi.
/// The reason we implement this is that we need to inject services into the
/// controller, which requires setting a IDependencyResolver in the HttpConfiguration.
///
/// Note that Controllers are created per request. 
/// So we register a lambda that constructs them on demand, rather than a singleton instance.
type DependencyResolver() =

    let _registeredTypes = new Dictionary<Type, TypeConstructor>();

    let getService(serviceType:Type) =
        let found, fn = _registeredTypes.TryGetValue(serviceType) 
        if not found then
            null
        else
            fn()

    member this.RegisterType<'a> (ctor:TypeConstructor) = 
        _registeredTypes.[typeof<'a>] <- ctor 

    interface IDependencyResolver with

        member this.GetService(serviceType:Type) = 
            getService serviceType

        member this.GetServices(serviceType:Type) =
            let obj = getService serviceType
            if (obj <> null) then
                seq {yield obj}
            else
                Seq.empty

        member this.Dispose() = 
            ()

        member this.BeginScope() = 
            upcast this 
