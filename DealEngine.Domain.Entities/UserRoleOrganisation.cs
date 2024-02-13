using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class UserRoleOrganisation : EntityBase, IAggregateRoot
    {       
        protected UserRoleOrganisation() : this(null) { }
        public UserRoleOrganisation(User createdBy) : base(createdBy) { }
        public virtual User User { get; set; }
        public virtual Organisation Organisation { get; set; }
        public virtual string RoleId { get; set; } // See aspnet_roles table
    }
}