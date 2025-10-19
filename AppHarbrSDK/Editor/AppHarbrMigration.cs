using System.IO;
using UnityEditor;
using UnityEngine;

namespace AppHarbr.Editor
{
    [InitializeOnLoad]
    public class AppHarbrMigration
    {
        private const string LEGACY_SDK_PATH = "Assets/AppHarbrSDK";
        private const string MIGRATION_FLAG_KEY = "AppHarbr.SDK.MigrationCompleted";

        static AppHarbrMigration()
        {
            EditorApplication.delayCall += CheckAndMigrate;
        }

        private static void CheckAndMigrate()
        {
            // Only run once per project
            if (EditorPrefs.GetBool(MIGRATION_FLAG_KEY, false))
            {
                return;
            }

            // Check if legacy SDK folder exists
            if (Directory.Exists(LEGACY_SDK_PATH))
            {
                bool shouldMigrate = EditorUtility.DisplayDialog(
                    "AppHarbr SDK Migration",
                    "An old version of AppHarbr SDK was detected in Assets/AppHarbrSDK.\n\n" +
                    "The SDK is now managed via Unity Package Manager. Would you like to remove the old version?\n\n" +
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
                    Debug.LogWarning("[AppHarbr] Legacy SDK folder kept. Please remove Assets/AppHarbrSDK manually to avoid conflicts.");
                }

                // Mark migration as completed
                EditorPrefs.SetBool(MIGRATION_FLAG_KEY, true);
            }
            else
            {
                // No legacy SDK found, mark as completed
                EditorPrefs.SetBool(MIGRATION_FLAG_KEY, true);
            }
        }

        private static void RemoveLegacySDK()
        {
            try
            {
                // Delete the directory
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
    }
}