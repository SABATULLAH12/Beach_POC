using coke_beach_reportGenerator_api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using static coke_beach_reportGenerator_api.Models.constants;

namespace coke_beach_reportGenerator_api.Extensions
{
    public static class ElasticSearchExtension
    {
        private static IServiceCollection _service;
        private static string baseUrl;

        private static string userName;
        private static string password;
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration iConfig)
        {
            _service = services;
            baseUrl = iConfig["Values:baseUrl"];
            userName = iConfig["Values:user"];
            password = iConfig["Values:password"];
            ConstantPath.GetRootPath  = iConfig["Values:rootPath"];
            var index = iConfig["Values:defaultIndex"];
            createConnection(index);

        }
        private static void createConnection(string index)
        {
            ConnectionSettings settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson().BasicAuthentication(userName, password).ServerCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true).DefaultIndex(index);
            settings.EnableApiVersioningHeader();
            settings.DefaultFieldNameInferrer(p => p);
            AddDefaultMappings(settings);
            var client = new ElasticClient(settings);
            _service.AddSingleton<IElasticClient>(client);
            CreateIndex(client, index);



        }
        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings.DefaultMappingFor<Check>(m => m);
        }

        private static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName, index => index.Map<Check>(x => x.AutoMap()));
        }

    }
}
