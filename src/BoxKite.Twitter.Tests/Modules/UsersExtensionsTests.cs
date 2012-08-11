using System.Reactive.Linq;
using System.Threading.Tasks;
using BoxKite.Twitter.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BoxKite.Twitter.Tests.Modules
{
    [TestClass]
    public class UsersExtensionsTests
    {
        readonly TestableSession session = new TestableSession();

        [TestMethod]
        public async Task GetProfile_WhenUserSent_ReturnsOneValue()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            var user = session.GetProfile("shiftkey");
            
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task GetProfile_WhenUserSent_ReceivesNameAsParameter()
        {
            // arrange
            var screenName = "shiftkey";
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            // act
            var user = session.GetProfile(screenName);

            Assert.IsTrue(session.ReceivedParameter("screen_name", screenName));
            Assert.IsTrue(session.ReceivedParameter("include_entities", "true"));
        }


        [TestMethod]
        public async Task GetProfile_WhenIdSent_ReceivesNameAsParameter()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            // act
            var user = await session.GetProfile(1234);

            Assert.IsTrue(session.ReceivedParameter("user_id", "1234"));
            Assert.IsTrue(session.ReceivedParameter("include_entities", "true"));
        }

        [TestMethod]
        public async Task GetProfile_WhenIdSent_ParsesResult()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\users\\show.txt"));

            // act
            var user = await session.GetProfile(1234);

            Assert.IsNotNull(user);
        }
    }
}
