// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         PageServiceAndUiTests.cs
// Author: Kyle L. Crowder
// Build Num: 212959



using System.Windows.Markup;

using Microsoft.Extensions.DependencyInjection;

using AgenticAIWPF.Services;
using AgenticAIWPF.ViewModels;
using AgenticAIWPF.Views;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class PageServiceAndUiTests
{









    [TestMethod]
    public void GetPageCanSurfaceXamlParseFailureWhenUiResourcesAreMissing()
    {
        StaTestHelper.Run(() =>
        {
            ServiceCollection services = [];
            _ = services.AddTransient<DataGridPage>();
            PageService service = new(services.BuildServiceProvider());

            Assert.ThrowsExactly<XamlParseException>(() => service.GetPage(typeof(DataGridViewModel).FullName));
        });
    }








    [TestMethod]
    public void GetPageThrowsWhenServiceProviderCannotResolvePage()
    {
        PageService service = new(new ServiceCollection().BuildServiceProvider());
        var key = typeof(DataGridViewModel).FullName;

        Assert.ThrowsExactly<InvalidOperationException>(() => service.GetPage(key));
    }








    [TestMethod]
    public void GetPageTypeReturnsConfiguredTypeForDataGridViewModel()
    {
        PageService service = new PageService(new ServiceCollection().BuildServiceProvider());

        Type pageType = service.GetPageType(typeof(DataGridViewModel).FullName);

        Assert.AreEqual(typeof(DataGridPage), pageType);
    }








    [TestMethod]
    public void GetPageTypeWithUnknownKeyThrowsArgumentException()
    {
        PageService service = new PageService(new ServiceCollection().BuildServiceProvider());

        Assert.ThrowsExactly<ArgumentException>(() => service.GetPageType("unknown"));
    }
}