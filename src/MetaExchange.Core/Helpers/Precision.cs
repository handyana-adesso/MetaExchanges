namespace MetaExchange.Core.Helpers;
public static class Precision
{
    public const decimal BtcStep = 0.00000001m;
    // Safe internal EUR precision
    public const int EurInternalDp = 8;

    public static decimal FloorToStep(decimal value, decimal step)
    {
        if (step <= 0)
        {
            return value;
        }
        var stepCount = Math.Floor(value / step);
        return stepCount * step;
    }
}
