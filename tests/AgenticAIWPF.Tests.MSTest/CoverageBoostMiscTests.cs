// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         CoverageBoostMiscTests.cs
// Author: Kyle L. Crowder
// Build Num: 212954



using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using AgentAILib.History.HistoryModels;
using AgentAILib.HistoryModels;

using AgenticAIWPF.Controls;
using AgenticAIWPF.Converters;
using AgenticAIWPF.Core.Models;
using AgenticAIWPF.Helpers;
using AgenticAIWPF.TemplateSelectors;

using MahApps.Metro.Controls;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class CoverageBoostMiscTests
{
    [TestMethod]
    public void BaseViewModelCanBeCreatedAndCollectionEventAccessorsAreCallable()
    {
        Type baseViewModelType = Type.GetType("AgenticAIWPF.ViewModels.BaseViewModel, AgenticAIWPF");
        Assert.IsNotNull(baseViewModelType);

        var instance = Activator.CreateInstance(baseViewModelType, true);

        Assert.IsInstanceOfType<INotifyPropertyChanged>(instance);
        Assert.IsInstanceOfType<INotifyPropertyChanging>(instance);
        Assert.IsInstanceOfType<INotifyCollectionChanged>(instance);

        INotifyCollectionChanged collectionChanged = (INotifyCollectionChanged)instance;
        NotifyCollectionChangedEventHandler handler = (_, _) => { };
        collectionChanged.CollectionChanged += handler;
        collectionChanged.CollectionChanged -= handler;
    }








    [TestMethod]
    public void FrameExtensionsCleanNavigationAndGetDataContextWorkForCommonCases()
    {
        StaTestHelper.Run(() =>
        {
            Frame frame = new Frame();
            _ = frame.Navigate(new Page());
            _ = frame.Navigate(new Page());

            frame.CleanNavigation();

            Frame dataFrame = new Frame();
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
            MarkdownToFlowDocumentConverter converter = new MarkdownToFlowDocumentConverter();

            FlowDocument empty = (FlowDocument)converter.Convert(null, typeof(FlowDocument), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.AreEqual(0, empty.Blocks.Count);

            const string markdown = "# Title\n\nParagraph with *italic*, **bold**, ~~strike~~, `code`, and [link](https://example.com).\n\n> Quote line\n\n- one\n- two\n\n---\n\n```csharp\nConsole.WriteLine(\"x\");\n```";

            FlowDocument rich = (FlowDocument)converter.Convert(markdown, typeof(FlowDocument), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.IsTrue(rich.Blocks.Count >= 5);
            Assert.AreSame(Binding.DoNothing, converter.ConvertBack(rich, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture));
        });
    }






    [TestMethod]
    public void MarkdownConverterRendersFencedCodeBlocksAsInteractiveControls()
    {
        StaTestHelper.Run(() =>
        {
            const string markdown = "```csharp\nConsole.WriteLine(\"x\");\n```";

            FlowDocument document = MarkdownToFlowDocumentConverter.ConvertToFlowDocument(markdown);
            BlockUIContainer codeBlock = document.Blocks.OfType<BlockUIContainer>().Single();
            MarkdownCodeBlockControl control = (MarkdownCodeBlockControl)codeBlock.Child;

            Assert.AreEqual("csharp", control.CodeLanguage);
            StringAssert.Contains(control.Code, "Console.WriteLine");
        });
    }






    [TestMethod]
    public void MarkdownConverterRendersHtmlAnchorTextWithoutTypeNames()
    {
        StaTestHelper.Run(() =>
        {
            const string markdown = "Paragraph with <a href=\"https://example.com\">Example Link</a>.";

            FlowDocument document = MarkdownToFlowDocumentConverter.ConvertToFlowDocument(markdown);
            string renderedText = new TextRange(document.ContentStart, document.ContentEnd).Text;
            Paragraph paragraph = document.Blocks.OfType<Paragraph>().Single();

            StringAssert.Contains(renderedText, "Example Link");
            StringAssert.DoesNotContain(renderedText, "HtmlInline");
            Assert.IsTrue(paragraph.Inlines.OfType<Hyperlink>().Any());
        });
    }








    [TestMethod]
    public void MenuItemTemplateSelectorReturnsExpectedTemplateByItemType()
    {
        StaTestHelper.Run(() =>
        {
            DataTemplate glyphTemplate = new DataTemplate();
            DataTemplate imageTemplate = new DataTemplate();
            MenuItemTemplateSelector selector = new MenuItemTemplateSelector { GlyphDataTemplate = glyphTemplate, ImageDataTemplate = imageTemplate };

            DataTemplate glyphResult = selector.SelectTemplate(new HamburgerMenuGlyphItem(), new DependencyObject());
            DataTemplate imageResult = selector.SelectTemplate(new HamburgerMenuImageItem(), new DependencyObject());
            DataTemplate fallbackResult = selector.SelectTemplate(new object(), new DependencyObject());

            Assert.AreSame(glyphTemplate, glyphResult);
            Assert.AreSame(imageTemplate, imageResult);
            Assert.IsNull(fallbackResult);
        });
    }








    [TestMethod]
    public void UserAndHistoryModelsRoundTripAssignedValues()
    {
        User user = new User
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

        ChatHistoryMessage message = new ChatHistoryMessage
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
            UserId = "user"
        };

        ChatHistoryTextChunk chunk = new ChatHistoryTextChunk
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
}