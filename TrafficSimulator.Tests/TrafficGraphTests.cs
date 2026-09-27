using System.Drawing;
using TrafficSimulator.Core.Models;
using Xunit;

namespace TrafficSimulator.Tests;

public class TrafficGraphTests
{
    [Fact]
    public void GetShortestPath_ChoosesLowestWeightedRoute()
    {
        var (graph, a, _, c) = BuildTriangle();

        var path = graph.GetShortestPath(a, c);

        Assert.Equal(2, path.Count);
        Assert.Equal(new[] { "A-B", "B-C" }, path.Select(road => road.Name));
    }

    [Fact]
    public void GetShortestPath_SkipsBlockedRoad()
    {
        var (graph, a, _, c) = BuildTriangle();
        graph.Roads.Single(road => road.Name == "B-C").IsBlocked = true;

        var path = graph.GetShortestPath(a, c);

        var directRoad = Assert.Single(path);
        Assert.Equal("A-C", directRoad.Name);
    }

    [Fact]
    public void GetShortestPath_ReturnsEmptyWhenDestinationIsUnreachable()
    {
        var (graph, a, _, c) = BuildTriangle();
        foreach (var road in graph.Roads.Where(road => road.DestinationCity == c || road.SourceCity == c))
        {
            road.IsBlocked = true;
        }

        Assert.Empty(graph.GetShortestPath(a, c));
    }

    [Fact]
    public void AddRoad_RejectsRoadWhenEndpointIsMissing()
    {
        var graph = new TrafficGraph();
        var a = new City("A", new Point(0, 0));
        var missing = new City("Missing", new Point(1, 1));
        graph.AddCity(a);

        var error = Assert.Throws<ArgumentException>(() => graph.AddRoad(new Road(a, missing)));

        Assert.Contains("deben existir", error.Message);
    }

    private static (TrafficGraph Graph, City A, City B, City C) BuildTriangle()
    {
        var graph = new TrafficGraph();
        var a = new City("A", new Point(0, 0));
        var b = new City("B", new Point(1, 0));
        var c = new City("C", new Point(2, 0));
        graph.AddCity(a);
        graph.AddCity(b);
        graph.AddCity(c);

        graph.AddRoad(new Road(a, b, "A-B"));
        graph.AddRoad(new Road(b, c, "B-C"));
        graph.AddRoad(new Road(a, c, "A-C") { TrafficLoad = 1.0 });
        return (graph, a, b, c);
    }
}
