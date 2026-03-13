using System.Text.Json;
using System.Text.Json.Serialization;
namespace RedScraps.Receive;

public class RedditReplyConverter : JsonConverter<CommentsRec.AllComments.CommentListing?>
{
    public override CommentsRec.AllComments.CommentListing? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return null; 
        }
        else
        {
            return JsonSerializer.Deserialize<CommentsRec.AllComments.CommentListing>(ref reader, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, CommentsRec.AllComments.CommentListing? value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}