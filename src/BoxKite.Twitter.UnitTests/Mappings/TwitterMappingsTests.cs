using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using BoxKite.Twitter.Extensions;
using BoxKite.Twitter.Mappings;
using BoxKite.Twitter.Models;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BoxKite.Twitter.Tests.Mappings
{
    [TestClass]
    public class TwitterMappingsTests
    {
        readonly TestScheduler sched = new TestScheduler();
        readonly Func<Tweet, bool> isDateTimeSet = t => t.Time != DateTimeOffset.MinValue;

        [TestMethod]
        public void FromSearchResponse_UsingSampleData_CanBeParsed()
        {
            var task = Json.FromFile(@"data\searchresponse.txt");
            task.Wait();
            var contents = task.Result;

            Assert.IsTrue(contents.FromSearchResponse().Any());
        }

        [TestMethod]
        public void FromSearchResponse_UsingSampleData_PopulatesDates()
        {
            var task = Json.FromFile(@"data\searchresponse.txt");
            task.Wait();
            var contents = task.Result;
            var results = contents.FromSearchResponse();

            Assert.IsTrue(results.All(isDateTimeSet));
        }

        [TestMethod]
        public void FromTweet_UsingSampleData_CanBeParsed()
        {
            var task = Json.FromFile(@"data\timeline.txt");
            task.Wait();
            var contents = task.Result;

            var result = sched.Start(contents.GetList);

            Assert.IsTrue(result.Messages.Any());
        }

        [TestMethod]
        public async Task FromTweet_UsingSampleData_PopulatesDates()
        {
            var task = await Json.FromFile(@"data\timeline.txt");
            
            var observer = sched.Start(task.GetList);

            var itemsReceieved = observer.GetMessagesOfType(NotificationKind.OnNext);

            Assert.IsTrue(itemsReceieved.All(m => isDateTimeSet(m.Value.Value)));
        }

        [TestMethod]
        public void Deserialize_SingleTweet_PopulatesFields()
        {
            var task = Json.FromFile(@"data\sampletweet.txt");
            task.Wait();
            var contents = task.Result;
            var tweet = contents.GetSingle();

            Assert.IsNotNull(tweet);
            Assert.IsTrue(tweet.User != null);
            Assert.IsTrue(tweet.Time != DateTimeOffset.MinValue);
        }

        [TestMethod]
        public void Deserialize_SingleTweet_DecodesText()
        {
            var task = Json.FromFile(@"data\sampletweet-withencoding.txt");
            task.Wait();
            var contents = task.Result;
            var tweet = contents.GetSingle();

            Assert.IsTrue(tweet.Text == "My take: design & development are facets of the same process. Design leads development, development informs design. #fowd");
        }

        [TestMethod]
        public void Deserialize_SingleTweet_DecodesLessThanAndGreaterThan()
        {
            var task = Json.FromFile(@"data\sampletweet-withencoding-2.txt");
            task.Wait();
            var contents = task.Result;
            var tweet = contents.GetSingle();

            Assert.IsTrue(tweet.Text == "School -> Research Assignment -> Uni -> Assignment -> Library -> Marking. #fml");
        }

        [TestMethod]
        public void Deserialize_Retweet_PopulatesFields()
        {
            // @hhariri retweets @wilderminds
            var task = Json.FromFile(@"data\retweet.txt");
            task.Wait();
            var contents = task.Result;

            var tweet = contents.GetSingle();

            Assert.IsNotNull(tweet);
            Assert.IsTrue(tweet.User.Name == "Wilder Minds");
            Assert.IsTrue(tweet.RetweetedBy.Name == "Hadi Hariri");
        }

        [TestMethod]
        public void ParseDateTime_UsingValidTwitterTime_ReturnsResult()
        {
            var dateTime = "Sun Apr 15 02:31:50 +0000 2012".ToDateTimeOffset();

            Assert.AreNotEqual(DateTimeOffset.MinValue, dateTime);
        }

        [TestMethod]
        public void ParseDateTime_UsingValidTime_ReturnsResult()
        {
            var dateTime = "Sun, 15 Apr 2012 02:31:50 +0000".ToDateTimeOffset();

            Assert.AreNotEqual(DateTimeOffset.MinValue, dateTime);
        }

        [TestMethod]
        public void Deserialize_TweetWithEntityField_SpecifiesHashtag()
        {
            var task = Json.FromFile(@"data\entity-with-hashtag.txt");
            task.Wait();
            var contents = task.Result;

            var tweet = contents.GetSingle();

            Assert.IsNotNull(tweet);
            Assert.IsTrue(tweet.Text == "Loved #devnestSF");
            Assert.IsTrue(tweet.Hashtags.Any());
        }

        [TestMethod]
        public void Deserialize_TweetWithEntityField_SpecifiesUserMentions()
        {
            var task = Json.FromFile(@"data\entity-with-mentions.txt");
            task.Wait();
            var contents = task.Result;

            var tweet = contents.GetSingle();

            Assert.IsNotNull(tweet);
            Assert.IsTrue(tweet.Text == "@rno Et demi!");
            Assert.IsTrue(tweet.Mentions.Any());
        }
    }
}