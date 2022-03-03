using System.IO;

namespace Auto.Projects
{
    public class Project
    {
        public string        Id           { get; internal set; }
        public string        FolderId     { get; internal set; }
        public string        Name         { get; internal set; }
        public string        RelativePath { get; internal set; }
        public FileInfo      File         { get; internal set; }
        public DirectoryInfo Directory    => File.Directory;
    }

    public class Folder
    {
        public string Id       { get; internal set; }
        public string FolderId { get; internal set; }
        public string Name     { get; internal set; }
    }
}