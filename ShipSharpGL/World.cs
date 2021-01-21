using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Assets;
using SharpGL.SceneGraph.Cameras;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;

namespace ShipSharpGL
{
    class World : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public float RotationX { get; set; }
        public float RotationY { get; set; }

        public float TranslateX { get; set; }
        public float TranslateZ { get; set; }

        public float LightZ { get; set; } = 0;
        public float LightY { get; set; } = 150;
        public float LightX { get; set; } = 0;

        public float RampPercentage { get; set; } = 0;
        public float PostPercentage { get; set; } = 0;

        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }

        private Vertex position;
        private Vertex target;
        private Vertex upVector;

        public Vertex direction;

        private LookAtCamera cam;
        private float walkSpeed = 0.1f;
        double horizontalAngle = 3.3f;
        double verticalAngle = 0f;

        private Vertex right;
        private Vertex up;



        private enum TextureObjects { Wood = 0, Metal, Water, Boat }
        public int TextureCount => Enum.GetNames(typeof(TextureObjects)).Length;

        private uint[] textures = null;
        private string[] textureFiles = {
            System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"wood.jpg"),
            System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "metal.jpg"),
            System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "water.jpg"),
            System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Ship", "boat_body_diffuse.jpg")
        };

        public AssimpScene Scene { get; private set; }

        public World(string sceneFilePath, string sceneFileName, int width, int height, OpenGL gl)
        {
            Scene = new AssimpScene(sceneFilePath, sceneFileName, gl);
            Width = width;
            Height = height;
            textures = new uint[TextureCount];
        }

        ~World()
        {
            Dispose(false);
        }

        private void DrawSea(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0, -300, 0);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Water]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(10, 10, 10);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0f, 1f, 0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-5000, 0, 5000);
            gl.TexCoord(0.0f, 1f);
            gl.Vertex(5000, 0, 5000);
            gl.TexCoord(1f, 1f);
            gl.Vertex(5000, 0, -5000);
            gl.TexCoord(1f, 0f);
            gl.Vertex(-5000, 0, -5000);

            gl.End();

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(1f, 1f, 1f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PopMatrix();
        }

        private void DrawDock(OpenGL gl)
        {

            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Wood]);
            Cube dock = new Cube();
            gl.Translate(420, 240, 800);

            DockLight(gl);
            gl.Scale(100, 5, 400);
            dock.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Metal]);

            Cube ramp = new Cube();
            //Ramp

            gl.Translate(420, 280, 340);
            gl.Translate(0, -35, 100);
            gl.Rotate(20 + (60 * RampPercentage / 100), 0, 0);
            gl.Translate(0, 0, -100);
            gl.Scale(60, 5, 100);
            gl.Color(0.6, 0.4, 0.4);
            ramp.Render(gl, RenderMode.Render);
            gl.PopMatrix();


            float baseRadius = 10.0f;
            float height = 300.0f;

            gl.PushMatrix();

            gl.Translate(315, 260 + PostPercentage, 500);
            gl.Rotate(90, 0, 0);
            Cylinder cyl = new Cylinder();
            Disk d = new Disk();
            d.OuterRadius = baseRadius;
            cyl.Height = height;
            cyl.BaseRadius = baseRadius;
            cyl.TopRadius = baseRadius;
            d.CreateInContext(gl);
            cyl.CreateInContext(gl);
            cyl.Render(gl, RenderMode.Render);
            gl.PushMatrix();
            gl.Rotate(180, 0, 0);
            d.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Translate(0, 300, 0);
            cyl.Render(gl, RenderMode.Render);
            gl.PushMatrix();
            gl.Rotate(180, 0, 0);
            d.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Translate(0, 300, 0);
            cyl.Render(gl, RenderMode.Render);
            gl.PushMatrix();
            gl.Rotate(180, 0, 0);
            d.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Translate(210, 0, 0);
            cyl.Render(gl, RenderMode.Render);
            gl.PushMatrix();
            gl.Rotate(180, 0, 0);
            d.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Translate(0, -300, 0);
            cyl.Render(gl, RenderMode.Render);
            gl.PushMatrix();
            gl.Rotate(180, 0, 0);
            d.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.Translate(0, -300, 0);
            cyl.Render(gl, RenderMode.Render);
            gl.PushMatrix();
            gl.Rotate(180, 0, 0);
            d.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PopMatrix();
        }

        private void DockLight(OpenGL gl)
        {
            float[] light1pos = new float[] { 0f, 200f, 0f, 1f };
            float[] light1ambient = new float[] { 0.1f * (Red / 255), 0.1f * (Green / 255), 0.1f * (Blue / 255), 1.0f };
            float[] light1diffuse = new float[] { 0.8f * (Red / 255), 0.8f * (Green / 255), 0.8f * (Blue / 255), 1.0f };
            float[] light1specular = new float[] { Red / 255, Green / 255, 1f * Blue / 255, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 25.0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_EXPONENT, 0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, new float[] { 0f, -1f, 0f });
            gl.Enable(OpenGL.GL_LIGHT1);

            //Kocka koja predstavlja izvor svetlosti
            gl.PushMatrix();
            Cube c = new Cube();
            gl.Color(0.9, 0.9, 0.9);
            gl.Translate(0f, 600f, 0f);
            gl.Scale(20, 20, 20);
            //c.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        private void DrawText(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-1f, -1f, -1f, -1f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Color(1, 1f, 0);
            gl.Translate(0.3, -0.6, 0);
            gl.Scale(0.05, 0.07, 0);

            //Pisanje teksta
            gl.PushMatrix();
            gl.DrawText3D("Arial", 10f, 0.1f, "Predmet: Racunaraska grafika");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0, -1, 0);
            gl.DrawText3D("Arial", 10f, 0.1f, "Sk.god: 2020/21.");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0, -2, 0);
            gl.DrawText3D("Arial", 10f, 0.1f, "Ime: Nikola");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0, -3, 0);
            gl.DrawText3D("Arial", 10f, 0.1f, "Prezime: Pantelic");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0, -4, 0);
            gl.DrawText3D("Arial", 10f, 0.1f, "Sifra zad: 10.2");
            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)Width / Height, 1, 10000);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
        }

        public void SetupLighting(OpenGL gl)
        {
            /*float[] global_ambient = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);*/
            float[] light0pos = new float[] { LightX, LightY, LightZ, 1f };
            float[] light0ambient = new float[] { 0.8f, 0.8f, 0.6f, 1.0f };
            float[] light0diffuse = new float[] { 0.8f, 0.8f, 0.6f, 1.0f };
            float[] light0specular = new float[] { 1f, 1f, 0.7f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);


            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.ShadeModel(OpenGL.GL_FLAT);
        }

        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            DrawText(gl);

            gl.LoadIdentity();

            
            gl.FrontFace(OpenGL.GL_CCW);

            gl.LookAt(position.X, position.Y, position.Z, target.X, target.Y, target.Z, upVector.X, upVector.Y, upVector.Z);
            
            gl.PushMatrix();
            
            gl.PushMatrix();
            Cube cube = new Cube();
            gl.Color(0.9, 0.9, 0.9);
            gl.Translate(LightX, LightY, LightZ);
            gl.Scale(0.1, 0.1, 0.1);
            //cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.Scale(0.1, 0.1, 0.1);
            gl.PushMatrix();
            gl.Rotate(-90, 0, 0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            Scene.Draw();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.PopMatrix();
            
            DrawDock(gl);

            gl.PushMatrix();
            gl.Translate(0, 500, 0);
            DrawSea(gl);
            gl.PopMatrix();
            
            gl.PopMatrix();
            
            gl.Flush();
        }

        internal void Initialize(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            LoadTextures(gl);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            SetupLighting(gl);
            SetupCamera(gl);
            Scene.LoadScene();
            Scene.Initialize();


        }

        private void SetupCamera(OpenGL gl)
        {
            cam = new LookAtCamera();
            position = new Vertex(30f, 50f, 180f);
            target = new Vertex(0f, 40f, 0f);
            upVector = new Vertex(0f, 1f, 0f);
            right = new Vertex(1f, 0f, 0f);
            direction = new Vertex(-30f, -10f, -180f);
            target = position + direction;
            UpdateCameraRotation(0, 0);
            cam.Project(gl);
        }

        public void UpdateCameraRotation(double hAngle, double vAngle)
        {
            horizontalAngle += hAngle;
            if (Math.Abs(verticalAngle + vAngle) < Math.PI / 2)
            verticalAngle += vAngle;

            //spherical coordinates to cartesian coordinates
            direction.X = (float)(Math.Cos(verticalAngle) * Math.Sin(horizontalAngle));
            direction.Y = (float)(Math.Sin(verticalAngle));
            direction.Z = (float)(Math.Cos(verticalAngle) * Math.Cos(horizontalAngle));

            right.X = (float)Math.Sin(horizontalAngle - (Math.PI / 2));
            right.Y = 0f;
            right.Z = (float)Math.Cos(horizontalAngle - (Math.PI / 2));

            up = right.VectorProduct(direction);

            target = position + direction;
            upVector = up;
        }

        public void UpdateCameraPosition(int deltaX, int deltaY, int deltaZ)
        {
            Vertex deltaForward = direction * deltaZ;
            Vertex deltaStrafe = right * deltaX;
            Vertex deltaUp = up * deltaY;
            Vertex delta = deltaForward + deltaStrafe + deltaUp;
            Vertex move = delta * walkSpeed;
            if((position + move).Y > 25)
            {
                position += move;
                target = position + direction;
                upVector = up;
            }   
        }

        private void LoadTextures(OpenGL gl)
        {
            gl.GenTextures(TextureCount, textures);
            for (int i = 0; i < TextureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA8, image.Width, image.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);

                //gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        internal void Resize(OpenGL gl, int actualWidth, int actualHeight)
        {
            Width = actualWidth;
            Height = actualHeight;
            //gl.Viewport(0, 0, Width, Height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)Width / Height, 1, 10000);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Scene.Dispose();
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
