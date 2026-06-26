using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    /// <summary>Real controller + service + repository against a throwaway SQL Server test DB.</summary>
    public class ProductIntegrationTests : IDisposable
    {
        // Dedicated test DB on the live server, but a different name so it can be dropped freely.
        private const string TestConnectionString =
            "Server=.;Database=P3Referential_Test;Trusted_Connection=True;" +
            "TrustServerCertificate=True;MultipleActiveResultSets=true";

        private readonly P3Referential _context;
        private readonly ProductService _service;
        private readonly ProductController _controller;
        private readonly Cart _cart;

        public ProductIntegrationTests()
        {
            // Feed the test connection string through IConfiguration, exactly like prod.
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:P3Referential"] = TestConnectionString
                })
                .Build();

            // Unconfigured options -> P3Referential.OnConfiguring runs the real UseSqlServer wiring.
            var options = new DbContextOptionsBuilder<P3Referential>().Options;

            _context = new P3Referential(options, config);

            // Fresh schema for every test.
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Real repository and service: nothing is mocked in the tested chain.
            var productRepository = new ProductRepository(_context);
            var orderRepository = new OrderRepository(_context);
            _cart = new Cart();

            _service = new ProductService(_cart, productRepository, orderRepository,
                Mock.Of<IStringLocalizer<ProductService>>());
            _controller = new ProductController(_service, Mock.Of<ILanguageService>());
        }

        // Drop the test DB after each test.
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Products shown on the client/user page.
        private IEnumerable<ProductViewModel> ClientProductList()
        {
            var result = _controller.Index() as ViewResult;
            return Assert.IsAssignableFrom<IEnumerable<ProductViewModel>>(result.Model);
        }

        [Fact]
        public void AddProduct_ShowsOnClientAndIsBuyable()
        {
            // Admin adds a product (service -> repository -> DB).
            _controller.Create(new ProductViewModel { Name = "Pen", Price = "10", Stock = "5" });

            // It shows up client-side with the right info.
            ProductViewModel shown = Assert.Single(ClientProductList());
            Assert.Equal("Pen", shown.Name);
            Assert.Equal("10", shown.Price);
            Assert.Equal("5", shown.Stock);

            // And it can be added to the cart.
            var product = _service.GetProductById(shown.Id);
            _cart.AddItem(product, 1);
            Assert.Contains(_cart.Lines, l => l.Product.Name == "Pen");
        }

        [Fact]
        public void DeleteProduct_RemovedFromClient()
        {
            _controller.Create(new ProductViewModel { Name = "Pen", Price = "10", Stock = "5" });
            int id = _service.GetAllProducts().Single().Id;

            // Visible before deletion.
            Assert.Contains(ClientProductList(), p => p.Id == id);

            // Admin deletes it.
            _controller.DeleteProduct(id);

            // Gone from the DB and from the client list.
            Assert.Empty(_service.GetAllProducts());
            Assert.DoesNotContain(ClientProductList(), p => p.Id == id);
        }

        [Fact]
        public void BuyProduct_ReducesStock()
        {
            _controller.Create(new ProductViewModel { Name = "Pen", Price = "10", Stock = "5" });
            var product = _service.GetAllProducts().Single();

            // Buy 2 via the real purchase flow (cart -> UpdateProductQuantities -> repository).
            _cart.AddItem(product, 2);
            _service.UpdateProductQuantities();

            // Client-side stock reflects 5 - 2 = 3.
            ProductViewModel shown = Assert.Single(ClientProductList());
            Assert.Equal("3", shown.Stock);
        }
    }
}
