using Carbon.Match.New.Contact;
using Google.Protobuf;
using Server.Protocols;
using System;
using System.Buffers.Binary;
using System.Text;

namespace Carbon.Match.New.Message
{
    public static class MessageConverter
    {
        public static (byte[], uint, uint?) Read(PvPMessage message)
        {
            var newMessage = message.Type switch
            {
                -6 => ReadControl(message),
                _ => ReadUnknown(message),
            };
            return (newMessage.ToByteArray(), message.Seq, message.Sseq != 0 ? message.Sseq : default);
        }

        private static NetworkingMessage ReadUnknown(PvPMessage message) => new NetworkingMessage() { Type = (uint)message.Type, Unknown = message.Body };

        public static NetworkingMessage ReadControl(PvPMessage message)
        {
            var span = message.Body.ToByteArray().AsSpan();
            var code = BinaryPrimitives.ReadUInt32LittleEndian(span[..4]);
            var command = Encoding.UTF8.GetString(span[4..]);
            return new NetworkingMessage() { Type = code, Control = command };
        }

        public static PvPMessage Write(byte[]? b, uint sequenceNo, uint? responseSequenceNo)
        {
            var raw = NetworkingMessage.Parser.ParseFrom(b);
            return raw.BodyCase switch
            {
                NetworkingMessage.BodyOneofCase.Unknown => WriteUnknown(raw, sequenceNo, responseSequenceNo),
                NetworkingMessage.BodyOneofCase.Control => WriteControl(raw, sequenceNo, responseSequenceNo),
                _ => throw new ArgumentOutOfRangeException(nameof(b)),
            };
        }

        private static PvPMessage WriteUnknown(NetworkingMessage raw, uint sequenceNo, uint? responseSequenceNo)
        {
            return new PvPMessage
            {
                Type = (int)raw.Type,
                Body = raw.Unknown,
                Seq = responseSequenceNo ?? 0,
                Sseq = sequenceNo,
            };
        }

        private static PvPMessage WriteControl(NetworkingMessage raw, uint sequenceNo, uint? responseSequenceNo)
        {
            var control = new byte[Encoding.UTF8.GetByteCount(raw.Control) + 4];
            BinaryPrimitives.WriteUInt32LittleEndian(control.AsSpan()[..4], raw.Type);
            Encoding.UTF8.GetBytes(raw.Control, control.AsSpan()[4..]);

            return new PvPMessage
            {
                Type = -6,
                Body = ByteString.CopyFrom(control),
                Seq = responseSequenceNo ?? 0,
                Sseq = sequenceNo,
            };
        }
    }
}
