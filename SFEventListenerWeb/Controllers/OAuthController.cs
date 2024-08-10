using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Mvc;
using SFEventListenerWeb;
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class OAuthTokenResponse
{
    public string access_token { get; set; }
    public string instance_url { get; set; }
}

public class OAuthController : Controller
{
    private static readonly string ConsumerKey = "3MVG9WCdh6PFin0i0XaOjVSh92GdVQU6hwoSBRvvI9UIUIqmzBrSHRAOFA2480RBZRruqF2c3Dr6z01sErl26";
    private static readonly string ConsumerSecret = "2E58EC6E1AE32047A04EE3FE64CF6F6A413238E5FCC15C86116D65636687B926";
    private static readonly string CallbackUrl = "https://localhost:7018/oauth/callback";
    private static readonly string AuthUrl = "https://test.salesforce.com/services/oauth2/authorize";
    private static readonly string TokenUrl = "https://test.salesforce.com/services/oauth2/token";

    public ActionResult Login()
    {
        string codeVerifier = PkceUtil.GenerateCodeVerifier();
        string codeChallenge = PkceUtil.GenerateCodeChallenge(codeVerifier);

        HttpContext?.Session.SetString("CodeVerifier", codeVerifier);

        string loginUrl = $"{AuthUrl}?response_type=code&client_id={ConsumerKey}&redirect_uri={CallbackUrl}&scope=full&code_challenge={codeChallenge}&code_challenge_method=S256";
        return Redirect(loginUrl);
    }

    public async Task<ActionResult> Callback(string code)
    {
        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
            string codeVerifier = HttpContext.Session.GetString("CodeVerifier");
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", ConsumerKey),
                new KeyValuePair<string, string>("client_secret", ConsumerSecret),
                new KeyValuePair<string, string>("redirect_uri", CallbackUrl),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("code_verifier", codeVerifier)
            });

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(content);
            string accessToken = tokenResponse.access_token;
            string instanceUrl = tokenResponse.instance_url;

            var cookieExpirationTime = DateTime.UtcNow.AddSeconds(7200);

            // Store the access token in a cookie
            Response.Cookies.Append("OAuthAccessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpirationTime
            });

            Response.Cookies.Append("InstanceUrl", instanceUrl, new CookieOptions
            {
                Expires = cookieExpirationTime
            });

            string pattern = @"--(.*?)\.sandbox";
            Match match = Regex.Match(instanceUrl, pattern);

            if (match.Success)
            {
                string result = match.Groups[1].Value;
                Response.Cookies.Append("OrgName", result, new CookieOptions
                {
                    Expires = cookieExpirationTime
                });
            }
            else
            {
                Console.WriteLine("No match found.");
            }
        }

        return RedirectToAction("Subscribe", "Event");
    }

    public ActionResult Logout()
    {
        // Implement logout by clearing the session or token
        return RedirectToAction("Index", "Home");
    }
}
