using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BoxKite.Twitter.Models;
#if !(NETFX_CORE || PORTABLE)
using System.Security.Cryptography;
#else
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#endif

namespace BoxKite.Twitter
{
    public class UserSession : IUserSession
    {
        // used http://garyshortblog.wordpress.com/2011/02/11/a-twitter-oauth-example-in-c/

        readonly TwitterCredentials credentials;

        const string OauthSignatureMethod = "HMAC-SHA1";
        const string OauthVersion = "1.0";

        public UserSession(TwitterCredentials credentials)
        {
            this.credentials = credentials;
        }

        public Task<HttpResponseMessage> GetAsync(string url, SortedDictionary<string, string> parameters)
        {
            var querystring = parameters.Aggregate("", (current, entry) => current + (entry.Key + "=" + entry.Value + "&"));

            var oauth = BuildAuthenticatedResult(url, parameters, "GET");
            var fullUrl = url;

            var client = new HttpClient { MaxResponseContentBufferSize = 10 * 1024 * 1024 };
            client.DefaultRequestHeaders.Add("Authorization", oauth.Header);

            if (!string.IsNullOrWhiteSpace(querystring))
                fullUrl += "?" + querystring.Substring(0, querystring.Length - 1);

            return client.GetAsync(fullUrl);
        }

        public Task<HttpResponseMessage> PostAsync(string url, SortedDictionary<string, string> parameters)
        {
            var oauth = BuildAuthenticatedResult(url, parameters, "POST");
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", oauth.Header);

            var content = parameters.Aggregate(string.Empty, (current, e) => current + string.Format("{0}={1}&", e.Key, e.Value));

            var data = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");

            return client.PostAsync(url, data);
        }

        public HttpRequestMessage CreateGet(string url, SortedDictionary<string, string> parameters)
        {
            var querystring = parameters.Aggregate("", (current, entry) => current + (entry.Key + "=" + entry.Value + "&"));
            var oauth = BuildAuthenticatedResult(url, parameters, "GET");
            var fullUrl = url;

            if (!string.IsNullOrWhiteSpace(querystring))
                fullUrl += "?" + querystring.Substring(0, querystring.Length - 1);

            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.Add("Authorization", oauth.Header);
            request.Headers.Add("User-Agent", "dodo");
            return request;
        }

        private OAuth BuildAuthenticatedResult(string fullUrl, IEnumerable<KeyValuePair<string, string>> parameters, string method)
        {
            var url = fullUrl;

            var oauthToken = credentials.Token;
            var oauthConsumerKey = credentials.ConsumerKey;
            var rand = new Random();
            var oauthNonce = rand.Next(1000000000).ToString();

            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var oauthTimestamp = Convert.ToInt64(ts.TotalSeconds).ToString();

            //GS - When building the signature string the params
            //must be in alphabetical order. I can't be bothered
            //with that, get SortedDictionary to do it's thing
            var sd = new SortedDictionary<string, string>
                         {
                             {"oauth_consumer_key", oauthConsumerKey},
                             {"oauth_nonce", oauthNonce},
                             {"oauth_signature_method", OauthSignatureMethod},
                             {"oauth_timestamp", oauthTimestamp},
                             {"oauth_token", oauthToken},
                             {"oauth_version", OauthVersion}
                         };

            var querystring = "";

            var baseString = method.ToUpper() + "&" + Uri.EscapeDataString(url) + "&";

            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var entry in parameters)
                {
                    querystring += entry.Key + "=" + entry.Value + "&";
                }
            }

            foreach (var entry in parameters)
                sd.Add(entry.Key, entry.Value);

            foreach (var entry in sd)
            {
                string value;
                if (entry.Key == "status" || entry.Key == "text")
                {
                    value = Uri.EscapeDataString(entry.Value);
                }
                else
                {
                    value = entry.Value;
                }

                baseString += Uri.EscapeDataString(entry.Key + "=" + value + "&");
            }

            baseString = baseString.Substring(0, baseString.Length - 3);

            var signingKey = Uri.EscapeDataString(credentials.ConsumerSecret) + "&" + Uri.EscapeDataString(credentials.TokenSecret);
#if (NETFX_CORE || PORTABLE)
            var keyMaterial = CryptographicBuffer.ConvertStringToBinary(signingKey, BinaryStringEncoding.Utf8);
            var hmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            var macKey = hmacSha1Provider.CreateKey(keyMaterial);
            var dataToBeSigned = CryptographicBuffer.ConvertStringToBinary(baseString, BinaryStringEncoding.Utf8);
            var signatureBuffer = CryptographicEngine.Sign(macKey, dataToBeSigned);
            var signatureString = CryptographicBuffer.EncodeToBase64String(signatureBuffer);
#else
            var encoding = Encoding.UTF8;
            var crypto = new HMACSHA1
            {
                Key = encoding.GetBytes(signingKey)
            };
            var data = Encoding.UTF8.GetBytes(baseString);
            var hash = crypto.ComputeHash(data);
            var signatureString = Convert.ToBase64String(hash);
#endif
            return new OAuth
                       {
                           Nonce = oauthNonce,
                           SignatureMethod = OauthSignatureMethod,
                           Timestamp = oauthTimestamp,
                           ConsumerKey = oauthConsumerKey,
                           Token = oauthToken,
                           SignatureString = signatureString,
                           Version = OauthVersion,
                           Header = string.Format(
                                        "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", oauth_token=\"{4}\", oauth_signature=\"{5}\", oauth_version=\"{6}\"",
                                        Uri.EscapeDataString(oauthNonce),
                                        Uri.EscapeDataString(OauthSignatureMethod),
                                        Uri.EscapeDataString(oauthTimestamp),
                                        Uri.EscapeDataString(oauthConsumerKey),
                                        Uri.EscapeDataString(oauthToken),
                                        Uri.EscapeDataString(signatureString),
                                        Uri.EscapeDataString(OauthVersion))
                       };
        }

        private struct OAuth
        {
            public string Nonce;
            public string SignatureMethod;
            public string Timestamp;
            public string ConsumerKey;
            public string Token;
            public string SignatureString;
            public string Version;
            public string Header;
        }
    }
}