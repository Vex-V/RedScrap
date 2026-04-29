using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using RedScraps.URLs;
using RedScraps.Receive;
using RedScraps.Sent;
using RedScraps.Map;

namespace RedScraps;

public class Scraper
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _options;
    private readonly bool _debug;


    public Scraper(string? userAgent = null, bool? debug = false)
    {
        _debug = debug ?? false;


        userAgent ??= "RedScrapsBot";

        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("User-Agent", userAgent);


        _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }


    private void LogDebug(string message)
    {
        if (_debug)
        {
            Console.WriteLine(message);
        }
    }

    private static void CheckResponseStatus(HttpResponseMessage response)
    {
        if ((int)response.StatusCode == 429)
        {
            double wait = response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 60.0;
            throw new HttpRequestException($"RateLimited:{wait}");
        }
        response.EnsureSuccessStatusCode();
    }


    public async Task<UserCommentsSent?> ScrapUserComments(
        string user,
        string? sort = null, 
        int? limit = null, 
        string? time = null, 
        string? after = null)
    {   
        try
        {     
            string type = "comments";
            LogDebug($"[1/5] Building URL for u/{user}...");
            string targetUrl = UserURL.CreateUserURL(user, type, sort, limit, time, after);
            LogDebug($"      URL -> {targetUrl}");


            LogDebug("[2/5] Sending GET request to Reddit...");
            HttpResponseMessage response = await _client.GetAsync(targetUrl);
            CheckResponseStatus(response);


            string jsonResponse = await response.Content.ReadAsStringAsync();
            LogDebug($"      Success! Received {jsonResponse.Length} bytes of JSON.");

            LogDebug("[3/5] Deserializing JSON into UserCommentsRec...");
            UserCommentsRec? receivedData = JsonSerializer.Deserialize<UserCommentsRec>(jsonResponse, _options);

            if (receivedData == null)
            {
                LogDebug("      [ERROR] Deserialization resulted in null.");
                return null;
            }

            LogDebug("[4/5] Mapping raw data to HomeSent clean class...");
            UserCommentsSent cleanData = UserMapper.MapToUserComSent(receivedData);
            LogDebug("      Mapping complete!");


            return cleanData;


        }
        catch (HttpRequestException ex) when (ex.Message.StartsWith("RateLimited:"))
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDebug($"\n[{ex.GetType().Name}] {ex.Message}");
            return null;
        }
    }



    public async Task<UserSubmittedSent?> ScrapUserData(
        string user,
        
        string? sort = null, 
        int? limit = null, 
        string? time = null, 
        string? after = null)
    {   
        try
        {   
            string type = "submitted";
            LogDebug($"[1/5] Building URL for u/{user}...");
            string targetUrl = UserURL.CreateUserURL(user, type, sort, limit, time, after);
            LogDebug($"      URL -> {targetUrl}");

            LogDebug("[2/5] Sending GET request to Reddit...");
            HttpResponseMessage response = await _client.GetAsync(targetUrl);
            CheckResponseStatus(response);

            string jsonResponse = await response.Content.ReadAsStringAsync();
            LogDebug($"      Success! Received {jsonResponse.Length} bytes of JSON.");

            LogDebug("[3/5] Deserializing JSON into UserSubmittedRec...");
            UserSubmittedRec? receivedData = JsonSerializer.Deserialize<UserSubmittedRec>(jsonResponse, _options);

            if (receivedData == null)
            {
                LogDebug("   [ERROR] Deserialization resulted in null.");
                return null;
            }

            LogDebug("[4/5] Mapping raw data to UserSubSent class...");
            UserSubmittedSent cleanData = UserMapper.MapToUserSubSent(receivedData);
            LogDebug("      Mapping complete!");

            return cleanData;

        }
        catch (HttpRequestException ex) when (ex.Message.StartsWith("RateLimited:"))
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDebug($"\n[{ex.GetType().Name}] {ex.Message}");
            return null;
        }
    }
    public async Task<HomeSent?> ScrapHome(
        string subreddit, 
        string? sort = null, 
        int? limit = null, 
        string? time = null, 
        string? after = null)
    {
        try
        {
            LogDebug($"[1/5] Building URL for r/{subreddit}...");
            string targetUrl = HomeURL.CreateHomeURL(subreddit, sort, limit, time, after);
            LogDebug($"      URL -> {targetUrl}");

            LogDebug("[2/5] Sending GET request to Reddit...");
            HttpResponseMessage response = await _client.GetAsync(targetUrl);
            CheckResponseStatus(response);

            string jsonResponse = await response.Content.ReadAsStringAsync();
            LogDebug($"      Success! Received {jsonResponse.Length} bytes of JSON.");

            LogDebug("[3/5] Deserializing JSON into HomeRec...");
            HomeRec? receivedData = JsonSerializer.Deserialize<HomeRec>(jsonResponse, _options);

            if (receivedData == null)
            {
                LogDebug("      [ERROR] Deserialization resulted in null.");
                return null;
            }

            LogDebug("[4/5] Mapping raw data to HomeSent clean class...");
            HomeSent cleanData = PostMapper.MapToHomeSent(receivedData);
            LogDebug("      Mapping complete!");

            return cleanData;
        }
        catch (HttpRequestException ex) when (ex.Message.StartsWith("RateLimited:"))
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDebug($"\n[{ex.GetType().Name}] {ex.Message}");
            return null;
        }
    }

    public async Task<CommentSent?> ScrapComments(
        string subreddit, 
        string postId, 
        string? sort = null, 
        int? limit = null)
    {
        try
        {
            LogDebug($"[1/5] Building Comment URL for Post: {postId}...");
            string targetUrl = CommentURL.CreateCommentURL(subreddit, postId, sort, limit);
            LogDebug($"      URL -> {targetUrl}");

            LogDebug("[2/5] Fetching JSON array from Reddit...");
            HttpResponseMessage response = await _client.GetAsync(targetUrl);
            CheckResponseStatus(response);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            LogDebug($"      Received {jsonResponse.Length} bytes.");

            LogDebug("[3/5] Parsing JSON array components...");
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 2)
            {
                LogDebug("      [ERROR] Unexpected JSON format (Expected Array of 2).");
                return null;
            }

            LogDebug("[4/5] Deserializing PostInfo and AllComments...");
            var postInfo = JsonSerializer.Deserialize<CommentsRec.PostInfo>(root[0].GetRawText(), _options);
            var commentsData = JsonSerializer.Deserialize<CommentsRec.AllComments>(root[1].GetRawText(), _options);

            if (postInfo == null || commentsData == null)
            {
                LogDebug("      [ERROR] Could not deserialize comment components.");
                return null;
            }

            LogDebug("[5/5] Flattening comment tree via Mapper...");
            CommentSent cleanComments = CommentMapper.MapToCommentSent(postInfo, commentsData);
            LogDebug($"      Success! Flattened {cleanComments.Comments?.Count ?? 0} comments.");

            return cleanComments;
        }
        catch (HttpRequestException ex) when (ex.Message.StartsWith("RateLimited:"))
        {
            throw;
        }
        catch (Exception ex)
        {
            LogDebug($"\n[{ex.GetType().Name}] {ex.Message}");
            return null;
        }
    }
}

