using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HFPS.Systems
{
    public class TextTableScriptable : TextTableAsset
    {
        public List<TextData> textTables = new List<TextData>();

        [Serializable]
        public sealed class TextData
        {
            public string Key;
            public string Text;
            public bool IsUppercase;
        }

        public override Dictionary<string, TextSourceTable> GetTextTable()
        {
            return textTables.ToDictionary(x => x.Key, y => new TextSourceTable()
            {
                IsUppercase = y.IsUppercase,
                Text = y.Text
            });
        }

        public override string GetText(string key)
        {
            foreach (var ttable in textTables)
            {
                if (ttable.Key.Equals(key))
                {
                    return ttable.Text;
                }
            }

            return string.Empty;
        }
    }

    public abstract class TextTableAsset : ScriptableObject
    {
        public abstract Dictionary<string, TextSourceTable> GetTextTable();

        public abstract string GetText(string key);
    }
}