using NetArchTest.Rules;
using Xunit;

namespace USPSimGame.ArchitectureTests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_should_not_depend_on_any_other_layer()
    {
        var domainAssembly = typeof(Domain.Entities.Plan).Assembly;

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "USPSimGame.Application",
                "USPSimGame.Infrastructure",
                "USPSimGame.Web",
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain must remain independent of the outer layers and frameworks.");
    }
}
