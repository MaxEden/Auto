using AutoLauncher.Shared;
using AutoLauncher.UnityPlugin;
using UnityEditor;

namespace MandarinAuto.UnityPlugin
{
    public static class Build
    {
        [MenuItem("Auto/Builder/Android/il2cpp")]
        public static void AndroidIl2Cpp() => AutoBuilder.Build(new BuilderArguments()
        {
            AutoBuildTarget = AutoBuildTarget.Android,
            Backend = Backend.Il2Cpp
        });

        [MenuItem("Auto/Builder/Android/mono")]
        public static void AndroidMono() => AutoBuilder.Build(new BuilderArguments()
        {
            AutoBuildTarget = AutoBuildTarget.Android,
            Backend = Backend.Mono
        });

        [MenuItem("Auto/Builder/iOS")]
        public static void Ios() => AutoBuilder.Build(new BuilderArguments()
        {
            AutoBuildTarget = AutoBuildTarget.Ios,
        });

        [MenuItem("Auto/Builder/WebGL")]
        public static void WebGL() => AutoBuilder.Build(new BuilderArguments()
        {
            AutoBuildTarget = AutoBuildTarget.WebGl,
        });
    }
}