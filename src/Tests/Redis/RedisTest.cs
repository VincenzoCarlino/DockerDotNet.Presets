namespace Tests.Redis;

using DockerDotNet.Presets;
using DockerDotNet.Presets.Configurations.Redis;
using DockerDotNet.Presets.DTO;

using NUnit.Framework;

using StackExchange.Redis;

using System.Threading.Tasks;

internal class RedisTest
{
    private string _containerId = string.Empty;

    [Test, Order(1)]
    public async Task CreateRedisContainerAsync()
    {
        var container = await DockerContainerGenerator.CreateContainerAsync(
                new RedisContainerConfiguration(
                    "redis_test",
                    null,
                    "5.0.9-alpine",
                    6379
                )
            ).ConfigureAwait(false);

        _containerId = container.ContainerId;

        for (int i = 0; i < 10; i++)
        {
            try
            {
                var redis = await ConnectionMultiplexer.ConnectAsync(
                    new ConfigurationOptions()
                    {
                        EndPoints = { $"localhost:{container.Port}" }
                    }
                ).ConfigureAwait(false);

                var db = redis.GetDatabase();
                var pong = await db.PingAsync().ConfigureAwait(false);
                Assert.Pass("Connected to redis");
            }
            catch(RedisConnectionException e)
            {
                await Task.Delay(i * 1000).ConfigureAwait(false);
            }
        }

        Assert.Fail("Unable to create redis container");
    }

    [Test, Order(2)]
    public async Task DeleteRedisContainerAsync()
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
