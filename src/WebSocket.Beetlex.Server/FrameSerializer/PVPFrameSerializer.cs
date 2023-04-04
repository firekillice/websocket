using System;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.WebSockets;
using Carbon.Match.Contact;
using Carbon.Match.Utility;
using Google.Protobuf;
using Server.Protocols;

namespace Carbon.Match.FrameSerializer
{
    public class PVPFrameSerializer : IDataFrameSerializer
    {
        public object? FrameDeserialize(DataFrame data, PipeStream stream, HttpRequest request)
        {
            ByteBlock bytes = default;
            try
            {
                if (data.Type == DataPacketType.binary)
                {
                    if (data.FIN == true)
                    {
                        bytes = stream.ReadBytes((int)data.Length);
                        return PvPMessage.Parser.ParseFrom(bytes.Data, bytes.Offset, bytes.Count);
                    }
                    else
                    {
                        LogUtility.SessionLog(LogType.Error, request.Session, "Serializer receive none-fin message{0}.", data.FIN);
                        return null;
                    }
                }
                else
                {
                    LogUtility.SessionLog(LogType.Debug, request.Session, "Serializer receive non-binary message{0}.", data.Type);
                    return null;
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public void FrameRecovery(byte[] buffer)
        {

        }

        public ArraySegment<byte> FrameSerialize(DataFrame packet, object body, HttpRequest request)
        {
            var pvpMessage = (PvPMessage)body;
            var bytes = pvpMessage.ToByteArray();
            var data = new ArraySegment<byte>(bytes, 0, bytes.Length);
            return data;
        }
    }
}
