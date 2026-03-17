using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JayamaliOptical.Web.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }
    }
}