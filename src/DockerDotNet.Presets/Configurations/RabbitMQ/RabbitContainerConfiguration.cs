namespace DockerDotNet.Presets.Configurations.Postgres;

using Docker.DotNet.Models;

using System.Collections.Generic;

public class RabbitContainerConfiguration : ContainerConfiguration
{
    public string RabbitDefaultUser { get; }
    public string RabbitDefaultPassword { get; }
    public string RabbitVHost { get; }

    public RabbitContainerConfiguration(string containerName,
        string? volumeName,
        string imageTag,
        int portBinding,
        string rabbitDefaultUser,
        string rabbitDefaultPassword,
        string rabbitVHost) : base(containerName,
            volumeName,
            "rabbitmq",
            imageTag,
            portBinding)
    {
        RabbitDefaultUser = rabbitDefaultUser;
        RabbitDefaultPassword = rabbitDefaultPassword;
        RabbitVHost = rabbitVHost;
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
                $"RABBITMQ_DEFAULT_USER={RabbitDefaultUser}",
                $"RABBITMQ_DEFAULT_PASS={RabbitDefaultPassword}",
                $"RABBITMQ_DEFAULT_VHOST={RabbitVHost}"
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        "5672/tcp",
                        new[]
                        {
                            new PortBinding
                            {
                                HostPort = PortBinding.ToString()
                            }
                        }
                    }
                },
            }
        };
    }
}