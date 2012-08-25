using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;
using System.Security.Cryptography;


namespace BoxKite.Twitter.Authentication
{
    public class TwitterAuthenticator
    {
        const string OauthSignatureMethod = "HMAC-SHA1";
        const string OauthVersion = "1.0";

        private static async Task<string> PostData(string url, string data)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                request.Headers.Add("Accept-Encoding", "identity");
                request.Headers.Add("User-Agent", "Boxkite.Twitter");
                request.Headers.Add("Authorization", data);
                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static async Task<TwitterCredentials> AuthenticateUser(string clientId, string clientSecret, string callbackUrl)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("clientId must be specified", clientId);

            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("clientSecret must be specified", clientSecret);

            if (string.IsNullOrWhiteSpace(callbackUrl))
                throw new ArgumentException("callbackUrl must be specified", callbackUrl);

            var sinceEpoch = GenerateTimeStamp();
            var nonce = GenerateNonce();
            const string requestTokenUrl = "http://api.twitter.com/oauth/request_token";

            var sigBaseStringParams =
                string.Format(
                    "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method=HMAC-SHA1&oauth_timestamp={2}&oauth_version=1.0",
                    clientId,
                    nonce,
                    sinceEpoch);

            var sigBaseString = string.Format("GET&{0}&{1}", requestTokenUrl.UrlEncode(), sigBaseStringParams.UrlEncode());
            var signature = GenerateSignature(clientSecret, sigBaseString, null);
            var dataToPost = string.Format(
                    "OAuth realm=\"\", oauth_nonce=\"{0}\", oauth_timestamp=\"{1}\", oauth_consumer_key=\"{2}\", oauth_signature_method=\"HMAC-SHA1\", oauth_version=\"1.0\", oauth_signature=\"{3}\"",
                    nonce,
                    sinceEpoch,
                    clientId,
                    signature.UrlEncode());

            var response = await PostData(requestTokenUrl, dataToPost);

            if (string.IsNullOrWhiteSpace(response))
                return TwitterCredentials.Null;

            string oauthToken = null;

            foreach (var splits in response.Split('&').Select(t => t.Split('=')))
            {
                switch (splits[0])
                {
                    case "oauth_token":
                        oauthToken = splits[1];
                        break;
                }
            }

            return await DelegateAuthentication(clientId, clientSecret, callbackUrl, oauthToken);
        }

        private static async Task<TwitterCredentials> GetUserCredentials(string consumerKey, string consumerSecret, string responseText)
        {
            var args = responseText.Substring(responseText.IndexOf("?") + 1)
                                   .Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);

            var verifier = "";
            var token = "";

            foreach (var a in args)
            {
                var index = a.IndexOf("=");
                var key = a.Substring(0, index);
                var value = a.Substring(index + 1);

                if (key.Equals("oauth_token", StringComparison.OrdinalIgnoreCase))
                    token = value;

                if (key.Equals("oauth_verifier", StringComparison.OrdinalIgnoreCase))
                    verifier = value;
            }

            var url = "https://api.twitter.com/oauth/access_token?oauth_verifier=" + verifier;

            var oauthToken = token;
            var oauthConsumerKey = consumerKey;
            var rand = new Random();
            var oauthNonce = rand.Next(1000000000).ToString();

            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var oauthTimestamp = Convert.ToInt64(ts.TotalSeconds).ToString();

            var sd = new SortedDictionary<string, string>
                         {
                             {"oauth_version", OauthVersion},
                             {"oauth_consumer_key", oauthConsumerKey},
                             {"oauth_nonce", oauthNonce},
                             {"oauth_signature_method", OauthSignatureMethod},
                             {"oauth_timestamp", oauthTimestamp},
                             {"oauth_token", oauthToken}
                         };

            var baseString = "GET&" + Uri.EscapeDataString(url) + "&";
            foreach (var entry in sd)
            {
                baseString += Uri.EscapeDataString(entry.Key + "=" + entry.Value + "&");
            }

            //GS - Remove the trailing ambersand char, remember 
            //it's been urlEncoded so you have to remove the last 3 chars - %26
            baseString = baseString.Substring(0, baseString.Length - 3);

            var signingKey = Uri.EscapeDataString(consumerSecret) + "&";
            var signatureString = GenerateSignature(signingKey, baseString, null);
            var hwr = WebRequest.Create(url);

            var authorizationHeaderParams = string.Format(
                "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", oauth_token=\"{4}\", oauth_signature=\"{5}\", oauth_version=\"{6}\"",
                Uri.EscapeDataString(oauthNonce),
                Uri.EscapeDataString(OauthSignatureMethod),
                Uri.EscapeDataString(oauthTimestamp),
                Uri.EscapeDataString(oauthConsumerKey),
                Uri.EscapeDataString(oauthToken),
                Uri.EscapeDataString(signatureString),
                Uri.EscapeDataString(OauthVersion));

            hwr.Headers["Authorization"] = authorizationHeaderParams;

            var response = await hwr.GetResponseAsync();

            var reader = new StreamReader(response.GetResponseStream());
            var content = reader.ReadToEnd();

            var credentials = new TwitterCredentials
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret
            };

            foreach (var a in content.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var index = a.IndexOf("=");
                var key = a.Substring(0, index);
                var value = a.Substring(index + 1);

                if (key.Equals("oauth_token", StringComparison.OrdinalIgnoreCase))
                    credentials.Token = value;

                if (key.Equals("oauth_token_secret", StringComparison.OrdinalIgnoreCase))
                    credentials.TokenSecret = value;

                if (key.Equals("screen_name", StringComparison.OrdinalIgnoreCase))
                    credentials.ScreenName = value;
            }

            credentials.Valid = true;

            return credentials;
        }

        const string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        private static string UrlEncode(string value)
        {
            var result = new StringBuilder();

            foreach (var symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }

            return result.ToString();
        }

        private static string GenerateSignature(string signingKey, string baseString, string tokenSecret)
        {
            var hmacsha1 = new HMACSHA1();
            hmacsha1.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}", UrlEncode(signingKey), string.IsNullOrEmpty(tokenSecret) ? "" : UrlEncode(tokenSecret)));
            var dataBuffer = Encoding.ASCII.GetBytes(baseString);
            var hashBytes = hmacsha1.ComputeHash(dataBuffer);
            var signatureString = Convert.ToBase64String(hashBytes);
            return signatureString;
        }

        private static async Task<TwitterCredentials> DelegateAuthentication(string clientId, string clientSecret, string callbackUrl, string oauthToken)
        {
            if (string.IsNullOrWhiteSpace(oauthToken))
                return TwitterCredentials.Null;
            return TwitterCredentials.Null; // TODO
        }

        public static string GenerateNonce()
        {
            var random = new Random();
            return random.Next(1234000, 99999999).ToString();
        }

        public static string GenerateTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}