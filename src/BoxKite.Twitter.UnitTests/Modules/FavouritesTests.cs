using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BoxKite.Twitter.Models;
using BoxKite.Twitter.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace BoxKite.Twitter.Tests.Modules
{
    [TestClass]
    public class FavouritesTests
    {
        readonly TestableSession session = new TestableSession();

        [TestMethod]
        public async Task GetFavourites_ForCurrentUser_ReturnsSet()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\favorites\\example.txt"));
            session.ExpectGet("https://api.twitter.com/1/favorites.json");

            var favourites = session.GetFavourites().ToListObservable();

            Assert.IsTrue(favourites.Count > 0);
        }

        [TestMethod]
        public async Task CreateFavourite_ForCurrentUser_ReturnsResult()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\favorites\\single.txt"));
            session.ExpectPost("https://api.twitter.com/1/favorites/create/1234.json");

            var tweet = new Tweet { Id = "1234" };

            var favourite = await session.CreateFavourite(tweet);

            Assert.IsNotNull(favourite);
        }

        [TestMethod]
        public async Task DestroyFavourite_ForCurrentUser_ReturnsResult()
        {
            // arrange
            session.Returns(await Json.FromFile("data\\favorites\\single.txt"));
            session.ExpectPost("https://api.twitter.com/1/favorites/destroy/1234.json");

            var tweet = new Tweet { Id = "1234" };

            var favourite = await session.DestroyFavourite(tweet);

            Assert.IsNotNull(favourite);
        }

    }
}
