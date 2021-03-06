using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xamarin.Cognitive.Speech
{
	/// <summary>
	/// Client that authenticates to the Speech API.
	/// </summary>
	class AuthenticationClient
	{
		readonly string subscriptionKey;
		readonly Endpoint authEndpoint;
		readonly HttpClient client;

		internal string Token { get; private set; }

		internal SpeechRegion SpeechRegion { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.Cognitive.Speech.AuthenticationClient"/> class.
		/// </summary>
		/// <param name="authEndpoint">The auth endpoint to get an auth token from.</param>
		/// <param name="subscriptionKey">Subscription identifier.</param>
		/// <param name="speechRegion">The <see cref="SpeechRegion"/> where your speech service is deployed.</param>
		public AuthenticationClient (Endpoint authEndpoint, string subscriptionKey, SpeechRegion speechRegion)
		{
			this.authEndpoint = authEndpoint;
			this.subscriptionKey = subscriptionKey;
			this.SpeechRegion = speechRegion;

			client = new HttpClient ();
			client.DefaultRequestHeaders.Add (Constants.Keys.SubscriptionKey, this.subscriptionKey);
		}

		/// <summary>
		/// Calls to the authentication endpoint to get a JWT token that is cached.
		/// </summary>
		/// <param name="forceNewToken">If set to <c>true</c>, force new token even if there is already a cached token.</param>
		public async Task Authenticate (bool forceNewToken = false)
		{
			if (string.IsNullOrEmpty (Token) || forceNewToken)
			{
				ClearToken ();
				Token = await FetchToken ();
			}
		}

		public void ClearToken ()
		{
			Token = null;
		}

		async Task<string> FetchToken ()
		{
			try
			{
				var uri = authEndpoint.ToUriBuilder (SpeechRegion).Uri;

				Debug.WriteLine ($"{DateTime.Now} :: Request Uri: {uri}");

				var result = await client.PostAsync (uri, null);

				if (result.IsSuccessStatusCode)
				{
					Debug.WriteLine ("New authentication token retrieved at {0}", DateTime.Now);

					return await result.Content.ReadAsStringAsync ();
				}

				throw new Exception ($"Unable to authenticate, auth endpoint returned: status code {result.StatusCode} ; Reason: {result.ReasonPhrase}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine ("Error during auth post: {0}", ex);
				throw;
			}
		}
	}
}