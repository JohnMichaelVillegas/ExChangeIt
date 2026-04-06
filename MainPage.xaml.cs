using System.Text.Json;

namespace ExChangeIt;

public partial class MainPage : ContentPage
{
    private const string apiKey = "bf5b018cc8f2164a3146ed29508d8afa";

    public MainPage()
    {
        InitializeComponent();

        var currencies = new List<string>
        {
            "PHP",
            "USD",
            "EUR",
            "JPY",
            "GBP",
            "AUD",
            "CAD",
            "SGD"
        };

        basePicker.ItemsSource = currencies;
        targetPicker.ItemsSource = currencies;

        basePicker.SelectedItem = "PHP";
        targetPicker.SelectedItem = "USD";
    }

    private async void OnShowRatesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RatesPage));
    }
    private async void OnConvertClicked(object sender, EventArgs e)
    {
        rateLabel.Text = string.Empty;
        resultLabel.Text = string.Empty;
        statusLabel.IsVisible = false;
        statusLabel.Text = string.Empty;

        if (basePicker.SelectedItem == null || targetPicker.SelectedItem == null)
        {
            await DisplayAlert("Input Error", "Please select both base and target currencies.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(amountEntry.Text))
        {
            await DisplayAlert("Input Error", "Please enter an amount.", "OK");
            return;
        }

        if (!double.TryParse(amountEntry.Text, out double amount) || amount <= 0)
        {
            await DisplayAlert("Input Error", "Please enter a valid positive number.", "OK");
            return;
        }

        string baseCurrency = basePicker.SelectedItem.ToString()!;
        string targetCurrency = targetPicker.SelectedItem.ToString()!;

        string url = $"https://api.exchangeratesapi.io/v1/latest?access_key={apiKey}&symbols={baseCurrency},{targetCurrency},EUR";

        try
        {
            convertButton.IsEnabled = false;
            loadingIndicator.IsVisible = true;
            loadingIndicator.IsRunning = true;
            statusLabel.Text = "Fetching rates...";
            statusLabel.IsVisible = true;

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            var response = await client.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<ExchangeRateResponse>(response);

            if (data == null || !data.success || data.rates == null)
            {
                await DisplayAlert("API Error", "Unable to fetch exchange rates. Please try again.", "OK");
                return;
            }

            if (!data.rates.ContainsKey(baseCurrency) && baseCurrency != "EUR")
            {
                await DisplayAlert("Currency Error", $"Base currency '{baseCurrency}' was not found.", "OK");
                return;
            }

            if (!data.rates.ContainsKey(targetCurrency) && targetCurrency != "EUR")
            {
                await DisplayAlert("Currency Error", $"Target currency '{targetCurrency}' was not found.", "OK");
                return;
            }

            double rate;

            if (baseCurrency == "EUR")
            {
                rate = data.rates[targetCurrency];
            }
            else if (targetCurrency == "EUR")
            {
                rate = 1 / data.rates[baseCurrency];
            }
            else
            {
                rate = data.rates[targetCurrency] / data.rates[baseCurrency];
            }

            double result = amount * rate;

            rateLabel.Text = $"1 {baseCurrency} = {rate:F4} {targetCurrency}";
            resultLabel.Text = $"{amount:F2} {baseCurrency} = {result:F2} {targetCurrency}";
            statusLabel.Text = "Conversion completed.";
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert("Timeout Error", "The request took too long. Please check your internet and try again.", "OK");
            statusLabel.Text = "Request timed out.";
            statusLabel.IsVisible = true;
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Network Error", "No internet connection or unable to reach the exchange rate server.", "OK");
            statusLabel.Text = "Network error occurred.";
            statusLabel.IsVisible = true;
        }
        catch (JsonException)
        {
            await DisplayAlert("Data Error", "Received invalid data from the server.", "OK");
            statusLabel.Text = "Invalid server response.";
            statusLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Unexpected Error", $"Something went wrong: {ex.Message}", "OK");
            statusLabel.Text = "Unexpected error occurred.";
            statusLabel.IsVisible = true;
        }
        finally
        {
            convertButton.IsEnabled = true;
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;
        }
    }
}