using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

            var favourites = await session.GetFavourites();

            Assert.IsTrue(favourites.Any());
        }

    }
}
