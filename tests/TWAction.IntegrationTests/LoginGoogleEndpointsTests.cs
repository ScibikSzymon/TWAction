using System.Net;

namespace TWAction.IntegrationTests;

public sealed class LoginGoogleEndpointsTests : IClassFixture<TWActionWebApplicationFactory>, IAsyncLifetime
{
    private readonly TWActionWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LoginGoogleEndpointsTests(TWActionWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task GetAuthGoogle_RedirectsToGoogleOAuth()
    {
        var response = await _client.GetAsync("/auth/google");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task GetAuthGoogle_RedirectUrlContainsRequiredOAuthParameters()
    {
        var response = await _client.GetAsync("/auth/google");

        var redirectUrl = response.Headers.Location?.ToString();
        Assert.NotNull(redirectUrl);
        Assert.Contains("client_id=", redirectUrl);
        Assert.Contains("redirect_uri=", redirectUrl);
        Assert.Contains("response_type=code", redirectUrl);
        Assert.Contains("scope=", redirectUrl);
        Assert.Contains("state=", redirectUrl);
    }

    [Fact]
    public async Task GetAuthGoogle_RedirectUrlContainsOpenIdScope()
    {
        var response = await _client.GetAsync("/auth/google");

        var redirectUrl = response.Headers.Location?.ToString();
        Assert.NotNull(redirectUrl);
        Assert.Contains("openid", redirectUrl);
        Assert.Contains("email", redirectUrl);
        Assert.Contains("profile", redirectUrl);
    }

    [Fact]
    public async Task GetAuthGoogleCallback_WithoutCode_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/auth/google/callback");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAuthGoogleCallback_WithInvalidCode_ReturnsBadGateway()
    {
        var response = await _client.GetAsync("/auth/google/callback?code=invalid_code");

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }
}
