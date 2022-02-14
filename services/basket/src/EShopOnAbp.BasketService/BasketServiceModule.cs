﻿using EShopOnAbp.BasketService.Entities;
using EShopOnAbp.BasketService.Localization;
using EShopOnAbp.CatalogService.Grpc;
using EShopOnAbp.Shared.Hosting.AspNetCore;
using EShopOnAbp.Shared.Hosting.Microservices;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AutoMapper;
using Volo.Abp.Caching;
using Volo.Abp.Http.Client;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace EShopOnAbp.BasketService
{
    [DependsOn(
    )]
    public class BasketServiceModule : AbpModule 
    {
        public const string RemoteServiceName = "Basket";
        
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();

            ConfigureAutoMapper();
            ConfigureGrpc(context);
            ConfigureDistributedCache();
            ConfigureVirtualFileSystem();
            ConfigureLocalization();
            ConfigureHttpApiProxy(context);
            ConfigureAuthentication(context, configuration);
        }
        
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCorrelationId();
            app.UseCors();
            app.UseAbpRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAbpClaimsMap();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket Service API");
                options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
                options.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
            });
            app.UseAbpSerilogEnrichers();
            app.UseAuditing();
            app.UseUnitOfWork();
            app.UseConfiguredEndpoints();
        }

        private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
        {
            JwtBearerConfigurationHelper.Configure(context, "BasketService");

            SwaggerWithAuthConfigurationHelper.Configure(
                context: context,
                authority: configuration["AuthServer:Authority"],
                scopes: new
                    Dictionary<string, string> /* Requested scopes for authorization code request and descriptions for swagger UI only */
                    {
                        {"BasketService", "Basket Service API"}
                    },
                apiTitle: "Basket Service API"
            );

            context.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins(
                            configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.Trim().RemovePostFix("/"))
                                .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            
            Configure<AbpAntiForgeryOptions>(options => { options.AutoValidate = false; });
        }

        private void ConfigureHttpApiProxy(ServiceConfigurationContext context)
        {
            context.Services.AddStaticHttpClientProxies(
                typeof(BasketServiceApplicationContractsModule).Assembly,
                RemoteServiceName
            );
        }

        private void ConfigureGrpc(ServiceConfigurationContext context)
        {
            context.Services.AddGrpcClient<ProductPublic.ProductPublicClient>((services, options) => 
            {
                var remoteServiceOptions = services.GetRequiredService<IOptionsMonitor<AbpRemoteServiceOptions>>().CurrentValue;
                var catalogServiceConfiguration = remoteServiceOptions.RemoteServices.GetConfigurationOrDefault("Catalog");
                var catalogGrpcUrl = catalogServiceConfiguration.GetOrDefault("GrpcUrl");
                
                options.Address = new Uri(catalogGrpcUrl);
            });
        }

        private void ConfigureAutoMapper()
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<BasketServiceApplicationModule>();
            });
        }

        private void ConfigureDistributedCache()
        {
            Configure<AbpDistributedCacheOptions>(options =>
            {
                options.CacheConfigurators.Add(cacheName =>
                {
                    if (cacheName == CacheNameAttribute.GetCacheName(typeof(Basket)))
                    {
                        return new DistributedCacheEntryOptions
                        {
                            SlidingExpiration = TimeSpan.FromDays(7)
                        };
                    }

                    return null;
                });
            });
        }

        private void ConfigureVirtualFileSystem()
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<BasketServiceModule>();
            });
        }

        private void ConfigureLocalization()
        {
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<BasketServiceResource>("en")
                    .AddBaseTypes(typeof(AbpValidationResource))
                    .AddVirtualJson("/Localization/BasketService");

                options.DefaultResourceType = typeof(BasketServiceResource);
            });

            Configure<AbpExceptionLocalizationOptions>(options =>
            {
                options.MapCodeNamespace("BasketService", typeof(BasketServiceResource));
            });
        }
    }
}
