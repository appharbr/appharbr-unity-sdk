using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[InitializeOnLoad]
public class AppHarbrPostprocessor
{
    private static readonly string OldAarPath = "Assets/AppHarbrSDK/Plugins/Android/AH-SDK-Android.aar";
    private static readonly string OldBridgeAarPath = "Assets/AppHarbrSDK/Plugins/Android/appharbr-unity-mediations-plugin.aar";

    // Source files - support both UPM and manual import
    private static readonly string UpmSourcePathInterstitial = "Packages/com.appharbr.sdk/Plugins/Android/InterstitialAd.zip";
    private static readonly string UpmSourcePathRewarded = "Packages/com.appharbr.sdk/Plugins/Android/RewardedAd.zip";
    private static readonly string AssetSourcePathInterstitial = "Assets/AppHarbrSDK/Plugins/Android/InterstitialAd.zip";
    private static readonly string AssetSourcePathRewarded = "Assets/AppHarbrSDK/Plugins/Android/RewardedAd.zip";

    // Asset-based paths for manual LevelPlay import
    private static readonly string AssetFilePathInterstitial = "Assets/LevelPlay/Runtime/Plugins/Android/InterstitialAd.java";
    private static readonly string AssetFilePathRewarded = "Assets/LevelPlay/Runtime/Plugins/Android/RewardedAd.java";

    // Local override directory for UPM packages (in Assets, so it persists)
    private static readonly string LocalOverrideDir = "Assets/AppHarbrSDK/LevelPlayOverrides/Android";
    private static readonly string LocalOverrideInterstitial = "Assets/AppHarbrSDK/LevelPlayOverrides/Android/InterstitialAd.java";
    private static readonly string LocalOverrideRewarded = "Assets/AppHarbrSDK/LevelPlayOverrides/Android/RewardedAd.java";

    // UPM package constants
    private static readonly string UpmPackageName = "com.unity.services.levelplay";

    // EditorPrefs key
    private static readonly string LastLevelPlayVersionKey = "AppHarbr_LastLevelPlayVersion";
    private static readonly string LastInstallationTypeKey = "AppHarbr_LastInstallationType";
    private static readonly string MigrationFlagKey = "AppHarbr.SDK.MigrationCompleted";

    static AppHarbrPostprocessor()
    {
        RemoveOldFile(OldAarPath);
        RemoveOldFile(OldAarPath + ".meta");
        RemoveOldFile(OldBridgeAarPath);
        RemoveOldFile(OldBridgeAarPath + ".meta");

        // Delay integration to ensure packages are loaded AND migration is complete (if running)
        EditorApplication.delayCall += () =>
        {
            // Check if both UPM and manual AppHarbr exist (migration scenario)
            bool upmAppHarbrExists = File.Exists("Packages/com.appharbr.sdk/package.json");
            bool manualAppHarbrExists = Directory.Exists("Assets/AppHarbrSDK");
            bool migrationCompleted = EditorPrefs.GetBool(MigrationFlagKey, false);

            // Run integration if:
            // 1. Migration already completed, OR
            // 2. Not a migration scenario (either only UPM, only manual, or neither)
            bool isMigrationScenario = upmAppHarbrExists && manualAppHarbrExists;

            if (migrationCompleted || !isMigrationScenario)
            {
                ReplaceLevelPlayIntegration();
            }
            else
            {
                // Migration scenario but not completed yet
                // Schedule another delay to wait for migration dialog
                EditorApplication.delayCall += ReplaceLevelPlayIntegration;
            }
        };
    }

