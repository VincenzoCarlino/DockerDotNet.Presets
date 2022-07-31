namespace DockerDotNet.Presets.Configurations.Postgres;

    #region

using System.Collections.Generic;
using Docker.DotNet.Models;

#endregion

public class RabbitContainerConfiguration : ContainerConfiguration
{
    public string RabbitDefaultUser { get; }
    public string RabbitDefaultPassword { get; }

    public RabbitContainerConfiguration(string containerName,
                                        string? volumeName,
                                        string imageTag,
                                        int portBinding,
                                        string rabbitDefaultUser,
                                        string rabbitDefaultPassword) : base(
        containerName,
        volumeName,
        "rabbitmq",
        imageTag,
        portBinding)
    {
        RabbitDefaultUser = rabbitDefaultUser;
        RabbitDefaultPassword = rabbitDefaultPassword;
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
                $"RABBITMQ_DEFAULT_PASS={RabbitDefaultPassword}"
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