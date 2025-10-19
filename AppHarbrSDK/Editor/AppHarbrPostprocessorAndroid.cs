using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AppHarbrPostprocessor
{
    private static readonly string OldAarPath = "Assets/AppHarbrSDK/Plugins/Android/AH-SDK-Android.aar";
    private static readonly string OldBridgeAarPath = "Assets/AppHarbrSDK/Plugins/Android/appharbr-unity-mediations-plugin.aar";
    private static readonly string SourceFilePathInterstitial = "Assets/AppHarbrSDK/Plugins/Android/InterstitialAd.zip";
    private static readonly string FileToCheckPathInterstitial = "Assets/LevelPlay/Runtime/Plugins/Android/InterstitialAd.java";
      private static readonly string SourceFilePathRewarded = "Assets/AppHarbrSDK/Plugins/Android/RewardedAd.zip";
      private static readonly string FileToCheckPathRewarded = "Assets/LevelPlay/Runtime/Plugins/Android/RewardedAd.java";

    static AppHarbrPostprocessor()
    {
        RemoveOldFile(OldAarPath);
        RemoveOldFile(OldAarPath + ".meta");

        RemoveOldFile(OldBridgeAarPath);
        RemoveOldFile(OldBridgeAarPath + ".meta");

        ReplaceLevelPlayIntegration();
    }

    private static void RemoveOldFile(string pathToRemove)
    {
        if (File.Exists(pathToRemove))
        {
          try
            {
//              Debug.Log($"[AppHarbr] Removing file: {pathToRemove}");
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
//            Debug.Log($"[AppHarbr] Target file exists: {FileToCheckPath}");

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
        else
        {
//            Debug.Log($"[AppHarbr] Target file does not exist: {FileToCheckPath}");
        }
    }
}