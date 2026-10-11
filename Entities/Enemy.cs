using System;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Visuals;
using HanlangGame.Systems;

namespace HanlangGame.Entities
{
    public enum EnemyType
    {
        Thug,        // 골목 깡패
        Scholar,     // 불량 훈도
        Ninja,       // 암흑 결사대 (후반부 정예)
        Archer,      // 흑호단 궁수 (원거리 화살 견제)
        ShieldGuard, // 철갑 방패무사 (전방 가드 / 가드 브레이크)
        MidBoss1,    // 3장 중간보스: 괴력 건달 마달
        MidBoss2,    // 6장 중간보스: 무과 배신자 철웅
        Boss         // 9장 최종 보스: 마도학사 흑호
    }

    public class Enemy : Entity
    {
        public EnemyType Type { get; set; }
        public string Name { get; set; } = "";
        public int MaxHp { get; set; }
        public int CurrentHp { get; set; }
        public int AttackPower { get; set; }
        public int ExpReward { get; set; }
        public int GoldReward { get; set; }
        public int ShieldHp { get; set; } = 0; // 철갑 방패 내구도

        public Action<float, float>? OnWallBounce;
        public Action<Enemy, int>? OnThrownLanded;

        public bool IsBossType => Type == EnemyType.Boss || Type == EnemyType.MidBoss1 || Type == EnemyType.MidBoss2;
        public bool IsRaging { get; set; } = false;
        public bool IsAlive => !IsDead && CurrentHp > 0;

        public bool IsCarried { get; set; } = false;
        public bool IsFlying { get; set; } = false;
        public float InvincibleTimer { get; set; } = 0f;

        public WorldObject? CarriedObject { get; set; }
        public bool IsCarrying => CarriedObject != null;
        private float _throwCooldown = 0f;

        private float _aiDecisionTimer = 0f;
        private string _aiMode = "chase";
        private float _attackCooldown = 0.2f;

        public bool HasDealtDamage { get; set; } = false;

        // 피격 시 넉다운(쓰러짐) 확률 (기본 20%)
        public float KnockdownChance { get; set; } = 0.20f;
        public Random KnockdownRng { get; set; } = new Random();

        private static readonly string[] HitQuotes = 
        { 
            "억!", "사맛디!", "따끔!", "어엿비너겨!", "아이고!", "나랏말싸미!", "크악!", "분하도다!" 
        };

        public void DropCarried()
        {
            if (CarriedObject != null)
            {
                CarriedObject.IsCarried = false;
                CarriedObject.Z = 0f;
                CarriedObject = null;
            }
        }

        public void ThrowCarried(Player player)
        {
            if (CarriedObject == null) return;
            var obj = CarriedObject;
            CarriedObject = null;
            obj.IsCarried = false;
            obj.IsFlying = true;
            obj.ThrownByPlayer = false; // 적이 던진 물체!
            obj.X = X + (FacingRight ? 16f : -16f);
            obj.Y = Y;
            obj.Z = Z + 18f;
            obj.Vx = (FacingRight ? 1f : -1f) * 230f;
            obj.Vz = 75f;
            SoundSynth.PlaySpecial();
            State = "punch";
            StateTimer = 0f;
        }

        public void ThrowSpawnedRock(Player player, System.Collections.Generic.List<WorldObject>? worldObjects)
        {
            if (worldObjects == null) return;
            var rock = new WorldObject(ObjectType.Rock, X + (FacingRight ? 16f : -16f), Y)
            {
                IsFlying = true,
                ThrownByPlayer = false,
                Z = Z + 16f,
                Vx = (FacingRight ? 1f : -1f) * 220f,
                Vz = 70f,
                Durability = 1
            };
            worldObjects.Add(rock);
            SoundSynth.PlaySpecial();
            State = "punch";
            StateTimer = 0f;
        }

