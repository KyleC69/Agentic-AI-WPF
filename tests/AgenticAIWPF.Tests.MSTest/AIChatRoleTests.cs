// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         AIChatRoleTests.cs
// Author: Kyle L. Crowder
// Build Num: 212953



using AgentAILib.Models;

using Microsoft.Extensions.AI;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class AIChatRoleTests
{
    [TestMethod]
    public void ConstructorWithNullValueThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _ = new AIChatRole(null!));
    }








    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void ConstructorWithWhitespaceValueThrowsArgumentException(string value)
    {
        Assert.ThrowsExactly<ArgumentException>(() => _ = new AIChatRole(value!));
    }








    [TestMethod]
    public void ConverterMethodsThrowNotImplementedException()
    {
        Converter converter = new();

        Assert.ThrowsExactly<NotImplementedException>(() => converter.ReadJson(null!, typeof(AIChatRole), default, false, null!));
        Assert.ThrowsExactly<NotImplementedException>(() => converter.WriteJson(null!, AIChatRole.User, null!));
    }








    [TestMethod]
    public void EqualityAgainstChatRoleIsCaseInsensitive()
    {
        AIChatRole aiRole = new("assistant");
        ChatRole chatRole = new("ASSISTANT");

        Assert.IsTrue(aiRole == chatRole);
        Assert.IsTrue(chatRole == aiRole);
        Assert.IsFalse(aiRole != chatRole);
        Assert.IsTrue(aiRole.Equals(chatRole));
    }








    [TestMethod]
    public void EqualityIsCaseInsensitive()
    {
        AIChatRole left = new("User");
        AIChatRole right = new("user");

        Assert.IsTrue(left == right);
        Assert.IsFalse(left != right);
        Assert.IsTrue(left.Equals(right));
    }








    [TestMethod]
    public void HashCodeMatchesForCaseVariants()
    {
        AIChatRole left = new("system");
        AIChatRole right = new("SYSTEM");

        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }








    [TestMethod]
    public void ImplicitConversionToAndFromChatRolePreservesValue()
    {
        AIChatRole source = new("user");
        ChatRole chatRole = source;
        AIChatRole roundTrip = chatRole;

        Assert.AreEqual("user", chatRole.Value);
        Assert.AreEqual(source, roundTrip);
    }








    [TestMethod]
    public void StaticRolesExposeExpectedValues()
    {
        Assert.AreEqual("system", AIChatRole.System.Value);
        Assert.AreEqual("assistant", AIChatRole.Assistant.Value);
        Assert.AreEqual("user", AIChatRole.User.Value);
        Assert.AreEqual("tool", AIChatRole.Tool.Value);
        Assert.AreEqual("context", AIChatRole.AIContext.Value);
        Assert.AreEqual("rag_context", AIChatRole.RAGContext.Value);
    }








    [TestMethod]
    public void ToChatRoleAndToAIChatRoleReturnExpectedValues()
    {
        AIChatRole role = new("tool");

        ChatRole chatRole = role.ToChatRole();
        AIChatRole sameRole = role.ToAIChatRole();

        Assert.AreEqual("tool", chatRole.Value);
        Assert.AreEqual(role, sameRole);
    }








    [TestMethod]
    public void ToStringReturnsUnderlyingValue()
    {
        AIChatRole role = new("rag_context");

        Assert.AreEqual("rag_context", role.ToString());
    }
}