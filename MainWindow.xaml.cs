using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Asteroids
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random _random = new Random();
        DispatcherTimer _timer = new DispatcherTimer();
        MidiKeyboard _myKeyboard = new MidiKeyboard();
        XboxController _controller = new XboxController(0);
        Stopwatch _fireTimer = new Stopwatch();
        Stopwatch _SaucerFireTimer = new Stopwatch();
        Stopwatch _deployTimer = new Stopwatch();
        List<Asteroid> _asteroids = new List<Asteroid>();
        List<Bullet> _shipsBullets = new List<Bullet>();
        List<Bullet> _saucersBullets = new List<Bullet>();
        Ship? _ship;
        Saucer? _saucer;
        bool _paused = false;
        bool _gameOver = true;
        Stopwatch _gameTimer = new Stopwatch();
        long lastGameTime = 0;
        DateTime _gameCompletion;

        public MainWindow()
        {
            InitializeComponent();

            _timer.Interval = TimeSpan.FromMilliseconds(GameData.ExpectedTimeSpan); // ~60 frames per second
            _timer.Tick += Timer_Tick;
            _timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // get/set timer info
            GameData.ElapsedTime = Math.Min(_gameTimer.ElapsedMilliseconds - lastGameTime, 50);
            //Debug.WriteLine($"elapsed time = {GameData.ElapsedTime}, speed factor = {GameData.SpeedFactor}");
            lastGameTime = _gameTimer.ElapsedMilliseconds;

            // get inputs from keyboard and/or controller
            CheckKeyboardInputs();
            PollController();

            // move asteroids
            foreach (Asteroid asteroid in _asteroids)
                asteroid.Update();

            // create new saucer
            if (_saucer == null && _deployTimer.ElapsedMilliseconds > GameData.SaucerRespawnDelay)
            {
                _saucer = new Saucer();
                canvas.Children.Add(_saucer.Shape);
                _SaucerFireTimer.Restart();
            }

            // move ship
            if (_ship != null)
                _ship?.Update();

            // move saucer
            if (_saucer != null)
                _saucer?.Update();

            // fire saucer bullets
            if (Properties.Settings.Default.SaucerAttack)
                SaucerFire();

            // move and remove ship's bullets
            _shipsBullets.RemoveAll(bullet =>
            {
                bool result = bullet.Update();
                if (result) canvas.Children.Remove(bullet.Shape);
                return result;
            });

            // move and remove saucer's bullets
            _saucersBullets.RemoveAll(bullet =>
            {
                bool result = bullet.Update();
                if (result) canvas.Children.Remove(bullet.Shape);
                return result;
            });

            // check for collisions between objects
            // and remove objects that collided
            CheckAsteroidBulletCollision();
            CheckSaucerBulletCollision();
            CheckAsteroidSaucerCollision();
            if (!Properties.Settings.Default.InvincibleShip)
            {
                CheckAsteroidShipCollision();
                CheckShipBulletCollision();
                CheckSaucerShipCollision();
            }

            // start new level when no more asteroids
            if (_asteroids.Count <= 0) NewLevel();
        }

        private void CheckAsteroidBulletCollision()
        {
            HashSet<Asteroid> hitThisFrame = new HashSet<Asteroid>();
            List<Asteroid> toSplit = new List<Asteroid>();
            List<Bullet> toRemoveShip = new List<Bullet>();
            List<Bullet> toRemoveSaucer = new List<Bullet>();
            foreach (var bullet in _shipsBullets)
            {
                foreach (var asteroid in _asteroids)
                {
                    if (hitThisFrame.Contains(asteroid)) continue; // already handled this frame
                    if (AsteroidHit2(asteroid, bullet))
                    {
                        hitThisFrame.Add(asteroid);
                        // queue this bullet + asteroid for removal/splitting
                        toSplit.Add(asteroid);
                        toRemoveShip.Add(bullet);
                        //score += asteroid.ItemScore;
                        //Score.Text = $"Score : {score}";
                        AddToScore(asteroid.ItemScore);
                    }
                }
            }

            foreach (var bullet in _saucersBullets)
            {
                foreach (var asteroid in _asteroids)
                {
                    if (hitThisFrame.Contains(asteroid)) continue; // already handled this frame
                    if (AsteroidHit2(asteroid, bullet))
                    {
                        hitThisFrame.Add(asteroid);
                        // queue this bullet + asteroid for removal/splitting
                        toSplit.Add(asteroid);
                        toRemoveSaucer.Add(bullet);
                        //score += asteroid.ItemScore;
                        //Score.Text = $"Score : {score}";
                    }
                }
            }

            for (int i = 0; i < toRemoveShip.Count; i++)
                DeleteShipsBullet(toRemoveShip[i]);
            for (int i = 0; i < toRemoveSaucer.Count; i++)
                DeleteSaucersBullet(toRemoveSaucer[i]);
            for (int i = 0; i < toSplit.Count; i++)
                SplitAsteroid(toSplit[i]);
        }

        private async void CheckSaucerBulletCollision()
        {
            bool isHit = false;
            if (_saucer != null)
            {
                List<Bullet> toRemove = new List<Bullet>();
                foreach (var bullet in _shipsBullets)
                {
                    if (SaucerHit(_saucer, bullet))
                    {
                        // queue this bullet + saucer for removal/splitting
                        toRemove.Add(bullet);
                        isHit = true;
                        break;
                    }
                }
                for (int i = 0; i < toRemove.Count; i++)
                    DeleteShipsBullet(toRemove[i]);
                if (isHit)
                {
                    //score += _saucer.ItemScore;
                    //Score.Text = $"Score : {score}";
                    AddToScore(_saucer.ItemScore);
                    SaucerWasHit();
                }
            }
        }

        private void CheckShipBulletCollision()
        {
            bool isHit = false;
            if (_ship != null)
            {
                List<Bullet> toRemove = new List<Bullet>();
                foreach (var bullet in _saucersBullets)
                {
                    if (ShipHit(_ship, bullet))
                    {
                        // queue this bullet + saucer for removal/splitting
                        toRemove.Add(bullet);
                        isHit = true;
                        break;
                    }
                }
                for (int i = 0; i < toRemove.Count; i++)
                    DeleteSaucersBullet(toRemove[i]);
                if (isHit)
                {
                    // ship hit by bullet
                    ShipWasHit();
                }
            }
        }

        private void CheckAsteroidShipCollision()
        {
            bool hit = false;
            foreach (var asteroid in _asteroids)
            {
                if (_ship != null)
                {
                    hit = ShipHitAsteroid(_ship, asteroid);
                    if (hit) break;
                }
            }
            if (hit)
            {
                ShipWasHit();
            }
        }

        private void CheckAsteroidSaucerCollision()
        {
            bool hit = false;
            foreach (var asteroid in _asteroids)
            {
                if (_saucer != null)
                {
                    hit = SaucerHitAsteroid(_saucer, asteroid);
                    if (hit) break;
                }
            }
            if (hit)
            {
                SaucerWasHit();
            }
        }

        private void CheckSaucerShipCollision()
        {
            bool hit = false;
            if (_ship != null && _saucer != null)
            {
                hit = ShipHitSaucer(_ship, _saucer);
            }
            if (hit)
            {
                ShipWasHit();
            }
        }

        private async void ShipWasHit()
        {
            GameData.Lives--;
            Lives.Text = $"Lives : {GameData.Lives}";
            if (GameData.Lives <= 0)
            {
                _timer.Stop();
                await PlaySound(81, 36, 1000); // ship destruction sound
                GameOver();
            }
            else
            {
                canvas.Children.Remove(_ship?.Shape);
                _ship = null;
                await PlaySound(81, 36, 1000); // ship destruction sound
                await PlaySound(121, 0, 1000); // too low to hear sound
                //RestartLevel();
                if (_ship == null)
                {
                    _ship = new Ship();
                    canvas.Children.Add(_ship.Shape);
                }
            }
        }

        private async void SaucerWasHit()
        {
            if (_saucer != null)
                canvas.Children.Remove(_saucer.Shape);
            _saucer = null;
            _deployTimer.Restart();
            await PlaySound(118, 60, 250); // saucer destruction sound
        }

        private void DeleteShipsBullet(Bullet bullet)
        {
            canvas.Children.Remove(bullet.Shape);
            _shipsBullets.Remove(bullet);
        }

        private void DeleteSaucersBullet(Bullet bullet)
        {
            canvas.Children.Remove(bullet.Shape);
            _saucersBullets.Remove(bullet);
        }

        private async void SplitAsteroid(Asteroid asteroid)
        {
            // divide asteroid into two pieces
            // if it is small enough, just remove it
            List<Asteroid> children = asteroid.Split();
            if (_asteroids.Count >= GameData.MaxAsteroids) // limit asteroid count to 27
            {
                if (children.Count > 0)
                {
                    int i = _random.Next(children.Count);
                    _asteroids.Add(children[i]);
                    canvas.Children.Add(children[i].Shape);
                }
            }
            else
            {
                for (int i = 0; i < children.Count; i++)
                {
                    _asteroids.Add(children[i]);
                    canvas.Children.Add(children[i].Shape);
                }
            }
            canvas.Children.Remove(asteroid.Shape);
            _asteroids.Remove(asteroid);
            await PlaySound(127, 60, 250); // asteroid destruction sound
        }

        private bool AsteroidHit1(Asteroid asteroid, Bullet bullet)
        {
            Point pb = new Point(bullet.X + GameData.BulletRadius, bullet.Y + GameData.BulletRadius);
            Point pa = new Point(asteroid.X, asteroid.Y);
            double distance = Math.Sqrt((pa.X - pb.X) * (pa.X - pb.X) + (pa.Y - pb.Y) * (pa.Y - pb.Y));
            return (distance < asteroid.Size);
        }

        private bool AsteroidHit2(Asteroid asteroid, Bullet bullet)
        {
            Point pb = new Point(bullet.X + GameData.BulletRadius, bullet.Y + GameData.BulletRadius);
            // Convert pb from canvas coordinates into asteroid.Shape's local coordinate space
            GeneralTransform transform = canvas.TransformToVisual(asteroid.Shape);
            Point localPoint = transform.Transform(pb);
            bool isInside = asteroid.Shape.RenderedGeometry.FillContains(localPoint);
            if (isInside)
            {
                asteroid.BulletVelocityX = bullet.VelocityX;
                asteroid.BulletVelocityY = bullet.VelocityY;
            }
            return isInside;
        }

        private bool SaucerHit(Saucer saucer, Bullet bullet)
        {
            Point pb = new Point(bullet.X + GameData.BulletRadius, bullet.Y + GameData.BulletRadius);
            // Convert pb from canvas coordinates into asteroid.Shape's local coordinate space
            GeneralTransform transform = canvas.TransformToVisual(saucer.Shape);
            Point localPoint = transform.Transform(pb);
            bool isInside = saucer.Shape.RenderedGeometry.FillContains(localPoint);
            return isInside;
        }

        private bool ShipHit(Ship ship, Bullet bullet)
        {
            Point pb = new Point(bullet.X + GameData.BulletRadius, bullet.Y + GameData.BulletRadius);
            // Convert pb from canvas coordinates into asteroid.Shape's local coordinate space
            GeneralTransform transform = canvas.TransformToVisual(ship.Shape);
            Point localPoint = transform.Transform(pb);
            bool isInside = ship.Shape.RenderedGeometry.FillContains(localPoint);
            return isInside;
        }

        private bool ShipHitAsteroid(Ship ship, Asteroid asteroid)
        {
            GeneralTransform generalTransform = asteroid.Shape.TransformToVisual(ship.Shape);
            MatrixTransform matrixTransform = (MatrixTransform)generalTransform;

            Geometry asteroidGeometry = asteroid.Shape.RenderedGeometry.Clone();
            asteroidGeometry.Transform = matrixTransform;

            CombinedGeometry intersection = new CombinedGeometry(
                GeometryCombineMode.Intersect,
                ship.Shape.RenderedGeometry,
                asteroidGeometry);

            return !intersection.Bounds.IsEmpty;
        }

        private bool SaucerHitAsteroid(Saucer saucer, Asteroid asteroid)
        {
            GeneralTransform generalTransform = asteroid.Shape.TransformToVisual(saucer.Shape);
            MatrixTransform matrixTransform = (MatrixTransform)generalTransform;

            Geometry asteroidGeometry = asteroid.Shape.RenderedGeometry.Clone();
            asteroidGeometry.Transform = matrixTransform;

            CombinedGeometry intersection = new CombinedGeometry(
                GeometryCombineMode.Intersect,
                saucer.Shape.RenderedGeometry,
                asteroidGeometry);

            return !intersection.Bounds.IsEmpty;
        }

        private bool ShipHitSaucer(Ship ship, Saucer saucer)
        {
            GeneralTransform generalTransform = saucer.Shape.TransformToVisual(ship.Shape);
            MatrixTransform matrixTransform = (MatrixTransform)generalTransform;

            Geometry asteroidGeometry = saucer.Shape.RenderedGeometry.Clone();
            asteroidGeometry.Transform = matrixTransform;

            CombinedGeometry intersection = new CombinedGeometry(
                GeometryCombineMode.Intersect,
                ship.Shape.RenderedGeometry,
                asteroidGeometry);

            return !intersection.Bounds.IsEmpty;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // load the properties from disk
            Properties.Settings.Default.Reload();

            // check for upgrade
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            this.Left = Properties.Settings.Default.WindowLeft;
            this.Top = Properties.Settings.Default.WindowTop;
            this.Width = Properties.Settings.Default.WindowWidth;
            this.Height = Properties.Settings.Default.WindowHeight;

            // load other properties here

            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;

            // ensure window size doesn't exceed screen size
            if (this.Width > screenWidth) this.Width = screenWidth;
            if (this.Height > screenHeight) this.Height = screenHeight;

            // ensure window is not off the left or top
            if (this.Left < 0) this.Left = 0;
            if (this.Top < 0) this.Top = 0;

            // ensure window is not off the right or bottom
            if (this.Left + this.Width > screenWidth)
                this.Left = screenWidth - this.Width;
            if (this.Top + this.Height > screenHeight)
                this.Top = screenHeight - this.Height;

            if (Properties.Settings.Default.WindowState == "Maximized")
                this.WindowState = WindowState.Maximized;

            // do other initialization
            NewGame();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Stop();
            Properties.Settings.Default.WindowState = this.WindowState.ToString();
            if (this.WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.WindowLeft = this.Left;
                Properties.Settings.Default.WindowTop = this.Top;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.WindowHeight = this.Height;
            }

            // save other properties here
            if (!_gameOver)
            {
                _gameCompletion = DateTime.Now;
                RecordHighScore(GameData.Score, _gameCompletion);
            }
            if (GameData.Score > Properties.Settings.Default.HighScore)
                Properties.Settings.Default.HighScore = GameData.Score;

            Properties.Settings.Default.Save();
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GameData.ScreenWidth = canvas.ActualWidth;
            GameData.ScreenHeight = canvas.ActualHeight;
            if (!_gameOver)
                RestartLevel();
        }

        private void NewGame()
        {
            GameData.Level = 0;
            GameData.Lives = GameData.StartingLives;
            GameData.Score = 0;
            _gameOver = false;
            NewLevel();
            //_timer.Start();
            _paused = false;
        }

        private void NewLevel()
        {
            GameData.Level++;
            GameData.LivesBackup = GameData.Lives;
            GameData.ScoreBackup = GameData.Score;
            RestartLevel();
        }

        private void RestartLevel()
        {
            GameData.Lives = GameData.LivesBackup;
            GameData.Score = GameData.ScoreBackup;
            Level.Text = $"Level : {GameData.Level}";
            Lives.Text = $"Lives : {GameData.Lives}";
            Score.Text = $"Score : {GameData.Score}";

            //PrintParameters();
            canvas.Children.Clear();
            _asteroids.Clear();
            for (int i = 0; i < GameData.StartingAsteroidCount; i++)
            {
                Asteroid asteroid = new Asteroid();
                _asteroids.Add(asteroid);
                canvas.Children.Add(asteroid.Shape);
            }
            _ship = new Ship();
            canvas.Children.Add(_ship.Shape);
            //_saucer = new Saucer(canvas.ActualWidth, canvas.ActualHeight);
            //canvas.Children.Add(_saucer.Shape);
            _saucer = null;
            _shipsBullets.Clear();
            _saucersBullets.Clear();
            _fireTimer.Restart();
            _SaucerFireTimer.Restart();
            _deployTimer.Restart();
            _gameTimer.Restart();
            lastGameTime = _gameTimer.ElapsedMilliseconds;
            _timer.Start();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (_gameOver) return;
            if (PauseButton.Content.ToString() == "Pause")
            {
                _timer.Start();
                _paused = false;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            _timer.Stop();
            _paused = true;
        }

        /*
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Debug.WriteLine("key pressed");
            //Debug.WriteLine($"paused = {_paused}");
            if (_gameOver || _paused) return;
            switch (e.Key)
            {
                case Key.Left: _ship?.Rotate(-5.625); break;
                case Key.Up: _ship?.Forward(); break;
                case Key.Right: _ship?.Rotate(5.625); break;
                case Key.Down: _ship?.Jump(); break;
                case Key.Space: Fire(); break;
                case Key.P: PauseButton_Click(this, new RoutedEventArgs()); break;
                case Key.C: /* cheat mode *
                    Properties.Settings.Default.InvincibleShip = !Properties.Settings.Default.InvincibleShip;
                    break;
                case Key.S: /* sound effects *
                    Properties.Settings.Default.SoundOn = !Properties.Settings.Default.SoundOn;
                    break;
            }

            e.Handled = true;
        }
        */
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.P: PauseButton_Click(this, new RoutedEventArgs()); break;
                case Key.C: /* cheat mode */
                    Properties.Settings.Default.InvincibleShip = !Properties.Settings.Default.InvincibleShip;
                    break;
                case Key.S: /* sound effects */
                    Properties.Settings.Default.SoundOn = !Properties.Settings.Default.SoundOn;
                    break;
            }
            e.Handled = true;
        }

        private async void Fire()
        {
            //Debug.WriteLine("trying to fire");
            if (!_paused && _fireTimer.ElapsedMilliseconds > GameData.ShipFiringDelay)
            {
                _fireTimer.Restart();
                if (_ship != null)
                {
                    //_ship.GetFiringParameters(out double xpos, out double ypos, out double angle);
                    //Bullet bullet = new Bullet(xpos, ypos, angle);
                    Bullet bullet = new Bullet(_ship.X, _ship.Y, _ship.Angle);
                    _shipsBullets.Add(bullet);
                    canvas.Children.Add(bullet.Shape);
                    //Debug.WriteLine("fired");
                    await PlaySound(118, 72, 250); // space firing sound
                    //PlaySound(127, 60, 250); // asteroid destruction sound
                    //PlaySound(81, 36, 1000); // ship destruction sound
                }
            }
        }

        private void SaucerFire()
        {
            if (_saucer != null && _ship != null)
            {
                if (_SaucerFireTimer.ElapsedMilliseconds > GameData.SaucerFiringDelay)
                {
                    _SaucerFireTimer.Restart();
                    double angleToShip = Math.Atan2(_ship.Y - _saucer.Y, _ship.X - _saucer.X);
                    angleToShip *= 180.0 / Math.PI;
                    double angleRange = (_saucer.Size == 2) ? 360.0 : GameData.SaucerFiringAngleRange;
                    double fireAngle = _random.NextDouble() * angleRange - angleRange / 2.0 + angleToShip;
                    Bullet bullet = new Bullet(_saucer.X, _saucer.Y, fireAngle);
                    _saucersBullets.Add(bullet);
                    canvas.Children.Add(bullet.Shape);
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Debug.WriteLine($"Window width = {this.Width}, height = {this.Height}");
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver) return;
            if (PauseButton.Content.ToString() == "Pause")
            {
                _timer.Stop();
                _paused = true;
                PauseButton.Content = "Resume";
            }
            else
            {
                _timer.Start();
                _paused = false;
                PauseButton.Content = "Pause";
            }
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            Options dialog = new Options();
            dialog.Owner = this;
            bool? result = dialog.ShowDialog(); // Modal dialog
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow dialog = new HelpWindow();
            dialog.Owner = this;
            bool? result = dialog.ShowDialog(); // Modal dialog
        }

        private async Task PlaySound(int instrument, int note, int duration)
        {
            if (Properties.Settings.Default.SoundOn)
            {
                _myKeyboard.ChangeInstrument(instrument);
                await _myKeyboard.PlayNote(note, duration);
            }
        }

        private async void GameOver()
        {
            _timer.Stop();
            _gameCompletion = DateTime.Now;
            _gameOver = true;
            RecordHighScore(GameData.Score, _gameCompletion);
            Level.Text = $"Level : {GameData.Level}";
            Lives.Text = $"Lives : {GameData.Lives}";
            Score.Text = $"Score : {GameData.Score}";
            bool highScore = (GameData.Score > Properties.Settings.Default.HighScore);

            if (GameData.Level <= 1) // level 1
            {
                //await PlaySound(13, 72, 100);
                //await PlaySound(13, 76, 100);
                //await PlaySound(13, 72, 100);
                //await PlaySound(13, 76, 100);
                //await PlaySound(13, 72, 100);
                WinnerWindow3 dialog3 = new WinnerWindow3(GameData.Score, GameData.Level, highScore);
                dialog3.Owner = Window.GetWindow(this);
                bool? result3 = dialog3.ShowDialog();
            }
            else if (GameData.Level <= 3) // level 2, 3
            {
                //await PlaySound(13, 84, 100);
                //await PlaySound(13, 79, 100);
                //await PlaySound(13, 76, 100);
                //await PlaySound(13, 72, 100);
                WinnerWindow2 dialog2 = new WinnerWindow2(GameData.Score, GameData.Level, highScore);
                dialog2.Owner = Window.GetWindow(this);
                bool? result2 = dialog2.ShowDialog();
            }
            else // level 4+
            {
                //await PlaySound(13, 72, 100);
                //await PlaySound(13, 76, 100);
                //await PlaySound(13, 79, 100);
                //await PlaySound(13, 84, 100);
                WinnerWindow1 dialog1 = new WinnerWindow1(GameData.Score, GameData.Level, highScore);
                dialog1.Owner = Window.GetWindow(this);
                bool? result1 = dialog1.ShowDialog();
            }

            if (GameData.Score > Properties.Settings.Default.HighScore)
                Properties.Settings.Default.HighScore = GameData.Score;
            Properties.Settings.Default.Save();
        }

        private void AddToScore(int points)
        {
            if ((GameData.Score + points) / GameData.ExtraLifeScoreThreshold > GameData.Score / GameData.ExtraLifeScoreThreshold)
            {
                GameData.Lives++;
                Lives.Text = $"Lives : {GameData.Lives}";
            }
            GameData.Score += points;
            Score.Text = $"Score : {GameData.Score}";
        }

        private void CheckKeyboardInputs()
        {
            if (!_gameOver && !_paused)
            {
                if (Keyboard.IsKeyDown(Key.Left))
                    _ship?.Rotate(-GameData.ShipRotationIncrement);

                if (Keyboard.IsKeyDown(Key.Right))
                    _ship?.Rotate(GameData.ShipRotationIncrement);

                if (Keyboard.IsKeyDown(Key.Up))
                    _ship?.Accelerate();

                if (Keyboard.IsKeyDown(Key.Down))
                    _ship?.Jump();

                if (Keyboard.IsKeyDown(Key.Space))
                    Fire();
            }

            /*
            if (Keyboard.IsKeyDown(Key.P)) // pause / resume
                PauseButton_Click(this, new RoutedEventArgs());

            if (Keyboard.IsKeyDown(Key.C)) // cheat / invincibility
                Properties.Settings.Default.InvincibleShip = !Properties.Settings.Default.InvincibleShip;

            if (Keyboard.IsKeyDown(Key.S)) // sound effect on/off
                Properties.Settings.Default.SoundOn = !Properties.Settings.Default.SoundOn;
            */
        }

        private void PollController()
        {
            if (_controller == null) return;
            _controller.Poll();
            if (_controller.IsConnected)
            {
                int direction = GetDirection(_controller.LeftStickX, _controller.LeftStickY);
                switch (direction) // (0=E, 1=N, 2=W, 3=S)
                {
                    case 0: _ship?.Rotate(GameData.ShipRotationIncrement); break;
                    case 2: _ship?.Rotate(-GameData.ShipRotationIncrement); break;
                }

                if (_controller.WasButtonJustPressed(XInputButtons.A) ||
                    _controller.IsButtonHeld(XInputButtons.A))
                    Fire();

                if (_controller.WasButtonJustPressed(XInputButtons.B) ||
                    _controller.IsButtonHeld(XInputButtons.B))
                    _ship?.Accelerate();

                if (_controller.WasButtonJustPressed(XInputButtons.X) ||
                    _controller.IsButtonHeld(XInputButtons.X))
                    _ship?.Jump();
            }
        }

        private int GetDirection(double x, double y)
        {
            // input is stick position -1 to 1
            // returns an int (0=E, 1=N, 2=W, 3=S)
            if (Math.Abs(x) < 0.1 && Math.Abs(y) < 0.1)
                return -1; // stick is centered
            else
            {
                double direction = Math.Atan2(y, x) * 180 / Math.PI; // in degrees -180 to 180
                if (direction < 0) direction += 360; // degrees 0 to 360
                int idir = (int)((direction + 45) / 90.0); // int 0 to 4
                idir = idir % 4; // int 0 to 3
                return idir;
            }
        }

        private void PrintParameters()
        {
            Debug.WriteLine($"Level = {GameData.Level}");
            Debug.WriteLine($"Lives = {GameData.Lives}");
            Debug.WriteLine($"Score = {GameData.Score}");
            Debug.WriteLine($"StartingAsteroidCount = {GameData.StartingAsteroidCount}");
            Debug.WriteLine($"AsteroidVelocityRange = {GameData.AsteroidVelocityRange}");
            Debug.WriteLine($"SaucerRespawnDelaySeconds = {GameData.SaucerRespawnDelay}");
            Debug.WriteLine($"ProbabilityOfSmallSaucer = {GameData.ProbabilityOfSmallSaucer}");
            Debug.WriteLine($"SaucerFiringAngleRange = {GameData.SaucerFiringAngleRange}");
            Debug.WriteLine($"SaucerFiringDelay = {GameData.SaucerFiringDelay}");
        }

        private void ScoresButton_Click(object sender, RoutedEventArgs e)
        {
            Scores dialog = new Scores();
            dialog.Owner = this;
            bool? result = dialog.ShowDialog(); // Modal dialog
        }

        private void RecordHighScore(int newScore, DateTime completionTime)
        {
            List<Score> scores = Scores.ReadScores();

            for (int i = 0; i < scores.Count; i++)
            {
                if (newScore > scores[i].HighScore)
                {
                    for (int j = scores.Count - 1; j > i; j--)
                    {
                        scores[j].HighScore = scores[j - 1].HighScore;
                        scores[j].DateOfScore = scores[j - 1].DateOfScore;
                    }
                    scores[i].HighScore = newScore;
                    scores[i].DateOfScore = completionTime;
                    break;
                }
            }

            string json = JsonSerializer.Serialize(scores);
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string asteroidsFolder = Path.Combine(appDataFolder, "Asteroids");
            string filePath = Path.Combine(asteroidsFolder, "highscores.json");
            Directory.CreateDirectory(asteroidsFolder); // ensure the folder exists first
            File.WriteAllText(filePath, json);
        }
    }
}