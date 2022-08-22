namespace DockerDotNet.Presets.Configurations.Redis;

using Docker.DotNet.Models;

using System.Collections.Generic;

public class RedisContainerConfiguration : ContainerConfiguration
{
    public RedisContainerConfiguration(string containerName,
        string? volumeName,
        string imageTag,
        int portBinding) : base(containerName,
            volumeName,
            "redis",
            imageTag,
            portBinding)
    {
    }

    public override CreateContainerParameters GetCreateContainerParameters()
    {
        if (!IsPortAvailable(PortBinding)) PortBinding = GetAvailablePort();

        return new CreateContainerParameters
        {
            Name = ContainerName,
            Image = $"{ImageName}:{ImageTag}",
            Cmd = new List<string>() { "redis-server" },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        "6379/tcp",
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
                { "6379", new EmptyStruct()}
            }
        };
    }
}