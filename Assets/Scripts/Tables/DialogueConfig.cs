// 此文件由 Azcel 自动生成，请勿手动修改

using System;
using System.Collections.Generic;
using System.IO;
using Azcel;
using Azathrix.Framework.Core;

namespace Game.Tables
{
    public class DialogueConfig : ConfigBase<int>
    {
        public override string ConfigName => "DialogueConfig";

        public string text { get; private set; }
        public int next { get; private set; }
        public float duration { get; private set; }

        public override void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            text = reader.ReadString();
            next = reader.ReadInt32();
            duration = reader.ReadSingle();
        }

        protected override bool TryGetValueInternal<T>(string fieldName, out T value)
        {
            value = default;
            if (string.IsNullOrEmpty(fieldName))
                return false;

            if (string.Equals(fieldName, "Id", StringComparison.OrdinalIgnoreCase))
            {
                return TryConvertValue(Id, out value);
            }
            if (string.Equals(fieldName, "text", StringComparison.OrdinalIgnoreCase))
            {
                return TryConvertValue(text, out value);
            }
            if (string.Equals(fieldName, "next", StringComparison.OrdinalIgnoreCase))
            {
                return TryConvertValue(next, out value);
            }
            if (string.Equals(fieldName, "duration", StringComparison.OrdinalIgnoreCase))
            {
                return TryConvertValue(duration, out value);
            }

            return false;
        }
    }

    public sealed class DialogueConfigTable : ConfigTable<DialogueConfig, int>, IConfigSchemaProvider
    {
        public int SchemaHash => 780878413;
        public int SchemaFieldCount => 4;

    }
}
