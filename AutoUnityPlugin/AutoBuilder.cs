using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoLauncher.Shared;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AutoLauncher.UnityPlugin
{
    public static class AutoBuilder
    {
        public static BuilderArguments ReadCommandLineArgs()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            Log("UNITY RUN ARGUMENTS :");
            foreach(string arg in args)
            {
                Log(arg);
            }

            var bargs = new BuilderArguments();
            bargs.FromArgs(args);

            ApplyParams(bargs);
            return bargs;
        }

        private static void Log(string msg)
        {
            if(Application.isBatchMode)
            {
                Console.WriteLine(msg);
            }
            else
            {
                Debug.Log(msg);
            }
        }

        private static void Error(string msg)
        {
            if(Application.isBatchMode)
            {
                Console.WriteLine(msg);
            }
            else
            {
                Debug.LogError(msg);
            }
        }

        [MenuItem("Auto/Builder/BuildFromArgs")]
        public static void BuildFromArgs()
        {
            var bargs = ReadCommandLineArgs();
            Build(bargs);
        }

        public static void Build(BuilderArguments args)
        {
            var target = BuildTarget.Android;
            switch(args.AutoBuildTarget)
            {
                case AutoBuildTarget.Android:
                    target = BuildTarget.Android;

                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
                    switch(args.Backend)
                    {
                        case Backend.Default:
                            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
                                ScriptingImplementation.Mono2x);
                            break;
                        case Backend.Mono:
                            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
                                ScriptingImplementation.Mono2x);
                            break;
                        case Backend.Il2Cpp:
                            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
                                ScriptingImplementation.IL2CPP);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                case AutoBuildTarget.Ios:
                    target = BuildTarget.iOS;
                    break;
                case AutoBuildTarget.WebGl:
                    target = BuildTarget.WebGL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Log("");
            Log("");
            Log($"BUILD METHOD CALLED=============================================================");
            Log($"FOR TARGET:{target}============================================================");
            Log("");
            Log("");

            AssetDatabase.Refresh();
            var scenes = GetScenes();

            string buildPath = GetBuildPath(args);

            var report = BuildPipeline.BuildPlayer(scenes, buildPath, target, BuildOptions.None);
            var summary = report.summary;

            if(summary.result == BuildResult.Succeeded)
            {
                Log("Build succeeded: " + summary.totalSize + " bytes");
                Log("Output: " + buildPath);
                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }

            if(summary.result == BuildResult.Failed)
            {
                Error("Build failed");

                if(Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static string GetBuildPath(BuilderArguments args)
        {
            string filename = "build";
            var buildPath = "../Out/" + args.AutoBuildTarget;
            if(args.AutoBuildTarget == AutoBuildTarget.Android)
            {
                if(args.BundleType == BundleType.AabBundle)
                {
                    filename += ".aab";
                }
                else
                {
                    filename += ".apk";
                }
            }

//        if(target == BuildTarget.WebGL)
//        {
//            if(PlayerSettings.productName.IsValid())
//            {
//                filename = PlayerSettings.productName.Slug();
//            }
//        }

            if(args.AutoBuildTarget == AutoBuildTarget.Android)
            {
                Directory.CreateDirectory(buildPath);
                Log("Directory created ::: " + buildPath);
            }

            buildPath += "/" + filename;
            buildPath = Path.GetFullPath(buildPath);

            if(args.AutoBuildTarget != AutoBuildTarget.Android)
            {
                Directory.CreateDirectory(buildPath);
                Log("Directory created ::: " + buildPath);
            }


            return buildPath;
        }

        private static string[] GetScenes()
        {
            var scenes = new List<string>();
            scenes.AddRange(new DirectoryInfo("Assets").GetFiles("*.unity").Select(p => p.FullName));
            scenes.AddRange(new DirectoryInfo("Assets/Scenes").GetFiles("*.unity").Select(p => p.FullName));
            return scenes.ToArray();
        }

        [MenuItem("Auto/Builder/DevBuild")]
        public static void DevelopmentBuild()
        {
            SetDefaultPlayerSettingsToWin();
            string[] scenes = GetScenes();

            BuildPipeline.BuildPlayer(scenes,
                "builds\\build.exe",
                BuildTarget.StandaloneWindows,
                BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler);
        }

        [MenuItem("Auto/Builder/Standalone")]
        public static void StandaloneBuild()
        {
            SetDefaultPlayerSettingsToWin();
            string[] scenes = GetScenes();

            BuildPipeline.BuildPlayer(scenes,
                "builds\\build.exe",
                BuildTarget.StandaloneWindows,
                BuildOptions.Development);
        }

        private static void SetDefaultPlayerSettingsToWin()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            //PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.allowFullscreenSwitch = false;
        }

        private static bool IsValid(this string str)
        {
            return !String.IsNullOrWhiteSpace(str);
        }

        private static string Slug(this string str)
        {
            var tmp = string.Concat(str.Split(Path.GetInvalidFileNameChars())).ToList();
            tmp.RemoveAll(p => char.IsWhiteSpace(p));
            return new string(tmp.ToArray());
        }

        private static void ApplyParams(BuilderArguments args)
        {
            PlayerSettings.bundleVersion = args.Version;

            if(args.BundleName.IsValid())
            {
                PlayerSettings.productName = args.BundleName;
            }

            if(!args.BundleId.IsValid() && PlayerSettings.productName.IsValid() && PlayerSettings.companyName.IsValid())
            {
                args.BundleId = $"com.{PlayerSettings.productName.Slug()}.{PlayerSettings.companyName.Slug()}";
            }

            if(args.BundleId.IsValid())
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, args.BundleId);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, args.BundleId);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, args.BundleId);
            }

            if(args.BuildNumber != 0)
            {
                PlayerSettings.iOS.buildNumber = args.BuildNumber.ToString();
                PlayerSettings.macOS.buildNumber = args.BuildNumber.ToString();
                PlayerSettings.Android.bundleVersionCode = args.BuildNumber;
            }
            else
            {
                if(int.TryParse(PlayerSettings.iOS.buildNumber, out int iosNumber))
                {
                    iosNumber++;
                    PlayerSettings.iOS.buildNumber = iosNumber.ToString();
                }

                if(int.TryParse(PlayerSettings.macOS.buildNumber, out int macOSNumber))
                {
                    macOSNumber++;
                    PlayerSettings.macOS.buildNumber = macOSNumber.ToString();
                }

                PlayerSettings.Android.bundleVersionCode++;
            }

            if(args.KeystorePath.IsValid()) PlayerSettings.Android.keystoreName = args.KeystorePath;
            if(args.KeystorePass.IsValid()) PlayerSettings.Android.keystorePass = args.KeystorePass;
            if(args.KeyAliasName.IsValid()) PlayerSettings.Android.keyaliasName = args.KeyAliasName;
            if(args.KeyAliasPass.IsValid()) PlayerSettings.Android.keyaliasPass = args.KeyAliasPass;

            if(args.BundleType == BundleType.AabBundle) EditorUserBuildSettings.buildAppBundle = true;
        }
    }
}