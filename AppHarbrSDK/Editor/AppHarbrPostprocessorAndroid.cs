using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// AppHarbr SDK - LevelPlay Integration Manager
/// Automatically manages integration with LevelPlay SDK for both manual and UPM installations
/// </summary>
[InitializeOnLoad]
public class AppHarbrPostprocessor : IPreprocessBuildWithReport
{
    #region Constants

    // Legacy AAR files to remove (old SDK versions)
    private const string OLD_AAR_PATH = "Assets/AppHarbrSDK/Plugins/Android/AH-SDK-Android.aar";
    private const string OLD_BRIDGE_AAR_PATH = "Assets/AppHarbrSDK/Plugins/Android/appharbr-unity-mediations-plugin.aar";

    // AppHarbr source files (support both UPM and manual installation)
    private const string UPM_SOURCE_INTERSTITIAL = "Packages/com.appharbr.sdk/Plugins/Android/InterstitialAd.zip";
    private const string UPM_SOURCE_REWARDED = "Packages/com.appharbr.sdk/Plugins/Android/RewardedAd.zip";
    private const string ASSET_SOURCE_INTERSTITIAL = "Assets/AppHarbrSDK/Plugins/Android/InterstitialAd.zip";
    private const string ASSET_SOURCE_REWARDED = "Assets/AppHarbrSDK/Plugins/Android/RewardedAd.zip";

    // LevelPlay manual installation paths
    private const string LEVELPLAY_MANUAL_INTERSTITIAL = "Assets/LevelPlay/Runtime/Plugins/Android/InterstitialAd.java";
    private const string LEVELPLAY_MANUAL_REWARDED = "Assets/LevelPlay/Runtime/Plugins/Android/RewardedAd.java";

    // LevelPlay UPM override paths (in Assets to persist)
    private const string LEVELPLAY_UPM_OVERRIDE_DIR = "Assets/Plugins/AppHarbr/LevelPlayOverrides/Android";
    private const string LEVELPLAY_UPM_OVERRIDE_INTERSTITIAL = "Assets/Plugins/AppHarbr/LevelPlayOverrides/Android/InterstitialAd.java";
    private const string LEVELPLAY_UPM_OVERRIDE_REWARDED = "Assets/Plugins/AppHarbr/LevelPlayOverrides/Android/RewardedAd.java";

    // LevelPlay UPM package name
    private const string LEVELPLAY_PACKAGE_NAME = "com.unity.services.levelplay";

    // EditorPrefs key for migration tracking (shared with AppHarbrMigration.cs)
    private const string MIGRATION_FLAG_KEY = "AppHarbr.SDK.MigrationCompleted";

    // Flag to track if migration is in progress
    private static bool isWaitingForMigration = false;

    #endregion

    #region Build Preprocessor

    public int callbackOrder => 0;

    /// <summary>
    /// Pre-build hook: Ensures LevelPlay integration is up-to-date before Android builds
    /// </summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
        {
            return;
        }

        // Wait for migration to complete if in progress
        while (isWaitingForMigration)
        {
            System.Threading.Thread.Sleep(100);
        }

