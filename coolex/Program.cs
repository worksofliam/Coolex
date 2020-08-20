using System;
using System.Collections.Generic;

namespace coolex
{
    class Program
    {
        static void Main(string[] args)
        {
            string Definition = args[0];
            string Structure = (args.Length > 1 ? args[1] : "");

            //CoolexGenerator gen = new CoolexGenerator(Definition, Structure);
            //gen.CreateOutput();
            //gen.GenerateParser();

            CoolexLex lex = new CoolexLex();

            lex.Lex("Dcl-S hello char(5); hello = 'hi' + 'world'; If abcd < 1244;");

            PrintBlock(lex.GetTokens());

            Console.WriteLine("");

            foreach (ParseError error in CoolexLex.Parse(lex.GetTokens().ToArray()))
            {
                Console.WriteLine("Error " + error.Line.ToString() + ": " + error.Text);
            }
            
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
