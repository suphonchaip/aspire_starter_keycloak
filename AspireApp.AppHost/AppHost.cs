var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

var seq = builder.AddSeq("seq")
    .WithEnvironment("ACEPT_EULA","Y")
    .WithLifetime(ContainerLifetime.Persistent);

var keycloak = builder.AddKeycloak("keycloak", 8080)
    .WithDataVolume("keycloak_data")
    .WithExternalHttpEndpoints();


var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.AspireApp_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(seq)
    .WaitFor(seq)
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.AddProject<Projects.AspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(seq)
    .WaitFor(seq);

builder.Build().Run();
