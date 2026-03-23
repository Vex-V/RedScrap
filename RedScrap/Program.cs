using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RedScraps.URLs;
using RedScraps.Receive;
using RedScraps.Sent;
using RedScraps.Map;

namespace RedScraps;

public static class Scrappers
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };


    public static async Task Main()
    {
        string URL1 = UserURL.CreateUserURL("testuser","comments");
        string URL2 = UserURL.CreateUserURL("testuser","comments","hot",100);
        //string URL3 = UserURL.CreateUserURL("testuser","comments","hot",100,"hour");
        string URL4 = UserURL.CreateUserURL("testuser","comments","top",100,"hour");
        Console.WriteLine(URL1);
        Console.WriteLine(URL2);
        //Console.WriteLine(URL3);
        Console.WriteLine(URL4);

    }

    public static async Task<HomeSent?> ScrapHome(
        string subreddit, 
        string? sort = null, 
        int? limit = null, 
        string? time = null, 
        string? after = null)
    {
        try
        {
            Console.WriteLine($"[1/5] Building URL for r/{subreddit}...");
            string targetUrl = HomeURL.CreateHomeURL(subreddit, sort, limit, time, after);
            Console.WriteLine($"      URL -> {targetUrl}");

            
            Console.WriteLine("[2/5] Preparing HTTP request headers...");
            if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            {
                
                client.DefaultRequestHeaders.Add("User-Agent", "RedScrapsBot/1.0 (Learning Project)");
            }

            
            Console.WriteLine("[3/5] Sending GET request to Reddit...");
            HttpResponseMessage response = await client.GetAsync(targetUrl);
            
            
            response.EnsureSuccessStatusCode(); 
            
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"      Success! Received {jsonResponse.Length} bytes of JSON.");

            
            Console.WriteLine("[4/5] Deserializing JSON into HomeRec...");
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            HomeRec? receivedData = JsonSerializer.Deserialize<HomeRec>(jsonResponse, jsonOptions);

            if (receivedData == null)
            {
                Console.WriteLine("      [ERROR] Deserialization resulted in null.");
                return null;
            }

            
            Console.WriteLine("[5/5] Mapping raw data to HomeSent clean class...");
            HomeSent cleanData = PostMapper.MapToHomeSent(receivedData);
            Console.WriteLine("      Mapping complete!");

            return cleanData;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"\n[HTTP ERROR] {e.Message}");
            return null;
        }
        catch (JsonException e)
        {
            Console.WriteLine($"\n[JSON ERROR] {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"\n[GENERAL ERROR] {e.Message}");
            return null;
        }
    }


    public static async Task<CommentSent?> ScrapComments(
        string subreddit, 
        string postId, 
        string? sort = null, 
        int? limit = null)
    {
        try
        {
            
            Console.WriteLine($"[1/5] Building Comment URL for Post: {postId}...");
            string targetUrl = CommentURL.CreateCommentURL(subreddit, postId, sort, limit);
            Console.WriteLine($"      URL -> {targetUrl}");

            
            if (!client.DefaultRequestHeaders.Contains("User-Agent"))
                client.DefaultRequestHeaders.Add("User-Agent", "RedScrapsBot/1.0");

            
            Console.WriteLine("[2/5] Fetching JSON array from Reddit...");
            string jsonResponse = await client.GetStringAsync(targetUrl);
            Console.WriteLine($"      Received {jsonResponse.Length} bytes.");

 
            Console.WriteLine("[3/5] Parsing JSON array components...");
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 2)
            {
                Console.WriteLine("      [ERROR] Unexpected JSON format (Expected Array of 2).");
                return null;
            }

            Console.WriteLine("[4/5] Deserializing PostInfo and AllComments...");
            var postInfo = JsonSerializer.Deserialize<CommentsRec.PostInfo>(root[0].GetRawText(), _options);
            var commentsData = JsonSerializer.Deserialize<CommentsRec.AllComments>(root[1].GetRawText(), _options);

            if (postInfo == null || commentsData == null)
            {
                Console.WriteLine("      [ERROR] Could not deserialize comment components.");
                return null;
            }


            Console.WriteLine("[5/5] Flattening comment tree via Mapper...");
            CommentSent cleanComments = CommentMapper.MapToCommentSent(postInfo, commentsData);
            Console.WriteLine($"      Success! Flattened {cleanComments.Comments?.Count ?? 0} comments.");

            return cleanComments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[SCRAPE ERROR] {ex.Message}");
            return null;
        }
    }
}