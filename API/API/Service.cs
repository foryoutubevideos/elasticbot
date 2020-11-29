using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API
{
    public class Service
    {
        readonly string _url = string.Empty;
        readonly string _index = string.Empty;
        const string noResult = "Sorry, no result";

        readonly ConnectionSettings _connectionSettings = null;
        readonly ElasticClient _client = null;

        public Service(IConfiguration configuration)
        {
            _url = configuration["Elastic:Url"];
            _index = configuration["Elastic:Index"];

            _connectionSettings = new ConnectionSettings(new Uri(_url))
                .DefaultIndex(_index);
            _client = new ElasticClient(_connectionSettings);
        }

        public async Task<IEnumerable<Document>> Search(string query)
        {
            List<Document> documents = null;

            if (!string.IsNullOrEmpty(query))
            {
                var result = await _client.SearchAsync<QNA>(s => s
                    .Query(q => q
                        .Match(m => m
                            .Field("question")
                            .Query(query)
                        )
                    )
                );

                var response = result?.Hits
                    .Select(i => new Document
                    {
                        Score = i?.Score,
                        Answer = string.IsNullOrEmpty(i?.Source?.Answer) ? noResult : i?.Source?.Answer,
                        Question = i?.Source?.Question
                    })
                    .ToList();

                double? mean = response?.Sum(j => (double)j.Score) / response.Count;

                response = response?.Where(k => k.Score >= mean).ToList();

                if (!response.Any())
                {
                    response = new List<Document>
                    {
                        new Document
                        {
                            Score = 1,
                            Question = noResult,
                            Answer = noResult
                        }
                    };
                }

                documents = response;
            }

            return documents;
        }
    }
}
