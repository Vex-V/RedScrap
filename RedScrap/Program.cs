using System;
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

    public async Task<IUserData?> ScrapUserData(
        string user,
        string type,
        string? sort = null, 
        int? limit = null, 
        string? time = null, 
        string? after = null)
    {   
        try
        {   
            LogDebug($"[1/5] Building URL for u/{user}...");
            string targetUrl = UserURL.CreateUserURL(user, type, sort, limit, time, after);
            LogDebug($"      URL -> {targetUrl}");

            LogDebug("[2/5] Sending GET request to Reddit...");
            HttpResponseMessage response = await _client.GetAsync(targetUrl);
            response.EnsureSuccessStatusCode(); 

            string jsonResponse = await response.Content.ReadAsStringAsync();
            LogDebug($"      Success! Received {jsonResponse.Length} bytes of JSON.");

            if (type == "comments")
            {
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
            else if (type == "submitted")
            {
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
            
            return null;
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
            response.EnsureSuccessStatusCode(); 
            
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
            string jsonResponse = await _client.GetStringAsync(targetUrl);
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
        Console.WriteLine("Initializing Scraper...");
        // Testing the default constructor behavior (debug ON to see your internal logs)
        var scraper = new Scraper(debug: true); 

        // ==========================================
        // TEST 1: ScrapHome
        // ==========================================
        Console.WriteLine("\n\n==========================================");
        Console.WriteLine("TEST 1: ScrapHome (r/programming)");
        Console.WriteLine("==========================================");
        
        var homeData = await scraper.ScrapHome("programming", limit: 3);
        
        if (homeData != null)
        {
            Console.WriteLine($"\n[HOME DATA RESULTS]");
            Console.WriteLine($"Subreddit:  {homeData.Subreddit}");
            Console.WriteLine($"TotalPosts: {homeData.TotalPosts}");
            Console.WriteLine($"FirstID:    {homeData.FirstID}");
            Console.WriteLine($"LastID:     {homeData.LastID}");

            if (homeData.Posts != null && homeData.Posts.Any())
            {
                Console.WriteLine("\n--- First 2 Posts ---");
                foreach (var post in homeData.Posts.Take(2))
                {
                    Console.WriteLine($"Title:  {post.Title}");
                    Console.WriteLine($"Author: {post.Author}");
                    Console.WriteLine($"PostID: {post.PostID}");
                    Console.WriteLine($"Link:   {post.Link}");
                    Console.WriteLine("---------------------");
                }
            }
        }

        // ==========================================
        // TEST 2: ScrapComments
        // ==========================================
        Console.WriteLine("\n\n==========================================");
        Console.WriteLine("TEST 2: ScrapComments");
        Console.WriteLine("==========================================");
        
        // REPLACE "POST_ID_HERE" WITH A REAL POST ID TO TEST PROPERLY
        string testPostId = "1s6b9zt"; 
        var commentData = await scraper.ScrapComments("programming", testPostId, limit: 5);

        if (commentData != null)
        {
            Console.WriteLine($"\n[COMMENT DATA RESULTS]");
            Console.WriteLine($"Post Title:   {commentData.Title}");
            Console.WriteLine($"Post Author:  {commentData.Author}");
            Console.WriteLine($"Total Comms:  {commentData.Num_comments}");
            Console.WriteLine($"Permalink:    {commentData.Permalink}");

            if (commentData.Comments != null && commentData.Comments.Any())
            {
                Console.WriteLine("\n--- First 3 Comments ---");
                foreach (var comment in commentData.Comments.Take(3))
                {
                    Console.WriteLine($"Author: {comment.Author}");
                    Console.WriteLine($"Body:   {(comment.Body?.Length > 50 ? comment.Body.Substring(0, 50) + "..." : comment.Body)}");
                    Console.WriteLine($"ComID:  {comment.CommentID} | ParentID: {comment.ParentID}");
                    Console.WriteLine("---------------------");
                }
            }
        }

        // ==========================================
        // TEST 3: ScrapUserData (Submitted)
        // ==========================================
        Console.WriteLine("\n\n==========================================");
        Console.WriteLine("TEST 3: ScrapUserData (User Posts)");
        Console.WriteLine("==========================================");
        
        // Using a well-known account for guaranteed data
        var userSubData = await scraper.ScrapUserData("spez", "submitted", limit: 3);

        // Pattern matching to safely cast IUserData to UserSubmittedSent
        if (userSubData is UserSubmittedSent subSent)
        {
            Console.WriteLine($"\n[USER SUBMITTED RESULTS]");
            Console.WriteLine($"Username:   {subSent.Username}");
            Console.WriteLine($"TotalCount: {subSent.TotalCount}");

            if (subSent.Posts != null && subSent.Posts.Any())
            {
                Console.WriteLine("\n--- First 2 User Posts ---");
                foreach (var post in subSent.Posts.Take(2))
                {
                    Console.WriteLine($"Title: {post.Title}");
                    Console.WriteLine($"Sub:   r/{post.Subreddit}");
                    Console.WriteLine($"Score: {post.Upvotes}");
                    Console.WriteLine("---------------------");
                }
            }
        }

        // ==========================================
        // TEST 4: ScrapUserData (Comments)
        // ==========================================
        Console.WriteLine("\n\n==========================================");
        Console.WriteLine("TEST 4: ScrapUserData (User Comments)");
        Console.WriteLine("==========================================");

        var userComData = await scraper.ScrapUserData("spez", "comments", limit: 3);

        // Pattern matching to safely cast IUserData to UserCommentsSent
        if (userComData is UserCommentsSent comSent)
        {
            Console.WriteLine($"\n[USER COMMENTS RESULTS]");
            Console.WriteLine($"Username:   {comSent.Username}");
            Console.WriteLine($"TotalCount: {comSent.TotalCount}");

            if (comSent.Comments != null && comSent.Comments.Any())
            {
                Console.WriteLine("\n--- First 2 User Comments ---");
                foreach (var comment in comSent.Comments.Take(2))
                {
                    Console.WriteLine($"Sub:   r/{comment.Subreddit}");
                    Console.WriteLine($"Post:  {comment.PostTitle}");
                    Console.WriteLine($"Body:  {(comment.Body?.Length > 50 ? comment.Body.Substring(0, 50) + "..." : comment.Body)}");
                    Console.WriteLine("---------------------");
                }
            }
        }

        Console.WriteLine("\nAll tests complete!");
        */
    }
    
}