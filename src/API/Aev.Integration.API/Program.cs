using Aev.Integration.API.Extensions;
using Aev.Integration.API.Modules;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Aev.Integration.DataSync.Infrastructure;
using Aev.Integration.DataSync.Infrastructure.Persistence;
using Aev.Integration.Invoicing.Infrastructure;
using Aev.Integration.Invoicing.Infrastructure.Persistence;
using Aev.Integration.ServiceRequest.Infrastructure;
using Aev.Integration.ServiceRequest.Infrastructure.Persistence;
using Aev.Integration.Waybill.Infrastructure;
using Aev.Integration.Waybill.Infrastructure.Persistence;
using Aev.Integration.BuildingBlocks.Application.CQRS;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=aev_integration;Username=postgres;Password=postgres";

builder.Services.AddDbContext<DataSyncDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<InvoicingDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ServiceRequestDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<WaybillDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();

builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString));
});

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddSingleton<SyncLockService>();

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule<DataSyncModule>();
    containerBuilder.RegisterModule<InvoicingModule>();
    containerBuilder.RegisterModule<ServiceRequestModule>();
    containerBuilder.RegisterModule<WaybillModule>();
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthorizationFilter()]
});

app.MapDataSyncEndpoints();
app.MapInvoicingEndpoints();
app.MapServiceRequestEndpoints();
app.MapWaybillEndpoints();

app.Run();
