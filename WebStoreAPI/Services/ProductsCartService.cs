using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Scriban;
using WebStoreAPI.Exceptions;
using WebStoreAPI.Models;

namespace WebStoreAPI.Services
{
    public class ProductsCartService
    {
        private readonly EmailService _emailService;
        private readonly IApplicationContext _applicationDb;
        private readonly ICurrencyService _currencyService;
        private ProductsCart _productsCart;

        public ProductsCartService(IApplicationContext productsContext,
            EmailService emailService, ICurrencyService currencyService)
        {
            _applicationDb = productsContext;
            _emailService = emailService;
            _currencyService = currencyService;
        }

        private void InitializeProductsCart()
        {
            _productsCart = _applicationDb.ProductsCarts.Single(x => x.UserId == User.Id);

            _productsCart.ProductsInCart = _applicationDb.ProductsInCarts
                .Include(x => x.Product)
                .Include(x => x.Product.Store)
                .Include(x => x.Product.Reviews)
                .Include(x => x.ProductsCart)
                .Where(x => x.ProductsCart.Id == _productsCart.Id)
                .ToList();
        }
        
        public User User { private get; set; }

        private void SetRegionalCost(ProductViewModel products)
        {
            products.Сurrency = User.RegionalCurrency;
            products.Cost = _currencyService.ConvertCurrency(products.Cost, User.RegionalCurrency);
        }

        private void SetRegionalCost(IEnumerable<ProductViewModel> products)
        {
            foreach (var product in products)
            {
                SetRegionalCost(product);
            }
        }

        public ProductsCartViewModel Get()
        {
            InitializeProductsCart();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductsCart, ProductsCartViewModel>();
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productsCartViewModel = mapper.Map<ProductsCart, ProductsCartViewModel>(_productsCart);
            SetRegionalCost(productsCartViewModel.ProductsInCart.Select(x => x.Product));

            return productsCartViewModel;
        }

        public ProductInCartViewModel AddProduct(long productId)
        {
            var product = _applicationDb.Products.Include(x => x.Store).Include(x => x.Reviews)
                .FirstOrDefault(x => x.Id == productId);

            if (product == null || product.QuantityInStock < 1)
                throw new NotFoundException("product not found or out of stock");

            InitializeProductsCart();
            var productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Product.Id == productId);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            if (productInCart != null)
            {
                productInCart.Count++;
                productInCart.Cost += product.Cost;
                _applicationDb.ProductsInCarts.Update(productInCart);
                _applicationDb.SaveChanges();
            }
            else
            {
                productInCart = new ProductInCart() {Cost = product.Cost, Product = product};
                _productsCart.ProductsInCart.Add(productInCart);
                _applicationDb.ProductsCarts.Update(_productsCart);
                _applicationDb.SaveChanges();
            }

            var productInCartViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
            SetRegionalCost(productInCartViewModel.Product);

            return productInCartViewModel;
        }

        public ProductInCartViewModel DeleteProduct(long productId)
        {
            InitializeProductsCart();

            var productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Product.Id == productId);
            if (productInCart == null)
                throw new NotFoundException("product not found");

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            if (productInCart.Count > 1)
            {
                productInCart.Count--;
                _applicationDb.ProductsInCarts.Update(productInCart);
                _applicationDb.SaveChanges();
            }
            else
            {
                _applicationDb.ProductsInCarts.Remove(productInCart);
                _applicationDb.SaveChanges();
            }

            var productInCartView = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
            SetRegionalCost(productInCartView.Product);

            return productInCartView;
        }
        
        public ProductInCartViewModel SelectProduct(long id)
        {
            InitializeProductsCart();

            var productInCart = _productsCart.ProductsInCart.FirstOrDefault(x => x.Id == id);

            if (productInCart == null || productInCart.Product.QuantityInStock < productInCart.Count)
                throw new NotFoundException("product not found or out of stock");

            productInCart.Selected = !productInCart.Selected;
            _applicationDb.ProductsInCarts.Update(productInCart);
            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var productInCartViewModel = mapper.Map<ProductInCart, ProductInCartViewModel>(productInCart);
            SetRegionalCost(productInCartViewModel.Product);

            return productInCartViewModel;
        }

        public IEnumerable<ProductInCartViewModel> BuySelectedProducts()
        {
            if (!User.EmailConfirmed)
            {
                throw new BadRequestException("email not confirmed");
            }
            
            InitializeProductsCart();
            List<ProductInCart> selectedProductsInCart = _productsCart.ProductsInCart.Where(x => x.Selected).ToList();

            if (selectedProductsInCart.Count < 1)
                throw new BadRequestException("no products selected");

            var htmlString = System.IO.File.ReadAllText("Views/PurchaseEmail.html");
            Template template = Template.Parse(htmlString);
            string message = template.Render(new
            {
                cart_cost = selectedProductsInCart.Sum(x => x.Cost),
                user_name = User.UserName,
                products_in_cart = selectedProductsInCart
            });
            
            _emailService.SendEmail(User.Email, "Спасибо за покупку", message);

            foreach (var productInCart in selectedProductsInCart)
            {
                productInCart.Product.QuantityInStock -= productInCart.Count;
                _applicationDb.ProductsInCarts.Remove(productInCart);
            }

            _applicationDb.SaveChanges();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductInCart, ProductInCartViewModel>();

                cfg.CreateMap<Product, ProductViewModel>();
                cfg.CreateMap<Store, StorePutModel>();
            });
            var mapper = new Mapper(mapperConfig);

            var selectedProductInCarViewModels =
                mapper.Map<List<ProductInCart>, List<ProductInCartViewModel>>(selectedProductsInCart);
            SetRegionalCost(selectedProductInCarViewModels.Select(x => x.Product));

            return selectedProductInCarViewModels;
        }
    }
}