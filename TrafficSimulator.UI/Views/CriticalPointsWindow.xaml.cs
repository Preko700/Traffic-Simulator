using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TrafficSimulator.Core.Models;
using TrafficSimulator.UI.ViewModels;

namespace TrafficSimulator.UI.Views
{
    public partial class CriticalPointsWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public CriticalPointsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            LoadCriticalPoints();
        }

        private void LoadCriticalPoints()
        {
            // Cargar datos de carreteras críticas
            var criticalRoads = _viewModel.GetMostUsedRoadsViaDijkstra(10);
            var roadItems = criticalRoads.Select((r, i) => new RoadCriticalItem
            {
                Source = r.SourceCity.Name,
                Destination = r.DestinationCity.Name,
                UsageLevel = GetUsageLevelText(i),
                TrafficLoad = r.TrafficLoad,
                Recommendation = GetRoadRecommendation(r),
                Road = r
            }).ToList();
            roadsListView.ItemsSource = roadItems;

            // Cargar datos de ciudades críticas
            var citiesToRoadCount = _viewModel.TrafficGraph.Cities
                .Select(city => new
                {
                    City = city,
                    Connections = _viewModel.TrafficGraph.Roads.Count(r =>
                        r.SourceCity.Id == city.Id || r.DestinationCity.Id == city.Id)
                })
                .OrderByDescending(item => item.Connections)
                .Take(5)
                .ToList();

            var cityItems = citiesToRoadCount.Select((item, i) => new CityCriticalItem
            {
                City = item.City.Name,
                Connections = item.Connections,
                CriticalLevel = (double)(5 - i) / 5.0,
                Recommendation = GetCityRecommendation(item.City, item.Connections),
                CityObj = item.City
            }).ToList();
            citiesListView.ItemsSource = cityItems;

            // Actualizar resumen
            UpdateSummary(roadItems, cityItems);
        }

        private void UpdateSummary(List<RoadCriticalItem> roadItems, List<CityCriticalItem> cityItems)
        {
            if (roadItems.Any() && cityItems.Any())
            {
                var mostCriticalRoad = roadItems.First();
                var mostCriticalCity = cityItems.First();

                txtSummary.Text = $"El análisis ha determinado que la ruta más congestionada es {mostCriticalRoad.Source} → " +
                    $"{mostCriticalRoad.Destination} con una carga de tráfico de {mostCriticalRoad.TrafficLoad:P1}. " +
                    $"\n\nLa ciudad con mayor congestión es {mostCriticalCity.City} con {mostCriticalCity.Connections} " +
                    $"conexiones. Se recomienda considerar la construcción de rutas alternativas o mejoras en la infraestructura actual.";
            }
            else
            {
                txtSummary.Text = "No hay suficientes datos para realizar un análisis completo.";
            }
        }

        private string GetUsageLevelText(int index)
        {
            if (index == 0) return "Muy Alto";
            if (index == 1) return "Alto";
            if (index == 2) return "Medio";
            return "Normal";
        }

        private string GetRoadRecommendation(Road road)
        {
            if (road.TrafficLoad > 0.8)
                return "Ampliar carretera a doble carril urgentemente";
            if (road.TrafficLoad > 0.6)
                return "Considerar construcción de ruta alternativa";
            if (road.TrafficLoad > 0.4)
                return "Planificar mejoras en la infraestructura";

            return "Monitorear niveles de tráfico";
        }

        private string GetCityRecommendation(City city, int connections)
        {
            if (connections > 4)
                return "Construir vías de circunvalación y distribuidores viales";
            if (connections > 3)
                return "Optimizar semáforos y evaluar construcción de rotondas";
            if (connections > 2)
                return "Mejorar señalización y control de tráfico";

            return "Monitorear crecimiento y desarrollo urbano";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HighlightButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Esta función estará disponible en una futura versión.",
                "Función no disponible", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Archivos de texto (*.txt)|*.txt",
                Title = "Guardar informe de tráfico",
                DefaultExt = "txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var roadItems = roadsListView.ItemsSource as List<RoadCriticalItem>;
                    var cityItems = citiesListView.ItemsSource as List<CityCriticalItem>;

                    using (var writer = new System.IO.StreamWriter(dialog.FileName))
                    {
                        writer.WriteLine("INFORME DE ANÁLISIS DE TRÁFICO");
                        writer.WriteLine("===============================");
                        writer.WriteLine($"Fecha: {DateTime.Now}");
                        writer.WriteLine();
                        writer.WriteLine(txtSummary.Text);
                        writer.WriteLine();

                        writer.WriteLine("CARRETERAS CRÍTICAS");
                        writer.WriteLine("------------------");
                        if (roadItems != null)
                        {
                            foreach (var road in roadItems)
                            {
                                writer.WriteLine($"{road.Source} → {road.Destination}: {road.UsageLevel} (carga: {road.TrafficLoad:P1})");
                                writer.WriteLine($"  Recomendación: {road.Recommendation}");
                            }
                        }

                        writer.WriteLine();
                        writer.WriteLine("CIUDADES CRÍTICAS");
                        writer.WriteLine("---------------");
                        if (cityItems != null)
                        {
                            foreach (var city in cityItems)
                            {
                                writer.WriteLine($"{city.City}: {city.Connections} conexiones (nivel crítico: {city.CriticalLevel:P1})");
                                writer.WriteLine($"  Recomendación: {city.Recommendation}");
                            }
                        }
                    }

                    MessageBox.Show("Informe guardado correctamente.",
                        "Informe de tráfico", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar el informe: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    // Clases para los elementos de las listas
    public class RoadCriticalItem
    {
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string UsageLevel { get; set; } = string.Empty;
        public double TrafficLoad { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public Road? Road { get; set; }
    }

    public class CityCriticalItem
    {
        public string City { get; set; } = string.Empty;
        public int Connections { get; set; }
        public double CriticalLevel { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public City? CityObj { get; set; }
    }
}
