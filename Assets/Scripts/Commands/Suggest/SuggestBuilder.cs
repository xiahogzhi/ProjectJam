using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.Commands.Core;
using hyjiacan.py4n;

namespace Framework.Commands.Suggest
{
    public abstract class SuggestBuilder
    {
        public abstract string currentSuggestString { get; }
        public abstract object GetCurrentSuggestValue();
        public abstract int index { get; set; }
        public abstract int size { get; }
        public abstract object GetSuggestValue(int index);
        public abstract string GetSuggestName(int index);
        public abstract string BuildText(string content);
        public abstract void Search(string suggest);
        public abstract void ClearSearch();
        public abstract void MoveLast();
        public abstract void MoveNext();
    }

    public class SuggestBuilder<T> : SuggestBuilder
    {
        public List<SuggestData> suggestList { get; } = new();

        private int _index = -1;

        public override int index
        {
            get => _index;
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnSelectEvt?.Invoke(currentSuggestString, currentSuggestValue);
                }
            }
        }

        public override int size => suggestList.Count;


        public override string currentSuggestString
        {
            get
            {
                if (isSearchMode)
                {
                    if (_index >= searchResult.Count || _index < 0)
                        return null;

                    return searchResult[_index].name;
                }

                if (_index >= suggestList.Count || _index < 0)
                    return null;

                return suggestList[_index].name;
            }
        }

        public override object GetSuggestValue(int index)
        {
            if (index < 0 || index >= suggestList.Count)
                return null;

            return suggestList[index].value;
        }

        public override string GetSuggestName(int index)
        {
            if (index < 0 || index >= suggestList.Count)
                return null;

            return suggestList[index].name;
        }

        public override object GetCurrentSuggestValue()
        {
            return currentSuggestValue;
        }

        public T currentSuggestValue
        {
            get
            {
                if (isSearchMode)
                {
                    if (_index >= searchResult.Count || _index < 0)
                        return default;

                    return searchResult[_index].value;
                }

                if (_index >= suggestList.Count || _index < 0)
                    return default;

                return suggestList[_index].value;
            }
        }

        public event Action<string, T> OnSelectEvt;

        private List<SuggestData> searchResult { get; set; } = new();

        /// <summary>
        /// 搜索模式
        /// </summary>
        private bool isSearchMode { get; set; } = false;

        public override void Search(string suggest)
        {
            index = -1;
            isSearchMode = false;
            if (!string.IsNullOrEmpty(suggest))
            {
                searchResult.Clear();
                isSearchMode = true;
                var searchContent = suggest.ToLower();

                bool MacroCheck(string str1, string str2)
                {
                    if (!str1.StartsWith("#") && str2.StartsWith("#"))
                        return false;

                    if (str1.StartsWith("#") && !str2.StartsWith("#"))
                        return false;

                    return true;
                }


                foreach (var variable in suggestList)
                {
                    if (!MacroCheck(searchContent, variable.normal))
                        continue;

                    PinyinFormat format = PinyinFormat.WITHOUT_TONE | PinyinFormat.LOWERCASE |
                                          PinyinFormat.WITH_U_UNICODE;
                    if (FuzzySearch.Contains(searchContent, variable.normal, out var s))
                    {
                        variable.priority = s;
                        searchResult.Add(variable);
                    }
                    else if (FuzzySearch.Contains(searchContent, Pinyin4Net.GetPinyin(variable.normal, format), out s))
                    {
                        variable.priority = s;
                        searchResult.Add(variable);
                    }
                }

                searchResult = searchResult.OrderByDescending(x => x.priority).Select(x => x).ToList();

                // searchResult.Sort((x, y) =>
                // {
                //     var m1 = JaroWinklerSimilarity(searchContent, x.name);
                //     var m2 = JaroWinklerSimilarity(searchContent, y.name);
                //     if (Math.Abs(m1 - m2) < 0.000001)
                //     {
                //         return String.Compare(x.name, y.name, StringComparison.Ordinal);
                //         // return x.name.CompareTo(y.name);
                //     }
                //
                //     return m2.CompareTo(m1);
                // });
            }
        }

        // 计算Jaro相似度
        public static double JaroSimilarity(string s1, string s2)
        {
            int len1 = s1.Length;
            int len2 = s2.Length;
            if (len1 == 0 && len2 == 0)
                return 1.0;

            int matchDistance = Math.Max(len1, len2) / 2 - 1;
            bool[] s1Matches = new bool[len1];
            bool[] s2Matches = new bool[len2];

            int matches = 0;
            for (int i = 0; i < len1; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance + 1, len2);
                for (int j = start; j < end; j++)
                {
                    if (!s2Matches[j] && s1[i] == s2[j])
                    {
                        s1Matches[i] = true;
                        s2Matches[j] = true;
                        matches++;
                        break;
                    }
                }
            }

            if (matches == 0)
                return 0.0;

            int transpositions = 0;
            int k = 0;
            for (int i = 0; i < len1; i++)
            {
                if (s1Matches[i])
                {
                    while (!s2Matches[k])
                        k++;
                    if (s1[i] != s2[k])
                        transpositions++;
                    k++;
                }
            }

            double m = matches;
            double t = transpositions / 2.0;

            return (m / len1 + m / len2 + (m - t) / m) / 3.0;
        }

        // 计算Jaro-Winkler相似度
        public static double JaroWinklerSimilarity(string s1, string s2)
        {
            double jaroSimilarity = JaroSimilarity(s1, s2);

            int prefixLength = 0;
            int maxLength = Math.Min(s1.Length, s2.Length);
            for (int i = 0; i < maxLength; i++)
            {
                if (s1[i] == s2[i])
                    prefixLength++;
                else
                    break;
            }

            // Jaro-Winkler 系数
            double jaroWinklerFactor = 0.1;

            return jaroSimilarity + prefixLength * jaroWinklerFactor * (1 - jaroSimilarity);
        }

        static double CalculateSimilarity(string s1, string s2)
        {
            int[,] distance = new int[s1.Length + 1, s2.Length + 1];


            for (int i = 0; i <= s1.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int j = 0; j <= s2.Length; j++)
            {
                distance[0, j] = j;
            }

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(Math.Min(
                            distance[i - 1, j] + 1,
                            distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            int maxLen = Math.Max(s1.Length, s2.Length);
            return 1.0 - (double) distance[s1.Length, s2.Length] / maxLen;
        }

        public class SuggestData
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="normal">显示的文本</param>
            /// <param name="high">选中显示</param>
            /// <param name="name">名字</param>
            /// <param name="value">值</param>
            public SuggestData(string normal, string high, string name, T value)
            {
                this.normal = normal;
                this.high = high;
                this.name = name;
                this.value = value;
            }

            /// <summary>
            /// 显示的文本
            /// </summary>
            public string normal { set; get; }

            /// <summary>
            /// 选中显示的文本
            /// </summary>
            public string high { set; get; }

            /// <summary>
            /// 名字
            /// </summary>
            public string name { set; get; }

            /// <summary>
            /// 优先级
            /// </summary>
            public int priority { set; get; }

            /// <summary>
            /// 值
            /// </summary>
            public T value { set; get; }
        }

        public void AddSuggest(ISuggestItem item)
        {
            var comment = string.Empty;
            if (!string.IsNullOrEmpty(item.GetComment()))
            {
                comment = $"  <color=#8A8A8A><i>{item.GetComment()}</i></color>";
            }

            var normal = $"{item.GetName()}{comment}";
            var high = $"<b><color=#FFF46C>> {item.GetName()}{comment}</color></b>";
            suggestList.Add(new(normal, high, item.GetName(), (T) item));
        }

        public void AddSuggest(string normalSuggest, string selectSuggest, string suggestName,
            T value)
        {
            suggestList.Add(new(normalSuggest, selectSuggest, suggestName, value));
        }

        public void Clear()
        {
            suggestList.Clear();
        }

        public void SelectSuggestWithoutNotice(T u)
        {
            _index = -1;
            if (u == null)
                return;

            if (isSearchMode)
            {
                for (int i = 0; i < searchResult.Count; i++)
                {
                    var variable = searchResult[i];
                    if (Equals(variable.value, u))
                    {
                        _index = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < suggestList.Count; i++)
                {
                    var variable = suggestList[i];
                    if (Equals(variable.value, u))
                    {
                        _index = i;
                        break;
                    }
                }
            }

            return;
        }


        public void SelectSuggest(string text)
        {
            index = -1;
            if (string.IsNullOrEmpty(text))
                return;

            if (isSearchMode)
            {
                for (int i = 0; i < searchResult.Count; i++)
                {
                    var variable = searchResult[i];
                    if (variable.name == text)
                    {
                        index = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < suggestList.Count; i++)
                {
                    var variable = suggestList[i];
                    if (variable.name == text)
                    {
                        index = i;
                        break;
                    }
                }
            }
        }

        public int count
        {
            get
            {
                if (isSearchMode)
                {
                    return searchResult.Count;
                }

                return suggestList.Count;
            }
        }


        public override void MoveNext()
        {
            var i = _index + 1;

            if (isSearchMode)
            {
                if (i >= searchResult.Count)
                    i = 0;
            }
            else
            {
                if (i >= suggestList.Count)
                    i = 0;
            }

            this.index = i;
        }

        public override void MoveLast()
        {
            var i = _index - 1;

            if (isSearchMode)
            {
                if (i < 0)
                    i = searchResult.Count - 1;
            }
            else
            {
                if (i < 0)
                    i = suggestList.Count - 1;
            }

            this.index = i;
        }

        public override string BuildText(string content)
        {
            StringBuilder sb = new StringBuilder();
            var list = suggestList;
            if (isSearchMode)
            {
                list = searchResult;
            }

            var top = 0;
            var bottom = 15;

            if (_index > bottom)
            {
                var offset = _index - bottom;
                top += offset;
                bottom += offset;
            }

            for (int i = top; i < list.Count; i++)
            {
                if (i > bottom || i < 0)
                    break;
                var s = list[i];
                if (i == _index || content == s.name)
                {
                    sb.AppendLine(s.high);
                }
                else
                {
                    sb.AppendLine(s.normal);
                }
            }

            return sb.ToString();
        }

        public override void ClearSearch()
        {
            Search(null);
        }
    }
}