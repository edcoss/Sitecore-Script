using Sitecore.Diagnostics;
using Sitecore.Owin.Pipelines.Initialize;
using Sitecore.Script.Owin.Pipelines.InitializeOwinMiddleware;
using Sitecore.Scripts.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Script.Pipelines
{
    public class MirrorSharpSitecoreOwinMiddleware : InitializeProcessor, IMirrorSharpOwinProcessor
    {
        private MirrorSharpOwinMiddleware msOwinMiddleware;

        public MirrorSharpSitecoreOwinMiddleware()
        {
            this.msOwinMiddleware = new MirrorSharpOwinMiddleware();
        }

        public override void Process(InitializeArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            msOwinMiddleware.Process(new InitializeOwinMiddlewareArgs(args.App));
        }

        public void AddAllAssembliesInPath(string path)
        {
            msOwinMiddleware.AddAllAssembliesInPath(path);
        }

        public void AddAssembly(string assemblyPath)
        {
            msOwinMiddleware.AddAssembly(assemblyPath);
        }

        public void ExcludeFile(string filename)
        {
            msOwinMiddleware.ExcludeFile(filename);
        }
    }
}