using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities;
using DealEngine.Domain.Services;
using DealEngine.Services.Impl.UnderwritingModuleServices;
using DealEngine.Services.Interfaces;

namespace DealEngine.Services.Impl
{
    public class UWMService : IUWMService
    {

        IClientAgreementService _clientAgreementService;
        IClientAgreementRuleService _clientAgreementRuleService;
        IClientAgreementTermService _clientAgreementTermService;
        IClientAgreementMVTermService _clientAgreementMVTermService;
        IClientAgreementEndorsementService _clientAgreementEndorsementService;
        IUnderwritingModule _underwritingModule;

        public UWMService(
            IUnderwritingModule underwritingModule,
            IClientAgreementService clientAgreementService, 
            IClientAgreementRuleService clientAgreementRuleService,
            IClientAgreementTermService clientAgreementTermService,
            IClientAgreementMVTermService clientAgreementMVTermService,
            IClientAgreementEndorsementService clientAgreementEndorsementService)
        {
            _clientAgreementService = clientAgreementService;
            _clientAgreementRuleService = clientAgreementRuleService;
            _clientAgreementTermService = clientAgreementTermService;
            _clientAgreementMVTermService = clientAgreementMVTermService;
            _clientAgreementEndorsementService = clientAgreementEndorsementService;
            _underwritingModule = underwritingModule;
        }


        public bool UWM(User createdBy, ClientInformationSheet sheet, string reference)
        {
            var _modules = new Dictionary<string, IUnderwritingModule>();
            var modules = RegisterModules();
            bool result = false;
            string referenceId = reference;
            try
            {
                foreach (Product product in sheet.Programme.BaseProgramme.Products.OrderBy(t => t.OrderNumber))
                {
                    if (!product.UnderwritingEnabled)
                        continue;

                    //Mark the existing agreement for this product deleted
                    List<ClientAgreement> clientAgreements =   sheet.Programme.Agreements.Where(a => a.Product != null && a.Product.Name == product.Name).ToList();
                   
                    foreach(ClientAgreement clientagreement in clientAgreements)
                    {
                        if (clientagreement != null)
                            clientagreement.Delete(createdBy);
                    }
                    
                    

                    //Check if the cover is required
                    try
                    {
                        if(product.OptionalProductRequiredAnswer != null)
                        {
                            var val = sheet.Answers.Where(sa => sa.ItemName == product.OptionalProductRequiredAnswer).First().Value;

                        }

                        var requiredAnswer = sheet.Answers.FirstOrDefault(sa => sa.ItemName == product.OptionalProductRequiredAnswer);

                        if ((product.IsOptionalProduct || product.IsOptionalProductWithoutSelectOption)
                            && requiredAnswer != null
                            && requiredAnswer.Value != "1"){
                            if (product.IsOptionalProductBasedSub)
                            {
                                bool prodsubuiscoverrequired = false;
                                if (sheet.SubClientInformationSheets.Where(subuis => subuis.DateDeleted == null).Count() > 0)
                                {
                                    foreach (var prodsubuis in sheet.SubClientInformationSheets.Where(prossubuis => prossubuis.DateDeleted == null))
                                    {
                                        if (prodsubuis.Answers.Where(sa => sa.ItemName == product.OptionalProductRequiredAnswer).First().Value != "1" && !prodsubuiscoverrequired)
                                        {
                                            prodsubuiscoverrequired = true;
                                        }
                                    }
                                }
                                else
                                {
                                    prodsubuiscoverrequired = true;
                                }
                                if (prodsubuiscoverrequired)
                                {
                                    continue;
                                }
                            }
                            else
                            {

                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    if (!product.IsMasterProduct)
                    {
                        int.TryParse(referenceId, out int newReference);
                        referenceId = (newReference + 1).ToString();
                    }

                    string uwmCode = product.UnderwritingModuleCode;
                    if (string.IsNullOrWhiteSpace(uwmCode))
                        throw new Exception("No underwriting module specificed for product '" + product.Id + "'");
                   
                    var uwm = Load(uwmCode, _modules);
                    
                    try { 
                    result &= uwm.Underwrite(createdBy, sheet, product, referenceId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }

                }
            }catch(Exception ex){
                Console.WriteLine(ex.Message);

            }
			return result;
        }

        public void Register(string key, IUnderwritingModule module, Dictionary<string, IUnderwritingModule> _modules)
        {
            _modules[key] = module;
        }

        public IUnderwritingModule Load(string key, Dictionary<string, IUnderwritingModule> _modules)
        {           
            var modules = RegisterModules();
            foreach (var module in modules)
                Register(module.Name, module, _modules);

            if (!_modules.ContainsKey(key))
                throw new Exception("No underwriting module for \"" + key + "\" registered");

            return _modules[key];
        }

        protected IUnderwritingModule[] RegisterModules()
        {
            var modules = new IUnderwritingModule[] {
                new EmptyUWModule(),
                new RotaryASUWModule(),
                new RotaryGLUWModule(),
                new RotaryMDUWModule(),
                new RotaryREUWModule(),
                new RotaryMVUWModule(),
                new MREPIUWModule(),
                new MREMLUWModule(),
                new MREDOUWModule(),
                new MREEDUWModule(),
                new MRESLUWModule(),
                new MREELUWModule(),
                new MREPLUWModule(),
                new MREFIDUWModule(),
                new MRECEUWModule(),
                new MRECLUWModule(),
                new MLProgrammePLUWModule(),
                new RunOffProgrammePIUWModule(),
                new MarshCoastGuardUWModule(),
               
            };
            return modules;
        }

    }
    
}
