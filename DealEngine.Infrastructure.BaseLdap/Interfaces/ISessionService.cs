using Novell.Directory.Ldap;

namespace DealEngine.Infrastructure.BaseLdap.Interfaces
{
    public interface ISessionService
    {
        LdapConnection GetConnection();
    }
}
