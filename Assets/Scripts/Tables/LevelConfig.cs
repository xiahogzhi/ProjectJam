// 此文件由 Azcel 自动生成，请勿手动修改

using System;
using System.Collections.Generic;
using System.IO;
using Azcel;
using Azathrix.Framework.Core;

namespace Game.Tables
{
    public class LevelConfig : ConfigBase<int>
    {
        public override string ConfigName => "LevelConfig";

        public string SceneName { get; private set; }
        public int NextLevel { get; private set; }

        public override void Deserialize(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            SceneName = reader.ReadString();
            NextLevel = reader.ReadInt32();
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
            if (string.Equals(fieldName, "SceneName", StringComparison.OrdinalIgnoreCase))
            {
                return TryConvertValue(SceneName, out value);
            }
            if (string.Equals(fieldName, "NextLevel", StringComparison.OrdinalIgnoreCase))
            {
                return TryConvertValue(NextLevel, out value);
            }

            return false;
        }
    }

    public sealed class LevelConfigTable : ConfigTable<LevelConfig, int>, IConfigSchemaProvider
    {
        public int SchemaHash => -1513007383;
        public int SchemaFieldCount => 3;

    }
}
