using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoLauncher.Shared
{
    public class BuilderArguments
    {
        public AutoBuildTarget AutoBuildTarget;

        public string     Version;
        public int        BuildNumber;
        public string     BundleId;
        public string     BundleName;
        public BundleType BundleType;
        public Backend    Backend;

//        keyStoreName - full path to the key store you want to use
//        keyStorePass - password for the keystore
//        keyaliasName - name of the key that you want to use
//        keyaliasPass - password for the key you want to use (if any)

        public string KeystorePath;
        public string KeystorePass;
        public string KeyAliasName;
        public string KeyAliasPass;

        public void FromArgs(string[] args)
        {
            var props = this.GetType().GetRuntimeFields();
            for(int i = 0; i < args.Length; i++)
            {
                if(args[i].StartsWith("-"))
                {
                    var name = args[i].Substring(1);
                    var prop = props.FirstOrDefault(p =>
                        String.Compare(p.Name, name, StringComparison.OrdinalIgnoreCase) == 0);

                    if(prop != null)
                    {
                        var str = args[i + 1];
                        object value = str;

                        if(prop.FieldType == typeof(int))
                        {
                            value = int.Parse(str);
                        }

                        if(prop.FieldType == typeof(float))
                        {
                            value = float.Parse(str);
                        }

                        if(prop.FieldType.IsEnum)
                        {
                            value = Enum.Parse(prop.FieldType, str);
                        }

                        prop.SetValue(this, value);
                    }

                    i++;
                }
            }
        }

        public string[] ToArgs()
        {
            //%UNITY% -buildTarget Android -batchmode -nographics -quit -logFile
            var list = new List<string>()
                       {
                           "-batchmode",
                           "-nographics",
                           "-logFile",
                           "-",
                           "-executeMethod",
                           "AutoBuilder.BuildFromArgs"
                       };

            var props = this.GetType().GetRuntimeFields();
            foreach(var prop in props)
            {
                var value = prop.GetValue(this);
                if(ReferenceEquals(value, null)) continue;

                list.Add("-" + prop.Name);
                list.Add(value.ToString());
            }

            return list.ToArray();
        }
    }

    public enum AutoBuildTarget
    {
        Android,
        Ios,
        WebGl,
    }

    public enum BundleType
    {
        None,
        AabBundle,
        Apk,
    }

    public enum Backend
    {
        Default,
        Mono,
        Il2Cpp
    }

    class GetUnityEditorPath
    {
        public int    Time   { get; set; }
        public string Return { get; set; }
    }
}