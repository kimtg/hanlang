using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace HanlangGame.Engine
{
    public class PixelBuffer
    {
        public const int Width = 320;
        public const int Height = 240;

        public Bitmap Bitmap { get; private set; }
        public Graphics Graphics { get; private set; }

        private Font _tinyFont;
        private Font _smallFont;
        private Font _titleFont;
        private Font _systemFont;

        public PixelBuffer()
        {
            Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            Graphics = Graphics.FromImage(Bitmap);
            Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            Graphics.SmoothingMode = SmoothingMode.None;
            Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            // 한글 렌더링에 적합한 시스템 폰트 준비
            _tinyFont = new Font("Malgun Gothic", 7.5f, FontStyle.Bold);
            _smallFont = new Font("Malgun Gothic", 9f, FontStyle.Bold);
            _titleFont = new Font("Malgun Gothic", 12f, FontStyle.Bold);
            _systemFont = new Font("Dotum", 8f, FontStyle.Regular);
        }

        public void Clear(Color color)
        {
            Graphics.Clear(color);
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                // direct drawing via small rect
                using var brush = new SolidBrush(color);
                Graphics.FillRectangle(brush, x, y, 1, 1);
            }
        }

        public void FillRect(int x, int y, int w, int h, Color color)
        {
            using var brush = new SolidBrush(color);
            Graphics.FillRectangle(brush, x, y, w, h);
        }

        public void DrawRect(int x, int y, int w, int h, Color color)
        {
            using var pen = new Pen(color, 1);
            Graphics.DrawRectangle(pen, x, y, w - 1, h - 1);
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Color color)
        {
            using var pen = new Pen(color, 1);
            Graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        public void DrawShadow(int centerX, int groundY, int radiusX, int radiusY)
        {
            using var brush = new SolidBrush(Color.FromArgb(90, 0, 0, 0));
            Graphics.FillEllipse(brush, centerX - radiusX, groundY - radiusY, radiusX * 2, radiusY * 2);
        }

        public void DrawText(string text, int x, int y, Color color, bool large = false, bool tiny = false)
        {
            Font font = large ? _titleFont : (tiny ? _tinyFont : _smallFont);
            
            // 1픽셀 그림자 (레트로 아웃라인 효과)
            using var shadowBrush = new SolidBrush(Color.FromArgb(200, 20, 20, 20));
            Graphics.DrawString(text, font, shadowBrush, x + 1, y + 1);

            // 본문 텍스트
            using var textBrush = new SolidBrush(color);
            Graphics.DrawString(text, font, textBrush, x, y);
        }

        public void DrawTextCentered(string text, int y, Color color, bool large = false, bool tiny = false)
        {
            Font font = large ? _titleFont : (tiny ? _tinyFont : _smallFont);
            var size = Graphics.MeasureString(text, font);
            int x = (Width - (int)size.Width) / 2;
            DrawText(text, x, y, color, large, tiny);
        }

        public void DrawScanlines()
        {
            using var pen = new Pen(Color.FromArgb(35, 0, 0, 0), 1);
            for (int y = 0; y < Height; y += 2)
            {
                Graphics.DrawLine(pen, 0, y, Width, y);
            }
        }

        public void DrawBitmap(Bitmap sprite, int x, int y, bool flipX = false)
        {
            if (sprite == null) return;
            if (!flipX)
            {
                Graphics.DrawImage(sprite, x, y);
            }
            else
            {
                // 좌우 반전 출력
                var destRect = new Rectangle(x, y, sprite.Width, sprite.Height);
                Graphics.DrawImage(sprite, new Rectangle(x + sprite.Width, y, -sprite.Width, sprite.Height));
            }
        }
    }
}
