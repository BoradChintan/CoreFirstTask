using CoreFirstTask.DataverseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreFirstTask.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataverseServices _dataverseService;
    

        public AuthController(ILogger<HomeController> logger, DataverseServices dataverseService, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _dataverseService = dataverseService;
        }
        public IActionResult Access()
        {
            return View();
        }
    }
}
