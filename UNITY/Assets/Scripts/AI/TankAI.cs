using UnityEngine;
using WarOfTanks.Navigation;
using WarOfTanks.Enums;
using System.Collections.Generic;
using System.Collections;

namespace WarOfTanks.AI
{
    //TODO
    /// TankAI currently mixes AI orchestration, detection, and rerouting logic in a single class for simplicity, 
    /// but as the AI system grows we may want to refactor into separate components


    /// <summary>
    /// Main AI controller for tank behavior. This class will manage the decision-making process for the tank, 
    /// including pathfinding, target selection, and movement. It will utilize the PathfinderFactory to obtain
    /// the appropriate pathfinding algorithm based on the current situation 
    /// (e.g., A* for general navigation, Dijkstra for hazard avoidance, Flow Field for group movement). 
    /// The TankAI will also handle interactions with the environment, such as detecting obstacles and hazards, and will update the tank's movement and actions accordingly.
    /// The class will be designed to be modular and extensible, allowing for easy addition of new behaviors and decision-making logic as needed. 
    /// It will also include error handling to ensure that the AI can gracefully handle situations where pathfinding fails or when the tank encounters unexpected obstacles.
    /// </summary>
    [RequireComponent(typeof(TankController))]
    public class TankAI : MonoBehaviour
    {
        private List<PathNode> _currentPath;
        private int _currentPathIndex;
        private Vector2Int _targetGridPosition;
        [SerializeField] private float _detectionRange = 1.5f;
        [SerializeField] private float _tankRadius = 0.4f;
        [SerializeField] private LayerMask _tankLayerMask;
        [SerializeField] private float _blockTimeout = 0.2f;
        [SerializeField] private int _maxRecalculations = 3;
        [SerializeField] private float _recalcWindowDuration = 2f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = false;
        private int _recalcCount;
        private float _recalcTimer;
        private bool _isWaiting;
        private bool _isHandlingBlock;
        private Vector2 _lastBlockerPosition;
        private INavigable _navigator;
        private NavigationGrid _grid;
        private TankController _tankController;
        
        private void Awake()
        {
            _grid = FindObjectOfType<NavigationGrid>();
            _tankController = GetComponent<TankController>();

            if (_grid == null)
            {
                DebugLogger.LogError($"{nameof(TankAI)} requires an active {nameof(NavigationGrid)} in the scene.", this);
                enabled = false;
                return;
            }

            if (_tankController == null)
            {
                DebugLogger.LogError($"{nameof(TankAI)} requires a {nameof(TankController)} on the same GameObject.", this);
                enabled = false;
                return;
            }

            if (_tankLayerMask == 0)
            {
                DebugLogger.LogWarning($"{nameof(TankAI)}: _tankLayerMask is not set. Block detection will not work.", this);
            }

            _navigator = PathfinderFactory.Create(EPathfinderType.ASTAR, _grid);
        }

        private void Update()
        {
            if (_currentPath == null || _currentPath.Count == 0 || _isWaiting)
                return;

            if (_currentPathIndex >= _currentPath.Count)
            {
                ClearPath();
                return;
            }

            PathNode targetNode = _currentPath[_currentPathIndex];
            Vector3 targetWorldPos = _grid.GridToWorldPosition(targetNode.GridPosition);
            Vector2 direction = ((Vector2)(targetWorldPos - transform.position)).normalized;

            _tankController.Move(direction);
            _tankController.RotateToward(direction);

            if (!_isHandlingBlock && CheckForBlockingTank())
            {
                _isHandlingBlock = true;
                _tankController.Stop();
                StartCoroutine(HandleBlock());
            }

            if (Vector2.Distance(transform.position, targetWorldPos) < 0.1f)
            {
                _currentPathIndex++;

                if (_currentPathIndex >= _currentPath.Count)
                {
                   ClearPath();
                }
            }

            if (_recalcCount > 0)
            {
                _recalcTimer += Time.deltaTime;
                if (_recalcTimer > _recalcWindowDuration)
                {
                    _recalcCount = 0;
                    _recalcTimer = 0f;
                }
            }
        }

        public List<PathNode> CurrentPath => _currentPath;
        public int CurrentPathIndex => _currentPathIndex;
        public float DetectionRange => _detectionRange;
        public float TankRadius => _tankRadius;

        /// <summary>
        /// Calculates a new path from the tank's current position to the given grid cell and begins following it.
        /// If a valid path cannot be found, the tank will stop and wait until the next SetDestination call.
        /// The targetGrid parameter should be in grid coordinates (not world coordinates). 
        /// The method will convert the tank's current world position to grid coordinates before requesting the path from the navigator.
        /// <param name="targetGrid">The destination grid cell coordinates.</param>
        /// </summary>
        public void SetDestination(Vector2Int targetGrid)
        {
            if (_grid == null || _navigator == null)
                return;

            _targetGridPosition = targetGrid;
            _currentPathIndex = 0;

            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);
            _currentPath = _navigator.FindPath(currentGrid, _targetGridPosition);
        }

