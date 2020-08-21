using System;
using System.Collections.Generic;
using System.Linq;

namespace coolex
{
    class Program
    {
        static void Main(string[] args)
        {
            string Definition = args[0];
            string Structure = (args.Length > 1 ? args[1] : "");

            CoolexGenerator gen = new CoolexGenerator(Definition, Structure);
            gen.CreateOutput();

            CoolexLex lex = new CoolexLex();

            List<String> lines = Properties.Resources.Example.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            int commentIndex;
            for(int i = lines.Count-1; i >= 0; i--)
            {
                if (lines[i].Contains("//"))
                {
                    commentIndex = lines[i].IndexOf("//");
                    if (lines[i].Substring(0, commentIndex).Count(f => f == '\'') % 2 == 0)
                        lines[i] = lines[i].Substring(0, commentIndex);
                }
            }

            lex.Lex(String.Join(" " + Environment.NewLine, lines));

            //PrintBlock(lex.GetTokens());

            Console.WriteLine("");

            foreach (ParseError error in CoolexLex.Parse(lex.GetTokens().ToArray()))
            {
                Console.WriteLine("Error " + error.Line.ToString() + ": " + error.Text);
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static int printIndex = -1;
        public static void PrintBlock(List<CoolexType> Block)
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
    }


}
