using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroids
{
    internal class Ship
    {
        public Polygon Shape { get; set; }
        public double Radius { get; set; }
        public double Angle { get; set; }
        private RotateTransform Rotation;
        private double VelocityX;
        private double VelocityY;
        public double X { get; set; }
        public double Y { get; set; }
        private static Random _random = new Random();
        private Stopwatch timer = new Stopwatch();
        private double _minX, _maxX, _minY, _maxY;
        private double _velocity;
        private Point _center;

        public Ship()
        {
            Point p;
            double x, y;
            double scale = GameData.ShipSizeFactor;
            _center = new Point(GameData.ScreenWidth / 2.0, GameData.ScreenHeight / 2.0);

            // define position
            //double margin = 0.16 * Math.Min(GameData.ScreenWidth, GameData.ScreenHeight) / 2.0;
            double margin = 0;
            _minX = 0 - margin;
            _maxX = GameData.ScreenWidth + margin;
            _minY = 0 - margin;
            _maxY = GameData.ScreenHeight + margin;
            X = (_maxX - _minX) / 2.0 + _minX;
            Y = (_maxY - _minY) / 2.0 + _minY;

            Angle = -90;
            //int sizeFactor = _random.Next(1, 3);
            //scale *= Math.Min(GameData.ScreenWidth, GameData.ScreenHeight);
            Radius = scale;
            _velocity = scale;

            // define speed
            //RotationSpeed = 0;
            VelocityX = 0;
            VelocityY = 0;

            // define shape
            Shape = new Polygon();
            Shape.Stroke = Brushes.White;
            Shape.StrokeThickness = 2;
            Shape.Fill = Brushes.DarkBlue;
            PointCollection myPointCollection = new PointCollection();

            x = 0;
            y = -1;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = -0.667;
            y = 1;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = -0.55;
            y = 1;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = -0.4;
            y = 0.7;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = 0.4;
            y = 0.7;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = 0.55;
            y = 1;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = 0.667;
            y = 1;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);

            Shape.Points = myPointCollection;

            // initialize
            Rotation = new RotateTransform(0); // angle in degrees
            Shape.RenderTransform = Rotation;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
            timer.Start();
        }

        public void Update()
        {
            //Rotation.Angle += RotationSpeed;
            X += VelocityX * GameData.SpeedFactor;
            Y += VelocityY * GameData.SpeedFactor;
            if (X < _minX) X = _maxX;
            if (X > _maxX) X = _minX;
            if (Y < _minY) Y = _maxY;
            if (Y > _maxY) Y = _minY;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
            VelocityX *= GameData.ShipDragFactor;
            VelocityY *= GameData.ShipDragFactor;
        }

        public void Rotate(double angle)
        {
            Rotation.Angle += angle;
            Angle += angle;
        }

        public void Accelerate()
        {
            //Debug.WriteLine($"Angle={Angle}");
            VelocityX += GameData.ShipAcceleration * Math.Cos(Angle * Math.PI / 180.0);
            VelocityY += GameData.ShipAcceleration * Math.Sin(Angle * Math.PI / 180.0);
            double total = Math.Sqrt(VelocityX * VelocityX + VelocityY * VelocityY);
            if (total > GameData.ShipMaxVelocity)
            {
                VelocityX *= GameData.ShipMaxVelocity / total;
                VelocityY *= GameData.ShipMaxVelocity / total;
            }
        }

        public void Jump()
        {
            if (timer.ElapsedMilliseconds > GameData.ShipJumpDelay)
            {
                timer.Restart();
                X = _random.NextDouble() * (_maxX - _minX) + _minX;
                Y = _random.NextDouble() * (_maxY - _minY) + _minY;
                Canvas.SetLeft(Shape, X);
                Canvas.SetTop(Shape, Y);
                VelocityX = 0.0;
                VelocityY = 0.0;
            }
        }

        public void GetFiringParameters(out double xpos, out double ypos, out double angle)
        {
            xpos = X;
            ypos = Y;
            angle = Angle;
        }

        public void Reset()
        {
            X = _center.X;
            Y = _center.Y;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
            VelocityX = 0.0;
            VelocityY = 0.0;
            Rotation = new RotateTransform(0); // angle in degrees
            Shape.RenderTransform = Rotation;
            //Rotation.Angle = 0;
            Angle = -90;
        }
    }
}
