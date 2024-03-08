using HackerNews.Interfaces;
using HackerNews.Models;
using HackerNews.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<HackerNewsConfig>(builder.Configuration.GetSection("HackerNewsConfig"));
builder.Services.Configure<CacheConfig>(builder.Configuration.GetSection("CacheConfig"));
builder.Services.AddControllers();
builder.Services.AddHttpClient("", (provider, client) =>
{
    IOptions<HackerNewsConfig>? hackerNewsConfig = provider.GetService<IOptions<HackerNewsConfig>>();

    if (hackerNewsConfig == null)
    {
        throw new NullReferenceException(nameof(hackerNewsConfig));
    }

    client.BaseAddress = new Uri(hackerNewsConfig.Value.BaseUrl);
});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IHackerNewsService, HackerNewsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
