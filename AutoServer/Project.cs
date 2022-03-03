using System;
using System.IO;
using Auto;
using AutoLauncher.Standard;
using McMaster.NETCore.Plugins;

namespace AutoServer
{
    public class Project
    {
        public string            DllPath            { get; set; }
        public PluginLoader      Loader             { get; set; }
        public ProjState         State              { get; set; }
        public Context           Context            { get; set; }
        public string            ProjectFile        { get; set; }
        public string            ProjectFolder      { get; set; }
        public string[]          Scripts            { get; set; }
        public string            Name               { get; set; }
        public FileSystemWatcher Watcher            { get; set; }
        public bool              NeedsRecompilation { get; set; }
        public DateTime          CheckTime          { get; set; }

        public Action<Project> OnReload;

        public void ReloadedHook(object sender, PluginReloadedEventArgs eventargs)
        {
            OnReload?.Invoke(this);
        }

        public enum ProjState
        {
            None,
            Waiting,
            Valid,
            Invalid
        }

        public void ScriptChanged(object sender, FileSystemEventArgs e)
        {
            NeedsRecompilation = true;
            CheckTime = DateTime.Now;
        }
    }
}