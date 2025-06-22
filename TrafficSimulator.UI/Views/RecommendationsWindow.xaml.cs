using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TrafficSimulator.Core.Models;
using TrafficSimulator.UI.ViewModels;

namespace TrafficSimulator.UI.Views
{
    public partial class RecommendationsWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly List<RecommendationItem> _recommendations = new List<RecommendationItem>();

        // Remove these field declarations since they conflict with XAML-defined controls
        // private ListView recommendationsListView;
        // private Button implementButton;

        public RecommendationsWindow(MainViewModel viewModel)
        {
            // This loads the XAML-defined controls
            InitializeComponent();

            _viewModel = viewModel;

            // Set up event handlers for existing controls
            if (recommendationsListView != null)
            {
                recommendationsListView.SelectionChanged += (s, e) => UpdateButtonState();
            }

            // Load data for the recommendations
            LoadRecommendations();
        }

        // Move UpdateButtonState out of the constructor to make it a proper method
        private void UpdateButtonState()
        {
            // Use null conditional to avoid null reference exceptions
            implementButton?.SetValue(IsEnabledProperty, recommendationsListView?.SelectedItem != null);
        }

        private void LoadRecommendations()
        {
            // Get recommendations from both methods for more comprehensive results
            var recommendedPairs = _viewModel.GetMostRequestedCityPairsWithoutRoad(5);
            var recommendedVehiclePairs = _viewModel.GetRecommendedNewRoads();

            // Clear existing recommendations
            _recommendations.Clear();

            // Process recommendations from path analysis
            foreach (var pair in recommendedPairs)
            {
                _recommendations.Add(new RecommendationItem
                {
                    Source = pair.Item1.Name,
                    Destination = pair.Item2.Name,
                    Benefit = "Alto",
                    Justification = "Ruta frecuentemente solicitada en cálculos de rutas óptimas",
                    SourceCity = pair.Item1,
                    DestinationCity = pair.Item2
                });
            }

            // Process recommendations from vehicle analysis
            foreach (var pair in recommendedVehiclePairs)
            {
                // Avoid duplicates
                if (!_recommendations.Exists(r =>
                    (r.Source == pair.Item1.Name && r.Destination == pair.Item2.Name) ||
                    (r.Source == pair.Item2.Name && r.Destination == pair.Item1.Name)))
                {
                    _recommendations.Add(new RecommendationItem
                    {
                        Source = pair.Item1.Name,
                        Destination = pair.Item2.Name,
                        Benefit = "Medio",
                        Justification = "Basado en patrones de tráfico vehicular",
                        SourceCity = pair.Item1,
                        DestinationCity = pair.Item2
                    });
                }
            }

            // Set the data source for the ListView
            if (recommendationsListView != null)
            {
                recommendationsListView.ItemsSource = _recommendations;
            }

            // Update button state after loading data
            UpdateButtonState();
        }

        private void ImplementButton_Click(object sender, RoutedEventArgs e)
        {
            if (recommendationsListView == null)
                return;

            var selectedItem = recommendationsListView.SelectedItem as RecommendationItem;
            if (selectedItem != null)
            {
                // Add the recommended road to the graph
                _viewModel.AddRoad(selectedItem.SourceCity, selectedItem.DestinationCity);
                MessageBox.Show($"Ruta implementada entre {selectedItem.Source} y {selectedItem.Destination}.",
                               "Ruta agregada", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh recommendations
                LoadRecommendations();
            }
            else
            {
                MessageBox.Show("Por favor seleccione una recomendación para implementar.",
                               "Selección requerida", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class RecommendationItem
    {
        // Fix nullable property warnings by using the required keyword (C# 11+)
        // or by initializing with empty strings
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Benefit { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;

        // Fix nullable reference type warnings for City properties
        public City SourceCity { get; set; } = null!;
        public City DestinationCity { get; set; } = null!;
    }
}
