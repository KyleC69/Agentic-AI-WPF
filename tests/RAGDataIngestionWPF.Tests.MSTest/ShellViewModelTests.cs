using MahApps.Metro.Controls;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ShellViewModelTests
{
    [TestMethod]
    public void LoadedCommandAddsUserMenuItem()
    {
        Mock<INavigationService> navigation = new();
        Mock<IUserDataService> userData = new();
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel { Name = "Alice", UserPrincipalName = "Alice" });

        ShellViewModel viewModel = new(navigation.Object, userData.Object);

        viewModel.LoadedCommand.Execute(null);

        Assert.AreEqual(2, viewModel.OptionMenuItems.Count);
        Assert.IsNotNull(viewModel.OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault());
    }

    [TestMethod]
    public void MenuItemInvokedNavigatesToSelectedTarget()
    {
        Mock<INavigationService> navigation = new();
        navigation.Setup(service => service.NavigateTo(It.IsAny<string>(), null, false)).Returns(true);
        Mock<IUserDataService> userData = new();
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel());

        ShellViewModel viewModel = new(navigation.Object, userData.Object);
        viewModel.SelectedMenuItem = viewModel.MenuItems[0];

        viewModel.MenuItemInvokedCommand.Execute(null);

        navigation.Verify(service => service.NavigateTo(viewModel.MenuItems[0].TargetPageType.FullName, null, false), Times.Once);
    }

    [TestMethod]
    public void OptionsMenuItemInvokedNavigatesToSelectedTarget()
    {
        Mock<INavigationService> navigation = new();
        navigation.Setup(service => service.NavigateTo(It.IsAny<string>(), null, false)).Returns(true);
        Mock<IUserDataService> userData = new();
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel());

        ShellViewModel viewModel = new(navigation.Object, userData.Object);
        viewModel.SelectedOptionsMenuItem = viewModel.OptionMenuItems[0];

        viewModel.OptionsMenuItemInvokedCommand.Execute(null);

        navigation.Verify(service => service.NavigateTo(viewModel.OptionMenuItems[0].TargetPageType.FullName, null, false), Times.Once);
    }

    [TestMethod]
    public void GoBackCommandUsesNavigationService()
    {
        Mock<INavigationService> navigation = new();
        navigation.SetupGet(service => service.CanGoBack).Returns(true);
        Mock<IUserDataService> userData = new();
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel());
        ShellViewModel viewModel = new(navigation.Object, userData.Object);

        Assert.IsTrue(viewModel.GoBackCommand.CanExecute(null));

        viewModel.GoBackCommand.Execute(null);

        navigation.Verify(service => service.GoBack(), Times.Once);
    }

    [TestMethod]
    public void UnloadedCommandRemovesUserImageMenuItem()
    {
        Mock<INavigationService> navigation = new();
        Mock<IUserDataService> userData = new();
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel { Name = "User" });
        ShellViewModel viewModel = new(navigation.Object, userData.Object);

        viewModel.LoadedCommand.Execute(null);
        viewModel.UnloadedCommand.Execute(null);

        Assert.AreEqual(1, viewModel.OptionMenuItems.Count);
        Assert.IsNull(viewModel.OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault());
    }

    [TestMethod]
    public void UserDataUpdatedEventUpdatesUserMenuItem()
    {
        Mock<INavigationService> navigation = new();
        Mock<IUserDataService> userData = new();
        userData.Setup(service => service.GetUser()).Returns(new UserViewModel { Name = "Before" });
        ShellViewModel viewModel = new(navigation.Object, userData.Object);
        viewModel.LoadedCommand.Execute(null);

        userData.Raise(service => service.UserDataUpdated += null, userData.Object, new UserViewModel { Name = "After" });

        HamburgerMenuImageItem userItem = viewModel.OptionMenuItems.OfType<HamburgerMenuImageItem>().First();
        Assert.AreEqual("After", userItem.Label);
    }
}
