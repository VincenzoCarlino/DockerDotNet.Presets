namespace DockerDotNet.Presets.Configurations.Redis;

using System.Collections.Generic;
using Docker.DotNet.Models;

public class RedisContainerConfiguration : ContainerConfiguration
{
    public RedisContainerConfiguration(string containerName, string? volumeName, string imageName, string imageTag, int portBinding) : base(containerName, volumeName, imageName, imageTag, portBinding)
    {
    }

    public override CreateContainerParameters GetCreateContainerParameters()
    {
        if (!IsPortAvailable(PortBinding)) PortBinding = GetAvailablePort();

        return new CreateContainerParameters
        {
            Name = ContainerName,
            Image = $"{ImageName}:{ImageTag}",
            Cmd = new List<string>(){"redis-server"},
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