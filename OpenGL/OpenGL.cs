using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;                  //add "OpenTK" as NuGet reference
using OpenTK.Graphics.OpenGL4; //add "OpenTK" as NuGet reference
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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
            }
        }
    }
}