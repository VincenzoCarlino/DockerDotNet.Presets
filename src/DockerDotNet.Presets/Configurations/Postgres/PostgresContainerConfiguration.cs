namespace DockerDotNet.Presets.Configurations.Postgres;

    #region

using System.Collections.Generic;
using Docker.DotNet.Models;

#endregion

public class PostgresContainerConfiguration : ContainerConfiguration
{
    public string DbName { get; }
    public string DbUser { get; }
    public string DbPassword { get; }

    public PostgresContainerConfiguration(string dbName,
                                          string dbUser,
                                          string dbPassword,
                                          string containerName,
                                          string? volumeName,
                                          string imageTag,
                                          int portBinding)
        : base(containerName, volumeName, "postgres", imageTag, portBinding)
    {
        DbName = dbName;
        DbUser = dbUser;
        DbPassword = dbPassword;
    }

    public override CreateContainerParameters GetCreateContainerParameters()
    {
        if (!IsPortAvailable(PortBinding)) PortBinding = GetAvailablePort();

        return new CreateContainerParameters
        {
            Name = ContainerName,
            Image = $"{ImageName}:{ImageTag}",
            Env = new List<string>
            {
                $"POSTGRES_USER={DbUser}",
                $"POSTGRES_DB={DbName}",
                $"POSTGRES_PASSWORD={DbPassword}"
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        "5432/tcp",
                        new[]
                        {
                            new PortBinding
                            {
                                HostPort = PortBinding.ToString()
                            }
                        }
                    }
                }
            },
        };
    }
}