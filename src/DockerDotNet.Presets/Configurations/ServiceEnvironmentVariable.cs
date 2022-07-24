namespace DockerDotNet.Presets.Configurations;

public class ServiceEnvironmentVariable
{
    public string Name { get; }
    public string Value { get; }

    public ServiceEnvironmentVariable(string name, string value)
    {
        Name = name;
        Value = value;
    }
}