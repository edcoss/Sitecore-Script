using Sitecore.Diagnostics;
using Sitecore.Owin.Pipelines.Initialize;
using Sitecore.Script.Owin.Pipelines.InitializeOwinMiddleware;

namespace Sitecore.Script.Pipelines
{
    public class MirrorSharpSitecoreOwinMiddleware : InitializeProcessor
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
    }
}