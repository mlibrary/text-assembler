// COPYRIGHT  2015-16
// MICHIGAN STATE UNIVERSITY BOARD OF TRUSTEES
// ALL RIGHTS RESERVED
//
// PERMISSION IS GRANTED TO USE, COPY, CREATE DERIVATIVE WORKS AND REDISTRIBUTE
// THIS SOFTWARE AND SUCH DERIVATIVE WORKS FOR ANY PURPOSE, SO LONG AS THE NAME
// OF MICHIGAN STATE UNIVERSITY IS NOT USED IN ANY ADVERTISING OR PUBLICITY
// PERTAINING TO THE USE OR DISTRIBUTION OF THIS SOFTWARE WITHOUT SPECIFIC,
// WRITTEN PRIOR AUTHORIZATION.  IF THE ABOVE COPYRIGHT NOTICE OR ANY OTHER
// IDENTIFICATION OF MICHIGAN STATE UNIVERSITY IS INCLUDED IN ANY COPY OF ANY
// PORTION OF THIS SOFTWARE, THEN THE DISCLAIMER BELOW MUST ALSO BE INCLUDED.
//
// THIS SOFTWARE IS PROVIDED AS IS, WITHOUT REPRESENTATION FROM MICHIGAN STATE
// UNIVERSITY AS TO ITS FITNESS FOR ANY PURPOSE, AND WITHOUT WARRANTY BY
// MICHIGAN STATE UNIVERSITY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE. THE MICHIGAN STATE UNIVERSITY BOARD OF TRUSTEES SHALL
// NOT BE LIABLE FOR ANY DAMAGES, INCLUDING SPECIAL, INDIRECT, INCIDENTAL, OR
// CONSEQUENTIAL DAMAGES, WITH RESPECT TO ANY CLAIM ARISING OUT OF OR IN
// CONNECTION WITH THE USE OF THE SOFTWARE, EVEN IF IT HAS BEEN OR IS HEREAFTER
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
//
// Written by Megan Schanz, 2015-16
// (c) Michigan State University Board of Trustees
// Licensed under GNU General Public License (GPL) Version 2.

using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Web.SessionState;

namespace LexisNexisWSKImplementation
{
    /// <summary>
    /// This is a custom class for the MSU OAuth2 client
    /// </summary>
    public class MsuOAuth2Client : OAuth2Client
    {
        #region Feilds
        /// <summary>
        /// The authorization endpoint.
        /// </summary>
        private const string AuthorizationEndPoint = "https://oauth.itservices.msu.edu/oauth/authorize";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string TokenEndPoint = "https://oauth.itservices.msu.edu/oauth/token";

        /// <summary>
        /// The token endpoint.
        /// </summary>
        private const string UserInfoEndPoint = "https://oauth.itservices.msu.edu/oauth/me";

        /// <summary>
        /// The _app id.
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        /// The _app secret.
        /// </summary>
        private readonly string _clientSecret;


        private readonly string _redirectUri;

        public MsuOAuth2Client(string clientId, string clientSecret, string redirectUri) :
            this(clientId, clientSecret, redirectUri, new Dictionary<string, string>()) { }

