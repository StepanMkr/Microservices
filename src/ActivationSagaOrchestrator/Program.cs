using CreationSagaOrchestrator.Saga;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddSagaStateMachine<CreationSagaStateMachine, CreationSagaState>()
                .InMemoryRepository();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("test");
                    h.Password("test");
                });

                cfg.ReceiveEndpoint("creation-saga", e =>
                {
                    e.UseInMemoryOutbox(context);
                    e.ConfigureSaga<CreationSagaState>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();