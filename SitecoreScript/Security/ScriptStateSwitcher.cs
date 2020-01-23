using Sitecore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Script.Security
{
    public class ScriptStateSwitcher : Switcher<ScriptSecurityState>
    {
        public ScriptStateSwitcher(ScriptSecurityState state) : base(state) { }
    }

    public enum ScriptSecurityState
    {
        Default,
        Limited,
        Full
    }
}