using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using TrafficSimulator.Core.Models;
using TrafficSimulator.Core.Services;

namespace TrafficSimulator.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields
        private readonly TrafficGraph _trafficGraph;
        private readonly ObservableCollection<Vehicle> _vehicles;
        private readonly SimulationSettings _simulationSettings;
        private SimulationEngine _simulationEngine;
        private readonly string _wazeApiKey = "YOUR_API_KEY_HERE";
        private object? _selectedElement;
        private Vehicle? _selectedVehicle;
        private bool _isSimulationRunning;
        private EditorMode _currentEditorMode;
        private City? _firstSelectedCity;
        private double _simulationSpeed = 1.0;
        #endregion

        #region Properties
        public TrafficGraph TrafficGraph => _trafficGraph;
        public ObservableCollection<Vehicle> Vehicles => _vehicles;
        public SimulationSettings SimulationSettings => _simulationSettings;

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    OnPropertyChanged();
                }
            }
        }

        public object? SelectedElement
        {
            get => _selectedElement;
            set
            {
                if (_selectedElement != value)
                {
                    _selectedElement = value;
                    OnPropertyChanged();

                    if (_selectedElement is Vehicle vehicle)
                    {
                        SelectedVehicle = vehicle;
                    }
                    else
                    {
                        SelectedVehicle = null;
                    }
                }
            }
        }

        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            private set
            {
                if (_isSimulationRunning != value)
                {
                    _isSimulationRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public EditorMode CurrentEditorMode
        {
            get => _currentEditorMode;
            set
            {
                if (_currentEditorMode != value)
                {
                    _currentEditorMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public double SimulationSpeed
        {
            get => _simulationSpeed;
            set
            {
                if (_simulationSpeed != value)
                {
                    _simulationSpeed = value;
                    OnPropertyChanged();
                    _simulationEngine?.SetSimulationSpeed(_simulationSpeed);
                }
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? VehiclesUpdated;
        #endregion

        #region Constructor
        public MainViewModel()
        {
            _trafficGraph = new TrafficGraph();
            _vehicles = new ObservableCollection<Vehicle>();
            _simulationSettings = new SimulationSettings();
            _currentEditorMode = EditorMode.Select;
            _simulationSpeed = 1.0;

            _simulationEngine = new SimulationEngine(_trafficGraph, _vehicles);
            _simulationEngine.VehicleReachedDestination += OnVehicleReachedDestination;
            _simulationEngine.VehiclesUpdated += OnVehiclesUpdated;

            _simulationSettings.PropertyChanged += OnSimulationSettingsChanged;
        }
        #endregion

        #region Graph Management Methods
        public void AddCity(System.Windows.Point position, string? name = null)
        {
            name ??= $"Ciudad {_trafficGraph.Cities.Count + 1}";
            var drawingPoint = new System.Drawing.Point((int)position.X, (int)position.Y);
            var city = new City(name, drawingPoint);
            _trafficGraph.AddCity(city);
        }

        public void AddRoad(City source, City destination, string? name = null)
        {
            name ??= $"{source.Name} - {destination.Name}";
            var road = new Road(source, destination, name);
            _trafficGraph.AddRoad(road);
            if (IsSimulationRunning)
            {
                RecalculateRoutes();
            }
        }

        public bool RemoveCity(City city)
        {
            if (city == null) return false;
            _trafficGraph.RemoveCity(city);
            return true;
        }

        public bool RemoveRoad(Road road)
        {
            if (road == null) return false;
            _trafficGraph.RemoveRoad(road);
            if (IsSimulationRunning)
            {
                RecalculateRoutes();
            }
            return true;
        }

        public void NewGraph()
        {
            StopSimulation();
            _trafficGraph.Cities.Clear();
            _trafficGraph.Roads.Clear();
            _vehicles.Clear();
            SelectedElement = null;
            SelectedVehicle = null;
            _simulationEngine = new SimulationEngine(_trafficGraph, _vehicles);
            _simulationEngine.VehicleReachedDestination += OnVehicleReachedDestination;
            _simulationEngine.VehiclesUpdated += OnVehiclesUpdated;
            OnPropertyChanged(nameof(TrafficGraph));
        }

        public void SaveGraph(string filePath)
        {
            try
            {
                var graphData = new
                {
                    Cities = _trafficGraph.Cities.Select(c => new
                    {
                        c.Id,
                        c.Name,
                        Position = new { c.Position.X, c.Position.Y },
                        c.TrafficFactor
                    }).ToList(),
                    Roads = _trafficGraph.Roads.Select(r => new
                    {
                        r.Id,
                        r.Name,
                        SourceCityId = r.SourceCity.Id,
                        DestinationCityId = r.DestinationCity.Id,
                        r.Distance,
                        r.TrafficLoad,
                        r.IsBlocked,
                        r.BlockReason
                    }).ToList()
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(graphData, jsonOptions);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el grafo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadGraph(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var document = JsonDocument.Parse(jsonString);
                NewGraph();
                var citiesDict = new Dictionary<Guid, City>();
                var citiesElement = document.RootElement.GetProperty("Cities");

                foreach (var cityElement in citiesElement.EnumerateArray())
                {
                    var id = cityElement.GetProperty("Id").GetGuid();
                    var name = cityElement.GetProperty("Name").GetString() ?? "Ciudad";
                    var position = cityElement.GetProperty("Position");
                    int x = position.GetProperty("X").GetInt32();
                    int y = position.GetProperty("Y").GetInt32();
                    double trafficFactor = cityElement.GetProperty("TrafficFactor").GetDouble();

                    var city = new City(name, new System.Drawing.Point(x, y))
                    {
                        Id = id,
                        TrafficFactor = trafficFactor
                    };

                    _trafficGraph.AddCity(city);
                    citiesDict[id] = city;
                }

                var roadsElement = document.RootElement.GetProperty("Roads");
                foreach (var roadElement in roadsElement.EnumerateArray())
                {
                    var sourceId = roadElement.GetProperty("SourceCityId").GetGuid();
                    var destId = roadElement.GetProperty("DestinationCityId").GetGuid();

                    if (citiesDict.TryGetValue(sourceId, out var sourceCity) &&
                        citiesDict.TryGetValue(destId, out var destCity))
                    {
                        var id = roadElement.GetProperty("Id").GetGuid();
                        var name = roadElement.GetProperty("Name").GetString() ?? $"{sourceCity.Name} - {destCity.Name}";
                        var road = new Road(sourceCity, destCity, name)
                        {
                            Id = id,
                            TrafficLoad = roadElement.GetProperty("TrafficLoad").GetDouble(),
                            IsBlocked = roadElement.GetProperty("IsBlocked").GetBoolean()
                        };

                        if (roadElement.TryGetProperty("BlockReason", out var blockReasonElement) &&
                            blockReasonElement.ValueKind != JsonValueKind.Null)
                        {
                            road.BlockReason = blockReasonElement.GetString() ?? string.Empty;
                        }

                        _trafficGraph.AddRoad(road);
                    }
                }

                OnPropertyChanged(nameof(TrafficGraph));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el grafo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Simulation Methods
        public void GenerateVehicles(int count = 10)
        {
            if (TrafficGraph.Cities.Count < 2)
            {
                MessageBox.Show("Se necesitan al menos 2 ciudades para generar vehículos.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsSimulationRunning)
            {
                StopSimulation();
            }

            Vehicles.Clear();
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                var cities = TrafficGraph.Cities.ToList();
                if (cities.Count < 2) break;

                var sourceIndex = random.Next(cities.Count);
                var source = cities[sourceIndex];
                cities.RemoveAt(sourceIndex);

                var destIndex = random.Next(cities.Count);
                var destination = cities[destIndex];

                var vehicle = new Vehicle(source, destination)
                {
                    Speed = 30 + random.NextDouble() * 70
                };

                var route = TrafficGraph.GetShortestPath(source, destination);
                vehicle.Route = route;
                vehicle.CurrentPosition = source.Position;
                Vehicles.Add(vehicle);
            }

            OnPropertyChanged(nameof(Vehicles));
        }

        public void StartSimulation()
        {
            if (!IsSimulationRunning && _trafficGraph.Cities.Count >= 2)
            {
                if (Vehicles.Count == 0)
                {
                    GenerateVehicles(_simulationSettings.VehicleCount);
                }

                _simulationEngine.Start();
                IsSimulationRunning = true;
            }
        }

        public void PauseSimulation()
        {
            if (IsSimulationRunning)
            {
                _simulationEngine.Pause();
            }
        }

        public void StopSimulation()
        {
            if (IsSimulationRunning)
            {
                _simulationEngine.Stop();
                IsSimulationRunning = false;
            }
        }

        public void RecalculateRoutes()
        {
            _simulationEngine.RecalculateAllRoutes();
        }

        public void UpdateTrafficLoad()
        {
            _simulationEngine.UpdateTrafficLoad();
            _trafficGraph.UpdateTrafficLoad(_simulationSettings.GlobalTrafficFactor);
        }

        public List<Road> FindCriticalPoints()
        {
            return _trafficGraph.Roads
                .OrderByDescending(r => r.TrafficLoad)
                .Take(3)
                .ToList();
        }

        public List<Tuple<City, City>> GetRecommendedNewRoads()
        {
            var recommendations = new List<Tuple<City, City>>();
            if (_vehicles.Count == 0)
            {
                return recommendations;
            }

            var allCityPairs = from c1 in _trafficGraph.Cities
                               from c2 in _trafficGraph.Cities
                               where c1.Id != c2.Id &&
                                     !_trafficGraph.Roads.Any(r =>
                                         (r.SourceCity.Id == c1.Id && r.DestinationCity.Id == c2.Id) ||
                                         (r.SourceCity.Id == c2.Id && r.DestinationCity.Id == c1.Id))
                               select new { Source = c1, Destination = c2 };

            var trafficPotential = new Dictionary<(Guid, Guid), int>();

            foreach (var pair in allCityPairs)
            {
                int count = _vehicles.Count(v =>
                    (v.Source.Id == pair.Source.Id && v.Destination.Id == pair.Destination.Id) ||
                    (v.Source.Id == pair.Destination.Id && v.Destination.Id == pair.Source.Id));

                if (count > 0)
                {
                    trafficPotential[(pair.Source.Id, pair.Destination.Id)] = count;
                }
            }

            var topPairs = trafficPotential.OrderByDescending(p => p.Value).Take(3);

            foreach (var pair in topPairs)
            {
                var sourceCity = _trafficGraph.Cities.First(c => c.Id == pair.Key.Item1);
                var destCity = _trafficGraph.Cities.First(c => c.Id == pair.Key.Item2);
                recommendations.Add(new Tuple<City, City>(sourceCity, destCity));
            }

            return recommendations;
        }

        #region Helper Methods
        public void SetFirstSelectedCity(City? city)
        {
            _firstSelectedCity = city;
        }

        public City? GetFirstSelectedCity()
        {
            return _firstSelectedCity;
        }

        public void ClearFirstSelectedCity()
        {
            _firstSelectedCity = null;
        }

        private void OnSimulationSettingsChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SimulationSettings.SimulationSpeed))
            {
                _simulationEngine.SetSimulationSpeed(_simulationSettings.SimulationSpeed);
            }
            else if (e.PropertyName == nameof(SimulationSettings.GlobalTrafficFactor))
            {
                UpdateTrafficLoad();
            }
        }

        private void OnVehicleReachedDestination(object? sender, Vehicle vehicle)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_vehicles.Contains(vehicle))
                {
                    _vehicles.Remove(vehicle);
                    if (SelectedVehicle == vehicle)
                    {
                        SelectedVehicle = null;
                        SelectedElement = null;
                    }
                }
            });
        }

        private void OnVehiclesUpdated(object? sender, EventArgs e)
        {
            VehiclesUpdated?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(Vehicles));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public enum EditorMode
    {
        Select,
        AddCity,
        AddRoad
    }
    #endregion
}
