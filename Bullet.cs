using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroids
{
    internal class Bullet
    {
        public Ellipse Shape { get; set; }
        private Stopwatch timer = new Stopwatch();
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public Bullet(double xpos, double ypos, double angle)
        {
            double velocity = GameData.BulletSpeed;

            // define position
            X = xpos;
            Y = ypos;

            // define speed
            VelocityX = Math.Cos(angle * Math.PI / 180.0) * velocity;
            VelocityY = Math.Sin(angle * Math.PI / 180.0) * velocity;

            // define shape
            Shape = new Ellipse();
            Shape.Width = GameData.BulletRadius * 2;
            Shape.Height = GameData.BulletRadius * 2;
            Shape.Fill = Brushes.White;

            // initialize
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
            timer.Start();
        }

        public bool Update()
        {
            X += VelocityX * GameData.SpeedFactor;
            Y += VelocityY * GameData.SpeedFactor;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
            if (timer.ElapsedMilliseconds > GameData.BulletLifeSpan)
                return true;
            else
                return false;
        }
    }
}
