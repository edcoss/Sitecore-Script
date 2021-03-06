﻿using ScriptSharp.ScriptEngine;
using ScriptSharp.ScriptEngine.Abstractions;
using ScriptSharp.ScriptEngine.Models;
using Sitecore.Script.Extensions.Resolvers;
using Sitecore.Script.Helpers;
using Sitecore.Script.Models;
using Sitecore.Script.Pipelines;
using Sitecore.Script.Security;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.Services;

namespace Sitecore.Script
{
    [Sitecore.DependencyInjection.AllowDependencyInjection]
    public partial class ScriptEditor : System.Web.UI.Page
    {
        private static IScriptManager ScriptManager;

        public ScriptEditor() { }

        public ScriptEditor(IScriptManager scriptManager) 
        {
            ScriptManager = scriptManager;
        }

        private readonly string initialCode = Settings.DefaultInitialCode;

        //private readonly string _securityGuid = "2f195b78-b989-4ec5-b75e-51ff124b6939";

        /// <summary>
        /// List of types treated as primitives, in which their value can be displayed by using Object.ToString()
        /// </summary>
        private static List<Type> displayTypesForObjectDetails = new List<Type>()
        {
            typeof(string),
            typeof(decimal),
        };

        /// <summary>
        /// List of types where Sitecore Script tool won't drill down for property and field evaluation
        /// </summary>
        private static List<Type> skipTypesForPropertyDetails = new List<Type>()
        {
            //typeof(string),
            //typeof(DateTime),
            //typeof(System.Globalization.CultureInfo),
            //typeof(Sitecore.Data.ID),
            //typeof(Sitecore.Data.Version),
            //typeof(Sitecore.Globalization.Language)
        };

        /// <summary>
        /// List of non-primitive types where we don't want to evaluate when drilling down for properties and fields,
        /// since first Object.ToString() call already provides its value.
        /// </summary>
        private static List<Type> skipTypesForValueReprint = new List<Type>()
        {
            typeof(string),
            typeof(KeyValuePair<string,object>)
        };

        /// <summary>
        /// List of types treated as primitives, where we don't want to drill down for properties and fields evaluation
        /// </summary>
        private static List<Type> typesAsPrimitivesForValuePrint = new List<Type>()
        {
            typeof(string),
            typeof(object),
            typeof(decimal),
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            // Optional security check ...

            //if (string.IsNullOrWhiteSpace(Request.QueryString["security"]) ||
            //    (!string.IsNullOrWhiteSpace(Request.QueryString["security"]) &&
            //    !Request.QueryString["security"].Equals(_securityGuid, StringComparison.InvariantCultureIgnoreCase)))
            //{
            //    throw new SecurityException("The security token is not correct!");
            //}

            var principal = new SitecorePrincipal(new SitecoreIdentity());
            if (!principal.Identity.IsAuthenticated)
            {
                Sitecore.Web.WebUtil.Redirect("/sitecore/admin/login.aspx?returnUrl=" + Request.Url.PathAndQuery, true);
            }
            Thread.CurrentPrincipal = principal;
            if (!principal.IsInRole("Administrator"))
            {
                throw new SecurityException("Only Sitecore Administrator users are allowed!");
            }

            if (!IsPostBack)
            {
                this.CodeLiteral.Text = initialCode;
                Initialize(true);
            }
            else
            {
                Initialize(false);
            }
        }

        /// <summary>
        /// Initializes backend Roslyn engine, for a scripting .Net language
        /// </summary>
        /// <param name="reset"></param>
        private void Initialize(bool reset)
        {
            var binPath = Server.MapPath("~/bin");
            if (reset) ScriptManager.InitializeEngine(binPath,
                MirrorSharpOwinMiddleware.DotNetDllFiles,
                new CacheMetadataReferenceResolver(ImmutableArray<string>.Empty, binPath),
                new ScriptSourceResolver(ImmutableArray<string>.Empty, binPath));
        }

        /// <summary>
        /// Code behind for Run button from Sitecore Script page.
        /// Uses custom script manager, initializing it with debugging and build output parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [PrincipalPermission(SecurityAction.Demand, Authenticated = true, Role = "Administrator")]
        protected void Run_Click(object sender, System.EventArgs e)
        {
            this.Output.Text = string.Empty;
            var code = HttpUtility.HtmlDecode(Request.Unvalidated.Form["code"]);
            this.CodeLiteral.Text = code;
            ScriptReturnResults response = null;

            using (new ScriptStateSwitcher(ScriptSecurityState.Limited))
            {
                response = ScriptManager.RunScript(new ScriptParameters()
                {
                    Eval = false,
                    Code = code,
                    EvalCode = new string[] { },
                    StartIndent = 0,
                    MaxIndent = int.MaxValue,
                    DisplayTypesForObjectDetails = displayTypesForObjectDetails,
                    SkipTypesForPropertyDetails = skipTypesForPropertyDetails,
                    SkipTypesForValueReprint = skipTypesForValueReprint,
                    TypesAsPrimitivesForValuePrint = typesAsPrimitivesForValuePrint
                });
            }

            this.TimeExecution.Text = response.ElapsedMilliseconds.ToString();
            if (response.IsHTMLResult)
            {
                this.Output.Text = string.Empty;
                this.OutputHTML.Text = response.Results;
            }
            else
            {
                this.Output.Text = response.Results;
                this.OutputHTML.Text = string.Empty;
            }
        }

