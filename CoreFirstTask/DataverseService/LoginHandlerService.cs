using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Security.Claims;


namespace CoreFirstTask.DataverseService
{
    public class LoginHandlerService
    {
        private readonly IOrganizationService _service;
        private readonly IEmailSender _emailSender;

        public LoginHandlerService(IOrganizationService service, IEmailSender emailSender)
        {
            _service = service;
            _emailSender = emailSender;
        }

        public async Task HandleLoginAsync(ClaimsPrincipal user)
        {
            var uniqueName = user.FindFirst("unique_name")?.Value;
            if (string.IsNullOrEmpty(uniqueName)) return;

            // Query Dataverse to find the user's email address
            var email = await GetEmailAddressFromUniqueNameAsync(uniqueName);
            if (string.IsNullOrEmpty(email)) return;

            // Check if it's the user's first login
            var query = new QueryExpression("new_usertracking")
            {
                Criteria = new FilterExpression
                {
                    Conditions =
                {
                    new ConditionExpression("new_name", ConditionOperator.Equal, email)
                }
                }
            };

            var result = _service.RetrieveMultiple(query);

            if (result.Entities.Count == 0)
            {
                // Create new user tracking record
                var newUser = new Entity("new_usertracking")
                {
                    ["new_name"] = email,
                    ["new_isfirstlogin"] = true
                };

                _service.Create(newUser);

                await SendWelcomeEmailAsync(email);
            }
            else
            {
                var userRecord = result.Entities[0];
                if ((bool)userRecord["new_isfirstlogin"])
                {
                    userRecord["new_isfirstlogin"] = false;
                    _service.Update(userRecord);
                }
            }
        }

        private async Task<string> GetEmailAddressFromUniqueNameAsync(string uniqueName)
        {
            var query = new QueryExpression("systemuser")
            {
                ColumnSet = new ColumnSet("internalemailaddress"),
                Criteria = new FilterExpression
                {
                    Conditions =
                {
                    new ConditionExpression("domainname", ConditionOperator.Equal, uniqueName)
                }
                }
            };

            var result = _service.RetrieveMultiple(query);
            return result.Entities.Count > 0 ? result.Entities[0].GetAttributeValue<string>("internalemailaddress") : null;
        }

        private async Task SendWelcomeEmailAsync(string email)
        {
            var message = new Message(new string[] { email }, "Welcome", "Welcome to our application!");
            await _emailSender.SendEmailAsync(message);
        }
    }
}
