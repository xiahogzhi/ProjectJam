using UnityEngine;

namespace Framework.Commands.Core
{
    /// <summary>
    /// Compare strings and produce a distance score between them.
    /// </summary>
    public static class FuzzySearch
    {
        public static bool Contains(string searchTerm, string text)
        {
            if (string.IsNullOrEmpty(searchTerm) || text == null || text.Length == 0)
                return false;
            int index = 0;
            int num = 0;
            int length1 = searchTerm.Length;
            int length2 = text.Length;
            char c = searchTerm[index];
            while (c == ' ' && ++index < length1)
                c = searchTerm[index];
            bool flag1 = char.IsUpper(c);
            if (!flag1 && c >= 'A' && c <= 'Z')
                c += ' ';
            bool flag2 = false;
            do
            {
                char upper = text[num++];
                if (flag1)
                {
                    if (!flag2)
                        upper = char.ToUpper(upper);
                }
                else if (upper >= 'A' && upper <= 'Z')
                    upper += ' ';

                if ((int) c == (int) upper)
                {
                    ++index;
                    if (index < length1)
                    {
                        c = searchTerm[index];
                        while (c == ' ' && ++index < length1)
                            c = searchTerm[index];
                        flag1 = char.IsUpper(c);
                        if (!flag1 && c >= 'A' && c <= 'Z')
                            c += ' ';
                    }
                    else
                        break;
                }

                flag2 = char.IsLetter(upper);
            } while (num < length2);

            return index >= length1;
        }

        public static bool Contains(string searchTerm, string text, out int score)
        {
            score = 0;
            if (string.IsNullOrEmpty(searchTerm) || text == null || text.Length == 0)
                return false;
            int index = 0;
            int num1 = 0;
            int length1 = searchTerm.Length;
            int length2 = text.Length;
            char c = searchTerm[index];
            while (c == ' ' && ++index < length1)
                c = searchTerm[index];
            bool flag1 = char.IsUpper(c);
            if (!flag1 && c >= 'A' && c <= 'Z')
                c += ' ';
            int num2 = 50;
            bool flag2 = false;
            do
            {
                char upper = text[num1++];
                if (flag1)
                {
                    if (!flag2)
                    {
                        num2 = 50;
                        upper = char.ToUpper(upper);
                    }
                }
                else if (upper >= 'A' && upper <= 'Z')
                    upper += ' ';

                if ((int) c == (int) upper)
                {
                    score += num2;

                    num2 += 20;

                    ++index;
                    if (index < length1)
                    {
                        c = searchTerm[index];
                        while (c == ' ' && ++index < length1)
                            c = searchTerm[index];
                        flag1 = char.IsUpper(c);
                        if (!flag1 && c >= 'A' && c <= 'Z')
                            c += ' ';
                    }
                    else
                        break;
                }
                else if (!flag1 || char.IsUpper(upper))
                    num2 = flag2 ? 20 : 50;

                flag2 = char.IsLetter(upper);
            } while (num1 < length2);

            for (int i = 0; i < text.Length && i < searchTerm.Length; i++)
            {
                if (char.ToLower(searchTerm[i]) == char.ToLower(text[i]))
                {
                    var t = 1f - i / (float) text.Length;
                    score += (int) (Mathf.Pow(Mathf.Lerp(0, 3, t), 3));
                }
            }

            score -= length2 - index;
            return index >= length1;
        }
    }
}