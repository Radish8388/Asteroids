namespace Asteroids
{
    public static class GameData
    {
        public static double ScreenWidth;
        public static double ScreenHeight;
        public static int Level;
        public static int Lives;
        public static int Score;
        public static int LivesBackup;
        public static int ScoreBackup;
        public static long ElapsedTime; // in milliseconds
        public const long ExpectedTimeSpan = 16; // in milliseconds

        // calculated parameters
        public static int StartingAsteroidCount
        {
            // ramp from 4 to max, increasing by 1 each level
            //get { return Math.Min(Level * 2 + 2, MaxStartingAsteroids); }
            get { return Math.Min(Level + 3, MaxStartingAsteroids); }
        }
        public static double SaucerRespawnDelay
        {
            // ramp from 10 at level=1 to 0 at level=21
            get
            {
                double maxDelay = 10000.0;
                double maxLevel = 21.0;
                double delay = (maxLevel - Level) * maxDelay / (maxLevel - 1.0);
                return Math.Clamp(delay, 0, maxDelay);
            }
        }

        public const double SaucerDifficultyStartScore = 10000.0;
        public const double SaucerDifficultyEndScore = 200000.0;
        public static double ProbabilityOfSmallSaucer
        {
            // ramp from 0 at score=10000 to 1 at score=200000
            // probability in the range 0 to 1
            get
            {
                double prob = (Score - SaucerDifficultyStartScore) / (SaucerDifficultyEndScore - SaucerDifficultyStartScore);
                return Math.Clamp(prob, 0.0, 1.0);
            }
        }
        public static double SaucerFiringAngleRange
        {
            // ramp from 40 at score=10000 to 0 at score=200000
            get
            {
                double max = 40.0; // in degrees
                double t = ProbabilityOfSmallSaucer; // reuse the same 0-to-1 progress
                return max * (1.0 - t);
            }
        }
        public static double SaucerFiringDelay
        {
            // ramp from 2000 at score=10000 to 100 at score=200000
            get
            {
                double max = 2000.0; // in milliseconds
                double min = 100.0; // in milliseconds
                double t = ProbabilityOfSmallSaucer;
                return max + (min - max) * t;
            }
        }

        public const int MaxStartingAsteroids = 11;
        public const int MaxAsteroids = 27;
        public const int BulletLifeSpan = 1000; // in milliseconds
        public const int ExtraLifeScoreThreshold = 10000;
        public const int StartingLives = 5;
        public const int ShipFiringDelay = 200; // in milliseconds
        //public const double ShipRotationIncrement = 5.625; // in degrees
        public const double ShipRotationIncrement = 4.0; // in degrees
        public const double ShipDragFactor = 0.98;
        public const int ShipJumpDelay = 500; // in milliseconds
        public const double AsteroidRotationSpeedRange = 2.0; // in degrees per frame
        public const double AsteroidDistanceFromCenter = 1.5; // in large asteroid diameters

        public static double ScreenSize => Math.Min(ScreenWidth, ScreenHeight);

        // object sizes proportional to window size
        // all sizes are in pixels
        public static double AsteroidSizeFactor => 0.08 * ScreenSize;
        public static double ShipSizeFactor => 0.02 * ScreenSize;
        public static double SaucerSizeFactor => 0.025 * ScreenSize;
        public static double BulletRadius => 2.5; // 0.003*ScreenSize???

        // object velocities proportional to window size
        // all velocities in pixels per frame
        // the 1.7 value below is a fudge factor which seems to work
        public static double SpeedFactor => (double)ElapsedTime / (double)ExpectedTimeSpan / 1.7;
        public static double BulletSpeed => 0.018 * ScreenSize;
        public static double ShipMaxVelocity => 0.018 * ScreenSize;
        public static double ShipAcceleration => 0.0006 * ScreenSize;
        public static double AsteroidSplitVelocity => 0.0018 * ScreenSize;
        public static double SaucerXVelocity => 0.0025 * ScreenSize;
        public static double SaucerYVelocityRange => 0.01 * ScreenSize;
        public static double AsteroidVelocityRange
        {
            // ramp from 4 at level=1 to 8 at level=11
            get
            {
                double min = 0.005 * ScreenSize;
                double max = 0.01 * ScreenSize;
                double vel = (Level - 1) * (max - min) / 10.0 + min;
                return Math.Clamp(vel, min, max);
            }
        }

        /* Here is a comparison between the above sizes and velocities
         * to those in the original game
         * 
         * object length (in fractions of screen height)
         *                 mine  original
         * ship            0.04  0.04
         * small saucer    0.05  0.03
         * large saucer    0.10  0.06
         * large asteroid  0.16  0.096
         * medium asteroid 0.08  0.048
         * small asteroid  0.04  0.024
         * 
         * object max velocity (in screens/sec)
         *                 mine          original      other source
         * ship            0.67          0.68          1.25
         * small saucer    0.21          0.16 to 0.26  0.31 to 0.39
         * large saucer    0.21          0.16 to 0.26  0.16
         * large asteroid  0.09 to 0.19  0.16 to 0.26  0.08 to 0.16
         * medium asteroid 0.16 to 0.25  0.16 to 0.26  0.16 to 0.23
         * small asteroid  0.23 to 0.32  0.16 to 0.26  0.23 to 0.39
         * bullet          0.67          0.68          ?
         */
    }
}
