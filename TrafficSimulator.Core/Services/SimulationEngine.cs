using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using TrafficSimulator.Core.Models;
using static System.Net.Mime.MediaTypeNames;


namespace TrafficSimulator.Core.Services
{
    public class SimulationEngine
    {
        private readonly System.Timers.Timer _timer; // Explicitly specify System.Timers.Timer  

        private readonly TrafficGraph _graph;
        private readonly ObservableCollection<Vehicle> _vehicles;
        private readonly Random _random = new Random();
        private double _simulationSpeed = 1.0;
        private bool _isPaused = false;

        public bool IsRunning { get; private set; }

        public event EventHandler? VehiclesUpdated;
        public event EventHandler<Vehicle>? VehicleReachedDestination;

        public SimulationEngine(TrafficGraph graph, ObservableCollection<Vehicle> vehicles)
        {
            _graph = graph;
            _vehicles = vehicles;

            // Configuración del timer para animación (16ms ≈ 60fps)  
            _timer = new System.Timers.Timer(16); // Explicitly specify System.Timers.Timer  
            _timer.Elapsed += Update;
            _timer.AutoReset = true;
        }
 
        public void Start()
        {
            if (_vehicles.Count == 0) return;

            _isPaused = false;
            IsRunning = true;
            _timer.Start();
        }

        public void Pause()
        {
            _isPaused = true;
            _timer.Stop();
        }

        public void Stop()
        {
            _timer.Stop();
            IsRunning = false;
            _isPaused = false;
        }

        public void SetSimulationSpeed(double speed)
        {
            _simulationSpeed = Math.Clamp(speed, 0.1, 5.0);
        }

        public void GenerateRandomVehicles(int count)
        {
            // Esta funcionalidad se maneja ahora en MainViewModel.GenerateVehicles
        }

        public void RecalculateAllRoutes()
        {
            foreach (var vehicle in _vehicles)
            {
                if (!vehicle.IsAtDestination())
                {
                    var currentCity = GetCurrentCityForVehicle(vehicle);
                    if (currentCity != null)
                    {
                        var newRoute = _graph.GetShortestPath(currentCity, vehicle.Destination);

                        // Solo actualizar si encontramos una ruta válida
                        if (newRoute.Any())
                        {
                            vehicle.CurrentRouteIndex = 0;
                            vehicle.DistanceTraveledOnCurrentRoad = 0;
                            vehicle.Route = newRoute;
                        }
                    }
                }
            }
        }

        private void Update(object? sender, ElapsedEventArgs e)
        {
            if (_isPaused) return;

            var vehiclesToRemove = new List<Vehicle>();

            foreach (var vehicle in _vehicles)
            {
                // Verificar si ya llegó a su destino
                if (vehicle.IsAtDestination())
                {
                    vehiclesToRemove.Add(vehicle);
                    continue;
                }

                // Avanzar el vehículo según su velocidad y la carga de tráfico
                double distanceToMove = vehicle.Speed * _simulationSpeed * 0.01;
                vehicle.AdvanceAlongRoad(distanceToMove);

                // Aumentar carga de tráfico en la carretera actual
                if (vehicle.CurrentRouteIndex < vehicle.Route.Count)
                {
                    var currentRoad = vehicle.Route[vehicle.CurrentRouteIndex];
                    // Incrementar ligeramente la carga de tráfico
                    currentRoad.TrafficLoad += 0.001 * _simulationSpeed;
                }
            }
            VehiclesUpdated?.Invoke(this, EventArgs.Empty);

            // Eliminar vehículos que han llegado a su destino
            foreach (var vehicle in vehiclesToRemove)
            {
                VehicleReachedDestination?.Invoke(this, vehicle);
            }
        }

        public void UpdateTrafficLoad()
        {
            // Normalizar la carga de tráfico para todas las carreteras
            var maxLoad = _graph.Roads.Any() ? _graph.Roads.Max(r => r.TrafficLoad) : 0;
            if (maxLoad > 0)
            {
                foreach (var road in _graph.Roads)
                {
                    road.TrafficLoad /= maxLoad;
                }
            }
        }

        private City? GetCurrentCityForVehicle(Vehicle vehicle)
        {
            if (vehicle.IsAtDestination())
                return vehicle.Destination;

            if (vehicle.CurrentRouteIndex == 0 && vehicle.DistanceTraveledOnCurrentRoad == 0)
                return vehicle.Source;

            if (vehicle.CurrentRouteIndex < vehicle.Route.Count)
            {
                var road = vehicle.Route[vehicle.CurrentRouteIndex];

                if (vehicle.DistanceTraveledOnCurrentRoad < 0.1) // Si está cerca del inicio
                    return road.SourceCity;

                if (vehicle.DistanceTraveledOnCurrentRoad > road.Distance * 0.9) // Si está cerca del final
                    return road.DestinationCity;
            }

            return null; // En medio de una carretera
        }
    }
}
