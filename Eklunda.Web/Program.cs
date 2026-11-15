using Eklunda.Web.Core.Events;
using Eklunda.Web.Core.Intentions;
using Eklunda.Web.Core.Notifications;
using Eklunda.Web.Core.Users;
using Eklunda.Web.Modules.Heating;
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

// Behind Caddy on the Pi, which terminates HTTPS.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddUsers();
builder.Services.AddEvents();
builder.Services.AddPushNotifications();
builder.Services.AddScoped<IntentionService>();
builder.Services.AddScoped<HeatingService>();
builder.Services.AddScoped<HeatingNotifier>();

var app = builder.Build();

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
app.MapHeatingEndpoints();

app.Run();
