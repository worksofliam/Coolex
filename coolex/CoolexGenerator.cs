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

        private bool insensitive = false;
        private List<string> Output = new List<string>();
        private Dictionary<string, string> Config = new Dictionary<string, string>();

        public CoolexGenerator(string Definition, string Structure)
        {
            this.Definition = Definition;
            this.Structure = Structure;
            string[] data;

            if (File.Exists(Definition))
            {
                foreach (string Line in File.ReadAllLines(Definition))
                {
                    if (Line.Trim() == "") continue;
                    if (Line.StartsWith("#")) continue;

                    data = Line.Split(new[] { ':' }, 2);
                    Config.Add(data[0], data[1].Trim());
                }


                if (Config.ContainsKey("SENSITIVE"))
                {
                    if (Config["SENSITIVE"] == "false")
                    {
                        this.insensitive = true;
                    }
                    Config.Remove("SENSITIVE");
                }
            }
            else
            {
                WriteMessage("Error", "File not found: " + Definition);
            }
            
            if (File.Exists(Structure))
            {
                this.GenerateParser();
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

            string Template = Properties.Resources.Template;

            //Is it a case insensitive language?
            if (this.insensitive)
            {
                Template = Template.Replace("(Value == piece)", "(Value.toUpperCase() == piece.toUpperCase())");
            }

            //Is it a case insensitive language?
            if (Config.ContainsKey("SEPERATE_BLOCK"))
            {
                if (Config["SEPERATE_BLOCK"] == "true")
                    Template = Template.Replace("//--SEPERATE_BLOCK--", "this.AddLastToken(0, new CoolexToken(Type.BLOCK, '', this.cIndex));");
                Config.Remove("SEPERATE_BLOCK");
            }


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

            //Write enum to output
            int enumValue = 0;
            Output.Add("const Type = {");
            foreach (string Type in TypeEnum)
            {
                Output.Add(Type + ": " + enumValue.ToString() + ",");
                enumValue++;
            }
            Output.Add("}");

            //Define operators
            Output.Add("const OPERATORS = [" + operators + "];");

            //Define valid string constants
            if (string_literal == "")
                Output.Add("const STRING_LITERAL = [];");
            else
                Output.Add("const STRING_LITERAL = [" + string_literal + "];");

            //Define BLOCK_OPEN
            if (block_open == "")
                Output.Add("const BLOCK_OPEN = [];");
            else
                Output.Add("const BLOCK_OPEN = [" + block_open + "];");

            //Define BLOCK_CLOSE
            if (block_open == "")
                Output.Add("const BLOCK_CLOSE = [];");
            else
                Output.Add("const BLOCK_CLOSE = [" + block_close + "];");

            //Define user pieces
            Output.Add("var Pieces = {}");
            foreach (var Piece in Config)
                Output.Add("Pieces[Type." + Piece.Key + "] = [" + Piece.Value + "];");

            Output.AddRange(Template.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

            Output.AddRange(Properties.Resources.TypeClass.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

            if (ParserOutput.Count > 0)
                Output.AddRange(ParserOutput);

            Output.Add("module.exports = {CoolexLexer, CoolexToken, ParseError, Parse};");

            string FileOut = Definition + ".js";
            WriteMessage("Notice", "Writing file: " + FileOut);
            File.WriteAllLines(FileOut, Output);
        }

        private List<string> ParserOutput = new List<string>();
        public void GenerateParser()
        {
            string[] Contents = File.ReadAllLines(this.Structure);

            ParserOutput.Add("function Parse(tokens) {");
            ParserOutput.Add("var inner; var doBreak = false; var errors = [];");
            ParserOutput.Add("for (var i = 0; i < tokens.length; i++) {");

            foreach (string Line in Contents)
            {
                if (Line.Trim() == "") continue;
                if (Line.StartsWith("#")) continue;
                ParserOutput.Add(GenerateSwitch(Line.Split(','), 0, false));
            }

            ParserOutput.Add("}");

            ParserOutput.Add("return errors;");
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

            code += "if (inner < tokens.length) {";

            if (isMany)
            {
                lastPart = parts.Last();
                parts.RemoveAt(parts.Count - 1);
                code += "doBreak = false;";
                code += "for (; inner < tokens.length && doBreak == false; ) {";
            }

            code += "switch (tokens[inner]." + (isValue ? "Value" + (this.insensitive ? ".toUpperCase()" : "") : "Type") + ") {";
            foreach (string value in parts)
            {
                code += "case " + (isValue ? "" : "Type.") + (this.insensitive ? value.ToUpper() : value) + ":";
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
                code += GenerateSwitch(allParts, partIndex + 1, true);
                code += "  break;";
            }

            if (hasErrorHandle)
            {
                if (isMany)
                {
                    code += "default: errors.push(new ParseError(tokens[inner].Index, \"Expected " + lastPart + ", got \" + tokens[inner].Value)); doBreak = true; inner++; break;";
                }
                else
                {
                    code += "default: errors.push(new ParseError(tokens[inner].Index, \"Expected " + String.Join(", ", parts).Replace("\"", "\\\"") + ", got \" + tokens[inner].Value)); inner++; break;";
                }
            }
            code += "}";

            if (isMany)
            {
                code += "}";
            }

            code += "} else {";
            code += "errors.push(new ParseError(tokens[tokens.Length - 1].Index, \"Expected " + String.Join(", ", parts).Replace("\"", "\\\"") + "\"));";
            code += "}";

            return code;
        }

        public string[] GetOutput()
        {
            return Output.ToArray();
        }
    }
}
