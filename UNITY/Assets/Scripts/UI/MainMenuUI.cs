using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Runtime controller for the generated main-menu prefab. Handles Play / Settings / Quit and the
/// editable match specs. All references are wired by <c>MainMenuUIGenerator</c>; the field names here
/// are the serialized-property paths the generator assigns to.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    #region Serialized references
    [Header("Panels")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _backButton;

    [Header("Spec fields")]
    [SerializeField] private TMP_InputField _tankHealthField;
    [SerializeField] private TMP_InputField _fireRateField;
    [SerializeField] private TMP_InputField _respawnDelayField;
    [SerializeField] private TMP_InputField _bulletDamageField;
    [SerializeField] private TMP_InputField _explosionRadiusField;
    [SerializeField] private TMP_InputField _matchDurationField;
    [SerializeField] private TMP_InputField _scoreLimitField;
    [SerializeField] private Slider _volumeSlider;

    [Header("Scene")]
    [SerializeField] private string _gameSceneName = "Game";
    #endregion

    private void Awake()
    {
        MatchSettings.ApplyAudio();
    }

    private void Start()
    {
        if (_playButton != null) _playButton.onClick.AddListener(Play);
        if (_settingsButton != null) _settingsButton.onClick.AddListener(OpenSettings);
        if (_quitButton != null) _quitButton.onClick.AddListener(Quit);
        if (_saveButton != null) _saveButton.onClick.AddListener(SaveSettings);
        if (_resetButton != null) _resetButton.onClick.AddListener(ResetSettings);
        if (_backButton != null) _backButton.onClick.AddListener(CloseSettings);

#if UNITY_WEBGL
        if (_quitButton != null) _quitButton.gameObject.SetActive(false);
#endif
        ShowMain();
    }

    #region Actions
    public void Play() => SceneManager.LoadScene(_gameSceneName);

    public void Quit()
    {
#if !UNITY_WEBGL
        Application.Quit();
#endif
    }

    public void OpenSettings()
    {
        LoadFields();
        ShowSettings();
    }

    public void CloseSettings() => ShowMain();

    public void SaveSettings()
    {
        if (_tankHealthField != null) MatchSettings.TankMaxHealth = ParseFloat(_tankHealthField, MatchSettings.TankMaxHealth);
        if (_fireRateField != null) MatchSettings.FireRate = ParseFloat(_fireRateField, MatchSettings.FireRate);
        if (_respawnDelayField != null) MatchSettings.RespawnDelay = ParseFloat(_respawnDelayField, MatchSettings.RespawnDelay);
        if (_bulletDamageField != null) MatchSettings.BulletDamage = ParseFloat(_bulletDamageField, MatchSettings.BulletDamage);
        if (_explosionRadiusField != null) MatchSettings.ExplosionRadius = ParseFloat(_explosionRadiusField, MatchSettings.ExplosionRadius);
        if (_matchDurationField != null) MatchSettings.MatchDuration = ParseFloat(_matchDurationField, MatchSettings.MatchDuration);
        if (_scoreLimitField != null) MatchSettings.ScoreLimit = ParseInt(_scoreLimitField, MatchSettings.ScoreLimit);
        if (_volumeSlider != null) MatchSettings.MasterVolume = _volumeSlider.value;

        MatchSettings.Save();
        MatchSettings.ApplyAudio();
        ShowMain();
    }

    public void ResetSettings()
    {
        MatchSettings.ResetToDefaults();
        MatchSettings.ApplyAudio();
        LoadFields();
    }
    #endregion

    #region Helpers
    private void LoadFields()
    {
        SetText(_tankHealthField, MatchSettings.TankMaxHealth);
        SetText(_fireRateField, MatchSettings.FireRate);
        SetText(_respawnDelayField, MatchSettings.RespawnDelay);
        SetText(_bulletDamageField, MatchSettings.BulletDamage);
        SetText(_explosionRadiusField, MatchSettings.ExplosionRadius);
        SetText(_matchDurationField, MatchSettings.MatchDuration);
        if (_scoreLimitField != null) _scoreLimitField.text = MatchSettings.ScoreLimit.ToString();
        if (_volumeSlider != null) _volumeSlider.value = MatchSettings.MasterVolume;
    }

    private void ShowMain()
    {
        if (_mainPanel != null) _mainPanel.SetActive(true);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    private void ShowSettings()
    {
        if (_mainPanel != null) _mainPanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(true);
    }

    private static void SetText(TMP_InputField field, float value)
    {
        if (field != null) field.text = value.ToString("0.##");
    }

    private static float ParseFloat(TMP_InputField field, float fallback)
        => float.TryParse(field.text, out float v) ? v : fallback;

    private static int ParseInt(TMP_InputField field, int fallback)
        => int.TryParse(field.text, out int v) ? v : fallback;
    #endregion
}
