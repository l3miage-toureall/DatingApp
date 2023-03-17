
using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtension
    {
        public static void AddHttpExtension(this HttpResponse response,PaginationHeader header)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions) );
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
        
    }
}