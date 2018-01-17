﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Butterfly.DataContract.Tracing;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;

namespace Butterfly.Elasticsearch
{
    public class ElasticClientFactory : IElasticClientFactory
    {
        private readonly ElasticsearchOptions _elasticsearchOptions;
        private readonly IIndexFactory _indexFactory;
        private readonly Lazy<ElasticClient> _value;

        public ElasticClientFactory(IOptions<ElasticsearchOptions> options, IIndexFactory indexFactory)
        {
            _elasticsearchOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _indexFactory = indexFactory ?? throw new ArgumentNullException(nameof(indexFactory));
            _value = new Lazy<ElasticClient>(CreatElasticClient, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ElasticClient Create()
        {
            return _value.Value;
        }

        private ElasticClient CreatElasticClient()
        {
            if (string.IsNullOrEmpty(_elasticsearchOptions.ElasticsearchUrls))
            {
                throw new InvalidOperationException("Invalid ElasticsearchUrls.");
            }

            var urls = _elasticsearchOptions.ElasticsearchUrls.Split(';').Select(x => new Uri(x)).ToArray();
            var pool = new StaticConnectionPool(urls);
            var settings = new ConnectionSettings(pool);
            var client = new ElasticClient(settings);
            return client;
            //var tracingIndexName = _indexFactory.CreateIndex();
            //var existsResponse = client.IndexExists(Indices.Parse(tracingIndexName));
            //if (!existsResponse.Exists)
            //{
            //    var tracingIndex = new CreateIndexDescriptor(tracingIndexName);
            //    tracingIndex.Mappings(x => x.Map<Span>(m => m.AutoMap()));
            //    var response = client.CreateIndex(tracingIndex);
            //    //todo log response
            //}

            //var cc = client.CatIndices();
        }
    }
}