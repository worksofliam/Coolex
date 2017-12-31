using System;

namespace coolex
{
    class Program
    {
        static void Main(string[] args)
        {
            string FileIn = String.Join(" ", args);
            
            CoolexGenerator gen = new CoolexGenerator(FileIn);
            gen.CreateOutput();
        }
    }


}
