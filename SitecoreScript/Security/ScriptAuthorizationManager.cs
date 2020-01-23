using System;
using System.Linq;
using Sitecore.Abstractions;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Security.AccessControl;
using Sitecore.Security.Accounts;

namespace Sitecore.Script.Security
{
	/// <summary>
	/// Sitecore Script authorization manager.
	/// </summary>
	public class ScriptAuthorizationManager : BaseAuthorizationManager
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sitecore.Script.Security.AuthorizationManager" /> class.
		/// </summary>
		/// <param name="providerHelper">
		/// The provider helper.
		/// </param>
		public ScriptAuthorizationManager(ProviderHelper<AuthorizationProvider, AuthorizationProviderCollection> providerHelper)
		{
			this.defaultProvider = providerHelper.Provider;
			this.scriptProvider = providerHelper.Providers["sitecore-script"] ?? providerHelper.Provider;
		}

		/// <summary>
		/// Determines the kind of access control that is associated with the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="account">The account.</param>
		/// <param name="accessRight">The access right.</param>
		/// <returns>The access control that is associated with the entity.</returns>
		public override AccessResult GetAccess(ISecurable entity, Account account, AccessRight accessRight)
		{
			Assert.ArgumentNotNull(entity, "entity");
			Assert.ArgumentNotNull(account, "account");
			Assert.ArgumentNotNull(accessRight, "accessRight");
			AccessResult access = this.AuthorizationProvider.GetAccess(entity, account, accessRight);
			this.UpdateAccessCounters(access.Permission);
			return access;
		}

		/// <summary>
		/// Gets the access rules for an entity.
		/// </summary>
		/// <param name="entity">The object representing the entity.</param>
		/// <returns>The list of rules. This is guaranteed never to be <c>null</c>.</returns>
		public override AccessRuleCollection GetAccessRules(ISecurable entity)
		{
			AccessRuleCollection accessRules = this.AuthorizationProvider.GetAccessRules(entity);
			if (accessRules != null)
			{
				return accessRules;
			}
			return new AccessRuleCollection();
		}

		/// <summary>
		/// Determines whether a specific operation on the specified entity is allowed.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="right">The operation.</param>
		/// <param name="account">The account.</param>
		/// <returns>
		///   <c>true</c> if the operation is allowed; otherwise, <c>false</c>.
		/// </returns>
		public override bool IsAllowed(ISecurable entity, AccessRight right, Account account)
		{
			Assert.ArgumentNotNull(entity, "entity");
			Assert.ArgumentNotNull(right, "right");
			Assert.ArgumentNotNull(account, "account");
			AccessResult access = this.GetAccess(entity, account, right);
			if (right.IsFieldRight)
			{
				return access.Permission != AccessPermission.Deny;
			}
			return access.Permission == AccessPermission.Allow;
		}

		/// <summary>
		/// Determines whether the specified operation on an entity is denied.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="right">The operation.</param>
		/// <param name="account">The account.</param>
		/// <returns>
		///   <c>true</c> if the specified operation is denied; otherwise, <c>false</c>.
		/// </returns>
		public override bool IsDenied(ISecurable entity, AccessRight right, Account account)
		{
			Assert.ArgumentNotNull(entity, "entity");
			Assert.ArgumentNotNull(right, "right");
			Assert.ArgumentNotNull(account, "account");
			AccessResult access = this.GetAccess(entity, account, right);
			if (right.IsFieldRight)
			{
				return access.Permission == AccessPermission.Deny;
			}
			return access.Permission != AccessPermission.Allow;
		}

		/// <summary>
		/// Removes all access rules from a specific entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		public override void RemoveAccessRules(ISecurable entity)
		{
			this.AuthorizationProvider.SetAccessRules(entity, new AccessRuleCollection());
		}

		/// <summary>
		/// Sets the access rules for an entity.
		/// </summary>
		/// <param name="entity">The object representing the entity.</param>
		/// <param name="rules">The rules.</param>
		public override void SetAccessRules(ISecurable entity, AccessRuleCollection rules)
		{
			this.AuthorizationProvider.SetAccessRules(entity, rules);
		}

		/// <summary>
		/// Updates the access counters.
		/// </summary>
		/// <param name="access">The access.</param>
		private void UpdateAccessCounters(AccessPermission access)
		{
			if (access == AccessPermission.Allow)
			{
				SecurityCount.AccessGranted.Increment(1L);
			}
			if (access == AccessPermission.Deny)
			{
				SecurityCount.AccessDenied.Increment(1L);
			}
			SecurityCount.AccessResolved.Increment(1L);
		}

		private AuthorizationProvider defaultProvider;
		private AuthorizationProvider scriptProvider;

		/// <summary>
		/// Default authorization provider.
		/// </summary>
		private AuthorizationProvider AuthorizationProvider
		{
			get
			{
				if (ScriptStateSwitcher.CurrentValue == ScriptSecurityState.Limited)
					return this.scriptProvider;
				
				return this.defaultProvider;
			}
		}
	}
}
