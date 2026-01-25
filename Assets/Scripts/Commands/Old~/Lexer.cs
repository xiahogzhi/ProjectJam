// using System.Collections.Generic;
//
// namespace Commands
// {
//     public class Lexer
//     {
//         private string _text;
//         private int _position;
//         private List<Token> _tokens = new List<Token>();
//
//         char current
//         {
//             get
//             {
//                 if (_position >= _text.Length)
//                     return '\0';
//                 return _text[_position];
//             }
//         }
//
//         void Next()
//         {
//             _position++;
//         }
//
//
//         public Token[] Lex(string text)
//         {
//             if (string.IsNullOrEmpty(text))
//                 return null;
//
//             _text = text;
//             _position = 0;
//             _tokens.Clear();
//             int index = 0;
//
//             while (current != '\0')
//             {
//                 if (char.IsWhiteSpace(current))
//                 {
//                     var start = _position;
//                     while (char.IsWhiteSpace(current))
//                         Next();
//
//                     var t = _text.Substring(start, _position - start);
//                     _tokens.Add(new Token(t, t, Kind.Whitespace, index++));
//                 }
//
//                 if (current == '"')
//                 {
//                     Next();
//                     var start = _position;
//                     while (current != '"' && current != '\0')
//                         Next();
//
//                     if (_position == start)
//                         _tokens.Add(new Token("\"\"", string.Empty, Kind.String, index++));
//                     else
//                     {
//                         var t = text.Substring(start, _position - start);
//                         _tokens.Add(new Token($"\"{t}\"", t, Kind.String, index++));
//                     }
//
//                     Next();
//                 }
//                 else
//                 {
//                     var start = _position;
//                     while (!char.IsWhiteSpace(current) && current != '\0')
//                         Next();
//
//                     if (_position != start)
//                     {
//                         var t = text.Substring(start, _position - start);
//                         _tokens.Add(new Token(t, t, Kind.String, index++));
//                     }
//                 }
//             }
//
//             return _tokens.ToArray();
//         }
//     }
// }