using DataIngestionLib.Models;

using Microsoft.Extensions.AI;



namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class AIChatRoleTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void ConstructorWithNullOrWhitespaceValueThrowsArgumentException(string value)
    {
        Assert.ThrowsExactly<ArgumentException>(() => new AIChatRole(value!));
    }

    [TestMethod]
    public void EqualityIsCaseInsensitiveAcrossAIChatRoleAndChatRole()
    {
        AIChatRole aiRole = new("USER");
        ChatRole chatRole = new("user");

        Assert.AreEqual(new AIChatRole("user"), aiRole);
        Assert.IsTrue(aiRole == chatRole);
        Assert.IsTrue(chatRole == aiRole);
        Assert.IsFalse(aiRole != chatRole);
    }

    [TestMethod]
    public void HashCodeMatchesForValuesThatDifferOnlyByCase()
    {
        AIChatRole uppercase = new("RAG_CONTEXT");
        AIChatRole lowercase = new("rag_context");

        Assert.AreEqual(uppercase.GetHashCode(), lowercase.GetHashCode());
    }

    [TestMethod]
    public void ImplicitConversionsPreserveCustomRoleValue()
    {
        AIChatRole originalRole = new("domain_specific_role");

        ChatRole asChatRole = originalRole;
        AIChatRole roundTrippedRole = asChatRole;

        Assert.AreEqual("domain_specific_role", asChatRole.Value);
        Assert.AreEqual(originalRole, roundTrippedRole);
    }

    [TestMethod]
    public void ToStringReturnsUnderlyingRoleValue()
    {
        AIChatRole role = AIChatRole.AIContext;

        Assert.AreEqual("context", role.ToString());
    }
}
