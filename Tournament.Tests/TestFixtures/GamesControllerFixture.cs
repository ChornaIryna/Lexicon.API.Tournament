using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Controllers;

namespace Tournament.Tests.TestFixtures;
public class GamesControllerFixture : IDisposable
{
    public Mock<IServiceManager> MockServiceManager { get; private set; }
    public Mock<IGameService> MockGameService { get; private set; }
    public GamesController Controller { get; private set; }
    public GamesControllerFixture()
    {
        MockServiceManager = new Mock<IServiceManager>();
        MockGameService = new Mock<IGameService>();
        MockServiceManager.Setup(serviceManager => serviceManager.GameService).Returns(MockGameService.Object);

        Controller = new GamesController(MockServiceManager.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = new DefaultHttpContext() }
        };
    }

    public void Clear()
    {
        MockServiceManager.Invocations.Clear();
        MockGameService.Invocations.Clear();
    }

    public void Dispose() { }
}
