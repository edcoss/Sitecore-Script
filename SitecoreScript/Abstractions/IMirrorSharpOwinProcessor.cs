using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Scripts.Abstractions
{
    public interface IMirrorSharpOwinProcessor
    {
        void AddAssembly(string assemblyPath);
        void ExcludeFile(string filename);
        void AddAllAssembliesInPath(string path);
        void ExcludeDiagnosticId(string diagnosticId);
    }
}
