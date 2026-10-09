using System;
using System.Drawing;
using System.Windows.Forms;
using HanlangGame.Engine;
using HanlangGame.Visuals;
using HanlangGame.Systems;

namespace HanlangGame.Entities
{
    public class Player : Entity
    {
        public StatusSystem Status { get; set; }

        public float InvincibleTimer { get; set; } = 0f;
        public int ComboCount { get; set; } = 0;
        public float AttackTimer { get; set; } = 0f;

        // 들기 시스템 (물체 및 쓰러진 적)
        public WorldObject? CarriedObject { get; set; }
        public Enemy? CarriedEnemy { get; set; }
        public float CarriedEnemyTimer { get; set; } = 0f;
        public bool IsCarrying => CarriedObject != null || CarriedEnemy != null;

        // 공격 판정 플래그
        public bool HasDealtDamage { get; set; } = false;
        public string CurrentAttackType { get; set; } = "";

        public Player(StatusSystem status)
        {
            Status = status;
            X = 60f;
            Y = 180f;
            Z = 0f;
            Width = 24;
            Height = 32;
        }

        public void DropCarried()
        {
            if (CarriedObject != null)
            {
                CarriedObject.IsCarried = false;
                CarriedObject.Z = 0f;
                CarriedObject = null;
            }
            if (CarriedEnemy != null)
            {
                CarriedEnemy.IsCarried = false;
                CarriedEnemy.State = "down";
                CarriedEnemy.Z = 0f;
                CarriedEnemy = null;
                CarriedEnemyTimer = 0f;
            }
        }

        public Enemy? EscapeCarriedEnemy()
        {
            if (CarriedEnemy == null) return null;
            var enemy = CarriedEnemy;
            CarriedEnemy = null;
            CarriedEnemyTimer = 0f;
            enemy.IsCarried = false;
            enemy.Z = 0f;
            if (enemy.CurrentHp > 0)
            {
                enemy.State = "idle";
                enemy.StateTimer = 0f;
                enemy.InvincibleTimer = 0.5f;
            }
            else
            {
                enemy.State = "down";
            }
            enemy.Vx = (FacingRight ? 1f : -1f) * 45f;
            SoundSynth.PlayHit();
            return enemy;
        }

        public void ThrowCarried()
        {
            if (CarriedObject != null)
            {
                var obj = CarriedObject;
                CarriedObject = null;
                obj.IsCarried = false;
                obj.IsFlying = true;
                obj.ThrownByPlayer = true;
                obj.X = X + (FacingRight ? 16f : -16f);
                obj.Y = Y;
                obj.Z = Z + 20f;
                obj.Vx = (FacingRight ? 1f : -1f) * 270f;
                obj.Vz = 100f;
                SoundSynth.PlaySpecial();
            }
            else if (CarriedEnemy != null)
            {
                var enemy = CarriedEnemy;
                CarriedEnemy = null;
                CarriedEnemyTimer = 0f;
                enemy.IsCarried = false;
                enemy.IsFlying = true;
                enemy.X = X + (FacingRight ? 16f : -16f);
                enemy.Y = Y;
                enemy.Z = Z + 20f;
                enemy.Vx = (FacingRight ? 1f : -1f) * 260f;
                enemy.Vz = 100f;
                SoundSynth.PlaySpecial();
            }
        }

