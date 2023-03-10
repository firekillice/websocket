using Carbon.Match.Contact;
using Carbon.Protocols;
using Google.Protobuf;
using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbon.Match.Message
{
    public static class MessageConverter
    {
        public static (byte[], uint, uint?) Read(PVPMessage message)
        {
            var newMessage = message.Type switch
            {
                -6 => ReadControl(message),
                _ => ReadUnknown(message),
            };
            return (newMessage.ToByteArray(), message.Seq, message.Sseq != 0 ? message.Sseq : default);
        }

        private static NetworkingMessage ReadUnknown(PVPMessage message) => new NetworkingMessage() { Type = (uint)message.Type, Unknown = message.Raw };

        public static NetworkingMessage ReadControl(PVPMessage message)
        {
            var span = message.Raw.ToByteArray().AsSpan();
            var code = BinaryPrimitives.ReadUInt32LittleEndian(span[..4]);
            var command = Encoding.UTF8.GetString(span[4..]);
            return new NetworkingMessage() { Type = code, Control = command };
        }

        public static PVPMessage Write(byte[]? b, uint sequenceNo, uint? responseSequenceNo)
        {
            var raw = NetworkingMessage.Parser.ParseFrom(b);
            return raw.BodyCase switch
            {
                NetworkingMessage.BodyOneofCase.Unknown => WriteUnknown(raw, sequenceNo, responseSequenceNo),
                NetworkingMessage.BodyOneofCase.Control => WriteControl(raw, sequenceNo, responseSequenceNo),
                _ => throw new ArgumentOutOfRangeException(nameof(b)),
            };
        }

        private static PVPMessage WriteUnknown(NetworkingMessage raw, uint sequenceNo, uint? responseSequenceNo)
        {
            return new PVPMessage
            {
                Type = (int)raw.Type,
                Raw = raw.Unknown,
                Seq = responseSequenceNo ?? 0,
                Sseq = sequenceNo,
            };
        }

        private static PVPMessage WriteControl(NetworkingMessage raw, uint sequenceNo, uint? responseSequenceNo)
        {
            var control = new byte[Encoding.UTF8.GetByteCount(raw.Control) + 4];
            BinaryPrimitives.WriteUInt32LittleEndian(control.AsSpan()[..4], raw.Type);
            Encoding.UTF8.GetBytes(raw.Control, control.AsSpan()[4..]);

            return new PVPMessage
            {
                Type = -6,
                Raw = ByteString.CopyFrom(control),
                Seq = responseSequenceNo ?? 0,
                Sseq = sequenceNo,
            };
        }
    }
}
