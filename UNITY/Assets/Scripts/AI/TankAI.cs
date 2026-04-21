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
        [SerializeField] private LayerMask _tankLayerMask;
        [SerializeField] private float _blockTimeout = 0.2f;
        [SerializeField] private int _maxRecalculations = 3;
        [SerializeField] private float _recalcWindowDuration = 2f;
        private int _recalcCount;
        private float _recalcTimer;
        private bool _isWaiting;
        private bool _isHandlingBlock;
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
            _tankController.Rotate(-Vector2.SignedAngle(direction, Vector2.up));

            if (!_isHandlingBlock && CheckForBlockingTank())
            {
                _isHandlingBlock = true;
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

            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _detectionRange, _tankLayerMask);

            return hit.collider != null
                && hit.transform != transform
                && hit.transform.CompareTag("Tank");
        }

        private IEnumerator HandleBlock()
        {
            yield return new WaitForSeconds(_blockTimeout);
            if (CheckForBlockingTank())
            {
                _recalcCount++;
                if (_recalcCount >= _maxRecalculations)
                    yield return ForceWait();
                else
                    yield return RequestRecalculation();
            }
            _isHandlingBlock = false;
        }

        private IEnumerator RequestRecalculation()
        {
            Vector2Int currentGridPosition = _grid.WorldToGridPosition(transform.position);
            List<PathNode> newPath = _navigator.FindPath(currentGridPosition, _targetGridPosition);
            if (newPath == null || newPath.Count == 0)
                yield return ForceWait();
            else
            {
                _currentPath = newPath;
                _currentPathIndex = 0;
            }
        }

        private IEnumerator ForceWait()
        {
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
