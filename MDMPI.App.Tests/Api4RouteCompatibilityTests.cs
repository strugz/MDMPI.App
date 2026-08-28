using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MDMPI.App.Tests;

public sealed class Api4RouteCompatibilityTests
{
    [Fact]
    public async Task Api4Prefix_MapsToApiControllersInDevelopment()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["ALLOW_PRODUCTION_DB"] = "true"
                        });
                });
            });

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        // An empty multipart request is rejected before the AI service runs.
        // A 400 proves /api4 was rewritten and matched the /api controller;
        // an unmapped prefix would return 404.
        using var form = new MultipartFormDataContent();
        using var response =
            await client.PostAsync("/api4/Gemini/analyze-file", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
