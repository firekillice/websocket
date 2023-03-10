﻿// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: protobuf
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace WebSocket.Contract
{

    /// <summary>Holder for reflection information generated from protobuf</summary>
    public static partial class ProtobufReflection
    {

        #region Descriptor
        /// <summary>File descriptor for protobuf</summary>
        public static pbr::FileDescriptor Descriptor
        {
            get { return descriptor; }
        }
        private static pbr::FileDescriptor descriptor;

        static ProtobufReflection()
        {
            byte[] descriptorData = global::System.Convert.FromBase64String(
                string.Concat(
                  "Cghwcm90b2J1ZhIUQ2FyYm9uLk1hdGNoLkNvbnRhY3QiUQoKUFZQTWVzc2Fn",
                  "ZRIMCgR0eXBlGAEgASgPEg0KBWZsYWdzGAIgASgHEgsKA3NlcRgDIAEoBxIM",
                  "CgRzc2VxGAQgASgHEgsKA3JhdxgFIAEoDGIGcHJvdG8z"));
            descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
                new pbr::FileDescriptor[] { },
                new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::WebSocket.Contract.PVPMessage), global::WebSocket.Contract.PVPMessage.Parser, new[]{ "Type", "Flags", "Seq", "Sseq", "Raw" }, null, null, null, null)
                }));
        }
        #endregion

    }
    #region Messages
    public sealed partial class PVPMessage : pb::IMessage<PVPMessage>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
