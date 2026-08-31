using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroids
{
    internal class Saucer
    {
        public Polygon Shape { get; set; }
        public double Radius { get; set; }
        public double Angle { get; set; }
        private double VelocityX;
        private double VelocityY;
        public double X { get; set; }
        public double Y { get; set; }
        private static Random _random = new Random();
        private double _minX, _maxX, _minY, _maxY;
        //private double _velocity;
        public int ItemScore { get; private set; }
        public int Size { get; private set; }

        public Saucer()
        {
            Point p;
            double x, y;
            double scale = GameData.SaucerSizeFactor;
            int whichSide = _random.Next(2);

            // define position
            double margin = scale / 2.0;
            //double margin = 0;
            _minX = 0 - margin;
            _maxX = GameData.ScreenWidth + margin;
            _minY = 0 - margin;
            _maxY = GameData.ScreenHeight + margin;
            //X = (_maxX - _minX) * 1.0 + _minX;
            X = whichSide == 0 ? _minX : _maxX;
            Y = (_maxY - _minY) * _random.NextDouble() + _minY;

            Angle = -90;
            //int Size = _random.Next(1, 3);
            Size = (_random.NextDouble() < GameData.ProbabilityOfSmallSaucer) ? 1 : 2;
            if (Size == 2)
                ItemScore = 200;
            else
                ItemScore = 1000;
            //Debug.WriteLine($"Size={Size}");
            scale *= Size;
            Radius = scale;
            //_velocity = scale;

            // define speed
            //VelocityX = -2.0;
            VelocityX = whichSide == 0 ? GameData.SaucerXVelocity : -GameData.SaucerXVelocity;
            VelocityY = _random.NextDouble() * GameData.SaucerYVelocityRange - GameData.SaucerYVelocityRange / 2.0;

            // define shape
            Shape = new Polygon();
            Shape.Stroke = Brushes.White;
            Shape.StrokeThickness = 2;
            if (Size == 2)
                Shape.Fill = Brushes.DarkGreen;
            else
                Shape.Fill = Brushes.DarkRed;
            PointCollection myPointCollection = new PointCollection();

            x = -0.29;
            y = -0.65;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = -0.5;
            y = -0.18;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = -1;
            y = 0.24;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = -0.59;
            y = 0.65;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = 0.59;
            y = 0.65;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = 1;
            y = 0.24;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = 0.5;
            y = -0.18;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);
            x = 0.29;
            y = -0.65;
            p = new Point(x * scale, y * scale);
            myPointCollection.Add(p);

            Shape.Points = myPointCollection;

            // initialize
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
        }

        public void Update()
        {
            X += VelocityX * GameData.SpeedFactor;
            Y += VelocityY * GameData.SpeedFactor;
            VelocityY = (_random.Next(48) == 0) ? (_random.NextDouble() * GameData.SaucerYVelocityRange - GameData.SaucerYVelocityRange / 2.0) : VelocityY;
            if (X < _minX) X = _maxX;
            if (X > _maxX) X = _minX;
            if (Y < _minY) Y = _maxY;
            if (Y > _maxY) Y = _minY;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
        }
    }
}
