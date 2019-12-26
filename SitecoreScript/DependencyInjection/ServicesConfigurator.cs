using Microsoft.Extensions.DependencyInjection;
using ScriptSharp.ScriptEngine.Abstractions;
using ScriptSharp.ScriptEngine.OutputBuilders;
using ScriptSharp.ScriptEngine.Engines;
using ScriptSharp.ScriptEngine.CSharp;
using Sitecore.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Script.DependencyInjection
{
    /// <summary>
    /// Dependency injection for Sitecore Object Debugger tool
    /// </summary>
    public class ServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IObjectOutputBuilder), typeof(HTMLObjectOutputBuilder));
            serviceCollection.AddScoped(typeof(IObjectDebugger), typeof(CSharpObjectDebugger));
            serviceCollection.AddScoped(typeof(IScriptEngine), typeof(CSharpScriptEngine));
            serviceCollection.AddScoped(typeof(IScriptManager), typeof(CSharpScriptManager));
        }
    }
}