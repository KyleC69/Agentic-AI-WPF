using System.Windows.Controls;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.ViewModels;
using RAGDataIngestionWPF.Services;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class NavigationServiceIntegrationTests
{
    private sealed class TrackingNavigationAware : INavigationAware
    {
        public int NavigatedFromCount { get; private set; }
        public int NavigatedToCount { get; private set; }
        public object LastParameter { get; private set; }

        public void OnNavigatedFrom()
        {
            NavigatedFromCount++;
        }

        public void OnNavigatedTo(object parameter)
        {
            NavigatedToCount++;
            LastParameter = parameter;
        }
    }

    private sealed class TestPageA : Page
    {
        public TestPageA(object dataContext)
        {
            DataContext = dataContext;
        }
    }

    private sealed class TestPageB : Page
    {
        public TestPageB(object dataContext)
        {
            DataContext = dataContext;
        }
    }

    [TestMethod]
    public void NavigateToResolvesPageTypeAndPage()
    {
        StaTestHelper.Run(() =>
        {
            TrackingNavigationAware aware = new();
            Mock<IPageService> pageService = new();
            pageService.Setup(service => service.GetPageType("A")).Returns(typeof(TestPageA));
            pageService.Setup(service => service.GetPage("A")).Returns(new TestPageA(aware));

            NavigationService navigationService = new(pageService.Object);
            navigationService.Initialize(new Frame());

            _ = navigationService.NavigateTo("A", "payload", clearNavigation: true);

            pageService.Verify(service => service.GetPageType("A"), Times.Once);
            pageService.Verify(service => service.GetPage("A"), Times.Once);
        });
    }

    [TestMethod]
    public void NavigateToWhenPageServiceThrowsPropagatesException()
    {
        StaTestHelper.Run(() =>
        {
            Mock<IPageService> pageService = new();
            pageService.Setup(service => service.GetPageType("A")).Throws(new ArgumentException("missing"));

            NavigationService navigationService = new(pageService.Object);
            navigationService.Initialize(new Frame());

            Assert.ThrowsExactly<ArgumentException>(() => _ = navigationService.NavigateTo("A"));
        });
    }

    [TestMethod]
    public void GoBackWithoutHistoryDoesNotThrow()
    {
        StaTestHelper.Run(() =>
        {
            Mock<IPageService> pageService = new();
            NavigationService navigationService = new(pageService.Object);
            navigationService.Initialize(new Frame());

            navigationService.GoBack();

            Assert.IsFalse(navigationService.CanGoBack);
        });
    }

    [TestMethod]
    public void UnsubscribeNavigationClearsFrameHandlers()
    {
        StaTestHelper.Run(() =>
        {
            Mock<IPageService> pageService = new();
            NavigationService navigationService = new(pageService.Object);
            navigationService.Initialize(new Frame());

            navigationService.UnsubscribeNavigation();

            Assert.ThrowsExactly<NullReferenceException>(() =>
            {
                bool canGoBack = navigationService.CanGoBack;
                _ = canGoBack;
            });
        });
    }
}
