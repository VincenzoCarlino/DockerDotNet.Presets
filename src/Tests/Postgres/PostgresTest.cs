namespace Tests.Postgres;

using DockerDotNet.Presets;
using DockerDotNet.Presets.Configurations.Postgres;
using DockerDotNet.Presets.DTO;

using Npgsql;

using NUnit.Framework;

using System;
using System.Threading.Tasks;

internal class PostgresTest
{
    private string _containerId = string.Empty;
    private const string CONTAINER_NAME = "postgres_test";
    private const string DB_NAME = "testDb";
    private const string DB_USER = "testUser";
    private const string DB_PASSWORD = "testPassword";

    [Test, Order(1)]
    public async Task CreatePostgresContainerAsync()
    {
        var containerResult = await DockerContainerGenerator.CreateContainerAsync(
                new PostgresContainerConfiguration(
                    DB_NAME,
                    DB_USER,
                    DB_PASSWORD,
                    CONTAINER_NAME,
                    "postgres_test_volume",
                    "latest",
                    5432
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
                    Assert.Pass("Successfully created pgsql container");
                    return;
                }
                await connection.CloseAsync().ConfigureAwait(false);
            }
            catch(NpgsqlException _)
            {
                await Task.Delay(i * 1000).ConfigureAwait(false);
            }
        }

        Assert.Fail("Unable to create pgsql container");
    }

    [Test, Order(2)]
    public async Task DeletePostgresContainerByIdAsync()
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
