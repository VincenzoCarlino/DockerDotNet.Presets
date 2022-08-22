namespace Tests.Postgres;

using DockerDotNet.Presets;
using DockerDotNet.Presets.Configurations.Postgres;
using DockerDotNet.Presets.DTO;

using Npgsql;

using NUnit.Framework;

using System.Threading.Tasks;

internal class PostgisTest
{
    private string _containerId = string.Empty;

    private const string CONTAINER_NAME = "postgis_test";
    private const string DB_NAME = "testDb";
    private const string DB_USER = "testUser";
    private const string DB_PASSWORD = "testPassword";

    [Test, Order(1)]
    public async Task CreatePostgisContainerAsync()
    {
        var containerResult = await DockerContainerGenerator.CreateContainerAsync(
                new PostgisContainerConfiguration(
                    DB_NAME,
                    DB_USER,
                    DB_PASSWORD,
                    CONTAINER_NAME,
                    "postgis_volume",
                    "10-3.0-alpine",
                    5433
                )
            ).ConfigureAwait(false);

        _containerId = containerResult.ContainerId;

        var connectionString = string.Format(
                "Host={0};Username={1};Password={2};Database={3};Port={4}",
                "localhost",
                DB_USER,
                DB_PASSWORD,
                DB_NAME,
                containerResult.Port
            );

        for (int i = 0; i < 10; i++)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync().ConfigureAwait(false);
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    Assert.Pass("Successfully created postgis container");
                    return;
                }
                await connection.CloseAsync().ConfigureAwait(false);
            }
            catch (NpgsqlException _)
            {
                await Task.Delay(i * 1000).ConfigureAwait(false);
            }
        }

        Assert.Fail("Unable to create postgis container");
    }

    [Test, Order(2)]
    public async Task DeletePostgisContainerAsync()
    {
        await DockerContainerGenerator.DropContainerById(_containerId)
            .ConfigureAwait(false);

        var result = await DockerContainerGenerator.FindContainerAsync(
            GetDockerContainerRequest.CreateByContainerId(
                _containerId
                )
            )
            .ConfigureAwait(false);

        Assert.IsNull(result);
    }
}
