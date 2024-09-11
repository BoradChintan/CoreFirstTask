using CoreFirstTask.DataverseService;
using CoreFirstTask.Models;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Org.BouncyCastle.Crmf;
using System.Collections.Generic;
using System.Net.Http;

namespace CoreFirstTask.Controllers
{
    public class SoftwareController : Controller
    {
        private readonly DataverseServices _dataverseService;
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;

        public SoftwareController(DataverseServices dataverseService, System.Net.Http.IHttpClientFactory httpClientFactory)
        {
            _dataverseService = dataverseService;
            _httpClientFactory = httpClientFactory;
        }

        //get label for option set 
        public string GetOptionSetValueLabel(string entityLogicalName, string attributeLogicalName, int optionSetValue)
        {
            var crmService = _dataverseService.GetServiceClient();
            var retrieveAttributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeLogicalName,
                RetrieveAsIfPublished = true
            };

            var retrieveAttributeResponse = (RetrieveAttributeResponse)crmService.Execute(retrieveAttributeRequest);
            var attributeMetadata = (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;

            foreach (var option in attributeMetadata.OptionSet.Options)
            {
                if (option.Value == optionSetValue)
                {
                    return option.Label.UserLocalizedLabel.Label;
                }
            }

            return string.Empty;
        }

        public IActionResult SoftwareInventory()
        {
            var crmService = _dataverseService.GetServiceClient();
            var query = new QueryExpression("new_softwareinventory");
            query.ColumnSet = new ColumnSet("new_icon", "new_displayoption", "new_category", "new_name", "new_softwareinventoryid");

            var result = crmService.RetrieveMultiple(query);
            var myDictionary = new Dictionary<int, CategoryModel>();

            foreach (Entity item in result.Entities)
            {
                int itemKey = item.GetAttributeValue<OptionSetValue>("new_category").Value;
                int display = item.GetAttributeValue<OptionSetValueCollection>("new_displayoption").FirstOrDefault().Value;
                var icon = item.GetAttributeValue<byte[]>("new_icon");
                Guid id = item.GetAttributeValue<Guid>("new_softwareinventoryid");

                var label = GetOptionSetValueLabel("new_softwareinventory", "new_category", itemKey);

                if (!myDictionary.ContainsKey(itemKey))
                {
                    myDictionary.Add(itemKey, new CategoryModel
                    {
                        new_category = label,
                        current = new List<IconGroup>(),
                        future = new List<IconGroup>(),
                        other = new List<IconGroup>(),
                    });
                }
                var currentIconGroup = new IconGroup
                {
                    SoftwareID = id,
                    Icon = Convert.ToBase64String(icon),
                };
                switch (display)
                {
                    case 0:
                        myDictionary[itemKey].current.Add(currentIconGroup);
                        break;

                    case 1:
                        myDictionary[itemKey].future.Add(currentIconGroup);
                        break;

                    case 2:
                        myDictionary[itemKey].other.Add(currentIconGroup);
                        break;
                }
            }
            return View(myDictionary);
        }


        public IActionResult AddNewSoftware(Software data, IFormFile new_icon)
        {
            var software = new Entity("new_softwareinventory");
            var crmService = _dataverseService.GetServiceClient();

            // Handle file upload
            if (new_icon != null && new_icon.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    new_icon.CopyTo(memoryStream);
                    software["new_icon"] = memoryStream.ToArray();
                }
            }
            software["new_name"] = data.new_name;

            if (data.new_category != null)
            {
                software["new_category"] = new OptionSetValue((int)data.new_category);
            }

            if (data.new_displayoption != null)
            {
                software["new_displayoption"] = new OptionSetValueCollection { new OptionSetValue((int)data.new_displayoption) };
            }

            try
            {
                crmService.Create(software);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                ViewBag.Error = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction("SoftwareInventory"); // Redirect to a suitable action or view
        }


