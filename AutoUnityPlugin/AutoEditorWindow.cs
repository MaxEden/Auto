using AutoUnityPlugin;
using MandarinAuto.UnityPlugin;
using UnityEditor;

namespace AutoLauncher.UnityPlugin
{
    public class AutoEditorWindow : EditorWindow
    {
        [MenuItem("Auto/Open")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            AutoEditorWindow window = (AutoEditorWindow)EditorWindow.GetWindow(typeof(AutoEditorWindow));
            window.Show();
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Auto status:", MandarinAuto.UnityPlugin.UnityPlugin.Instance.AutoIsConnected ? "Active" : "Stopped");
            Toggle(MandarinAuto.UnityPlugin.UnityPlugin.EnableHiLogs);
        }

        private void Toggle(SProp<bool> sprop)
        {
            sprop.Value = EditorGUILayout.Toggle(sprop.Name, sprop.Value);
        }

        private void Label(SProp sprop)
        {
            EditorGUILayout.LabelField(sprop.Name, sprop.ValueObj.ToString());
        }
    }
}