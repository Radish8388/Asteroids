using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Asteroids
{
    internal class Asteroid
    {
        public Polygon Shape { get; set; }
        private RotateTransform Rotation;
        private double RotationSpeed;
        private double VelocityX;
        private double VelocityY;
        public double X { get; set; }
        public double Y { get; set; }
        public double Size { get; set; }
        public int ItemScore { get; set; }
        private double _rmax;
        private static Random _random = new Random();
        private int _size = 0; // 0=large, 1=medium, 2=small
        private double _minX, _maxX, _minY, _maxY;
        public double BulletVelocityX { get; set; } = 0;
        public double BulletVelocityY { get; set; } = 0;

        public Asteroid()
        {
            Point p;
            double x, y, r;
            //double[] scale = { 0.06, 0.03, 0.015, 0.0 };
            int[] scores = { 20, 50, 100, 0 };
            Point center = new Point(GameData.ScreenWidth/2.0, GameData.ScreenHeight/2.0);
            double distance = 0;

            // define margins
            double margin = GameData.AsteroidSizeFactor / 2.0;
            _minX = 0 - margin;
            _maxX = GameData.ScreenWidth + margin;
            _minY = 0 - margin;
            _maxY = GameData.ScreenHeight + margin;

            // define speed
            RotationSpeed = _random.NextDouble() * GameData.AsteroidRotationSpeedRange - GameData.AsteroidRotationSpeedRange / 2.0;
            VelocityX = _random.NextDouble() * GameData.AsteroidVelocityRange - GameData.AsteroidVelocityRange / 2.0;
            VelocityY = _random.NextDouble() * GameData.AsteroidVelocityRange - GameData.AsteroidVelocityRange / 2.0;
            //Debug.WriteLine($"elapsed time = {GameData.ElapsedTime}, speed factor = {GameData.SpeedFactor}");
            //Debug.WriteLine($"asteroid velocity = {GameData.AsteroidVelocityRange}");

            // define size
            //_size = _random.Next(0, 3);
            _size = 0;
            ItemScore = scores[_size];
            _rmax = GameData.AsteroidSizeFactor;
            Size = _rmax * 2.0 / 3.0;

            // define position
            X = _random.NextDouble() * (_maxX - _minX) + _minX;
            Y = _random.NextDouble() * (_maxY - _minY) + _minY;
            distance = Math.Sqrt((X - center.X) * (X - center.X) + (Y - center.Y) * (Y - center.Y));
            while (distance < _rmax * GameData.AsteroidDistanceFromCenter)
            {
                X = _random.NextDouble() * (_maxX - _minX) + _minX;
                Y = _random.NextDouble() * (_maxY - _minY) + _minY;
                distance = Math.Sqrt((X - center.X) * (X - center.X) + (Y - center.Y) * (Y - center.Y));
            }

            // define shape
            Shape = new Polygon();
            Shape.Stroke = Brushes.White;
            Shape.StrokeThickness = 2;
            Shape.Fill = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            PointCollection myPointCollection = new PointCollection();
            for (int angle = 0; angle < 360; angle += 15)
            {
                r = (2 + _random.NextDouble()) * _rmax / 3.0;
                x = Math.Cos(angle * Math.PI / 180.0) * r;
                y = Math.Sin(angle * Math.PI / 180.0) * r;
                p = new Point(x, y);
                myPointCollection.Add(p);
            }
            Shape.Points = myPointCollection;

            // initialize
            Rotation = new RotateTransform(0); // angle in degrees
            Shape.RenderTransform = Rotation;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
        }

        public Asteroid(Asteroid parent)
        {
            Point p;
            double x, y, r;
            //double rmax;
            //double[] scale = { 0.16, 0.08, 0.04, 0.0 };
            int[] scores = { 20, 50, 100, 0 };

            X = parent.X;
            Y = parent.Y;

            // define speed
            RotationSpeed = parent.RotationSpeed;
            VelocityX = parent.VelocityX;
            VelocityY = parent.VelocityY;

            // define size
            _rmax = parent._rmax / 2.0;
            _size = parent._size + 1;
            ItemScore = scores[_size];
            Size = _rmax * 2.0 / 3.0;

            // define shape
            Shape = new Polygon();
            Shape.Stroke = Brushes.White;
            Shape.StrokeThickness = 2;
            Shape.Fill = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            PointCollection myPointCollection = new PointCollection();
            for (int angle = 0; angle < 360; angle += 15)
            {
                r = (2 + _random.NextDouble()) * _rmax / 3.0;
                x = Math.Cos(angle * Math.PI / 180.0) * r;
                y = Math.Sin(angle * Math.PI / 180.0) * r;
                p = new Point(x, y);
                myPointCollection.Add(p);
            }
            Shape.Points = myPointCollection;

            // initialize
            Rotation = new RotateTransform(0); // angle in degrees
            Shape.RenderTransform = Rotation;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);

            //_minX, _maxX, _minY, _maxY
            _minX = parent._minX;
            _maxX = parent._maxX;
            _minY = parent._minY;
            _maxY = parent._maxY;
        }

        public void Update()
        {
            Rotation.Angle += RotationSpeed;
            X += VelocityX * GameData.SpeedFactor;
            Y += VelocityY * GameData.SpeedFactor;
            if (X < _minX) X = _maxX;
            if (X > _maxX) X = _minX;
            if (Y < _minY) Y = _maxY;
            if (Y > _maxY) Y = _minY;
            Canvas.SetLeft(Shape, X);
            Canvas.SetTop(Shape, Y);
        }

        public List<Asteroid> Split()
        {
            double newVelocity = GameData.AsteroidSplitVelocity;
            //Debug.WriteLine($"elapsed time = {GameData.ElapsedTime}, speed factor = {GameData.SpeedFactor}");
            //Debug.WriteLine($"split velocity = {GameData.AsteroidSplitVelocity}");

            if (_size < 2)
            {
                double bulletAngle = Math.Atan2(BulletVelocityY, BulletVelocityX);
                double dvx = Math.Cos(bulletAngle - Math.PI / 2.0);
                double dvy = Math.Sin(bulletAngle - Math.PI / 2.0);
                var child1 = new Asteroid(this);
                var child2 = new Asteroid(this);

                // give each child a different heading so they fly apart
                child1.SetVelocity(this.VelocityX + newVelocity * dvx, this.VelocityY + newVelocity * dvy);
                child2.SetVelocity(this.VelocityX - newVelocity * dvx, this.VelocityY - newVelocity * dvy);

                return new List<Asteroid> { child1, child2 };
            }
            else
                return new List<Asteroid>();
        }

        private void SetVelocity(double velocityX, double velocityY)
        {
            VelocityX = velocityX;
            VelocityY = velocityY;
        }

    }
}
