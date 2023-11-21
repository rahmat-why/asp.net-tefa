using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.Linq;

namespace ASP.NET_TEFA.Models
{
    public class AuthorizedUser : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizedUser(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var isAuthorized = !string.IsNullOrEmpty(context.HttpContext.Session.GetString("userAuthentication"));

            if (!isAuthorized)
            {
                // User is not authorized, redirect to Authentication's Login action.
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "User",
                    action = "Login"
                }));
                return; // Stop further execution
            }

            // Check if the user has the required role
            string userAuthentication = context.HttpContext.Session.GetString("userAuthentication");
            MsUser user = JsonConvert.DeserializeObject<MsUser>(userAuthentication);

            if (!_allowedRoles.Contains(user.Position))
            {
                // User does not have the required role for this action, redirect to a different action or show an unauthorized page.
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Authentication",
                    action = "NotFound"
                }));
            }
        }
    }
}