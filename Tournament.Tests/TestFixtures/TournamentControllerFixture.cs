using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Contracts.Interfaces;
using Tournament.Presentation.Controllers;

namespace Tournament.Tests.TestFixtures;
public class TournamentControllerFixture : IDisposable
{
    public Mock<ITournamentService> MockTournamentService { get; private set; }
    public Mock<IServiceManager> MockServiceManager { get; private set; }
    public TournamentsController Controller { get; private set; }
    public TournamentControllerFixture()
    {
        MockTournamentService = new Mock<ITournamentService>();
        MockServiceManager = new Mock<IServiceManager>();
        MockServiceManager.Setup(serviceManager => serviceManager.TournamentService).Returns(MockTournamentService.Object);

        Controller = new TournamentsController(MockServiceManager.Object);
        Controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
    }
    public void Dispose()
    {
        MockServiceManager.Invocations.Clear();
        MockTournamentService.Invocations.Clear();
    }
}
