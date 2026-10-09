using System;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Visuals;

namespace HanlangGame.Entities
{
    public enum ObjectType
    {
        Club,   // 목봉 (빠르고 사거리 긴 무기)
        Crate,  // 나무 궤짝 (넓은 범위, 던지면 산산조각)
        Rock    // 돌멩이 (단단하고 묵직한 투척 무기)
    }

    public class WorldObject : Entity
    {
        public ObjectType Type { get; set; }
        public bool IsCarried { get; set; } = false;
        public bool IsFlying { get; set; } = false;
        public bool ThrownByPlayer { get; set; } = true;
        public int Durability { get; set; } = 3;

        public WorldObject(ObjectType type, float x, float y)
        {
            Type = type;
            X = x;
            Y = y;
            Z = 0f;

            switch (type)
            {
                case ObjectType.Club:
                    Width = 20;
                    Height = 10;
                    Durability = 4;
                    break;
                case ObjectType.Crate:
                    Width = 18;
                    Height = 16;
                    Durability = 2;
                    break;
                case ObjectType.Rock:
                    Width = 16;
                    Height = 14;
                    Durability = 3;
                    break;
            }
        }

        public override void Update(float dt)
        {
            if (IsCarried) return; // 플레이어가 들고 있을 때는 플레이어 위치를 따라감

            base.Update(dt);

            Y = Math.Clamp(Y, 145f, 225f);

            if (IsFlying)
            {
                // 날아가는 동안 약간의 회전 또는 감속
                if (IsGrounded)
                {
                    IsFlying = false;
                    Vx = 0f;
                    Vy = 0f;
                    // 착지 시 내구도 감소
                    Durability--;
                    if (Durability <= 0)
                    {
                        IsDead = true;
                        SoundSynth.PlayHit();
                    }
                }
            }
        }

        public override void Render(PixelBuffer buffer, float cameraX)
        {
            if (IsDead) return;

            if (!IsCarried)
            {
                RenderShadow(buffer, cameraX);
            }

            float projX = Perspective.ProjectX(X, cameraX, Y);
            float p = Perspective.GetScale(Y);
            float drawW = Width * p;
            float drawH = Height * p;
            float sx = projX - drawW / 2f;
            float sy = (Y - Z) - drawH;

            string spriteKey = Type switch
            {
                ObjectType.Club => "obj_club",
                ObjectType.Crate => "obj_crate",
                ObjectType.Rock => "obj_rock",
                _ => "obj_club"
            };

            var sprite = RetroSprites.GetSprite(spriteKey);
            if (sprite != null)
            {
                buffer.DrawBitmap(sprite, sx, sy, flipX: !FacingRight, scaleFactor: p);
            }
        }
    }
}
