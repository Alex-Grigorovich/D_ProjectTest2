using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using NetworkMessaging.Messages;

namespace NetworkMessaging.Services
{
    public class NetworkMessageService
    {
        private readonly Dictionary<Type, HashSet<NetworkConnection>> _subscriptions = new();

        public void Initialize()
        {
            NetworkServer.RegisterHandler<SubscribeRequestMessage>(OnSubscribeRequestReceived);
            NetworkClient.RegisterHandler<HelloMessage>(OnHelloMessageReceived);
        }

        public void Subscribe<T>() where T : struct, NetworkMessage
        {
            if (!NetworkClient.active || NetworkClient.connection == null)
            {
                Debug.LogWarning("[NetworkMessageService] Client not connected, cannot subscribe.");
                return;
            }

            var msg = new SubscribeRequestMessage { MessageTypeFullName = typeof(T).FullName };
            NetworkClient.connection.Send(msg);
        }

        public void SendToSubscribers<T>(T message) where T : struct, NetworkMessage
        {
            if (!NetworkServer.active) 
            {
                Debug.LogWarning("[Server] Server not active, cannot send");
                return;
            }

            var type = typeof(T);
            if (!_subscriptions.TryGetValue(type, out var subscribers))
            {
                Debug.LogWarning($"[Server] No subscribers for {type.Name}");
                return;
            }

            foreach (var conn in subscribers)
            {
                if (conn != null && conn.isReady)
                {
                    conn.Send(message);
                }
            }
        }

        private void OnSubscribeRequestReceived(NetworkConnection conn, SubscribeRequestMessage msg)
        {
            var type = Type.GetType(msg.MessageTypeFullName);
            if (type == null || !typeof(NetworkMessage).IsAssignableFrom(type))
            {
                Debug.LogError($"[Server] Unknown message type requested: {msg.MessageTypeFullName}");
                return;
            }

            if (!_subscriptions.ContainsKey(type))
                _subscriptions[type] = new HashSet<NetworkConnection>();

            _subscriptions[type].Add(conn);
        }

        private void OnHelloMessageReceived(HelloMessage _)
        {
        }
    }
}