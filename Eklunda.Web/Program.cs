using Eklunda.Web.Core.Events;
using Eklunda.Web.Core.Intentions;
using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating;
using Eklunda.Web.Modules.Suggestions;
using Eklunda.Web.Persistence;
using Eklunda.Web.Shared;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
    {
        // Discover pages in both /Pages and /Modules, without the folder name in the URL.
        options.RootDirectory = "/";
        options.Conventions.Add(new FolderPrefixRouteConvention("Pages", "Modules"));
    })
    .AddRazorOptions(options =>
    {
        options.PageViewLocationFormats.Add("/Pages/Shared/{0}.cshtml");
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

// Behind Caddy on the Pi, which terminates HTTPS. Caddy runs in another container, so its
// address isn't known up front. The app is only reachable through Caddy.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddUsers();
builder.Services.AddEvents();
builder.Services.AddPushNotifications();
builder.Services.AddScoped<IntentionService>();
builder.Services.AddScoped<IntentionNotifier>();
builder.Services.AddHostedService<IntentionReminderJob>();
builder.Services.AddScoped<HeatingService>();
builder.Services.AddScoped<HeatingNotifier>();
builder.Services.AddScoped<SuggestionService>();
builder.Services.AddScoped<SuggestionNotifier>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Icons, manifest and service worker load before anyone has picked a name.
app.MapStaticAssets()
   .AllowAnonymous();
app.MapRazorPages()
   .WithStaticAssets();
app.MapPushEndpoints();
app.MapIntentionEndpoints();
app.MapHeatingEndpoints();

app.Run();
