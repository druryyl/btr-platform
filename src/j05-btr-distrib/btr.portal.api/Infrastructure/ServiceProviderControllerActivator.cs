using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace btr.portal.api.Infrastructure
{
    public class ServiceProviderControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(
            HttpRequestMessage request,
            HttpControllerDescriptor controllerDescriptor,
            Type controllerType)
        {
            var scope = request.GetDependencyScope();
            var controller = scope.GetService(controllerType) as IHttpController;

            if (controller != null)
                return controller;

            return (IHttpController)Activator.CreateInstance(controllerType);
        }
    }
}
