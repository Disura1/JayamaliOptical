using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace JayamaliOptical.Web.Services
{
    public interface ICartService
    {
        ShoppingCart GetCart();
        void AddToCart(CartItem item);
        void RemoveFromCart(int productId);
        void UpdateQuantity(int productId, int quantity);
        void ClearCart();
    }

    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CartSessionKey = "ShoppingCart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? Context => _httpContextAccessor.HttpContext;

        public ShoppingCart GetCart()
        {
            var cartJson = Context?.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new ShoppingCart();
            }

            return JsonSerializer.Deserialize<ShoppingCart>(cartJson) ?? new ShoppingCart();
        }

        private void SaveCart(ShoppingCart cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            Context?.Session.SetString(CartSessionKey, cartJson);
        }

        public void AddToCart(CartItem item)
        {
            var cart = GetCart();
            cart.AddItem(item);
            SaveCart(cart);
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var cart = GetCart();
            cart.Clear();
            SaveCart(cart);
        }
    }
}