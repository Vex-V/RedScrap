using System.Text.Json;
using System.Text.Json.Serialization;
namespace RedScraps.Responses;

public class RedditReplyConverter : JsonConverter<Comments.AllComments.CommentListing?>
{
    public override Comments.AllComments.CommentListing? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return null; 
        }
        else
        {
            return JsonSerializer.Deserialize<Comments.AllComments.CommentListing>(ref reader, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, Comments.AllComments.CommentListing? value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}