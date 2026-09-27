# Traffic Simulator

A desktop traffic simulation built in C# with graph-based route planning and a WPF interface. The solution separates domain logic, simulation services, and UI code so that algorithms can evolve independently from visualization.

## Engineering highlights

- Weighted directed traffic graph with cities and roads
- Dijkstra shortest-path routing
- Blocked-road handling and route recalculation
- Vehicle movement and configurable simulation speed
- Traffic-load normalization
- WPF graph editor, road/vehicle controls, and critical-point views

## Solution structure

```text
TrafficSimulator.Core/    Models, routing, and simulation engine
TrafficSimulator.UI/      WPF views, controls, and view models
TrafficSimulator.Tests/   xUnit tests for routing and graph validation
```

## Build and run

Requirements: Windows and the .NET SDK version declared by the project files.

```bash
dotnet restore
dotnet build TrafficSimulator.sln
dotnet run --project TrafficSimulator.UI
```

## Priority test cases

The automated suite currently verifies:

- the lowest weighted route is selected;
- blocked roads are never selected;
- unreachable destinations return an empty route;
- roads with missing graph endpoints are rejected.

GitHub Actions runs the suite on every push and pull request.

## Roadmap

- Extend coverage to route recalculation and simulation-engine behavior
- Add a GIF of the simulator workflow
- Document graph editing and simulation controls
- Add deterministic random seeds for reproducible scenarios

## License

Apache-2.0
