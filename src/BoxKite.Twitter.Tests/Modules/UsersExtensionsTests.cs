using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using BoxKite.Twitter.Modules;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BoxKite.Twitter.Tests.Modules
{
    [TestClass]
    public class UsersExtensionsTests
    {
        readonly TestScheduler sched = new TestScheduler();
        readonly TestableSession session = new TestableSession();

        [TestMethod]
        public async Task GetProfile_WhenUserSent_ReturnsOneValue()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            // act
            var observable = session.GetProfile("shiftkey");
            var result = sched.Start(() => observable);

            // NOTE: tests are not passing as expected. hrm.
            
            var results = result.GetMessagesOfType(NotificationKind.OnNext);
            Assert.AreEqual(1, results.Count());
        }

        [TestMethod]
        public async Task GetProfile_WhenUserSent_ReceivesNameAsParameter()
        {
            // arrange
            var screenName = "shiftkey";
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            // act
            session.GetProfile(screenName);

            Assert.IsTrue(session.ReceivedParameter("screen_name", screenName));
            Assert.IsTrue(session.ReceivedParameter("include_entities", "true"));
        }

        [TestMethod]
        public async Task GetProfile_WhenIdSent_ReturnsOneValue()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            // act
            var result = sched.Start(() => session.GetProfile(1234));

            var results = result.GetMessagesOfType(NotificationKind.OnNext);
            Assert.AreEqual(1, results.Count());
        }

        [TestMethod]
        public async Task GetProfile_WhenIdSent_ReceivesNameAsParameter()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            // act
            session.GetProfile(1234);

            Assert.IsTrue(session.ReceivedParameter("user_id", "1234"));
            Assert.IsTrue(session.ReceivedParameter("include_entities", "true"));
        }
    }
}
