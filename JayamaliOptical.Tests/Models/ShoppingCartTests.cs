using JayamaliOptical.Web.Models;

namespace JayamaliOptical.Tests.Models
{
    public class ShoppingCartTests
    {
        private static CartItem MakeItem(int productId, decimal price = 100m, int quantity = 1) => new()
        {
            ProductId = productId,
            ProductName = $"Product {productId}",
            Price = price,
            Quantity = quantity
        };

        [Fact]
        public void AddItem_NewProduct_AddsToItems()
        {
            var cart = new ShoppingCart();

            cart.AddItem(MakeItem(1));

            Assert.Single(cart.Items);
        }

        [Fact]
        public void AddItem_ExistingProduct_IncreasesQuantityInsteadOfDuplicating()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1, quantity: 1));

            cart.AddItem(MakeItem(1, quantity: 2));

            var item = Assert.Single(cart.Items);
            Assert.Equal(3, item.Quantity);
        }

        [Fact]
        public void RemoveItem_ExistingProduct_RemovesIt()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1));

            cart.RemoveItem(1);

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void RemoveItem_UnknownProduct_IsNoOp()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1));

            cart.RemoveItem(999);

            Assert.Single(cart.Items);
        }

        [Fact]
        public void UpdateQuantity_PositiveValue_UpdatesQuantity()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1, quantity: 1));

            cart.UpdateQuantity(1, 5);

            Assert.Equal(5, cart.Items.Single().Quantity);
        }

        [Fact]
        public void UpdateQuantity_ZeroOrLess_RemovesItem()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1));

            cart.UpdateQuantity(1, 0);

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1));
            cart.AddItem(MakeItem(2));

            cart.Clear();

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void TotalItems_SumsQuantitiesAcrossLines()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1, quantity: 2));
            cart.AddItem(MakeItem(2, quantity: 3));

            Assert.Equal(5, cart.TotalItems);
        }

        [Fact]
        public void TotalPrice_SumsPriceTimesQuantityAcrossLines()
        {
            var cart = new ShoppingCart();
            cart.AddItem(MakeItem(1, price: 100m, quantity: 2));
            cart.AddItem(MakeItem(2, price: 50m, quantity: 1));

            Assert.Equal(250m, cart.TotalPrice);
        }
    }
}
