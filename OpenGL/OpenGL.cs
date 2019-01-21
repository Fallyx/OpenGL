using System;
using OpenGL.Scenes;

namespace OpenGL
{
    static class Program
    {
        static void Main()
        {
            string input = "";

            while (input != "q" && input != "Q")
            {
                Console.Clear();
                Console.WriteLine("Press a number to start a program:");
                Console.WriteLine("----------------------------------");
                Console.WriteLine("[1] Three Cubes");
                Console.WriteLine("[2] Textured Cube");
                Console.WriteLine("[3] Transparent Cubes");
                Console.WriteLine("[4] Transparent Texture");
                Console.WriteLine("[Q] Quit");
                Console.WriteLine("");

                input = Console.ReadLine();

                if (input == "1")
                {
                    Cubes.Start();
                }
                else if (input == "2")
                {
                    Textured.Start();
                }
                else if (input == "3")
                {
                    TransparentCubes.Start();
                }
                else if (input == "4")
                {
                    TransparentTexture.Start();
                }
            }
        }
    }
}