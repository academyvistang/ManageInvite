using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AspNetCore.Identity.DocumentDb;
using CosmosDBService;
using IdentitySample.Models;
using IdentitySample.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace ManageInvite
{

    public struct DocumentDbClientConfig
    {
        public string EndpointUri;
        public string AuthenticationKey;
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private static readonly string EndpointUri = "https://cabbashlive.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "BheHZZiUm1QD6MGrKRwouY1QJqUC19S1PznR6Q4h5Qcy86Zr42GjA5hZTqEweami67CwejOBAODgMF16lL7nmg==";




        private void InitializeDocumentClient(DocumentClient client)
        {
            try
            {
                // Does the DB exist?
                var db = client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri("AspNetCoreIdentityCabbash")).Result;
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex.GetType() == typeof(DocumentClientException) && ((DocumentClientException)ex).StatusCode == HttpStatusCode.NotFound)
                    {
                        // Create DB
                        var db = client.CreateDatabaseAsync(new Database() { Id = "AspNetCoreIdentityCabbash" }).Result;
                        return true;
                    }

                    return false;
                });
            }

            try
            {
                // Does the Collection exist?
                var collection = client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri("AspNetCoreIdentityCabbash", "AspNetIdentity")).Result;
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex =>
                {
                    if (ex.GetType() == typeof(DocumentClientException) && ((DocumentClientException)ex).StatusCode == HttpStatusCode.NotFound)
                    {
                        DocumentCollection collection = new DocumentCollection() { Id = "AspNetIdentity" };
                        collection = client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri("AspNetCoreIdentityCabbash"), collection).Result;

                        return true;
                    }

                    return false;
                });
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultDocumentClientForIdentity(
              Configuration.GetValue<Uri>("DocumentDbClient:EndpointUri"),
              Configuration.GetValue<string>("DocumentDbClient:AuthorizationKey"),
              afterCreation: InitializeDocumentClient);

            // Add framework services.

            services.AddAuthentication();

            services.AddIdentity<ApplicationUser, DocumentDbIdentityRole>()
                .AddDocumentDbStores(options =>
                {
                    options.UserStoreDocumentCollection = "AspNetIdentity";
                    options.Database = "AspNetCoreIdentityCabbash";
                });


            //services.AddAuthentication(
            //options => options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            //services.AddAuthentication(
            //options => options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            //services.AddSingleton<DocumentDbService>(x => new DocumentDbService(Configuration.GetSection("DocumentDb")));
            //services.Configure<Auth0Settings>(Configuration.GetSection("Auth0"));

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddMemoryCache();

            services.AddSingleton<IDbProvider>(x => new DbProvider(EndpointUri, PrimaryKey, null));
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseCors(x =>
            {
                x.WithOrigins("http://localhost:4200")
                .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });

            //app.UseSignalR(route =>
            //{
            //    route.MapHub<driverlocationnotifications>("/driverlocationnotifications");
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
