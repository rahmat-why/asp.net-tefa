using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ASP.NET_TEFA.Models
{
    public class AuthorizedCustomer : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var isAuthorized = !string.IsNullOrEmpty(context.HttpContext.Session.GetString("authentication"));

            if (!isAuthorized)
            {
                // User is not authorized, redirect to Authentication's Login action.
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Authentication",
                    action = "Login"
                }));
            }
        }
    }
}
