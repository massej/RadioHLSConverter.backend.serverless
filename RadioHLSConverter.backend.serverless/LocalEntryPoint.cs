/*
 * LocalEntryPoint.cs
 * Local entry point used when running the software local.
 * Date : 2021-05-24.
 * By : Jonathan Massé
 */


// Includes.
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Logging;


namespace RadioHLSConverter.backend.serverless
{
    /// <summary>
    /// LocalEntryPoint
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureLogging(logging =>
                {
                    logging.AddFile("logs/console-{Date}.log"); // <== Exception
                })
                .UseWindowsService()
                .UseSystemd()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(o => { o.Limits.KeepAliveTimeout = TimeSpan.MaxValue; });
                });
    }
}