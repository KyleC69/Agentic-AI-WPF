using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using DataIngestionLib.History.HistoryModels;
using DataIngestionLib.HistoryModels;

using MahApps.Metro.Controls;

using RAGDataIngestionWPF.Converters;
using RAGDataIngestionWPF.Core.Models;
using RAGDataIngestionWPF.Helpers;
using RAGDataIngestionWPF.TemplateSelectors;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class CoverageBoostMiscTests
{
    [TestMethod]
    public void BaseViewModelCanBeCreatedAndCollectionEventAccessorsAreCallable()
    {
        Type baseViewModelType = Type.GetType("RAGDataIngestionWPF.ViewModels.BaseViewModel, RAGDataIngestionWPF");
        Assert.IsNotNull(baseViewModelType);

        object instance = Activator.CreateInstance(baseViewModelType, true);

        Assert.IsInstanceOfType<INotifyPropertyChanged>(instance);
        Assert.IsInstanceOfType<INotifyPropertyChanging>(instance);
        Assert.IsInstanceOfType<INotifyCollectionChanged>(instance);

        INotifyCollectionChanged collectionChanged = (INotifyCollectionChanged)instance;
        NotifyCollectionChangedEventHandler handler = (_, _) => { };
        collectionChanged.CollectionChanged += handler;
        collectionChanged.CollectionChanged -= handler;
    }

    [TestMethod]
    public void UserAndHistoryModelsRoundTripAssignedValues()
    {
        User user = new()
        {
            BusinessPhones = ["+1-555-0100"],
            DisplayName = "Display",
            GivenName = "Given",
            Id = "id-1",
            JobTitle = "Engineer",
            Mail = "user@example.com",
            MobilePhone = "+1-555-0101",
            OfficeLocation = "HQ",
            Photo = "photo",
            PreferredLanguage = "en-US",
            Surname = "Surname",
            UserPrincipalName = "upn"
        };

        ChatHistoryMessage message = new()
        {
            AgentId = "agent",
            ApplicationId = "app",
            Content = "content",
            ConversationId = "conv",
            CreatedAt = DateTime.UtcNow,
            Enabled = true,
            MessageId = Guid.NewGuid(),
            Metadata = "{\"x\":1}",
            Role = "assistant",
            Summary = "summary",
            TimestampUtc = DateTime.Now,
            UserId = "user"
        };

        ChatHistoryTextChunk chunk = new()
        {
            ChunkLength = 10,
            ChunkOffset = 20,
            ChunkOrder = 1,
            ChunkRecordId = 7,
            ChunkSetId = 99,
            ChunkText = "chunk text",
            CreatedAt = DateTime.UtcNow,
            MessageId = Guid.NewGuid()
        };

        Assert.AreEqual("Display", user.DisplayName);
        Assert.AreEqual("upn", user.UserPrincipalName);
        Assert.AreEqual("assistant", message.Role);
        Assert.IsTrue(message.Enabled.Value);
        Assert.AreEqual("chunk text", chunk.ChunkText);
        Assert.AreEqual(99L, chunk.ChunkSetId);
    }

    [TestMethod]
    public void MenuItemTemplateSelectorReturnsExpectedTemplateByItemType()
    {
        StaTestHelper.Run(() =>
        {
            DataTemplate glyphTemplate = new();
            DataTemplate imageTemplate = new();
            MenuItemTemplateSelector selector = new()
            {
                GlyphDataTemplate = glyphTemplate,
                ImageDataTemplate = imageTemplate
            };

            DataTemplate glyphResult = selector.SelectTemplate(new HamburgerMenuGlyphItem(), new DependencyObject());
            DataTemplate imageResult = selector.SelectTemplate(new HamburgerMenuImageItem(), new DependencyObject());
            DataTemplate fallbackResult = selector.SelectTemplate(new object(), new DependencyObject());

            Assert.AreSame(glyphTemplate, glyphResult);
            Assert.AreSame(imageTemplate, imageResult);
            Assert.IsNull(fallbackResult);
        });
    }

    [TestMethod]
    public void FrameExtensionsCleanNavigationAndGetDataContextWorkForCommonCases()
    {
        StaTestHelper.Run(() =>
        {
            Frame frame = new();
            _ = frame.Navigate(new Page());
            _ = frame.Navigate(new Page());

            frame.CleanNavigation();

            Frame dataFrame = new();
            Assert.IsNull(dataFrame.GetDataContext());

            dataFrame.Content = new object();
            Assert.IsNull(dataFrame.GetDataContext());
        });
    }

    [TestMethod]
    public void MarkdownConverterHandlesEmptyAndRichMarkdown()
    {
        StaTestHelper.Run(() =>
        {
            MarkdownToFlowDocumentConverter converter = new();

            FlowDocument empty = (FlowDocument)converter.Convert(null, typeof(FlowDocument), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(0, empty.Blocks.Count);

            const string markdown = "# Title\n\nParagraph with *italic*, **bold**, ~~strike~~, `code`, and [link](https://example.com).\n\n> Quote line\n\n- one\n- two\n\n---\n\n```csharp\nConsole.WriteLine(\"x\");\n```";

            FlowDocument rich = (FlowDocument)converter.Convert(markdown, typeof(FlowDocument), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.IsTrue(rich.Blocks.Count >= 5);
            Assert.AreSame(Binding.DoNothing, converter.ConvertBack(rich, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
        });
    }
}
