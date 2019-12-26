using Sitecore.Data.Items;
using Sitecore.Script.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Script.Helpers
{
    public class ItemMapper
    {
        /// <summary>
        /// Maps a Sitecore Item and all its descendants, to a basic tree node structure type
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TreeNodeData MapItemToTreeNode(Item item)
        {
            var node = new TreeNodeData();
            node.data = new Dictionary<string, string>();
            if (item.TemplateName == Settings.ScriptCodeTemplateName)
            {
                node.id = item.ID.ToString();
                node.data.Add("type", "script");
            }
            else
            {
                node.id = item.ID.ToString();
                node.data.Add("type", "folder");
            }

            node.text = item.Name;
            node.state = new TreeNodeState() { disabled = false, opened = true, selected = false };
            node.icon = "/temp/iconcache/" +item.Appearance.Icon;
            node.data.Add("path", item.Paths.FullPath.Replace(Settings.ScriptLibraryPath, string.Empty));
            if (item.HasChildren && item.Children != null)
            {
                var childrenList = new List<TreeNodeData>();
                foreach (Item child in item.Children)
                {
                    childrenList.Add(MapItemToTreeNode(child));
                }
                node.children = childrenList.ToArray();
            }
            return node;
        }
    }
}