namespace DockerDotNet.Presets.Configurations.Redmine;

#region

using System.Collections.Generic;
using Docker.DotNet.Models;

#endregion

public class RedmineContainerConfiguration : ContainerConfiguration
{
    public string DbUsername { get; }
    public string DbPassword { get; }
    public string DbName { get; }
    public string DbHost { get; }
    public int DbPort { get; }
    public string RedmineSecretKeyBase { get; }
    public RedmineContainerConfiguration(string containerName,
                                         string? volumeName,
                                         string imageName,
                                         string imageTag,
                                         int portBinding,
                                         string dbUsername,
                                         string dbPassword,
                                         string dbName,
                                         string redmineSecretKeyBase,
                                         string dbHost,
                                         int dbPort) : base(containerName, volumeName, imageName, imageTag, portBinding)
    {
        DbUsername = dbUsername;
        DbPassword = dbPassword;
        DbName = dbName;
        RedmineSecretKeyBase = redmineSecretKeyBase;
        DbHost = dbHost;
        DbPort = dbPort;
    }

    public override CreateContainerParameters GetCreateContainerParameters()
    {
        
        if (!IsPortAvailable(PortBinding)) PortBinding = GetAvailablePort();

        return new CreateContainerParameters
        {
            Name = ContainerName,
            Image = $"{ImageName}:{ImageTag}",
            Env = new[]
            {
                $"REDMINE_DB_POSTGRES={DbHost}",
                $"REDMINE_DB_USERNAME={DbUsername}",
                $"REDMINE_DB_PASSWORD={DbPassword}",
                $"REDMINE_DB_DATABASE={DbName}",
                $"REDMINE_SECRET_KEY_BASE={RedmineSecretKeyBase}",
                $"REDMINE_DB_PORT={DbPort}"
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        "3000/tcp",
                        new[]
                        {
                            new PortBinding
                            {
                                HostPort = PortBinding.ToString()
                            }
                        }
                    }
                },
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>()
            {
                { "3000", new EmptyStruct()}
            }
        };
    }
}