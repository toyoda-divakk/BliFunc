using BliFunc.Library.Interfaces;
using BliFunc.Library.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddTransient<IFunctionService, FunctionService>();
        services.AddTransient<ISemanticService, SemanticService>();
        services.AddTransient<IWorkRecordService, WorkRecordService>();
        services.AddTransient<ITodoService, TodoService>();
    })
    .Build();

host.Run();
