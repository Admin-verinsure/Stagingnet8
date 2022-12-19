using Bismuth.Ldap;
using System.Collections.Generic;

namespace DealEngine.Infrastructure.Ldap.Interfaces
{
	public interface ILdapEntityMapping<TEntity>
	{
		TEntity FromLdap (LdapEntry entry);
		TEntity FromLdapforupload(LdapEntry entry);

		string GetDn (TEntity entity, string baseDn);
		LdapEntry ToLdap (TEntity entity, string baseDn);
		List<ModifyAttribute> ToModify (TEntity entity);
	}
}

