using CoreFirstTask.DataverseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using CoreFirstTask.Models;

namespace CoreFirstTask.Controllers
{
    public class ChartController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly DataverseServices _dataverseService;
        private readonly IAuthorizationService _authorizationService;

        public ChartController(ILogger<HomeController> logger, DataverseServices dataverseService, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _dataverseService = dataverseService;
            _authorizationService = authorizationService;
        }
        public IActionResult PieChart()
        {
            var crmService = _dataverseService.GetServiceClient();
            var query = new QueryExpression("opportunityproduct")
            {
                ColumnSet = new ColumnSet("productid", "priceperunit", "quantity")
            };

            // Join with Product entity to get product names
            query.LinkEntities.Add(new LinkEntity
            {
                LinkFromEntityName = "opportunityproduct",
                LinkFromAttributeName = "productid",
                LinkToEntityName = "product",
                LinkToAttributeName = "productid",
                Columns = new ColumnSet("name"),
                EntityAlias = "product_alias"
            });

            var results = crmService.RetrieveMultiple(query);

            var data = results.Entities
           .GroupBy(e => e.GetAttributeValue<AliasedValue>("product_alias.name")?.Value.ToString())
           .Select(g => new ProductAmount
           {
               ProductName = g.Key,
               TotalAmount = g.Sum(e => (e.GetAttributeValue<Money>("priceperunit")?.Value ?? 0m) * (e.GetAttributeValue<decimal>("quantity")))
           }).ToList();

            return View(data);
             
        }
    }
}
