using UnityEngine;
using WarOfTanks.Navigation;
using WarOfTanks.Enums;
using System.Collections.Generic;
using System.Collections;

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
                Debug.LogError($"{nameof(TankAI)} requires an active {nameof(NavigationGrid)} in the scene.", this);
                enabled = false;
                return;
            }

            if (_tankController == null)
            {
                Debug.LogError($"{nameof(TankAI)} requires a {nameof(TankController)} on the same GameObject.", this);
                enabled = false;
                return;
            }

            if (_tankLayerMask == 0)
            {
                Debug.LogWarning($"{nameof(TankAI)}: _tankLayerMask is not set. Block detection will not work.", this);
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

        public void SetDestination(Vector2Int targetGrid)
        {
            if (_grid == null || _navigator == null)
                return;

            _targetGridPosition = targetGrid;
            _currentPathIndex = 0;

            Vector2Int currentGrid = _grid.WorldToGridPosition(transform.position);
            _currentPath = _navigator.FindPath(currentGrid, _targetGridPosition);
        }

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

        private IEnumerator HandleBlock()
        {
            yield return new WaitForSeconds(_blockTimeout);
            if (CheckForBlockingTank())
            {
                _recalcCount++;
                Debug.Log($"[TankAI] Block detected on {name} — recalc #{_recalcCount}/{_maxRecalculations}");
                if (_recalcCount >= _maxRecalculations)
                    yield return ForceWait();
                else
                    yield return RequestRecalculation();
            }
            _isHandlingBlock = false;
        }

        private IEnumerator RequestRecalculation()
        {
            HashSet<Vector2Int> blockedPositions = GetDynamicBlockedPositions();
            Vector2Int currentGridPosition = _grid.WorldToGridPosition(transform.position);
            List<PathNode> newPath = _navigator.FindPath(currentGridPosition, _targetGridPosition, blockedPositions);

            if (newPath == null || newPath.Count == 0)
            {
                Debug.Log($"[TankAI] No alternate path found on {name} — forcing wait");
                yield return ForceWait();
            }
            else
            {
                Debug.Log($"[TankAI] New path found on {name} — {newPath.Count} nodes");
                _currentPath = newPath;
                _currentPathIndex = 0;
            }
        }

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

        private IEnumerator ForceWait()
        {
            Debug.Log($"[TankAI] Force wait triggered on {name} — anti-oscillation lock");
            _tankController.Stop();
            _isWaiting = true;
            yield return new WaitForSeconds(1.0f);
            _isWaiting = false;
            _recalcCount = 0;
            _recalcTimer = 0f;
        }

        private void ClearPath()
        {
            _currentPath = null;
            _currentPathIndex = 0;
            _tankController.Stop();
        }
    }
}
