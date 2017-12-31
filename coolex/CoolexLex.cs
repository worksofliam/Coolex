using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace coolex
{
    class CoolexType
    {
        public CoolexLex.Type Type;
        public string Value;

        public CoolexType(CoolexLex.Type type, string value)
        {
            Type = type;
            Value = value;
        }
    }

    class CoolexLex
    {
        public enum Type
        {
            UNKNOWN,
            OPERATOR,
            STRING_LITERAL,

            NUMERIC_LITERAL,
            PLUS,
            MINUS,
            DIVIDE,
            MULTIPLY
        }

        private string[] OPERATORS = new[] { "+", "-", "/", "*", " " };
        private char[] STRING_LITERAL = new[] { '"', '\'' };

        private Dictionary<Type, string[]> Pieces = new Dictionary<Type, string[]>
        {
            { Type.NUMERIC_LITERAL, new[] { "/[-0-9]+/" } },
            { Type.PLUS, new[] { "+" } },
            { Type.MINUS, new[] { "-" } },
            { Type.DIVIDE, new[] { "/" } },
            { Type.MULTIPLY, new[] { "*" } },
        };

        public List<CoolexType> TokenList = new List<CoolexType>();

        private Boolean InString = false;
        private string token = "";
        private int cIndex = 0;
        private bool IsOperator = false;
        public void Lex(string Text)
        {
            while (cIndex < Text.Length)
            {
                IsOperator = false;
                if (InString == false)
                {
                    foreach (string Operator in OPERATORS)
                    {
                        if (Text.Substring(cIndex, Operator.Length) == Operator)
                        {
                            //Sort the old token before adding the operator
                            WorkToken();

                            token = Text.Substring(cIndex, Operator.Length);
                            WorkToken();
                            cIndex += Operator.Length;
                            IsOperator = true;
                            break;
                        }
                    }
                }

                if (IsOperator == false)
                {
                    char c = Text.Substring(cIndex, 1).ToCharArray()[0];

                    if (STRING_LITERAL.Contains(c))
                    {
                        if (Text.Substring(cIndex - 1, 1) == "\\")
                            token += c;
                        else
                        {
                            WorkToken(InString);
                            InString = !InString;
                        }
                    }
                    else
                        token += c;


                    cIndex++;
                }
            }

            WorkToken();
        }

        public void WorkToken(Boolean stringToken = false)
        {
            string piece = token;
            token = "";

            if (piece != "")
            {
                if (stringToken == false)
                {
                    foreach (var Piece in Pieces)
                    {
                        foreach (string Value in Piece.Value)
                        {
                            if (Value.Length > 1 && Value.StartsWith("/") && Value.EndsWith("/") && !OPERATORS.Contains(piece))
                            {
                                if (Regex.IsMatch(piece, Value.Trim('/')))
                                {
                                    TokenList.Add(new CoolexType(Piece.Key, piece));
                                    return;
                                }
                            }
                            else
                            {
                                if (Value == piece)
                                {
                                    TokenList.Add(new CoolexType(Piece.Key, piece));
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    TokenList.Add(new CoolexType(Type.STRING_LITERAL, piece));
                }
            }

        }
    }
}
