using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CodeVerdict.Web.Tests.Smoke;

public sealed class HomePageTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Home_page_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.EnsureSuccessStatusCode();
    }
}
