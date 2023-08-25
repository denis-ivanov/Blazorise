using System;
using System.IO.Compression;
using System.Linq;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Docs.Core;
using Blazorise.Docs.Infrastructure;
using Blazorise.Docs.Models;
using Blazorise.Docs.Options;
using Blazorise.Docs.Services;
using Blazorise.FluentValidation;
using Blazorise.Icons.FontAwesome;
using Blazorise.RichTextEdit;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Blazorise.Docs;

public class Startup
{
    public Startup( IConfiguration configuration )
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices( IServiceCollection services )
    {
        //services.AddRazorPages();
        //services.AddServerSideBlazor();

        //services.AddServerSideBlazor().AddHubOptions( ( o ) =>
        //{
        //    o.MaximumReceiveMessageSize = 1024 * 1024 * 100;
        //} );

        // Add services to the container.
        services.AddRazorComponents();

        services.AddHttpContextAccessor();

        services
            .AddBlazorise( options =>
            {
                options.ProductToken = Configuration["Licensing:ProductToken"];
                options.Immediate = true; // optional
            } )
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons()
            .AddBlazoriseRichTextEdit()
            .AddBlazoriseFluentValidation();

        services.AddValidatorsFromAssembly( typeof( App ).Assembly );

        services.AddMemoryCache();
        services.AddScoped<Shared.Data.EmployeeData>();
        services.AddScoped<Shared.Data.CountryData>();
        services.AddScoped<Shared.Data.PageEntryData>();

        var emailOptions = Configuration.GetSection( "Email" ).Get<EmailOptions>();
        services.AddSingleton<IEmailOptions>( serviceProvider => emailOptions );

        services.AddSingleton<EmailSender>();

        services.AddResponseCompression( options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        } );

        services.Configure<BrotliCompressionProviderOptions>( options =>
        {
            options.Level = CompressionLevel.Fastest;
        } );

        services.Configure<GzipCompressionProviderOptions>( options =>
        {
            options.Level = CompressionLevel.SmallestSize;
        } );

        services.AddHsts( options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays( 365 );
        } );

        services.AddSingleton( new DocsVersionOptions
        {
            Versions = Configuration.GetSection( "DocsVersions" ).Get<DocsVersion[]>().ToList()
        } );

        services.AddHealthChecks();
    }

    public void Configure( WebApplication app )
    {
        //app.UseResponseCompression();

        if ( !app.Environment.IsDevelopment() )
        //{
        //    app.UseDeveloperExceptionPage();
        //}
        //else
        {
            app.UseExceptionHandler( "/Error" );
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        //app.UseRouting();

        //app.MapGet( "/robots.txt", async context =>
        //{
        //    await SeoGenerator.GenerateRobots( context );
        //} );

        //app.MapGet( "/sitemap.txt", async context =>
        //{
        //    await SeoGenerator.GenerateSitemap( context );
        //} );

        //app.MapGet( "/sitemap.xml", async context =>
        //{
        //    await SeoGenerator.GenerateSitemapXml( context );
        //} );

        //app.MapHealthChecks( "/healthcheck" );

        app.MapRazorComponents<App>();
    }
}
