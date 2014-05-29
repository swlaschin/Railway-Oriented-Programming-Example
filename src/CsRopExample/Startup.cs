using System.Web.Http;
using CsRopExample.Controllers;
using CsRopExample.Database;
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

            appBuilder.UseWebApi(config);
        }

        private static void ConfigureRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        private static void ConfigureDependencies(HttpConfiguration config)
        {
            var dependencyResolver = new DependencyResolver();
            var repository = new CustomerRepository();
            dependencyResolver.RegisterType<CustomersController>(() => new CustomersController(repository));
            config.DependencyResolver = dependencyResolver;
        }

        private static void ConfigureJsonSerialization(HttpConfiguration config)
        {
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
            jsonSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        }

    }

}
