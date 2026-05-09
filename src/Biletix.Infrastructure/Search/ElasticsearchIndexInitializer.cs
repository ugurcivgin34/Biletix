using Biletix.Application.Common.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;

namespace Biletix.Infrastructure.Search;

/// <summary>
/// Etkinlik arama indeksinin Elasticsearch tarafinda var olmasini saglar.
/// </summary>
public sealed class ElasticsearchIndexInitializer
{
    private const string IndexName = "biletix-events";
    private readonly ElasticsearchClient _client;

    /// <summary>
    /// Elasticsearch client bagimliligini alir.
    /// </summary>
    /// <param name="client">Elasticsearch istemcisi.</param>
    public ElasticsearchIndexInitializer(ElasticsearchClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Indeks yoksa arama ve filtreleme alanlari icin mapping tanimlariyla olusturur.
    /// </summary>
    public async Task InitializeAsync()
    {
        var exists = await _client.Indices.ExistsAsync(IndexName);
        if (exists.Exists)
        {
            return;
        }

        var response = await _client.Indices.CreateAsync(
            IndexName,
            descriptor => descriptor.Mappings(mapping => mapping
                .Properties<EventSearchDocument>(properties => properties
                    .Text(document => document.Title, config => config.Analyzer("standard"))
                    .Text(document => document.Description, config => config.Analyzer("standard"))
                    .Text(document => document.PerformerName, config => config.Analyzer("standard"))
                    .Text(document => document.VenueName, config => config.Analyzer("standard"))
                    .Keyword(document => document.VenueCity)
                    .Keyword(document => document.PerformerGenre)
                    .Keyword(document => document.Status)
                    .Date(document => document.StartDate)
                    .Date(document => document.CreatedAt)
                    .DoubleNumber(document => document.MinPrice)
                    .IntegerNumber(document => document.TotalAvailableTickets))));

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException(
                $"Elasticsearch index creation failed: {response.DebugInformation}");
        }
    }
}
