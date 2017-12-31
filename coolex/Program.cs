using System;

namespace coolex
{
    class Program
    {
        static void Main(string[] args)
        {
            /*CoolexLex lexer = new CoolexLex();

            lexer.Lex("1324 + 1 / 12 * 6 - 5 + 10 + 'HELLO WORLDs' - \"Hello again!!\"");

            foreach (var Token in lexer.TokenList)
            {
                Console.WriteLine(Token.Type.ToString() + " " + Token.Value);
            }*/

            string FileIn = String.Join(" ", args);
            
            CoolexGenerator gen = new CoolexGenerator(FileIn);
            gen.CreateOutput();

            //Console.ReadKey();
        }
    }


}
