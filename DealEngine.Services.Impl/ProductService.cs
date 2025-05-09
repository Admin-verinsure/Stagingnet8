using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using System.Linq.Dynamic.Core;

namespace DealEngine.Services.Impl
{
    public class ProductService : IProductService
    {        
        IMapperSession<Product> _productRepository;

        public ProductService(
            IMapperSession<Product> productRepository
            )
        {
            _productRepository = productRepository;
        }

        public async Task CreateProduct(Product product)
        {
            await _productRepository.AddAsync(product);
        }

        public async Task<List<Product>> GetAllProducts()
        {
            var productList = new List<Product>();
            var allProducts = await _productRepository.FindAll().ToListAsync();
            foreach (var product in allProducts)
            {
                var isBaseClass = await IsBaseClass(product);
                if (isBaseClass)
                {
                    productList.Add(product);
                }
            }
            return productList;
        }

        private async Task<bool> IsBaseClass(Product product)
        {
            var objectType = product.GetType();
            if (!objectType.IsSubclassOf(typeof(Product)))
            {
                return true;
            }
            return false;
        }

        public async Task<Product> GetProductById(Guid Id)
        {
            return await _productRepository.GetByIdAsync(Id);
        }

        public async Task UpdateProduct(Product product)
        {
            await _productRepository.AddAsync(product);
        }
    }
}
