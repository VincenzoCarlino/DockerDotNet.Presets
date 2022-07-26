namespace DockerDotNet.Presets.Configurations;

using Docker.DotNet.Models;

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public abstract class ContainerConfiguration
{
    public string ContainerName { get; }
    public string? VolumeName { get; }
    public string ImageName { get; }
    public string ImageTag { get; }
    public int PortBinding { get; protected set; }

    protected ContainerConfiguration(string containerName,
        string? volumeName,
        string imageName,
        string imageTag,
        int portBinding)
    {
        ContainerName = containerName;
        VolumeName = volumeName;
        ImageName = imageName;
        ImageTag = imageTag;
        PortBinding = portBinding;
    }

    public static bool IsPortAvailable(int port)
    {
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
        var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

        foreach (var endpoint in tcpConnInfoArray)
            if (endpoint.Port == port)
                return false;

        return true;
    }

    public static int GetAvailablePort()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }

    public abstract CreateContainerParameters GetCreateContainerParameters();
}