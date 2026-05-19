using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;

namespace PracticaApp.Properties.Autorizare
{
    internal static class GoogleAuthService
    {
        private static readonly string[] Scopes =
        {
            "openid",
            "email",
            "profile"
        };

        public static async Task<GoogleUserProfile> SignInAsync()
        {
            GoogleOAuthSettings settings = LoadSettings();

            GoogleAuthorizationCodeFlow.Initializer initializer = new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = settings.ClientId,
                    ClientSecret = settings.ClientSecret
                },
                Prompt = "select_account"
            };

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                initializer,
                Scopes,
                "fitpro-google-user-" + Guid.NewGuid(),
                true,
                CancellationToken.None,
                new NullDataStore()
            );

            if (!string.IsNullOrWhiteSpace(credential.Token.IdToken))
                return await GetProfileFromIdTokenAsync(credential.Token.IdToken, settings.ClientId);

            if (!string.IsNullOrWhiteSpace(credential.Token.AccessToken))
                return await GetProfileFromUserInfoAsync(credential.Token.AccessToken);

            throw new InvalidOperationException("Google did not return a valid sign-in token.");
        }

        private static async Task<GoogleUserProfile> GetProfileFromIdTokenAsync(string idToken, string clientId)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                }
            );

            if (string.IsNullOrWhiteSpace(payload.Email))
                throw new InvalidOperationException("Google account does not contain an email address.");

            return new GoogleUserProfile(
                payload.Email.Trim(),
                payload.Name ?? payload.Email.Trim()
            );
        }

        private static async Task<GoogleUserProfile> GetProfileFromUserInfoAsync(string accessToken)
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using HttpResponseMessage response = await httpClient.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");
            response.EnsureSuccessStatusCode();

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            GoogleUserInfoResponse? userInfo = await JsonSerializer.DeserializeAsync<GoogleUserInfoResponse>(stream);

            if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Email))
                throw new InvalidOperationException("Google account does not contain an email address.");

            return new GoogleUserProfile(
                userInfo.Email.Trim(),
                string.IsNullOrWhiteSpace(userInfo.Name) ? userInfo.Email.Trim() : userInfo.Name.Trim()
            );
        }

        private static GoogleOAuthSettings LoadSettings()
        {
            string settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

            if (!File.Exists(settingsPath))
                throw new InvalidOperationException("Google OAuth settings were not found. Add appsettings.json near the application executable.");

            using FileStream stream = File.OpenRead(settingsPath);
            using JsonDocument document = JsonDocument.Parse(stream);

            if (!document.RootElement.TryGetProperty("GoogleOAuth", out JsonElement googleOAuth))
                throw new InvalidOperationException("GoogleOAuth section is missing in appsettings.json.");

            string clientId = ReadString(googleOAuth, "ClientId");
            string clientSecret = ReadString(googleOAuth, "ClientSecret");

            if (IsPlaceholder(clientId) || IsPlaceholder(clientSecret))
                throw new InvalidOperationException("Fill GoogleOAuth ClientId and ClientSecret in appsettings.json.");

            return new GoogleOAuthSettings(clientId, clientSecret);
        }

        private static string ReadString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out JsonElement value)
                ? value.GetString()?.Trim() ?? ""
                : "";
        }

        private static bool IsPlaceholder(string value)
        {
            return string.IsNullOrWhiteSpace(value) || value.Contains("YOUR_", StringComparison.OrdinalIgnoreCase);
        }

        private sealed record GoogleOAuthSettings(string ClientId, string ClientSecret);

        private sealed class GoogleUserInfoResponse
        {
            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }
    }

    internal sealed record GoogleUserProfile(string Email, string Name);
}
