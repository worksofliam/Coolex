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
            string operators = "", string_literal = "";

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

            //Remove required configs so only user input appears
            Config.Remove("OPERATORS");
            Config.Remove("STRING_LITERAL");

            //Add default and user enums
            TypeEnum.AddRange(new[] { "UNKNOWN", "OPERATOR", "STRING_LITERAL" });
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
