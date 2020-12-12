using System;
using SharpGL;
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

        public AssimpScene Scene { get; private set; }

        public World(string sceneFilePath, string sceneFileName, int width, int height, OpenGL gl)
        {
            Scene = new AssimpScene(sceneFilePath, sceneFileName, gl);
            Width = width;
            Height = height;
        }

        ~World()
        {
            Dispose(false);
        }

        private void DrawSea(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(0, -300, 0);
            
            gl.Begin(OpenGL.GL_QUADS);

            gl.Color(0.2, 0.2, 1.0);
            gl.Vertex(-5000, 0, 5000);
            gl.Vertex(5000, 0, 5000);
            gl.Vertex(5000, 0, -5000);
            gl.Vertex(-5000, 0, -5000);

            gl.End();
            gl.PopMatrix();
        }

        private void DrawDock(OpenGL gl)
        {
            gl.PushMatrix();
            Cube dock = new Cube();
            gl.Color(0.7, 0.5, 0.5);
            gl.Translate(420, 240, 800);
            gl.Scale(100, 5, 400);
            dock.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            Cube ramp = new Cube();
            gl.Translate(420, 280, 340);
            gl.Rotate(20, 0, 0);
            gl.Scale(60, 5, 100);
            gl.Color(0.6, 0.4, 0.4);
            ramp.Render(gl, RenderMode.Render);
            gl.PopMatrix();


            float baseRadius = 10.0f;
            float height = 300.0f;

            gl.PushMatrix();
            gl.Color(0.7, 0.4, 0.4);
            gl.Translate(315, 260, 500);
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

        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            DrawText(gl);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.FrontFace(OpenGL.GL_CCW);
            

            gl.PushMatrix();
            gl.Translate(TranslateX, 0, TranslateZ);

            gl.Translate(0, -500, -2500);
            gl.Rotate(RotationX, 1.0f, 0.0f, 0.0f);
            gl.Rotate(RotationY, 0.0f, 1.0f, 0.0f);

            gl.PushMatrix();
            gl.Rotate(-90, 0, 0);
            Scene.Draw();
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0, 500, 0);
            DrawSea(gl);
            gl.PopMatrix();
            DrawDock(gl);

            gl.PopMatrix();
            gl.Flush();
        }

        internal void Initialize(OpenGL gl)
        {
            // Crna pozadina i zuta boja za crtanje
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 1f, 0f);
            // Model sencenja na flat (konstantno)
            gl.ShadeModel(OpenGL.GL_FLAT);
            // Podrazumevano
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            Scene.LoadScene();
            Scene.Initialize();
        }

        internal void Resize(OpenGL gl, int actualWidth, int actualHeight)
        {
            float nRange = 100;
            Width = actualWidth;
            Height = actualHeight;
            gl.Viewport(0, 0, Width, Height);
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
