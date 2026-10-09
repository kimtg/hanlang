using System;
using System.Drawing;
using HanlangGame.Engine;

namespace HanlangGame.UI
{
    public class DamageNumber
    {
        public int Value { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Vy { get; set; } = -42f; // 위로 떠오르는 초기 속도
        public float LifeTime { get; set; } = 0.85f;
        public float MaxLife { get; set; } = 0.85f;
        public bool IsPlayerHit { get; set; }
        public bool IsExpired => LifeTime <= 0;

        public DamageNumber(int value, float x, float y, bool isPlayerHit)
        {
            Value = value;
            X = x;
            Y = y;
            IsPlayerHit = isPlayerHit;
        }

        public void Update(float dt)
        {
            LifeTime -= dt;
            Y += Vy * dt; // 위로 솟아오름
            Vy *= 0.94f; // 완만하게 감속
        }

        public void Render(PixelBuffer buffer, float cameraX)
        {
            if (IsExpired) return;

            buffer.SetHighQualityModes();

            float p = Perspective.GetScale(Y);
            float scx = Perspective.ProjectScreenX(X, cameraX, Y);
            float scy = Y * PixelBuffer.Scale;

            string text = $"-{Value}";

            Color color;
            if (IsPlayerHit)
            {
                color = Color.FromArgb(255, 75, 75);
            }
            else
            {
                color = (Value >= 24) ? Color.FromArgb(255, 235, 60) : Color.FromArgb(255, 185, 45);
            }

            Font font = (Value >= 24) ? buffer.FontTitle : buffer.FontSubtitle;
            float textW = buffer.Graphics.MeasureString(text, font).Width;

            // 선명한 고해상도 아웃라인 드롭섀도우와 함께 렌더링
            buffer.DrawOutlinedText(text, scx - textW / 2f, scy, color, Color.FromArgb(200, 20, 20, 20), font, 3.5f);
        }
    }
}
