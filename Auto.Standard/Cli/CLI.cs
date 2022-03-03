using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Auto
{
    public static class CLI
    {
        public static void Run(Logger logger, string command, params string[] args)
        {
            RunImpl(logger, command, args, null, false);
        }

        public static List<OutputLine> RunAndRead(Logger logger, string command, params string[] args)
        {
            List<OutputLine> output = new List<OutputLine>();
            RunImpl(logger, command, args, output, false);
            return output;
        }

        private static void RunImpl(Logger logger, string command, string[] args, ICollection<OutputLine> collection, bool admin)
        {
            var argsString = GetArgsString(args);

            logger?.Info?.Invoke("RUN:" + command + " " + argsString);

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = command;
            startInfo.Arguments = argsString;
            if(admin) startInfo.Verb = "runas";

            var process = new Process();
            GetOutput(logger, startInfo, process, collection);
            process.WaitForExit();
        }

        public static string GetArgsString(params string[] args)
        {
            var args1 = args.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            for(int i = 0; i < args1.Count; i++)
            {
                if(args1[i].Any(p => Char.IsWhiteSpace(p)))
                {
                    args1[i] = "\"" + args1[i] + "\"";
                }
            }

            var argsString = String.Join(' ', args1);
            return argsString;
        }

        public static void RunShell(Logger logger, bool admin, string command, params string[] args)
        {
            var argsString = GetArgsString(args);
            var script = command + " " + argsString;
            logger.Info?.Invoke("RUN:" + command + " " + argsString);

            script = script.Trim().Replace("\n", "&");

            if(IsUnix)
            {
                RunImpl(null,
                    @"/bin/bash",
                    new string[] { "-c '" + script + "'" },
                    null,
                    admin);
            }
            else
            {
                RunImpl( // + "&exit"},
                    null,
                    @"cmd.exe",
                    new string[] { "/C " + script },
                    null,
                    admin);
            }
        }

        public static Process RunInBackground(Logger logger, string command, params string[] args)
        {
            var argsString = GetArgsString(args);
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = argsString,
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Minimized,
                ErrorDialog = true,
            };
            logger.Info?.Invoke("RUN:" + command + " " + argsString);
            var process = Process.Start(startInfo);
            logger.Info?.Invoke("Started PID:" + process.Id);
            return process;
        }

        public static void RunCli(Logger logger, string tool, string firstArg, params string[] args)
        {
            logger.Info?.Invoke("RUN:" + tool + " " + firstArg + ";\nTHEN:\n" + String.Join("| ", args));

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = tool,
                Arguments = firstArg,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };

            Process process = new Process();
            GetOutput(logger, startInfo, process, null);

            foreach(var arg in args)
            {
                process.StandardInput.WriteLine(arg);
            }

            process.WaitForExit();
            if(process.ExitCode != 0)
            {
                logger.Error?.Invoke($"CLI {tool} exited with code:{process.ExitCode}");
            }
        }

        private static bool IsUnix
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static void MakeSymlink(Logger logger, UnknownPath targetFrom, UnknownPath linkTo, bool replace = false)
        {
            if(replace) RemoveSymlink(linkTo);

            var isFile = File.Exists(targetFrom);
            if(Directory.Exists(linkTo) || File.Exists(linkTo)) return;

            if(targetFrom.FullName.Contains(linkTo) || linkTo.FullName.Contains(targetFrom)) return;

            if(IsUnix)
            {
                RunShell(logger, false, "ln -s", targetFrom, linkTo);
            }
            else
            {
                if(isFile)
                {
                    RunShell(logger, true, "MKLINK", linkTo, targetFrom);
                }
                else
                {
                    //RunShell(true, "MKLINK /D", linkTo, targetFrom);
                    RunShell(logger, true, "MKLINK /J", linkTo, targetFrom);
                }
            }
        }

        public static void RemoveSymlink(UnknownPath linkTo)
        {
            var isFile = File.Exists(linkTo.FullName);
            if(!Directory.Exists(linkTo.FullName) && !File.Exists(linkTo.FullName)) return;
            if(isFile)
            {
                File.Delete(linkTo);
            }
            else
            {
                Directory.Delete(linkTo);
            }
        }

        private static void GetOutput(Logger logger,
                                      ProcessStartInfo startInfo,
                                      Process process,
                                      ICollection<OutputLine> collection)
        {
            bool redirect = logger != null || collection != null;
            if(!redirect)
            {
                process.StartInfo = startInfo;
                process.Start();
                return;
            }

            process.OutputDataReceived += (DataReceivedEventHandler)((s, e) =>
            {
                if(e.Data != null)
                {
                    collection?.Add(new OutputLine()
                    {
                        Text = e.Data,
                        Type = OutputType.Std
                    });
                    logger?.Info?.Invoke(e.Data);
                }
            });
            process.ErrorDataReceived += (DataReceivedEventHandler)((s, e) =>
            {
                if(e.Data != null)
                {
                    collection?.Add(new OutputLine()
                    {
                        Text = e.Data,
                        Type = OutputType.Err
                    });
                    logger?.Error?.Invoke(e.Data);
                }
            });

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;

            process.StartInfo = startInfo;
            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        public struct OutputLine
        {
            public OutputType Type;
            public string     Text;
        }

        public enum OutputType
        {
            Std,
            Err,
        }
    }
}