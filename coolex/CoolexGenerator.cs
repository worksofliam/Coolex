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
        private String FileIn;
        private List<string> Output = new List<string>();
        private Dictionary<string, string> Config = new Dictionary<string, string>();

        public CoolexGenerator(string FileLoc)
        {
            FileIn = FileLoc;
            string[] data;
            if (File.Exists(FileLoc))
            {
                foreach (string Line in File.ReadAllLines(FileLoc))
                {
                    if (Line.Trim() == "") continue;
                    if (Line.StartsWith("#")) continue;

                    data = Line.Split(new[] { ':' }, 2);
                    Config.Add(data[0], data[1]);
                }
            }
            else
            {
                WriteMessage("Error", "File not found: " + FileLoc);
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

            Output.Add("}");

            Output.AddRange(Properties.Resources.TypeClass.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

            string FileOut = FileIn + ".cs";
            WriteMessage("Notice", "Writing file: " + FileOut);
            File.WriteAllLines(FileOut, Output);
        }

        public string[] GetOutput()
        {
            return Output.ToArray();
        }
    }
}