        public void HandleInput(Input input, System.Collections.Generic.List<WorldObject>? worldObjects = null, System.Collections.Generic.List<Enemy>? enemies = null)
        {
            if (State == "hit" || State == "down") return;

            // 1. 공중(점프) 상태일 때의 조작
            if (!IsGrounded)
            {
                // 공중에서 X키 누르면 들고 있는 것 투척
                if (input.IsJustPressed(Keys.X) && IsCarrying)
                {
                    ThrowCarried();
                    return;
                }
                else if ((input.IsJustPressed(Keys.Z) || input.IsJustPressed(Keys.X)) && State != "jumpkick")
                {
                    State = "jumpkick";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                    CurrentAttackType = "jumpkick";
                    SoundSynth.PlayKick();
                }

                // 공중 이동 (방향키 조작으로 부드러운 공중 기동 및 관성 유지)
                float airMoveX = 0f;
                if (input.IsHeld(Keys.Left)) airMoveX -= 1f;
                if (input.IsHeld(Keys.Right)) airMoveX += 1f;

                if (airMoveX != 0)
                {
                    FacingRight = airMoveX > 0;
                    float airSpeed = input.IsDashing ? Status.DashSpeed : Status.MoveSpeed;
                    Vx = airMoveX * airSpeed;
                }

                float airMoveY = 0f;
                if (input.IsHeld(Keys.Up)) airMoveY -= 1f;
                if (input.IsHeld(Keys.Down)) airMoveY += 1f;
                if (airMoveY != 0)
                {
                    Vy = airMoveY * (Status.MoveSpeed * 0.45f);
                }
                else
                {
                    Vy *= 0.85f;
                }

                return; // 공중 상태일 때는 지상 공격/보행 로직 건너뜀
            }

            // 2. 지상 공격 동작 중 이동 불가
            if (State == "punch" || State == "kick" || State == "spin" || State == "swing")
            {
                Vx *= 0.5f;
                Vy *= 0.5f;
                return;
            }

            // 3. 지상 Z키: 들고 있으면 '들고 때린다', 들 게 있으면 '주워 든다', 없으면 '주먹'
            if (input.IsJustPressed(Keys.Z))
            {
                if (IsCarrying)
                {
                    State = "swing";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                    CurrentAttackType = "swing";
                    SoundSynth.PlayKick();
                    return;
                }
                else
                {
                    // 1) 바닥에 있는 물체 줍기
                    if (worldObjects != null)
                    {
                        var nearbyObj = worldObjects.Find(o => !o.IsDead && !o.IsCarried && !o.IsFlying &&
                                                               Math.Abs(X - o.X) < 26f && Math.Abs(Y - o.Y) < 15f);
                        if (nearbyObj != null)
                        {
                            CarriedObject = nearbyObj;
                            nearbyObj.IsCarried = true;
                            SoundSynth.PlayJump();
                            return;
                        }
                    }

                    // 2) 바닥에 쓰러진 적 들기
                    if (enemies != null)
                    {
                        var nearbyDownEnemy = enemies.Find(e => !e.IsDead && !e.IsCarried && e.State == "down" &&
                                                               Math.Abs(X - e.X) < 28f && Math.Abs(Y - e.Y) < 16f);
                        if (nearbyDownEnemy != null)
                        {
                            CarriedEnemy = nearbyDownEnemy;
                            CarriedEnemyTimer = 0f;
                            nearbyDownEnemy.IsCarried = true;
                            SoundSynth.PlayJump();
                            return;
                        }
                    }

                    // 3) 주울 것이 없으면: 대시 중이면 대시 펀치(슬라이딩 정권), 아니면 일반 펀치!
                    if (input.IsDashing)
                    {
                        State = "dashpunch";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "dashpunch";
                        Vx = (FacingRight ? 1f : -1f) * 270f;
                        SoundSynth.PlaySpecial();
                        return;
                    }
                    else
                    {
                        State = "punch";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "punch";
                        ComboCount = (ComboCount % 3) + 1;
                        SoundSynth.PlayPunch();
                        return;
                    }
                }
            }
            // 4. 지상 X키: 들고 있으면 '던진다', 대시 중이면 '대시 킥(돌진 날아차기)', 없으면 '발차기'
            else if (input.IsJustPressed(Keys.X))
            {
                if (IsCarrying)
                {
                    ThrowCarried();
                    return;
                }
                else
                {
                    if (input.IsDashing)
                    {
                        State = "dashkick";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "dashkick";
                        Vx = (FacingRight ? 1f : -1f) * 250f;
                        SoundSynth.PlayKick();
                        return;
                    }
                    else
                    {
                        State = "kick";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "kick";
                        ComboCount = 0;
                        SoundSynth.PlayKick();
                        return;
                    }
                }
            }
            else if (input.IsJustPressed(Keys.Space))
            {
                if (Status.CurrentMp >= 15)
                {
                    DropCarried();
                    Status.CurrentMp -= 15;
                    State = "spin";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                    CurrentAttackType = "spin";
                    SoundSynth.PlaySpecial();
                    return;
                }
            }

            // 4. 지상 이동 입력 계산
            float moveX = 0f;
            float moveY = 0f;

            if (input.IsHeld(Keys.Left)) moveX -= 1f;
            if (input.IsHeld(Keys.Right)) moveX += 1f;
            if (input.IsHeld(Keys.Up)) moveY -= 1f;
            if (input.IsHeld(Keys.Down)) moveY += 1f;

            if (moveX != 0) FacingRight = moveX > 0;

            float speed = input.IsDashing ? Status.DashSpeed : Status.MoveSpeed;

            if (moveX != 0 && moveY != 0)
            {
                moveX *= 0.7071f;
                moveY *= 0.7071f;
            }

            // 5. 점프 입력 (C 키) - 지상에서만 발동
            if (input.IsJustPressed(Keys.C))
            {
                Vz = Status.JumpPower;
                State = "jump";
                StateTimer = 0f;
                SoundSynth.PlayJump();

                // 점프 시 관성 반영 (대시 점프 / 전진 점프 / 제자리 점프)
                if (input.IsDashing)
                {
                    Vx = (FacingRight ? 1f : -1f) * Status.DashSpeed;
                }
                else if (moveX != 0)
                {
                    Vx = moveX * Status.MoveSpeed;
                }
                else
                {
                    Vx = 0f;
                }
                Vy = moveY * (speed * 0.5f);
                return;
            }

            // 6. 지상 보행/달리기 이동 적용
            Vx = moveX * speed;
            Vy = moveY * (speed * 0.75f);

            if (moveX != 0 || moveY != 0)
            {
                State = input.IsDashing ? "run" : "walk";
            }
            else
            {
                State = "idle";
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (InvincibleTimer > 0)
            {
                InvincibleTimer -= dt;
            }

            // 길목 경계선 제한 (한양 저잣거리 바닥 깊이: Y 145 ~ 225)
            Y = Math.Clamp(Y, 145f, 225f);
            if (X < 14f) X = 14f;

            // 공중 및 지면 착지 상태 갱신
            if (IsGrounded)
            {
                if (State == "jump" || State == "jumpkick")
                {
                    State = "idle";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                }
            }
            else
            {
                // 공중 체공 중일 때는 날라차기나 피격이 아닌 경우 점프 상태 보장
                if (State != "jumpkick" && State != "hit" && State != "down")
                {
                    State = "jump";
                }
            }

            // 들고 있는 물체 및 쓰러진 적의 위치 동기화 (머리 위 또는 스윙 타격 위치)
            if (CarriedObject != null)
            {
                if (State == "swing")
                {
                    CarriedObject.X = X + (FacingRight ? 14f : -14f);
                    CarriedObject.Y = Y;
                    CarriedObject.Z = Z + 8f;
                }
                else
                {
                    CarriedObject.X = X;
                    CarriedObject.Y = Y;
                    CarriedObject.Z = Z + Height + 2f;
                }
                CarriedObject.FacingRight = FacingRight;
            }
            if (CarriedEnemy != null)
            {
                CarriedEnemyTimer += dt;
                if (CarriedEnemyTimer >= 3.0f)
                {
                    EscapeCarriedEnemy();
                }
                else
                {
                    if (State == "swing")
                    {
                        CarriedEnemy.X = X + (FacingRight ? 14f : -14f);
                        CarriedEnemy.Y = Y;
                        CarriedEnemy.Z = Z + 8f;
                    }
                    else
                    {
                        CarriedEnemy.X = X;
                        CarriedEnemy.Y = Y;
                        CarriedEnemy.Z = Z + Height + 2f;
                    }
                    CarriedEnemy.FacingRight = FacingRight;
                }
            }

            // 상태별 타이머 진행
            if (State == "dashpunch")
            {
                Vx *= 0.94f;
                if (StateTimer > 0.28f) State = "idle";
            }
            else if (State == "dashkick")
            {
                Vx *= 0.94f;
                if (StateTimer > 0.32f) State = "idle";
            }
            else if (State == "punch")
            {
                if (StateTimer > 0.22f) State = "idle";
            }
            else if (State == "kick")
            {
                if (StateTimer > 0.28f) State = "idle";
            }
            else if (State == "swing")
            {
                Vx *= 0.5f;
                if (StateTimer > 0.28f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                }
            }
            else if (State == "spin")
            {
                Vx = (FacingRight ? 1f : -1f) * 60f;
                if (StateTimer > 0.45f) State = "idle";
            }
            else if (State == "hit")
            {
                Vx *= 0.85f;
                if (StateTimer > 0.35f) State = "idle";
            }
            else if (State == "down")
            {
                Vx *= 0.8f;
                if (StateTimer > 0.75f)
                {
                    State = "idle";
                    InvincibleTimer = 1.0f; // 기상 후 1초 무적
                }
            }

            // 자연 MP 완만 회복
            if (Status.CurrentMp < Status.MaxMp)
            {
                Status.CurrentMp = Math.Min(Status.MaxMp, Status.CurrentMp + (int)(dt * 2));
            }
        }

        public RectangleF? GetAttackHitbox()
        {
            if (State != "punch" && State != "kick" && State != "jumpkick" && State != "spin" && State != "swing" && State != "dashpunch" && State != "dashkick")
                return null;

            float reach = (State == "kick" || State == "jumpkick" || State == "dashkick") ? 22f : 16f;
            if (State == "spin" || State == "swing" || State == "dashpunch") reach = 28f;

            float attackX = FacingRight ? X + 4f : X - 4f - reach;
            if (State == "spin") attackX = X - reach / 2f;

            if (State == "jumpkick")
            {
                // 공중 날라차기: 발끝부터 지상 적의 몸통/머리까지 닿도록 하단으로 판정 확장
                float attackY = Y - Z - 12f;
                float attackHeight = 24f + Z; // 높이 뜰수록 지면까지 판정 연결
                return new RectangleF(attackX, attackY, reach + 8f, attackHeight);
            }
            else
            {
                float attackY = Y - Z - 24f;
                return new RectangleF(attackX, attackY, reach + 6f, 22f);
            }
        }

        public void TakeDamage(int damage, float knockbackDir)
        {
            if (InvincibleTimer > 0 || State == "down") return;

            DropCarried(); // 피격 시 들고 있던 물체/적을 놓침

            Status.CurrentHp -= damage;
            SoundSynth.PlayHit();

            if (Status.CurrentHp <= 0)
            {
                Status.CurrentHp = 0;
                State = "down";
                StateTimer = 0f;
                Vz = 120f;
                Vx = knockbackDir * 100f;
            }
            else
            {
                State = "hit";
                StateTimer = 0f;
                InvincibleTimer = 0.6f;
                Vx = knockbackDir * 80f;
            }
        }

        public override void Render(PixelBuffer buffer, float cameraX)
        {
            // 무적 깜빡임
            if (InvincibleTimer > 0 && ((int)(InvincibleTimer * 12) % 2 == 0))
            {
                return;
            }

            RenderShadow(buffer, cameraX);

            string spriteKey = State switch
            {
                "swing" => "player_swing",
                "dashpunch" => "player_punch_2",
                "dashkick" => "player_kick_2",
                "punch" => $"player_punch_{AnimFrame}",
                "kick" => $"player_kick_{AnimFrame}",
                "jumpkick" => "player_jumpkick",
                "spin" => $"player_spin_{AnimFrame}",
                "jump" => IsCarrying ? "player_carry_jump" : "player_jump",
                "walk" or "run" => IsCarrying ? $"player_carry_walk_{AnimFrame}" : $"player_walk_{AnimFrame}",
                "hit" => "player_hit",
                "down" => "player_down",
                _ => (IsCarrying ? (IsGrounded ? "player_carry_idle" : "player_carry_jump") : (!IsGrounded ? "player_jump" : $"player_idle_{AnimFrame}"))
            };

            var sprite = RetroSprites.GetSprite(spriteKey);
            if (sprite != null)
            {
                float projX = Perspective.ProjectX(X, cameraX, Y);
                float p = Perspective.GetScale(Y);
                float w = sprite.Width * p;
                float h = sprite.Height * p;
                float sx = projX - w / 2f;
                float sy = (Y - Z) - h;
                buffer.DrawBitmap(sprite, sx, sy, flipX: !FacingRight, scaleFactor: p);
            }

            // ==========================================
            // 비기 훈민선풍각 화려한 광(빛) 효과 & 선풍 아우라 렌더링
            // ==========================================
            if (State == "spin")
            {
                RenderHunminSpinGlow(buffer, cameraX);
            }
        }

        private void RenderHunminSpinGlow(PixelBuffer buffer, float cameraX)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            float scx = Perspective.ProjectScreenX(X, cameraX, Y);
            float scy = Perspective.ProjectScreenY(Y - 15f, Z);
            float sGroundY = Perspective.ProjectScreenY(Y, 0f);

            // 1. 지면 고해상도 선풍 회오리 풍압륜 (Ground Wind Cyclone)
            float gwPulse = (24f + (float)Math.Sin(StateTimer * 25f) * 6f) * PixelBuffer.Scale;
            using (var pGroundWind = new Pen(Color.FromArgb(160, 110, 230, 255), 3f))
            {
                g.DrawEllipse(pGroundWind, scx - gwPulse, sGroundY - 10f, gwPulse * 2f, 20f);
            }

            // 2. 비기 천상 선풍 황금/청색 이중 광륜 (Radiant Celestial Aura Rings)
            float rx = (24f + (float)Math.Sin(StateTimer * 30f) * 5f) * PixelBuffer.Scale;
            float ry = (18f + (float)Math.Cos(StateTimer * 30f) * 4f) * PixelBuffer.Scale;
            using (var pGoldHalo = new Pen(Color.FromArgb(230, 255, 230, 70), 4f))
            using (var pCyanHalo = new Pen(Color.FromArgb(200, 80, 220, 255), 2.5f))
            {
                g.DrawEllipse(pGoldHalo, scx - rx, scy - ry, rx * 2f, ry * 2f);
                g.DrawEllipse(pCyanHalo, scx - (rx - 8f), scy - (ry - 6f), (rx - 8f) * 2f, (ry - 6f) * 2f);
            }

            // 3. 고속 회전 6방향 고해상도 빛의 검무 (Rotating Radiant Light Blades)
            float rotAngle = StateTimer * 36f;
            for (int i = 0; i < 6; i++)
            {
                float angle = rotAngle + i * ((float)Math.PI / 3f);
                float cosA = (float)Math.Cos(angle);
                float sinA = (float)Math.Sin(angle);

                float x1 = scx + cosA * 15f;
                float y1 = scy + sinA * 10f;
                float x2 = scx + cosA * (75f + (float)Math.Sin(StateTimer * 20f) * 10f);
                float y2 = scy + sinA * (55f + (float)Math.Cos(StateTimer * 20f) * 8f);

                using var coreBeam = new Pen(Color.FromArgb(255, 255, 250, 180), 3f);
                using var cyanBeam = new Pen(Color.FromArgb(160, 100, 225, 255), 5f);
                g.DrawLine(cyanBeam, x1, y1, x2, y2);
                g.DrawLine(coreBeam, x1, y1, x2, y2);
            }

            // 4. 훈민정음 자모 비급(ㄱ, ㄴ, ㄷ, ㄹ, ㅁ, ㅅ, ㅇ, ㅈ) 궤도 공전 광채 텍스트
            string[] jamo = { "ㄱ", "ㄴ", "ㄷ", "ㄹ", "ㅁ", "ㅅ", "ㅇ", "ㅈ" };
            for (int p = 0; p < jamo.Length; p++)
            {
                float sparkAngle = -rotAngle * 1.1f + p * ((float)Math.PI * 2f / jamo.Length);
                float sparkR = (22f + ((p % 2 == 0) ? 6f : -4f)) * PixelBuffer.Scale;
                float px = scx + (float)Math.Cos(sparkAngle) * sparkR;
                float py = scy + (float)Math.Sin(sparkAngle) * (sparkR * 0.72f);

                Color jamoCol = (p % 2 == 0) ? Color.FromArgb(255, 245, 90) : Color.FromArgb(130, 240, 255);
                buffer.DrawOutlinedText(jamo[p], px - 8f, py - 10f, jamoCol, Color.FromArgb(40, 20, 10), buffer.FontHeader, 2f);

                // 반짝이는 별빛 파티클
                using var starBrush = new SolidBrush(Color.White);
                g.FillEllipse(starBrush, px - 2f, py - 2f, 4f, 4f);
            }
        }
    }
}
