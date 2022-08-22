namespace DockerDotNet.Presets.DTO;

public class GetDockerContainerRequest
{
    internal string? ContainerId { get; }
    internal string? ContainerName { get; }

    private GetDockerContainerRequest(string? containerId, string? containerName)
    {
        ContainerId = containerId;
        ContainerName = containerName;
    }

    public static GetDockerContainerRequest CreateByContainerId(string? containerId)
        => new(containerId, null);

    public static GetDockerContainerRequest CreateByContainerName(string? containerName)
        => new(null, containerName);
}
