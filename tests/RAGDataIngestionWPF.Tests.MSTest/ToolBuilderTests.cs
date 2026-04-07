// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         ToolBuilderTests.cs
// Author: Kyle L. Crowder
// Build Num: 213003



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

        var result = getAiTools.Invoke(null, null);

        Assert.IsInstanceOfType<IList<AITool>>(result);
        var tools = (IList<AITool>)result;
        Assert.AreEqual(7, tools.Count);
    }
}