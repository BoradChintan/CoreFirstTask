using CoreFirstTask.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System.Diagnostics;
using System;
using CoreFirstTask.DataverseService;
using Microsoft.Xrm.Sdk;
using System.Security.Claims;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;


namespace CoreFirstTask.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly DataverseServices _dataverseService;
        private readonly IAuthorizationService _authorizationService;
        private readonly UserRecordService _userRecordService;
        private readonly PdfService _pdfService;
        // private readonly LoginHandlerService _loginHandler;

        public HomeController(ILogger<HomeController> logger, DataverseServices dataverseService, IAuthorizationService authorizationService, UserRecordService userRecordService , PdfService pdfService)
        {
            _logger = logger;
            _dataverseService = dataverseService;
            _authorizationService = authorizationService;
            _userRecordService = userRecordService;
            _pdfService = pdfService;
        }

 

        //[Authorize(Roles ="Admin,Employee,User,Manager")]
        public async Task<IActionResult> Index()
        {
           var  role = HttpContext.Session.GetString("role");
            // Get the ServiceClient from the DataverseServices
            var crmServiceClient = _dataverseService.GetServiceClient();

            var user = User;

            var isSuperAdmin = await _authorizationService.AuthorizeAsync(user, "Admin");
            var isAdmin = await _authorizationService.AuthorizeAsync(user, "Manager");
            var isUser = await _authorizationService.AuthorizeAsync(user, "User");
            var isEmployee = await _authorizationService.AuthorizeAsync(user, "Employee");

            var claimIdentity = User.Identity as ClaimsIdentity;

           
            if (claimIdentity != null)
            {
                HttpContext.Session.SetString("role", claimIdentity.Claims?.FirstOrDefault(x => x.Type == "roles")?.Value);
                HttpContext.Session.SetString("name", claimIdentity.Claims?.FirstOrDefault(x => x.Type == "name")?.Value);
                HttpContext.Session.SetString("unique_name", claimIdentity.Claims?.FirstOrDefault(x => x.Type == "unique_name")?.Value);

                var azureId = claimIdentity.Claims?.FirstOrDefault(x => x.Type == "oid")?.Value;

                if (azureId != null)
                {
                    var userId = _dataverseService.GetSystemUserID(new Guid(azureId));

                    if (userId != null)
                    {
                        HttpContext.Session.SetString("UserId", userId.ToString());
                    }
                }
            }

            // Example logic to get the unique name from claims
            var user1 = HttpContext.Session.GetString("unique_name"); // Adjust based on your authentication scheme

            if (user1 != null)
            {
                _userRecordService.HandleUserLoginAsync(user1);
            }

            // Create the QueryExpression for the "invoice" entity
            var query = new QueryExpression("invoice")
            {
                ColumnSet = new ColumnSet(
                    "invoiceid",
                    "customerid",   
                    "accountid",    
                    "createdon",
                    "invoicenumber",
                    "name"
                ),
                LinkEntities =
                 {
                     new LinkEntity
                     {
                         // Link to the "account" entity
                         LinkFromEntityName = "invoice",
                         LinkFromAttributeName = "accountid",
                         LinkToEntityName = "account",
                         LinkToAttributeName = "accountid",
                         JoinOperator = JoinOperator.LeftOuter,
                         Columns = new ColumnSet("name"),  
                         EntityAlias = "account_alias"
                     },
                     new LinkEntity
                     {
                         // Link to the "contact" entity
                         LinkFromEntityName = "invoice",
                         LinkFromAttributeName = "customerid",
                         LinkToEntityName = "contact",
                         LinkToAttributeName = "contactid",
                         JoinOperator = JoinOperator.LeftOuter,
                         Columns = new ColumnSet("fullname"), // Get the full name of the contact
                         EntityAlias = "contact_alias"
                     }
                 }
            };

            // Example usage: Retrieving the records and accessing the aliases
            var results = crmServiceClient.RetrieveMultiple(query);

            var invoices = new List<Invoice>();

            // Use LINQ to select and project data into the Invoice model
            var invoice = results.Entities.Select(entity => new Invoice
            {
                invoiceid = entity.GetAttributeValue<Guid>("invoiceid"),
                accountid = entity.GetAttributeValue<EntityReference>("accountid")?.Id ?? Guid.Empty,
                customerid = entity.GetAttributeValue<EntityReference>("customerid")?.Id ?? Guid.Empty,
                invoicenumber = entity.GetAttributeValue<string>("invoicenumber"),
                name = entity.GetAttributeValue<string>("name"),
                createdon = entity.GetAttributeValue<DateTime>("createdon"),

                // Safely get aliased fields with null checks
                accountidname = entity.GetAttributeValue<AliasedValue>("account_alias.name")?.Value as string,
                customeridname = entity.GetAttributeValue<AliasedValue>("contact_alias.fullname")?.Value as string
            }).ToList();

            return View(invoice);
        }
     
        public IActionResult Privacy()
        {
           var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
            {
                return RedirectToAction("Access","Auth");
            }
            return View();
        } 
        
        public IActionResult Export(Guid invoiceId)
        {
            var crmService = _dataverseService.GetServiceClient();

            // Fetch invoice data
            var invoice = _dataverseService.GetInvoiceById(invoiceId);

            return View("PdfViewer",invoice);       
        }

        [HttpGet]
        public async Task<IActionResult> GetPdf(Guid invoiceId)
        {
            try
            {
                // Fetch invoice data
                var invoice = _dataverseService.GetInvoiceById(invoiceId);

                if (invoice == null)
                {
                    return NotFound();  
                }

                // Generate PDF from invoice data
                var pdfBytes = _pdfService.GeneratePdf(invoice);
                var base64Pdf = Convert.ToBase64String(pdfBytes);

                // Return JSON with the base64 PDF string
                return Json(new { base64Pdf });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        private object GetAliasedValue(Entity entity, string attributeName)
        {
            if (entity.Contains(attributeName))
            {
                var aliasedValue = entity[attributeName] as AliasedValue;
                return aliasedValue?.Value;
            }
            return null;
        }

        public IActionResult WordPopulate()
        {

            // Get the ServiceClient from the DataverseServices
            var crmServiceClient = _dataverseService.GetServiceClient();
            QueryExpression query = new QueryExpression("salesorder")
            {
                ColumnSet = new ColumnSet("salesorderid", "customerid", "customeridname", "name", "statecode", "totalamount"),
                LinkEntities =
            {
                new LinkEntity
                {
                    LinkFromAttributeName = "customerid",
                    LinkToAttributeName = "contactid",
                    LinkToEntityName = "contact",
                    JoinOperator = JoinOperator.LeftOuter,
                    Columns = new ColumnSet("fullname"),
                    EntityAlias = "customerid_contact"
                }
            }
            };

            EntityCollection results = crmServiceClient.RetrieveMultiple(query);


            // Map results to the Order model
            var orders = results.Entities.Select(e => new Order
            {
                salesorderid = (Guid)e.GetAttributeValue<Guid>("salesorderid"),
                name = e.GetAttributeValue<string>("name"),
                customername = e.GetAttributeValue<AliasedValue>("customerid_contact.fullname")?.Value as string,
                totalamount = e.GetAttributeValue<Money>("totalamount").Value,
                statecode = e.GetAttributeValue<OptionSetValue>("statecode").Value
            }).ToList();

            return View(orders);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
