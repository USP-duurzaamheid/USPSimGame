using USPSimGame.Services.GameSessions;

namespace USPSimGame.Tests.Services.GameSessions;

public class JoinCodeFormatTests {

    [Theory]
    [InlineData ("123456", "123456")]
    [InlineData ("000123", "000123")]
    [InlineData (" 123456 ", "123456")]

    public void TryNormalize_ValidInput_ReturnsTrueAndCode(string input, string expectedCode)
    {
        //Act
        bool result = JoinCodeFormat.TryNormalize(input, out var code);
        
        //Assert
        Assert.True(result);
        Assert.Equal(expectedCode, code);
    }

    [Theory]
    [InlineData ("123 456")]
    [InlineData ("1234567")]
    [InlineData ("12345")]
    [InlineData ("12a456")]
    [InlineData (null)]
    [InlineData ("")]
    [InlineData (" ")]
    [InlineData ("１２３４５６")]

    public void TryNormalize_InvalidInput_ReturnsFalse(string? input)
    {
        //Act
        bool result = JoinCodeFormat.TryNormalize(input, out var code);

        //Assert
        Assert.False(result);
        Assert.Equal(string.Empty, code);
    }
}