﻿using Microsoft.Owin;
using Owin;

// Uncomment when using Sitecore or use web.config appSetting
//[assembly: OwinStartup(typeof(Sitecore.Script.Owin.Startup))]
namespace Sitecore.Script.Owin
{
  using Sitecore.Script.Owin.Pipelines.InitializeOwinMiddleware;
  using Sitecore.Pipelines;

    public class Startup
    {
        public virtual void Configuration(IAppBuilder app)
        {
            CorePipeline.Run("initializeOwinMiddleware", new InitializeOwinMiddlewareArgs(app));
        }
    }
}
