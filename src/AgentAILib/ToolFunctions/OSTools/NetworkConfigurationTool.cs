// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgentAILib
// File:         NetworkConfigurationTool.cs
// Author: Kyle L. Crowder
// Build Num: 194513



using System.ComponentModel;
using System.Net.NetworkInformation;

using AgentAILib.ToolFunctions.Utils;




namespace AgentAILib.ToolFunctions.OSTools;





public sealed class NetworkConfigurationSnapshot
{
    public string AdapterName { get; init; } = string.Empty;
    public string DnsServers { get; init; } = string.Empty;
    public string GatewayAddresses { get; init; } = string.Empty;
    public string InterfaceType { get; init; } = string.Empty;
    public string OperationalStatus { get; init; } = string.Empty;
    public string UnicastAddresses { get; init; } = string.Empty;
}





public sealed class NetworkConfigurationTool
{
    private const int MaxResults = 12;








    [Description("Read a bounded snapshot of active local network configuration.")]
    public ToolResult<IReadOnlyList<NetworkConfigurationSnapshot>> ReadActiveAdapters([Description("Maximum number of adapters to return. Range: 1 to 12.")] int maxResults = 6)
    {
        if (maxResults < 1 || maxResults > MaxResults)
        {
            return ToolResult<IReadOnlyList<NetworkConfigurationSnapshot>>.Fail($"maxResults must be between 1 and {MaxResults}.");
        }

        try
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
                    .OrderBy(adapter => adapter.Name, StringComparer.OrdinalIgnoreCase)
                    .Take(maxResults)
                    .Select(adapter =>
                    {
                        IPInterfaceProperties properties = adapter.GetIPProperties();
                        return new NetworkConfigurationSnapshot
                        {
                                AdapterName = DiagnosticsText.Truncate(adapter.Name, 128),
                                InterfaceType = adapter.NetworkInterfaceType.ToString(),
                                OperationalStatus = adapter.OperationalStatus.ToString(),
                                UnicastAddresses = DiagnosticsText.JoinBounded(properties.UnicastAddresses.Select(address => address.Address.ToString()), 6),
                                GatewayAddresses = DiagnosticsText.JoinBounded(properties.GatewayAddresses.Select(address => address.Address.ToString()), 4),
                                DnsServers = DiagnosticsText.JoinBounded(properties.DnsAddresses.Select(address => address.ToString()), 4)
                        };
                    })
                    .ToList()
                    .AsReadOnly();

            return ToolResult<IReadOnlyList<NetworkConfigurationSnapshot>>.Ok(adapters);
        }
        catch (NetworkInformationException ex)
        {
            return ToolResult<IReadOnlyList<NetworkConfigurationSnapshot>>.Fail($"Network inspection failed: {ex.Message}");
        }
    }
}