using System;
using System.Collections.Generic;
using Centaurus.Xdr;
using MessagePack;

namespace Centaurus.Models
{
    [MessagePackObject]
    [XdrContract]
    public abstract class BinaryData : IEquatable<BinaryData>
    {
        [IgnoreMember]
        public abstract int ByteLength { get; }

        private byte[] _Data;

        [Key(0)]
        [XdrField(0)]
        public byte[] Data
        {
            get
            {
                return _Data;
            }
            set
            {
                ByteArrayPrimitives.CheckBufferLength(value, ByteLength);
                _Data = value;
            }
        }

        public override int GetHashCode()
        {
            return ByteArrayPrimitives.GetHashCode(Data);
        }

        public override string ToString()
        {
            return Data.ToHex();
        }

        public byte[] ToArray()
        {
            return Data;
        }

        public bool Equals(BinaryData other)
        {
            if (other == null) return false;
            return ByteArrayPrimitives.Equals(Data, other.Data);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BinaryData)) return false;
            return Equals((BinaryData)obj);
        }
    }
}
