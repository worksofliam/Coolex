using System;
using System.Collections.Generic;

namespace coolex
{
    class Program
    {
        static void Main(string[] args)
        {
            /*CoolexLex lex = new CoolexLex();

            lex.Lex("1 + 2 - 3 * 4 / { 5 + 5 } + 10 + {100 + {12 + 8}}");

            PrintBlock(lex.TokenList.Block);

            Console.ReadLine();*/

            string FileIn = String.Join(" ", args);

            CoolexGenerator gen = new CoolexGenerator(FileIn);
            gen.CreateOutput();
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
