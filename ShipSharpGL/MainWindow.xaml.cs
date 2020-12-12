using SharpGL;
using SharpGL.WPF;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;

namespace ShipSharpGL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        World world = null;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                world = new World(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "Ship"
                        ),
                    "boat.obj",
                    (int)openGLControl.ActualWidth,
                    (int)openGLControl.ActualHeight,
                    openGLControl.OpenGL
                    );
            }
            catch (Exception)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta", "GRESKA", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL gl = openGLControl.OpenGL;
            world.Initialize(gl);
        }


        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLRoutedEventArgs args)
        {
            world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F10: Close(); break;
                case Key.W: world.RotationX -= 5.0f; break;
                case Key.S: world.RotationX += 5.0f; break;
                case Key.A: world.RotationY -= 5.0f; break;
                case Key.D: world.RotationY += 5.0f; break;
                case Key.E: world.TranslateX -= 100.0f; break;
                case Key.Q: world.TranslateX += 100.0f; break;
                case Key.Z: world.TranslateZ += 100.0f; break;
                case Key.X: world.TranslateZ -= 100.0f; break;
            }
        }
    }
}
