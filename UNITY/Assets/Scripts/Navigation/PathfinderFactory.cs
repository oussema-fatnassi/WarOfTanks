using System;
using WarOfTanks.Enums;

namespace WarOfTanks.Navigation
{
    /// <summary>
    /// Factory class for creating pathfinder instances based on the specified algorithm type.
    /// This class provides a centralized method to instantiate different pathfinding algorithms 
    /// (e.g., A*, Dijkstra, Flow Field) without exposing the details of their construction to the rest of the codebase. 
    /// The factory method takes an enum value representing the desired algorithm and returns an 
    /// instance of the corresponding pathfinder class. 
    /// This design allows for easy swapping of pathfinding algorithms by simply changing the enum value, 
    /// without needing to modify the code that uses the pathfinder instances.
    /// </summary>
    public static class PathfinderFactory
    {
        public static INavigable Create (EPathfinderType type, NavigationGrid grid)
        {
            switch (type)
            {
                case EPathfinderType.ASTAR:
                    return new AStarPathfinder(grid);
                case EPathfinderType.DIJKSTRA:
                    return new DijkstraPathfinder(grid);
                case EPathfinderType.FLOWFIELD:
                    return new FlowFieldPathfinder(grid);
                default:
                    throw new ArgumentException($"Unsupported pathfinder type: {type}");
            }
        }
    }
}
