using ElmahCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Models;
using DealEngine.WebUI.Models.ProductModels;
using DealEngine.Infrastructure.FluentNHibernate;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace DealEngine.WebUI.Controllers
{
	[Authorize]
	public class ProductController : BaseController
	{		
		IInformationTemplateService _informationService;        		
		IProductService _productService;
        ITerritoryService _territoryService;
		IRiskCategoryService _riskCategoryService;
		IRiskCoverService _riskCoverService;		
		IOrganisationService _organisationService;
		IMapperSession<Document> _documentRepository;
        IProgrammeService _programmeService;
		IApplicationLoggingService _applicationLoggingService;
		ILogger<ProductController> _logger;

        public ProductController(
			ILogger<ProductController> logger,
			IApplicationLoggingService applicationLoggingService,
			IRiskCategoryService riskCategoryService,
			IRiskCoverService riskCoverService,
			IProductService productService,
			IUserService userRepository, 
			IInformationTemplateService informationService, 							
			ITerritoryService territoryService, 						
			IOrganisationService organisationService,
			IMapperSession<Document> documentRepository, 
			IProgrammeService programmeService
			)
			: base (userRepository)
		{
			_logger = logger;
			_applicationLoggingService = applicationLoggingService;
			_riskCoverService = riskCoverService;
			_riskCategoryService = riskCategoryService;
			_informationService = informationService;
			_productService = productService;
            _territoryService = territoryService;            			
			_organisationService = organisationService;
			_documentRepository = documentRepository;
            _programmeService = programmeService;
		}

		[HttpGet]
        public async Task<IActionResult> MyProducts()
        {
			User user = null;
			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				var productList = await _productService.GetAllProducts();
				var products = productList.Where(p => p.OwnerCompany == user.PrimaryOrganisation.Id);
				BaseListViewModel<ProductInfoViewModel> models = new BaseListViewModel<ProductInfoViewModel>();
				foreach (Product p in products)
				{
					var company = await _organisationService.GetOrganisation(p.CreatorCompany);
					ProductInfoViewModel model = new ProductInfoViewModel
					{
						DateCreated = LocalizeTime(p.DateCreated.GetValueOrDefault()),
						Id = p.Id,
						Name = p.Name,
						OwnerCompany = company.Name,
						SelectedLanguages = p.Languages
					};
					models.Add(model);
				}
				return View("AllProducts", models);
			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return View("AllProducts");
			}			
        }

		[HttpGet]
		public async Task<IActionResult> AllProducts ()
		{
			User user = null;
			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				var productList = await _productService.GetAllProducts();
				var products = productList.Where(p => p.Public);
				BaseListViewModel<ProductInfoViewModel> models = new BaseListViewModel<ProductInfoViewModel> ();
				foreach (Product p in products) {
                    var creatorCompany = await _organisationService.GetOrganisation(p.CreatorCompany);
                    ProductInfoViewModel model = new ProductInfoViewModel {
						DateCreated = LocalizeTime (p.DateCreated.GetValueOrDefault ()),
						Id = p.Id,
						Name = p.Name,
						OwnerCompany = creatorCompany.Name,
						SelectedLanguages = p.Languages
					};
					models.Add (model);
				}
				return View (models);
			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return View();
			}			
		}

		// Proposal Element,
		// Premium Element,
        // Policy Element
		// Can not create a product without different insurance elements existing
		// Can only map and not add new elements
		[HttpGet]
		public async Task<IActionResult> CreateProduct()
		{
			User user = null;
			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				ProductViewModel model = new ProductViewModel();
				model.Description = new ProductDescriptionVM
				{
					CreatorOrganisation = user.PrimaryOrganisation.Id,
					OwnerOrganisation = user.PrimaryOrganisation.Id,
					// TODO - load this from db
					Languages = new List<SelectListItem> {
					new SelectListItem { Text = "English (NZ)", Value = "nz" },
					new SelectListItem { Text = "English (US)", Value = "uk" },
					new SelectListItem { Text = "English (UK)", Value = "us" },
					new SelectListItem { Text = "German", Value = "de" },
					new SelectListItem { Text = "French", Value = "fr" },
					new SelectListItem { Text = "Chinese", Value = "cn" }
				},
					BaseProducts = new List<SelectListItem> { new SelectListItem { Text = "Select Base Product", Value = "" } }
				};

				model.Description.BaseProducts.Add(new SelectListItem { Text = "Set as base product", Value = Guid.Empty.ToString() });

				var productList = await _productService.GetAllProducts();
				foreach (Product product in productList.Where(p => p.IsMasterProduct))
				{
					model.Description.BaseProducts.Add(new SelectListItem { Text = product.Name, Value = product.Id.ToString() });
				}

				var riskList = await _riskCategoryService.GetAllRiskCategories();
				foreach (RiskCategory risk in riskList)
					model.Risks.Add(new RiskEntityViewModel { Insured = risk.Name, Id = risk.Id, CoverAll = false, CoverLoss = false, CoverInterruption = false, CoverThirdParty = false });

				// set product settings
				foreach (Document doc in _documentRepository.FindAll().Where(d => d.OwnerOrganisation == user.PrimaryOrganisation))
					model.Settings.Documents.Add(new SelectListItem { Text = doc.Name, Value = doc.Id.ToString() });

				model.Settings.PossibleOwnerOrganisations.Add(new SelectListItem { Text = "Select Product Owner", Value = "" });
				model.Settings.PossibleOwnerOrganisations.Add(new SelectListItem { Text = user.PrimaryOrganisation.Name, Value = user.PrimaryOrganisation.Id.ToString() });
				// loop over all non personal organisations and add them, excluding our own since its already added
				var orgList = await _organisationService.GetAllOrganisations();
				foreach (Organisation org in orgList.Where(org => org.OrganisationType.Name != "personal").OrderBy(o => o.Name))
					if (org.Id.ToString() != model.Settings.PossibleOwnerOrganisations[1].Value)
						model.Settings.PossibleOwnerOrganisations.Add(new SelectListItem { Text = org.Name, Value = org.Id.ToString() });

				var programmes = new List<Programme>();
				var programmeList = await _programmeService.GetAllProgrammes();
				foreach (Programme programme in programmeList)
					model.Settings.InsuranceProgrammes.Add(
						new SelectListItem
						{
							Text = programme.Name,
							Value = programme.Id.ToString()
						}
					);

				model.Parties = new ProductPartiesVM
				{
					Brokers = new List<SelectListItem>(),
					Insurers = new List<SelectListItem>()
				};

				return View(model);
			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}

		[HttpGet]
		public async Task<IActionResult> CreateRole()
		{
			RoleViewModel model = new RoleViewModel();
			User user = null;

			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				model.Builder = new RoleBuilderVM();

				List<SelectListItem> proglist = new List<SelectListItem>();
				var progList = await _programmeService.GetProgrammesByOwner(user.PrimaryOrganisation.Id);
				if (user.PrimaryOrganisation.IsTC)
				{
					progList = await _programmeService.GetAllProgrammes();
				}
				foreach (Programme programme in progList)
				{
					proglist.Add(new SelectListItem
					{
						Selected = false,
						Text = programme.Name,
						Value = programme.Id.ToString(),
					});

				}

				model.RoleAttach = new RoleAttachVM
				{
					BaseProgList = proglist
				};

				return View("RoleBuilder", model);
			}
			catch (Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}

		[HttpPost]
		public async Task<IActionResult> CreateRoleForProgramme(string Role, string ProgrammeId)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("", "Form has not been completed");
				throw new Exception("Form has not been completed");
			}

			User user = null;
			try
			{
				user = await CurrentUser();
				var programme = await _programmeService.GetProgrammeById(Guid.Parse(ProgrammeId));

				SharedDataRoleTemplate template = new SharedDataRoleTemplate();
				template.Name = Role;
				await _programmeService.AttachProgrammeToDataRole(programme, template);

				return Ok();
			}
			catch (Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}

		[HttpGet]
        public async Task<IActionResult> CreateTerritory()
        {
            TerritoryViewModel model = new TerritoryViewModel();
			User user = null;

			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				model.Builder = new TerritoryBuilderVM
				{
					Location = "",
					Zoneorder = 0,
					Ispublic = false,					
					BaseExclIncl = new List<SelectListItem> {
						new SelectListItem { Text = "Inclusion", Value = "Incl" },
						new SelectListItem { Text = "Exclusion", Value = "Excl" },
					}
				};

				List<SelectListItem> proglist = new List<SelectListItem>();
				var progList = await _programmeService.GetProgrammesByOwner(user.PrimaryOrganisation.Id);
				if (user.PrimaryOrganisation.IsTC)
				{
					progList = await _programmeService.GetAllProgrammes();
				}
				foreach (Programme programme in progList)
				{
					proglist.Add(new SelectListItem
					{
						Selected = false,
						Text = programme.Name,
						Value = programme.Id.ToString(),
					});

				}

				model.TerritoryAttach = new TerritoryAttachVM
				{
					BaseProgList = proglist
				};

				return View("TerritoryBuilder", model);
			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}


        [HttpPost]
        public async Task<IActionResult> CreateTerritoryForProgramme(string Location, string IncluExclu, int ZoneNo, bool IsPublic, string ProgrammeId)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Form has not been completed");
                throw new Exception("Form has not been completed");
            }

			User user = null;
            try
            {
                user = await CurrentUser();
                var programme = await _programmeService.GetProgrammeById(Guid.Parse(ProgrammeId));

                TerritoryTemplate territoryTemplate = new TerritoryTemplate(user, Location)
                {
                    Ispublic = IsPublic,
                    Zoneorder = ZoneNo,
                    ExclorIncl = IncluExclu,                    
                };

                var territoryNZ = await _territoryService.GetTerritoryTemplateByName("New Zealand");
                await _territoryService.AddTerritoryTemplate(territoryTemplate);
                await _programmeService.AttachProgrammeToTerritory(programme, territoryTemplate);

                return RedirectToAction("Index", "Home");
            }
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}


        [HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateProduct(ProductViewModel model)
		{
			if (!ModelState.IsValid) {
				ModelState.AddModelError ("", "Form has not been completed");
				throw new Exception ("Form has not been completed");
			}

			Programme programme = null;
			User user = null;
			try {
                user = await CurrentUser();
				Product baseProduct = null;
				Guid baseProductId = Guid.Empty;
				if (Guid.TryParse (model.Description.SelectedBaseProduct, out baseProductId))
					baseProduct = await _productService.GetProductById(baseProductId);

				Guid ownerCompanyGuid = Guid.Empty;
				if (!Guid.TryParse (model.Settings.SelectedOwnerOrganisation, out ownerCompanyGuid))
					throw new Exception ("Invalid owner organisation id: " + model.Settings.SelectedOwnerOrganisation);

				Product product = new Product (user, model.Description.CreatorOrganisation, model.Description.Name) {
					Description = model.Description.Description,
					OwnerCompany = ownerCompanyGuid,
					Languages = new List<string> (model.Description.SelectedLanguages),
					OriginalProductId = baseProductId,
					Public = model.Description.Public,
					IsBaseProduct = (baseProductId == Guid.Empty)
				};

				foreach (RiskEntityViewModel risk in model.Risks) {
					RiskCover cover = new RiskCover (user) {
						BaseRisk = await _riskCategoryService.GetRiskCategoryById(risk.Id),
						CoverAll = risk.CoverAll,
						Interuption = risk.CoverInterruption,
						Loss = risk.CoverLoss,
						ThirdParty = risk.CoverThirdParty
					};
					if (risk.CoverAll)
						cover.SelectAll ();
					product.RiskCategoriesCovered.Add (cover);
				}

				product.Documents = new List<Document> ();
				if (model.Settings != null && model.Settings.SelectedDocuments != null) {
					foreach (string sid in model.Settings.SelectedDocuments) {
						Guid id = Guid.Empty;
						if (Guid.TryParse (sid, out id))
							product.Documents.Add (await _documentRepository.GetByIdAsync(id));
					}
				}

				if (!string.IsNullOrEmpty (model.Settings.SelectedInsuranceProgramme)) {
                    Guid programmeId = Guid.Empty;
                    if (Guid.TryParse(model.Settings.SelectedInsuranceProgramme, out programmeId))
                    {
                        programme = await _programmeService.GetProgrammeById(programmeId);                        
                    }
				}
				else
				{
					var programmeList = await _programmeService.GetAllProgrammes();
					programme = programmeList.LastOrDefault();
				}
				programme.Products.Add(product);

				if (baseProduct != null)
                    baseProduct.ChildProducts.Add(product);

                await _productService.CreateProduct(product);

				return NoContent();

			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}

		[HttpGet]
		public async Task<IActionResult> ViewProduct (Guid id)
		{
			ProductViewModel model = new ProductViewModel ();
			User user = null;

			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				Product product = await _productService.GetProductById(id);
				if (product != null)
				{
					model.Description = new ProductDescriptionVM
					{
						DateCreated = LocalizeTime(product.DateCreated.GetValueOrDefault()),
						Description = product.Description,
						Name = product.Name,
						SelectedLanguages = product.Languages.ToArray(),
						Public = product.Public
					};
					model.Risks = new ProductRisksVM();
					foreach (RiskCover risk in product.RiskCategoriesCovered)
						model.Risks.Add(new RiskEntityViewModel
						{
							Insured = risk.BaseRisk.Name,
							CoverAll = risk.CoverAll,
							CoverLoss = risk.Loss,
							CoverInterruption = risk.Interuption,
							CoverThirdParty = risk.ThirdParty
						});
				}
				return View(model);
			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}

		[HttpGet]
		public async Task<IActionResult> CloneProduct (Guid id)
		{
			User user = null;
			ProductViewModel model = new ProductViewModel();

			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				Product originalProduct = await _productService.GetProductById(id);
				if (originalProduct != null)
				{
					model.Description = new ProductDescriptionVM
					{
						OriginalProductId = originalProduct.Id,
						CreatorOrganisation = user.PrimaryOrganisation.Id,
						OwnerOrganisation = user.PrimaryOrganisation.Id,
						Name = originalProduct.Name,
						Description = originalProduct.Description,
						SelectedLanguages = originalProduct.Languages.ToArray(),
						// TODO - load this from db
						Languages = new List<SelectListItem> {
						new SelectListItem { Text = "English (NZ)", Value = "nz" },
						new SelectListItem { Text = "English (US)", Value = "uk" },
						new SelectListItem { Text = "English (UK)", Value = "us" },
						new SelectListItem { Text = "German", Value = "de" },
						new SelectListItem { Text = "French", Value = "fr" },
						new SelectListItem { Text = "Chinese", Value = "cn" }
					},
						BaseProducts = new List<SelectListItem> { new SelectListItem { Text = "Select Base Product", Value = "" } },
						SelectedBaseProduct = id.ToString(),
						IsBaseProduct = originalProduct.IsBaseProduct
					};

					var productList = await _productService.GetAllProducts();
					foreach (Product product in productList.Where(p => p.IsBaseProduct))
					{
						model.Description.BaseProducts.Add(new SelectListItem { Text = product.Name, Value = product.Id.ToString() });
					}

					model.Risks = new ProductRisksVM();
					var riskCategoryList = await _riskCategoryService.GetAllRiskCategories();
					foreach (RiskCategory risk in riskCategoryList)
					{
						RiskCover productRisk = originalProduct.RiskCategoriesCovered.FirstOrDefault(r => r.BaseRisk == risk);
						if (productRisk == null)
							productRisk = new RiskCover(user) { CoverAll = false, Loss = false, Interuption = false, ThirdParty = false };
						model.Risks.Add(
							new RiskEntityViewModel
							{
								Insured = risk.Name,
								Id = risk.Id,
								CoverAll = productRisk.CoverAll,
								CoverLoss = productRisk.Loss,
								CoverInterruption = productRisk.Interuption,
								CoverThirdParty = productRisk.ThirdParty
							});
					}

					model.Settings = new ProductSettingsVM();
					model.Settings.Documents = new List<SelectListItem>();
					foreach (Document doc in _documentRepository.FindAll().Where(d => d.OwnerOrganisation == user.PrimaryOrganisation))
						model.Settings.Documents.Add(new SelectListItem { Text = doc.Name, Value = doc.Id.ToString() });

					model.Settings.InformationSheets = new List<SelectListItem>();
					var templates = await _informationService.GetAllTemplates();
					foreach (var template in templates)
						model.Settings.InformationSheets.Add(
							new SelectListItem
							{
								Text = template.Name,
								Value = template.Id.ToString()
							}
						);

					model.Settings.PossibleOwnerOrganisations.Add(new SelectListItem { Text = user.PrimaryOrganisation.Name, Value = user.PrimaryOrganisation.Id.ToString() });
					// loop over all non personal organisations and add them, excluding our own since its already added
					var orgList = await _organisationService.GetAllOrganisations();
					foreach (Organisation org in orgList.Where(org => org.OrganisationType.Name != "personal").OrderByDescending(o => o.Name))
						if (org.Id.ToString() != model.Settings.PossibleOwnerOrganisations[0].Value)
							model.Settings.PossibleOwnerOrganisations.Add(new SelectListItem { Text = org.Name, Value = org.Id.ToString() });

					model.Parties = new ProductPartiesVM
					{
						Brokers = new List<SelectListItem>(),
						Insurers = new List<SelectListItem>()
					};
				}
				return View("CreateNew", model);
			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}

		[HttpPost]
		public async Task<IActionResult> CloneProduct (ProductViewModel model)
		{
			return await CreateProduct(model);
		}

		[HttpGet]
		public async Task<IActionResult> FindProducts ()
		{
			ProductRisksVM model = new ProductRisksVM ();
			User user = null;

			try
			{
				user = await CurrentUser();
				if (user.IsLoggedout)
					return PageNotFound();

				if (user == null)
					return PageNotFound();

				var riskCategoryList = await _riskCategoryService.GetAllRiskCategories();
				foreach (RiskCategory risk in riskCategoryList)
					model.Add(new RiskEntityViewModel { Insured = risk.Name, Id = risk.Id, CoverAll = false, CoverLoss = false, CoverInterruption = false, CoverThirdParty = false });

				return View(model);
			}
			catch(Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}

		[HttpPost]
		public async Task<IActionResult> FindProducts (ProductRisksVM model)
		{
			BaseListViewModel<ProductInfoViewModel> models = new BaseListViewModel<ProductInfoViewModel> ();
			User user = null;

			try
			{
				user = await CurrentUser();
				var riskCoverList = await _riskCoverService.GetAllRiskCovers();
				foreach (var m in model)
				{
					var covers = riskCoverList.Where(rc => rc.BaseRisk.Id == m.Id &&
														  rc.CoverAll == m.CoverAll &&
														  rc.Interuption == m.CoverInterruption &&
														  rc.Loss == m.CoverLoss &&
														  rc.ThirdParty == m.CoverThirdParty);

					foreach (var r in covers)
					{
						Product p = r.Product;
						var creatorCompany = await _organisationService.GetOrganisation(p.CreatorCompany);
						ProductInfoViewModel vm = new ProductInfoViewModel
						{
							DateCreated = LocalizeTime(p.DateCreated.GetValueOrDefault()),
							Id = p.Id,
							Name = p.Name,
							OwnerCompany = creatorCompany.Name,
							SelectedLanguages = p.Languages
						};
						models.Add(vm);
					}
				}

				return View("AllProducts", models);

			}
			catch (Exception ex)
			{
				await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
				return RedirectToAction("Error500", "Error");
			}
		}
	}
}
