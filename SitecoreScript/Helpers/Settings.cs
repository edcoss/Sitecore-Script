using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Script.Helpers
{
    /// <summary>
    /// Contains constants and references to configurable Sitecore settings, found in Sitecore.Script.config
    /// </summary>
    public class Settings
    {
        public static string DefaultInitialCode = @"using System;
using System.Collections.Generic;
    
public class Script
{
    public IEnumerable<string> Run()
    {
        //Your code here    
        yield return ""Hello"";
        yield return ""World!"";
    }
}

new Script().Run()
";

        public static string ContextDatabase => Sitecore.Configuration.Settings
            .GetSetting("Sitecore.Script.ContextDatabase",
            "master");

        public static string ScriptFolderTemplateName => Sitecore.Configuration.Settings
            .GetSetting("Sitecore.Script.ScriptFolderTemplateName",
            "Sitecore Script Folder");

        public static string ScriptCodeTemplateName => Sitecore.Configuration.Settings
            .GetSetting("Sitecore.Script.ScriptCodeTemplateName",
            "Sitecore Script Code");

        public static string ScriptCodeTemplateFullName => Sitecore.Configuration.Settings
            .GetSetting("Sitecore.Script.ScriptCodeTemplateFullName",
            "modules/Sitecore Script/Sitecore Script Code");

        public static string CodeFieldName => Sitecore.Configuration.Settings
            .GetSetting("Sitecore.Script.CodeFieldName",
            "Code");

        public static string ScriptLibraryPath => Sitecore.Configuration.Settings
            .GetSetting("Sitecore.Script.LibraryPath",
            "/sitecore/system/Modules/Sitecore Script/Script Library");

    }
}