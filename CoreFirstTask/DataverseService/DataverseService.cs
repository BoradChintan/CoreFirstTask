using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Rest;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using CoreFirstTask.Models;
using Org.BouncyCastle.Tls;

namespace CoreFirstTask.DataverseService
{
    public class DataverseServices
    {
        private readonly ServiceClient _crmServiceClient;

        public DataverseServices(IConfiguration configuration)
        {
            // Retrieve the connection string from configuration
            var connectionString = configuration.GetSection("Dataverse:ConnectionString").Value;
            _crmServiceClient = new ServiceClient(connectionString);

            if (!_crmServiceClient.IsReady)
            {
                throw new Exception("Failed to connect to Dataverse.");
            }
        }

        public ServiceClient GetServiceClient()
        {
            return _crmServiceClient;
        }

        public Order GetOrder(Guid orderId)
        {
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

            query.Criteria.AddCondition("salesorderid", ConditionOperator.Equal, orderId);

            var orderEntity = _crmServiceClient.RetrieveMultiple(query).Entities.FirstOrDefault();
            if (orderEntity == null) return null;

            return new Order
            {
                salesorderid = orderEntity.Id,
                name = orderEntity.GetAttributeValue<string>("name"),
                customername = orderEntity.GetAttributeValue<EntityReference>("customerid")?.Name,
                totalamount = orderEntity.GetAttributeValue<Money>("totalamount")?.Value ?? 0,
                statecode = orderEntity.GetAttributeValue<OptionSetValue>("statecode").Value,
            };
        }

        public Guid? GetSystemUserID(Guid azureId)
        {
            var query = new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet("fullname"),
                Criteria = {
                Conditions = {
                new ConditionExpression("azureactivedirectoryobjectid", ConditionOperator.Equal, azureId)
            }
        }
            };

            var result = _crmServiceClient.RetrieveMultiple(query);

            if (result.Entities.Count > 0)
            {
                var systemUserId = result.Entities[0].Id;
                return systemUserId;

            }
            else
            {
                return null;
            }
        }
        public Invoice GetInvoiceById(Guid invoiceId)
        {
            try
            {
                var invoiceEntities = _crmServiceClient.Retrieve("invoice",invoiceId, new ColumnSet ( true));

                var invoiceEntity = invoiceEntities;

                if (invoiceEntity == null)
                {
                    return null;  
                }

                
                var customerName = invoiceEntity.GetAttributeValue<EntityReference>("customerid").Name;

                return new Invoice
                {
                    invoiceid = invoiceEntity.GetAttributeValue<Guid>("invoiceid"),
                    customerid = invoiceEntity.GetAttributeValue<EntityReference>("customerid").Id,
                    customeridname = customerName,
                    invoicenumber = invoiceEntity.GetAttributeValue<string>("invoicenumber"),
                    name = invoiceEntity.GetAttributeValue<string>("name"),
                    createdon = invoiceEntity.GetAttributeValue<DateTime>("createdon")
                };
            }
            catch (Exception ex)
            {
                 
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;  
            }
        }
    }
}
