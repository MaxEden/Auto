using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Hi;
using AutoUnityPlugin;
using Mini;
using UnityEditor;
using UnityEditor.Compilation;
using Debug = UnityEngine.Debug;

namespace MandarinAuto.UnityPlugin
{
    [InitializeOnLoad]
    public class UnityPlugin
    {
        public static UnityPlugin Instance { get; }

        public bool AutoIsConnected => AutoClient.IsConnected;

        public static SProp<bool> EnableHiLogs = new SProp<bool>(nameof(EnableHiLogs));
        public static SProp<bool> EditorState  = new SProp<bool>(nameof(EditorState));
        public static SProp<bool> CanProcess   = new SProp<bool>(nameof(CanProcess));

        private double? _startedTime;
        private bool    _started;
        private bool    _sceneLoaded;
        private int     _turn;


        private Mini.Client AutoClient;
        private Mini.Client MHostClient;
        private SProp<bool> _enableHiLogs;

        [MenuItem("Auto/StartServices")]
        private static void MenuStartServices()
        {
            Instance.StartServices();
        }

        [MenuItem("Auto/RestoreDlls")]
        private static void BeforeCompile()
        {
            Debug.Log("BeforeCompile");
            //Instance.MHostClient.Send(Commands.ToCommand("stop", "ServerLib"));
            Instance.AutoClient.Send("unitybefore ", EditorApplication.applicationPath);
        }

        [MenuItem("Auto/Process")]
        private static void AfterCompile()
        {
            Debug.Log("AfterCompile");
            //Instance.MHostClient.Send(Commands.ToCommand("add", "ServerLib"));
            Instance.AutoClient.Send("unityafter");
        }

        static UnityPlugin()
        {
            Instance = new UnityPlugin();
            Instance.Subscribe();
        }

        public void Start()
        {
            _enableHiLogs = EnableHiLogs;

            Debug.Log("init UnityPlugin");

            AssetDatabase.DisallowAutoRefresh();


            AutoClient = new Mini.Client("Auto", AutoLog, Receive, "UnityPlugin");
            MHostClient = new Mini.Client("MandarinHost", MHostLog, Receive, "UnityPlugin");

            if(FirstTime("EditorStart"))
            {
                EditorStarted();
            }

            AutoClient.Connect();

            CanProcess.Value = true;
            EditorApplication.isPlaying = EditorState.Value;
        }

        private void AutoLog(string obj)
        {
            if(_enableHiLogs)
            {
                Debug.Log(obj);
            }
        }

        private void MHostLog(string obj)
        {
            if(_enableHiLogs)
            {
                Debug.Log(obj);
            }
        }

        private void EditorStarted()
        {
            Debug.Log("============Editor Started=============");
            EditorState.Value = false;
            Instance.StartServices();

            Debug.Log("==============first processing==========");
            CanProcess.Value = true;
            CompilationPipeline.RequestScriptCompilation();
            Recompile();
        }

        private static bool FirstTime(string key)
        {
            var processId = Process.GetCurrentProcess().Id;
            var lastProcessId = EditorPrefs.GetInt(key, 0);
            if(lastProcessId != processId)
            {
                EditorPrefs.SetInt(key, processId);
                return true;
            }

            return false;
        }

        private void EditorApplicationOnHierarchyChanged()
        {
            //Debug.Log("scene loaded");
            _sceneLoaded = true;
        }

        private void Subscribe()
        {
            Debug.Log("Subscribe");
            EditorApplication.update += EditorUpdate;
            CompilationPipeline.compilationStarted += CompilationPipelineOnCompilationStarted;
            CompilationPipeline.compilationFinished += CompilationPipelineOnCompilationFinished;
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
            EditorApplication.hierarchyChanged += EditorApplicationOnHierarchyChanged;
            CanProcess.Value = false;
        }

        private void CompilationPipelineOnCompilationStarted(object obj)
        {
            if(obj as string == "Editor Compilation") return;
            if(CanProcess.Value)
            {
                BeforeCompile();
            }
        }

        private void CompilationPipelineOnCompilationFinished(object obj)
        {
            if(obj as string == "Editor Compilation") return;
            if(CanProcess.Value)
            {
                AfterCompile();
            }
        }

        private void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
        {
            if(obj == PlayModeStateChange.ExitingEditMode)
            {
                EditorState.Value = true;
            }

            if(obj == PlayModeStateChange.ExitingPlayMode)
            {
                EditorState.Value = false;
            }
        }

        private void StartServices()
        {
            Debug.Log("StartServices");
            AutoClient.OnStart = () =>
            {
                Debug.Log("OnAutoStart");
                var path = Path.GetFullPath(Directory.GetFiles("..", "_Auto.exe", SearchOption.AllDirectories)
                    .First());
                AutoClient.Send("add " + path, 20);
                AutoClient.Send("unitystart", 20);
            };

            AutoClient.EnsureRunning();
            MHostClient.EnsureRunning();

            Recompile();
        }

        private static void EditorUpdate()
        {
            Instance?.Update();
        }


        private void Update()
        {
            //if(!_sceneLoaded) return;

            if(!_startedTime.HasValue)
            {
                _startedTime = EditorApplication.timeSinceStartup;
                return;
            }
            else
            {
                if(_turn < 3)
                {
                    Debug.Log(new string('.', _turn));
                    _turn++;
                    return;
                }
            }

            if(!_started)
            {
                _started = true;
                Instance.Start();
            }

            AutoClient.Update();
            MHostClient.Update();
        }

        private Msg Receive(Msg msg, Sender sender)
        {
            var untyped = Commands.ParseUntyped(msg.Text);
            Debug.Log("Recieved " + msg);
            if(untyped.Name == "CompilerService|asset-database-refresh")
            {
                Recompile();
            }

            return "Ok";
        }

        private void Recompile()
        {
            Debug.Log("Recompile");
            EditorApplication.isPlaying = false;
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            //_client.Disconnect();
        }
    }
}