        public void ShootArrow(Player player, System.Collections.Generic.List<WorldObject>? worldObjects)
        {
            if (worldObjects == null) return;
            FacingRight = player.X >= X;
            var arrow = new WorldObject(ObjectType.Arrow, X + (FacingRight ? 16f : -16f), Y)
            {
                IsFlying = true,
                ThrownByPlayer = false,
                Z = Z + 14f,
                Vx = (FacingRight ? 1f : -1f) * 260f,
                Vz = 0f,
                Durability = 1,
                FacingRight = FacingRight
            };
            worldObjects.Add(arrow);
            SoundSynth.PlayWhoosh();
            State = "punch";
            StateTimer = 0f;
        }

        public Enemy(EnemyType type, float x, float y, int stage = 1)
        {
            Type = type;
            X = x;
            Y = y;
            Z = 0f;
            Width = 26;
            Height = 32;

            int stageBonusHp = (stage - 1) * 8;
            int stageBonusAtk = (stage - 1) * 2;

            switch (type)
            {
                case EnemyType.Thug:
                    Name = "골목 깡패";
                    MaxHp = 45 + stageBonusHp;
                    AttackPower = 7 + stageBonusAtk;
                    ExpReward = 30 + stage * 5;
                    GoldReward = 18 + stage * 3;
                    break;

                case EnemyType.Scholar:
                    Name = "불량 훈도";
                    MaxHp = 65 + stageBonusHp;
                    AttackPower = 11 + stageBonusAtk;
                    ExpReward = 50 + stage * 6;
                    GoldReward = 28 + stage * 4;
                    break;

                case EnemyType.Ninja:
                    Name = "암흑 결사대";
                    MaxHp = 90 + stageBonusHp;
                    AttackPower = 15 + stageBonusAtk;
                    ExpReward = 75 + stage * 8;
                    GoldReward = 40 + stage * 5;
                    break;

                case EnemyType.Archer:
                    Name = "흑호단 궁수";
                    MaxHp = 50 + stageBonusHp;
                    AttackPower = 10 + stageBonusAtk;
                    ExpReward = 55 + stage * 6;
                    GoldReward = 32 + stage * 4;
                    break;

                case EnemyType.ShieldGuard:
                    Name = "철갑 방패무사";
                    MaxHp = 80 + stageBonusHp;
                    AttackPower = 12 + stageBonusAtk;
                    ExpReward = 70 + stage * 7;
                    GoldReward = 38 + stage * 5;
                    ShieldHp = 3;
                    Width = 28;
                    Height = 34;
                    break;

                case EnemyType.MidBoss1:
                    Name = "괴력 건달 마달";
                    MaxHp = 240;
                    AttackPower = 18;
                    ExpReward = 220;
                    GoldReward = 160;
                    Width = 30;
                    Height = 36;
                    break;

                case EnemyType.MidBoss2:
                    Name = "배신자 철웅";
                    MaxHp = 350;
                    AttackPower = 24;
                    ExpReward = 380;
                    GoldReward = 260;
                    Width = 30;
                    Height = 36;
                    break;

                case EnemyType.Boss:
                    Name = "마도학사 흑호";
                    MaxHp = 520;
                    AttackPower = 30;
                    ExpReward = 600;
                    GoldReward = 500;
                    Width = 32;
                    Height = 36;
                    break;
            }
            CurrentHp = MaxHp;
        }

        public string GetRandomHitQuote()
        {
            var rand = new Random();
            return HitQuotes[rand.Next(HitQuotes.Length)];
        }

