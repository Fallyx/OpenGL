﻿using System;
using System.Drawing;
using OpenTK;                  //add "OpenTK" as NuGet reference
using OpenTK.Graphics.OpenGL4; //add "OpenTK" as NuGet reference
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenGL.Helpers;

namespace OpenGL.Scenes
{
    class TransparentTexture
    {
        public static void Start()
        {
            Bitmap bMap = new Bitmap(@"Textures/tree.png");
            Bitmap bMapSolid = new Bitmap(@"Textures/bricks.png");

            using (var w = new GameWindow(720, 480, null, "ComGr", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible))
            {
                int hProgram = 0;
                int hTxtrSolid = 0;
                int hTxtrAlpha = 1;

                float alpha = 0f;

                int vaoTriangle = 0;
                int[] triangleIndices = null;
                int vboTriangleIndices = 0;

                w.Load += (o, ea) =>
                {
                    //set up opengl
                    GL.Enable(EnableCap.FramebufferSrgb);
                    GL.ClearColor(0.5f, 0.5f, 0.5f, 0);
                    GL.ClearDepth(1f);
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    GL.DepthMask(true);
                    GL.Disable(EnableCap.CullFace);

                    //load, compile and link shaders
                    //see https://www.khronos.org/opengl/wiki/Vertex_Shader
                    var VertexShaderSource = @"
                        #version 400 core

                        in vec3 pos;
                        in vec2 textureCoordinates;
                        in vec3 normals;
                        in int hasTexture;
                        
                        uniform mat4 m;
                        uniform mat4 proj;                        

                        out vec2 txtCoords;
                        out vec3 norms;
                        out vec3 point;

                        void main()
                        {
                            gl_Position =  proj * vec4(pos,1);

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

                        in vec2 txtCoords;
                        in vec3 norms;
                        in vec3 point;

                        uniform sampler2D txtr;

                        out vec4 color;

                        void main()
                        {
                            vec4 col = vec4(0);

                            vec4 txtColor = texture(txtr, txtCoords);
                            col = vec4(txtColor.b, txtColor.g, txtColor.r, txtColor.a);
                            

                            if(col.a < 0.5) { discard; }

                            vec3 lPos = vec3(0, 0, 5);
                            vec4 lCol = vec4(1);
                            vec3 eye = vec3(0, 0, 0);
                            vec3 PL = normalize(lPos - point);


                            vec3 diff = vec3(0);
                            float nL = dot(norms, PL);
                            if(col.a < 1) { nL = max(nL, 1 - nL); }
                            if(nL >= 0) { diff = lCol.xyz * col.xyz * nL; } 

                            vec3 viewDir = normalize(eye - point);
                            vec3 reflectDir = reflect(-PL, norms);
                            float fSpec = pow(max(dot(viewDir, reflectDir), 0.0), 128);
                            vec3 spec = 0.5 * fSpec * lCol.rgb;

                            color = vec4(0.2, 0.2, 0.2, 1) * col + vec4(diff, 1) + vec4(spec, 1);
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

                    var triangleVertices = OpenGLArrays.TriangleVertices();

                    var vboTriangleVertices = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboTriangleVertices);
                    GL.BufferData(BufferTarget.ArrayBuffer, triangleVertices.Length * sizeof(float), triangleVertices, BufferUsageHint.StaticDraw);

                    // upload model indices to a vbo
                    triangleIndices = OpenGLArrays.TriangleIndices();

                    vboTriangleIndices = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, triangleIndices.Length * sizeof(int), triangleIndices, BufferUsageHint.StaticDraw);

                    // upload texture coords to a vbo
                    var textureCoords = OpenGLArrays.TextureCoords();

                    var vboTexCoords = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                    GL.BufferData(BufferTarget.ArrayBuffer, textureCoords.Length * sizeof(float), textureCoords, BufferUsageHint.StaticDraw);

                    // upload normals to a vbo
                    var normals = OpenGLArrays.Normals();

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

                    var txtAttribIndex = GL.GetAttribLocation(hProgram, "textureCoordinates");
                    if (txtAttribIndex != -1)
                    {
                        GL.EnableVertexAttribArray(txtAttribIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                        GL.VertexAttribPointer(txtAttribIndex, 2, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    var normAttribIndex = GL.GetAttribLocation(hProgram, "normals");
                    if (normAttribIndex != -1)
                    {
                        GL.EnableVertexAttribArray(normAttribIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
                        GL.VertexAttribPointer(normAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    // Setup Texture
                    GL.GenTextures(0, out hTxtrSolid);
                    GL.BindTexture(TextureTarget.Texture2D, hTxtrSolid);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                    // Copy bitmapdata into a byte array
                    BitmapData dataSolid = bMapSolid.LockBits(new Rectangle(0, 0, bMapSolid.Width, bMapSolid.Height),
                ImageLockMode.ReadOnly, bMapSolid.PixelFormat);

                    int bLenSolid = Math.Abs(dataSolid.Stride) * dataSolid.Height;
                    byte[] imgDataSolid = new byte[bLenSolid];
                    Marshal.Copy(dataSolid.Scan0, imgDataSolid, 0, bLenSolid);

                    bMapSolid.UnlockBits(dataSolid);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Srgb8, bMap.Width, bMap.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, PixelType.UnsignedByte, imgDataSolid);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);


                    GL.GenTextures(1, out hTxtrAlpha);
                    GL.BindTexture(TextureTarget.Texture2D, hTxtrAlpha);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                    // Copy bitmapdata into a byte array
                    BitmapData data = bMap.LockBits(new Rectangle(0, 0, bMap.Width, bMap.Height),
                ImageLockMode.ReadOnly, bMap.PixelFormat);

                    int bLen = Math.Abs(data.Stride) * data.Height;
                    byte[] imgData = new byte[bLen];
                    Marshal.Copy(data.Scan0, imgData, 0, bLen);

                    bMap.UnlockBits(data);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Srgb8Alpha8, bMap.Width, bMap.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, imgData);
                    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                    //check for errors during all previous calls
                    var error = GL.GetError();
                    if (error != ErrorCode.NoError)
                        throw new Exception(error.ToString());
                };

                w.UpdateFrame += (o, fea) =>
                {
                    //perform logic

                    alpha += 0.5f * (float)fea.Time;
                };

                w.RenderFrame += (o, fea) =>
                {
                    //clear screen and z-buffer
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    //switch to our shader
                    GL.UseProgram(hProgram);

                    GL.DepthMask(true);
                    GL.Disable(EnableCap.Blend);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, hTxtrSolid);
                    var txtrUniformIndex = GL.GetUniformLocation(hProgram, "txtr");
                    if (txtrUniformIndex != -1)
                        GL.Uniform1(txtrUniformIndex, 0);

                    var scale = Matrix4.CreateScale(0.5f);
                    var rotateY = Matrix4.CreateRotationY(alpha);
                    var rotateX = Matrix4.CreateRotationX(alpha);

                    var modelView =
                        //model
                        Matrix4.Identity

                        //view
                        * Matrix4.LookAt(new Vector3(0, 0, -10), new Vector3(0, 0, 0), new Vector3(0, 1, 0)); //view
                    var projection =
                        //projection
                        Matrix4.CreatePerspectiveFieldOfView(45 * (float)(Math.PI / 180d), w.ClientRectangle.Width / (float)w.ClientRectangle.Height, 0.1f, 100f);

                    var M = rotateX * rotateY * modelView;

                    var mAttribIndex = GL.GetUniformLocation(hProgram, "m");
                    if (mAttribIndex != -1)
                    {
                        GL.UniformMatrix4(mAttribIndex, false, ref M);
                    }

                    var MVP = M * projection;
                    var projAttribIndex = GL.GetUniformLocation(hProgram, "proj");
                    if (projAttribIndex != -1)
                    {
                        GL.UniformMatrix4(projAttribIndex, false, ref MVP);
                    }

                    var hasTxtrAttribIndex = GL.GetUniformLocation(hProgram, "hasTexture");
                    if (hasTxtrAttribIndex != -1)
                    {
                        GL.Uniform1(hasTxtrAttribIndex, 0);
                    }

                    //render our model
                    GL.BindVertexArray(vaoTriangle);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                    GL.DrawElements(PrimitiveType.Triangles, triangleIndices.Length, DrawElementsType.UnsignedInt, 0);




                    // Transparent Cube

                    GL.Enable(EnableCap.Blend);
                    GL.DepthMask(true);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, hTxtrAlpha);
                    txtrUniformIndex = GL.GetUniformLocation(hProgram, "txtr");
                    if (txtrUniformIndex != -1)
                        GL.Uniform1(txtrUniformIndex, 0);

                    var translate = Matrix4.CreateTranslation(-1.5f, 0, 0);
                    var rotateZ = Matrix4.CreateRotationZ(alpha);

                    M = rotateY * rotateZ * translate * modelView;

                    mAttribIndex = GL.GetUniformLocation(hProgram, "m");
                    if (mAttribIndex != -1)
                    {
                        GL.UniformMatrix4(mAttribIndex, false, ref M);
                    }

                    MVP = M * projection;
                    projAttribIndex = GL.GetUniformLocation(hProgram, "proj");
                    if (projAttribIndex != -1)
                    {
                        GL.UniformMatrix4(projAttribIndex, false, ref MVP);
                    }

                    var clrTransparent = new Vector4(0, 1, 0, 0.5f);

                    hasTxtrAttribIndex = GL.GetUniformLocation(hProgram, "hasTexture");
                    if (hasTxtrAttribIndex != -1)
                    {
                        GL.Uniform1(hasTxtrAttribIndex, 1);
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
