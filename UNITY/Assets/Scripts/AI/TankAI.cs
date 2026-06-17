using UnityEngine;
using WarOfTanks.Navigation;
using WarOfTanks.Enums;
using System.Collections.Generic;
using ActionNode = WarOfTanks.AI.BehaviourTree.ActionNode;
using BehaviourTreeController = WarOfTanks.AI.BehaviourTree.BehaviourTree;
using NodeStatus = WarOfTanks.AI.BehaviourTree.NodeStatus;
using ZoneController = WarOfTanks.Zone.Zone;
using WarOfTanks.AI.BehaviourTree;

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
    [RequireComponent(typeof(VisionSystem))]
    public partial class TankAI : MonoBehaviour
    {
        private List<PathNode> _currentPath;
        private int _currentPathIndex;
        private Vector2Int _targetGridPosition;
        [SerializeField] private float _detectionRange = 1.5f;
        [SerializeField] private float _tankRadius = 0.4f;

        [Header("Pathfinding")]
        [SerializeField] private EPathfinderType _pathfinderType = EPathfinderType.ASTAR;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = false;
        [SerializeField] private float _minimumWaypointProgress = 0.1f;
        private float _lastProgressTime;
        private Vector2 _lastCheckedPosition;
        private float _lastWaypointDistance;
        private bool _patrolTowardEnemy = true;
        private INavigable _navigator;
        private NavigationGrid _grid;
        private TankController _tankController;
        private Tank _tank;
        private HealthSystem _healthSystem;

        [Header("Behaviour Tree")]
        [SerializeField] private ETankRole _role;
        [SerializeField] private float _btTickInterval = 0.1f;

        private float _btTimer;
        private BehaviourTreeController _behaviourTree;
        private TankBlackboard _blackboard;
        private VisionSystem _visionSystem;
        private ZoneController _zone;
        private Vector2Int _currentTargetGridPosition;
        private EStrategicOrder _strategicOrder = EStrategicOrder.NONE;
        /// <summary>
        /// Gets the enemies currently detected by this tank, or an empty list when the blackboard is not ready.
        /// </summary>
        public List<DetectionResult> EnemyResults => _blackboard?.enemyResults ?? new List<DetectionResult>();

        /// <summary>
        /// Gets the tactical role assigned to this tank.
        /// </summary>
        public ETankRole Role => _role;

        /// <summary>
        /// Gets the current health percentage used by commander and behaviour tree decisions.
        /// </summary>
        public float HealthRatio => _healthSystem != null ? _healthSystem.HealthPercentage : 1f;

        /// <summary>
        /// Gets whether this tank should prioritize returning to spawn for healing.
        /// </summary>
        public bool NeedsHealing => HealthRatio < 0.3f;
        
        private void Awake()
        {
            _grid = FindObjectOfType<NavigationGrid>();
            _tankController = GetComponent<TankController>();
            _tank = GetComponent<Tank>();
            _healthSystem = GetComponent<HealthSystem>();
            _visionSystem = GetComponent<VisionSystem>();

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

            if (_tank == null)
            {
                DebugLogger.LogError($"{nameof(TankAI)} requires a {nameof(Tank)} on the same GameObject.", this);
                enabled = false;
                return;
            }

            if (_visionSystem == null)
            {
                DebugLogger.LogError($"{nameof(TankAI)} requires a {nameof(VisionSystem)} on the same GameObject.", this);
                enabled = false;
                return;
            }

            _navigator = PathfinderFactory.Create(_pathfinderType, _grid);

            if (_navigator is FlowFieldPathfinder flowField)
                _grid.RegisterFlowFieldForDebug(flowField);
        }

        private void Start()
        {
            _zone = FindObjectOfType<ZoneController>();

            _blackboard = new TankBlackboard
            {
                self = _tank,
                zone = _zone
            };

            _behaviourTree = BuildBehaviourTree();
        }

        private void Update()
        {
            TickBehaviourTree();

            if (_currentPath == null || _currentPath.Count == 0)
                return;

            if (_currentPathIndex >= _currentPath.Count)
            {
                ClearPath();
                return;
            }

            CheckForStall();

            if (_currentPath == null || _currentPath.Count == 0 || _currentPathIndex >= _currentPath.Count)
                return;

            PathNode targetNode = _currentPath[_currentPathIndex];
            Vector3 targetWorldPos = _grid.GridToWorldPosition(targetNode.GridPosition);
            Vector2 direction = ((Vector2)(targetWorldPos - transform.position)).normalized;

            _tankController.Move(direction);
            _tankController.RotateToward(direction);

            if (Vector2.Distance(transform.position, targetWorldPos) < 0.1f)
            {
                _currentPathIndex++;

                if (_currentPathIndex >= _currentPath.Count)
                {
                   ClearPath();
                }
            }
        }

        /// <summary>Current path being followed by the tank.</summary>
        public List<PathNode> CurrentPath => _currentPath;

        /// <summary>Index of the next node in the current path.</summary>
        public int CurrentPathIndex => _currentPathIndex;

        /// <summary>Distance used for forward tank block detection.</summary>
        public float DetectionRange => _detectionRange;

        /// <summary>Radius used for tank block detection casts.</summary>
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
            if (IsAlreadyMovingTo(targetGrid))
                return;

            if (_grid == null || _navigator == null)
                return;

            _currentTargetGridPosition = targetGrid;
            _targetGridPosition = targetGrid;
            _currentPathIndex = 0;

            _currentPath = FindPathToTarget(_targetGridPosition, true);

            if (_currentPath == null || _currentPath.Count == 0)
                _currentPath = FindPathToTarget(_targetGridPosition, false);

            ResetStallTracking();
        }

        /// <summary>
        /// Computes a path to the target, optionally treating nearby tanks and tank-layer blockers as dynamic obstacles.
        /// </summary>
        private List<PathNode> FindPathToTarget(Vector2Int targetGrid, bool useDynamicBlocks)
        {
            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);
            HashSet<Vector2Int> blockedPositions = useDynamicBlocks && _tank != null
                ? _tank.GetBlockedCells(transform.position)
                : null;

            return _navigator.FindPath(currentGrid, targetGrid, blockedPositions);
        }

        /// <summary>
        /// Recomputes the active path when the tank has not made enough progress.
        /// </summary>
        private void RecalculateCurrentPath()
        {
            if (_grid == null || _navigator == null)
                return;

            List<PathNode> newPath = FindPathToTarget(_targetGridPosition, true);
            if (newPath == null || newPath.Count == 0)
            {
                newPath = FindPathToTarget(_targetGridPosition, false);
            }

            if (newPath == null || newPath.Count == 0)
            {
                DebugLogger.Log(_showDebugLogs, $"[TankAI] No path found on {name} during stall recovery.");
                ClearPath();
                return;
            }

            DebugLogger.Log(_showDebugLogs, $"[TankAI] Stall recovery path found on {name} — {newPath.Count} nodes");
            _currentPath = newPath;
            _currentPathIndex = 0;
            ResetStallTracking();
        }

        /// <summary>
        /// Detects whether the tank has stopped making progress and refreshes the path from its live position.
        /// </summary>
        private void CheckForStall()
        {
            if (Time.time - _lastProgressTime < TankConstants.STALL_CHECK_INTERVAL)
                return;

            Vector2 currentPosition = transform.position;
            float currentWaypointDistance = GetCurrentWaypointDistance();
            bool hasMovedEnough = Vector2.Distance(currentPosition, _lastCheckedPosition) >= TankConstants.STALL_DISTANCE_THRESHOLD;
            bool hasProgressedTowardWaypoint = currentWaypointDistance < _lastWaypointDistance - _minimumWaypointProgress;

            if (!hasMovedEnough || !hasProgressedTowardWaypoint)
            {
                RecalculateCurrentPath();
                return;
            }

            _lastCheckedPosition = currentPosition;
            _lastWaypointDistance = currentWaypointDistance;
            _lastProgressTime = Time.time;
        }

        /// <summary>
        /// Resets progress tracking after a new path is assigned.
        /// </summary>
        private void ResetStallTracking()
        {
            _lastCheckedPosition = transform.position;
            _lastWaypointDistance = GetCurrentWaypointDistance();
            _lastProgressTime = Time.time;
        }

        /// <summary>
        /// Returns the distance to the active waypoint used to detect whether movement is useful progress.
        /// </summary>
        private float GetCurrentWaypointDistance()
        {
            if (_grid == null || _currentPath == null || _currentPathIndex >= _currentPath.Count)
                return float.PositiveInfinity;

            Vector3 targetWorldPos = _grid.GridToWorldPosition(_currentPath[_currentPathIndex].GridPosition);
            return Vector2.Distance(transform.position, targetWorldPos);
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

        /// <summary>
        /// Returns whether the tank has reached the current grid target.
        /// </summary>
        private bool IsAtDestination()
        {
            if (_grid == null)
                return false;

            return _grid.WorldToGridPosition(transform.position) == _currentTargetGridPosition
                && (_currentPath == null || _currentPath.Count == 0);
        }

        /// <summary>
        /// Returns whether the tank is currently inside or close to the capture zone.
        /// </summary>
        private bool IsInZone()
        {
            if (_zone == null)
                return false;

            Collider2D zoneCollider = _zone.GetComponent<Collider2D>();
            if (zoneCollider != null)
            {
                return zoneCollider.OverlapPoint(transform.position);
            }

            return Vector2.Distance(transform.position, _zone.transform.position) < 1f;
        }

        /// <summary>
        /// Returns whether the tank already has an active path to the requested target.
        /// </summary>
        private bool IsAlreadyMovingTo(Vector2Int target)
        {
            return target == _currentTargetGridPosition
                && _currentPath != null
                && _currentPath.Count > 0;
        }

        /// <summary>
        /// Refreshes the blackboard and ticks the behaviour tree on the configured interval.
        /// </summary>
        private void TickBehaviourTree()
        {
            if (_behaviourTree == null || _blackboard == null)
                return;

            _btTimer += Time.deltaTime;
            if (_btTimer < _btTickInterval)
                return;

            _btTimer = 0f;
            List<Tank> allTanks = GameManager.Instance != null ? GameManager.Instance.GetAllTanks() : new List<Tank>();
            _blackboard.Update(_visionSystem, allTanks);
            _behaviourTree.Tick();
        }

        /// <summary>
        /// Creates the behaviour tree for this tank, prioritizing healing and commander orders before role behaviour.
        /// </summary>
        /// <returns>The behaviour tree controller used to tick this tank's decisions.</returns>
        private BehaviourTreeController BuildBehaviourTree()
        {
            Selector root = new Selector(new List<IBehaviourNode>
            {
                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => NeedsHealing),
                    new ActionNode(MoveToSpawn)
                }),

                new Sequence(new List<IBehaviourNode>
                {
                    new ConditionNode(() => _strategicOrder != EStrategicOrder.NONE),
                    new ActionNode(ExecuteStrategicOrder)
                }),

                BuildRoleTreeRoot()
            });

            return new BehaviourTreeController(root);
        }

        /// <summary>
        /// Builds the role-specific subtree used when no higher-priority healing or commander order is active.
        /// </summary>
        /// <returns>The root node for the configured tank role, or a no-op node for unknown roles.</returns>
        private IBehaviourNode BuildRoleTreeRoot()
        {
            switch (_role)
            {
                case ETankRole.ATTACKER:
                    return BuildAttackerTreeRoot();

                case ETankRole.DEFENDER:
                    return BuildDefenderTreeRoot();

                case ETankRole.CAPTOR:
                    return BuildCaptorTreeRoot();

                default:
                    return new ActionNode(() => NodeStatus.Success);
            }
        }

        /// <summary>
        /// Creates a no-op behaviour tree used as a safe fallback.
        /// </summary>
        private BehaviourTreeController BuildIdleTree()
        {
            return new BehaviourTreeController(new ActionNode(() => NodeStatus.Success));
        }

        /// <summary>
        /// Receives a strategic order from the commander, unless healing should keep priority over that order.
        /// </summary>
        /// <param name="order">The strategic order to store for the next behaviour tree tick.</param>
        public void ReceiveOrder(EStrategicOrder order)
        {
            if (NeedsHealing && order != EStrategicOrder.FALLBACK)
                return;

            _strategicOrder = order;
        }
    }
}
