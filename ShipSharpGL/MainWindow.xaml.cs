using SharpGL;
using SharpGL.WPF;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using System.Runtime.InteropServices;

namespace ShipSharpGL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        private const int BORDER = 100;
        private Point oldPos;

        public bool Enabled 
        {
            get => !(world.timer.IsEnabled || world.timer2.IsEnabled);
        }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        World world = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
            SetCursorPos((int)(Left + Width / 2), (int)(Top + Height / 2));
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
            if (!Enabled) return;
            switch (e.Key)
            {
                case Key.W: world.UpdateCameraRotation(0, 0.09f); break;
                case Key.S: world.UpdateCameraRotation(0, -0.09f); break;
                case Key.A: world.UpdateCameraRotation(0.09f, 0); break;
                case Key.D: world.UpdateCameraRotation(-0.09f, 0); break;
                case Key.Add: world.UpdateCameraPosition(0, 0, 10); break;
                case Key.Subtract: world.UpdateCameraPosition(0, 0, -10); break;
                case Key.C: world.ResetAnimation(); break;
                case Key.Escape: Close(); break;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;
            bool outOfBoundsX = false;
            bool outOfBoundsY = false;
            Point point = e.GetPosition(this);
            if (point.Y <= BORDER || point.Y >= this.Height - BORDER)
            {
                outOfBoundsY = true;
            }
            if (point.X <= BORDER || point.X >= this.Width - BORDER)
            {
                outOfBoundsX = true;
            }

            if (!outOfBoundsY && !outOfBoundsX)
            {
                double deltaX = oldPos.X - point.X;
                double deltaY = oldPos.Y - point.Y;
                world.UpdateCameraRotation(deltaX, deltaY);
                oldPos = point;
            }
            else
            {
                if (outOfBoundsX)
                {
                    SetCursorPos((int)Left + (int)Width / 2, (int)Top + (int)point.Y);
                    oldPos.X = Width / 2;
                    oldPos.Y = point.Y;
                }
                else
                {
                    SetCursorPos((int)this.Left + (int)point.X, (int)this.Top + (int)this.Height / 2);
                    oldPos.Y = this.Height / 2;
                    oldPos.X = point.X;
                }
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Enabled) return;
            world.PostPercentage = (float)e.NewValue;
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!Enabled) return;
            world.RampPercentage = (float)e.NewValue;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!Enabled) return;
            float.TryParse(RedText.Text, out float red);
            world.Red = red;
        }

        private void TextBox_TextChanged_1(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!Enabled) return;
            float.TryParse(GreenText.Text, out float green);
            world.Green = green;
        }

        private void TextBox_TextChanged_2(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!Enabled) return;
            float.TryParse(BlueText.Text, out float blue);
            world.Blue = blue;
        }
    }
}
