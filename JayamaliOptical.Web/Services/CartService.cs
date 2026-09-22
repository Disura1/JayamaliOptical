using JayamaliOptical.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
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

        // Session-backed reads/writes are read-modify-write with no built-in locking, so two
        // requests for the same session (a double-click, or two tabs) can race and one write
        // silently overwrites the other. Serialize mutations per session id to close that race.
        // Entries are never evicted — for this app's scale that's a bounded, acceptable amount
        // of memory (one small SemaphoreSlim per session ever seen since the last app restart).
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> SessionLocks = new();

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

            try
            {
                return JsonSerializer.Deserialize<ShoppingCart>(cartJson) ?? new ShoppingCart();
            }
            catch (JsonException)
            {
                // Corrupted or schema-mismatched session data — fall back to an empty cart
                // instead of a 500 on every page that touches the cart.
                return new ShoppingCart();
            }
        }

        private void SaveCart(ShoppingCart cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            Context?.Session.SetString(CartSessionKey, cartJson);
        }

        private void MutateCart(Action<ShoppingCart> mutate)
        {
            var session = Context?.Session;
            if (session == null)
            {
                var cart = GetCart();
                mutate(cart);
                SaveCart(cart);
                return;
            }

            var sessionLock = SessionLocks.GetOrAdd(session.Id, _ => new SemaphoreSlim(1, 1));
            sessionLock.Wait();
            try
            {
                var cart = GetCart();
                mutate(cart);
                SaveCart(cart);
            }
            finally
            {
                sessionLock.Release();
            }
        }

        public void AddToCart(CartItem item) => MutateCart(cart => cart.AddItem(item));

        public void RemoveFromCart(int productId) => MutateCart(cart => cart.RemoveItem(productId));

        public void UpdateQuantity(int productId, int quantity) => MutateCart(cart => cart.UpdateQuantity(productId, quantity));

        public void ClearCart() => MutateCart(cart => cart.Clear());
    }
}
