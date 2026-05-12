using Mirror;

namespace NetworkMessaging.Messages
{
    public struct SubscribeRequestMessage : NetworkMessage
    {
        public string MessageTypeFullName;
    }

    public struct HelloMessage : NetworkMessage
    {
        public string Text;
    }
}