#endif
    {
        private static readonly pb::MessageParser<PVPMessage> _parser = new pb::MessageParser<PVPMessage>(() => new PVPMessage());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pb::MessageParser<PVPMessage> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pbr::MessageDescriptor Descriptor
        {
            get { return global::WebSocket.Contract.ProtobufReflection.Descriptor.MessageTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        pbr::MessageDescriptor pb::IMessage.Descriptor
        {
            get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public PVPMessage()
        {
            OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public PVPMessage(PVPMessage other) : this()
        {
            type_ = other.type_;
            flags_ = other.flags_;
            seq_ = other.seq_;
            sseq_ = other.sseq_;
            raw_ = other.raw_;
            _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public PVPMessage Clone()
        {
            return new PVPMessage(this);
        }

        /// <summary>Field number for the "type" field.</summary>
        public const int TypeFieldNumber = 1;
        private int type_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int Type
        {
            get { return type_; }
            set
            {
                type_ = value;
            }
        }

        /// <summary>Field number for the "flags" field.</summary>
        public const int FlagsFieldNumber = 2;
        private uint flags_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public uint Flags
        {
            get { return flags_; }
            set
            {
                flags_ = value;
            }
        }

        /// <summary>Field number for the "seq" field.</summary>
        public const int SeqFieldNumber = 3;
        private uint seq_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public uint Seq
        {
            get { return seq_; }
            set
            {
                seq_ = value;
            }
        }

        /// <summary>Field number for the "sseq" field.</summary>
        public const int SseqFieldNumber = 4;
        private uint sseq_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public uint Sseq
        {
            get { return sseq_; }
            set
            {
                sseq_ = value;
            }
        }

        /// <summary>Field number for the "raw" field.</summary>
        public const int RawFieldNumber = 5;
        private pb::ByteString raw_ = pb::ByteString.Empty;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public pb::ByteString Raw
        {
            get { return raw_; }
            set
            {
                raw_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
            }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override bool Equals(object other)
        {
            return Equals(other as PVPMessage);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool Equals(PVPMessage other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (Type != other.Type) return false;
            if (Flags != other.Flags) return false;
            if (Seq != other.Seq) return false;
            if (Sseq != other.Sseq) return false;
            if (Raw != other.Raw) return false;
            return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override int GetHashCode()
        {
            int hash = 1;
            if (Type != 0) hash ^= Type.GetHashCode();
            if (Flags != 0) hash ^= Flags.GetHashCode();
            if (Seq != 0) hash ^= Seq.GetHashCode();
            if (Sseq != 0) hash ^= Sseq.GetHashCode();
            if (Raw.Length != 0) hash ^= Raw.GetHashCode();
            if (_unknownFields != null)
            {
                hash ^= _unknownFields.GetHashCode();
            }
            return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override string ToString()
        {
            return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void WriteTo(pb::CodedOutputStream output)
        {
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
            output.WriteRawMessage(this);
#else
      if (Type != 0) {
        output.WriteRawTag(13);
        output.WriteSFixed32(Type);
      }
      if (Flags != 0) {
        output.WriteRawTag(21);
        output.WriteFixed32(Flags);
      }
      if (Seq != 0) {
        output.WriteRawTag(29);
        output.WriteFixed32(Seq);
      }
      if (Sseq != 0) {
        output.WriteRawTag(37);
        output.WriteFixed32(Sseq);
      }
      if (Raw.Length != 0) {
        output.WriteRawTag(42);
        output.WriteBytes(Raw);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
#endif
        }

#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output)
        {
            if (Type != 0)
            {
                output.WriteRawTag(13);
                output.WriteSFixed32(Type);
            }
            if (Flags != 0)
            {
                output.WriteRawTag(21);
                output.WriteFixed32(Flags);
            }
            if (Seq != 0)
            {
                output.WriteRawTag(29);
                output.WriteFixed32(Seq);
            }
            if (Sseq != 0)
            {
                output.WriteRawTag(37);
                output.WriteFixed32(Sseq);
            }
            if (Raw.Length != 0)
            {
                output.WriteRawTag(42);
                output.WriteBytes(Raw);
            }
            if (_unknownFields != null)
            {
                _unknownFields.WriteTo(ref output);
            }
        }
#endif

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int CalculateSize()
        {
            int size = 0;
            if (Type != 0)
            {
                size += 1 + 4;
            }
            if (Flags != 0)
            {
                size += 1 + 4;
            }
            if (Seq != 0)
            {
                size += 1 + 4;
            }
            if (Sseq != 0)
            {
                size += 1 + 4;
            }
            if (Raw.Length != 0)
            {
                size += 1 + pb::CodedOutputStream.ComputeBytesSize(Raw);
            }
            if (_unknownFields != null)
            {
                size += _unknownFields.CalculateSize();
            }
            return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(PVPMessage other)
        {
            if (other == null)
            {
                return;
            }
            if (other.Type != 0)
            {
                Type = other.Type;
            }
            if (other.Flags != 0)
            {
                Flags = other.Flags;
            }
            if (other.Seq != 0)
            {
                Seq = other.Seq;
            }
            if (other.Sseq != 0)
            {
                Sseq = other.Sseq;
            }
            if (other.Raw.Length != 0)
            {
                Raw = other.Raw;
            }
            _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(pb::CodedInputStream input)
        {
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
            input.ReadRawMessage(this);
#else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 13: {
            Type = input.ReadSFixed32();
            break;
          }
          case 21: {
            Flags = input.ReadFixed32();
            break;
          }
          case 29: {
            Seq = input.ReadFixed32();
            break;
          }
          case 37: {
            Sseq = input.ReadFixed32();
            break;
          }
          case 42: {
            Raw = input.ReadBytes();
            break;
          }
        }
      }
#endif
        }

#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input)
        {
            uint tag;
            while ((tag = input.ReadTag()) != 0)
            {
                switch (tag)
                {
                    default:
                        _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
                        break;
                    case 13:
                        {
                            Type = input.ReadSFixed32();
                            break;
                        }
                    case 21:
                        {
                            Flags = input.ReadFixed32();
                            break;
                        }
                    case 29:
                        {
                            Seq = input.ReadFixed32();
                            break;
                        }
                    case 37:
                        {
                            Sseq = input.ReadFixed32();
                            break;
                        }
                    case 42:
                        {
                            Raw = input.ReadBytes();
                            break;
                        }
                }
            }
        }
#endif

    }

    #endregion

}

#endregion Designer generated code