using Eklunda.Web.Core.Events;
using Eklunda.Web.Modules.Heating;
using Eklunda.Web.Persistence;
using Eklunda.Web.Shared;
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
    options.UseSqlite("Data Source=./eklunda.db");
});
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddEvents();
builder.Services.AddScoped<HeatingService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
