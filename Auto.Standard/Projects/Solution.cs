using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Auto.Projects
{
    public class Solution
    {
        public FileInfo      Path      { get; internal set; }
        public DirectoryInfo Directory => Path.Directory;
        public List<Project> Projects  { get; } = new List<Project>();
        public List<Folder>  Folders   { get; } = new List<Folder>();

        public void Parse(string filePath)
        {
            Projects.Clear();
            Folders.Clear();

            Path = new FileInfo(filePath);

            foreach(string line in File.ReadAllLines(filePath))
            {
                if(line.StartsWith("Project"))
                {
                    var sublines = line.Split(new[]
                        {
                            "Project(\"{",
                            "}\") = \"",
                            "\", \"",
                            "\", \"{",
                            "}\""
                        },
                        StringSplitOptions.RemoveEmptyEntries);

                    var folderId = sublines[0];
                    var name = sublines[1];
                    var file = sublines[2];
                    var id = sublines[3];

                    if(file.EndsWith(".csproj"))
                    {
                        Projects.Add(new Project()
                        {
                            RelativePath = file,
                            File = new FileInfo(System.IO.Path.Join(Path.Directory.FullName, file)),
                            FolderId = folderId,
                            Id = id,
                            Name = name
                        });
                    }
                    else
                    {
                        Folders.Add(new Folder()
                        {
                            FolderId = folderId,
                            Id = id,
                            Name = name
                        });
                    }
                }
            }
        }

        public Project GetProject(string name)
        {
            return Projects.FirstOrDefault(p => p.Name == name);
        }
    }
}