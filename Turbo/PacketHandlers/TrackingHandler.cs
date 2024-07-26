using Microsoft.Extensions.Logging;
using Turbo.Core.Events;
using Turbo.Core.Networking.Game.Clients;
using Turbo.Core.PacketHandlers;
using Turbo.Core.Packets;
using Turbo.Packets.Incoming.Tracking;

namespace Turbo.Main.PacketHandlers;

public class TrackingHandler : ITrackingHandler
{
    
    private readonly ILogger<AuthenticationMessageHandler> _logger;
    private readonly IPacketMessageHub _messageHub;
    
    public TrackingHandler(
        IPacketMessageHub messageHub,
        ILogger<AuthenticationMessageHandler> logger
    )
    {
        _messageHub = messageHub;
        _logger = logger;
        
        _messageHub.Subscribe<LatencyPingReportMessage>(this, OnLatencyPingReport);
    }
    
    
    private async void OnLatencyPingReport(LatencyPingReportMessage message, ISession session)
    {

        int averageLatency = message.AverageLatency;
        int validPingAverage = message.ValidPingAverage;
        int numPings = message.NumPings;
        
        _logger.LogInformation("Latency Ping Report: {0} {1} {2} from {3}", averageLatency, validPingAverage, numPings, session.IPAddress);
    }
}