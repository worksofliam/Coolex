using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coolex
{
    class CoolexGenerator
    {
        private String Definition;
        private String Structure;
        private List<string> Output = new List<string>();
        private Dictionary<string, string> Config = new Dictionary<string, string>();

        public CoolexGenerator(string Definition, string Structure)
        {
            this.Definition = Definition;
            this.Structure = Structure;
            string[] data;

            if (File.Exists(Structure))
            {
                this.GenerateParser();
            }

            if (File.Exists(Definition))
            {
                foreach (string Line in File.ReadAllLines(Definition))
                {
                    if (Line.Trim() == "") continue;
                    if (Line.StartsWith("#")) continue;

                    data = Line.Split(new[] { ':' }, 2);
                    Config.Add(data[0], data[1]);
                }
            }
            else
            {
                WriteMessage("Error", "File not found: " + Definition);
            }
        }

        private void WriteMessage(string Prefix, string Text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(Prefix + ": ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Text);
        }

        public void CreateOutput()
        {
            List<string> TypeEnum = new List<string>();
            List<string> Pieces = new List<string>();
            string operators = "", string_literal = "", block_open = "", block_close = "";

            //Grab operators
            if (!Config.ContainsKey("OPERATORS"))
                WriteMessage("Error", "OPERATORS key required.");

            operators = Config["OPERATORS"];
            operators += ", \" \""; //Required space!!

            //Grab string constants
            if (Config.ContainsKey("STRING_LITERAL"))
            {
                string_literal = Config["STRING_LITERAL"];
                WriteMessage("Notice", "String constants found.");
            }

            if (Config.ContainsKey("BLOCK_OPEN"))
            {
                WriteMessage("Notice", "Open blocks found.");
                if (!Config.ContainsKey("BLOCK_CLOSE"))
                    WriteMessage("Error", "BLOCK_CLOSE is missing, which is required with BLOCK_OPEN.");

                block_open = Config["BLOCK_OPEN"];
            }

            if (Config.ContainsKey("BLOCK_CLOSE"))
            {
                WriteMessage("Notice", "Close blocks found.");
                if (!Config.ContainsKey("BLOCK_OPEN"))
                    WriteMessage("Error", "BLOCK_OPEN is missing, which is required with BLOCK_CLOSE.");

                block_close = Config["BLOCK_CLOSE"];
            }

            //Remove required configs so only user input appears
            //We do not remove BLOCK_OPEN and BLOCK_CLOSE so the lexer sees it as a token
            Config.Remove("OPERATORS");
            Config.Remove("STRING_LITERAL");

            //Add default and user enums
            TypeEnum.AddRange(new[] { "BLOCK", "UNKNOWN", "OPERATOR", "STRING_LITERAL" });
            TypeEnum.AddRange(Config.Keys);

            WriteMessage("Notice", "Generating enum Type:");
            foreach (string EnumValue in TypeEnum)
            {
                WriteMessage("\t", EnumValue);
            }

            Output.Add("using System;");
            Output.Add("using System.Collections.Generic;");
            Output.Add("using System.Linq;");
            Output.Add("using System.Text.RegularExpressions;");

            Output.Add("namespace coolex {");
            Output.Add("class CoolexLex {");

            //Write enum to output
            Output.Add("public enum Type {");
            Output.Add(String.Join(", ", TypeEnum));
            Output.Add("}");

            //Define operators
            Output.Add("private string[] OPERATORS = new[] {" + operators + "};");

            //Define valid string constants
            if (string_literal == "")
                Output.Add("private char[] STRING_LITERAL = new char[0];");
            else
                Output.Add("private char[] STRING_LITERAL = new[] {" + string_literal + "};");

            //Define BLOCK_OPEN
            if (block_open == "")
                Output.Add("private string[] BLOCK_OPEN = new string[0];");
            else
                Output.Add("private string[] BLOCK_OPEN = new[] {" + block_open + "};");

            //Define BLOCK_CLOSE
            if (block_open == "")
                Output.Add("private string[] BLOCK_CLOSE = new string[0];");
            else
                Output.Add("private string[] BLOCK_CLOSE = new[] {" + block_close + "};");

            //Define user pieces
            Output.Add("private Dictionary<Type, string[]> Pieces = new Dictionary<Type, string[]>");
            Output.Add("{");
            foreach (var Piece in Config)
                Pieces.Add("{ Type." + Piece.Key + ", new[] { " + Piece.Value + " } }");
            Output.Add(String.Join(",", Pieces));
            Output.Add("};");

            Output.AddRange(Properties.Resources.Template.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

            if (ParserOutput.Count > 0)
                Output.AddRange(ParserOutput);

            Output.Add("}");

            Output.AddRange(Properties.Resources.TypeClass.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

            Output.Add("}");

            string FileOut = Definition + ".cs";
            WriteMessage("Notice", "Writing file: " + FileOut);
            File.WriteAllLines(FileOut, Output);
        }

        private List<string> ParserOutput = new List<string>();
        public void GenerateParser()
        {
            string[] Contents = File.ReadAllLines(this.Structure);

            ParserOutput.Add("public static ParseError[] Parse(CoolexType[] tokens) {");
            ParserOutput.Add("int inner; bool doBreak; List<ParseError> errors = new List<ParseError>();");
            ParserOutput.Add("for (int i = 0; i < tokens.Length; i++) {");

            foreach (string Line in Contents)
            {
                if (Line.Trim() == "") continue;
                if (Line.StartsWith("#")) continue;
                ParserOutput.Add(GenerateSwitch(Line.Split(','), 0, false));
            }

            ParserOutput.Add("}");

            ParserOutput.Add("return errors.ToArray();");
            ParserOutput.Add("}");
        }

        private string GenerateSwitch(string[] allParts, int partIndex, bool hasErrorHandle)
        {
            string part = allParts[partIndex].Trim();

            bool isMany = false;
            string lastPart = "";

            if (part.StartsWith("[") && part.EndsWith("]"))
            {
                part = part.Trim('[', ']');
                isMany = true;
            }

            string code = "";
            bool isValue = (part.StartsWith("\""));
            List<string> parts = part.Split('|').ToList();

            if (hasErrorHandle)
            {
                code += "inner++;";
            }
            else
            {
                code += "inner = i;";
            }

            code += "if (inner < tokens.Length) {";

            if (isMany)
            {
                lastPart = parts.Last();
                parts.RemoveAt(parts.Count - 1);
                code += "doBreak = false;";
                code += "for (; inner < tokens.Length && doBreak == false; ) {";
            }

            code += "switch (tokens[inner]." + (isValue ? "Value" : "Type") + ") {";
            foreach (string value in parts)
            {
                code += "case " + (isValue ? "" : "Type.") + value + ":";
            }

            if ((partIndex + 1) < allParts.Length)
            {
                if (isMany)
                {
                    code += "/* all good babies */";
                    code += "inner++;";
                }
                else
                {
                    code += GenerateSwitch(allParts, partIndex + 1, true);
                }
            }
            else
            {
                code += "/* all good babies */";
            }
            code += "break;";

            if (isMany)
            {
                code += "case " + (isValue ? "" : "Type.") + lastPart + ":";
                code += "  /* all good babies */";
                code += "  doBreak = true; inner--;";
                code += "  break;";
            }

            if (hasErrorHandle)
            {
                code += "default: errors.Add(new ParseError(tokens[inner].Line, \"Expected " + String.Join(", ", parts).Replace("\"", "\\\"") + "\")); inner++; break;";
            }
            code += "}";

            if (isMany)
            {
                code += "}";
                code += GenerateSwitch(allParts, partIndex + 1, true);
            }

            code += "} else {";
            code += "errors.Add(new ParseError(tokens[tokens.Length - 1].Line, \"Expected " + String.Join(", ", parts).Replace("\"", "\\\"") + "\"));";
            code += "}";

            return code;
        }

        public string[] GetOutput()
        {
            return Output.ToArray();
        }
    }
}
