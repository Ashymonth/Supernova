using Projects;
using SupernovaSchool.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddPrometheus("prometheus");
builder.AddGrafana("grafana-server");

builder.AddProject<SupernovaSchool_Host>("supernovaschool-host");

builder.Build().Run();