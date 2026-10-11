using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using HanlangGame.Engine;

namespace HanlangGame.UI
{
    public class SpeechBubble
    {
        public string Text { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float LifeTime { get; set; } = 1.0f; // 1초 지속
        public float MaxLife { get; set; } = 1.0f;
        public bool IsExpired => LifeTime <= 0;

        public SpeechBubble(string text, float x, float y)
        {
            Text = text;
            X = x;
            Y = y;
        }

        public void Update(float dt)
        {
            LifeTime -= dt;
            Y -= 15f * dt; // 위로 살짝 떠오름
        }

        public void Render(PixelBuffer buffer, float cameraX)
        {
            if (IsExpired) return;

            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            float scx = Perspective.ProjectScreenX(X, cameraX, Y);
            float scy = Y * PixelBuffer.Scale;

            var font = buffer.FontHeader;
            var size = g.MeasureString(Text, font);
            int boxW = (int)size.Width + 24;
            int boxH = (int)size.Height + 10;
            int boxX = (int)(scx - boxW / 2);
            boxX = Math.Clamp(boxX, 10, PixelBuffer.Width - boxW - 10);
            int boxY = (int)(scy - boxH - 10);

            // 말풍선 부드러운 그림자
            buffer.FillRoundedRect(boxX + 2, boxY + 2, boxW, boxH, 8f, Color.FromArgb(70, 0, 0, 0));

            // 말풍선 본체 (고해상도 둥근 사각형)
            buffer.FillRoundedRect(boxX, boxY, boxW, boxH, 8f, Color.FromArgb(252, 250, 235));
            buffer.DrawRoundedRect(boxX, boxY, boxW, boxH, 8f, Color.FromArgb(50, 40, 30), 2f);

            // 말풍선 꼬리 삼각 폴리곤
            int tailX = (int)scx;
            int tailY = (int)scy;
            Point[] tailPts =
            {
                new Point(tailX - 7, boxY + boxH - 1),
                new Point(tailX + 7, boxY + boxH - 1),
                new Point(tailX, tailY - 2)
            };
            using (var tailBrush = new SolidBrush(Color.FromArgb(252, 250, 235)))
            using (var borderPen = new Pen(Color.FromArgb(50, 40, 30), 1.8f))
            {
                g.FillPolygon(tailBrush, tailPts);
                g.DrawLine(borderPen, tailPts[0], tailPts[2]);
                g.DrawLine(borderPen, tailPts[1], tailPts[2]);
            }

            // 안티앨리어싱 고해상도 텍스트 출력
            buffer.DrawOutlinedText(Text, boxX + 12, boxY + 5, Color.FromArgb(170, 25, 25), Color.Transparent, font, 1f);
        }
    }
}
