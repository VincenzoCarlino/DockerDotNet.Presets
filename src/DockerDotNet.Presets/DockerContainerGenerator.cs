namespace DockerDotNet.Presets;

    #region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerDotNet.Presets.Configurations;
using DockerDotNet.Presets.DTO;

#endregion

public class DockerContainerGenerator
{
    public static async Task<DockerContainerResult> CreateContainerAsync(ContainerConfiguration containerConfiguration)
    {
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

    public static async Task DropContainerById(string containerId)
    {
        var dockerClient = GetDockerClient();

        await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters()).ConfigureAwait(false);
        await dockerClient.Containers
            .RemoveContainerAsync(containerId, new ContainerRemoveParameters {RemoveVolumes = true, Force = true})
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
                .RemoveContainerAsync(container.ID, new ContainerRemoveParameters {RemoveVolumes = true, Force = true})
                .ConfigureAwait(false);
        }
    }

    private static async Task CleanupRunningVolume(string volumeName)
    {
        var dockerClient = GetDockerClient();

        var runningVolumes = await dockerClient.Volumes.ListAsync().ConfigureAwait(false);

        foreach (var volume in runningVolumes.Volumes.Where(volume => volume.Name.Contains(volumeName)))
            await dockerClient.Volumes.RemoveAsync(volume.Name).ConfigureAwait(false);
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