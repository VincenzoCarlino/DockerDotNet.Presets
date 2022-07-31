namespace DockerDotNet.Presets.Configurations.Postgres;

using System.Collections.Generic;
using Docker.DotNet.Models;

public class PostgisContainerConfiguration : ContainerConfiguration
{
    public string DbName { get; }
    public string DbUser { get; }
    public string DbPassword { get; }

    public PostgisContainerConfiguration(string dbName,
                                         string dbUser,
                                         string dbPassword,
                                         string containerName,
                                         string? volumeName,
                                         string imageTag,
                                         int portBinding)
        : base(containerName, volumeName, "postgis/postgis", imageTag, portBinding)
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