using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;

namespace DealEngine.Infrastructure.Payment.EGlobalAPI.BaseClasses
{
    //package product
    public class EGlobalPolicyRiskConfig
    {        
        public Guid PolicyID { get; set; }
        public Guid ID { get; set; }
        public Guid ClassOfBusinessID { get; set; }
        public string RiskCode { get; set; }
        public string SubCover { get; set; }
        public Guid MergeWithCOBID { get; set; }
        public bool AlwaysInclude { get; set; }
        public IList<EGlobalInsurerConfig> Insurers { get; set; }
        public IList<EGlobalSubAgent> SubAgents { get; set; }
        public PackageProduct PackageProduct { get; set; }

        public EGlobalPolicyRiskConfig(PackageProduct _packageProduct)
        {
            PackageProduct = _packageProduct;
            Load();
            LoadInsurers();
            if (PackageProduct.PackageProductDefaultSubAgent != null)
            {
                LoadSubAgents();
            }
        }

        void Load()
        {
            ID = PackageProduct.Id;
            PolicyID = PackageProduct.PackageProductPackage.Id;
            ClassOfBusinessID = PackageProduct.PackageProductProduct.Id;
            RiskCode = PackageProduct.PackageProductRiskCode;
            SubCover = PackageProduct.PackageProductSubCover;
            MergeWithCOBID = PackageProduct.PackageProductPackage.Id; //PackageProduct.PackageProductMergeProduct.Id currently value is null and crashing
            AlwaysInclude = PackageProduct.PackageProductAlwaysInclude;

        }

        void LoadInsurers()
        {
            Insurers = new List<EGlobalInsurerConfig>();
            foreach(PackageProductInsurer packageProductInsurer in PackageProduct.PackageProductInsurers)
            {
                Insurers.Add(new EGlobalInsurerConfig(packageProductInsurer));
            }
        }

        void LoadSubAgents()
        {
            SubAgents = new List<EGlobalSubAgent>
            {
                new EGlobalSubAgent(PackageProduct.PackageProductDefaultSubAgent)
            };
        }
    }
}

