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
        /// <summary>
        /// Builds a real ProductService with mocked dependencies.
        /// Use of mocks to satisfy the constructor.
        /// </summary>
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
        public void ProductViewModel_FailsValidationWithMissingName()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "", Price = "10", Stock = "5" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Contains("MissingName", errors);
        }

        [Fact]
        public void ProductViewModel_FailsValidationWithMissingPrice()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "", Stock = "5" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Contains("MissingPrice", errors);
        }

        [Fact]
        public void ProductViewModel_FailsValidationWithPriceNotANumber()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "abc", Stock = "5" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Contains("PriceNotANumber", errors);
        }

        [Fact]
        public void ProductViewModel_FailsValidationWithPriceNotGreaterThanZero()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "0", Stock = "5" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Contains("PriceNotGreaterThanZero", errors);
        }

        [Fact]
        public void ProductViewModel_FailsValidationWithMissingQuantity()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Contains("MissingQuantity", errors);
        }

        [Fact]
        public void ProductViewModel_StockIsNotAnInteger_FailsValidationWithStockNotAnInteger()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "1.5" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Contains("StockNotAnInteger", errors);
        }

        [Fact]
        public void ProductViewModel_StockIsNotGreaterThanZero_FailsValidationWithStockNotGreaterThanZero()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "0" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Contains("StockNotGreaterThanZero", errors);
        }

        [Fact]
        public void ProductViewModel_AllFieldsValid_PassesValidation()
        {
            // Arrange
            var productService = CreateProductService();
            var product = new ProductViewModel { Name = "Pen", Price = "10", Stock = "5" };

            // Act
            List<string> errors = productService.CheckProductModelErrors(product);

            // Assert
            Assert.Empty(errors);
        }
    }
}
