using Diskor;
using ECF;

await new ECFHostBuilder()
    .UseSingleCommand<AnalyzeDirectoryCommand>()
    .Configure((ctx, services, _) =>
    {

    })
    .RunAsync(args);