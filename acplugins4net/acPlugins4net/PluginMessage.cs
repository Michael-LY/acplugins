﻿using acPlugins4net.kunos;
using System;
using System.IO;
using System.Text;

namespace acPlugins4net
{
    public abstract class PluginMessage
    {
        public ACSProtocol.MessageType Type { get; private set; }

        public PluginMessage(ACSProtocol.MessageType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            var s = "";
            foreach (var prop in this.GetType().GetProperties())
            {
                if (prop.Name != "StringRepresentation")
                    s += prop.Name + "=" + prop.GetValue(this) + Environment.NewLine;
            }
            return s;
        }

        protected internal abstract void Serialize(BinaryWriter bw);
        protected internal abstract void Deserialize(BinaryReader br);

        public byte[] ToBinary()
        {
            using (var m = new MemoryStream())
            using (var bw = new BinaryWriter(m))
            {
                bw.Write((byte)Type);
                Serialize(bw);
                return m.ToArray();
            }
        }

        public void FromBinary(byte[] data)
        {
            using (var m = new MemoryStream(data))
            using (var br = new BinaryReader(m))
            {
                var type = br.Read();
                if ((byte)Type != type)
                    throw new Exception("FromBinary() Type != type");

                Deserialize(br);
            }
        }

        #region Helpers: (write & read binary stuff)

        public struct Vector3f
        {
            public float x, y, z;
            private static Random R = new Random();

            public static Vector3f RandomSmall()
            {
                return new Vector3f()
                {
                    x = (float)(R.NextDouble() - 0.5) * 10,
                    y = (float)(R.NextDouble() - 0.5),
                    z = (float)(R.NextDouble() - 0.5) * 10,
                };
            }

            public static Vector3f RandomBig()
            {
                return new Vector3f()
                {
                    x = (float)(R.NextDouble() - 0.5) * 1000,
                    y = (float)(R.NextDouble() - 0.5) * 20,
                    z = (float)(R.NextDouble() - 0.5) * 1000,
                };
            }

            public override string ToString()
            {
                return "[" + x.ToString() + " , " + y.ToString() + " , " + z.ToString() + "]";
            }
        }

        protected static string readStringW(BinaryReader br)
        {
            // Read the length, 1 byte
            var length = br.ReadByte();

            // Read the chars
            return Encoding.UTF32.GetString(br.ReadBytes(length * 4));

        }

        protected static void writeStringW(BinaryWriter bw, string message)
        {
            bw.Write((byte)(message.Length));
            bw.Write(Encoding.UTF32.GetBytes(message));
        }

        protected static string readString(BinaryReader br)
        {
            // Read the length, 1 byte
            var length = br.ReadByte();

            // Read the chars
            return new string(br.ReadChars(length));

        }

        protected static void writeString(BinaryWriter bw, string message)
        {
            var array = message.ToCharArray();
            bw.Write((byte)array.Length);
            bw.Write(array);
        }

        protected static Vector3f readVector3f(BinaryReader br)
        {
            Vector3f res = new Vector3f();

            res.x = br.ReadSingle();
            res.y = br.ReadSingle();
            res.z = br.ReadSingle();

            return res;
        }

        protected static void writeVector3f(BinaryWriter bw, Vector3f vec)
        {
            bw.Write(vec.x);
            bw.Write(vec.y);
            bw.Write(vec.z);
        }

        #endregion
    }
}
