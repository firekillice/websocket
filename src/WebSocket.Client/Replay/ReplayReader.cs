using Google.Protobuf;
using Server.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSocket.Client.Replay
{
    public static class ReplayReader
    {
        public static BiInputs Read(params string[] captures)
        {
            if (captures.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(captures));
            }
            var records = captures
                .Select((x, i) => ReadRaw(x, i == 0 ? ConnectionRole.Home : ConnectionRole.Away))
                .SelectMany(x => x)
                .Select(Convert)
                .Where(x => x.Message is not null)
                .OrderBy(x => x.TimestampMs)
                .ToArray();
            var sequences = new Dictionary<ConnectionRole, uint>();
            foreach (var record in records)
            {
                if (record.Direction == Directions.Incoming)
                {
                    record.SequenceNo = sequences.GetValueOrDefault(record.Role) + 1;
                    sequences[record.Role] = record.SequenceNo;
                }
            }

            return new BiInputs()
            {
                HomeInputs = records.Where(x => x.Direction == Directions.Incoming && x.Role == ConnectionRole.Home).ToImmutableArray(),
                AwayInputs = records.Where(x => x.Direction == Directions.Incoming && x.Role == ConnectionRole.Away).ToImmutableArray(),
            };
        }

        private static RawRecord[] ReadRaw(string capture, ConnectionRole role)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(capture));
            return document.RootElement.EnumerateArray().Select(x => ReadElement(x, role)).ToArray();
        }

        private static RawRecord ReadElement(JsonElement element, ConnectionRole role)
        {
            var record = new RawRecord
            {
                Role = role,
                Direction = element.GetProperty("dir").GetString(),
                TimeOnly = element.GetProperty("time").GetString(),
                Category = element.GetProperty("type").GetString(),
                Raw = element.GetProperty("raw").GetString(),
            };
            if (element.TryGetProperty("detail", out var details))
            {
                record.C = details.GetProperty("code").GetUInt32();
                record.S = details.TryGetProperty("cmd", out var c) && !string.IsNullOrWhiteSpace(c.GetString()) ? c.GetString() : string.Empty;
            }
            return record;
        }

        private static Record Convert(RawRecord raw)
        {
            var record = new Record
            {
                Role = raw.Role,
                Direction = raw.Direction == "send"
                    ? Directions.Incoming
                    : raw.Direction == "recv"
                        ? Directions.Outgoing
                        : throw new ArgumentOutOfRangeException(nameof(raw)),
                TimestampMs = DateTimeOffset.ParseExact($"2023/02/28 {raw.TimeOnly}", "yyyy/MM/dd HH:mm:ss.fff", CultureInfo.CurrentCulture).ToUnixTimeMilliseconds(),
                Message = raw.Category switch
                {
                    "Control" => new NetworkingMessage() { Type = raw.C, Control = raw.S },
                    "nostate.PitchStartInput" => new NetworkingMessage() { Type = 3050, Unknown = ByteString.FromBase64(raw.Raw) },
                    "nostate.PitchInput" => new NetworkingMessage() { Type = 3052, Unknown = ByteString.FromBase64(raw.Raw) },
                    "nostate.BatInput" => new NetworkingMessage() { Type = 3053, Unknown = ByteString.FromBase64(raw.Raw) },
                    "protocols.UploadEmulatorState" => new NetworkingMessage() { Type = 5001, Unknown = ByteString.FromBase64(raw.Raw) },
                    "protocols.StartMatchResp" => new NetworkingMessage() { Type = 10, Unknown = ByteString.FromBase64(raw.Raw) },
                    _ => null,
                },
            };
            return record;
        }

        private static CaptureUnit[] Split(Record[] records)
        {
            var units = new List<CaptureUnit>();
            var k = 0;
            while (true)
            {
                var inputs = records.Skip(k).TakeWhile(x => x.Direction == Directions.Incoming).ToImmutableArray();
                k += inputs.Length;
                var outputs = records.Skip(k).TakeWhile(x => x.Direction == Directions.Outgoing).ToImmutableArray();
                k += outputs.Length;

                units.Add(new() { Inputs = inputs, Outputs = outputs });

                if (outputs.Length == 0)
                {
                    break;
                }
            }
            return units.ToArray();
        }

        public enum Directions
        {
            Incoming,
            Outgoing
        }

        public sealed class RawRecord
        {
            public ConnectionRole Role { get; set; }

            public string Direction { get; set; }

            public string TimeOnly { get; set; }

            public string Category { get; set; }

            public uint C { get; set; }

            public string S { get; set; }

            public string Raw { get; set; }
        }

        public sealed class Record
        {
            public ConnectionRole Role { get; init; }

            public Directions Direction { get; init; }

            public long TimestampMs { get; init; }

            public NetworkingMessage Message { get; init; }

            public uint SequenceNo { get; set; }
        }

        public sealed class CaptureUnit
        {
            public ImmutableArray<Record> Inputs { get; init; }

            public ImmutableArray<Record> Outputs { get; init; }
        }

        public sealed class BiInputs
        {
            public ImmutableArray<Record> HomeInputs { get; init; }

            public ImmutableArray<Record> AwayInputs { get; init; }
        }
    }
}
