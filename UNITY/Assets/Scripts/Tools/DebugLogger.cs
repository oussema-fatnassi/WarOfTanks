using UnityEngine;

/// <summary>
/// Centralized debug logging utility for the MAM project.
///
/// Log        — only emits in Editor or Development Build, AND only when IsEnabled is true.
/// LogWarning — always emits (Editor + builds), regardless of IsEnabled.
/// LogError   — always emits (Editor + builds), regardless of IsEnabled.
///
/// Build behaviour:
///   Release build (no DEVELOPMENT_BUILD symbol) → Log calls are stripped at compile
///   time via [System.Diagnostics.Conditional]. Zero performance cost, no allocation.
///
/// Usage:
///   // Global control only:
///   DebugLogger.Log("Player spawned", nameof(RandomSpawnZone));
///
///   // Per-script control (script keeps its own [SerializeField] bool):
///   DebugLogger.Log(_showDebugLogs, "Playing animation", nameof(AnimationComponent));
///
///   // Always visible:
///   DebugLogger.LogWarning("No animator found", nameof(AnimationComponent));
///   DebugLogger.LogError("Controller missing", nameof(InteractionComponent));
/// </summary>
public static class DebugLogger
{
    /// <summary>
    /// Global master switch. Set via GameManager inspector checkbox.
    /// When false, all Log calls are silenced at runtime (dev builds and editor).
    /// Has no effect in release builds — Log calls are already stripped.
    /// </summary>
    public static bool IsEnabled { get; set; } = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetBeforeSceneLoad()
    {
        // Default to disabled until GameManager explicitly applies the project setting.
        IsEnabled = false;
    }

    // -------------------------------------------------------------------------
    // Log — global control only
    // -------------------------------------------------------------------------

    /// <summary>
    /// Emits a Debug.Log only in Editor or Development Build, and only when IsEnabled is true.
    /// Calls are entirely removed from release builds by the C# compiler.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR"),
     System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message, string context = "")
    {
        if (!IsEnabled) return;
        Debug.Log(string.IsNullOrEmpty(context) ? message : $"[{context}] {message}");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"),
     System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message, Object context)
    {
        if (!IsEnabled) return;
        Debug.Log(message, context);
    }

    // -------------------------------------------------------------------------
    // Log — per-script control
    // -------------------------------------------------------------------------

    /// <summary>
    /// Emits a Debug.Log only when both IsEnabled and the per-script condition are true.
    /// Use this overload in scripts that keep their own [SerializeField] bool toggle.
    /// Calls are entirely removed from release builds by the C# compiler.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR"),
     System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(bool condition, string message, string context = "")
    {
        if (!IsEnabled || !condition) return;
        Debug.Log(string.IsNullOrEmpty(context) ? message : $"[{context}] {message}");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR"),
     System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(bool condition, string message, Object context)
    {
        if (!IsEnabled || !condition) return;
        Debug.Log(message, context);
    }

    // -------------------------------------------------------------------------
    // LogWarning — always active
    // -------------------------------------------------------------------------

    /// <summary>
    /// Emits a Debug.LogWarning in all build types. Not affected by IsEnabled.
    /// </summary>
    public static void LogWarning(string message, string context = "")
    {
        Debug.LogWarning(string.IsNullOrEmpty(context) ? message : $"[{context}] {message}");
    }

    public static void LogWarning(string message, Object context)
    {
        Debug.LogWarning(message, context);
    }

    // -------------------------------------------------------------------------
    // LogError — always active
    // -------------------------------------------------------------------------

    /// <summary>
    /// Emits a Debug.LogError in all build types. Not affected by IsEnabled.
    /// </summary>
    public static void LogError(string message, string context = "")
    {
        Debug.LogError(string.IsNullOrEmpty(context) ? message : $"[{context}] {message}");
    }

    public static void LogError(string message, Object context)
    {
        Debug.LogError(message, context);
    }
}
