using System;
using System.Drawing;
using HanlangGame.Engine;

namespace HanlangGame.Entities
{
    public abstract class Entity
    {
        // 2.5D 좌표계: X(좌우), Y(깊이 바닥), Z(높이/공중)
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Vx { get; set; }
        public float Vy { get; set; }
        public float Vz { get; set; }

        public bool FacingRight { get; set; } = true;
        public int Width { get; set; } = 28;
        public int Height { get; set; } = 32;

        public bool IsDead { get; set; } = false;

        // Z가 0에 가깝고 상승 중이지 않을 때 착지 상태로 판정
        public bool IsGrounded => Z <= 0.001f && Vz <= 0.001f;

        // 애니메이션 및 상태
        public string State { get; set; } = "idle";
        public float StateTimer { get; set; } = 0f;
        public int AnimFrame { get; set; } = 0;

        public virtual RectangleF GetBaseHitbox()
        {
            // 바닥 충돌 및 2.5D 위치 판정
            return new RectangleF(X - Width / 2f, Y - 6f, Width, 12f);
        }

        public virtual RectangleF GetBodyHitbox()
        {
            // 타격 피격 판정 (높이 Z 포함)
            return new RectangleF(X - Width / 2f, Y - Z - Height, Width, Height);
        }

        public virtual void Update(float dt)
        {
            // 2.5D 물리 이동
            X += Vx * dt;
            Y += Vy * dt;
            Z += Vz * dt;

            // 중력 및 지면 착지 판정
            if (Z <= 0f)
            {
                Z = 0f;
                Vz = 0f;
            }
            else
            {
                Vz -= 550f * dt; // 중력 가속도
            }

            StateTimer += dt;

            // 60FPS 환경에 맞춘 부드러운 4프레임 보행/달리기, 호흡 및 4프레임 공격 애니메이션
            if (State == "walk" || State == "run")
            {
                float animSpeed = (State == "run") ? 12f : 8f;
                AnimFrame = (int)(StateTimer * animSpeed) % 4;
            }
            else if (State == "idle")
            {
                AnimFrame = (int)(StateTimer * 2.5f) % 2;
            }
            else if (State == "punch")
            {
                // 4프레임 주먹 지르기 (0: 당기기, 1: 중간찌르기, 2: 최대타격, 3: 회수)
                AnimFrame = Math.Clamp((int)(StateTimer / 0.05f), 0, 3);
            }
            else if (State == "kick")
            {
                // 4프레임 발차기 (0: 무릎들기, 1: 중간뻗기, 2: 최대앞차기, 3: 접기)
                AnimFrame = Math.Clamp((int)(StateTimer / 0.065f), 0, 3);
            }
            else if (State == "spin")
            {
                // 비기 훈민선풍각: 초고속 4프레임 회전 회오리
                AnimFrame = (int)(StateTimer * 16f) % 4;
            }
            else
            {
                AnimFrame = 0;
            }
        }

        public virtual void RenderShadow(PixelBuffer buffer, float cameraX)
        {
            int screenX = (int)(X - cameraX);
            int screenY = (int)Y;
            // 공중에 높이 뜰수록 그림자가 약간 작아짐
            int rX = Math.Max(4, 12 - (int)(Z * 0.1f));
            int rY = Math.Max(2, 4 - (int)(Z * 0.05f));
            buffer.DrawShadow(screenX, screenY, rX, rY);
        }

        public abstract void Render(PixelBuffer buffer, float cameraX);
    }
}
