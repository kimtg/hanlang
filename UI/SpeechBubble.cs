using System;
using System.Drawing;
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

            int sx = (int)(X - cameraX);
            int sy = (int)Y;

            // 말풍선 배경 (레트로 노란색/흰색 박스 + 검정 테두리)
            int textW = Text.Length * 12 + 8;
            int textH = 14;
            int boxX = sx - textW / 2;
            int boxY = sy - textH;

            buffer.FillRect(boxX, boxY, textW, textH, Color.FromArgb(255, 255, 210));
            buffer.DrawRect(boxX, boxY, textW, textH, Color.FromArgb(40, 40, 40));

            // 말풍선 꼬리
            buffer.FillRect(sx - 1, sy, 3, 2, Color.FromArgb(255, 255, 210));
            buffer.SetPixel(sx, sy + 2, Color.FromArgb(40, 40, 40));

            // 텍스트 출력
            buffer.DrawText(Text, boxX + 4, boxY + 1, Color.FromArgb(160, 20, 20));
        }
    }
}
