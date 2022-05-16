using ConsumerBackgroundServices;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Consumer>(); })
    .Build();

await host.RunAsync();
