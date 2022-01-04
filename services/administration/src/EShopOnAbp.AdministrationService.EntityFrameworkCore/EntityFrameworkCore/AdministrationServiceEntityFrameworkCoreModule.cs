﻿using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace EShopOnAbp.AdministrationService.EntityFrameworkCore
{
    [DependsOn(
        typeof(AdministrationServiceDomainModule),
        typeof(AbpEntityFrameworkCorePostgreSqlModule),
        typeof(AbpPermissionManagementEntityFrameworkCoreModule),
        typeof(AbpSettingManagementEntityFrameworkCoreModule),
        typeof(AbpAuditLoggingEntityFrameworkCoreModule),
        typeof(AbpFeatureManagementEntityFrameworkCoreModule),
        typeof(BlobStoringDatabaseEntityFrameworkCoreModule)
        )]
    public class AdministrationServiceEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<AdministrationServiceDbContext>(options =>
            {
                options.ReplaceDbContext<IPermissionManagementDbContext>();
                options.ReplaceDbContext<ISettingManagementDbContext>();
                options.ReplaceDbContext<IFeatureManagementDbContext>();
                options.ReplaceDbContext<IAuditLoggingDbContext>();
                options.ReplaceDbContext<IBlobStoringDbContext>();

                options.AddDefaultRepositories(includeAllEntities: true);
            });

            Configure<AbpDbContextOptions>(options =>
            {
                options.Configure<AdministrationServiceDbContext>(c =>
                {
                    c.UseNpgsql(b =>
                    {
                        b.MigrationsHistoryTable("__AdministrationService_Migrations");
                    });
                });
            });
        }
    }
}
