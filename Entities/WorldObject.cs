using System;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Visuals;

namespace HanlangGame.Entities
{
    public enum ObjectType
    {
        Club,     // 목봉 (빠르고 사거리 긴 무기)
        Crate,    // 나무 궤짝 (넓은 범위, 던지면 산산조각)
        Rock,     // 돌멩이 (단단하고 묵직한 투척 무기)
        Jar,      // 옹기 항아리 (깨지면 엽전이나 주먹밥 드롭)
        Barrel,   // 주막 술통 (굴러가며 적들을 차례로 넘어뜨림)
        Cart,     // 대형 달구지 수레 (크고 무거움, 투척/타격 시 굴러가며 다수 적 분쇄)
        Table,    // 대형 주막 탁자 / 서당 책상 (넓은 면적, 강력한 내려치기와 파쇄 투척)
        Bench,    // 대형 주막 평상 / 긴 의자 (광역 가로 휩쓸기 및 적 다운)
        RiceSack, // 볏짚 쌀가마니 (묵직한 타격, 충격 시 터지며 주먹밥/엽전 드롭)
        Arrow,    // 흑호단 궁수의 날아가는 화살 (패링 시 반사 가능!)
        Spear,    // 오랏줄 장창 (초장거리 찌르기, 관통, 내구도 5)
        Cudgel,   // 육각 쇠방망이 (묵직한 강타, 100% 스턴 및 가드 브레이크, 내구도 6)
        Fan,      // 선비 쇠살 부채 (광역 횡베기, 바람 장풍, 화살 쳐내기, 내구도 8)
        Hwacha    // 광화문 신기전 화차 (타격 시 전방으로 화전 일제 사격)
    }

    public class WorldObject : Entity
    {
        public ObjectType Type { get; set; }
        public bool IsCarried { get; set; } = false;
        public bool IsFlying { get; set; } = false;
        public bool IsRolling { get; set; } = false;
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
                case ObjectType.Jar:
                    Width = 16;
                    Height = 18;
                    Durability = 1;
                    break;
                case ObjectType.Barrel:
                    Width = 20;
                    Height = 22;
                    Durability = 3;
                    break;
                case ObjectType.Cart:
                    Width = 34;
                    Height = 22;
                    Durability = 4;
                    break;
                case ObjectType.Table:
                    Width = 30;
                    Height = 18;
                    Durability = 2;
                    break;
                case ObjectType.Bench:
                    Width = 32;
                    Height = 16;
                    Durability = 3;
                    break;
                case ObjectType.RiceSack:
                    Width = 24;
                    Height = 18;
                    Durability = 1;
                    break;
                case ObjectType.Arrow:
                    Width = 18;
                    Height = 6;
                    Durability = 1;
                    break;
                case ObjectType.Spear:
                    Width = 28;
                    Height = 10;
                    Durability = 5;
                    break;
                case ObjectType.Cudgel:
                    Width = 22;
                    Height = 10;
                    Durability = 6;
                    break;
                case ObjectType.Fan:
                    Width = 18;
                    Height = 10;
                    Durability = 8;
                    break;
                case ObjectType.Hwacha:
                    Width = 36;
                    Height = 24;
                    Durability = 999;
                    break;
            }
        }

        public override void Update(float dt)
        {
            if (IsCarried) return; // 플레이어가 들고 있을 때는 플레이어 위치를 따라감

            base.Update(dt);

            // 히트스탑(역경직) 중에는 물리 이동 및 상태 갱신 일시 정지
            if (IsInHitStop) return;

            Y = Math.Clamp(Y, 145f, 225f);

            if (IsRolling)
            {
                X += Vx * dt;
                Vx *= 0.985f;
                if (Math.Abs(Vx) < 15f)
                {
                    IsRolling = false;
                    Vx = 0f;
                }
            }

            if (IsFlying)
            {
                // 날아가는 동안 약간의 회전 또는 감속
                if (IsGrounded)
                {
                    IsFlying = false;
                    if (Type == ObjectType.Barrel || Type == ObjectType.Cart)
                    {
                        // 술통과 수레는 착지 후 관성으로 계속 굴러감!
                        IsRolling = true;
                        Vz = 0f;
                    }
                    else
                    {
                        Vx = 0f;
                        Vy = 0f;
                    }

                    // 착지 시 내구도 감소
                    Durability--;
                    if (Durability <= 0 || Type == ObjectType.RiceSack)
                    {
                        IsDead = true;
                        if (Type == ObjectType.Jar || Type == ObjectType.RiceSack || Type == ObjectType.Table)
                            SoundSynth.PlayShatter();
                        else
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
                ObjectType.Jar => "obj_jar",
                ObjectType.Barrel => "obj_barrel",
                ObjectType.Cart => "obj_cart",
                ObjectType.Table => "obj_table",
                ObjectType.Bench => "obj_bench",
                ObjectType.RiceSack => "obj_ricesack",
                ObjectType.Arrow => "obj_arrow",
                ObjectType.Spear => "obj_spear",
                ObjectType.Cudgel => "obj_cudgel",
                ObjectType.Fan => "obj_fan",
                ObjectType.Hwacha => "obj_hwacha",
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
