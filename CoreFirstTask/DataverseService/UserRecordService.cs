using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace CoreFirstTask.DataverseService
{
    public class UserRecordService
    {
        private readonly DataverseServices _dService;
        private readonly ServiceClient _serviceClient;
        public UserRecordService(DataverseServices dService)
        {
            _dService = dService;
            _serviceClient = _dService.GetServiceClient();  
        }

        public void HandleUserLoginAsync(string uniqueName)
        {
            // Check if a record with the unique name already exists
            var fetchXml = $@"
            <fetch top='1'>
              <entity name='new_usertracking'>
                <attribute name='new_name' />
                <filter>
                  <condition attribute='new_name' operator='eq' value='{uniqueName}' />
                </filter>
              </entity>
            </fetch>";

            var result = _serviceClient.RetrieveMultiple(new FetchExpression(fetchXml));

            if (result.Entities.Count == 0)
            {
                // Create a new record if it doesn't exist
                var newRecord = new Entity("new_usertracking")
                {
                    ["new_name"] = uniqueName,
                    ["new_isfirstlogin"] = false
                };

                _serviceClient.Create(newRecord);
            }
        }
    }
}
