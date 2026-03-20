using System.Reflection;

using Microsoft.Extensions.AI;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ToolBuilderTests
{
    [TestMethod]
    public void GetAiToolsReturnsExpectedToolCollection()
    {
        Type toolBuilderType = Type.GetType("DataIngestionLib.ToolFunctions.ToolBuilder, DataIngestionLib");
        Assert.IsNotNull(toolBuilderType);

        MethodInfo getAiTools = toolBuilderType.GetMethod("GetAiTools", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(getAiTools);

        object result = getAiTools.Invoke(null, null);

        Assert.IsInstanceOfType<IList<AITool>>(result);
        IList<AITool> tools = (IList<AITool>)result;
        Assert.AreEqual(7, tools.Count);
    }
}
