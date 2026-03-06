using Azure.Provisioning.Storage;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("env");

var storage = builder.AddAzureStorage("storage").RunAsEmulator()
    .ConfigureInfrastructure((infrastructure) =>
    {
        var storageAccount = infrastructure.GetProvisionableResources().OfType<StorageAccount>().FirstOrDefault(r => r.BicepIdentifier == "storage")
            ?? throw new InvalidOperationException($"Could not find configured storage account with name 'storage'");

        // Ensure that public access to blobs is disabled
        storageAccount.AllowBlobPublicAccess = false;
    });

var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

var db = builder.AddSqlite("db").WithSqliteWeb();

var apiService = builder
    .AddProject<Projects.AspireAppReactApp_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db)
    .WithHttpHealthCheck("/health");


builder.AddViteApp(name: "todo-frontend", workingDirectory: "../todo-frontend")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithNpmPackageInstallation();

builder.AddNpmApp(name: "todo-frontend-angular", workingDirectory: "../todo-angualr-app")
.WithReference(apiService)
.WaitFor(apiService)
.WithHttpEndpoint(env: "PORT")
// optional
//.PublishAsDockerFile()
    .WithNpmPackageInstallation();

builder.AddJavaScriptApp(name: "vue", "../vue-frontend")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();
    // optional
    //.PublishAsDockerFile();

builder.AddProject<Projects.AspireAppReactApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    
    .WithReference(apiService)
    .WithReference(queues)
    .WithReference(blobs)
    .WaitFor(apiService)
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.MvcWebApp>("mvcwebapp")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
