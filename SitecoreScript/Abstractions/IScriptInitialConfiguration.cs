using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Script.Abstractions
{
    public interface IScriptInitialConfiguration
    {
        IEnumerable<string> DotNetReferenceFiles { get; }
        IEnumerable<string> ExcludedFiles { get; }
        IEnumerable<string> ExcludedDiagnosticIds { get; }

    }
}