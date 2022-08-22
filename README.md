# DockerDotNet.Presets

This library is made to speed up and simplify the working experience with docker containers.
It is made up on [Docker.DotNet](https://github.com/dotnet/Docker.DotNet) that is a really great library that exposes good api to interact with docker,
but it could be a bit verbose and long to bring up containers, for this reason I built this library with a set of ready configurations to run up containers.

## Installation
You can add this library to your project using [NuGet](https://www.nuget.org/packages/DockerDotNet.Presets/).

**Package Manager Console**
Run the following command in the “Package Manager Console”:

> PM> Install-Package DockerDotNet.Presets

**.NET Core Command Line Interface**
Run the following command from your favorite shell or terminal:

> dotnet add package DockerDotNet.Presets

## Usage

Create a container from a preset [configuration](https://github.com/VincenzoCarlino/DockerDotNet.Presets/tree/master/src/DockerDotNet.Presets/Configurations):

```csharp
using DockerDotNet.Presets;
using DockerDotNet.Presets.Configurations.Postgres;
using DockerDotNet.Presets.DTO;

DockerContainerResult containerResult = await DockerContainerGenerator.CreateContainerAsync(
        new PostgresContainerConfiguration(
            "DB_NAME",
            "DB_USER",
            "DB_PASSWORD",
            "postgres_container_name",
            "postgres_test_volume",
            "latest",
            5432
        )
    ).ConfigureAwait(false);
```

Drop a container by container id:

```csharp
using DockerDotNet.Presets;

await DockerContainerGenerator.DropContainerById(_containerId)
    .ConfigureAwait(false);
```

Find a container by container id or container name:

```csharp
using Docker.DotNet.Models;

using DockerDotNet.Presets;
using DockerDotNet.Presets.DTO;

ContainerListResponse? result = await DockerContainerGenerator.FindContainerAsync(
        GetDockerContainerRequest.CreateByContainerId(
            _containerId
        )
    ).ConfigureAwait(false);
```

If you need to create a container that is not present in this [configurations](https://github.com/VincenzoCarlino/DockerDotNet.Presets/tree/master/src/DockerDotNet.Presets/Configurations) you can create your own
(and if it useful improve configurations of this repo) by extending abstract class: [ContainerConfiguration](https://github.com/VincenzoCarlino/DockerDotNet.Presets/blob/master/src/DockerDotNet.Presets/Configurations/ContainerConfiguration.cs)

## License

DockerDotNet.Presets is licensed under the [MIT](https://github.com/VincenzoCarlino/DockerDotNet.Presets/blob/master/LICENSE.txt) license.