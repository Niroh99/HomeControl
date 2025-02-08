using HomeControl.Helpers;
using HomeControl.Database;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FW_HomeControl");
AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetCurrentDirectory());

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5000);
});

builder.Services.AddControllers();

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IDatabaseConnection>((serviceProvider) => new DatabaseConnection(connectionString));

builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(HomeControl.Pages.Media.IndexModel.BasePath));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(HomeControl.Pages.Media.IndexModel.BasePath),
    ContentTypeProvider = FileHelper.CreateFileExtensionContentTypeProvider(),
    RequestPath = new PathString("/Media"),
    ServeUnknownFileTypes = true,
    DefaultContentType = FileHelper.ApplicationOctedStreamContentType,
});

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.MapControllers();

app.Run();
