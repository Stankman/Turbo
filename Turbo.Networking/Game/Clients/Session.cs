using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Turbo.Core.Game.Players;
using Turbo.Core.Networking.Game.Clients;
using Turbo.Core.Packets.Messages;
using Turbo.Core.Packets.Revisions;
using Turbo.Core.Security;
using Turbo.Networking.Extensions;

namespace Turbo.Networking.Game.Clients;

public class Session : ISession
{
    private readonly IChannelHandlerContext _channel;
    private readonly ILogger<Session> _logger;
    private readonly BlockingCollection<IClientPacket> pendingReadMessages = new(15);

    public Session(IChannelHandlerContext channel, IRevision initialRevision, ILogger<Session> logger)
    {
        _channel = channel;
        _logger = logger;

        Revision = initialRevision;
        LastPongTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    public IChannel Channel => _channel.Channel;
    public IRevision Revision { get; set; }
    public IRc4Service Rc4 { get; set; }
    public IPlayer Player { get; private set; }

    public string IPAddress { get; }
    public long LastPongTimestamp { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (Player != null)
        {
            await Player.DisposeAsync();

            Player = null;
        }

        await _channel.CloseAsync();
    }

    public bool SetPlayer(IPlayer player)
    {
        if (Player != null && Player != player) return false;

        Player = player;

        return true;
    }

    public async Task Send(IComposer composer)
    {
        await Send(composer, false);
    }

    public async Task SendQueue(IComposer composer)
    {
        await Send(composer, true);
    }

    public void Flush()
    {
        _channel.Flush();
    }
    
    public void OnMessageReceived(IClientPacket messageEvent)
    {
        if (!pendingReadMessages.TryAdd(messageEvent))
        {
            messageEvent.Content.ReleaseAll();
        }
    }
    
    public async Task HandleDecodedMessages()
    {
        foreach (var msg in pendingReadMessages.GetConsumingEnumerable())
        {
            if (Revision.Parsers.TryGetValue(msg.Header, out var parser))
            {
                _logger.LogDebug("Received {}:{}", msg.Header, parser.GetType().Name);
                await parser.HandleAsync(this, msg, _messageHub);
            }
            else
            {
                _logger.LogDebug("No matching parser found for message {}:{}", msg.Header, msg);
            }
        }
    }

    protected async Task Send(IComposer composer, bool queue)
    {
        if (!IsConnected()) return;

        if (Revision.Serializers.TryGetValue(composer.GetType(), out var serializer))
        {
            var packet = serializer.Serialize(_channel.Allocator.Buffer(), composer);

            try
            {
                if (queue) await _channel.WriteAsync(packet);
                else await _channel.WriteAndFlushAsync(packet);
            }

            catch (Exception exception)
            {
                _logger.LogDebug(exception.Message);
            }

            _logger.LogDebug($"Sent {packet.Header}: {composer.GetType().Name}");
        }
        else
        {
            _logger.LogDebug($"No matching serializer found for message {composer.GetType().Name}");
        }
    }

    public bool IsConnected()
    {
        return _channel.Channel.Open;
    }
}