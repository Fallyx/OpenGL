using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.Helpers
{
    static class OpenGLArrays
    {
        public static float[] TriangleVertices()
        {
            var triangleVertices = new float[]
            {               
                // top
               -1, +1, -1,
               +1, +1, -1,
               +1, +1, +1,
               -1, +1, +1,

               // bottom
               -1, -1, -1,
               +1, -1, -1,
               +1, -1, +1,
               -1, -1, +1,
               
                //left
                -1, -1, -1,
                -1, -1, +1,
                -1, +1, +1,
                -1, +1, -1,
                
                //right
                +1, -1, +1,
                +1, -1, -1,
                +1, +1, -1,
                +1, +1, +1,
                
                //front
                -1, -1, +1,
                +1, -1, +1,
                +1, +1, +1,
                -1, +1, +1,
                
                //back
                +1, -1, -1,
                -1, -1, -1,
                -1, +1, -1,
                +1, +1, -1,
                
            };

            return triangleVertices;
        }

        public static int[] TriangleIndices()
        {
            var triangleIndices = new int[]
            { 
                 0,  1,  2, // top
                 0,  2,  3,
                 7,  6,  5, // bottom
                 7,  5,  4,
                 8,  9, 10, // left
                 8, 10, 11,
                12, 13, 14, // right
                12, 14, 15,
                16, 17, 18, // front
                16, 18, 19,
                20, 21, 22, // back
                20, 22, 23,
                
                
            };

            return triangleIndices;
        }

        public static float[] Colors()
        {
            var colors = new float[]
            {
                // top
                0, 0, 1,
                0, 1, 1,
                1, 1, 0,
                1, 0, 0,

                // bottom
                1, 0, 0,
                1, 1, 0,
                0, 1, 1,
                0, 0, 1,

                // left
                1, 0, 0,
                0, 0, 1,
                1, 0, 0,
                0, 0, 1,

                // right
                0, 1, 1,
                1, 1, 0,
                0, 1, 1,
                1, 1, 0,

                // front
                0, 0, 1,
                0, 1, 1,
                1, 1, 0,
                1, 0, 0,

                // back
                1, 1, 0,
                1, 0, 0,
                0, 0, 1,
                0, 1, 1,
            };

            return colors;
        }

        public static float[] TextureCoords()
        {
            var textureCoords = new float[]
            {
                // top
                0, 1,
                1, 1,
                1, 0,
                0, 0,

                // bottom
                0, 1,
                1, 1,
                1, 0,
                0, 0,

                // left
                0, 1,
                1, 1,
                1, 0,
                0, 0,

                // right
                0, 1,
                1, 1,
                1, 0,
                0, 0,

                // front
                0, 1,
                1, 1,
                1, 0,
                0, 0,

                // back
                0, 1,
                1, 1,
                1, 0,
                0, 0,
            };

            return textureCoords;
        }

        public static float[] Normals()
        {
            var normals = new float[]
            {                
                // top
                +0, +1, +0,
                +0, +1, +0,
                +0, +1, +0,
                +0, +1, +0,

                // bottom
                +0, -1, +0,
                +0, -1, +0,
                +0, -1, +0,
                +0, -1, +0,

                // left
                -1, +0, +0,
                -1, +0, +0,
                -1, +0, +0,
                -1, +0, +0,

                // right
                +1, +0, +0,
                +1, +0, +0,
                +1, +0, +0,
                +1, +0, +0,

                // front
                +0, +0, +1,
                +0, +0, +1,
                +0, +0, +1,
                +0, +0, +1,

                // back
                +0, +0, -1,
                +0, +0, -1,
                +0, +0, -1,
                +0, +0, -1,
                
            };

            return normals;
        }
    }
}
