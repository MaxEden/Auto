using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace Auto
{
    public static class Remote
    {
        public static void DeployFolderViaSftp(Logger logger,
                                               Login login,
                                               DirectoryInfo localDir,
                                               string remoteDir,
                                               string user,
                                               DirectoryInfo outputDirectory)
        {
            var localDirInfo = localDir;
            if(!localDirInfo.Exists) throw new ArgumentException($"localDir: {localDir} doesn't exist!");

            var name = localDirInfo.Name;
            var zipFileName = "SftpDeploy_" + name + ".zip";
            var zipFile = outputDirectory.S(zipFileName);
            File.Delete(zipFile);

            ZipFile.CreateFromDirectory(localDir.FullName, zipFile);
            //CompressionTasks.Compress(localDir, zipFile);

            var remoteDirInfo = new DirectoryInfo(remoteDir);
            var remoteDirName = remoteDirInfo.Name;

            Ssh(logger,
                login,
                new[]
                {
                    $"cd {remoteDir}",
                    //$"ls",
                    $"rm -rf {remoteDir}",
                    $"mkdir {remoteDir}",
                });

            SftpCopy(logger, login, zipFile, remoteDir);

            Ssh(logger,
                login,
                new[]
                {
                    $"cd {remoteDir}",
                    $"unzip -o {zipFileName}",
                    $"rm {zipFileName}",

                    $"cd {remoteDir}/..",
                    $"chown -R {user} {remoteDirName}",
                    $"chmod -R 775 {remoteDirName}",

                    $"cd {remoteDir}",
                    //$"ls",
                });
        }

        public static void Ssh(Logger logger, Login login, string[] commands)
        {
            if(commands.Last() != "exit") commands = commands.Concat(new[] { "exit" }).ToArray();
            CLI.RunCli(logger, "ssh", $"-tt {login.User}@{login.Ip}", commands);
        }

        public static void SftpCopy(Logger logger, Login login, string local, string remote)
        {
            local = local.Replace('\\', '/');

            Sftp(logger,
                login,
                new[]
                {
                    $"cd {remote}",
                    $"put {local}"
                });
        }

        public static void Sftp(Logger logger, Login login, string[] commands)
        {
            if(commands.Last() != "bye") commands = commands.Concat(new[] { "bye" }).ToArray();
            CLI.RunCli(logger, "sftp", $"{login.User}@{login.Ip}", commands);
        }

        public class Login
        {
            public readonly string Ip;
            public readonly string User;
            public readonly string PrivateKey;

            public Login(string user, string ip, string privatePrivateKey = null)
            {
                User = user;
                Ip = ip;
                IPAddress.Parse(ip);
                PrivateKey = privatePrivateKey;
            }
        }
    }
}