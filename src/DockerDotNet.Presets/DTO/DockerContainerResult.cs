namespace DockerDotNet.Presets.DTO;

public class DockerContainerResult
{
    public string ContainerId { get; }
    public int Port { get; }

    public DockerContainerResult(string containerId, int port)
    {
        ContainerId = containerId;
        Port = port;
    }
}