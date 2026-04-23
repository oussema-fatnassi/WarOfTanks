using UnityEngine;
using System.Collections.Generic;
using WarOfTanks.StateMachine;
using WarOfTanks.UI;

namespace WarOfTanks.Zone
{
    /// <summary>
    /// The Control Zone — the central game mechanic.
    /// Tracks which teams have tanks inside and manages capture progress.
    /// Owns the ZoneCaptureStateMachine which drives all state transitions.
    /// Zone is the sole owner of captureProgress mutations — states call
    /// IncrementProgress() and DecayGauge() instead of writing directly.
    /// </summary>
    public class Zone : MonoBehaviour
    {
        [Header("Capture Settings")]
        [SerializeField] private float _captureSpeed = 10f;
        [SerializeField] private float _decaySpeed = 5f;
        [SerializeField] private float _capturedTimeout = 3f;

        [Header("Scoring Settings")]
        [SerializeField] private float _scoringRate = 1f;

        [Header("UI")]
        [SerializeField] private ZoneUIController _zoneUIController;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = false;

        // Internal tank tracking — private, states read counts via properties below
        private List<GameObject> _teamPlayerTanksInZone = new List<GameObject>();
        private List<GameObject> _teamAITanksInZone = new List<GameObject>();

        private ZoneCaptureStateMachine _stateMachine;

        // --- UML fields ---

        /// <summary>Team ID of the team currently controlling the zone. -1 means neutral.</summary>
        public int controllingTeam = -1;

        /// <summary>Current capture progress as a percentage (0–100).</summary>
        public float captureProgress;

        /// <summary>Team IDs currently present in the zone. Two entries means contested.</summary>
        public List<int> contestedBy = new List<int>();

        // --- Read-only accessors for states ---
        public int PlayerTankCount => _teamPlayerTanksInZone.Count;
        public int AITankCount     => _teamAITanksInZone.Count;
        public bool ShowDebugLogs => _showDebugLogs;

        // Config values exposed so states can read them without owning them
        public float CaptureSpeed    => _captureSpeed;
        public float ScoringRate     => _scoringRate;
        public float CapturedTimeout => _capturedTimeout;

        /// <summary>Exposed so states can drive visuals via Context.UI</summary>
        public ZoneUIController UI => _zoneUIController;

        // ---------------------------------------------------------------

        private void Awake()
        {
            _stateMachine = new ZoneCaptureStateMachine(this);
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        // --- UML methods ---

        /// <summary>Returns true if tanks from both teams are present simultaneously.</summary>
        public bool IsContested()
        {
            return contestedBy.Count > 1;
        }

        /// <summary>Returns true if a team has fully captured the zone.</summary>
        public bool IsCaptured()
        {
            return controllingTeam != -1;
        }

        // --- Progress mutation — Zone is the sole owner ---
        #region Progress Mutation
        /// <summary>
        /// Called by CapturingState each frame to fill the gauge.
        /// Zone owns the mutation and syncs the UI bar.
        /// </summary>
        public void IncrementProgress(float deltaTime)
        {
            captureProgress = Mathf.Clamp(captureProgress + _captureSpeed * deltaTime, 0f, 100f);
            _zoneUIController.UpdateCaptureBar(captureProgress);
        }

        /// <summary>
        /// Called by CapturingState and CapturedState when the gauge should drain.
        /// Zone owns the mutation and syncs the UI bar.
        /// </summary>
        public void DecayGauge(float deltaTime)
        {
            captureProgress = Mathf.Clamp(captureProgress - _decaySpeed * deltaTime, 0f, 100f);
            _zoneUIController.UpdateCaptureBar(captureProgress);
        }

        /// <summary>
        /// Resets the capture progress to 0%.
        /// </summary>
        public void ResetProgress()
        {
            captureProgress = 0f;
            _zoneUIController.UpdateCaptureBar(0f);
        }
        #endregion

        // --- Physics callbacks ---
        #region Physics Callbacks
        public void OnTriggerEnter2D(Collider2D other)
        {
            DebugLogger.Log(_showDebugLogs, $"OnTriggerEnter2D: {other.gameObject.name}", nameof(Zone));
            var tank = other.GetComponentInParent<Tank>();
            if (tank == null) return;

            if (tank.TeamId == 0)
            {
                _teamPlayerTanksInZone.Add(other.gameObject);
                if (!contestedBy.Contains(0)) contestedBy.Add(0);
            }
            else
            {
                _teamAITanksInZone.Add(other.gameObject);
                if (!contestedBy.Contains(1)) contestedBy.Add(1);
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            DebugLogger.Log(_showDebugLogs, $"OnTriggerExit2D: {other.gameObject.name}", nameof(Zone));
            var tank = other.GetComponentInParent<Tank>();
            if (tank == null) return;

            if (tank.TeamId == 0)
            {
                _teamPlayerTanksInZone.Remove(other.gameObject);
                if (_teamPlayerTanksInZone.Count == 0) contestedBy.Remove(0);
            }
            else
            {
                _teamAITanksInZone.Remove(other.gameObject);
                if (_teamAITanksInZone.Count == 0) contestedBy.Remove(1);
            }
        }
        #endregion
    }
}
