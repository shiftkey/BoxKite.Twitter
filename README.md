BoxKite.Twitter
===============

## About

I started writing this code a few months ago to support an app. Unfortunately I came back from holidays and wanted to rework the UI so the app has remained in my backlog.

I've not looked at the state of this code, but I'm wrapping it in tests as I go through and clean it up so hopefully that helps.

## Getting Started

Its up on NuGet as a Pre-Release package:

`Install Package BoxKite.Twtter -Pre`

You need an application token and key from Twitter before you can get started. Ensuring you have the right permissions is out of scope of this topic (report an issue if you want me to document this).

### Authentication

Once you have those, have a look at authenticating the user (this currently leverages the WebAuthenticationBroker components in Windows 8):

	public async Task SignInAndGetProfile(string clientKey, string clientSecret) 
	{
		// NOTE: I don't think the callback URL is required
        var credentials = await TwitterAuthenticator.AuthenticateUser(key, secret,"http://somesite.com/callback");
	    if (!credentials.Valid)
        {
            // oops, something went wrong with the authentication step
            return;
	    }

	    var session = new UserSession(credentials);
	    var profile = await session.GetProfile(credentials.ScreenName);
	}

On the desktop it's a bit different (we need to open a browser and the user needs to add the PIN back into the app) but the code itself is still relatively simple:

    var authenticator = new TwitterAuthenticator("clientId", "clientSecret");
    bool flowStarted = await authenticator.StartAuthenticatorFlow();
    if (!flowStarted) UserCancelled();
    string pin = // get PIN from user
    bool isTokenValid = await authenticator.ValidateInput(pin);
    if (isTokenValid)
	{
        var tc = authenticator.GetUserCredentials();
        Console.WriteLine(tc.ScreenName + " is authorised to use BoxKite.Twitter. Yay");

        var session = new UserSession(tc);
        var profile = await session.GetProfile(credentials.ScreenName);
    }
            
**NOTE:** This feature is being discussed in a pull request and is not currently available.

### Reactive Extensions All The Things

Rx gurus can shout at me where I'm doing things wrong. Needs some cleanup in specific areas but I actually prefer this API to doing async/await/Task everywhere.

If you want to throw rotten vegetables at me for that last remark, please form an orderly queue on the left.

### Anonymous and User sessions

A couple of the APIs exposed by Twitter do not require authentication to make requests (this will be [deprecated](https://dev.twitter.com/blog/changes-coming-to-twitter-api) in the future). 

	public async Task SearchForSomething(string text) 
	{
        var session = new AnonymousSession();
	    var result = await session.SearchFor("twitter");
	}

### Await-able Results - A Warning

Subscribers are cool. Await is cool. But don't forget to differentiate between the two.

	public async Task SearchForFirstResult(string text) 
	{
        var session = new AnonymousSession();
	    var result = await session.SearchFor("twitter");
	}

	public void SubscribeToSearchResults(string text) 
	{
        var session = new AnonymousSession();
	    session.SearchFor("twitter")
	    	   .Subscribe(t => AddToCollection(t));
	}

If you are working with observables that are expected to return more than one result, you shouldn't await - that will give you the first result only.

### Extension methods

Rather than using god classes everywhere, I've broken out the API calls into extension methods. For example, this is all the code I require to fetch a user's profile:

    public static IObservable<User> GetProfile(this IUserSession session, string screenName)
    {
        var parameters = new SortedDictionary<string, string>
                             {
                                 {"screen_name", screenName},
                                 {"include_entities", "true"},
                             };
        var url = Api.Resolve("/1/users/show.json");
        return session.GetAsync(url, parameters)
                      .ContinueWith(c => c.MapToSingleUser())
                      .ToObservable();
    }

Pros: 

 - Single Responsibility Principle - API calls work out the parameters necessary, delegating the plumbing to other components.
 - Maintainability - simple template to read and tweak based on the input and output parameters

Cons: 

 - namespace clutter is still unavoidable. you shouldn't need to add additional namespaces, but perhaps that is unavoidable.



### User Streams

I had way too much fun getting user streams supported. It felt like a natural fit for applying Rx to it, and the basics are supported too:

 	public async Task StreamResults(TwitterCredentials credentials) 
	{
        var session = new UserSession(credentials);
        var stream = session.GetUserStream();
	    stream.Tweets.Subscribe(t => Debug.WriteLine(t.Text));
	    stream.Start();
	}

There's some more events to hook into from user streams, but priorities.