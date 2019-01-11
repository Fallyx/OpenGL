using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;                  //add "OpenTK" as NuGet reference
using OpenTK.Graphics.OpenGL4; //add "OpenTK" as NuGet reference
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenGL
{
    static class Program
    {
        static void Main()
        {
            Bitmap bMap = new Bitmap(@"Textures/bricks.jpg");

            using (var w = new GameWindow(720, 480, null, "ComGr", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible))
            {
                int hProgram = 0;
                int hTxtr = 0;

                float alpha = 0f;

                int vaoTriangle = 0;
                int[] triangleIndices = null;
                int vboTriangleIndices = 0;

                double time = 0;

                w.Load += (o, ea) =>
                {
                    //set up opengl
                    GL.Enable(EnableCap.FramebufferSrgb);
                    GL.ClearColor(0.5f, 0.5f, 0.5f, 0);
                    GL.ClearDepth(1f);
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthFunc(DepthFunction.Less);
                    //GL.Enable(EnableCap.CullFace);
                    //GL.CullFace(CullFaceMode.Front);

                    //load, compile and link shaders
                    //see https://www.khronos.org/opengl/wiki/Vertex_Shader
                    var VertexShaderSource = @"
                        #version 400 core

                        in vec3 pos;
                        in vec3 colors;
                        in vec2 textureCoordinates;
                        in vec3 normals;
                        
                        uniform mat4 m;
                        uniform mat4 proj;                        

                        uniform float time;
                        out vec3 vColors;
                        out vec2 txtCoords;
                        out vec3 norms;
                        out vec3 point;

                        void main()
                        {
                            
                            gl_Position =  proj * vec4(pos,1);
                            //vec4 txtr = texture(bricks, textureCoordinates);

                            //vColors = colors;
                            //vColors = vec3(txtr.r, txtr.g, txtr.b);
                            txtCoords = textureCoordinates;
                            vec4 hNorm = vec4(normals, 0);
                            vec4 hPos = vec4(pos,1);
                            norms = (m * hNorm).xyz;
                            point = (m * hPos).xyz;
                        }
                        ";

                    var hVertexShader = GL.CreateShader(ShaderType.VertexShader);
                    GL.ShaderSource(hVertexShader, VertexShaderSource);
                    GL.CompileShader(hVertexShader);
                    GL.GetShader(hVertexShader, ShaderParameter.CompileStatus, out int status);
                    if (status != 1)
                        throw new Exception(GL.GetShaderInfoLog(hVertexShader));

                    //see https://www.khronos.org/opengl/wiki/Fragment_Shader
                    var FragmentShaderSource = @"
                        #version 400 core

                        in vec3 vColors;
                        in vec2 txtCoords;
                        in vec3 norms;
                        in vec3 point;
            
                        uniform sampler2D bricks;

                        out vec4 color;

                        void main()
                        {
                            vec4 txtColor = texture(bricks, txtCoords);
                            //color = vec4(txtColor.b, txtColor.g, txtColor.r, txtColor.a);
                            vec4 col = vec4(txtColor.b, txtColor.g, txtColor.r , 1.0);
                            //color = vec4(vColors, 1.0);

                            vec3 lPos = vec3(3, 3, 0);
                            vec3 eye = vec3(0, 0, 0);
                            vec3 lookAt = normalize(point - eye);
                            vec3 PL = normalize(lPos - point);

                            float diff = dot(PL, norms);
                            float diffuse = max(0, diff);

                            vec3 specDirection = normalize(norms * (2 * dot(norms, PL)) - PL);
                            vec3 spec = vec3(0.8) * pow(max(0.0, -dot(specDirection, lookAt)), 50);
                            //color = diffuse * col + vec4(spec, 1);
                            color = col;
                        }
                        ";
                    var hFragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                    GL.ShaderSource(hFragmentShader, FragmentShaderSource);
                    GL.CompileShader(hFragmentShader);
                    GL.GetShader(hFragmentShader, ShaderParameter.CompileStatus, out status);
                    if (status != 1)
                        throw new Exception(GL.GetShaderInfoLog(hFragmentShader));

                    //link shaders to a program
                    hProgram = GL.CreateProgram();
                    GL.AttachShader(hProgram, hFragmentShader);
                    GL.AttachShader(hProgram, hVertexShader);
                    GL.LinkProgram(hProgram);
                    GL.GetProgram(hProgram, GetProgramParameterName.LinkStatus, out status);
                    if (status != 1)
                        throw new Exception(GL.GetProgramInfoLog(hProgram));

                    //upload model vertices to a vbo

                    var triangleVertices = new float[]
                    {
                        // top
                        -1, -1, -1,
                        +1, -1, -1,
                        +1, +1, -1,
                        -1, +1, -1,

                        // bottom
                        -1, -1, +1,
                        +1, -1, +1,
                        +1, +1, +1,
                        -1, +1, +1,
                    };

                    var vboTriangleVertices = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
                    GL.BufferData(BufferTarget.ArrayBuffer, triangleVertices.Length * sizeof(float), triangleVertices, BufferUsageHint.StaticDraw);
                    
                    // upload model indices to a vbo
                    triangleIndices = new int[]
                    {
                        0, 1, 2, // top
                        0, 2, 3,
                        7, 6, 5, // bottom
                        7, 5, 4,
                        0, 3, 7, // left
                        0, 7, 4,
                        2, 1, 5, // right
                        2, 5, 6,
                        3, 2, 6, // front
                        3, 6, 7,
                        1, 0, 4, // back 
                        1, 4, 5,
                    };
                    
                    //triangleIndices = new int[] { 0, 1, 2, 3, 4, 5 };
                    vboTriangleIndices = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, triangleIndices.Length * sizeof(int), triangleIndices, BufferUsageHint.StaticDraw);
                    
                    var colors = new float[]
                    {
                        1, 0, 0,
                        1, 0, 0,
                        0, 1, 0,
                        0, 1, 0,

                        0, 0, 1,
                        0, 0, 1,
                        0, 0, 0,
                        0, 0, 0
                    };                   

                    var vboColor = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboColor);
                    GL.BufferData(BufferTarget.ArrayBuffer, colors.Length * sizeof(float), colors, BufferUsageHint.StaticDraw);

                    var textureCoords = new float[]
                    {
                        0, 0,
                        1, 0,
                        1, 1,
                        0, 1,

                        0, 0,
                        1, 0,
                        1, 1,
                        0, 1,

                        
                        0, 0,
                        0, 0,
                        0, 0,
                        0, 0,

                        0, 0,
                        0, 0,
                        0, 0,
                        0, 0,


                        0, 0,
                        0, 0,
                        0, 0,
                        0, 0,

                        0, 0,
                        0, 0,
                        0, 0,
                        0, 0,
                    };

                    var vboTexCoords = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                    GL.BufferData(BufferTarget.ArrayBuffer, textureCoords.Length * sizeof(float), textureCoords, BufferUsageHint.StaticDraw);

                    var normals = new float[]
                    {
                        +0, +0, -1,
                        +0, +0, -1,
                        +0, +0, -1,
                        +0, +0, -1,

                        +0, +0, +1,
                        +0, +0, +1,
                        +0, +0, +1,
                        +0, +0, +1,

                        -1, +0, +0,
                        -1, +0, +0,
                        -1, +0, +0,
                        -1, +0, +0,

                        +1, +0, +0,
                        +1, +0, +0,
                        +1, +0, +0,
                        +1, +0, +0,

                        +0, +1, +0,
                        +0, +1, +0,
                        +0, +1, +0,
                        +0, +1, +0,

                        +0, -1, +0,
                        +0, -1, +0,
                        +0, -1, +0,
                        +0, -1, +0,
                    };

                    var vboNormals = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
                    GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * sizeof(float), normals, BufferUsageHint.StaticDraw);

                    //set up a vao
                    vaoTriangle = GL.GenVertexArray();
                    GL.BindVertexArray(vaoTriangle);

                    var posAttribIndex = GL.GetAttribLocation(hProgram, "pos");
                    if (posAttribIndex != -1)
                    {
                        GL.EnableVertexAttribArray(posAttribIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
                        GL.VertexAttribPointer(posAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    var colAttribIndex = GL.GetAttribLocation(hProgram, "colors");
                    if(colAttribIndex != -1)
                    {
                        GL.EnableVertexAttribArray(colAttribIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboColor);
                        GL.VertexAttribPointer(colAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    var txtAttribIndex = GL.GetAttribLocation(hProgram, "textureCoordinates");
                    if (txtAttribIndex != -1)
                    {
                        GL.EnableVertexAttribArray(txtAttribIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                        GL.VertexAttribPointer(txtAttribIndex, 2, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    var normAttribIndex = GL.GetAttribLocation(hProgram, "normals");
                    if(normAttribIndex != -1)
                    {
                        GL.EnableVertexAttribArray(normAttribIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
                        GL.VertexAttribPointer(normAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    GL.GenTextures(1, out hTxtr);
                    GL.BindTexture(TextureTarget.Texture2D, hTxtr);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                    BitmapData data = bMap.LockBits(new Rectangle(0, 0, bMap.Width, bMap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    int bLen = Math.Abs(data.Stride) * data.Height;
                    byte[] imgData = new byte[bLen];
                    Marshal.Copy(data.Scan0, imgData, 0, bLen);

                    bMap.UnlockBits(data);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Srgb8, bMap.Width, bMap.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, PixelType.UnsignedByte, imgData);
                    //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Srgb8Alpha8, bMap.Width, bMap.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, imgData);


                    //check for errors during all previous calls
                    var error = GL.GetError();
                    if (error != ErrorCode.NoError)
                        throw new Exception(error.ToString());
                };

                w.UpdateFrame += (o, fea) =>
                {
                    //perform logic

                    time += fea.Time;
                    alpha += 0.01f;
                };

                w.RenderFrame += (o, fea) =>
                {
                    //clear screen and z-buffer
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    //switch to our shader
                    GL.UseProgram(hProgram);
                    var timeUniformIndex = GL.GetUniformLocation(hProgram, "time");
                    if (timeUniformIndex != -1)
                        GL.Uniform1(timeUniformIndex, (float)time);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, hTxtr);
                    var txtrUniformIndex = GL.GetUniformLocation(hProgram, "bricks");
                    if (txtrUniformIndex != -1)
                        GL.Uniform1(txtrUniformIndex, 0);
                    //GL.Uniform1(GL.GetAttribLocation(hProgram, "bricks"), 0);


                    var scale = Matrix4.CreateScale(0.5f);
                    var rotateY = Matrix4.CreateRotationY(alpha);
                    var zTrans = Matrix4.CreateTranslation(0f, 0f, -5f);
                    var perspective = Matrix4.CreatePerspectiveFieldOfView(45 * (float)(Math.PI / 180d), w.ClientRectangle.Width / (float)w.ClientRectangle.Height, 0.1f, 100f);

                    var M = scale * rotateY * zTrans;

                    var mAttribIndex = GL.GetUniformLocation(hProgram, "m");
                    if(mAttribIndex != -1)
                    {
                        GL.UniformMatrix4(mAttribIndex, false, ref M);
                    }

                    M *= perspective;
                    var projAttribIndex = GL.GetUniformLocation(hProgram, "proj");
                    if (projAttribIndex != -1)
                    {
                        GL.UniformMatrix4(projAttribIndex, false, ref M);
                    }

                    //render our model
                    GL.BindVertexArray(vaoTriangle);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                    GL.DrawElements(PrimitiveType.Triangles, triangleIndices.Length, DrawElementsType.UnsignedInt, 0);



                    var translate = Matrix4.CreateTranslation(-3f, 0, 0f);
                    var rotateX = Matrix4.CreateRotationX(alpha);

                    M = translate * scale * rotateX * zTrans * perspective;

                    projAttribIndex = GL.GetUniformLocation(hProgram, "proj");
                    if (projAttribIndex != -1)
                    {
                        GL.UniformMatrix4(projAttribIndex, false, ref M);
                    }

                    //render our model
                    GL.BindVertexArray(vaoTriangle);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                    GL.DrawElements(PrimitiveType.Triangles, triangleIndices.Length, DrawElementsType.UnsignedInt, 0);



                    translate = Matrix4.CreateTranslation(3f, 0, 0f);
                    rotateX = Matrix4.CreateRotationX(-alpha);

                    M = translate * scale * rotateX * zTrans * perspective;

                    projAttribIndex = GL.GetUniformLocation(hProgram, "proj");
                    if (projAttribIndex != -1)
                    {
                        GL.UniformMatrix4(projAttribIndex, false, ref M);
                    }

                    //render our model
                    GL.BindVertexArray(vaoTriangle);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                    GL.DrawElements(PrimitiveType.Triangles, triangleIndices.Length, DrawElementsType.UnsignedInt, 0);

                    //display
                    w.SwapBuffers();

                    var error = GL.GetError();
                    if (error != ErrorCode.NoError)
                        throw new Exception(error.ToString());
                };

                w.Resize += (o, ea) =>
                {
                    GL.Viewport(w.ClientRectangle);
                };

                w.Run();
            }
        }
    }
}