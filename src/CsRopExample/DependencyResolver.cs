using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;

namespace CsRopExample
{
    /// <summary>
    /// Simple implementation of DI
    /// </summary>
    internal class DependencyResolver : IDependencyResolver
    {
        private readonly Dictionary<Type, Func<object>> _registeredTypes = new Dictionary<Type, Func<object>>();

        public void RegisterType<T>(Func<object> constructor)
        {
            _registeredTypes[typeof(T)] = constructor;
        }

        public object GetService(Type serviceType)
        {
            if (!_registeredTypes.ContainsKey(serviceType))
            {
                return null;

            }
            var fn = _registeredTypes[serviceType];
            return fn();
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var obj = GetService(serviceType);
            if (obj == null)
            {
                return Enumerable.Empty<object>();
            }
            return new List<object> { obj };
        }

        public void Dispose()
        {
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }
    }
}
