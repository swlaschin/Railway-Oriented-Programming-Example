namespace FsRopExample

open System
open System.Web.Http
open FsRopExample.Controllers
open FsRopExample.SqlDatabase
open FsRopExample.DataAccessLayer
open global.Owin

 /// IMPORTANT: When running interactively, the controllers will not be found with error:
/// "No type was found that matches the controller named 'XXX'."
/// The fix is to override the ControllerResolver to use the current assembly
type ControllerResolver() =
    inherit System.Web.Http.Dispatcher.DefaultHttpControllerTypeResolver()

    override this.GetControllerTypes (_:System.Web.Http.Dispatcher.IAssembliesResolver) = 
        let t = typeof<System.Web.Http.Controllers.IHttpController>
        System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
        |> Array.filter t.IsAssignableFrom
        :> System.Collections.Generic.ICollection<Type>   


type Startup() = 

    // Configure Web API for self-host. 
    let config = new HttpConfiguration()

    let configureRoutes() = 
        config.MapHttpAttributeRoutes();

    /// Setup the dependency injection for controllers.
    let configureDependencies() = 
        let dependencyResolver = new DependencyResolver()

        // create the services to be injected
        let fsCustomerDao = new CustomerDao()
        let csCustomerDao = new CsRopExample.DataAccessLayer.CustomerDao()

        // setup a constructor for a CustomersController
        let ctor () = new CustomersController(csCustomerDao,fsCustomerDao) :> obj
        dependencyResolver.RegisterType<CustomersController> ctor 

        // assign the resolver to the config
        config.DependencyResolver <- dependencyResolver

    let configureJsonSerialization() =
        let jsonSettings = config.Formatters.JsonFormatter.SerializerSettings
        jsonSettings.Formatting <- Newtonsoft.Json.Formatting.Indented
        jsonSettings.ContractResolver <- 
            Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()


    // Here is where the controllers are resolved
    let configureServices() = 
        config.Services.Replace(
            typeof<System.Web.Http.Dispatcher.IHttpControllerTypeResolver>, 
            new ControllerResolver())


    /// This code configures Web API. The Startup class is specified as a type
    /// parameter in the WebApp.Start method.
    member this.Configuration(appBuilder:IAppBuilder) = 

        configureRoutes()
        configureDependencies()
        configureJsonSerialization()
        configureServices() 

        // add logging
        config.MessageHandlers.Add(new MessageLoggingHandler())

        appBuilder.UseWebApi(config) |> ignore
    