    private static void RemoveOldFile(string pathToRemove)
    {
        if (File.Exists(pathToRemove))
        {
            try
            {
                File.Delete(pathToRemove);
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AppHarbr] Error removing file: {pathToRemove}. {e.Message}");
            }
        }
    }

    private static void ReplaceLevelPlayIntegration()
    {
        // Find AppHarbr source files (could be in UPM package or Assets)
        string sourceInterstitial = FindAppHarbrSourceFile(UpmSourcePathInterstitial, AssetSourcePathInterstitial);
        string sourceRewarded = FindAppHarbrSourceFile(UpmSourcePathRewarded, AssetSourcePathRewarded);

        if (string.IsNullOrEmpty(sourceInterstitial) || string.IsNullOrEmpty(sourceRewarded))
        {
            return; // Source files not found, silently skip
        }

        // Detect LevelPlay installation type
        LevelPlayInstallationType installType = DetectLevelPlayInstallation();

        switch (installType)
        {
            case LevelPlayInstallationType.ManualAssets:
                HandleManualInstallation(sourceInterstitial, sourceRewarded);
                break;

            case LevelPlayInstallationType.UpmPackage:
                HandleUpmInstallation(sourceInterstitial, sourceRewarded);
                break;

            case LevelPlayInstallationType.NotFound:
                // LevelPlay not installed, silently skip
                break;
        }
    }

    private static void HandleManualInstallation(string sourceInterstitial, string sourceRewarded)
    {
        bool success = true;

        if (!ReplaceFile(sourceInterstitial, AssetFilePathInterstitial, "InterstitialAd.java"))
        {
            success = false;
        }

        if (!ReplaceFile(sourceRewarded, AssetFilePathRewarded, "RewardedAd.java"))
        {
            success = false;
        }

        if (success)
        {
            // Save installation type
            EditorPrefs.SetString(LastInstallationTypeKey, "Manual");
            Debug.Log("[AppHarbr] LevelPlay integration completed successfully");
        }
    }

    private static void HandleUpmInstallation(string sourceInterstitial, string sourceRewarded)
    {
        // Get current LevelPlay version
        string currentVersion = GetLevelPlayVersion();
        string lastVersion = EditorPrefs.GetString(LastLevelPlayVersionKey, "");
        string lastInstallType = EditorPrefs.GetString(LastInstallationTypeKey, "");

        bool versionChanged = !string.IsNullOrEmpty(currentVersion) && currentVersion != lastVersion;
        bool installTypeChanged = lastInstallType == "Manual"; // Migrated from manual to UPM
        bool overridesExist = File.Exists(LocalOverrideInterstitial) && File.Exists(LocalOverrideRewarded);

        // Auto-refresh if version changed OR installation type changed
        if ((versionChanged || installTypeChanged) && overridesExist)
        {
            if (File.Exists(LocalOverrideInterstitial))
            {
                File.Delete(LocalOverrideInterstitial);
            }

            if (File.Exists(LocalOverrideRewarded))
            {
                File.Delete(LocalOverrideRewarded);
            }
        }

        // Create local override directory
        if (!Directory.Exists(LocalOverrideDir))
        {
            Directory.CreateDirectory(LocalOverrideDir);
        }

        // Copy files to local override location
        bool interstitialCopied = CopyToLocalOverride(sourceInterstitial, LocalOverrideInterstitial, "InterstitialAd.java");
        bool rewardedCopied = CopyToLocalOverride(sourceRewarded, LocalOverrideRewarded, "RewardedAd.java");

        if (interstitialCopied && rewardedCopied)
        {
            // Save current version and installation type
            if (!string.IsNullOrEmpty(currentVersion))
            {
                EditorPrefs.SetString(LastLevelPlayVersionKey, currentVersion);
            }
            EditorPrefs.SetString(LastInstallationTypeKey, "UPM");

            AssetDatabase.Refresh();
            Debug.Log("[AppHarbr] LevelPlay integration completed successfully");
        }
    }

    private static bool CopyToLocalOverride(string sourceZipPath, string targetPath, string fileName)
    {
        try
        {
            // Check if file already exists
            if (File.Exists(targetPath))
            {
                return true;
            }

            File.Copy(sourceZipPath, targetPath, true);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AppHarbr] Integration failed: {e.Message}");
            return false;
        }
    }

    private static LevelPlayInstallationType DetectLevelPlayInstallation()
    {
        // Check for manual installation first (in Assets)
        if (File.Exists(AssetFilePathInterstitial) || File.Exists(AssetFilePathRewarded))
        {
            return LevelPlayInstallationType.ManualAssets;
        }

        // Check for UPM installation
        if (IsLevelPlayInstalledViaUpm())
        {
            return LevelPlayInstallationType.UpmPackage;
        }

        return LevelPlayInstallationType.NotFound;
    }

    private static bool IsLevelPlayInstalledViaUpm()
    {
        try
        {
            // Try PackageManager API
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{UpmPackageName}");
            if (packageInfo != null)
            {
                return true;
            }

            // Fallback: check PackageCache directory
            string packageCacheDir = Path.Combine("Library", "PackageCache");
            if (Directory.Exists(packageCacheDir))
            {
                var levelPlayDirs = Directory.GetDirectories(packageCacheDir, $"{UpmPackageName}@*", SearchOption.TopDirectoryOnly);
                return levelPlayDirs.Length > 0;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool ReplaceFile(string sourceZipPath, string targetFilePath, string targetFileName)
    {
        try
        {
            // Ensure target directory exists
            string targetDirectory = Path.GetDirectoryName(targetFilePath);
            if (!Directory.Exists(targetDirectory))
            {
                return false;
            }

            // Copy source file to destination
            File.Copy(sourceZipPath, targetFilePath, true);
            AssetDatabase.Refresh();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AppHarbr] Integration failed: {e.Message}");
            return false;
        }
    }

    private static string GetLevelPlayVersion()
    {
        try
        {
            // Try PackageManager API first
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{UpmPackageName}");
            if (packageInfo != null)
            {
                return packageInfo.version;
            }

            // Fallback: parse from PackageCache directory name
            string packageCacheDir = Path.Combine("Library", "PackageCache");
            if (Directory.Exists(packageCacheDir))
            {
                var levelPlayDirs = Directory.GetDirectories(packageCacheDir, $"{UpmPackageName}@*", SearchOption.TopDirectoryOnly);
                if (levelPlayDirs.Length > 0)
                {
                    var dirName = Path.GetFileName(levelPlayDirs[0]);
                    string prefix = $"{UpmPackageName}@";
                    if (dirName.Length > prefix.Length)
                    {
                        return dirName.Substring(prefix.Length);
                    }
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string FindAppHarbrSourceFile(string upmPath, string assetPath)
    {
        if (File.Exists(upmPath))
        {
            return upmPath;
        }

        if (File.Exists(assetPath))
        {
            return assetPath;
        }

        return null;
    }

    private enum LevelPlayInstallationType
    {
        NotFound,
        ManualAssets,
        UpmPackage
    }

    // Menu item to manually refresh integration
    [MenuItem("Tools/AppHarbr/Refresh LevelPlay Integration", true)]
    private static bool ValidateManualRefreshIntegration()
    {
        // Only show menu item if LevelPlay is installed
        return DetectLevelPlayInstallation() != LevelPlayInstallationType.NotFound;
    }

    [MenuItem("Tools/AppHarbr/Refresh LevelPlay Integration")]
    public static void ManualRefreshIntegration()
    {
        // Clear stored version and installation type to force refresh
        EditorPrefs.DeleteKey(LastLevelPlayVersionKey);
        EditorPrefs.DeleteKey(LastInstallationTypeKey);

        // Force delete existing overrides to recreate them
        if (File.Exists(LocalOverrideInterstitial))
        {
            File.Delete(LocalOverrideInterstitial);
        }

        if (File.Exists(LocalOverrideRewarded))
        {
            File.Delete(LocalOverrideRewarded);
        }

        ReplaceLevelPlayIntegration();
    }
}