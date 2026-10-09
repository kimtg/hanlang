using System;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Visuals;

namespace HanlangGame.Entities
{
    public enum ItemType
    {
        Coin,       // 엽전 (골드 획득)
        RiceBall,   // 주먹밥 (HP 30 회복)
        Scroll      // 자모 두루마리 (EXP 40 획득)
    }

    public class Item : Entity
    {
        public ItemType Type { get; set; }
        public float LifeTime { get; set; } = 12f; // 12초 후 소멸

        public Item(float x, float y, ItemType type)
        {
            X = x;
            Y = y;
            Z = 15f; // 공중에서 튀어 나옴
            Vz = 120f;
            Vx = (float)(new Random().NextDouble() * 80 - 40);
            Vy = (float)(new Random().NextDouble() * 30 - 15);
            Type = type;
            Width = 12;
            Height = 12;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            LifeTime -= dt;
            if (LifeTime <= 0)
            {
                IsDead = true;
            }

            // 바닥에 닿으면 감속
            if (IsGrounded)
            {
                Vx *= 0.85f;
                Vy *= 0.85f;
            }
        }

        public override void Render(PixelBuffer buffer, float cameraX)
        {
            if (IsDead) return;

            // 소멸 직전 깜빡임
            if (LifeTime < 3f && ((int)(LifeTime * 8) % 2 == 0))
            {
                return;
            }

            RenderShadow(buffer, cameraX);

            float projX = Perspective.ProjectX(X, cameraX, Y);
            float p = Perspective.GetScale(Y);
            float drawW = Width * p;
            float drawH = Height * p;
            float sx = projX - drawW / 2f;
            float sy = (Y - Z) - drawH;

            Bitmap? sprite = Type switch
            {
                ItemType.Coin => RetroSprites.GetSprite("item_coin"),
                ItemType.RiceBall => RetroSprites.GetSprite("item_rice"),
                ItemType.Scroll => RetroSprites.GetSprite("item_scroll"),
                _ => null
            };

            if (sprite != null)
            {
                buffer.DrawBitmap(sprite, sx, sy, flipX: false, scaleFactor: p);
            }
        }
    }
}