        PerformLevelPlayIntegration(forceRefresh: true);
    }

    #endregion

    #region Initialization

    static AppHarbrPostprocessor()
    {
        // Clean up old legacy files
        CleanupLegacyFiles();

        // Perform initial integration check on Unity load
        EditorApplication.delayCall += InitialIntegrationCheck;
    }

    private static void InitialIntegrationCheck()
    {
        // Check if we're in a migration scenario (both UPM and manual AppHarbr exist)
        bool isUpmAppHarbr = File.Exists("Packages/com.appharbr.sdk/package.json");
        bool isManualAppHarbr = Directory.Exists("Assets/AppHarbrSDK");
        bool migrationCompleted = EditorPrefs.GetBool(MIGRATION_FLAG_KEY, false);

        if (isUpmAppHarbr && isManualAppHarbr && !migrationCompleted)
        {
            // Migration in progress - wait for it to complete
            isWaitingForMigration = true;
            WaitForMigrationCompletion();
        }
        else
        {
            // No migration needed, proceed with integration
            PerformLevelPlayIntegration(forceRefresh: false);
        }
    }

    /// <summary>
    /// Called by AppHarbrMigration when migration dialog completes
    /// Public so it can be invoked via reflection
    /// </summary>
    public static void OnMigrationComplete()
    {
        isWaitingForMigration = false;
        EditorApplication.delayCall += () => PerformLevelPlayIntegration(forceRefresh: false);
    }

    /// <summary>
    /// Polls for migration completion before running integration (fallback if notification fails)
    /// </summary>
    private static void WaitForMigrationCompletion()
    {
        // Check if migration completed
        bool migrationCompleted = EditorPrefs.GetBool(MIGRATION_FLAG_KEY, false);
        bool manualStillExists = Directory.Exists("Assets/AppHarbrSDK");

        if (migrationCompleted || !manualStillExists)
        {
            // Migration completed (either user chose to remove or keep)
            isWaitingForMigration = false;
            PerformLevelPlayIntegration(forceRefresh: false);
        }
        else if (!isWaitingForMigration)
        {
            // Migration completed via notification, don't keep polling
            return;
        }
        else
        {
            // Still waiting, check again in 500ms
            EditorApplication.delayCall += WaitForMigrationCompletion;
        }
    }

    private static void CleanupLegacyFiles()
    {
        DeleteFileIfExists(OLD_AAR_PATH);
        DeleteFileIfExists(OLD_AAR_PATH + ".meta");
        DeleteFileIfExists(OLD_BRIDGE_AAR_PATH);
        DeleteFileIfExists(OLD_BRIDGE_AAR_PATH + ".meta");
    }

    #endregion

    #region Core Integration Logic

    /// <summary>
    /// Main integration method: detects LevelPlay installation and integrates AppHarbr files
    /// </summary>
    private static void PerformLevelPlayIntegration(bool forceRefresh)
    {
        // Find AppHarbr source files (try UPM first, then Assets)
        string sourceInterstitial = FindFile(UPM_SOURCE_INTERSTITIAL, ASSET_SOURCE_INTERSTITIAL);
        string sourceRewarded = FindFile(UPM_SOURCE_REWARDED, ASSET_SOURCE_REWARDED);

        if (string.IsNullOrEmpty(sourceInterstitial) || string.IsNullOrEmpty(sourceRewarded))
        {
            // AppHarbr source files not found, nothing to do
            return;
        }

        // Detect LevelPlay installation type
        LevelPlayInstallType installType = DetectLevelPlayInstallation();

        switch (installType)
        {
            case LevelPlayInstallType.Manual:
                IntegrateManualLevelPlay(sourceInterstitial, sourceRewarded, forceRefresh);
                break;

            case LevelPlayInstallType.UPM:
                IntegrateUpmLevelPlay(sourceInterstitial, sourceRewarded, forceRefresh);
                break;

            case LevelPlayInstallType.NotInstalled:
                // LevelPlay not installed, nothing to do
                break;
        }
    }

    /// <summary>
    /// Integrates with manually installed LevelPlay (in Assets folder)
    /// </summary>
    private static void IntegrateManualLevelPlay(string sourceInterstitial, string sourceRewarded, bool forceRefresh)
    {
        bool success = true;

        success &= CopyFile(sourceInterstitial, LEVELPLAY_MANUAL_INTERSTITIAL, forceRefresh);
        success &= CopyFile(sourceRewarded, LEVELPLAY_MANUAL_REWARDED, forceRefresh);

        if (success && forceRefresh)
        {
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// Integrates with UPM-installed LevelPlay (creates overrides in Assets)
    /// </summary>
    private static void IntegrateUpmLevelPlay(string sourceInterstitial, string sourceRewarded, bool forceRefresh)
    {
        // Ensure override directory exists
        if (!Directory.Exists(LEVELPLAY_UPM_OVERRIDE_DIR))
        {
            Directory.CreateDirectory(LEVELPLAY_UPM_OVERRIDE_DIR);
        }

        bool success = true;

        success &= CopyFile(sourceInterstitial, LEVELPLAY_UPM_OVERRIDE_INTERSTITIAL, forceRefresh);
        success &= CopyFile(sourceRewarded, LEVELPLAY_UPM_OVERRIDE_REWARDED, forceRefresh);

        if (success && forceRefresh)
        {
            AssetDatabase.Refresh();
        }
    }

    #endregion

    #region Detection Methods

    private enum LevelPlayInstallType
    {
        NotInstalled,
        Manual,
        UPM
    }

    /// <summary>
    /// Detects how LevelPlay is installed (manual in Assets vs UPM package)
    /// </summary>
    private static LevelPlayInstallType DetectLevelPlayInstallation()
    {
        // Check for manual installation first (files in Assets folder)
        if (File.Exists(LEVELPLAY_MANUAL_INTERSTITIAL) || File.Exists(LEVELPLAY_MANUAL_REWARDED))
        {
            return LevelPlayInstallType.Manual;
        }

        // Check for UPM installation
        if (IsLevelPlayInstalledViaUpm())
        {
            return LevelPlayInstallType.UPM;
        }

        return LevelPlayInstallType.NotInstalled;
    }

    /// <summary>
    /// Checks if LevelPlay is installed via Unity Package Manager
    /// </summary>
    private static bool IsLevelPlayInstalledViaUpm()
    {
        try
        {
            // Try Unity's PackageInfo API first
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{LEVELPLAY_PACKAGE_NAME}");
            if (packageInfo != null)
            {
                return true;
            }

            // Fallback: check PackageCache directory
            string packageCacheDir = Path.Combine("Library", "PackageCache");
            if (Directory.Exists(packageCacheDir))
            {
                var levelPlayDirs = Directory.GetDirectories(packageCacheDir, $"{LEVELPLAY_PACKAGE_NAME}@*", SearchOption.TopDirectoryOnly);
                return levelPlayDirs.Length > 0;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the installed LevelPlay version (for UPM installations)
    /// </summary>
    private static string GetLevelPlayVersion()
    {
        try
        {
            // Try PackageInfo API
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{LEVELPLAY_PACKAGE_NAME}");
            if (packageInfo != null)
            {
                return packageInfo.version;
            }

            // Fallback: parse from PackageCache directory name
            string packageCacheDir = Path.Combine("Library", "PackageCache");
            if (Directory.Exists(packageCacheDir))
            {
                var levelPlayDirs = Directory.GetDirectories(packageCacheDir, $"{LEVELPLAY_PACKAGE_NAME}@*", SearchOption.TopDirectoryOnly);
                if (levelPlayDirs.Length > 0)
                {
                    string dirName = Path.GetFileName(levelPlayDirs[0]);
                    string prefix = $"{LEVELPLAY_PACKAGE_NAME}@";
                    return dirName.Substring(prefix.Length);
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region File Utilities

    /// <summary>
    /// Finds first existing file from multiple possible paths
    /// </summary>
    private static string FindFile(params string[] paths)
    {
        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    /// <summary>
    /// Copies file from source to destination with optional force refresh
    /// </summary>
    private static bool CopyFile(string sourcePath, string destPath, bool forceRefresh)
    {
        try
        {
            // Ensure destination directory exists
            string destDir = Path.GetDirectoryName(destPath);
            if (!Directory.Exists(destDir))
            {
                return false; // Target directory doesn't exist (LevelPlay not installed)
            }

            // Check if copy is needed (unless forcing refresh)
            if (!forceRefresh && File.Exists(destPath))
            {
                // Quick size comparison to avoid unnecessary copy
                FileInfo sourceInfo = new FileInfo(sourcePath);
                FileInfo destInfo = new FileInfo(destPath);

                if (sourceInfo.Length == destInfo.Length)
                {
                    return true; // Files appear identical
                }
            }

            // Perform copy
            File.Copy(sourcePath, destPath, overwrite: true);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AppHarbr] Failed to copy {Path.GetFileName(sourcePath)}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Safely deletes a file if it exists
    /// </summary>
    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                File.Delete(path);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[AppHarbr] Could not delete {path}: {e.Message}");
            }
        }
    }

    #endregion

    #region Menu Items

    [MenuItem("Tools/AppHarbr/Refresh LevelPlay Integration", true)]
    private static bool ValidateManualRefresh()
    {
        return DetectLevelPlayInstallation() != LevelPlayInstallType.NotInstalled;
    }

    [MenuItem("Tools/AppHarbr/Refresh LevelPlay Integration")]
    public static void ManualRefreshIntegration()
    {
        // Force delete existing override files to ensure clean refresh
        DeleteFileIfExists(LEVELPLAY_UPM_OVERRIDE_INTERSTITIAL);
        DeleteFileIfExists(LEVELPLAY_UPM_OVERRIDE_REWARDED);

        // Perform integration with force refresh
        PerformLevelPlayIntegration(forceRefresh: true);

        Debug.Log("[AppHarbr] LevelPlay integration refreshed successfully");
    }

    #endregion
}