using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BoxKite.Twitter.Tests.Modules
{
    public class TestableSession : IUserSession
    {
        string contents;
        SortedDictionary<string, string> receviedParameters;
        string expectedGetUrl;
        string expectedPostUrl;

        public Task<HttpResponseMessage> GetAsync(string relativeUrl, SortedDictionary<string, string> parameters)
        {
            if (!string.IsNullOrWhiteSpace(expectedGetUrl))
            {
                Assert.AreEqual(expectedGetUrl, relativeUrl);
            }

            this.receviedParameters = parameters;

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(contents);
            return Task.FromResult(response);
        }

        public Task<HttpResponseMessage> PostAsync(string relativeUrl, SortedDictionary<string, string> parameters)
        {
            if (!string.IsNullOrWhiteSpace(expectedPostUrl))
            {
                Assert.AreEqual(expectedPostUrl, relativeUrl);
            }

            this.receviedParameters = parameters;

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(contents);
            return Task.FromResult(response);
        }

        public HttpRequestMessage CreateGet(string fullUrl, SortedDictionary<string, string> parameters)
        {
            throw new System.NotImplementedException();
        }

        public void Returns(string contents)
        {
            this.contents = contents;
        }

        public bool ReceivedParameter(string key, string value)
        {
            if (!receviedParameters.ContainsKey(key))
                return false;

            var actualValue = receviedParameters[key];
            return actualValue == value;
        }

        public void ExpectGet(string url)
        {
            expectedGetUrl = url;
        }

        public void ExpectPost(string url)
        {
            expectedPostUrl = url;
        }
    }
}