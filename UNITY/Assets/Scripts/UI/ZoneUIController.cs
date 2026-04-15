using UnityEngine;
using UnityEngine.UI;

namespace WarOfTanks.UI
{
    /// <summary>
    /// Owns all visual feedback for the Control Zone.
    /// States call these methods through Context.UI — Zone never calls UI directly
    /// except when mutating captureProgress (IncrementProgress / DecayGauge).
    /// </summary>
    public class ZoneUIController : MonoBehaviour
    {
        [SerializeField] private Image _captureBar;
        [SerializeField] private SpriteRenderer _zoneSprite;

        private static readonly Color NeutralColor   = Color.grey;
        private static readonly Color PlayerColor    = Color.blue;
        private static readonly Color AIColor        = Color.red;
        private static readonly Color ContestedColor = Color.yellow;

        /// <summary>Updates the radial fill bar to reflect current capture progress (0–100).</summary>
        public void UpdateCaptureBar(float captureProgress)
        {
            _captureBar.fillAmount = captureProgress / 100f;
        }

        /// <summary>Called by NeutralState.Enter() — resets bar and color.</summary>
        public void SetNeutral()
        {
            SetColor(NeutralColor);
            UpdateCaptureBar(0f);
        }

        /// <summary>Called by CapturingState.Enter() — colors zone to the capturing team.</summary>
        public void SetCapturing(bool isPlayer)
        {
            SetColor(isPlayer ? PlayerColor : AIColor);
        }

        /// <summary>Called by CapturedState.Enter() — zone is fully owned by this team.</summary>
        public void SetCaptured(bool isPlayer)
        {
            SetColor(isPlayer ? PlayerColor : AIColor);
        }

        /// <summary>Called by ContestedState.Enter() — both teams present, gauge frozen.</summary>
        public void SetContested()
        {
            SetColor(ContestedColor);
        }

        private void SetColor(Color color)
        {
            _zoneSprite.color = color;
        }
    }
}