class Program
{
    
    public static async Task Main(string[] args)
    {
        /*
        Scraper scraper = new Scraper(userAgent: "RedScrapsTestBot/1.0", debug: true);

        Console.WriteLine("\n==================================================");
        Console.WriteLine("TEST 1: ScrapHome (Subreddit: csharp)");
        Console.WriteLine("==================================================");
        
        var homeData = await scraper.ScrapHome("csharp", limit: 5);
        string? firstPostId = null;

        if (homeData != null)
        {
            Console.WriteLine($"\n--- SUCCESS: ScrapHome ---");
            Console.WriteLine($"Subreddit: {homeData.Subreddit}");
            Console.WriteLine($"Total Posts Fetched: {homeData.TotalPosts}");
            
            if (homeData.Posts != null && homeData.Posts.Count > 0)
            {
                // Save the first PostID to dynamically test ScrapComments next
                firstPostId = homeData.Posts[0].PostID; 
                
                foreach (var post in homeData.Posts)
                {
                    Console.WriteLine($" - [{post.PostID}] {post.Title} (by u/{post.Author})");
                }
            }
        }
        else
        {
            Console.WriteLine("Failed to fetch Home data.");
        }


        Console.WriteLine("\n==================================================");
        Console.WriteLine("TEST 2: ScrapComments");
        Console.WriteLine("==================================================");
        
        if (!string.IsNullOrEmpty(firstPostId))
        {
            Console.WriteLine($"Fetching comments for dynamic Post ID: {firstPostId} from r/csharp...");
            var commentData = await scraper.ScrapComments("csharp", firstPostId, limit: 5);

            if (commentData != null)
            {
                Console.WriteLine($"\n--- SUCCESS: ScrapComments ---");
                Console.WriteLine($"Post Title: {commentData.Title}");
                Console.WriteLine($"Reported Comments: {commentData.Num_comments}");
                Console.WriteLine($"Comments Flattened: {commentData.Comments?.Count ?? 0}");

                if (commentData.Comments != null)
                {
                    // Print up to 3 comments to keep the console output clean
                    for (int i = 0; i < Math.Min(3, commentData.Comments.Count); i++)
                    {
                        var comment = commentData.Comments[i];
                        
                        // Truncate long comment bodies
                        string snippet = comment.Body?.Length > 60 
                            ? comment.Body.Substring(0, 57) + "..." 
                            : comment.Body ?? "";
                            
                        Console.WriteLine($" - [{comment.CommentID}] u/{comment.Author}: {snippet}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed to fetch Comment data.");
            }
        }
        else
        {
            Console.WriteLine("Skipping comment test because no Post ID was successfully fetched in Test 1.");
        }


        Console.WriteLine("\n==================================================");
        Console.WriteLine("TEST 3: ScrapUserData (User submissions for u/spez)");
        Console.WriteLine("==================================================");
        
        var userSubData = await scraper.ScrapUserData("spez", limit: 3);
        
        if (userSubData != null)
        {
            Console.WriteLine($"\n--- SUCCESS: ScrapUserData ---");
            Console.WriteLine($"Username: {userSubData.Username}");
            Console.WriteLine($"Total Submissions Fetched: {userSubData.TotalCount}");

            if (userSubData.Posts != null)
            {
                foreach (var post in userSubData.Posts)
                {
                    Console.WriteLine($" - [r/{post.Subreddit}] {post.Title} ({post.Upvotes} upvotes)");
                }
            }
        }
        else
        {
            Console.WriteLine("Failed to fetch User Submitted data.");
        }


        Console.WriteLine("\n==================================================");
        Console.WriteLine("TEST 4: ScrapUserComments (User comments for u/spez)");
        Console.WriteLine("==================================================");
        
        var userComData = await scraper.ScrapUserComments("spez", limit: 3);
        
        if (userComData != null)
        {
            Console.WriteLine($"\n--- SUCCESS: ScrapUserComments ---");
            Console.WriteLine($"Username: {userComData.Username}");
            Console.WriteLine($"Total Comments Fetched: {userComData.TotalCount}");

            if (userComData.Comments != null)
            {
                foreach (var comment in userComData.Comments)
                {
                    string snippet = comment.Body?.Length > 60 
                        ? comment.Body.Substring(0, 57) + "..." 
                        : comment.Body ?? "";
                        
                    Console.WriteLine($" - [r/{comment.Subreddit}] on '{comment.PostTitle}': {snippet}");
                }
            }
        }
        else
        {
            Console.WriteLine("Failed to fetch User Comments data.");
        }
        Console.WriteLine("\nAll tests completed.");
        */
        await Task.CompletedTask;
    }
    
}