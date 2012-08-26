﻿using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace BoxKite.Twitter.Authentication
{
    public class TwitterAuthenticator
    {
        private readonly string clientID = ""; // twitter API calls these Consumers, or from their perspective consumers of their API
        private readonly string clientSecret = ""; // twitter API calls these Consumers, or from their perspective consumers of their API
        private string oAuthToken = "";
        private string OAuthTokenSecret = "";
        private string accessToken = "";
        private string accessTokenSecret = "";
        private string userID = "";
        private string screenName = "";

        public TwitterAuthenticator(string clientID, string clientSecret)
        {
            this.clientID = clientID;
            this.clientSecret = clientSecret;
        }

        public async Task<bool> AuthenticateUser()
        {
            if (string.IsNullOrWhiteSpace(clientID))
                throw new ArgumentException("ClientID must be specified", clientID);

            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("ClientSecret must be specified", clientSecret);

            var sinceEpoch = GenerateTimeStamp();
            var nonce = GenerateNonce();

            var sigBaseStringParams =
                string.Format(
                    "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method=HMAC-SHA1&oauth_timestamp={2}&oauth_version=1.0",
                    clientID,
                    nonce,
                    sinceEpoch);

            var sigBaseString = string.Format("POST&{0}&{1}", RequestTokenUrl.UrlEncode(), sigBaseStringParams.UrlEncode());
            var signature = GenerateSignature(clientSecret, sigBaseString, null);
            var dataToPost = string.Format(
                    "OAuth realm=\"\", oauth_nonce=\"{0}\", oauth_timestamp=\"{1}\", oauth_consumer_key=\"{2}\", oauth_signature_method=\"HMAC-SHA1\", oauth_version=\"1.0\", oauth_signature=\"{3}\"",
                    nonce,
                    sinceEpoch,
                    clientID,
                    signature.UrlEncode());

            var response = await PostData(RequestTokenUrl, dataToPost);

            if (string.IsNullOrWhiteSpace(response))
                return false;

            var oauthCallbackConfirmed = false;

            foreach (var splits in response.Split('&').Select(t => t.Split('=')))
            {
                switch (splits[0])
                {
                    case "oauth_token": //these tokens are request tokens, first step before getting access tokens
                        oAuthToken = splits[1];
                        break;
                    case "oauth_token_secret":
                        OAuthTokenSecret = splits[1];
                        break;
                    case "oauth_callback_confirmed":
                        if (splits[1].ToLower() == "true") oauthCallbackConfirmed = true;
                        break;
                }
            }

            if (oauthCallbackConfirmed)
                Process.Start(AuthenticateUrl + oAuthToken);

            return oauthCallbackConfirmed;
        }

        public TwitterCredentials GetUserCredentials()
        {
            var credentials = new TwitterCredentials
                                  {
                                      ConsumerKey = clientID,
                                      ConsumerSecret = clientSecret,
                                      Token = accessToken,
                                      TokenSecret = accessTokenSecret,
                                      ScreenName = screenName,
                                      UserID = userID,
                                      Valid = true
                                  };

            return credentials;
        }

        public async Task<bool> DelegateAuthentication(string pinAuthorizationCode)
        {
            if (string.IsNullOrWhiteSpace(pinAuthorizationCode))
                throw new ArgumentException("pinAuthorizationCode must be specified", pinAuthorizationCode);

            var sinceEpoch = GenerateTimeStamp();
            var nonce = GenerateNonce();

            var dataToPost = string.Format(
                    "OAuth realm=\"\", oauth_nonce=\"{0}\", oauth_timestamp=\"{1}\", oauth_consumer_key=\"{2}\", oauth_signature_method=\"HMAC-SHA1\", oauth_version=\"1.0\", oauth_verifier=\"{3}\", oauth_token=\"{4}\"",
                    nonce,
                    sinceEpoch,
                    clientID,
                    pinAuthorizationCode,
                    oAuthToken);

            var response = await PostData(AuthorizeTokenUrl, dataToPost);

            if (string.IsNullOrWhiteSpace(response))
                return false;

            var useraccessConfirmed = false;

            foreach (var splits in response.Split('&').Select(t => t.Split('=')))
            {
                switch (splits[0])
                {
                    case "oauth_token": //these tokens are request tokens, first step before getting access tokens
                        accessToken = splits[1];
                        break;
                    case "oauth_token_secret":
                        accessTokenSecret = splits[1];
                        break;
                    case "user_id":
                        userID = splits[1];
                        useraccessConfirmed = true;
                        break;
                    case "screen_name":
                        screenName = splits[1];
                        break;
                }
            }

            return useraccessConfirmed;
        }

        /* Utilities */
        const string SafeURLEncodeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
        const string RequestTokenUrl = "http://api.twitter.com/oauth/request_token";
        const string AuthenticateUrl = "https://api.twitter.com/oauth/authorize?oauth_token=";
        const string AuthorizeTokenUrl = "https://api.twitter.com/oauth/access_token";

        private static string GenerateSignature(string signingKey, string baseString, string tokenSecret)
        {
            var hmacsha1 = new HMACSHA1
            {
                Key =
                    Encoding.ASCII.GetBytes(string.Format("{0}&{1}", OAuthUrlEncode(signingKey),
                                                          string.IsNullOrEmpty(tokenSecret)
                                                              ? ""
                                                              : OAuthUrlEncode(tokenSecret)))
            };
            var dataBuffer = Encoding.ASCII.GetBytes(baseString);
            var hashBytes = hmacsha1.ComputeHash(dataBuffer);
            var signatureString = Convert.ToBase64String(hashBytes);
            return signatureString;
        }

        private static string OAuthUrlEncode(string value)
        {
            var result = new StringBuilder();

            foreach (var symbol in value)
            {
                if (SafeURLEncodeChars.IndexOf(symbol) != -1)
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

        private static string GenerateNonce()
        {
            var random = new Random();
            return random.Next(1234000, 99999999).ToString(CultureInfo.InvariantCulture);
        }

        private static string GenerateTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private static async Task<string> PostData(string url, string data)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
                request.Headers.Add("Accept-Encoding", "identity");
                request.Headers.Add("User-Agent", "Boxkite.Twitter");
                request.Headers.Add("Authorization", data);
                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return "";
            }
        }

    }
}