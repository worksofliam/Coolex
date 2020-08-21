using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace coolex
{
    class CoolexLex
    {
        public enum Type
        {
            BLOCK, UNKNOWN, OPERATOR, STRING_LITERAL, BLOCK_OPEN, BLOCK_CLOSE, DCL, ENDDCL, OPERATION, BIF, SPECIAL, DIRECTIVE, DOUBLE_LITERAL, INT_LITERAL, WORD_LITERAL, EQUALS, PARMS, STMT_END, DOT, ADD, SUB, DIV, MUL, LESS_THAN, MORE_THAN, NOT, MT_EQUAL, LT_EQUAL
        }
        private string[] OPERATORS = new[] { "<>", "<=", ">=", ".", "(", ")", ";", ":", "=", "+", "/", "*", "-", "<", ">", " " };
        private char[] STRING_LITERAL = new[] { '\'' };
        private string[] BLOCK_OPEN = new[] { "(" };
        private string[] BLOCK_CLOSE = new[] { ")" };
        private Dictionary<Type, string[]> Pieces = new Dictionary<Type, string[]>
{
{ Type.BLOCK_OPEN, new[] { "(" } },{ Type.BLOCK_CLOSE, new[] { ")" } },{ Type.DCL, new[] { "DCL" } },{ Type.ENDDCL, new[] { "END" } },{ Type.OPERATION, new[] { "ENDIF", "ENDSL", "ENDDO", "ENDFOR", "ACQ", "ADD", "ADDDUR", "ALLOC", "ANDxx", "BEGSR", "BITOFF", "BITON", "CABxx", "CALL", "CALLB", "CALLP", "CASxx", "CAT", "CHAIN", "CHECK", "CHECKR", "CLEAR", "CLOSE", "COMMIT", "COMP", "DEALLOC", "DEFINE", "DELETE", "DIV", "DO", "DOU", "DOUxx", "DOW", "DOWxx", "DSPLY", "DUMP", "ELSE", "ELSEIF", "ENDyy", "ENDSR", "EVAL", "EVALR", "EVAL-CORR", "EXCEPT", "EXFMT", "EXSR", "EXTRCT", "FEOD", "FOR", "FORCE", "GOTO", "IF", "IFxx", "IN", "ITER", "KFLD", "KLIST", "LEAVE", "LEAVESR", "LOOKUP", "MHHZO", "MHLZO", "MLHZO", "MLLZO", "MONITOR", "MOVE", "MOVEA", "MOVEL", "MULT", "MVR", "NEXT", "OCCUR", "ON-ERROR", "ON-EXIT", "OPEN", "ORxx", "OTHER", "OUT", "PLIST", "POST", "READ", "READC", "READE", "READP", "READPE", "REALLOC", "REL", "RESET", "RETURN", "ROLBK", "SCAN", "SELECT", "SETGT", "SETLL", "SETOFF", "SETON", "SHTDN", "SORTA", "SQRT", "SUB", "SUBDUR", "SUBST", "TAG", "TEST", "TESTB", "TESTN", "TESTZ", "TIME", "UNLOCK", "UPDATE", "WHEN", "WHENxx", "WRITE", "XFOOT", "XLATE", "XML-INTO", "XML-SAX", "Z-ADD", "Z-SUB" } },{ Type.BIF, new[] { "/\\%\\S*/" } },{ Type.SPECIAL, new[] { "/\\*\\S*/" } },{ Type.DIRECTIVE, new[] { "/\\/\\S*/" } },{ Type.DOUBLE_LITERAL, new[] { @"/(?<=^| )\\d+(\\.\\d+)?(?=$| )/" } },{ Type.INT_LITERAL, new[] { "/^[-+]?\\d+$/" } },{ Type.WORD_LITERAL, new[] { "/.*?/" } },{ Type.EQUALS, new[] { "=" } },{ Type.PARMS, new[] { ":" } },{ Type.STMT_END, new[] { ";" } },{ Type.DOT, new[] { "." } },{ Type.ADD, new[] { "+" } },{ Type.SUB, new[] { "-" } },{ Type.DIV, new[] { "/" } },{ Type.MUL, new[] { "*" } },{ Type.LESS_THAN, new[] { "<" } },{ Type.MORE_THAN, new[] { ">" } },{ Type.NOT, new[] { "<>" } },{ Type.MT_EQUAL, new[] { ">=" } },{ Type.LT_EQUAL, new[] { "<=" } }
};


        //***************************************************
        private CoolexType TokenList = new CoolexType(Type.BLOCK, "", 0);
        public List<CoolexType> GetTokens() => TokenList.Block;

        //***************************************************
        private int printIndex = -1;
        public void PrintBlock(List<CoolexType> Block)
        {
            printIndex++;
            foreach (CoolexType Token in Block)
            {
                Console.WriteLine("".PadRight(printIndex, '\t') + Token.Type.ToString() + " " + Token.Value);

                if (Token.Block != null)
                    PrintBlock(Token.Block);

            }
            printIndex--;
        }


        //***************************************************
        private Boolean InString = false;
        private string token = "";
        private int cIndex = 0;
        private bool IsOperator = false;
        private int CurrentLine = 1;
        public void Lex(string Text)
        {
            TokenList.Block = new List<CoolexType>();
            while (cIndex < Text.Length)
            {
                IsOperator = false;

                if (cIndex + 2 > Text.Length)
                { }
                else
                {
                    if (Text.Substring(cIndex, 2) == Environment.NewLine)
                    {
                        CurrentLine++;
                        cIndex += 2;
                        continue;
                    }
                }

                if (InString == false)
                {
                    foreach (string Operator in OPERATORS)
                    {
                        if (cIndex + Operator.Length > Text.Length) continue;
                        if (Text.Substring(cIndex, Operator.Length) == Operator)
                        {
                            //Sort the old token before adding the operator
                            WorkToken();

                            //Insert new token (operator token)
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
                            //This means it's end of STRING_LITERAL, and must be added to token list
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

        private int BlockIndex = 0;
        private List<CoolexType> GetLastToken(int Direction = 0)
        {
            List<CoolexType> Result = TokenList.Block;

            BlockIndex += Direction;

            for (int levels = 0; levels < BlockIndex; levels++)
            {
                if (Result.Count() > 0)
                {
                    if (Result[Result.Count - 1].Block == null)
                        Result[Result.Count - 1].Block = new List<CoolexType>();

                    Result = Result[Result.Count - 1].Block;
                }
            }

            return Result;
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
                                if (System.Text.RegularExpressions.Regex.IsMatch(piece, Value.Trim('/')))
                                {
                                    GetLastToken().Add(new CoolexType(Piece.Key, piece, CurrentLine));
                                    return;
                                }
                            }
                            else
                            {
                                if (Value.ToUpper() == piece.ToUpper())
                                {
                                    if (BLOCK_OPEN.Contains(piece))
                                    {
                                        GetLastToken().Add(new CoolexType(Type.BLOCK, piece, CurrentLine));
                                        GetLastToken(1);
                                    }
                                    else if (BLOCK_CLOSE.Contains(piece))
                                    {
                                        GetLastToken(-1);
                                    }
                                    else
                                    {
                                        GetLastToken().Add(new CoolexType(Piece.Key, piece, CurrentLine));
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    GetLastToken().Add(new CoolexType(Type.STRING_LITERAL, piece, CurrentLine));
                }
            }

        }
        public static ParseError[] Parse(CoolexType[] tokens)
        {
            int inner; bool doBreak; List<ParseError> errors = new List<ParseError>();
            for (int i = 0; i < tokens.Length; i++)
            {
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.DCL: inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.SUB: inner++; if (inner < tokens.Length) { switch (tokens[inner].Value.ToUpper()) { case "F": case "S": case "DS": case "PARM": case "SUBF": case "PR": case "PI": case "C": case "PROC":/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected \"F\", \"S\", \"DS\", \"PARM\", \"SUBF\", \"PR\", \"PI\", \"C\", \"PROC\", got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected \"F\", \"S\", \"DS\", \"PARM\", \"SUBF\", \"PR\", \"PI\", \"C\", \"PROC\"")); } break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected SUB, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected SUB")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected DCL")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Value.ToUpper()) { case "F": case "S": case "DS": case "PARM": case "SUBF": case "PR": case "PROC": inner++; if (inner < tokens.Length) { doBreak = false; for (; inner < tokens.Length && doBreak == false;) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.BLOCK:/* all good babies */inner++; break; case Type.STMT_END:  /* all good babies */  doBreak = true; inner--; inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.STMT_END:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected STMT_END")); } break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); doBreak = true; inner++; break; } } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, BLOCK")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected \"F\", \"S\", \"DS\", \"PARM\", \"SUBF\", \"PR\", \"PROC\"")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Value.ToUpper()) { case "PI": inner++; if (inner < tokens.Length) { doBreak = false; for (; inner < tokens.Length && doBreak == false;) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.SPECIAL: case Type.ENDDCL: case Type.SUB: case Type.BLOCK:/* all good babies */inner++; break; case Type.STMT_END:  /* all good babies */  doBreak = true; inner--; inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.STMT_END:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected STMT_END")); } break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); doBreak = true; inner++; break; } } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, SPECIAL, ENDDCL, SUB, BLOCK")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected \"PI\"")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Value.ToUpper()) { case "C": inner++; if (inner < tokens.Length) { doBreak = false; for (; inner < tokens.Length && doBreak == false;) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.DOUBLE_LITERAL: case Type.INT_LITERAL: case Type.STRING_LITERAL:/* all good babies */inner++; break; case Type.STMT_END:  /* all good babies */  doBreak = true; inner--; inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.STMT_END:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected STMT_END")); } break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); doBreak = true; inner++; break; } } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, STRING_LITERAL")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected \"C\"")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.OPERATION: inner++; if (inner < tokens.Length) { doBreak = false; for (; inner < tokens.Length && doBreak == false;) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.DOUBLE_LITERAL: case Type.INT_LITERAL: case Type.STRING_LITERAL: case Type.BIF: case Type.BLOCK: case Type.EQUALS: case Type.DOT: case Type.ADD: case Type.SUB: case Type.DIV: case Type.MUL: case Type.LESS_THAN: case Type.MORE_THAN: case Type.NOT: case Type.MT_EQUAL: case Type.LT_EQUAL:/* all good babies */inner++; break; case Type.STMT_END:  /* all good babies */  doBreak = true; inner--; inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.STMT_END:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected STMT_END")); } break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected STMT_END, got " + tokens[inner].Value)); doBreak = true; inner++; break; } } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, STRING_LITERAL, BIF, BLOCK, EQUALS, DOT, ADD, SUB, DIV, MUL, LESS_THAN, MORE_THAN, NOT, MT_EQUAL, LT_EQUAL")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected OPERATION")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Value.ToUpper()) { case "BEGSR": inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.WORD_LITERAL:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected WORD_LITERAL, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected \"BEGSR\"")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.ADD: inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.DOUBLE_LITERAL: case Type.INT_LITERAL: case Type.STRING_LITERAL: case Type.BLOCK:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, STRING_LITERAL, BLOCK, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, STRING_LITERAL, BLOCK")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected ADD")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.SUB: inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.DOUBLE_LITERAL: case Type.INT_LITERAL: case Type.BLOCK:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, BLOCK, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, BLOCK")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected SUB")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.DIV: inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.DOUBLE_LITERAL: case Type.INT_LITERAL: case Type.BLOCK:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, BLOCK, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, BLOCK")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected DIV")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.MUL: inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.WORD_LITERAL: case Type.DOUBLE_LITERAL: case Type.INT_LITERAL: case Type.BLOCK:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, BLOCK, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected WORD_LITERAL, DOUBLE_LITERAL, INT_LITERAL, BLOCK")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected MUL")); }
                inner = i; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.BIF: inner++; if (inner < tokens.Length) { switch (tokens[inner].Type) { case Type.BLOCK:/* all good babies */break; default: errors.Add(new ParseError(tokens[inner].Line, "Expected BLOCK, got " + tokens[inner].Value)); inner++; break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected BLOCK")); } break; } } else { errors.Add(new ParseError(tokens[tokens.Length - 1].Line, "Expected BIF")); }
            }
            return errors.ToArray();
        }
    }
    class CoolexType
    {
        public List<CoolexType> Block;
        public CoolexLex.Type Type;
        public string Value;
        public int Line;

        public CoolexType(CoolexLex.Type type, string value, int line)
        {
            Type = type;
            Value = value;
            Line = line;
            Block = null;
        }
    }

    class ParseError
    {
        public int Line;
        public string Text;

        public ParseError(int line, string text)
        {
            this.Line = line;
            this.Text = text;
        }
    }
}
