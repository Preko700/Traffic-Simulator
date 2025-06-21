using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TrafficSimulator.Core.Models;
using TrafficSimulator.UI.ViewModels;
using DrawingPoint = System.Drawing.Point;
using WpfPoint = System.Windows.Point;

namespace TrafficSimulator.UI.Controls
{
    public partial class GraphEditorCanvas : UserControl
    {
        private MainViewModel? _viewModel;
        private UIElement? _draggedElement;
        private CityControl? _selectedSourceCity;
        private WpfPoint _dragStartPosition;
        private Dictionary<Guid, CityControl> _cityControlsMap = new Dictionary<Guid, CityControl>();
        private Dictionary<Guid, RoadControl> _roadControlsMap = new Dictionary<Guid, RoadControl>();
        private Dictionary<Guid, VehicleControl> _vehicleControlsMap = new Dictionary<Guid, VehicleControl>();

        // Create a Line for road connection preview
        private Line ConnectionLine = new Line
        {
            Stroke = Brushes.Gray,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 5, 3 },
            Visibility = Visibility.Collapsed
        };

        public MainViewModel? ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != value)
                {
                    if (_viewModel != null)
                    {
                        UnsubscribeFromViewModelEvents();
                    }

                    _viewModel = value;

                    if (_viewModel != null)
                    {
                        SubscribeToViewModelEvents();
                        UpdateCanvasFromViewModel();
                    }
                }
            }
        }

        public GraphEditorCanvas()
        {
            InitializeComponent();

            // Add the connection line to the canvas
            if (EditorCanvas != null)
            {
                EditorCanvas.Children.Add(ConnectionLine);
                Canvas.SetZIndex(ConnectionLine, 50); // Above roads but below cities
            }
        }

        private void SubscribeToViewModelEvents()
        {
            if (_viewModel != null)
            {
                // Fix Problem 1: Use correct delegate signature with object? sender
                if (_viewModel.Vehicles is INotifyCollectionChanged vehicles)
                {
                    vehicles.CollectionChanged += Vehicles_CollectionChanged;
                }

                // Fix Problem 2: Use correct delegate signature with object? sender
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void UnsubscribeFromViewModelEvents()
        {
            if (_viewModel != null)
            {
                // Fix Problem 3: Use correct delegate signature with object? sender
                if (_viewModel.Vehicles is INotifyCollectionChanged vehicles)
                {
                    vehicles.CollectionChanged -= Vehicles_CollectionChanged;
                }

                // Fix Problem 4: Use correct delegate signature with object? sender
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        // Fix Problems 1-4: Update method signatures to match delegate signatures
        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedVehicle))
            {
                UpdateVehicleSelection();
            }
        }

        private void Vehicles_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateVehiclesOnCanvas();
        }

        public void UpdateCanvasFromViewModel()
        {
            if (_viewModel == null || _viewModel.TrafficGraph == null)
                return;

            EditorCanvas.Children.Clear();
            _cityControlsMap.Clear();
            _roadControlsMap.Clear();
            _vehicleControlsMap.Clear();

            // Re-add the connection line (it was removed when clearing the canvas)
            EditorCanvas.Children.Add(ConnectionLine);

            foreach (var road in _viewModel.TrafficGraph.Roads)
            {
                AddRoadToCanvas(road);
            }

            foreach (var city in _viewModel.TrafficGraph.Cities)
            {
                AddCityToCanvas(city);
            }

            UpdateVehiclesOnCanvas();
        }

        private void UpdateVehiclesOnCanvas()
        {
            if (_viewModel?.Vehicles == null) return;

            // Remover vehículos que ya no existen
            var vehiclesToRemove = _vehicleControlsMap.Keys
                .Where(id => !_viewModel.Vehicles.Any(v => v.Id == id))
                .ToList();

            foreach (var vehicleId in vehiclesToRemove)
            {
                if (_vehicleControlsMap.TryGetValue(vehicleId, out var control))
                {
                    EditorCanvas.Children.Remove(control);
                    _vehicleControlsMap.Remove(vehicleId);
                }
            }

            // Añadir nuevos vehículos y actualizar existentes
            foreach (var vehicle in _viewModel.Vehicles)
            {
                if (!_vehicleControlsMap.ContainsKey(vehicle.Id))
                {
                    AddVehicleToCanvas(vehicle);
                }
                else
                {
                    // Actualizar posición de vehículos existentes
                    UpdateVehiclePosition(vehicle);
                }
            }
        }

        private void AddVehicleToCanvas(Vehicle vehicle)
        {
            // Fix Problem 5: Create VehicleControl properly
            var vehicleControl = new VehicleControl();
            vehicleControl.SetVehicle(vehicle); // Add this method to VehicleControl class

            vehicleControl.MouseLeftButtonDown += Vehicle_MouseLeftButtonDown;

            // Posicionar centrado
            Canvas.SetLeft(vehicleControl, vehicle.CurrentPosition.X - vehicleControl.Width / 2);
            Canvas.SetTop(vehicleControl, vehicle.CurrentPosition.Y - vehicleControl.Height / 2);
            Canvas.SetZIndex(vehicleControl, 100); // Vehículos por encima de todo

            EditorCanvas.Children.Add(vehicleControl);
            _vehicleControlsMap[vehicle.Id] = vehicleControl;

            // Suscribirse a cambios de posición
            vehicle.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(Vehicle.CurrentPosition))
                {
                    Dispatcher.InvokeAsync(() => UpdateVehiclePosition(vehicle));
                }
            };
        }

        private void UpdateVehiclePosition(Vehicle vehicle)
        {
            if (_vehicleControlsMap.TryGetValue(vehicle.Id, out var control))
            {
                Canvas.SetLeft(control, vehicle.CurrentPosition.X - control.ActualWidth / 2);
                Canvas.SetTop(control, vehicle.CurrentPosition.Y - control.ActualHeight / 2);
            }
        }

        private void UpdateVehicleSelection()
        {
            // Fix Problem 6: Check if the VehicleControl has SetIsSelected method
            foreach (var kvp in _vehicleControlsMap)
            {
                kvp.Value.SetIsSelected(_viewModel?.SelectedVehicle?.Id == kvp.Key);
            }
        }

        private void Vehicle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Fix Problems 7-9: Use GetVehicle method instead of Vehicle property
            var vehicleControl = sender as VehicleControl;
            if (vehicleControl != null && _viewModel != null)
            {
                var vehicle = vehicleControl.GetVehicle(); // Add this method to VehicleControl
                if (vehicle != null)
                {
                    _viewModel.SelectedVehicle = vehicle;
                    _viewModel.SelectedElement = vehicle;
                    e.Handled = true;
                }
            }
        }

        public void UpdateCity(City city)
        {
            if (_cityControlsMap.TryGetValue(city.Id, out var cityControl))
            {
                Canvas.SetLeft(cityControl, city.Position.X - cityControl.Width / 2);
                Canvas.SetTop(cityControl, city.Position.Y - cityControl.Height / 2);

                cityControl.City = city;
                UpdateConnectedRoads(city);
            }
        }

        public void UpdateRoad(Road road)
        {
            if (_roadControlsMap.TryGetValue(road.Id, out var roadControl))
            {
                roadControl.Road = road;
                UpdateRoadPosition(roadControl);
            }
        }

        public void AddCityToCanvas(City city)
        {
            var cityControl = new CityControl(city);
            cityControl.MouseLeftButtonDown += City_MouseLeftButtonDown;
            cityControl.MouseLeftButtonUp += City_MouseLeftButtonUp;
            cityControl.MouseRightButtonDown += City_MouseRightButtonDown;

            Canvas.SetLeft(cityControl, city.Position.X - cityControl.Width / 2);
            Canvas.SetTop(cityControl, city.Position.Y - cityControl.Height / 2);
            Canvas.SetZIndex(cityControl, 75); // Above roads and connection lines

            EditorCanvas.Children.Add(cityControl);
            _cityControlsMap[city.Id] = cityControl;
        }

        public void AddRoadToCanvas(Road road)
        {
            var roadControl = new RoadControl(road);
            roadControl.MouseLeftButtonDown += Road_MouseLeftButtonDown;

            Canvas.SetZIndex(roadControl, 25); // Below cities and connection lines
            EditorCanvas.Children.Add(roadControl);
            _roadControlsMap[road.Id] = roadControl;

            UpdateRoadPosition(roadControl);
        }

        public void UpdateRoadPosition(RoadControl roadControl)
        {
            var road = roadControl.Road;
            var source = road.SourceCity.Position;
            var target = road.DestinationCity.Position;

            roadControl.X1 = source.X;
            roadControl.Y1 = source.Y;
            roadControl.X2 = target.X;
            roadControl.Y2 = target.Y;
        }

        private void EditorCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel?.CurrentEditorMode == EditorMode.AddCity)
            {
                var position = e.GetPosition(EditorCanvas);
                AddNewCity(position);
            }
            else if (_viewModel?.CurrentEditorMode == EditorMode.Select)
            {
                _selectedSourceCity = null;
                ConnectionLine.Visibility = Visibility.Collapsed;
                _viewModel.SelectedElement = null;
                _viewModel.SelectedVehicle = null;
            }
        }

        private void EditorCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(EditorCanvas);
                var offset = position - _dragStartPosition;

                if (_draggedElement is CityControl cityControl)
                {
                    var left = Canvas.GetLeft(cityControl) + offset.X;
                    var top = Canvas.GetTop(cityControl) + offset.Y;
                    Canvas.SetLeft(cityControl, left);
                    Canvas.SetTop(cityControl, top);

                    cityControl.City.Position = new DrawingPoint(
                        (int)(left + cityControl.Width / 2),
                        (int)(top + cityControl.Height / 2)
                    );

                    UpdateConnectedRoads(cityControl.City);

                    _dragStartPosition = position;
                }
            }

            if (_viewModel?.CurrentEditorMode == EditorMode.AddRoad && _selectedSourceCity != null)
            {
                var position = e.GetPosition(EditorCanvas);
                ConnectionLine.X2 = position.X;
                ConnectionLine.Y2 = position.Y;
            }
        }

        private void EditorCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedElement = null;
        }

        private void EditorCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _selectedSourceCity = null;
            ConnectionLine.Visibility = Visibility.Collapsed;
            _draggedElement = null;
        }

        private void City_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cityControl = sender as CityControl;
            if (cityControl == null) return;

            if (_viewModel?.CurrentEditorMode == EditorMode.Select)
            {
                _draggedElement = cityControl;
                _dragStartPosition = e.GetPosition(EditorCanvas);
                _viewModel.SelectedElement = cityControl.City;
                e.Handled = true;
            }
            else if (_viewModel?.CurrentEditorMode == EditorMode.AddRoad)
            {
                if (_selectedSourceCity == null)
                {
                    _selectedSourceCity = cityControl;
                    var sourcePosition = cityControl.City.Position;
                    ConnectionLine.X1 = sourcePosition.X;
                    ConnectionLine.Y1 = sourcePosition.Y;
                    ConnectionLine.X2 = sourcePosition.X;
                    ConnectionLine.Y2 = sourcePosition.Y;
                    ConnectionLine.Visibility = Visibility.Visible;
                }
                else
                {
                    var sourceCity = _selectedSourceCity.City;
                    var targetCity = cityControl.City;

                    if (sourceCity.Id != targetCity.Id)
                    {
                        AddNewRoad(sourceCity, targetCity);
                    }

                    _selectedSourceCity = null;
                    ConnectionLine.Visibility = Visibility.Collapsed;
                }
                e.Handled = true;
            }
        }

        private void City_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Manejar si es necesario
        }

        private void City_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var cityControl = sender as CityControl;
            if (cityControl == null) return;

            e.Handled = true;
        }

        private void Road_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var roadControl = sender as RoadControl;
            if (roadControl == null) return;

            if (_viewModel?.CurrentEditorMode == EditorMode.Select)
            {
                _viewModel.SelectedElement = roadControl.Road;
                e.Handled = true;
            }
        }

        private void UpdateConnectedRoads(City city)
        {
            foreach (var road in _viewModel?.TrafficGraph?.Roads ?? new List<Road>())
            {
                if (road.SourceCity.Id == city.Id || road.DestinationCity.Id == city.Id)
                {
                    if (_roadControlsMap.TryGetValue(road.Id, out var roadControl))
                    {
                        UpdateRoadPosition(roadControl);
                    }
                }
            }
        }

        private void AddNewCity(WpfPoint position)
        {
            var cityName = $"City_{_viewModel?.TrafficGraph?.Cities.Count + 1}";
            var newCity = new City(cityName, new DrawingPoint((int)position.X, (int)position.Y));
            _viewModel?.TrafficGraph?.AddCity(newCity);
            AddCityToCanvas(newCity);
        }

        private void AddNewRoad(City source, City destination)
        {
            var existingRoad = _viewModel?.TrafficGraph?.Roads.Find(r =>
                (r.SourceCity.Id == source.Id && r.DestinationCity.Id == destination.Id) ||
                (r.SourceCity.Id == destination.Id && r.DestinationCity.Id == source.Id));

            if (existingRoad != null)
                return;

            var roadName = $"Road_{source.Name}_to_{destination.Name}";
            var newRoad = new Road(source, destination, roadName);
            _viewModel?.TrafficGraph?.AddRoad(newRoad);
            AddRoadToCanvas(newRoad);
        }
        private void Road_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var roadControl = sender as RoadControl;
            if (roadControl == null) return;

            // Bloquear/desbloquear
            roadControl.Road.IsBlocked = !roadControl.Road.IsBlocked;
            roadControl.UpdateVisual();

            // Recalcula rutas de vehículos afectados (puedes hacerlo para todos, o solo los afectados)
            _viewModel?.RecalculateRoutes();

            e.Handled = true;
        }
    }
}
