using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TrafficSimulator.Core.Models
{
    public class TrafficGraph : INotifyPropertyChanged
    {
        private List<City> _cities;
        private List<Road> _roads;

        public List<City> Cities
        {
            get => _cities;
            set
            {
                if (_cities != value)
                {
                    _cities = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<Road> Roads
        {
            get => _roads;
            set
            {
                if (_roads != value)
                {
                    _roads = value;
                    OnPropertyChanged();
                }
            }
        }

        public TrafficGraph()
        {
            _cities = new List<City>();
            _roads = new List<Road>();
        }

        public void AddCity(City city)
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            if (!Cities.Any(c => c.Id == city.Id))
            {
                Cities.Add(city);
                OnPropertyChanged(nameof(Cities));
            }
        }

        public void RemoveCity(City city)
        {
            if (city == null)
                throw new ArgumentNullException(nameof(city));

            // Primero eliminamos todas las carreteras conectadas a esta ciudad
            var roadsToRemove = Roads.Where(r =>
                r.SourceCity.Id == city.Id || r.DestinationCity.Id == city.Id).ToList();

            foreach (var road in roadsToRemove)
            {
                Roads.Remove(road);
            }

            if (roadsToRemove.Any())
            {
                OnPropertyChanged(nameof(Roads));
            }

            // Luego eliminamos la ciudad
            if (Cities.Remove(city))
            {
                OnPropertyChanged(nameof(Cities));
            }
        }

        public void AddRoad(Road road)
        {
            if (road == null)
                throw new ArgumentNullException(nameof(road));

            // Verificar que ambas ciudades existen en el grafo
            if (!Cities.Any(c => c.Id == road.SourceCity.Id) ||
                !Cities.Any(c => c.Id == road.DestinationCity.Id))
            {
                throw new ArgumentException("Ambas ciudades deben existir en el grafo.");
            }

            // Verificar que no existe ya una carretera igual
            if (!Roads.Any(r =>
                (r.SourceCity.Id == road.SourceCity.Id && r.DestinationCity.Id == road.DestinationCity.Id) ||
                (r.SourceCity.Id == road.DestinationCity.Id && r.DestinationCity.Id == road.SourceCity.Id)))
            {
                Roads.Add(road);
                OnPropertyChanged(nameof(Roads));
            }
        }

        public void RemoveRoad(Road road)
        {
            if (road == null)
                throw new ArgumentNullException(nameof(road));

            if (Roads.Remove(road))
            {
                OnPropertyChanged(nameof(Roads));
            }
        }

        // Implementación del algoritmo Dijkstra
        public List<Road> GetShortestPath(City source, City destination)
        {
            if (source == null || destination == null)
                return new List<Road>();

            // If source and destination are the same, return empty path
            if (source.Id == destination.Id)
                return new List<Road>();

            // Dictionary to store the shortest distance to each city
            var distances = new Dictionary<Guid, double>();

            // Dictionary to store the previous road for each city in the path
            var previousRoads = new Dictionary<Guid, Road>();

            // Set of unvisited cities
            var unvisitedCities = new HashSet<City>(Cities);

            // Initialize distances
            foreach (var city in Cities)
            {
                distances[city.Id] = double.MaxValue;
            }
            distances[source.Id] = 0;

            while (unvisitedCities.Count > 0)
            {
                // Find the unvisited city with the smallest distance
                City current = unvisitedCities
                    .OrderBy(c => distances[c.Id])
                    .FirstOrDefault();

                // If we reached the destination or no path exists
                if (current == null || distances[current.Id] == double.MaxValue)
                    break;

                // If we reached the destination
                if (current.Id == destination.Id)
                    break;

                // Remove current city from unvisited set
                unvisitedCities.Remove(current);

                // For each connected road
                foreach (var road in Roads.Where(r => r.SourceCity.Id == current.Id || r.DestinationCity.Id == current.Id))
                {
                    // Skip blocked roads
                    if (road.IsBlocked)
                        continue;

                    // Determine the neighboring city
                    City neighbor = road.SourceCity.Id == current.Id ? road.DestinationCity : road.SourceCity;

                    // Calculate distance including traffic load factor
                    double newDistance = distances[current.Id] + road.Distance * (1 + road.TrafficLoad);

                    // If we found a shorter path
                    if (newDistance < distances[neighbor.Id])
                    {
                        distances[neighbor.Id] = newDistance;
                        previousRoads[neighbor.Id] = road;
                    }
                }
            }

            // If no path was found
            if (!previousRoads.ContainsKey(destination.Id))
                return new List<Road>();

            // Reconstruct the path
            var path = new List<Road>();
            var currentCityId = destination.Id;

            while (currentCityId != source.Id)
            {
                var road = previousRoads[currentCityId];
                path.Add(road);

                // Move to the previous city in the path
                currentCityId = road.SourceCity.Id == currentCityId ? road.DestinationCity.Id : road.SourceCity.Id;
            }

            // The path is from destination to source, so reverse it
            path.Reverse();

            return path;
        }



        // Actualizar la carga de tráfico en todas las carreteras
        public void UpdateTrafficLoad(double globalFactor = 1.0)
        {
            foreach (var road in Roads)
            {
                road.TrafficLoad *= globalFactor;
            }
        }

        // Encontrar puntos críticos en el grafo
        public List<CriticalPoint> FindCriticalPoints()
        {
            var criticalPoints = new List<CriticalPoint>();

            // Temporary implementation until FindCriticalNodes is available
            var criticalNodes = new List<City>();
            foreach (var node in criticalNodes)
            {
                var connectedRoads = Roads.Count(r =>
                    r.SourceCity.Id == node.Id || r.DestinationCity.Id == node.Id);

                criticalPoints.Add(new CriticalPoint(
                    node,
                    connectedRoads * 10, // Severity basada en número de conexiones
                    $"Nodo con {connectedRoads} conexiones",
                    "Considerar construcción de bypass o mejora de infraestructura"
                ));
            }

            // Temporary implementation until FindCriticalRoads is available
            var criticalRoads = new List<Road>();
            foreach (var road in criticalRoads)
            {
                criticalPoints.Add(new CriticalPoint(
                    road.SourceCity,
                    road.TrafficLoad * 20,
                    $"Alta carga de tráfico: {road.TrafficLoad:F2}",
                    $"Considerar ampliación de la carretera {road.Name}"
                ));
            }

            return criticalPoints.OrderByDescending(cp => cp.Severity).ToList();
        }

        public void ResetTrafficLoad()
        {
            foreach (var road in Roads)
            {
                road.TrafficLoad = 0;
            }
        }

        public List<Road> GetOutgoingRoads(City city)
        {
            // Returns all roads where the source city matches the given city  
            return Roads.Where(road => road.SourceCity == city).ToList();
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
