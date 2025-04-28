using HomeControl.Helpers;
using HomeControl.Database;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Net;
using HomeControl.Events;
using HomeControl.Integrations;
using HomeControl.Weather;
using HomeControl.Routines;
using HomeControl.Actions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("FW_HomeControl");
AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetCurrentDirectory());

var timerInterval = builder.Configuration.GetValue<int?>("TimerInterval");

builder.Services.AddControllers();

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(HomeControl.Pages.Media.IndexModel.BasePath));
builder.Services.AddSingleton<IWeatherService, WeatherService>();

builder.Services.AddScoped<IDatabaseConnection>((serviceProvider) => new DatabaseConnection(connectionString, serviceProvider));
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRoutinesService, RoutinesService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IActionsService, ActionsService>();

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

if (timerInterval != null)
{
    var timer = new System.Timers.Timer(TimeSpan.FromSeconds(timerInterval.Value));

    timer.Elapsed += async (s, e) =>
    {
        using var serviceScope = app.Services.CreateScope();

        var eventService =  serviceScope.ServiceProvider.GetService<IEventService>();
        var routinesService = serviceScope.ServiceProvider.GetService<IRoutinesService>();

        await eventService.ExecuteScheduledEventsAsync();
        await routinesService.ExecuteActiveRoutinesAsync();
    };

    timer.Start();
}

app.Run();
