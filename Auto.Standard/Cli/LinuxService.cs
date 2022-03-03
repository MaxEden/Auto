using System;
using System.IO;
using System.Linq;

namespace Auto
{
    public class LinuxService
    {
        public string        ServiceName        { get; }
        public FileInfo      ServiceOutputPath  { get; }
        public DirectoryInfo TemporaryDirectory { get; }
        public DirectoryInfo OutputDirectory    { get; }

        public Remote.Login ServerMachineLogin { get; }
        public string       ServiceLinuxUser   => "dotnetuser";

        public Logger Logger { get; }

        public LinuxService(Logger logger,
                            Remote.Login serverLogin,
                            string serviceName,
                            FileInfo serviceOutputPath,
                            DirectoryInfo temporaryDirectory,
                            DirectoryInfo outputDirectory)
        {
            ServerMachineLogin = serverLogin;
            ServiceName = serviceName;
            ServiceOutputPath = serviceOutputPath;
            TemporaryDirectory = temporaryDirectory;
            OutputDirectory = outputDirectory;
            Logger = logger;
        }

        public void Install()
        {
            var login = ServerMachineLogin;
            var serviceName = ServiceName;

            var dllName = ServiceOutputPath.Name;
            var serviceDir = $"/var/{serviceName}";
            var dotnetPath = "/usr/share/dotnet/dotnet";
            var serviceUser = ServiceLinuxUser;

            var serviceFileName = serviceName + ".service";
            var serviceFile = TemporaryDirectory.S(serviceFileName);

            var serviceFileContents =
                $@"
                   [Unit]
                   Description={serviceName}
                   DefaultDependencies=no
                   StartLimitIntervalSec=30
                   StartLimitBurst=1
                    
                   [Service]
                   Type=simple
                   RemainAfterExit=no
                   ExecStart={dotnetPath} ""{dllName}""
                   Restart=always
                   RestartSec=1
                   WorkingDirectory={serviceDir}
                   User={serviceUser}
                   Group={serviceUser}
                    
                   [Install]
                   WantedBy=multi-user.target";

            serviceFileContents = string.Join('\n',
                serviceFileContents
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())).Trim() + "\n";

            File.WriteAllText(serviceFile, serviceFileContents);

            Remote.SftpCopy(Logger, login, serviceFile, "/etc/systemd/system");

            Remote.Ssh(Logger,
                login,
                new[]
                {
                    $"useradd --create-home -s /sbin/nologin {serviceUser}",
                    $"mkhomedir_helper {serviceUser}",
                    $"systemctl enable {serviceName}",
                    $"systemctl daemon-reload",
                    $"systemctl start {serviceName}",
                    $"mkdir {serviceDir}"
                });
        }

        public void Deploy()
        {
            var login = ServerMachineLogin;
            var serviceName = ServiceName;
            Remote.Ssh(Logger,
                login,
                new[]
                {
                    $"service {serviceName} stop"
                });

            var serviceUser = ServiceLinuxUser;
            var serviceDir = $"/var/{serviceName}";

            Remote.DeployFolderViaSftp(Logger,
                login,
                ServiceOutputPath.Directory,
                serviceDir,
                serviceUser,
                OutputDirectory);

            Remote.Ssh(Logger,
                login,
                new[]
                {
                    $"sudo systemctl enable {serviceName}",
                    $"service {serviceName} restart",
                    $"service {serviceName} status | grep exception",
                    $"sleep 1",
                    $"journalctl -u {serviceName}.service --since \"5 seconds ago\" -n 100 --no-pager",
                    $"service nginx restart"
                });
        }

        public void InstallAndDeploy()
        {
            Install();
            Deploy();
        }
    }
}