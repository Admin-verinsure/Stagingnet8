using System;
using System.Collections.Generic;
using Bismuth.Ldap;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.Ldap.Interfaces;

namespace DealEngine.Infrastructure.Ldap.Mapping
{
	public class OrganisationMapping : BaseEntityMapping, ILdapEntityMapping<Organisation>
	{
		public Organisation FromLdap (LdapEntry entry)
		{
			Guid id = Guid.Parse (entry.GetAttributeValue ("o"));					// Need to swap this to use 'uniqueIdentifier'
			string orgName = entry.GetAttributeValue ("buildingname");              // Need to swap this to use 'o'

            Organisation organisation = new Organisation(null, id, orgName)
            {
                Domain = entry.GetAttributeValue("associateddomain"),
                Phone = entry.GetAttributeValue("telephonenumber"),
                Description = entry.GetAttributeValue("description")
            };
            string organisationType = entry.GetAttributeValue ("businesscategory");

			return organisation;
		}

		public Organisation FromLdapforupload(LdapEntry entry)
		{
			Guid id = Guid.Parse(entry.GetAttributeValue("o"));                 // Need to swap this to use 'uniqueIdentifier'
			string orgName = entry.GetAttributeValue("buildingname");              // Need to swap this to use 'o'

			Organisation organisation = new Organisation(null, id, orgName)
			{
				Domain = entry.GetAttributeValue("associateddomain"),
				Phone = entry.GetAttributeValue("telephonenumber"),
				Description = entry.GetAttributeValue("description")
			};
			string organisationType = entry.GetAttributeValue("businesscategory");

			return organisation;
		}
		//TEntity FromLdapforupload(LdapEntry entry);

		public string GetDn (Organisation entity, string baseDn)
		{
			return "o=" + entity.Id + ",ou=organisations," + baseDn;
		}

		public LdapEntry ToLdap (Organisation entity, string baseDn)
		{
			LdapEntry entry = new LdapEntry (GetDn (entity, baseDn))
				.AddAttribute ("o", entity.Id.ToString ())
				.AddAttribute ("objectclass", "top", "pilotOrganization", "domainRelatedObject");
				//.AddAttribute ("uniqueIdentifier", entity.Id.ToString());       // Removed in RFC4524 schema

			AddNonNullAttribute (entry, "ou", "organisation");
			AddNonNullAttribute (entry, "buildingname", entity.Name);
			AddNonNullAttribute (entry, "description", entity.Description);
			AddNonNullAttribute (entry, "telephonenumber", entity.Phone);
			AddDefaultAttribute (entry, "associateddomain", entity.Domain, "#");
			if (entity.OrganisationType != null)
				AddNonNullAttribute (entry, "businesscategory", entity.OrganisationType.Name);


			return entry;
		}

        public LdapEntry ToLdapPassword(Organisation entity, string baseDn, string password)
        {
            LdapEntry entry = new LdapEntry(GetDn(entity, baseDn))
                .AddAttribute("o", entity.Id.ToString())
                .AddAttribute("objectclass", "top", "pilotOrganization", "domainRelatedObject");

            AddNonNullAttribute(entry, "ou", "organisation");
            AddNonNullAttribute(entry, "buildingname", entity.Name);
            AddNonNullAttribute(entry, "description", entity.Description);
            AddNonNullAttribute(entry, "telephonenumber", entity.Phone);
            AddDefaultAttribute(entry, "associateddomain", entity.Domain, "#");
            if (entity.OrganisationType != null)
                AddNonNullAttribute(entry, "businesscategory", entity.OrganisationType.Name);

            // Add the password attribute
            if (!string.IsNullOrWhiteSpace(password))
            {
                // Assuming the password is already hashed or in the correct format for LDAP
                entry.AddAttribute("userPassword", password);
            }

            return entry;
        }


        public List<ModifyAttribute> ToModify (Organisation entity)
		{
			var orgMods = new List<ModifyAttribute> ();
			AddNonNullAttribute (orgMods, "telephonenumber", ModificationType.Replace, entity.Phone);
			AddNonNullAttribute (orgMods, "description", ModificationType.Replace, entity.Description);
			AddNonNullAttribute (orgMods, "associateddomain", ModificationType.Replace, entity.Domain);
			if (entity.OrganisationType != null)
				AddNonNullAttribute (orgMods, "businesscategory", ModificationType.Replace, entity.OrganisationType.Name);
			return orgMods;
		}
	}
}