        /// <summary>
        /// Returns true if another tank is detected ahead via CircleCastAll. Stores the blocker's world position in _lastBlockerPosition.
        /// The method checks the next target node in the current path and casts a circle in that direction to detect any tanks within the detection range.
        /// It ignores the tank's own colliders and only considers hits on objects tagged as "Tank". 
        /// If a blocking tank is detected, it stores the blocker's position for use in dynamic path recalculation. 
        /// The method returns true if a blocking tank is detected, and false otherwise.
        /// </summary>
        private bool CheckForBlockingTank()
        {
            if (_currentPath == null || _currentPathIndex >= _currentPath.Count)
                return false;

            Vector3 targetWorldPos = _grid.GridToWorldPosition(_currentPath[_currentPathIndex].GridPosition);
            Vector2 direction = ((Vector2)(targetWorldPos - transform.position)).normalized;

            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _tankRadius, direction, _detectionRange, _tankLayerMask);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.root == transform)
                    continue;

                if (hit.transform.root.CompareTag("Tank"))
                {
                    _lastBlockerPosition = hit.transform.root.position;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Waits _blockTimeout seconds, then triggers recalculation or ForceWait depending on the rolling recalc count.
        /// This coroutine is started when a blocking tank is detected. It waits for a short duration to see if the block clears on its own.
        /// After the wait, it checks if the block is still present by calling CheckForBlockingTank again. 
        /// If the block persists, it increments the recalc count and decides whether to request a new path or to force a wait based on the number of recent recalculations.
        /// </summary>
        private IEnumerator HandleBlock()
        {
            yield return new WaitForSeconds(_blockTimeout);
            if (CheckForBlockingTank())
            {
                _recalcCount++;
                DebugLogger.Log(_showDebugLogs, $"[TankAI] Block detected on {name} — recalc #{_recalcCount}/{_maxRecalculations}");
                if (_recalcCount >= _maxRecalculations)
                    yield return ForceWait();
                else
                    yield return RequestRecalculation();
            }
            _isHandlingBlock = false;
        }

        /// <summary>
        /// Requests a new path that avoids the blocker's 3×3 grid zone. Replaces the current path on success; triggers ForceWait if no alternate route exists.
        /// The method first builds a set of blocked grid positions around the last known blocker position, excluding the tank's current cell and the target cell. 
        /// It then requests a new path from the navigator, passing the blocked positions to ensure they are avoided in the new path calculation. 
        /// If a valid new path is found, it replaces the current path and resets the path index. If no valid path can be found, it triggers the ForceWait
        /// </summary>
        private IEnumerator RequestRecalculation()
        {
            HashSet<Vector2Int> blockedPositions = GetDynamicBlockedPositions();
            Vector2Int currentGridPosition = _grid.WorldToGridPosition(transform.position);
            List<PathNode> newPath = _navigator.FindPath(currentGridPosition, _targetGridPosition, blockedPositions);

            if (newPath == null || newPath.Count == 0)
            {
                DebugLogger.Log(_showDebugLogs, $"[TankAI] No alternate path found on {name} — forcing wait");
                yield return ForceWait();
            }
            else
            {
                DebugLogger.Log(_showDebugLogs, $"[TankAI] New path found on {name} — {newPath.Count} nodes");
                _currentPath = newPath;
                _currentPathIndex = 0;
            }
        }

        /// <summary>
        /// Builds a set of grid positions to avoid during recalculation: the 3×3 zone around the blocker, excluding the tank's own cell and the destination.
        /// The method calculates the grid coordinates of the last known blocker position and creates a set of positions that form a 3×3 area around it. 
        /// It ensures that the tank's current grid cell and the target grid cell are not included in the blocked positions, 
        /// as the tank needs to be able to move out of its current cell and reach the target cell. 
        /// The resulting set of blocked positions is returned for use in the pathfinding recal
        /// </summary>
        private HashSet<Vector2Int> GetDynamicBlockedPositions()
        {
            var blockedPositions = new HashSet<Vector2Int>();
            Vector2Int blockerGridPos = _grid.WorldToGridPosition(_lastBlockerPosition);
            Vector2Int tankGridPos = _grid.WorldToGridPosition(transform.position);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int checkPos = new Vector2Int(blockerGridPos.x + dx, blockerGridPos.y + dy);
                    if (checkPos == tankGridPos || checkPos == _targetGridPosition)
                        continue;

                    if (_grid.IsValidPosition(checkPos.x, checkPos.y))
                        blockedPositions.Add(checkPos);
                }
            }

            return blockedPositions;
        }

        /// <summary>
        /// Stops the tank for 1 second and resets the recalc counter. Triggered when recalculations exceed _maxRecalculations within the rolling window.
        /// This coroutine is started when the number of recalculations exceeds the defined threshold. 
        /// It logs a message indicating that a forced wait has been triggered, stops the tank's movement, and sets the _isWaiting flag to true.
        /// It then waits for a fixed duration (1 second) before allowing the tank to move again. After the wait, it resets the recalc count and timer to allow for fresh recalcul
        /// </summary>
        private IEnumerator ForceWait()
        {
            DebugLogger.Log(_showDebugLogs, $"[TankAI] Force wait triggered on {name} — anti-oscillation lock");
            _tankController.Stop();
            _isWaiting = true;
            yield return new WaitForSeconds(1.0f);
            _isWaiting = false;
            _recalcCount = 0;
            _recalcTimer = 0f;
        }

        /// <summary>Clears the active path and stops the tank.</summary>
        /// This method is called when the tank reaches its destination or when a path needs to be abandoned. 
        /// It sets the current path to null, resets the path index, and commands the tank controller to stop movement.
        private void ClearPath()
        {
            _currentPath = null;
            _currentPathIndex = 0;
            _tankController.Stop();
        }
    }
}
