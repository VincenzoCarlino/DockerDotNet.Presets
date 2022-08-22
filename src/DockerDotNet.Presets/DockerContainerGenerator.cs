namespace DockerDotNet.Presets;

using Docker.DotNet;
using Docker.DotNet.Models;

using DockerDotNet.Presets.Configurations;
using DockerDotNet.Presets.DTO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DockerContainerGenerator
{
    /// <summary>
    /// Creates a docker container using the passed configuration.
    /// </summary>
    /// <param name="containerConfiguration"></param>
    /// <returns></returns>
    /// <exception cref="Exception">If we are unable to create the container</exception>
    public static async Task<DockerContainerResult> CreateContainerAsync(ContainerConfiguration containerConfiguration)
    {
        var existingContainer = await FindContainerAsync(
                GetDockerContainerRequest.CreateByContainerName(containerConfiguration.ContainerName)
            )
            .ConfigureAwait(false);

        if (existingContainer is not null && existingContainer.Ports.Any(x => x.PublicPort == containerConfiguration.PortBinding))
        {
            return new DockerContainerResult(existingContainer.ID, containerConfiguration.PortBinding);
        }

        var createContainerParameters = containerConfiguration.GetCreateContainerParameters();

        await CleanupRunningContainersAsync(containerConfiguration.ContainerName);

        if (containerConfiguration.VolumeName is not null)
        {
            await CleanupRunningVolume(containerConfiguration.VolumeName);
            createContainerParameters.HostConfig.Binds = new List<string>
            {
                $"{containerConfiguration.VolumeName}:/data"
            };
        }

        var dockerClient = GetDockerClient();

        await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
        {
            FromImage = $"{containerConfiguration.ImageName}:{containerConfiguration.ImageTag}"
        }, null, new Progress<JSONMessage>())
            .ConfigureAwait(false);

        var container = await dockerClient.Containers.CreateContainerAsync(createContainerParameters).ConfigureAwait(false);

        var result = await dockerClient.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()).ConfigureAwait(false);

        if (!result) throw new Exception("Failing on creating container");


        return new DockerContainerResult(container.ID, containerConfiguration.PortBinding);
    }

    /// <summary>
    /// Get the container requested if founded otherwise it returns null.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static async Task<ContainerListResponse?> FindContainerAsync(GetDockerContainerRequest request)
    {
        var dockerClient = GetDockerClient();

        var runningContainers = await dockerClient.Containers
            .ListContainersAsync(
                new ContainersListParameters()
            ).ConfigureAwait(false);

        if (request.ContainerName is not null)
            return runningContainers.FirstOrDefault(
                container => container.Names.Any(
                    name => name.Contains(request.ContainerName)
                )
            );

        if (request.ContainerId is not null)
            return runningContainers.FirstOrDefault(
                container => container.ID == request.ContainerId
            );

        return null;
    }

    /// <summary>
    /// Drop the container with the id passed.
    /// </summary>
    /// <param name="containerId"></param>
    /// <returns></returns>
    public static async Task DropContainerById(string containerId)
    {
        var dockerClient = GetDockerClient();

        await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters()).ConfigureAwait(false);
        await dockerClient.Containers
            .RemoveContainerAsync(containerId, new ContainerRemoveParameters { RemoveVolumes = true, Force = true })
            .ConfigureAwait(false);
    }

    private static async Task CleanupRunningContainersAsync(string containerName)
    {
        var dockerClient = GetDockerClient();

        var runningContainers = await dockerClient.Containers
            .ListContainersAsync(
                new ContainersListParameters()
            ).ConfigureAwait(false);

        var containersToDelete = runningContainers.Where(
            container => container.Names.Any(
                name => name.Contains(containerName)
            )
        ).ToList();


        foreach (var container in containersToDelete)
        {
            await dockerClient.Containers.StopContainerAsync(container.ID, new ContainerStopParameters()).ConfigureAwait(false);
            await dockerClient.Containers
                .RemoveContainerAsync(container.ID, new ContainerRemoveParameters { RemoveVolumes = true, Force = true })
                .ConfigureAwait(false);
        }
    }

    private static async Task CleanupRunningVolume(string volumeName)
    {
        var dockerClient = GetDockerClient();

        var runningVolumes = await dockerClient.Volumes.ListAsync().ConfigureAwait(false);

        foreach (var volume in runningVolumes.Volumes.Where(volume => volume.Name.Contains(volumeName)))
            await dockerClient.Volumes.RemoveAsync(volume.Name, true).ConfigureAwait(false);
    }

    private static DockerClient GetDockerClient()
    {
        var dockerUri = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? "npipe://./pipe/docker_engine"
            : "unix:///var/run/docker.sock";
        return new DockerClientConfiguration(new Uri(dockerUri))
            .CreateClient();
    }
}