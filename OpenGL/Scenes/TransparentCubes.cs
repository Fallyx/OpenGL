using System;
using System.Drawing;
using OpenTK;                  //add "OpenTK" as NuGet reference
using OpenTK.Graphics.OpenGL4; //add "OpenTK" as NuGet reference
using OpenGL.Helpers;

namespace OpenGL.Scenes
{
    static class TransparentCubes
    {
        public static void Start()
        {
            Bitmap bMap = new Bitmap(@"Textures/bricks.jpg");

            using (var w = new GameWindow(720, 480, null, "ComGr", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible))
            {
                int hProgram = 0;

                float alpha = 0f;

                int vaoTriangle = 0;
                int[] triangleIndices = null;
                int vboTriangleIndices = 0;

                w.Load += (o, ea) =>
                {
                    //set up opengl
                    GL.ClearColor(0.5f, 0.5f, 0.5f, 0);
                    GL.ClearDepth(1f);
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthFunc(DepthFunction.Less);
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                    //load, compile and link shaders
                    //see https://www.khronos.org/opengl/wiki/Vertex_Shader
                    var VertexShaderSource = @"
                        #version 400 core

                        in vec3 pos;
                        in vec3 normals;
                        
                        uniform mat4 m;
                        uniform mat4 proj;

                        out vec3 norms;
                        out vec3 point;

                        void main()
                        {
                            gl_Position =  proj * vec4(pos,1);

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
            
                        in vec3 norms;
                        in vec3 point;
    
                        uniform vec4 oClr;

                        out vec4 color;

                        void main()
                        {
                            //vec4 clr = vec4(1, 0, 0, 0.5);   
                            vec4 tmpClr = oClr;

                            vec3 lPos = vec3(0, 0, 5);
                            vec4 lCol = vec4(0.8, 0.8, 0.8, 1);
                            vec3 eye = vec3(0, 0, 0);
                            vec3 PL = normalize(lPos - point);
                            

                            vec3 diff = vec3(0);
                            float nL = dot(norms, PL);
                            if(nL >= 0) { diff = lCol.xyz * tmpClr.xyz * nL; } 

                            vec3 viewDir = normalize(eye - point);
                            vec3 reflectDir = reflect(-PL, norms);
                            float fSpec = pow(max(dot(viewDir, reflectDir), 0.0), 128);
                            vec3 spec = 0.5 * fSpec * lCol.rgb;

                            color = vec4(diff + spec, oClr.a);
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

                    var normAttribIndex = GL.GetAttribLocation(hProgram, "normals");
                    if (normAttribIndex != -1)
                    {
                        GL.EnableVertexAttribArray(normAttribIndex);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
                        GL.VertexAttribPointer(normAttribIndex, 3, VertexAttribPointerType.Float, false, 0, 0);
                    }

                    //check for errors during all previous calls
                    var error = GL.GetError();
                    if (error != ErrorCode.NoError)
                        throw new Exception(error.ToString());
                };

                w.UpdateFrame += (o, fea) =>
                {
                    //perform logic

                    alpha += 0.2f * (float)fea.Time;
                };

                w.RenderFrame += (o, fea) =>
                {
                    //clear screen and z-buffer
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    //switch to our shader
                    GL.UseProgram(hProgram);

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

                    // Solid Cube
                    var translate = Matrix4.CreateTranslation(-1.3f, 0, 0);
                    var M = rotateX * rotateY * translate * modelView;

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

                    var clrSolid = new Vector4(1, 0, 0, 1);

                    var clrAttribIndex = GL.GetUniformLocation(hProgram, "oClr");
                    if (clrAttribIndex != -1)
                    {
                        GL.Uniform4(clrAttribIndex, ref clrSolid);
                    }

                    GL.Disable(EnableCap.CullFace);

                    //render our model
                    GL.BindVertexArray(vaoTriangle);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboTriangleIndices);
                    GL.DrawElements(PrimitiveType.Triangles, triangleIndices.Length, DrawElementsType.UnsignedInt, 0);
                    




                    // Transparent Cube
                    var rotateZ = Matrix4.CreateRotationZ(alpha);

                    M = rotateY * rotateZ * modelView;

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

                    clrAttribIndex = GL.GetUniformLocation(hProgram, "oClr");
                    if (clrAttribIndex != -1)
                    {
                        GL.Uniform4(clrAttribIndex, ref clrTransparent);
                    }

                    GL.Enable(EnableCap.CullFace);

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