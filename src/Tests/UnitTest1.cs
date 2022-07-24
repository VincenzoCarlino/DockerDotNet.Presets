namespace Tests;

#region

using System.Reflection;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDotNet.Presets;
using DockerDotNet.Presets.Configurations;
using DockerDotNet.Presets.Configurations.Postgres;

#endregion

public class Tests
{
    private const string POSTGRES_CONTAINER_NAME = "test_container_db_pgsql";
    private const string RABBIT_CONTAINER_NAME = "test_container_db_rabbit";


    [Test]
    [Order(1)]
    public async Task CreatePostgresContainer()
    {
        var port = ContainerConfiguration.GetAvailablePort();

        var containerConfiguration = new PostgresContainerConfiguration(
            "db_test",
            "user",
            "password",
            POSTGRES_CONTAINER_NAME,
            "test_container_db_pgsql_db_test",
            "latest",
            port
        );

        var containerResult = await DockerContainerGenerator.CreateContainerAsync(containerConfiguration).ConfigureAwait(false);

        var existContainer = await ExistContainer(POSTGRES_CONTAINER_NAME).ConfigureAwait(false);

        Assert.That(containerResult.Port, Is.EqualTo(port));
        Assert.That(containerResult.ContainerId, Is.Not.Empty);
        Assert.That(existContainer, Is.EqualTo(true));
    }

    [Test]
    [Order(2)]
    public async Task CreateRabbitMQContainer()
    {
        var containerConfiguration = new RabbitContainerConfiguration(
            RABBIT_CONTAINER_NAME,
            null,
            "3-management",
            5672,
            "admin",
            "password"
        );

        var containerResult = await DockerContainerGenerator.CreateContainerAsync(containerConfiguration).ConfigureAwait(false);

        var existContainer = await ExistContainer(POSTGRES_CONTAINER_NAME).ConfigureAwait(false);
        
        Assert.That(containerResult.ContainerId, Is.Not.Empty);
        Assert.That(existContainer, Is.EqualTo(true));
    }

    [Test]
    [Order(3)]
    [TestCase(POSTGRES_CONTAINER_NAME)]
    [TestCase(RABBIT_CONTAINER_NAME)]
    public async Task DeleteContainers(string containerName)
    {
        var method = typeof(DockerContainerGenerator).GetMethod("CleanupRunningContainersAsync", BindingFlags.NonPublic | BindingFlags.Static);
        if (method is null)
        {
            Assert.Fail("Missing method CleanupRunningContainersAsync in DockerContainerGenerator");
            return;
        }

        var deletingContainers = (Task) method.Invoke(null, new[] {containerName});
        await deletingContainers!.ConfigureAwait(false);
        var existContainer = await ExistContainer(containerName).ConfigureAwait(false);
        Assert.That(existContainer, Is.EqualTo(false));
    }

    private async Task<bool> ExistContainer(string containerName)
    {
        var method = typeof(DockerContainerGenerator).GetMethod("GetDockerClient", BindingFlags.NonPublic | BindingFlags.Static);
        if (method is null) throw new Exception("Missing method CheckIfPortIsAvailable in DockerContainerGenerator");

        var dockerClient = (DockerClient) method.Invoke(null, null);

        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters())
            .ConfigureAwait(false);

        return containers.Any(x => x.Names.Contains($"/{containerName}"));
    }
}