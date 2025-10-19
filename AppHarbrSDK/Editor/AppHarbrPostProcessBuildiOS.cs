#if UNITY_IOS || UNITY_IPHONE

#if UNITY_2019_3_OR_NEWER
using UnityEditor.iOS.Xcode.Extensions;
#endif
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace AppHarbrSDK.Scripts.Editor
{
    public class iOSBuildProcessor
    {
#if !UNITY_2019_3_OR_NEWER
        private const string MainTargetName = "Unity-iPhone";
#endif

        // Fix "Cycle inside Unity-iPhone; building could produce unreliable results" build error because of Crashlytics Run Script
        private const int embedFrameworksPriority = 92;

		private const string TargetUnityIphonePodfileLine = "target 'Unity-iPhone' do";
        private const string UseFrameworksPodfileLine = "use_frameworks!";
        private const string UseFrameworksDynamicPodfileLine = "use_frameworks! :linkage => :dynamic";
        private const string UseFrameworksStaticPodfileLine = "use_frameworks! :linkage => :static";
        private static readonly List<string> LibrariesToEmbed = new List<string>
        {
            "AppHarbrSDK.xcframework"
        };

        [PostProcessBuildAttribute(embedFrameworksPriority)]
        public static void ProcessXcodeProject(BuildTarget buildTarget, string buildPath)
        {
            var projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

#if UNITY_2019_3_OR_NEWER
            var mainTargetGuid = pbxProject.GetUnityMainTargetGuid();
            var frameworkTargetGuid = pbxProject.GetUnityFrameworkTargetGuid();
#else
            var mainTargetGuid = pbxProject.TargetGuidByName(MainTargetName);
            var frameworkTargetGuid = pbxProject.TargetGuidByName(MainTargetName);
#endif
            EmbedLibrariesIfNeeded(buildPath, pbxProject, mainTargetGuid);

            AddSwiftSupportIfNeeded(buildPath, pbxProject, frameworkTargetGuid);
            EmbedSwiftStandardLibrariesIfNeeded(buildPath, pbxProject, mainTargetGuid);

            pbxProject.WriteToFile(projectPath);
        }

        private static void EmbedLibrariesIfNeeded(string buildPath, PBXProject pbxProject, string targetGuid)
        {
            // Check if the Pods directory exists
            var podsDirectory = Path.Combine(buildPath, "Pods");
            if (!Directory.Exists(podsDirectory)  || !ShouldEmbedDynamicLibraries(buildPath)) return;

            var libraryPathsInProject = new List<string>();
            foreach (var libraryToSearch in LibrariesToEmbed)
            {
                var directories = Directory.GetDirectories(podsDirectory, libraryToSearch, SearchOption.AllDirectories);
                if (directories.Length <= 0) continue;

                var libraryAbsolutePath = directories[0];
                var index = libraryAbsolutePath.LastIndexOf("Pods");
                var relativePath = libraryAbsolutePath.Substring(index);

                if (!IsLibraryAlreadyEmbedded(pbxProject, targetGuid, relativePath))
                {
                    libraryPathsInProject.Add(relativePath);
                }
            }

            if (libraryPathsInProject.Count <= 0) return;

#if UNITY_2019_3_OR_NEWER
            foreach (var libraryPath in libraryPathsInProject)
            {
                var fileGuid = pbxProject.AddFile(libraryPath, libraryPath);
                pbxProject.AddFileToEmbedFrameworks(targetGuid, fileGuid);
            }
#else
            string runpathSearchPaths;
#if UNITY_2018_2_OR_NEWER
            runpathSearchPaths = pbxProject.GetBuildPropertyForAnyConfig(targetGuid, "LD_RUNPATH_SEARCH_PATHS");
#else
            runpathSearchPaths = "$(inherited)";          
#endif
            runpathSearchPaths += string.IsNullOrEmpty(runpathSearchPaths) ? "" : " ";

            if (runpathSearchPaths.Contains("@executable_path/Frameworks")) return;

            runpathSearchPaths += "@executable_path/Frameworks";
            pbxProject.SetBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", runpathSearchPaths);
#endif
        }

        private static bool IsLibraryAlreadyEmbedded(PBXProject pbxProject, string targetGuid, string libraryPath)
        {
            var embeddedLibraries = pbxProject.GetFrameworksBuildPhaseByTarget(targetGuid);
            return pbxProject.ContainsFileByProjectPath(libraryPath);
        }

        private static bool ShouldEmbedDynamicLibraries(string buildPath)
        {
            var podfilePath = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(podfilePath)) return false;

            var lines = File.ReadAllLines(podfilePath);
            var containsUnityIphoneTarget = lines.Any(line => line.Contains(TargetUnityIphonePodfileLine));
            if (!containsUnityIphoneTarget) return true;

            var useFrameworksStaticLineIndex = Array.FindIndex(lines, line => line.Contains(UseFrameworksStaticPodfileLine));
            if (useFrameworksStaticLineIndex == -1) return false;

            var useFrameworksLineIndex = Array.FindIndex(lines, line => line.Trim() == UseFrameworksPodfileLine);
            var useFrameworksDynamicLineIndex = Array.FindIndex(lines, line => line.Contains(UseFrameworksDynamicPodfileLine));

            return useFrameworksLineIndex < useFrameworksStaticLineIndex && useFrameworksDynamicLineIndex < useFrameworksStaticLineIndex;
        }

        private static void AddSwiftSupportIfNeeded(string buildPath, PBXProject pbxProject, string targetGuid)
        {
            var swiftFileRelativePath = "Classes/AppHarbrSwiftSupport.swift";
            var swiftFilePath = Path.Combine(buildPath, swiftFileRelativePath);

            CreateSwiftFileIfNeeded(swiftFilePath);
            var swiftFileGuid = pbxProject.AddFile(swiftFileRelativePath, swiftFileRelativePath, PBXSourceTree.Source);
            pbxProject.AddFileToBuild(targetGuid, swiftFileGuid);

#if UNITY_2018_2_OR_NEWER
            var swiftVersion = pbxProject.GetBuildPropertyForAnyConfig(targetGuid, "SWIFT_VERSION");
#else
            const string swiftVersion = "";
#endif
            if (string.IsNullOrEmpty(swiftVersion))
            {
                pbxProject.SetBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            }

            // Enable Swift modules
            pbxProject.AddBuildProperty(targetGuid, "CLANG_ENABLE_MODULES", "YES");
        }

        private static void EmbedSwiftStandardLibrariesIfNeeded(string buildPath, PBXProject pbxProject, string mainTargetGuid)
        {
            // This needs to be added to the main target, not UnityFramework.
            pbxProject.AddBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
        }

        private static void CreateSwiftFileIfNeeded(string swiftFilePath)
        {
            if (File.Exists(swiftFilePath)) return;

            // Create a file to write to.
            using (var writer = File.CreateText(swiftFilePath))
            {
                writer.WriteLine("//\n//  AppHarbrSwiftSupport.swift\n//");
                writer.WriteLine("\nimport Foundation\n");
                writer.WriteLine("// This file ensures the project includes Swift support.");
                writer.WriteLine("// It is automatically generated by the AppHarbr Unity Plugin.");
                writer.Close();
            }
        }

    }
}

#endif