        public void UpdateAI(Player player, float dt, System.Collections.Generic.List<WorldObject>? worldObjects = null)
        {
            // 보스 체력 50% 이하 시 붉은 투기 발동 (광폭화 & 슈퍼아머)
            if (IsBossType && CurrentHp <= MaxHp * 0.5f && !IsRaging && !IsDead)
            {
                IsRaging = true;
                SoundSynth.PlaySpecial();
            }

            // 히트스탑(역경직) 중에는 적 AI 판단 및 쿨다운 일시 정지
            if (IsInHitStop) return;

            if (State == "hit" || State == "down" || IsDead || IsCarried || IsFlying) return;

            _attackCooldown -= dt;
            _aiDecisionTimer -= dt;
            _throwCooldown -= dt;

            float dx = player.X - X;
            float dy = player.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            FacingRight = dx > 0;

            // 2-1. 궁수(Archer)의 원거리 활 사격 AI
            if (Type == EnemyType.Archer)
            {
                if (worldObjects != null && _throwCooldown <= 0 && dist >= 45f && Math.Abs(dy) < 20f)
                {
                    _throwCooldown = 2.4f + (float)new Random().NextDouble() * 1.2f;
                    ShootArrow(player, worldObjects);
                    return;
                }

                // 플레이어와 적정 사거리(130px)를 유지하며 거리 벌리기 / Y축 정렬
                float targetDist = 135f;
                float desiredX = player.X + (player.FacingRight ? -targetDist : targetDist);
                float dirX = Math.Sign(desiredX - X);
                float dirY = Math.Sign(dy);
                Vx = dirX * 62f;
                Vy = dirY * 50f;
                State = (Math.Abs(Vx) > 5f || Math.Abs(Vy) > 5f) ? "walk" : "idle";
                FacingRight = player.X >= X;
                return;
            }

            // 1. 이미 물체를 들고 있는 상태에서의 투척 AI
            if (IsCarrying)
            {
                if (Math.Abs(dy) < 22f && dist >= 35f && dist <= 220f && _throwCooldown <= 0)
                {
                    ThrowCarried(player);
                    _throwCooldown = 2.0f;
                    return;
                }

                // 투척 각도를 잡기 위해 플레이어의 Y축 선상으로 이동
                float baseCarrySpeed = 60f;
                float dirX = Math.Sign(dx);
                float dirY = Math.Sign(dy);
                Vx = dirX * (baseCarrySpeed * 0.7f);
                Vy = dirY * (baseCarrySpeed * 0.8f);
                State = "walk";
                return;
            }

            // 2. 바닥에 있는 물체 줍기 시도 (쿨다운 완료 시)
            if (worldObjects != null && _throwCooldown <= 0)
            {
                var nearbyObj = worldObjects.Find(o => !o.IsDead && !o.IsCarried && !o.IsFlying &&
                                                       Math.Abs(X - o.X) < 28f && Math.Abs(Y - o.Y) < 16f);
                if (nearbyObj != null)
                {
                    CarriedObject = nearbyObj;
                    nearbyObj.IsCarried = true;
                    nearbyObj.ThrownByPlayer = false;
                    SoundSynth.PlayJump();
                    _throwCooldown = 0.5f; // 주운 후 잠깐 조준 후 투척
                    return;
                }
            }

            // 3. 중거리 투척 공격 (돌멩이 집어던지기)
            if (worldObjects != null && _throwCooldown <= 0 && dist >= 60f && dist <= 200f && Math.Abs(dy) < 18f)
            {
                var rand = new Random();
                if (rand.Next(10) < 5)
                {
                    _throwCooldown = 2.6f + (float)rand.NextDouble() * 1.5f;
                    ThrowSpawnedRock(player, worldObjects);
                    return;
                }
            }

            // 빈번하고 공격적인 의사 결정 (0.15초 ~ 0.35초 주기)
            if (_aiDecisionTimer <= 0)
            {
                _aiDecisionTimer = 0.15f + (float)new Random().NextDouble() * 0.2f;

                if (dist < 36f && Math.Abs(dy) < 16f)
                {
                    _aiMode = "attack";
                }
                else
                {
                    _aiMode = "chase";
                }
            }

            // 고속 공격적 기동 (광폭화 시 30% 추가 가속)
            float baseSpeed = Type switch
            {
                EnemyType.Boss => 85f,
                EnemyType.MidBoss2 => 80f,
                EnemyType.MidBoss1 => 70f,
                EnemyType.Ninja => 75f,
                EnemyType.Scholar => 68f,
                _ => 58f
            };
            if (IsRaging) baseSpeed *= 1.3f;

            if (_aiMode == "chase")
            {
                float targetX = player.X + (FacingRight ? -22f : 22f);
                float dirX = Math.Sign(targetX - X);
                float dirY = Math.Sign(dy);

                Vx = dirX * baseSpeed;
                Vy = dirY * (baseSpeed * 0.75f);
                State = "walk";
            }
            else if (_aiMode == "attack")
            {
                Vx *= 0.3f;
                Vy *= 0.3f;

                // 쿨다운이 끝나는 즉시 맹렬한 공격 감행!
                if (_attackCooldown <= 0)
                {
                    _attackCooldown = IsBossType
                        ? (0.25f + (float)new Random().NextDouble() * 0.2f)
                        : (0.38f + (float)new Random().NextDouble() * 0.25f);

                    State = (new Random().Next(10) < 6) ? "punch" : "kick";
                    StateTimer = 0f;
                    HasDealtDamage = false;

                    if (State == "punch") SoundSynth.PlayPunch();
                    else SoundSynth.PlayKick();
                }
                else
                {
                    State = "idle";
                }
            }
        }