        public MsuOAuth2Client(string clientId, string clientSecret, string redirectUri, Dictionary<string, string> requestedInfo)
            : base("msunet")
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("Client ID cannot be blank");
            }
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException("Client Secret cannot be blank");
            }

            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Verifies the authentication once redirected back to the return URL
        /// </summary>
        /// <param name="code">Code returned from MSU OAuth2</param>
        /// <param name="returnPageUrl">Page return URL</param>
        /// <returns>AuthenticationResults object with the user's data</returns>
        public AuthenticationResult VerifyAuthentication(string code, Uri returnPageUrl)
        {
            //var code = context.Request.QueryString["code"];
            if (string.IsNullOrEmpty(code))
            {
                return AuthenticationResult.Failed;
            }

            string accessToken = this.QueryAccessToken(returnPageUrl, code);
            if (accessToken == null)
            {
                return AuthenticationResult.Failed;
            }
            IDictionary<string, string> userData = this.GetUserData(accessToken);
            if (userData == null)
            {
                return AuthenticationResult.Failed;
            }
            string id = userData["uid"];
            string name = userData["email"];
            userData["accessToken"] = accessToken;
            return new AuthenticationResult(isSuccessful: true, provider: this.ProviderName, providerUserId: id, userName: name, extraData: userData);
        }

        /// <summary>
        /// This is here in case state ever gets passed back from MSU.
        /// This should be called before verifying the request, so that the url is rewritten to support this.
        /// </summary>
        public static void RewriteRequest()
        {
            var ctx = HttpContext.Current;

            var stateString = HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString == null || !stateString.Contains("__provider__=msunet"))
                return;

            var q = HttpUtility.ParseQueryString(stateString);
            q.Add(ctx.Request.QueryString);
            q.Remove("state");

            ctx.RewritePath(ctx.Request.Path + "?" + q);
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Uses the code returned from the MSU OAuth2 authentication to get a query token
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <param name="authorizationCode">Code returned by OAuth2</param>
        /// <returns></returns>
        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) // this is called by VerifyAuthentication 
        {
            /* Get the data ready to be POSTed */
            var postData = HttpUtility.ParseQueryString(string.Empty);
            postData.Add(new NameValueCollection
                {
                    { "grant_type", "authorization_code" },
                    { "code", authorizationCode },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "redirect_uri", _redirectUri },
                });

            /* Creates the Web Request*/
            var webRequest = (HttpWebRequest)WebRequest.Create(TokenEndPoint);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "application/json";

            using (var s = webRequest.GetRequestStream())
            using (var sw = new StreamWriter(s))
                sw.Write(postData.ToString());
            using (var webResponse = webRequest.GetResponse()) 
            {
                var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                    return null;

                using (var reader = new StreamReader(responseStream))
                {
                    var response = reader.ReadToEnd();
                    var json = JObject.Parse(response);
                    var accessToken = json.Value<string>("access_token");
                    return accessToken;
                }
            }
        }

        /// <summary>
        /// Gets the user's data once a query token is retrieved
        /// </summary>
        /// <param name="accessToken">Query token</param>
        /// <returns>Dictionary containing the user's data</returns>
        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            var uri = BuildUri(UserInfoEndPoint, new NameValueCollection { { "access_token", accessToken } });

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);

            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            {
                if (stream == null)
                    return null;

                using (var textReader = new StreamReader(stream))
                {
                    var json = textReader.ReadToEnd();
                    var msuInfo = JsonConvert.DeserializeObject<MSUOAuth2ResponseSchema>(json);
                    var extraData = msuInfo.info;
                    extraData.Add("uid", msuInfo.uid);
                    return extraData;
                }
            }
        }

        /// <summary>
        /// Retrieves the service login URL based on the return URL
        /// </summary>
        /// <param name="returnUrl">Return URL</param>
        /// <returns>Service Login URL</returns>
        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            return BuildUri(AuthorizationEndPoint, new NameValueCollection
            {
                {"response_type", "code"},
                {"client_id", _clientId},
                {"redirect_uri", _redirectUri}
            });
        }

        /// <summary>
        /// Builds the Uir from the base Uri and provided parameters
        /// </summary>
        /// <param name="baseUri">Base Uri</param>
        /// <param name="queryParameters">Query parameters to be included in the Uri</param>
        /// <returns>Uri with both the baseUri and query parameters</returns>
        private static Uri BuildUri(string baseUri, NameValueCollection queryParameters)
        {
            var q = System.Web.HttpUtility.ParseQueryString(string.Empty);
            q.Add(queryParameters);
            var builder = new UriBuilder(baseUri) { Query = q.ToString() };
            return builder.Uri;
        }



        #endregion
    }

    /// <summary>
    /// Schema for the response from MSU OAuth2
    /// </summary>
    public class MSUOAuth2ResponseSchema
    {
        public string provider { get; set; }
        public string uid { get; set; }
        public Dictionary<string, string> info { get; set; }
    }
}