using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClipboardDetector.Providers
{
    public class ProviderBytes
    {
        public static void Load(string path, Dictionary<string, string> map)
        {
            var data = File.ReadAllBytes(path);

            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            var filesize = reader.ReadInt32();
            if (filesize != data.Length)
            {
                return;
            }
            //reader.ReadUInt16();
            // 所有语言文字
            var valCount = reader.ReadUInt16();
            var Values = new string[valCount];
            for (var i = 0; i < valCount; i++)
            {
                if (!ReadString(data, stream, out var str))
                    return;

                Values[i] = str;
            }

            // str key map
            var strKeyCount = reader.ReadUInt16();
            for (var i = 0; i < strKeyCount; i++)
            {
                if (!ReadString(data, stream, out var str))
                    return;

                int idx = reader.ReadUInt16();
                map[str] = Values[idx];
            }

            // int key map
            var intKeyCount = reader.ReadUInt16();
            for (var i = 0; i < intKeyCount; i++)
            {
                var key = reader.ReadUInt16();
                var idx = reader.ReadUInt16();
                map[$"language_{key}"] = Values[idx];
            }
        }

        private static bool ReadString(byte[] buffer, MemoryStream stream, out string result)
        {
            int pos = (int)stream.Position;
            var idx = Array.IndexOf<byte>(buffer, 0, pos);
            result = null;
            if (idx >= pos)
            {
                result = idx == pos ? "" : Encoding.UTF8.GetString(buffer, pos, idx - pos);
                stream.Position = idx + 1;
            }
            return idx >= 0;
        }
    }
}