        public override void Update(float dt)
        {
            if (IsCarried) return; // 플레이어 머리 위에서 위치 제어

            base.Update(dt);

            // 히트스탑(역경직) 중에는 적의 상태 전이 및 추가 이동 로직 일시 정지
            if (IsInHitStop) return;

            if (InvincibleTimer > 0)
            {
                InvincibleTimer -= dt;
            }

            // 보스 체력 50% 이하 시 붉은 투기 발동 (광폭화 & 슈퍼아머)
            if (IsBossType && CurrentHp <= MaxHp * 0.5f && !IsRaging && !IsDead)
            {
                IsRaging = true;
                SoundSynth.PlaySpecial();
            }

            // 들고 있는 물체 위치 동기화 (머리 위)
            if (CarriedObject != null)
            {
                CarriedObject.X = X;
                CarriedObject.Y = Y;
                CarriedObject.Z = Z + Height + 2f;
                CarriedObject.FacingRight = FacingRight;
            }

            if (IsFlying)
            {
                if (IsGrounded)
                {
                    IsFlying = false;
                    Vx = 0f;
                    Vy = 0f;
                    State = "down";
                    StateTimer = 0f;
                    int landDamage = 18;
                    TakeDamage(landDamage, 0f, out _); // 투척 착지 충격 데미지
                    OnThrownLanded?.Invoke(this, landDamage);
                }
            }

            // 벽 바운드 (Wall Bounce / Slam) 판정
            if ((IsFlying || State == "down") && Math.Abs(Vx) > 95f)
            {
                if (X <= 18f && Vx < 0)
                {
                    X = 18f;
                    Vx = -Vx * 0.60f;
                    Vz = 135f;
                    CurrentHp = Math.Max(0, CurrentHp - 12);
                    SoundSynth.PlayHit();
                    OnWallBounce?.Invoke(X, Y);
                }
                else if (X >= 305f && Vx > 0)
                {
                    X = 305f;
                    Vx = -Vx * 0.60f;
                    Vz = 135f;
                    CurrentHp = Math.Max(0, CurrentHp - 12);
                    SoundSynth.PlayHit();
                    OnWallBounce?.Invoke(X, Y);
                }
            }

            Y = Math.Clamp(Y, 145f, 225f);

            if (State == "punch" && StateTimer > 0.20f) State = "idle";
            if (State == "kick" && StateTimer > 0.26f) State = "idle";
            if (State == "hit")
            {
                Vx *= 0.82f;
                if (StateTimer > 0.30f) State = "idle";
            }
            if (State == "down")
            {
                Vx *= 0.8f;
                // 2초(2.0f) 후 기상 (사망 시 소멸)
                if (StateTimer > 2.0f)
                {
                    if (CurrentHp <= 0)
                    {
                        IsDead = true;
                    }
                    else
                    {
                        State = "idle";
                    }
                }
            }
        }

