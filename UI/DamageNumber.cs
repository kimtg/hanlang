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

            int sx = (int)(X - cameraX);
            int sy = (int)Y;

            string text = $"-{Value}";

            // 피격 주체에 따른 시각적 구분:
            // 플레이어 피격: 강렬한 선홍색
            // 적 피격: 밝은 주황색 (치명타 시 샛노란색)
            Color color;
            if (IsPlayerHit)
            {
                color = Color.FromArgb(255, 65, 65);
            }
            else
            {
                color = (Value >= 24) ? Color.FromArgb(255, 235, 45) : Color.FromArgb(255, 175, 40);
            }

            int textW = text.Length * 7;
            int drawX = sx - textW / 2;

            bool isLarge = Value >= 24;
            buffer.DrawText(text, drawX, sy, color, large: isLarge);
        }
    }
}
