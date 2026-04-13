using System.ComponentModel;




namespace DataIngestionLib.ToolFunctions.OSTools;

/// <summary>
/// Displays DNS resolution information.
/// </summary>
[Description("Queries DNS servers for domain name resolution and diagnostics.")]
public class NsLookupTool(CommandExecutor executor)
{
    private const string Command = "nslookup.exe";

    [Description("Resolves a domain name to IP addresses.")]
    public async Task<CommandResult> Resolve(
            [Description("Domain name to resolve (e.g., 'microsoft.com')")] string domain)
        => await executor.ExecuteAsync(Command, domain);

    [Description("Resolves a domain using a specific DNS server.")]
    public async Task<CommandResult> ResolveWithServer(
            [Description("Domain name to resolve")] string domain,
            [Description("DNS server IP address")] string dnsServer)
        => await executor.ExecuteAsync(Command, $"{domain} {dnsServer}");

    [Description("Performs a reverse lookup on an IP address.")]
    public async Task<CommandResult> ReverseLookup(
            [Description("IP address to lookup")] string ipAddress)
        => await executor.ExecuteAsync(Command, ipAddress);
}