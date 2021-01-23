using SharpGL;
using System;
using Assimp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpGL.SceneGraph;
using System.Drawing;
using System.Drawing.Imaging;

namespace ShipSharpGL
{
    internal class AssimpScene : IDisposable
    {
        public Assimp.Scene Scene { get; private set; }

        private OpenGL gl;

        private DisplayList displayList;

        private string sceneFilePath;

        private string sceneFileName;

        /// <summary>
        ///	 Identifikator tekstura.
        /// </summary>
        private uint[] texIds;

        /// <summary>
        ///	 Mapiranje teksture na njen identifikator.
        /// </summary>
        private Dictionary<TextureSlot, uint> texMappings;

        public AssimpScene(string sceneFilePath, string sceneFileName, OpenGL gl)
        {
            this.gl = gl;
            this.sceneFilePath = sceneFilePath;
            this.sceneFileName = sceneFileName;
            this.texMappings = new Dictionary<TextureSlot, uint>();
        }

        public void Draw()
        {
            displayList.Call(gl);
        }

        public void LoadScene()
        {
            AssimpContext context = new AssimpContext();

            Scene = context.ImportFile(Path.Combine(sceneFilePath, sceneFileName));

            context.Dispose();
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }

        public void Initialize()
        {
            LoadTextures();

            displayList = new DisplayList();
            displayList.Generate(gl);
            displayList.New(gl, DisplayList.DisplayListMode.Compile);
            gl.PushAttrib(OpenGL.GL_ENABLE_BIT);
            gl.PushAttrib(OpenGL.GL_TEXTURE_BIT);
            gl.PushAttrib(OpenGL.GL_POLYGON_BIT);
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.FrontFace(OpenGL.GL_CCW);
            RenderNode(Scene.RootNode);
            gl.PopAttrib();
            gl.PopAttrib();
            gl.PopAttrib();
            gl.PopAttrib();
            displayList.End(gl);
        }

        private void LoadTextures()
        {
            int texCount = 0;
            foreach (Material material in Scene.Materials)
            {
                foreach (TextureSlot texSlot in material.GetAllMaterialTextures())
                {
                    texCount++;
                }
            }

            texIds = new uint[texCount];
            gl.GenTextures(texCount, texIds);
            int index = 0;
            foreach (Material material in Scene.Materials)
            {
                foreach (TextureSlot texSlot in material.GetAllMaterialTextures())
                {
                    texMappings[texSlot] = texIds[index];

                    // Pridruzi teksturu odgovarajucem identifikatoru.
                    gl.BindTexture(OpenGL.GL_TEXTURE_2D, texIds[index]);

                    // Formiranje putanje do fajla koji predstavlja teksturu.
                    string fileName = Path.Combine(sceneFilePath, texSlot.FilePath.StartsWith("/") ? texSlot.FilePath.Substring(1) : texSlot.FilePath);
                    if (!File.Exists(fileName))
                        throw new ArgumentException();

                    // Ucitavanje teksture iz datog fajla.
                    Bitmap textureBitmap = new Bitmap(fileName);
                    BitmapData textureData = textureBitmap.LockBits(new Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA8, textureData.Width, textureData.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, textureData.Scan0);

                    // Podesavanje filtriranja teksture.
                    gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                    gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

                    // Podesavanje ponavljanja teksture za dati materijal. 
                    if (texSlot.WrapModeU == TextureWrapMode.Clamp)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP);
                    if (texSlot.WrapModeV == TextureWrapMode.Clamp)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP);
                    if (texSlot.WrapModeU == TextureWrapMode.Wrap)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                    if (texSlot.WrapModeV == TextureWrapMode.Wrap)
                        gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                    // Oslobadjanje resursa teksture.
                    textureBitmap.UnlockBits(textureData);
                    textureBitmap.Dispose();

                    index++;
                }
            }
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
                    Material material = Scene.Materials[mesh.MaterialIndex];

                    ApplyMaterial(material);

                    if (material.GetAllMaterialTextures().Length > 0)
                        gl.BindTexture(OpenGL.GL_TEXTURE_2D, texMappings[material.GetAllMaterialTextures()[0]]);

                    bool hasNormals = mesh.HasNormals;
                    if (hasNormals)
                        gl.Enable(OpenGL.GL_LIGHTING);
                    else
                        gl.Disable(OpenGL.GL_LIGHTING);

                    bool hasColors = mesh.HasVertexColors(0);
                    if (hasColors)
                        gl.Enable(OpenGL.GL_COLOR_MATERIAL);
                    else
                        gl.Disable(OpenGL.GL_COLOR_MATERIAL);

                    bool hasTexCoords = material.GetAllMaterialTextures().Length > 0 && mesh.HasTextureCoords(0);
                    if (hasTexCoords)
                        gl.Enable(OpenGL.GL_TEXTURE_2D);
                    else
                        gl.Disable(OpenGL.GL_TEXTURE_2D);

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
                            int indice = face.Indices[i];
                            if (hasColors)
                            {
                                gl.Color(
                                    mesh.VertexColorChannels[0][indice].R,
                                    mesh.VertexColorChannels[0][indice].G,
                                    mesh.VertexColorChannels[0][indice].B,
                                    mesh.VertexColorChannels[0][indice].A
                                    );
                            }
                            if (hasNormals)
                                gl.Normal(mesh.Normals[indice].X, mesh.Normals[indice].Y, mesh.Normals[indice].Z);
                            if (hasTexCoords)
                                gl.TexCoord(mesh.TextureCoordinateChannels[0][indice].X, 1 - mesh.TextureCoordinateChannels[0][indice].Y);
                            gl.Vertex(mesh.Vertices[indice].X, mesh.Vertices[indice].Y, mesh.Vertices[indice].Z);
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

        private void ApplyMaterial(Material material)
        {
            // Primena ambijentalne komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] ambientColor = material.HasColorAmbient ? new float[] { material.ColorAmbient.R, material.ColorAmbient.G, material.ColorAmbient.B, material.ColorAmbient.A } : new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT, ambientColor);

            // Primena difuzne komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] diffuseColor = material.HasColorDiffuse ? new float[] { material.ColorDiffuse.R, material.ColorDiffuse.G, material.ColorDiffuse.B, material.ColorDiffuse.A } : new float[] { 0.8f, 0.8f, 0.8f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_DIFFUSE, diffuseColor);

            // Primena spekularne komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] specularColor = material.HasColorSpecular ? new float[] { material.ColorSpecular.R, material.ColorSpecular.G, material.ColorSpecular.B, material.ColorSpecular.A } : new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SPECULAR, specularColor);


            // Primena emisione komponente datog materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float[] emissiveColor = material.HasColorEmissive ? new float[] { material.ColorEmissive.R, material.ColorEmissive.G, material.ColorEmissive.B, material.ColorEmissive.A } : new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_EMISSION, emissiveColor);

            // Primena sjaja materijala. U slucaju da ista nije definisana, koristi se podrazumevana vrednost.
            float shininess = material.HasShininess ? material.Shininess : 1.0f;
            float strength = material.HasShininessStrength ? material.ShininessStrength : 1.0f;
            gl.Material(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_SHININESS, shininess * strength);
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
