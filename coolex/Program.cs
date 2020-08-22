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
