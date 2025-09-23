using System.Text.Json;
using System.Text.Json.Serialization;

using MetaExchange.Core.Enums;
using MetaExchange.Core.Models;

namespace MetaExchange.Tests;
public class SerializationEnumTests
{
    [Fact]
    public void TradeType_Serializes_As_LowerCase_String()
    {
        ExecutionPlan executionPlan = new(
            TradeType.BUY,
            1.0m,
            1.0m,
            0m,
            25000m,
            25000m,
            new(),
            new());

        JsonSerializerOptions options = new()  { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        var json = JsonSerializer.Serialize(executionPlan, options);

        Assert.Contains("\"TradeType\": \"buy\"", json);
        Assert.DoesNotContain("\"TradeType\": 0", json);
    }
}
