using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using WASSLink.Configuration.DependencyInjection;
using WASSLink.Download.DependencyInjection;
using WASSLink.Library.DependencyInjection;
using WASSLink.Player.DependencyInjection;
using WASSLink.Search.DependencyInjection;
using WASSLink.Plugins.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddConfigurationServices()
    .AddDownloadServices()
    .AddLibraryServices()
    .AddPlayerServices()
    .AddSearchServices()
    .AddPluginServices();

var app = builder.Build();

Console.WriteLine("GowLink CLI iniciado.");

await app.RunAsync();
