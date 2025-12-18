using Azure.Identity;
using HunterWebServices.EmailService.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(HunterWebServices.EmailService.Startup))]

namespace HunterWebServices.EmailService
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var configBuilder = builder
                .ConfigurationBuilder
                .AddEnvironmentVariables();

            var azureKeyVaultUrl = new Uri(configBuilder.Build()["AzureKeyVaultUrl"]);

            configBuilder.AddAzureKeyVault(azureKeyVaultUrl, new DefaultAzureCredential());

            base.ConfigureAppConfiguration(builder);
        }
    }
}