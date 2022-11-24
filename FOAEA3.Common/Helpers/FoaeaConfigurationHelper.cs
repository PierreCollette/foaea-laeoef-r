﻿using FOAEA3.Model;
using FOAEA3.Model.Interfaces;
using FOAEA3.Resources.Helpers;
using Microsoft.Extensions.Configuration;

namespace FOAEA3.Common.Helpers
{
    public class FoaeaConfigurationHelper : IFoaeaConfigurationHelper
    {
        public string FoaeaConnection { get; }

        public RecipientsConfig Recipients { get; }
        public TokenConfig Tokens { get; }
        public DeclarationData LicenceDenialDeclaration { get; }
        public List<string> ProductionServers { get; }
        public List<string> ESDsites { get; }

        public FoaeaConfigurationHelper(string[] args = null)
        {
            string aspnetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("FoaeaConfiguration.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"FoaeaConfiguration.{aspnetCoreEnvironment}.json", optional: true, reloadOnChange: true);

            if (args is not null)
                builder = builder.AddCommandLine(args);

            IConfiguration configuration = builder.Build();

            FoaeaConnection = configuration.GetConnectionString("FOAEAMain").ReplaceVariablesWithEnvironmentValues();

            Recipients = configuration.GetSection("RecipientsConfig").Get<RecipientsConfig>();
            Tokens = configuration.GetSection("Tokens").Get<TokenConfig>();
            LicenceDenialDeclaration = configuration.GetSection("Declaration:LicenceDenial").Get<DeclarationData>();
            ESDsites = configuration.GetSection("ESDsites").Get<List<string>>();
            ProductionServers = configuration.GetSection("ProductionServers").Get<List<string>>();
        }
    }
}
