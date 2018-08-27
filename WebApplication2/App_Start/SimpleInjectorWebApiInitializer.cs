//[assembly: WebActivator.PostApplicationStartMethod(typeof(WebApplication1.App_Start.SimpleInjectorWebApiInitializer), "Initialize")]

namespace WebApplication1.App_Start
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using SimpleInjector;
    using SimpleInjector.Integration.WebApi;
    using SimpleInjector.Lifestyles;
    using System.Web.Http.Controllers;
    using System.Linq.Expressions;
    using WebApplication2.Controllers;

    public static class SimpleInjectorWebApiInitializer
    {
        /// <summary>Initialize the container and register it as Web API Dependency Resolver.</summary>
        public static void Initialize()
        {
            // Web API configuration and services

            // Web API routes
            var config = GlobalConfiguration.Configuration;
            var container = new Container();
            var controllers = new HashSet<Type>();

            var repo = container.RegisterService(new SqlUserRepository());
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/values",
            //    defaults: new { controller = "Values" }
            //);

            config.MapRoute(controllers, "DefaultApi", "api/values", () => new ValuesController(repo), "Gt");



            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerTypeResolver), new ListHttpControllerTypeResolver(controllers));
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
        private static void MapRoute<T>(this HttpConfiguration config, ICollection<Type> controllers, string name, string routeTemplate, Expression<Func<T>> builder, string action)
        {
            var controllerType = typeof(T);
            controllers.Add(controllerType);
            config.Routes.MapHttpRoute(
                name: name,
                routeTemplate: routeTemplate,
                defaults: new { controller = controllerType.Name.Substring(0, controllerType.Name.Length - "Controller".Length) }
            );
        }
        private static T RegisterService<T>(this Container container, T t) where T: class
        {
            container.RegisterInstance(t);
            foreach(var i in typeof(T).GetInterfaces())
            {
                container.RegisterInstance(i, t);
            }

            return t;
        }
    }

    public interface IUserRepository
    {

    }

    public class SqlUserRepository : IUserRepository
    {

    }

    public class ListHttpControllerTypeResolver : IHttpControllerTypeResolver
    {
        ICollection<Type> types;
        public ListHttpControllerTypeResolver(IEnumerable<Type> types)
        {
            this.types = types.ToArray();
        }

        public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
        {
            return this.types;
        }
    }
}