        public RectangleF? GetAttackHitbox()
        {
            if (State != "punch" && State != "kick") return null;

            float reach = (State == "kick") ? 20f : 16f;
            float attackX = FacingRight ? X + 2f : X - 2f - reach;
            float attackY = Y - Z - 22f;
            return new RectangleF(attackX, attackY, reach + 6f, 18f);
        }

        public void TakeDamage(int damage, float knockbackDir = 0f) => TakeDamage(damage, knockbackDir, out _);

        public void TakeDamage(int damage, float knockbackDir, out bool isKnockedDown)
        {
            isKnockedDown = false;
            if (InvincibleTimer > 0) return;

            DropCarried(); // 피격 시 들고 있던 물체 떨어뜨림

            // 철갑 방패무사의 전방 가드 판정
            if (Type == EnemyType.ShieldGuard && ShieldHp > 0)
            {
                bool isFrontal = (FacingRight && knockbackDir < 0) || (!FacingRight && knockbackDir > 0);
                if (isFrontal)
                {
                    if (damage >= 25)
                    {
                        // 강력한 비기/강타: 가드 파쇄 (Guard Break!)
                        ShieldHp = 0;
                        CurrentHp -= damage;
                        isKnockedDown = true;
                        State = "hit";
                        StateTimer = -1.2f; // 1.2초 스턴
                        SoundSynth.PlayShatter();
                        Vx = knockbackDir * 50f;
                        return;
                    }
                    else
                    {
                        // 일반 공격: 방패로 가드 (피해 0 & 막기 경직)
                        ShieldHp--;
                        SoundSynth.PlayGuard();
                        State = "hit";
                        StateTimer = 0.08f;
                        Vx = knockbackDir * 20f;
                        return;
                    }
                }
            }

            CurrentHp -= damage;
            SoundSynth.PlayHit();

            if (IsBossType && CurrentHp <= MaxHp * 0.5f && !IsRaging && !IsDead)
            {
                IsRaging = true;
            }

            // 쓰러진 상태(down)에서 피격 시: 데미지는 이미 입었으므로 경직(hit 전이, 넉백, 체공, 기상 타이머 리셋, 히트스탑) 없이 반환!
            if (State == "down")
            {
                isKnockedDown = false;
                HitStopTimer = 0f; // 역경직/히트스탑 면역
                // State 및 StateTimer는 리셋하지 않고 보존하여 2초 기상 타이머 정상 진행
                return;
            }

            bool wasFlying = IsFlying;

            if (CurrentHp <= 0 || wasFlying || damage >= (IsBossType && IsRaging ? 32 : 22))
            {
                isKnockedDown = true;
                State = "down";
                StateTimer = 0f;
                HitStopTimer = 0f;
                if (!wasFlying)
                {
                    Vz = 135f;
                    Vx = knockbackDir * 110f;
                }
            }
            else
            {
                // 슈퍼아머: 광폭화 보스는 일반 공격 피격 경직(hit)을 무시하고 공격 강행!
                if (IsBossType && IsRaging)
                {
                    isKnockedDown = false;
                    return;
                }

                // 피격 시 20% 확률로 쓰러짐(down), 80% 확률로 일반 피격 경직(hit)
                bool randomKnockdown = KnockdownRng.NextDouble() < KnockdownChance;
                if (randomKnockdown)
                {
                    isKnockedDown = true;
                    State = "down";
                    StateTimer = 0f;
                    HitStopTimer = 0f;
                    Vz = 135f;
                    Vx = knockbackDir * 110f;
                }
                else
                {
                    isKnockedDown = false;
                    State = "hit";
                    StateTimer = 0f;
                    Vx = knockbackDir * 70f;
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

            string prefix = Type switch
            {
                EnemyType.Thug => "enemy1",
                EnemyType.Scholar => "enemy2",
                EnemyType.Ninja => "enemy3",
                EnemyType.Archer => "archer",
                EnemyType.ShieldGuard => "shieldguard",
                EnemyType.MidBoss1 => "midboss1",
                EnemyType.MidBoss2 => "midboss2",
                EnemyType.Boss => "boss",
                _ => "enemy1"
            };

            if (IsCarried || State == "down" || IsFlying)
            {
                var downSprite = RetroSprites.GetSprite($"{prefix}_down");
                if (downSprite != null)
                {
                    float projX = Perspective.ProjectX(X, cameraX, Y);
                    float p = Perspective.GetScale(Y);
                    float w = downSprite.Width * p;
                    float h = downSprite.Height * p;
                    float dx = projX - w / 2f;
                    float dy = (Y - Z) - h;
                    buffer.DrawBitmap(downSprite, dx, dy, flipX: !FacingRight, scaleFactor: p);
                    return;
                }
            }

            string spriteKey = State switch
            {
                "punch" => $"{prefix}_punch_{AnimFrame}",
                "kick" => $"{prefix}_kick_{AnimFrame}",
                "walk" or "run" => IsCarrying ? $"{prefix}_carry_walk_{AnimFrame}" : $"{prefix}_walk_{AnimFrame}",
                "hit" => $"{prefix}_hit",
                "down" => $"{prefix}_down",
                _ => (IsCarrying ? $"{prefix}_carry_idle" : $"{prefix}_idle_{AnimFrame}")
            };

            var sprite = RetroSprites.GetSprite(spriteKey);
            float pNorm = Perspective.GetScale(Y);
            float drawW = (sprite != null ? sprite.Width : Width) * pNorm;
            float drawH = (sprite != null ? sprite.Height : Height) * pNorm;
            float projBaseX = Perspective.ProjectX(X, cameraX, Y);
            float sx = projBaseX - drawW / 2f;
            float sy = (Y - Z) - drawH;

            if (sprite != null)
            {
                // 960x720 고해상도 광폭화 붉은 투기 아우라 렌더링
                if (IsBossType && IsRaging && !IsDead)
                {
                    float scx = sx * PixelBuffer.Scale;
                    float scy = sy * PixelBuffer.Scale;
                    float sdrawW = drawW * PixelBuffer.Scale;
                    float sdrawH = drawH * PixelBuffer.Scale;

                    bool blink = ((int)(DateTime.UtcNow.Millisecond / 160) % 2 == 0);
                    int auraAlpha = blink ? 90 : 50;
                    buffer.FillRoundedRect(scx - 8, scy - 8, sdrawW + 16, sdrawH + 16, 12f, Color.FromArgb(auraAlpha, 240, 30, 30));
                    buffer.DrawRoundedRect(scx - 8, scy - 8, sdrawW + 16, sdrawH + 16, 12f, Color.FromArgb(210, 255, 60, 50), 2.5f);
                }

                buffer.DrawBitmap(sprite, sx, sy, flipX: !FacingRight, scaleFactor: pNorm);
            }

            // 960x720 고해상도 적 머리 위 체력바 표시 (원근 머리 위치 정밀 추적)
            if (!IsDead && State != "down" && !IsCarried)
            {
                float scx = sx * PixelBuffer.Scale;
                float scy = sy * PixelBuffer.Scale;
                float sdrawW = drawW * PixelBuffer.Scale;

                int barW = (int)((IsBossType ? 90 : 66) * pNorm);
                int barH = 7;
                int barX = (int)(scx + (sdrawW - barW) / 2);
                int barY = (int)(scy - 18);

                buffer.FillRoundedRect(barX - 1, barY - 1, barW + 2, barH + 2, 3f, Color.FromArgb(200, 20, 20, 20));
                buffer.FillRoundedRect(barX, barY, barW, barH, 2f, Color.FromArgb(220, 60, 20, 20));

                int fillW = Math.Max(0, (int)((float)CurrentHp / MaxHp * barW));
                if (fillW > 0)
                {
                    Color hpCol = IsBossType ? (IsRaging ? Color.FromArgb(255, 60, 60) : Color.FromArgb(255, 130, 40)) : Color.FromArgb(235, 50, 50);
                    buffer.FillRoundedRect(barX, barY, fillW, barH, 2f, hpCol);
                }
            }
        }
    }
}
