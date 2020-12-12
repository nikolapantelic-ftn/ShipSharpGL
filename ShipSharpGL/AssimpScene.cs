using SharpGL;
using System;
using Assimp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpGL.SceneGraph;

namespace ShipSharpGL
{
    internal class AssimpScene : IDisposable
    {
        public Assimp.Scene Scene { get; private set; }

        private OpenGL gl;

        private DisplayList displayList;

        private string sceneFilePath;

        private string sceneFileName;

        public AssimpScene(string sceneFilePath, string sceneFileName, OpenGL gl)
        {
            this.gl = gl;
            this.sceneFilePath = sceneFilePath;
            this.sceneFileName = sceneFileName;
        }

        public void Draw()
        {
            displayList.Call(gl);
        }

        public void LoadScene()
        {
            AssimpContext context = new AssimpContext();
            Console.WriteLine(Path.Combine(sceneFilePath, sceneFileName));
            Scene = context.ImportFile(Path.Combine(sceneFilePath, sceneFileName));
            Material mat = new Material();
            string textureFilePath = Path.Combine(sceneFilePath, "boat_body_diffuse.jpg");
            mat.TextureDiffuse = new TextureSlot(textureFilePath, TextureType.Diffuse, 0, TextureMapping.FromUV, 0, 0f, TextureOperation.SmoothAdd, TextureWrapMode.Wrap, TextureWrapMode.Wrap, 0);
            Scene.Materials.Add(mat);
            Console.WriteLine(Scene.HasMaterials);
            context.Dispose();
        }

        public void Initialize()
        {
            displayList = new DisplayList();
            displayList.Generate(gl);
            displayList.New(gl, DisplayList.DisplayListMode.Compile);
            gl.Color(0.5, 0.5, 0.5);
            RenderNode(Scene.RootNode);
            displayList.End(gl);
        }

        private void RenderNode(Node node)
        {
            gl.PushMatrix();

            float[] matrix = new float[16] { 
                node.Transform.A1,
                node.Transform.B1,
                node.Transform.C1,
                node.Transform.D1,
                node.Transform.A2,
                node.Transform.B2,
                node.Transform.C2,
                node.Transform.D2,
                node.Transform.A3,
                node.Transform.B3,
                node.Transform.C3,
                node.Transform.D3,
                node.Transform.A4,
                node.Transform.B4,
                node.Transform.C4,
                node.Transform.D4
            };

            gl.MultMatrix(matrix);

            if (node.HasMeshes)
            {
                foreach (int meshIndex in node.MeshIndices)
                {
                    Mesh mesh = Scene.Meshes[meshIndex];
                    bool hasColors = mesh.HasVertexColors(0);
                    int indexCount = mesh.Faces[0].IndexCount;

                    foreach (Assimp.Face face in mesh.Faces)
                    {
                        switch (face.IndexCount)
                        {
                            case 1:
                                gl.Begin(OpenGL.GL_POINTS);
                                break;
                            case 2:
                                gl.Begin(OpenGL.GL_LINES);
                                break;
                            case 3:
                                gl.Begin(OpenGL.GL_TRIANGLES);
                                break;
                            default:
                                gl.Begin(OpenGL.GL_POLYGON);
                                break;
                        }

                        for (int i = 0; i < face.IndexCount; i++)
                        {
                            int vertexIndex = face.Indices[i];
                            if (hasColors)
                            {
                                gl.Color(
                                    mesh.VertexColorChannels[0][vertexIndex].R,
                                    mesh.VertexColorChannels[0][vertexIndex].G,
                                    mesh.VertexColorChannels[0][vertexIndex].B,
                                    mesh.VertexColorChannels[0][vertexIndex].A
                                    );
                            }
                            else
                            {
                                if (vertexIndex % 2 == 0)
                                {
                                    gl.Color(0.5, 0.5, 0.5);
                                }
                                else
                                {
                                    gl.Color(0.6, 0.6, 0.6);
                                }
                            }
                            gl.Vertex(mesh.Vertices[vertexIndex].X, mesh.Vertices[vertexIndex].Y, mesh.Vertices[vertexIndex].Z);
                        }
                        gl.End();
                    }
                }
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                RenderNode(node.Children[i]);
            }
            gl.PopMatrix();
        }

        ~AssimpScene()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                displayList.Delete(gl);
            }
        }
    }
}
