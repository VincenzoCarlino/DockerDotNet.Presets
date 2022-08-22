namespace Tests.Rabbit;

using DockerDotNet.Presets;
using DockerDotNet.Presets.Configurations.Postgres;
using DockerDotNet.Presets.DTO;

using NUnit.Framework;

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

using System.Threading.Tasks;

internal class RabbitTest
{
    private string _containerId = string.Empty;

    private const string RABBIT_USER = "user";
    private const string RABBIT_PASSWORD = "password";
    private const string RABBIT_VHOST = "/";

    [Test, Order(1)]
    public async Task CreateRabbitContainerAsync()
    {
        var containerResult = await DockerContainerGenerator.CreateContainerAsync(
                new RabbitContainerConfiguration(
                    "rabbit_mq_container_name",
                    null,
                    "3-management",
                    5672,
                    RABBIT_USER,
                    RABBIT_PASSWORD,
                    RABBIT_VHOST
                )
            ).ConfigureAwait(false);

        _containerId = containerResult.ContainerId;

        ConnectionFactory factory = new ConnectionFactory();

        factory.UserName = RABBIT_USER;
        factory.Password = RABBIT_PASSWORD;
        factory.VirtualHost = RABBIT_VHOST;
        factory.Port = containerResult.Port;
        factory.HostName = "localhost";

        for (int i = 0; i < 10; i++)
        {
            try
            {
                IConnection conn = factory.CreateConnection();

                if (conn.IsOpen)
                {
                    Assert.Pass("Connected to rabbit mq");
                }
            }
            catch (BrokerUnreachableException _)
            {
                await Task.Delay(i * 1000).ConfigureAwait(false);
            }
        }

        Assert.Fail("Unable to create container for rabbitmq");
    }

    [Test, Order(2)]
    public async Task DeleteRabbitContainerAsync()
    {
        await DockerContainerGenerator.DropContainerById(_containerId)
            .ConfigureAwait(false);

        var result = await DockerContainerGenerator.FindContainerAsync(
                GetDockerContainerRequest.CreateByContainerId(
                    _containerId
                )
            ).ConfigureAwait(false);

        Assert.IsNull(result);
    }
}
