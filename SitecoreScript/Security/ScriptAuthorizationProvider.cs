using Sitecore.Data.Items;
using Sitecore.Security.AccessControl;
using Sitecore.Security.Accounts;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Sitecore.Script.Security
{
    public class ScriptAuthorizationProvider : SqlServerAuthorizationProvider
	{
		private List<string> limitedAccessRightsList;
		
		private readonly List<string> defaultLimitedAccessRights = new List<string>
		{
			"field:write",
			"item:write",
			"item:rename",
			"item:create",
			"item:delete",
			"item:admin",
			"language:write",
			"workflowState:write"
		};

		public ScriptAuthorizationProvider()
		{
			this.limitedAccessRightsList = new List<string>();
		}

		public override void Initialize(string name, NameValueCollection config)
		{
			base.Initialize(name, config);
			var limitedAccessRights = config["limitedAccessRights"];
			if (!string.IsNullOrWhiteSpace(limitedAccessRights))
			{
				var accessRights = limitedAccessRights.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
				limitedAccessRightsList.AddRange(accessRights);
			}
			else
			{
				// Optional: if you want to define default access rights, when configuration does not provide them
				// limitedAccessRightsList.AddRange(defaultLimitedAccessRights);
			}
		}

		public override AccessResult GetAccess(ISecurable entity, Account account, AccessRight accessRight)
		{
			AccessResult accessResult = this.GetStateAccess(entity, account, accessRight);
			if (accessResult != null)
			{
				return accessResult;
			}
			// we skip checking cache, and read directly from database
			accessResult = this.GetAccessCore(entity, account, accessRight);
			// we skip also updating cache
			return accessResult;
		}

		protected override AccessResult GetAccessCore(ISecurable entity, Account account, AccessRight accessRight)
		{
			if (ScriptStateSwitcher.CurrentValue == ScriptSecurityState.Limited)
			{
				AccessResult itemAccessResult = null;

				if (limitedAccessRightsList.Any(ar => ar.ToLowerInvariant() == accessRight.Name.ToLowerInvariant()))
				{
					var accessExplanationText = string.Format("Access right {0} is denied for Sitecore Script module, requested by {1}.", accessRight.Name, account.Name);
					var deniedItemAccessExplanation = new AccessExplanation(accessExplanationText, entity);
					itemAccessResult = new AccessResult(AccessPermission.Deny, deniedItemAccessExplanation);
					Sitecore.Diagnostics.Log.Debug(accessExplanationText, this);
				}
				else
				{
					var itemAccessExplanation = new AccessExplanation(string.Format("{0} access right granted for Sitecore Script module.", accessRight.Name, entity));
					itemAccessResult = new AccessResult(AccessPermission.Allow, itemAccessExplanation);
				}

				return itemAccessResult;
			}
			else
			{
				// for non-Sitecore Script authorizations
				return base.GetAccessCore(entity, account, accessRight);
			}
		}
	}
}