using System.Web.Http;
using CsRopExample.Controllers;
using CsRopExample.DataAccessLayer;
using Owin;

namespace CsRopExample
{
    class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            var config = new HttpConfiguration();

            ConfigureRoutes(config);
            ConfigureDependencies(config);
            ConfigureJsonSerialization(config);

            // add logging
            config.MessageHandlers.Add(new MessageLoggingHandler());

            appBuilder.UseWebApi(config);
        }

        private static void ConfigureRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        /// <summary>
        /// Setup the dependency injection for controllers.
        /// </summary>
        private static void ConfigureDependencies(HttpConfiguration config)
        {
            var dependencyResolver = new DependencyResolver();

            // create the service to be injected
            var customerDao = new CustomerDao();

            // setup a constructor for a CustomersController
            dependencyResolver.RegisterType<CustomersController>(() => new CustomersController(customerDao));

            // assign the resolver to the config
            config.DependencyResolver = dependencyResolver;
        }

        private static void ConfigureJsonSerialization(HttpConfiguration config)
        {
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

    }

}