        [HttpGet]
        public IActionResult SelectedSoftware()
        {
            if (!Request.Headers.TryGetValue("Category", out var categoryHeader))
            {
                return BadRequest("Category header is missing");
            }

            if (!int.TryParse(categoryHeader.FirstOrDefault(), out int category))
            {
                return BadRequest("Invalid category value");
            }

            // Logic to handle the selected category
            var crmService = _dataverseService.GetServiceClient();

            var query = new QueryExpression("new_softwareinventory")
            {
                ColumnSet = new ColumnSet("new_softwareinventoryid", "new_category", "new_displayoption", "new_name"),
                Criteria = new FilterExpression
                {
                    Conditions =
            {
                new ConditionExpression("new_category", ConditionOperator.Equal, category)
            }
                }
            };

            var result = crmService.RetrieveMultiple(query);
            var softwareList = new List<Software>();

            foreach (var entity in result.Entities)
            {
                var software = new Software
                {
                    new_name = entity.GetAttributeValue<string>("new_name"),
                    new_category = entity.GetAttributeValue<OptionSetValue>("new_category")?.Value,
                    new_displayoption = entity.GetAttributeValue<OptionSetValueCollection>("new_displayoption")?.FirstOrDefault()?.Value,
                    new_softwareinventoryid = entity.GetAttributeValue<Guid>("new_softwareinventoryid")
                };
                softwareList.Add(software);
            }
            return Json(softwareList);
        }


        public IActionResult AddMySoftware(Software data)
        {
            var crmService = _dataverseService.GetServiceClient();
            EntityReference id = new EntityReference("new_softwareinventory", new Guid(data.new_name));

            // Retrieve the software record using the GUID
            Entity selectedSoftware = crmService.Retrieve("new_softwareinventory", id.Id, new ColumnSet("new_icon", "new_name"));

            byte[] image = selectedSoftware.GetAttributeValue<byte[]>("new_icon");
            data.new_name = selectedSoftware.GetAttributeValue<string>("new_name");

            Entity software = new Entity("new_softwareinventory");
            software["new_name"] = data.new_name;
            software["new_icon"] = image;
            software["new_category"] = new OptionSetValue(data.new_category.Value);
            software["new_displayoption"] = new OptionSetValueCollection { new OptionSetValue(data.new_displayoption.Value) };

            crmService.Create(software);
            return RedirectToAction("SoftwareInventory");
        }


        [HttpPost]
        [Route("Software/DragAndDrop")]
        public IActionResult DragAndDrop([FromBody] DragAndDropRequest request)
        {
            {
                var crmService = _dataverseService.GetServiceClient();
                if (request == null || string.IsNullOrEmpty(request.SoftwareId))
                {
                    return BadRequest("Invalid request data");
                }

                try
                {
                    var result = crmService.Retrieve("new_softwareinventory", new Guid(request.SoftwareId),new ColumnSet (true));

                    // Create an entity object with the ID of the record to update
                    
                    var categoryLabelValue = result.GetAttributeValue<OptionSetValue>("new_category").Value;
                    var categoryLabel = GetOptionSetValueLabel("new_softwareinventory", "new_category", categoryLabelValue);
                    var software_name = result.GetAttributeValue<string>("new_name");

                    var query2 = new QueryExpression("new_softwareinventory");
                    query2.Criteria.AddCondition("new_category", ConditionOperator.Equal,categoryLabelValue);
                    query2.Criteria.AddCondition("new_name", ConditionOperator.Equal, software_name);
                    query2.Criteria.AddCondition("new_displayoption", ConditionOperator.Equal, request.DisplayOption);

                    var selecteddata = crmService.RetrieveMultiple(query2);

                    if (selecteddata.Entities.Count != 0)
                    {
                        return Json(new { Status = "Selected software is already present!" });
                    }


                    Entity softwareEntity = new Entity("new_softwareinventory", new Guid(request.SoftwareId));
                    softwareEntity["new_displayoption"] = new OptionSetValueCollection { new OptionSetValue(request.DisplayOption) };
                    crmService.Update(softwareEntity);

                    return Json(new { Status = "Ok" });

                }
                catch (Exception ex)
                {
                    return Json(new { Status = ex.Message });
                }
            }
        }
    }
}
