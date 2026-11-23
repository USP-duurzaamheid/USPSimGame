using USPSimGame.Data.Enums;
using USPSimGame.Utils;

namespace USPSimGame.Tests.Utils;

public class CommonGameUtilsTests
{
    [Theory]
    [InlineData(GameState.Setup, true)]
    [InlineData(GameState.Pause, true)]
    [InlineData(GameState.Play, true)]
    [InlineData(GameState.Simulation, false)]
    [InlineData(GameState.Complete, false)]

    public void IsJoinable_State_ReturnsExpected(GameState state, bool joinable)
    {
        //Act
        bool result = CommonGameUtils.IsJoinable(state);

        //Assert
        Assert.Equal(joinable, result);
    }
}
