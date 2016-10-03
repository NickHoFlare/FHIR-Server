using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Practices.Unity;
using ServerExperiment.Models.Repository;
using ServerExperiment.Utils;

namespace ServerExperiment
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Configuration of Unity Dependency Resolver
            var container = new UnityContainer();
            container.RegisterType<IPatientRepository, PatientRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IDeviceRepository, DeviceRepository>(new HierarchicalLifetimeManager());
            container.RegisterType<IObservationRepository, ObservationRepository>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "fhir/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SpecificPatient",
                routeTemplate: "fhir/Patient/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "SpecificPatientRecord",
                routeTemplate: "fhir/PatientRecord/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SpecificDevice",
                routeTemplate: "fhir/Device/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "SpecificDeviceRecord",
                routeTemplate: "fhir/DeviceRecord/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SpecificObservation",
                routeTemplate: "fhir/Observation/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "SpecificObservationRecord",
                routeTemplate: "fhir/ObservationRecord/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
