using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Script.Models
{
    /// <summary>
    /// Basic tree node structure used by jsTree, sent back to the page as JSON
    /// </summary>
    public class TreeNodeData
    {
        public string id { get; set; }
        public string text { get; set; }
        public string icon { get; set; }
        public TreeNodeState state { get; set; }
        public TreeNodeData[] children { get; set; }
        public Dictionary<string,string> data { get; set; }
    }

    public class TreeNodeState
    {
        public bool opened { get; set; }
        public bool disabled { get; set; }
        public bool selected { get; set; }
    }
}