using USPSimGame.Services.GameSessions;
using System.Text.RegularExpressions;

namespace USPSimGame.Tests.Services.GameSessions;

public class JoinCodeGeneratorTests
{
    private const int ExpectedCodeLength = 6;

    [Fact]
    public void Generate_ReturnsSixCharacterCode()
    {
        //Arrange
        var generator = new JoinCodeGenerator();

        for (int i = 0; i < 1000; i++)
        {
            //Act
            var code = generator.Generate();
            //Assert
            Assert.Equal(ExpectedCodeLength, code.Length);
        }
    }

    [Fact]
    public void Generate_ReturnsCodeWithOnlyDigits()
    {
        //Arrange
        var generator = new JoinCodeGenerator();

        for (int i = 0; i < 1000; i++)
        {
            //Act
            var code = generator.Generate();
            //Assert
            Assert.True(Regex.IsMatch(code, @"^[0-9]+$"), "Code mag alleen cijfers bevatten.");
        }
    }
}
