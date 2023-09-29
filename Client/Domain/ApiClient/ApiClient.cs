namespace BlazorBasic.Domain.ApiClient;

using BlazorBasic.Domain.ApiClient.Sites;

using GraphQL.Client.Http;
using System.Net.Http;

using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;

public class ApiClient
{
    private readonly HttpClient httpClient;
    private readonly Uri sitesUri;

    public ApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "Http Client cannot be null");
        this.sitesUri = new Uri("https://ashy-ground-027a9da03.3.azurestaticapps.net/data-api/graphql", UriKind.Absolute);
        // TODO : ^ should come from config
    }

    public async Task<IEnumerable<Site>> GetSites()
    {
        var graphQLHttpClientOptions = new GraphQLHttpClientOptions
        {
            EndPoint = this.sitesUri
        };

        var gqlClient = new GraphQLHttpClient(graphQLHttpClientOptions, new NewtonsoftJsonSerializer(), this.httpClient);

        var siteRequest = new GraphQLRequest
        {
            Query = @"query Sites {
    sites {
        items {
            name
            id
            TenantId
        }
    }
}"
        };

        var graphQlResponse = await gqlClient.SendQueryAsync<SitesGraphQlResponse>(siteRequest);
        if (graphQlResponse == null)
        {
            Console.WriteLine("Null response");
            throw new Exception("Server returned nothing");
        }
        else
        {
            if (graphQlResponse.Errors != null && graphQlResponse.Errors.Any())
            {
                foreach (var graphQlError in graphQlResponse.Errors)
                {
                    Console.Write($"{graphQlError.Message}");
                }

                throw new Exception("Server returned errors");
            }

            foreach (var site in graphQlResponse.Data.Sites.Items)
            {
                Console.WriteLine($"{site.TenantId}, {site.id}, {site.name}");
            }

            return graphQlResponse.Data.Sites.Items;
        }
    }
}