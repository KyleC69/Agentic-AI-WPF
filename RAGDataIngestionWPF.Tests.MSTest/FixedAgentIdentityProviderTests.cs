// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Tests.MSTest
//  File:         FixedAgentIdentityProviderTests.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Models;
using DataIngestionLib.Services;

using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Tests.MSTest;





/// <summary>
///     Unit tests for <see cref="FixedAgentIdentityProvider" /> and related model correctness.
/// </summary>
[TestClass]
public class FixedAgentIdentityProviderTests
{




    [TestMethod]
    public void AIChatMessage_ToString_ReturnsEmptyString_WhenContentIsNull()
    {
        AIChatMessage message = new(ChatRole.User, (string?)null);
        Assert.AreEqual(string.Empty, message.ToString());
    }








    [TestMethod]
    public void AIChatMessage_ToString_ReturnsMessageText()
    {
        AIChatMessage message = new(ChatRole.User, "Hello, world!");
        Assert.AreEqual("Hello, world!", message.ToString());
    }








    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Constructor_ThrowsArgumentException_WhenAgentIdIsNullOrWhiteSpace(string? agentId)
    {
        Assert.ThrowsExactly<ArgumentException>(() => _ = new FixedAgentIdentityProvider(agentId!));
    }








    [TestMethod]
    public void GetAgentId_ReturnsConfiguredValue()
    {
        FixedAgentIdentityProvider provider = new("my-agent");
        Assert.AreEqual("my-agent", provider.GetAgentId());
    }
}