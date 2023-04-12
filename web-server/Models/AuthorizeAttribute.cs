using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

namespace web_server.Models
{
    public class AuthorizeAttribute : TypeFilterAttribute
    {
        public AuthorizeAttribute(params string[] claim) : base(typeof(AuthorizeFilter))
        {
            Arguments = new object[] { claim };
        }
    }

    public class AuthorizeFilter : IAuthorizationFilter
    {
        readonly string[] _claim;

        public AuthorizeFilter(params string[] claim)
        {
            _claim = claim;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var IsAuthenticated = context.HttpContext.User.Identity.IsAuthenticated;
            var claimsIndentity = context.HttpContext.User.Identity as ClaimsIdentity;

            if (IsAuthenticated)
            {
                if (claimsIndentity.Claims.Count() == 0)
                {
                    return;
                }

                bool flagClaim = false;
                foreach (var item in claimsIndentity.Claims)
                {
                    if (context.HttpContext.User.HasClaim("Role", item.Value))
                        flagClaim = true;
                }
                if (!flagClaim)
                    context.Result = new UnauthorizedObjectResult("У Вас недостаточно прав для просмотра данной страницы");
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
            return;
        }
    }
}
