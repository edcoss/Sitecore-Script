using Microsoft.Extensions.DependencyInjection;
using ScriptSharp.ScriptEngine.Abstractions;
using ScriptSharp.ScriptEngine.OutputBuilders;
using ScriptSharp.ScriptEngine.Engines;
using ScriptSharp.ScriptEngine.CSharp;
using Sitecore.DependencyInjection;

namespace Sitecore.Script.DependencyInjection
{
    /// <summary>
    /// Dependency injection for Sitecore Object Debugger tool
    /// </summary>
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(typeof(IObjectOutputBuilder), typeof(HTMLObjectOutputBuilder));
            serviceCollection.AddSingleton(typeof(IObjectDebugger), typeof(CSharpObjectDebugger));
            serviceCollection.AddSingleton(typeof(IScriptEngine), typeof(CSharpScriptEngine));
            serviceCollection.AddSingleton(typeof(IScriptManager), typeof(CSharpScriptManager));
        }
    }
}