        /// <summary>
        /// Code behind for Reset button from Sitecore Script page.
        /// It resets the state of the Roslyn engine, where script code has been compiled and evaluated, to an empty state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [PrincipalPermission(SecurityAction.Demand, Authenticated = true, Role = "Administrator")]
        protected void ResetRun_Click(object sender, System.EventArgs e)
        {
            this.Output.Text = string.Empty;
            Initialize(true);
        }

        /// <summary>
        /// Evaluates properties and fields from Sitecore Script output, called from AJAX script, when expanding/drilling down on
        /// properties or fields.
        /// </summary>
        /// <param name="source">Script code from editor</param>
        /// <param name="evalCode">Evaluation code using reflection</param>
        /// <param name="indent">Horizontal indentation</param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public static ScriptReturnResults EvaluateObject(string source, string[] evalCode, string indent)
        {
            var response = ScriptManager.RunScript(new ScriptParameters()
            {
                Eval = true,
                Code = source,
                EvalCode = evalCode,
                StartIndent = int.Parse(indent) + 2,
                MaxIndent = int.MaxValue,
                DisplayTypesForObjectDetails = displayTypesForObjectDetails,
                SkipTypesForPropertyDetails = skipTypesForPropertyDetails,
                SkipTypesForValueReprint = skipTypesForValueReprint,
                TypesAsPrimitivesForValuePrint = typesAsPrimitivesForValuePrint
            });
            return response;
        }

        /// <summary>
        /// Returns item and subitems mapped to JSON format compatible with jsTree plugin
        /// </summary>
        /// <param name="rootItemId"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public static TreeNodeData LoadItems(string rootItemId)
        {
            var database = Sitecore.Configuration.Factory.GetDatabase(Settings.ContextDatabase);
            var rootItem = database.GetItem(rootItemId);
            var treeNode = ItemMapper.MapItemToTreeNode(rootItem);
            return treeNode;
        }

        /// <summary>
        /// Returns the script code stored in script code item, specified by the Sitecore item Id
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public static string GetScriptCode(string itemId)
        {
            var database = Sitecore.Configuration.Factory.GetDatabase(Settings.ContextDatabase);
            var item = database.GetItem(itemId);
            if (item != null && item.TemplateName == Settings.ScriptCodeTemplateName)
            {
                return item[Settings.CodeFieldName];
            }
            return string.Empty;
        }

        /// <summary>
        /// Saves script code from editor, into a Sitecore Script Code item.
        /// If Item Id is a folder item, it uses the provided name parameter to create new script code item.
        /// If Item Id is a script item, it updates existing item with provided script code.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="name"></param>
        /// <param name="code"></param>
        [WebMethod(EnableSession = true)]
        public static TreeNodeData SaveScriptCode(string itemId, string name, string code)
        {
            var database = Sitecore.Configuration.Factory.GetDatabase(Settings.ContextDatabase);
            var item = database.GetItem(itemId);
            TreeNodeData treeNode = null;
            if (item != null)
            {
                if (item.TemplateName == Settings.ScriptFolderTemplateName)
                {
                    // when item is parent node, use template full name to get template item
                    var templateItem = database.GetTemplate(Settings.ScriptCodeTemplateFullName);
                    var formattedName = Data.Items.ItemUtil.ProposeValidItemName(name);

                    Sitecore.Data.Items.Item newItem = null;
                    using (new SecurityModel.SecurityDisabler())
                    {
                        newItem = item.Add(formattedName, templateItem);
                        newItem.Editing.BeginEdit();
                        newItem[Settings.CodeFieldName] = code;
                        newItem.Editing.EndEdit();
                    }
                    treeNode = ItemMapper.MapItemToTreeNode(newItem);
                }
                else if (item.TemplateName == Settings.ScriptCodeTemplateName)
                {
                    // item is a script code item
                    using (new SecurityModel.SecurityDisabler())
                    {
                        item.Editing.BeginEdit();
                        item[Settings.CodeFieldName] = code;
                        item.Editing.EndEdit();
                    }
                    treeNode = ItemMapper.MapItemToTreeNode(item);
                }
            }
            return treeNode;
        }

        /// <summary>
        /// REPL mode used to evaluate lines of code, which are syntactically complete
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public static REPLReturnResults EvaluateCode(string source)
        {
            using (new ScriptStateSwitcher(ScriptSecurityState.Limited))
            {
                var response = ScriptManager.RunREPL(new REPLParameters()
                {
                    Code = source,
                });
                return response;
            }
        }

        /// <summary>
        /// Indicates whether a piece of code is syntactically complete
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public static REPLReturnResults IsBalancedCode(string source)
        {
            REPLReturnResults results = new REPLReturnResults()
            {
                IsBalanced = ScriptManager.IsCompilableCode(source)
            };
            return results;
        }
    }
}