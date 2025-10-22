namespace EdgegapProxy.Models;

/// <summary>
/// Server information returned to Unity clients.
/// </summary>
public class ServerInfo
{
    public string ServerId { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Region { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int PingMs { get; set; }
    public bool IsHealthy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Get connection address (IP:Port format).
    /// </summary>
    public string GetConnectionAddress() => $"{IpAddress}:{Port}";

    /// <summary>
    /// Get display name for UI (e.g., "Chicago (5/300 players) - 45ms").
    /// </summary>
    public string GetDisplayName()
    {
        if (!IsHealthy)
            return $"{City} - Offline";

        return $"{City} ({CurrentPlayers}/{MaxPlayers} players) - {PingMs}ms";
    }
}

/// <summary>
/// Edgegap deployment response model.
/// </summary>
public class EdgegapDeploymentResponse
{
    public List<EdgegapDeployment> Data { get; set; } = new();
}

/// <summary>
/// Edgegap deployment model.
/// </summary>
public class EdgegapDeployment
{
    public string RequestId { get; set; } = string.Empty;
    public string Fqdn { get; set; } = string.Empty;
    public string PublicIp { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public Dictionary<string, EdgegapPort> Ports { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Edgegap port mapping model.
/// </summary>
public class EdgegapPort
{
    public int External { get; set; }
    public int Internal { get; set; }
    public string Protocol { get; set; } = string.Empty;
}

/// <summary>
/// Health check response from game server.
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public int Players { get; set; }
    public int MaxPlayers { get; set; }
    public int Uptime { get; set; }
    public long Timestamp { get; set; }
}
