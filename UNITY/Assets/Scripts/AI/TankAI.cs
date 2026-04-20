using UnityEngine;
using WarOfTanks.Navigation;
using WarOfTanks.Enums;

namespace WarOfTanks.AI
{
    /// <summary>
    /// Main AI controller for tank behavior. This class will manage the decision-making process for the tank, 
    /// including pathfinding, target selection, and movement. It will utilize the PathfinderFactory to obtain
    /// the appropriate pathfinding algorithm based on the current situation 
    /// (e.g., A* for general navigation, Dijkstra for hazard avoidance, Flow Field for group movement). 
    /// The TankAI will also handle interactions with the environment, such as detecting obstacles and hazards, and will update the tank's movement and actions accordingly.
    /// The class will be designed to be modular and extensible, allowing for easy addition of new behaviors and decision-making logic as needed. 
    /// It will also include error handling to ensure that the AI can gracefully handle situations where pathfinding fails or when the tank encounters unexpected obstacles.
    /// </summary>
    public class TankAI : MonoBehaviour
    {

    }
}
