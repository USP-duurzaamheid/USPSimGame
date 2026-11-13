using System.Net;
using Microsoft.AspNetCore.Http;
using USPSimGame.Services.PlayerSessions;

namespace USPSimGame.Tests.Services.PlayerSessions;

public class ClientConnectionInfoTests
{
    private static HttpContextAccessor AccessorWithIp(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return new HttpContextAccessor { HttpContext = context };
    }

    [Fact]
    public void ClientKey_WithIp_IsTheIp()
    {
        //Arrange
        var accessor = AccessorWithIp("10.0.0.1");

        //Act
        var info = new ClientConnectionInfo(accessor);

        //Assert
        Assert.Equal("10.0.0.1", info.ClientKey);
    }

    [Fact]
    public void ClientKey_WithoutHttpContext_IsUnknown()
    {
        //Arrange
        var accessor = new HttpContextAccessor { HttpContext = null };

        //Act
        var info = new ClientConnectionInfo(accessor);

        //Assert
        Assert.Equal(ClientConnectionInfo.UnknownClient, info.ClientKey);
    }

    [Fact]
    public void Refresh_WhileUnknown_AndIpNowAvailable_UpdatesKey()
    {
        //Arrange
        var info = new ClientConnectionInfo(new HttpContextAccessor { HttpContext = null });

        //Act
        info.Refresh(AccessorWithIp("10.0.0.1"));

        //Assert
        Assert.Equal("10.0.0.1", info.ClientKey);
    }

    [Fact]
    public void Refresh_WhileAlreadyKnown_DoesNotChangeKey()
    {
        //Arrange
        var info = new ClientConnectionInfo(AccessorWithIp("10.0.0.1"));

        //Act
        info.Refresh(AccessorWithIp("10.0.0.2"));

        //Assert
        // A client must not be able to swap to a fresh rate limit key mid-circuit.
        Assert.Equal("10.0.0.1", info.ClientKey);
    }
}
