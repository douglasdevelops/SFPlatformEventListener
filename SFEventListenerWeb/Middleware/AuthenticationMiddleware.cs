using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace SFEventListenerWeb.Attributes
{
    public class OAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public OAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Cookies["OAuthAccessToken"];

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Items["OAuthAccessToken"] = accessToken;
            }

            await _next(context);
        }
    }

}
