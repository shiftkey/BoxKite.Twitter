using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BoxKite.Twitter.Tests.Modules
{
    public class TestableSession : IUserSession
    {
        private string contents;
        private SortedDictionary<string, string> receviedParameters;

        public Task<HttpResponseMessage> GetAsync(string relativeUrl, SortedDictionary<string, string> parameters)
        {
            this.receviedParameters = parameters;

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(contents);
            return Task.FromResult(response);
        }

        public Task<HttpResponseMessage> PostAsync(string relativeUrl, SortedDictionary<string, string> parameters)
        {
            throw new System.NotImplementedException();
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
    }
}