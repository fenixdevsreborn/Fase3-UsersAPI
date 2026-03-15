var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);
var usersDb = postgres.AddDatabase("UsersDb");

var api = builder.AddProject<Projects.Fcg_Users_Api>("fcg-users-api")
    .WaitFor(usersDb)
    .WithReference(usersDb);

builder.Build().Run();
