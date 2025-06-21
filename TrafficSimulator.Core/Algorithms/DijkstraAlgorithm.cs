using System.Collections.Generic;
using System.Linq;
using TrafficSimulator.Core.Models;

public static class DijkstraAlgorithm
{
    public static List<Road> FindShortestPath(TrafficGraph graph, City source, City destination)
    {
        var previous = new Dictionary<City, Road>();
        var distances = new Dictionary<City, double>();
        var queue = new PriorityQueue<City, double>();

        foreach (var city in graph.Cities)
        {
            distances[city] = double.PositiveInfinity;
        }
        distances[source] = 0;
        queue.Enqueue(source, 0);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Early exit
            if (current == destination) break;

            foreach (var road in graph.GetOutgoingRoads(current))
            {
                // 🟢 CLAVE: Ignorar carreteras bloqueadas
                if (road.IsBlocked)
                    continue;

                var neighbor = road.DestinationCity;
                double tentative = distances[current] + road.Weight;

                if (tentative < distances[neighbor])
                {
                    distances[neighbor] = tentative;
                    previous[neighbor] = road;
                    queue.Enqueue(neighbor, tentative);
                }
            }
        }

        // Reconstruir el camino
        var path = new List<Road>();
        var cur = destination;
        while (cur != source && previous.ContainsKey(cur))
        {
            var road = previous[cur];
            path.Insert(0, road);
            cur = road.SourceCity;
        }

        // Si no se llegó al destino, retorna vacío
        if (path.Count == 0 || path.First().SourceCity != source)
            return new List<Road>();

        return path;
    }
}