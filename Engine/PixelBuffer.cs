using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace HanlangGame.Engine
{
    public class PixelBuffer
    {
        // 960 * 720 고해상도 렌더 버퍼
        public const int Width = 960;
        public const int Height = 720;

        // 기존 2.5D 게임 로직 / 물리 좌표계 (320 * 240)
        public const int VirtualWidth = 320;
        public const int VirtualHeight = 240;
        public const int Scale = 3;

        public Bitmap Bitmap { get; private set; }
        public Graphics Graphics { get; private set; }

        public Font FontHuge { get; }
        public Font FontTitle { get; }
        public Font FontSubtitle { get; }
        public Font FontHeader { get; }
        public Font FontBody { get; }
        public Font FontSmall { get; }
        public Font FontTiny { get; }

        public PixelBuffer()
        {
            Bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            Graphics = Graphics.FromImage(Bitmap);

            SetHighQualityModes();

            // 960x720 고해상도 글자용 안티앨리어싱 시스템 폰트 준비
            FontHuge = new Font("Batang", 34f, FontStyle.Bold);
            FontTitle = new Font("Malgun Gothic", 24f, FontStyle.Bold);
            FontSubtitle = new Font("Malgun Gothic", 18f, FontStyle.Bold);
            FontHeader = new Font("Malgun Gothic", 15f, FontStyle.Bold);
            FontBody = new Font("Malgun Gothic", 13f, FontStyle.Bold);
            FontSmall = new Font("Malgun Gothic", 11f, FontStyle.Regular);
            FontTiny = new Font("Malgun Gothic", 9.5f, FontStyle.Regular);
        }

        public void SetHighQualityModes()
        {
            Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        public void Clear(Color color)
        {
            Graphics.Clear(color);
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                using var brush = new SolidBrush(color);
                Graphics.FillRectangle(brush, x, y, 1, 1);
            }
        }

        public void FillRect(int x, int y, int w, int h, Color color)
        {
            using var brush = new SolidBrush(color);
            Graphics.FillRectangle(brush, x, y, w, h);
        }

        public void DrawRect(int x, int y, int w, int h, Color color, float lineWidth = 1f)
        {
            using var pen = new Pen(color, lineWidth);
            Graphics.DrawRectangle(pen, x, y, w, h);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float lineWidth = 1f)
        {
            using var pen = new Pen(color, lineWidth);
            Graphics.DrawLine(pen, x1, y1, x2, y2);
        }

        public GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public void FillRoundedRect(float x, float y, float w, float h, float radius, Color color)
        {
            using var path = CreateRoundedRectangle(new RectangleF(x, y, w, h), radius);
            using var brush = new SolidBrush(color);
            Graphics.FillPath(brush, path);
        }

        public void DrawRoundedRect(float x, float y, float w, float h, float radius, Color color, float lineWidth = 1.5f)
        {
            using var path = CreateRoundedRectangle(new RectangleF(x, y, w, h), radius);
            using var pen = new Pen(color, lineWidth);
            Graphics.DrawPath(pen, path);
        }

        public void DrawShadow(float logicCenterX, float logicGroundY, float logicRadiusX, float logicRadiusY)
        {
            float scx = logicCenterX * Scale;
            float scy = logicGroundY * Scale;
            float srx = logicRadiusX * Scale;
            float sry = logicRadiusY * Scale;

            using var brush = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
            Graphics.FillEllipse(brush, scx - srx, scy - sry, srx * 2, sry * 2);
        }

        public void DrawShadow(int logicCenterX, int logicGroundY, int logicRadiusX, int logicRadiusY)
            => DrawShadow((float)logicCenterX, (float)logicGroundY, (float)logicRadiusX, (float)logicRadiusY);


        public void DrawOutlinedText(string text, float x, float y, Color textColor, Color outlineColor, Font font, float outlineWidth = 2.5f)
        {
            using var outlinePen = new Pen(outlineColor, outlineWidth) { LineJoin = LineJoin.Round };
            using var path = new GraphicsPath();
            path.AddString(text, font.FontFamily, (int)font.Style, font.SizeInPoints * 1.333f, new PointF(x, y), StringFormat.GenericTypographic);
            Graphics.DrawPath(outlinePen, path);
            using var fillBrush = new SolidBrush(textColor);
            Graphics.FillPath(fillBrush, path);
        }

        public void DrawOutlinedTextCentered(string text, float y, Color textColor, Color outlineColor, Font font, float outlineWidth = 2.5f)
        {
            var size = Graphics.MeasureString(text, font);
            float x = (Width - size.Width) / 2f;
            DrawOutlinedText(text, x, y, textColor, outlineColor, font, outlineWidth);
        }

        public void DrawOutlinedTextCenteredAt(string text, float centerX, float y, Color textColor, Color outlineColor, Font font, float outlineWidth = 2.5f)
        {
            var size = Graphics.MeasureString(text, font);
            float x = centerX - (size.Width / 2f);
            DrawOutlinedText(text, x, y, textColor, outlineColor, font, outlineWidth);
        }

        public void DrawText(string text, int x, int y, Color color, bool large = false, bool tiny = false)
        {
            Font font = large ? FontSubtitle : (tiny ? FontSmall : FontHeader);
            
            // 고해상도 그림자
            using var shadowBrush = new SolidBrush(Color.FromArgb(180, 10, 10, 10));
            Graphics.DrawString(text, font, shadowBrush, x + 1.5f, y + 1.5f);

            // 본문 텍스트
            using var textBrush = new SolidBrush(color);
            Graphics.DrawString(text, font, textBrush, x, y);
        }

        public void DrawTextCentered(string text, int y, Color color, bool large = false, bool tiny = false)
        {
            Font font = large ? FontSubtitle : (tiny ? FontSmall : FontHeader);
            var size = Graphics.MeasureString(text, font);
            int x = (Width - (int)size.Width) / 2;
            DrawText(text, x, y, color, large, tiny);
        }

        public void DrawScanlines()
        {
            // 960x720 고해상도 모드에서는 화면 흐림을 유발하는 CRT 도트 스캔라인을 적용하지 않고
            // 맑고 선명한 고해상도 그래픽을 온전히 출력합니다.
        }

        // 오브젝트(캐릭터, 들 수 있는 물체, 음식, 돈)는 도트 그대로 NearestNeighbor 확대 출력 (원근 스케일 지원)
        public void DrawBitmap(Bitmap sprite, float logicX, float logicY, bool flipX = false, float scaleFactor = 1.0f)
        {
            if (sprite == null) return;

            var prevInterp = Graphics.InterpolationMode;
            var prevOffset = Graphics.PixelOffsetMode;

            Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            float sx = logicX * Scale;
            float sy = logicY * Scale;
            float sw = sprite.Width * Scale * scaleFactor;
            float sh = sprite.Height * Scale * scaleFactor;

            if (!flipX)
            {
                Graphics.DrawImage(sprite, sx, sy, sw, sh);
            }
            else
            {
                // 좌우 반전 출력
                Graphics.DrawImage(sprite, sx + sw, sy, -sw, sh);
            }

            Graphics.InterpolationMode = prevInterp;
            Graphics.PixelOffsetMode = prevOffset;
        }

        public void DrawBitmap(Bitmap sprite, int logicX, int logicY, bool flipX = false)
            => DrawBitmap(sprite, (float)logicX, (float)logicY, flipX, 1.0f);

    }
}
