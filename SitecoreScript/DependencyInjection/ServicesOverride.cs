using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using System.Linq;
using Sitecore.Abstractions;
using Sitecore.Script.Security;

namespace Sitecore.Script.DependencyInjection
{
    /// <summary>
    /// Dependency injection for Sitecore Object Debugger tool
    /// </summary>
    public class ServicesOverride : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            var defaultAuthorizationManager = serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(BaseAuthorizationManager));
            serviceCollection.Remove(defaultAuthorizationManager);
            serviceCollection.AddSingleton<BaseAuthorizationManager, ScriptAuthorizationManager>();
        }
    }
}