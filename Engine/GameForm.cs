using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HanlangGame.Visuals;

namespace HanlangGame.Engine
{
    public class GameForm : Form
    {
        private readonly GameScene _scene;
        private readonly Stopwatch _stopwatch = new();
        private long _lastTicks;

        public const int ScaleFactor = 3; // 3배 정수 스케일링
        public const int WindowWidth = PixelBuffer.Width * ScaleFactor;   // 960
        public const int WindowHeight = PixelBuffer.Height * ScaleFactor; // 720

        public GameForm()
        {
            Text = "한랑 (韓浪) : 훈민정음 탈환기";
            ClientSize = new Size(WindowWidth, WindowHeight);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;

            // 더블 버퍼링 설정
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            RetroSprites.Initialize();

            _scene = new GameScene();
            _scene.Initialize();

            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            _stopwatch.Start();
            _lastTicks = _stopwatch.ElapsedTicks;

            // 60 FPS 타이머 설정 (~16.6ms)
            var gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoopStep;
            gameTimer.Start();
        }

        private void GameLoopStep(object? sender, EventArgs e)
        {
            long currentTicks = _stopwatch.ElapsedTicks;
            float dt = (float)(currentTicks - _lastTicks) / Stopwatch.Frequency;
            _lastTicks = currentTicks;

            // dt clamp (창 이동 등으로 인한 지연 방지)
            if (dt > 0.05f) dt = 0.05f;

            _scene.Update(dt);
            _scene.Render();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.None;

            // 320x240 -> 960x720 칼 같은 도트 정수 확대
            g.DrawImage(_scene.Buffer.Bitmap,
                        new Rectangle(0, 0, WindowWidth, WindowHeight),
                        new Rectangle(0, 0, PixelBuffer.Width, PixelBuffer.Height),
                        GraphicsUnit.Pixel);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            _scene.Input.OnKeyDown(e.KeyCode);
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            _scene.Input.OnKeyUp(e.KeyCode);
        }
    }
}
