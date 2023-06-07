using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

class Program
{
    private static readonly string EndpointUrl = "https://cosmosdbunaidemo.documents.azure.com:443/";
    //private static readonly string AuthorizationKey = "YOUR_COSMOSDB_AUTHORIZATION_KEY";
    private static readonly string DatabaseId = "RestaurantReviewsDB";

    //The upload program will create a container named ReviewsById and another one with same data named ReviewsByRestaurantName
    private static readonly string ContainerId1 = "ReviewsById";
    private static readonly string ContainerId2 = "ReviewsByRestaurantName";    

    //Get Key from environment variable (GitHub Secrets)
    private static readonly string AuthorizationKey = Environment.GetEnvironmentVariable("COSMOSDB_KEY");   

    private static readonly int RestaurantCount = 10; 

    static async Task Main(string[] args)
    {
        using (CosmosClient client = new CosmosClient(EndpointUrl, AuthorizationKey))
        {
            Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseId);
            Container container1 = await database.CreateContainerIfNotExistsAsync(ContainerId1, "/id");
            Container container2 = await database.CreateContainerIfNotExistsAsync(ContainerId2, "/restaurantName"); 

            List<Task> tasks = new List<Task>();
            for (int restaurantIndex = 1; restaurantIndex <= RestaurantCount; restaurantIndex++)
            {
                int documentCount = new Random().Next(1000, 3000);

                for (int documentIndex = 1; documentIndex <= documentCount; documentIndex++)
                {
                    tasks.Add(CreateDocument(container1, container2 , restaurantIndex, documentIndex));
                }
            }

            await Task.WhenAll(tasks);
        }

        Console.WriteLine("Documents created successfully.");
        Console.ReadLine();
    }

    private static async Task CreateDocument(Container container1, Container container2, int restaurantIndex, int documentIndex)
    {
        var document = new
        {
            id = Guid.NewGuid().ToString(),
            restaurantName = $"Restaurant {restaurantIndex}",
            review = $"Review for Restaurant {restaurantIndex}",
            rating = new Random().Next(1, 5)
        };

        // For container 1 --> using id as partition key
        ItemResponse<dynamic> response1 = await container1.CreateItemAsync<dynamic>(document);
        Console.WriteLine($"Document {documentIndex} created for container1. Status code: {response1.StatusCode}");

        //for container 2 --> using restaurantName as partition key
        ItemResponse<dynamic> response = await container2.CreateItemAsync<dynamic>(document);
        Console.WriteLine($"Document {documentIndex} created for container2. Status code: {response.StatusCode}");
        

    }
}
