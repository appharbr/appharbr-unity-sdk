using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AppHarbrPostprocessor
{
    // Dynamic path detection - works for both UPM and manual import
    private static readonly string PackagePath = GetPackagePath();
    private static readonly string OldAarPath = $"{PackagePath}/Plugins/Android/AH-SDK-Android.aar";
    private static readonly string OldBridgeAarPath = $"{PackagePath}/Plugins/Android/appharbr-unity-mediations-plugin.aar";
    private static readonly string SourceFilePathInterstitial = $"{PackagePath}/Plugins/Android/InterstitialAd.zip";
    private static readonly string FileToCheckPathInterstitial = "Assets/LevelPlay/Runtime/Plugins/Android/InterstitialAd.java";
    private static readonly string SourceFilePathRewarded = $"{PackagePath}/Plugins/Android/RewardedAd.zip";
    private static readonly string FileToCheckPathRewarded = "Assets/LevelPlay/Runtime/Plugins/Android/RewardedAd.java";

    static AppHarbrPostprocessor()
    {
        RemoveOldFile(OldAarPath);
        RemoveOldFile(OldAarPath + ".meta");

        RemoveOldFile(OldBridgeAarPath);
        RemoveOldFile(OldBridgeAarPath + ".meta");

        ReplaceLevelPlayIntegration();
    }

    /// <summary>
    /// Detects whether the SDK is installed via UPM or manually imported to Assets
    /// </summary>
    private static string GetPackagePath()
    {
        // Check if installed via UPM (Package Manager)
        if (Directory.Exists("Packages/com.appharbr.sdk"))
        {
            return "Packages/com.appharbr.sdk";
        }
        
        // Check if manually imported to Assets folder
        if (Directory.Exists("Assets/AppHarbrSDK"))
        {
            return "Assets/AppHarbrSDK";
        }
        
        // Fallback to UPM path
        Debug.LogWarning("[AppHarbr] Could not detect SDK installation path. Using default UPM path.");
        return "Packages/com.appharbr.sdk";
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
                Debug.LogError($"[AppHarbr] Error removing File: {e.Message}");
            }
        }
    }

    private static void ReplaceLevelPlayIntegration()
    {
        ReplaceLevelPlayIntegrationIfExists(SourceFilePathInterstitial, FileToCheckPathInterstitial);
        ReplaceLevelPlayIntegrationIfExists(SourceFilePathRewarded, FileToCheckPathRewarded);
    }

    private static void ReplaceLevelPlayIntegrationIfExists(string NewSource, string FileToCheckPath)
    {
        // Check if the target file exists (InterstitialAd.java)
        if (File.Exists(FileToCheckPath))
        {
            try
            {
                // Copy source file to destination (replacing the target file)
                File.Copy(NewSource, FileToCheckPath, true);
                Debug.Log($"[AppHarbr] LevelPlay Compatibility Successfully Done");

                // Refresh AssetDatabase to recognize the changes
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AppHarbr] Error LevelPlay Integration: {e.Message}");
            }
        }
    }
}