using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1ForWebApplication1
{
    // Модель для реклам
    public class Ad
    {
        public string AdName { get; set; }
        public decimal AdCost { get; set; }
    }

    // Модель для популярных реклам
    public class PopularAd
    {
        public string AdCode { get; set; }
        public int TotalDuration { get; set; }
    }

    // Сервис для работы с API
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7133/") };
        }

        public async Task<decimal> GetAverageAdCost()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<decimal>("Ad/average-cost");
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Не удалось получить среднюю стоимость рекламы.", ex);
            }
        }

        public async Task<List<Ad>> GetAdsAboveCost(decimal cost)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Ad>>($"Ad/ads-above-cost/{cost}");
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Не удалось получить рекламу по цене, превышающей стоимость for {cost}.", ex);
            }
        }

        public async Task<List<PopularAd>> GetMostPopularAds()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<PopularAd>>("Ad/most-popular-ads");
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Не удалось найти самые популярные объявления.", ex);
            }
        }
    }

    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;

        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            UpdatePlaceholderVisibility();
            txtCostThreshold.GotFocus += (s, e) => UpdatePlaceholderVisibility();
            txtCostThreshold.LostFocus += (s, e) => UpdatePlaceholderVisibility();
            txtCostThreshold.TextChanged += (s, e) => UpdatePlaceholderVisibility();
        }

        private void UpdatePlaceholderVisibility()
        {
            if (string.IsNullOrWhiteSpace(txtCostThreshold.Text))
            {
                txtPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                txtPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnGetAverageCost_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var averageCost = await _apiService.GetAverageAdCost();
                txtAverageCost.Text = $"Средняя стоимость рекламы: {averageCost:C}";
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка при получении средней стоимости: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }

        private async void BtnGetAdsAboveCost_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtCostThreshold.Text, out decimal threshold))
            {
                try
                {
                    var ads = await _apiService.GetAdsAboveCost(threshold);
                    lstAdsAboveCost.ItemsSource = ads;
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Ошибка при получении объявлений по цене, превышающей стоимость: {ex.Message}\n{ex.InnerException?.Message}");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите действительную стоимость.");
            }
        }

        private async void BtnGetMostPopularAds_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var popularAds = await _apiService.GetMostPopularAds();
                lstMostPopularAds.ItemsSource = popularAds;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка при просмотре популярных объявлений: {ex.Message}\n{ex.InnerException?.Message}");
            }
        }
    }
}
