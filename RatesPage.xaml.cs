using System.Text.Json;

namespace ExChangeIt;

public partial class RatesPage : ContentPage
{
    private List<CurrencyBoardRate> allRates = new();

    public RatesPage()
    {
        InitializeComponent();
        _ = LoadRates();
    }

    private async Task LoadRates()
    {
        try
        {
            string url = $"https://api.exchangeratesapi.io/v1/latest?access_key={ApiConfig.ApiKey}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            var response = await client.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<ExchangeRateResponse>(response);

            if (data == null || !data.success || data.rates == null)
            {
                await DisplayAlert("Error", "Unable to load live rates.", "OK");
                return;
            }

            allRates = data.rates
                .OrderBy(r => r.Key)
                .Select(r => new CurrencyBoardRate
                {
                    Currency = r.Key,
                    Buying = r.Value.ToString("F4"),
                    Selling = r.Value.ToString("F4")
                })
                .ToList();

            currencyCollectionView.ItemsSource = allRates;
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert("Timeout Error", "The request took too long. Please try again.", "OK");
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Network Error", "Unable to connect to the live rates server.", "OK");
        }
        catch (JsonException)
        {
            await DisplayAlert("Data Error", "Invalid data was returned by the server.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Error", ex.Message, "OK");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.Trim().ToUpper() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            currencyCollectionView.ItemsSource = allRates;
            return;
        }

        var filteredRates = allRates
            .Where(r => r.Currency.ToUpper().Contains(searchText))
            .ToList();

        currencyCollectionView.ItemsSource = filteredRates;
    }
}
