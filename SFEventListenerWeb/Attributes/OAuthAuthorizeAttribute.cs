using Microsoft.AspNetCore.Authorization;

namespace SFEventListenerWeb
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class OAuthAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string RedirectUrl { get; set; } = "/oAuth/Login";
        public string CookieName { get; set; } = "OAuthAccessToken";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var accessToken = context.HttpContext.Request.Cookies[CookieName];

            if (string.IsNullOrEmpty(accessToken))
            {
                context.Result = new RedirectResult(RedirectUrl);
            }
        }
    }
}
