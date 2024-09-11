using CoreFirstTask.DataverseService;
using Microsoft.AspNetCore.Mvc;

namespace CoreFirstTask.Controllers
{
    public class Orders : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataverseServices _dataverseService;
        private readonly WordTemplateService _wordTemplateService;

        public Orders(ILogger<HomeController> logger, DataverseServices dataverseService, WordTemplateService wordTemplateService)
        {
            _logger = logger;
            _dataverseService = dataverseService;
            _wordTemplateService = wordTemplateService;
        }

        public IActionResult Order()
        {
            return View();
        }

        public IActionResult DownloadOrderWordTemplate(Guid orderId)
        {
            var order = _dataverseService.GetOrder(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "OrderTemplate.docx");
            var fileContents = _wordTemplateService.PopulateTemplate(order, templatePath);

            return File(fileContents, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Order.docx");
        }
    }
}
