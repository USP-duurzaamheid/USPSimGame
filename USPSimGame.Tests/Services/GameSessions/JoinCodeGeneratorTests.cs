using USPSimGame.Services.GameSessions;

namespace USPSimGame.Tests.Services.GameSessions;
    
    public class JoinCodeGeneratorTests
    {            
        [Fact]
        public void Generate_ReturnsCodeOfConfiguredLength()
        {
            //Arrange
            var generator = new JoinCodeGenerator();
            //Act
            var code = generator.Generate();
            //Assert
            Assert.Equal(JoinCodeGenerator.CodeLength, code.Length);
        }
    }
    