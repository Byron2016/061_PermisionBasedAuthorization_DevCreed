using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBaseWebADotNet5.Web.Constants;

namespace PBaseWebADotNet5.Web.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Permissions.Products.Edit)]
        public IActionResult Edit()
        {
            return View();
        }
    }
}
