// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         ToolBuilderTests.cs
// Author: Kyle L. Crowder
// Build Num: 213003



using AgentAILib.ToolFunctions;
using AgentAILib.ToolFunctions.FileSystemReaders;
using AgentAILib.ToolFunctions.FileSystemWriters;
using AgentAILib.ToolFunctions.General;
using AgentAILib.ToolFunctions.OSTools;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using Moq;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class ToolBuilderTests
{
    [TestMethod]
    public void GetAiToolsReturnsExpectedToolCollection()
    {
        var sandboxRoot = Path.Combine(Path.GetTempPath(), "tool-builder-tests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(sandboxRoot);

        try
        {
            Mock<IHttpClientFactory> mockFactory = new();
            mockFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

            using ILoggerFactory loggerFactory = LoggerFactory.Create(_ => { });
            CommandExecutor executor = new();

            ToolBuilder toolBuilder = new(
                new WebSearchPlugin(mockFactory.Object),
                new SandboxEventLogReader(),
                new EventErrorsTool(),
                new PowerShellTool(),
                new FileContentsReadingTool([sandboxRoot]),
                new FileSystemWriterTool([sandboxRoot]),
                new InstalledUpdatesTool(),
                new ListFolderContentsTool([sandboxRoot]),
                new LogFileListingTool([sandboxRoot]),
                new LogFileReader([sandboxRoot]),
                new NetworkConfigurationTool(),
                new PerformanceCounterTool(),
                new RegistryReaderTool(loggerFactory),
                new ReliabilityHistoryTool(),
                new ServiceHealthTool(),
                new StartupInventoryTool(),
                new StorageHealthTool(),
                new WindowsEventChannelReaderTool(),
                new WindowsWmiReaderTool(),
                new PsInfoTool(executor),
                new PsListTool(executor),
                new NsLookupTool(executor),
                new NetStatTool(executor),
                new HandleTool(executor));

            IList<AITool> readOnlyTools = toolBuilder.GetReadOnlyAiTools();
            IList<AITool> allTools = toolBuilder.GetAiTools();
            IList<AITool> writingTools = toolBuilder.GetWritingAiTools();

            Assert.AreEqual(readOnlyTools.Count, allTools.Count);
            Assert.AreEqual(35, readOnlyTools.Count);
            Assert.AreEqual(1, writingTools.Count);
        }
        finally
        {
            if (Directory.Exists(sandboxRoot))
            {
                Directory.Delete(sandboxRoot, true);
            }
        }
    }
}