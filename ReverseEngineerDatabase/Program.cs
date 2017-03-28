using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace ReverseEngineerDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            // Add base services for scaffolding
            var serviceCollection = new ServiceCollection()
                .AddScaffolding()
                .AddLogging();

            // Add database provider services
            var provider = new SqlServerDesignTimeServices();
            provider.ConfigureDesignTimeServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var generator = serviceProvider.GetService<ReverseEngineeringGenerator>();
            var options = new ReverseEngineeringConfiguration
            {
                ConnectionString = @"Data Source=WIN-DTLAIS5TR8U\LOCALHOST;Integrated Security=True;Initial Catalog=SharedLibrary;MultipleActiveResultSets=True;App=EntityFramework",
                ProjectPath = @"C:\temp\",
                ProjectRootNamespace = "My.Namespace"
            };

            generator.GenerateAsync(options).Wait();
        }
    }
}
