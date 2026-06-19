using UnityEngine;

/// <summary>
/// Player-adjustable match specifications, persisted with <see cref="PlayerPrefs"/>.
/// Defaults intentionally match the prefab/Inspector values, so gameplay is unchanged until the
/// player edits them in the main-menu settings panel. Gameplay components read these in their
/// <c>Awake</c>; the settings UI writes them and calls <see cref="Save"/>.
/// </summary>
public static class MatchSettings
{
    private const string KeyTankMaxHealth = "wot.tankMaxHealth";
    private const string KeyFireRate = "wot.fireRate";
    private const string KeyRespawnDelay = "wot.respawnDelay";
    private const string KeyBulletDamage = "wot.bulletDamage";
    private const string KeyExplosionRadius = "wot.explosionRadius";
    private const string KeyMatchDuration = "wot.matchDuration";
    private const string KeyScoreLimit = "wot.scoreLimit";
    private const string KeyMasterVolume = "wot.masterVolume";

    // Defaults mirror the current prefab/Inspector values.
    public const float DefaultTankMaxHealth = 100f;
    public const float DefaultFireRate = 1f;
    public const float DefaultRespawnDelay = 5f;
    public const float DefaultBulletDamage = 10f;
    public const float DefaultExplosionRadius = 2f;
    public const float DefaultMatchDuration = 180f;
    public const int DefaultScoreLimit = 100;
    public const float DefaultMasterVolume = 1f;

    public static float TankMaxHealth
    {
        get => PlayerPrefs.GetFloat(KeyTankMaxHealth, DefaultTankMaxHealth);
        set => PlayerPrefs.SetFloat(KeyTankMaxHealth, value);
    }

    public static float FireRate
    {
        get => PlayerPrefs.GetFloat(KeyFireRate, DefaultFireRate);
        set => PlayerPrefs.SetFloat(KeyFireRate, value);
    }

    public static float RespawnDelay
    {
        get => PlayerPrefs.GetFloat(KeyRespawnDelay, DefaultRespawnDelay);
        set => PlayerPrefs.SetFloat(KeyRespawnDelay, value);
    }

    public static float BulletDamage
    {
        get => PlayerPrefs.GetFloat(KeyBulletDamage, DefaultBulletDamage);
        set => PlayerPrefs.SetFloat(KeyBulletDamage, value);
    }

    public static float ExplosionRadius
    {
        get => PlayerPrefs.GetFloat(KeyExplosionRadius, DefaultExplosionRadius);
        set => PlayerPrefs.SetFloat(KeyExplosionRadius, value);
    }

    public static float MatchDuration
    {
        get => PlayerPrefs.GetFloat(KeyMatchDuration, DefaultMatchDuration);
        set => PlayerPrefs.SetFloat(KeyMatchDuration, value);
    }

    public static int ScoreLimit
    {
        get => PlayerPrefs.GetInt(KeyScoreLimit, DefaultScoreLimit);
        set => PlayerPrefs.SetInt(KeyScoreLimit, value);
    }

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(KeyMasterVolume, DefaultMasterVolume);
        set => PlayerPrefs.SetFloat(KeyMasterVolume, Mathf.Clamp01(value));
    }

    /// <summary>Persists any pending changes to disk.</summary>
    public static void Save() => PlayerPrefs.Save();

    /// <summary>Restores every spec to its default by clearing the stored keys.</summary>
    public static void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(KeyTankMaxHealth);
        PlayerPrefs.DeleteKey(KeyFireRate);
        PlayerPrefs.DeleteKey(KeyRespawnDelay);
        PlayerPrefs.DeleteKey(KeyBulletDamage);
        PlayerPrefs.DeleteKey(KeyExplosionRadius);
        PlayerPrefs.DeleteKey(KeyMatchDuration);
        PlayerPrefs.DeleteKey(KeyScoreLimit);
        PlayerPrefs.DeleteKey(KeyMasterVolume);
        PlayerPrefs.Save();
    }

    /// <summary>Applies the saved master volume to the global audio listener.</summary>
    public static void ApplyAudio() => AudioListener.volume = MasterVolume;
}
