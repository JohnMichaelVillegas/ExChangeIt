namespace ExChangeIt;

public class ExchangeRateResponse
{
    public bool success { get; set; }
    public string? @base { get; set; }
    public Dictionary<string, double>? rates { get; set; }
}

public class CurrencyBoardRate
{
    public string Currency { get; set; } = "";
    public string Buying { get; set; } = "";
    public string Selling { get; set; } = "";
}

