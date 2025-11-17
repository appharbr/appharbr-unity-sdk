using System.IO;
using UnityEditor;
using UnityEngine;

namespace AppHarbrSDK.Editor 
{
    [InitializeOnLoad]
    public class AppHarbrMigration
    {
        private const string LEGACY_SDK_PATH = "Assets/AppHarbrSDK";
        private const string LEGACY_SCRIPTS_PATH = "Assets/AppHarbrSDK/Scripts";
        private const string UPM_PACKAGE_PATH = "Packages/com.appharbr.sdk/package.json";
        private const string MIGRATION_FLAG_KEY = "AppHarbr.SDK.MigrationCompleted";
        private const string CLEANUP_FLAG_KEY = "AppHarbr.SDK.CleanupCompleted";

        // Old AAR files that should be removed
        private const string OLD_AAR_PATH = "Assets/AppHarbrSDK/Plugins/Android/AH-SDK-Android.aar";
        private const string OLD_BRIDGE_AAR_PATH = "Assets/AppHarbrSDK/Plugins/Android/appharbr-unity-mediations-plugin.aar";

        static AppHarbrMigration()
        {
            EditorApplication.delayCall += CheckAndMigrate;
        }

        private static void CheckAndMigrate()
        {
            bool upmExists = File.Exists(UPM_PACKAGE_PATH);
            bool manualExists = Directory.Exists(LEGACY_SDK_PATH);
            bool oldScriptsExist = Directory.Exists(LEGACY_SCRIPTS_PATH);
            bool oldAarsExist = File.Exists(OLD_AAR_PATH) || File.Exists(OLD_BRIDGE_AAR_PATH);

            // Scenario 1: Both UPM and manual - only check once
            if (upmExists && manualExists)
            {
                bool shouldMigrate = EditorUtility.DisplayDialog(
                    "AppHarbr SDK Migration",
                    "Both UPM and manual versions of AppHarbr SDK were detected.\n\n" +
                    "The UPM version is now active. Would you like to remove the old manual version from Assets/AppHarbrSDK?\n\n" +
                    "Note: This will delete the Assets/AppHarbrSDK folder.",
                    "Yes, Remove Old Version",
                    "No, Keep It"
                );

                if (shouldMigrate)
                {
                    RemoveLegacySDK();
                }
                else
                {
                    Debug.LogWarning("[AppHarbr] Legacy SDK folder kept. Please remove Assets/AppHarbrSDK manually to avoid conflicts with the UPM version.");
                }

                EditorPrefs.SetBool(MIGRATION_FLAG_KEY, true);
            }
            // Scenario 2: Manual install with old resources - automatic cleanup every time until clean
            else if (manualExists && (oldScriptsExist || oldAarsExist))
            {
                // Check if Runtime folder exists (indicating new structure)
                bool newStructureExists = Directory.Exists("Assets/AppHarbrSDK/Runtime");

                if (newStructureExists || oldAarsExist)
                {
                    // Automatic cleanup - runs every time until old files are gone
                    CleanupOldManualFiles(oldScriptsExist && newStructureExists, oldAarsExist);
                }
            }
            // Scenario 3: Clean state - mark as checked
            else
            {
                if (!EditorPrefs.GetBool(MIGRATION_FLAG_KEY, false))
                {
                    EditorPrefs.SetBool(MIGRATION_FLAG_KEY, true);
                }
            }
        }

        private static void RemoveLegacySDK()
        {
            try
            {
                FileUtil.DeleteFileOrDirectory(LEGACY_SDK_PATH);
                FileUtil.DeleteFileOrDirectory(LEGACY_SDK_PATH + ".meta");

                AssetDatabase.Refresh();

                Debug.Log("[AppHarbr] Successfully removed legacy SDK from Assets/AppHarbrSDK");

                EditorUtility.DisplayDialog(
                    "Migration Complete",
                    "Old AppHarbr SDK has been removed successfully.\n\n" +
                    "The SDK is now managed via Unity Package Manager.",
                    "OK"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AppHarbr] Failed to remove legacy SDK: {e.Message}");
                EditorUtility.DisplayDialog(
                    "Migration Error",
                    $"Failed to remove old SDK automatically.\n\nError: {e.Message}\n\n" +
                    "Please manually delete Assets/AppHarbrSDK folder.",
                    "OK"
                );
            }
        }

        private static void CleanupOldManualFiles(bool removeScripts, bool removeOldAars)
        {
            int filesRemoved = 0;

            try
            {
                // Remove old Scripts folder if needed
                if (removeScripts && Directory.Exists(LEGACY_SCRIPTS_PATH))
                {
                    FileUtil.DeleteFileOrDirectory(LEGACY_SCRIPTS_PATH);
                    FileUtil.DeleteFileOrDirectory(LEGACY_SCRIPTS_PATH + ".meta");
                    filesRemoved++;
                    Debug.Log("[AppHarbr] Removed old Scripts folder");
                }

                // Remove old AAR files if needed
                if (removeOldAars)
                {
                    if (File.Exists(OLD_AAR_PATH))
                    {
                        FileUtil.DeleteFileOrDirectory(OLD_AAR_PATH);
                        FileUtil.DeleteFileOrDirectory(OLD_AAR_PATH + ".meta");
                        filesRemoved++;
                        Debug.Log("[AppHarbr] Removed old AH-SDK-Android.aar");
                    }

                    if (File.Exists(OLD_BRIDGE_AAR_PATH))
                    {
                        FileUtil.DeleteFileOrDirectory(OLD_BRIDGE_AAR_PATH);
                        FileUtil.DeleteFileOrDirectory(OLD_BRIDGE_AAR_PATH + ".meta");
                        filesRemoved++;
                        Debug.Log("[AppHarbr] Removed old appharbr-unity-mediations-plugin.aar");
                    }
                }

                if (filesRemoved > 0)
                {
                    AssetDatabase.Refresh();
                    Debug.Log($"[AppHarbr] Successfully cleaned up {filesRemoved} old file(s)");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AppHarbr] Failed to cleanup old files: {e.Message}");
            }
        }
    }
}