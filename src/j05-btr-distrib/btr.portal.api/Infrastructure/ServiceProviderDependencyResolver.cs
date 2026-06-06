using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace btr.portal.api.Infrastructure
{
    public class ServiceProviderDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderDependencyResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDependencyScope BeginScope()
        {
            return new ServiceProviderDependencyScope(_serviceProvider.CreateScope());
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _serviceProvider.GetServices(serviceType);
        }

        private sealed class ServiceProviderDependencyScope : IDependencyScope
        {
            private readonly IServiceScope _scope;

            public ServiceProviderDependencyScope(IServiceScope scope)
            {
                _scope = scope;
            }

            public void Dispose()
            {
                _scope.Dispose();
            }

            public object GetService(Type serviceType)
            {
                return _scope.ServiceProvider.GetService(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                return _scope.ServiceProvider.GetServices(serviceType);
            }
        }
    }
}
