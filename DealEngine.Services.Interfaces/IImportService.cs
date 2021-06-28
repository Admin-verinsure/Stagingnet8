using System;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IImportService
    {
        Task ImportAOEServiceIndividuals(User user);
        Task ImportAOEServicePrincipals(User user);
        Task ImportAOEServiceContract(User user);
        Task ImportAOEServiceClaims(User user);
        Task ImportActivities(User user);
        Task ImportCEASServiceIndividuals(User user);
        Task ImportCEASServiceUpdateUsers(User user);
        Task ImportPMINZServiceIndividuals(User user);
        Task ImportCEASServiceClaims(User user);
        Task ImportCEASServiceContract(User user);
        Task ImportPMINZServiceContract(User user);
        Task ImportCEASServicePrincipals(User user);
        Task ImportPMINZServicePrincipals(User user);
        Task ImportPMINZServicePreRenewData(User user);
        Task ImportDANZServicePreRenewData(User user);
        Task ImportDANZServiceIndividuals(User user);
        Task ImportDANZServicePersonnel(User user);
        Task ImportDANZServiceClaims(User user);
        Task ImportNZFSGServicePI(User user);
        Task ImportNZFSGServicePINewCompany(User user);
        Task ImportNZFSGServicePINewAll(User user);
        Task ImportNZFSGServiceML(User user);
        Task ImportNZFSGServiceMLNewCompany(User user);
        Task ImportNZFSGServiceMLNewAll(User user);
        Task ImportNZFSGServiceRO(User user);
        Task ImportAAAServiceIndividuals(User user);
        Task ImportNZPIImportOwners(User user);
        Task ImportNZPIImportPlanners(User user);
        Task ImportAAAServicePreRenewData(User user);
        Task ImportNZPIImportContractors(User user);
        Task ImportNZPIServicePreRenewData(User user);
        Task ImportApolloImportOwners(User user);
        Task ImportApolloServicePreRenewData(User user);
        Task ImportApolloSetELDefaultVale(User user);
        Task ImportAbbottImportOwners(User user);
        Task ImportAbbottServicePreRenewData(User user);
        Task ImportFanzImportOwners(User user);
        Task ImportFANZImportML(User user);
        Task ImportFANZMLPreRenewData(User user);

    }
}
