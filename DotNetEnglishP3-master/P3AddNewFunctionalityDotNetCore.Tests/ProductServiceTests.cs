using Xunit;
using System.Collections.Generic;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using Microsoft.Extensions.Localization;
using Moq;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {
        // Real ProductService with mocked constructor dependencies.
        private static ProductService CreateProductService()
        {
            var cart = new Mock<ICart>();
            var productRepository = new Mock<IProductRepository>();
            var orderRepository = new Mock<IOrderRepository>();
            var localizer = new Mock<IStringLocalizer<ProductService>>();

            return new ProductService(
                cart.Object, productRepository.Object, orderRepository.Object, localizer.Object);
        }

        [Fact]
        public void EmptyName_ReturnsMissingName()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "", Price = "10", Stock = "5" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Contains("MissingName", errors);
        }

        [Fact]
        public void EmptyPrice_ReturnsMissingPrice()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "", Stock = "5" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Contains("MissingPrice", errors);
        }

        [Fact]
        public void NonNumericPrice_ReturnsPriceNotANumber()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "abc", Stock = "5" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Contains("PriceNotANumber", errors);
        }

        [Fact]
        public void ZeroPrice_ReturnsPriceNotGreaterThanZero()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "0", Stock = "5" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Contains("PriceNotGreaterThanZero", errors);
        }

        [Fact]
        public void EmptyStock_ReturnsMissingQuantity()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Contains("MissingQuantity", errors);
        }

        [Fact]
        public void DecimalStock_ReturnsStockNotAnInteger()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "1.5" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Contains("StockNotAnInteger", errors);
        }

        [Fact]
        public void ZeroStock_ReturnsStockNotGreaterThanZero()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "0" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Contains("StockNotGreaterThanZero", errors);
        }

        [Fact]
        public void ValidProduct_ReturnsNoErrors()
        {
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "5" };

            List<string> errors = productService.CheckProductModelErrors(product);

            Assert.Empty(errors);
        }
    }
}
