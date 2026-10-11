using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        // 방어 및 순간 쳐내기 (Guard & Just Parry)
        public float ZHoldTimer { get; set; } = 0f;
        public float ParryWindowTimer { get; set; } = 0f;
        public float ParryCooldownTimer { get; set; } = 0f;
        public float PostAttackDelayTimer { get; set; } = 0f;
        public bool JustParried { get; set; } = false;
        public float JustParryVisualTimer { get; set; } = 0f;
        public bool IsGuarding => State == "guard";

        // 손에 쥐는 근접 무기 및 공중 기동
        public WorldObject? EquippedWeapon { get; set; }
        public bool HasDoubleJumped { get; set; } = false;

        // 격투 게임식 공격 동작 및 후딜레이 판정
        public bool IsAttackingOrRecovering =>
            State == "punch" || State == "kick" || State == "jumpkick" ||
            State == "dashpunch" || State == "dashkick" || State == "spin" ||
            State == "dragon" || State == "wave" || State == "swing" ||
            State == "knee" || State == "giantswing" ||
            State == "spear_stab" || State == "cudgel_swing" || State == "fan_wave" ||
            State == "ex_spin" || State == "ex_dragon" || State == "ex_wave" ||
            PostAttackDelayTimer > 0f;

        // 들기 시스템 (물체 및 쓰러진 적)
        public WorldObject? CarriedObject { get; set; }
        public Enemy? CarriedEnemy { get; set; }
        public float CarriedEnemyTimer { get; set; } = 0f;
        public bool IsCarrying => CarriedObject != null || CarriedEnemy != null;

        // 공격 판정 플래그
        public bool HasDealtDamage { get; set; } = false;
        public string CurrentAttackType { get; set; } = "";

        // 피격 시 넉다운(쓰러짐) 확률 (기본 20%)
        public float KnockdownChance { get; set; } = 0.20f;
        public Random KnockdownRng { get; set; } = new Random();

        // 잡은 적 투척 시 데미지 콜백
        public Action<Enemy, int>? OnCarriedEnemyThrown { get; set; }

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

        public void SyncCarriedPositions()
        {
            if (CarriedObject != null)
            {
                CarriedObject.X = X;
                CarriedObject.Y = Y;
                CarriedObject.Z = Z + Height + 2f;
                CarriedObject.FacingRight = FacingRight;
                CarriedObject.IsCarried = true;
                CarriedObject.IsFlying = false;
                CarriedObject.IsRolling = false;
                CarriedObject.Vx = 0f;
                CarriedObject.Vy = 0f;
                CarriedObject.Vz = 0f;
            }
            if (EquippedWeapon != null)
            {
                EquippedWeapon.X = X + (FacingRight ? 12f : -12f);
                EquippedWeapon.Y = Y;
                EquippedWeapon.Z = Z + 14f;
                EquippedWeapon.FacingRight = FacingRight;
                EquippedWeapon.IsCarried = true;
                EquippedWeapon.IsFlying = false;
                EquippedWeapon.Vx = 0f;
                EquippedWeapon.Vy = 0f;
                EquippedWeapon.Vz = 0f;
            }
            if (CarriedEnemy != null)
            {
                CarriedEnemy.X = X;
                CarriedEnemy.Y = Y;
                CarriedEnemy.Z = Z + Height + 2f;
                CarriedEnemy.FacingRight = FacingRight;
                CarriedEnemy.IsCarried = true;
                CarriedEnemy.IsFlying = false;
                CarriedEnemy.Vx = 0f;
                CarriedEnemy.Vy = 0f;
                CarriedEnemy.Vz = 0f;
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
                float throwSpeed = obj.Type switch
                {
                    ObjectType.Cart => 230f,
                    ObjectType.Table => 240f,
                    ObjectType.Bench => 240f,
                    ObjectType.RiceSack => 250f,
                    _ => 270f
                };
                obj.Vx = (FacingRight ? 1f : -1f) * throwSpeed;
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
                enemy.State = "down";
                enemy.StateTimer = 0f;
                enemy.X = X + (FacingRight ? 16f : -16f);
                enemy.Y = Y;
                enemy.Z = Z + 20f;
                enemy.Vx = (FacingRight ? 1f : -1f) * 260f;
                enemy.Vz = 100f;

                // 쓰러진 캐릭터 투척 시 던져진 캐릭터에게 즉시 투척 피해 적용
                int throwDamage = Status.PunchDamage + 10;
                enemy.TakeDamage(throwDamage, FacingRight ? 1f : -1f, out _);
                OnCarriedEnemyThrown?.Invoke(enemy, throwDamage);

                SoundSynth.PlaySpecial();
            }
        }

        public void EquipWeapon(WorldObject weapon)
        {
            EquippedWeapon = weapon;
        }

        public void ThrowEquippedWeapon()
        {
            if (EquippedWeapon == null) return;
            var wp = EquippedWeapon;
            EquippedWeapon = null;
            wp.IsCarried = false;
            wp.IsFlying = true;
            wp.ThrownByPlayer = true;
            wp.X = X + (FacingRight ? 16f : -16f);
            wp.Y = Y;
            wp.Z = Z + 16f;
            float speed = wp.Type == ObjectType.Spear ? 320f : (wp.Type == ObjectType.Fan ? 280f : 250f);
            wp.Vx = (FacingRight ? 1f : -1f) * speed;
            wp.Vz = 25f;
            SoundSynth.PlaySpecial();
        }

        public void HandleInput(Input input, System.Collections.Generic.List<WorldObject>? worldObjects = null, System.Collections.Generic.List<Enemy>? enemies = null)
        {
            // 히트스탑(역경직) 중에는 입력 및 모션 전이 정지
            if (IsInHitStop) return;

            // 비상 낙법 (Ukemi): 공중 피격 또는 쓰러지는 도중(Z > 0 및 지면 28px 이내) C키 입력 시 즉시 착지 기상! (사망 시 불가능)
            if (Status.CurrentHp > 0 && (State == "hit" || State == "down") && Z > 0 && Z < 28f)
            {
                if (input.IsJustPressed(Keys.C))
                {
                    State = "idle";
                    StateTimer = 0f;
                    Z = 0f;
                    Vz = 0f;
                    Vx = (FacingRight ? 1f : -1f) * 40f;
                    InvincibleTimer = 0.5f;
                    SoundSynth.PlayUkemi();
                    return;
                }
            }

            if (State == "hit" || State == "down" || Status.CurrentHp <= 0) return;

            // 0. 격투 게임식 공격 후딜레이(Recovery Delay): 딜레이 중에는 이동, 공격, 가드, 패링 불가
            if (PostAttackDelayTimer > 0f)
            {
                Vx *= 0.5f;
                Vy *= 0.5f;
                ParryWindowTimer = 0f;
                return;
            }

            // 공격 동작 진행 중에는 이동 및 추가 액션/패링 불가
            if (State == "punch" || State == "kick" || State == "dashpunch" || State == "dashkick" ||
                State == "swing" || State == "spin" || State == "dragon" || State == "wave" ||
                State == "spear_stab" || State == "cudgel_swing" || State == "fan_wave" ||
                State == "ex_spin" || State == "ex_dragon" || State == "ex_wave")
            {
                Vx *= 0.5f;
                Vy *= 0.5f;
                ParryWindowTimer = 0f;
                return;
            }

            // Z키 누름 시간 및 순간 쳐내기(Just Parry) 입력 타이머 계측
            if (input.IsJustPressed(Keys.Z))
            {
                ZHoldTimer = 0f;
                // 중립(대기/이동/가드) 상태에서 적의 공격을 예측하고 타이밍 맞춰 눌렀을 때만 0.14초 저스트 패링 발동!
                if (!IsAttackingOrRecovering && ParryCooldownTimer <= 0f)
                {
                    ParryWindowTimer = 0.14f;
                    ParryCooldownTimer = 0.28f; // 헛패링 연타 방지 쿨다운
                }
                else
                {
                    ParryWindowTimer = 0f;
                }
            }
            else if (input.IsHeld(Keys.Z))
            {
                ZHoldTimer += 0.016f;
            }
            else
            {
                ZHoldTimer = 0f;
            }

            // 1. 공중(점프) 상태일 때의 조작
            if (!IsGrounded)
            {
                // 공중 2단 점프 (풍운의 짚신 유물)
                if (input.IsJustPressed(Keys.C) && Status.HasRelic("shoes") && !HasDoubleJumped)
                {
                    HasDoubleJumped = true;
                    Vz = Status.JumpPower * 0.9f;
                    SoundSynth.PlayJump();
                }

                // 공중에서 X키 누르면 들고 있는 것 또는 무기 투척
                if (input.IsJustPressed(Keys.X) && (IsCarrying || EquippedWeapon != null))
                {
                    if (IsCarrying) ThrowCarried();
                    else ThrowEquippedWeapon();
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

            // 방어(가드) 자세 유지 및 해제 (Z키를 누르고 있는 동안 방어 자세 유지)
            if (State == "guard")
            {
                if (input.IsHeld(Keys.Z))
                {
                    Vx = 0f;
                    Vy = 0f;
                    return;
                }
                else
                {
                    State = "idle";
                    StateTimer = 0f;
                }
            }

            // Z키 홀드로 방어(가드) 진입 (0.12초 이상 누르고 있을 때 지상 대기/보행 상태에서 전이)
            if (input.IsHeld(Keys.Z) && ZHoldTimer >= 0.12f && IsGrounded && !IsCarrying)
            {
                if (State == "idle" || State == "walk" || State == "run" || (State == "punch" && StateTimer < 0.14f))
                {
                    State = "guard";
                    StateTimer = 0f;
                    Vx = 0f;
                    Vy = 0f;
                    return;
                }
            }

            // 2. 지상 공격 동작 중 이동 불가
            if (State == "punch" || State == "kick" || State == "spin" || State == "swing" || State == "dragon" || State == "wave")
            {
                Vx *= 0.5f;
                Vy *= 0.5f;
                return;
            }

            // 2. 비기(Special) 및 EX 강화 비기 (Space, Space + Z, Space + X)
            bool isExWithZ = (input.IsJustPressed(Keys.Space) && (input.IsHeld(Keys.Z) || input.IsJustPressed(Keys.Z))) ||
                             (input.IsHeld(Keys.Space) && input.IsJustPressed(Keys.Z));
            bool isExWithX = (input.IsJustPressed(Keys.Space) && (input.IsHeld(Keys.X) || input.IsJustPressed(Keys.X))) ||
                             (input.IsHeld(Keys.Space) && input.IsJustPressed(Keys.X));
            bool isSpacePressed = input.IsJustPressed(Keys.Space);

            if (isExWithZ || isExWithX || isSpacePressed)
            {
                // 잡은 적 자이언트 스윙 (Space)
                int giantSwingCost = Status.GetSpecialMpCost(10);
                if (CarriedEnemy != null && isSpacePressed && !isExWithZ && !isExWithX && Status.CurrentMp >= giantSwingCost)
                {
                    Status.CurrentMp -= giantSwingCost;
                    State = "giantswing";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                    CurrentAttackType = "giantswing";
                    SoundSynth.PlaySpecial();
                    return;
                }

                // EX 강화 비기: MP 20 소모, 1초 무적, 강력한 범위 판정
                if (isExWithZ || isExWithX || (isSpacePressed && (input.IsHeld(Keys.Z) || input.IsHeld(Keys.X))))
                {
                    int exCost = Status.GetSpecialMpCost(20);
                    if (Status.CurrentMp >= exCost)
                    {
                        DropCarried();
                        Status.CurrentMp -= exCost;
                        InvincibleTimer = 1.0f;
                        HasDealtDamage = false;

                        if (input.IsHeld(Keys.Down))
                        {
                            State = "ex_wave";
                            StateTimer = 0f;
                            CurrentAttackType = "ex_wave";
                            Vx = 0f;
                            Vy = 0f;
                            SoundSynth.PlayWave();
                            SoundSynth.PlaySpecial();
                        }
                        else if (isExWithX || input.IsHeld(Keys.Up))
                        {
                            State = "ex_dragon";
                            StateTimer = 0f;
                            CurrentAttackType = "ex_dragon";
                            Vz = 240f;
                            Vx = (FacingRight ? 1f : -1f) * 85f;
                            SoundSynth.PlayDragon();
                            SoundSynth.PlaySpecial();
                        }
                        else
                        {
                            State = "ex_spin";
                            StateTimer = 0f;
                            CurrentAttackType = "ex_spin";
                            SoundSynth.PlaySpecial();
                        }
                    }
                    return;
                }

                // 일반 3대 비기
                if (isSpacePressed)
                {
                    if (input.IsHeld(Keys.Down))
                    {
                        int waveCost = Status.GetSpecialMpCost(25);
                        if (Status.CurrentMp >= waveCost)
                        {
                            DropCarried();
                            Status.CurrentMp -= waveCost;
                            State = "wave";
                            StateTimer = 0f;
                            HasDealtDamage = false;
                            CurrentAttackType = "wave";
                            Vx = 0f;
                            Vy = 0f;
                            SoundSynth.PlayWave();
                            SoundSynth.PlaySpecial();
                            return;
                        }
                    }
                    else if (input.IsHeld(Keys.Up))
                    {
                        int dragonCost = Status.GetSpecialMpCost(20);
                        if (Status.CurrentMp >= dragonCost)
                        {
                            DropCarried();
                            Status.CurrentMp -= dragonCost;
                            State = "dragon";
                            StateTimer = 0f;
                            HasDealtDamage = false;
                            CurrentAttackType = "dragon";
                            Vz = 185f;
                            Vx = (FacingRight ? 1f : -1f) * 65f;
                            SoundSynth.PlayDragon();
                            SoundSynth.PlaySpecial();
                            return;
                        }
                    }
                    else
                    {
                        int spinCost = Status.GetSpecialMpCost(15);
                        if (Status.CurrentMp >= spinCost)
                        {
                            DropCarried();
                            Status.CurrentMp -= spinCost;
                            State = "spin";
                            StateTimer = 0f;
                            HasDealtDamage = false;
                            CurrentAttackType = "spin";
                            SoundSynth.PlaySpecial();
                            return;
                        }
                    }
                }
            }

            // 3. 지상 Z키: 무기 들고 있으면 '무기 공격', 들고 있으면 '들고 때린다', 들 게 있으면 '주워 든다', 없으면 '주먹'
            if (input.IsJustPressed(Keys.Z))
            {
                if (EquippedWeapon != null)
                {
                    if (EquippedWeapon.Type == ObjectType.Spear)
                    {
                        State = "spear_stab";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "spear_stab";
                        SoundSynth.PlayWhoosh();
                        return;
                    }
                    else if (EquippedWeapon.Type == ObjectType.Cudgel)
                    {
                        State = "cudgel_swing";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "cudgel_swing";
                        SoundSynth.PlayKick();
                        return;
                    }
                    else if (EquippedWeapon.Type == ObjectType.Fan)
                    {
                        State = "fan_wave";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "fan_wave";
                        SoundSynth.PlayWhoosh();
                        return;
                    }
                }

                if (IsCarrying)
                {
                    if (CarriedEnemy != null)
                    {
                        // 잡은 적 무릎 치기 (Knee Strike)
                        State = "knee";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "knee";
                        SoundSynth.PlayHit();
                        return;
                    }
                    else
                    {
                        State = "swing";
                        StateTimer = 0f;
                        HasDealtDamage = false;
                        CurrentAttackType = "swing";
                        SoundSynth.PlayKick();
                        return;
                    }
                }
                else
                {
                    // 1) 바닥에 있는 무기나 물체 줍기
                    if (worldObjects != null)
                    {
                        var nearbyObj = worldObjects.Find(o => !o.IsDead && !o.IsCarried && !o.IsFlying && !o.IsRolling &&
                                                               Math.Abs(X - o.X) < Math.Max(26f, o.Width / 2f + 14f) && Math.Abs(Y - o.Y) < 18f);
                        if (nearbyObj != null)
                        {
                            if (nearbyObj.Type == ObjectType.Spear || nearbyObj.Type == ObjectType.Cudgel || nearbyObj.Type == ObjectType.Fan)
                            {
                                EquippedWeapon = nearbyObj;
                                nearbyObj.IsCarried = true;
                                SoundSynth.PlayJump();
                                return;
                            }
                            else if (nearbyObj.Type != ObjectType.Hwacha && nearbyObj.Type != ObjectType.Arrow)
                            {
                                CarriedObject = nearbyObj;
                                nearbyObj.IsCarried = true;
                                SoundSynth.PlayJump();
                                return;
                            }
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
            // 4. 지상 X키: 들고 있으면 '던진다', 무기 있으면 '무기 투척', 대시 중이면 '대시 킥(돌진 날아차기)', 없으면 '발차기'
            else if (input.IsJustPressed(Keys.X))
            {
                if (IsCarrying)
                {
                    ThrowCarried();
                    return;
                }
                else if (EquippedWeapon != null)
                {
                    ThrowEquippedWeapon();
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

            // 히트스탑(역경직) 중에는 플레이어의 물리 이동 및 상태 갱신 일시 정지
            if (IsInHitStop) return;

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
                HasDoubleJumped = false;
                if (State == "jump" || State == "jumpkick")
                {
                    State = "idle";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                }
                else if (State == "dragon" && StateTimer > 0.40f)
                {
                    State = "idle";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.28f;
                }
                else if (State == "ex_dragon" && StateTimer > 0.48f)
                {
                    State = "idle";
                    StateTimer = 0f;
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.28f;
                }
            }
            else
            {
                // 공중 체공 중일 때는 날라차기, 비기(dragon, spin, wave) 또는 피격이 아닌 경우 점프 상태 보장
                if (State != "jumpkick" && State != "hit" && State != "down" &&
                    State != "dragon" && State != "ex_dragon" &&
                    State != "spin" && State != "ex_spin" &&
                    State != "wave" && State != "ex_wave")
                {
                    State = "jump";
                }
            }

            // 손에 쥔 무기 위치 동기화
            if (EquippedWeapon != null)
            {
                EquippedWeapon.X = X + (FacingRight ? 12f : -12f);
                EquippedWeapon.Y = Y;
                EquippedWeapon.Z = Z + 14f;
                EquippedWeapon.FacingRight = FacingRight;
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
            if (ParryWindowTimer > 0)
            {
                ParryWindowTimer -= dt;
            }
            if (ParryCooldownTimer > 0)
            {
                ParryCooldownTimer -= dt;
            }
            if (PostAttackDelayTimer > 0)
            {
                PostAttackDelayTimer -= dt;
            }
            if (JustParryVisualTimer > 0)
            {
                JustParryVisualTimer -= dt;
            }

            if (State == "dashpunch")
            {
                Vx *= 0.94f;
                if (StateTimer > 0.28f)
                {
                    State = "idle";
                    ParryCooldownTimer = 0.24f;
                }
            }
            else if (State == "dashkick")
            {
                Vx *= 0.94f;
                if (StateTimer > 0.32f)
                {
                    State = "idle";
                    ParryCooldownTimer = 0.24f;
                }
            }
            else if (State == "punch")
            {
                if (StateTimer > 0.22f)
                {
                    State = "idle";
                    ParryCooldownTimer = 0.24f;
                }
            }
            else if (State == "kick")
            {
                if (StateTimer > 0.28f)
                {
                    State = "idle";
                    ParryCooldownTimer = 0.24f;
                }
            }
            else if (State == "swing")
            {
                Vx *= 0.5f;
                if (StateTimer > 0.15f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.24f;
                }
            }
            else if (State == "spin")
            {
                Vx = (FacingRight ? 1f : -1f) * 60f;
                if (StateTimer > 0.45f)
                {
                    State = "idle";
                    ParryCooldownTimer = 0.28f;
                }
            }
            else if (State == "dragon")
            {
                if (StateTimer > 0.42f && IsGrounded)
                {
                    State = "idle";
                    ParryCooldownTimer = 0.28f;
                }
            }
            else if (State == "wave")
            {
                Vx *= 0.7f;
                if (StateTimer > 0.50f)
                {
                    State = "idle";
                    ParryCooldownTimer = 0.28f;
                }
            }
            else if (State == "knee")
            {
                Vx = 0f;
                if (StateTimer > 0.24f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.20f;
                }
            }
            else if (State == "giantswing")
            {
                Vx = (float)(Math.Sin(StateTimer * 22f) * 35f);
                if (CarriedEnemy != null)
                {
                    CarriedEnemy.X = X + (float)Math.Cos(StateTimer * 22f) * 26f;
                    CarriedEnemy.Y = Y + (float)Math.Sin(StateTimer * 22f) * 12f;
                    CarriedEnemy.Z = Z + 12f;
                }
                if (StateTimer > 0.44f)
                {
                    if (CarriedEnemy != null)
                    {
                        var e = CarriedEnemy;
                        CarriedEnemy = null;
                        CarriedEnemyTimer = 0f;
                        e.IsCarried = false;
                        e.IsFlying = true;
                        e.State = "down";
                        e.StateTimer = 0f;
                        e.Vx = (FacingRight ? 1f : -1f) * 280f;
                        e.Vz = 70f;

                        // 쓰러진 캐릭터 자이언트 스윙 투척 시 던져진 캐릭터에게 스윙 피해 적용
                        int swingDamage = 25;
                        e.TakeDamage(swingDamage, FacingRight ? 1f : -1f, out _);
                        OnCarriedEnemyThrown?.Invoke(e, swingDamage);

                        SoundSynth.PlaySpecial();
                    }
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.28f;
                }
            }
            else if (State == "spear_stab")
            {
                Vx = (FacingRight ? 1f : -1f) * 45f;
                if (StateTimer > 0.28f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.22f;
                }
            }
            else if (State == "cudgel_swing")
            {
                Vx *= 0.6f;
                if (StateTimer > 0.32f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.24f;
                }
            }
            else if (State == "fan_wave")
            {
                Vx = (FacingRight ? 1f : -1f) * 35f;
                if (StateTimer > 0.24f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.20f;
                }
            }
            else if (State == "ex_spin")
            {
                Vx = (FacingRight ? 1f : -1f) * 90f;
                if (StateTimer > 0.55f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.28f;
                }
            }
            else if (State == "ex_dragon")
            {
                if (StateTimer > 0.50f && IsGrounded)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.28f;
                }
            }
            else if (State == "ex_wave")
            {
                Vx *= 0.5f;
                if (StateTimer > 0.60f)
                {
                    State = "idle";
                    HasDealtDamage = false;
                    ParryCooldownTimer = 0.28f;
                }
            }
            else if (State == "hit")
            {
                Vx *= 0.85f;
                if (StateTimer > 0.35f) State = "idle";
            }
            else if (State == "down")
            {
                Vx *= 0.8f;
                // 체력이 남아있을 때만 2초(2.0f) 후 기상, HP가 0 이하로 사망한 상태면 누워있음
                if (StateTimer > 2.0f && Status.CurrentHp > 0)
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
            if (State != "punch" && State != "kick" && State != "jumpkick" && State != "spin" && State != "swing" && State != "dashpunch" && State != "dashkick" && State != "dragon" && State != "wave" && State != "knee" && State != "giantswing" &&
                State != "spear_stab" && State != "cudgel_swing" && State != "fan_wave" &&
                State != "ex_spin" && State != "ex_dragon" && State != "ex_wave")
                return null;

            // 격투 게임식 판정: 공격 후딜레이(Recovery) 구간에는 타격 판정 소멸
            if (State == "punch" && StateTimer > 0.16f) return null;
            if (State == "kick" && StateTimer > 0.20f) return null;
            if (State == "dashpunch" && StateTimer > 0.20f) return null;
            if (State == "dashkick" && StateTimer > 0.22f) return null;
            if (State == "swing" && StateTimer > 0.20f) return null;
            if (State == "knee" && StateTimer > 0.18f) return null;
            if (State == "spin" && StateTimer > 0.32f) return null;
            if (State == "dragon" && StateTimer > 0.30f) return null;
            if (State == "wave" && StateTimer > 0.36f) return null;
            if (State == "giantswing" && StateTimer > 0.40f) return null;
            if (State == "spear_stab" && StateTimer > 0.22f) return null;
            if (State == "cudgel_swing" && StateTimer > 0.24f) return null;
            if (State == "fan_wave" && StateTimer > 0.20f) return null;
            if (State == "ex_spin" && StateTimer > 0.42f) return null;
            if (State == "ex_dragon" && StateTimer > 0.38f) return null;
            if (State == "ex_wave" && StateTimer > 0.45f) return null;

            if (State == "spear_stab")
            {
                float reach = 85f;
                float attackX = FacingRight ? X + 4f : X - 4f - reach;
                return new RectangleF(attackX, Y - Z - 24f, reach, 20f);
            }
            if (State == "cudgel_swing")
            {
                float reach = 34f;
                float attackX = FacingRight ? X + 4f : X - 4f - reach;
                return new RectangleF(attackX, Y - Z - 28f, reach, 28f);
            }
            if (State == "fan_wave")
            {
                float reach = 45f;
                float attackX = FacingRight ? X + 4f : X - 4f - reach;
                return new RectangleF(attackX, Y - Z - 26f, reach, 26f);
            }
            if (State == "ex_spin")
            {
                return new RectangleF(X - 55f, Y - Z - 28f, 110f, 32f);
            }
            if (State == "ex_dragon")
            {
                float reach = 38f;
                float attackX = FacingRight ? X + 2f : X - 2f - reach;
                return new RectangleF(attackX, Y - Z - 65f, reach + 10f, 72f);
            }
            if (State == "ex_wave")
            {
                float reach = 280f;
                float attackX = FacingRight ? X + 6f : X - 6f - reach;
                return new RectangleF(attackX, Y - Z - 28f, reach, 34f);
            }

            if (State == "giantswing")
            {
                // 자이언트 스윙: 360도 전방위 70px 회전 타격 판정
                return new RectangleF(X - 35f, Y - Z - 24f, 70f, 26f);
            }
            if (State == "knee")
            {
                float reachKnee = 18f;
                float attackX = FacingRight ? X + 2f : X - 2f - reachKnee;
                return new RectangleF(attackX, Y - Z - 24f, reachKnee + 6f, 22f);
            }

            if (State == "dragon")
            {
                // 제2 비기 용비어천격: 수직으로 솟구치는 상단 48px 승룡 판정
                float reach = 26f;
                float attackX = FacingRight ? X + 2f : X - 2f - reach;
                float attackY = Y - Z - 42f;
                return new RectangleF(attackX, attackY, reach + 8f, 48f);
            }
            if (State == "wave")
            {
                // 제3 비기 월인천강포: 전방 150px 일직선 관통 광선 파동포 판정
                float reach = 150f;
                float attackX = FacingRight ? X + 6f : X - 6f - reach;
                float attackY = Y - Z - 24f;
                return new RectangleF(attackX, attackY, reach, 28f);
            }

            float reachStd = (State == "kick" || State == "jumpkick" || State == "dashkick") ? 22f : 16f;
            if (State == "spin" || State == "swing" || State == "dashpunch") reachStd = 28f;

            float attackXStd = FacingRight ? X + 4f : X - 4f - reachStd;
            if (State == "spin") attackXStd = X - reachStd / 2f;

            if (State == "jumpkick")
            {
                // 공중 날라차기: 체공 중인 캐릭터 위치(Y - Z)에 공격 판정(히트박스) 집중 배치
                // 바닥/그림자 위치(Y)로 불필요하게 늘어나지 않고 캐릭터의 타격점(상체~발끝)에 정확히 밀착
                float attackY = Y - Z - 24f;
                float attackHeight = 24f;
                return new RectangleF(attackXStd, attackY, reachStd + 8f, attackHeight);
            }
            else
            {
                float attackY = Y - Z - 24f;
                return new RectangleF(attackXStd, attackY, reachStd + 6f, 22f);
            }
        }

        public void TakeDamage(int damage, float knockbackDir)
        {
            TakeDamage(damage, knockbackDir, out _, out _, out _);
        }

        public void TakeDamage(int damage, float knockbackDir, out bool parried, out bool guarded)
        {
            TakeDamage(damage, knockbackDir, out parried, out guarded, out _);
        }

        public void TakeDamage(int damage, float knockbackDir, out bool parried, out bool guarded, out bool counterHit)
        {
            parried = false;
            guarded = false;
            counterHit = false;

            if (InvincibleTimer > 0) return;

            // 쓰러진 상태(down)에서 피격 시: 데미지는 입되 경직(hit 전이, 넉백, 기상 타이머 리셋, 히트스탑) 발생 금지!
            if (State == "down")
            {
                int actualDmg = damage;
                if (Status.HasRelic("cheolgap")) actualDmg = Math.Max(1, actualDmg - 4);

                Status.CurrentHp = Math.Max(0, Status.CurrentHp - actualDmg);
                SoundSynth.PlayHit();
                HitStopTimer = 0f; // 역경직/히트스탑 면역
                // State 및 StateTimer는 리셋하지 않고 보존하여 2초 기상 타이머 정상 진행
                return;
            }

            // 도깨비 감투 유물: 대시(달리기) 중 완전 무적
            if (State == "run" && Status.HasRelic("dokkaebi")) return;

            // 격투 게임식 카운터 판정: 공격 동작 중이거나 공격 후딜레이(PostAttackDelay) 중에는 패링/가드 불가!
            bool isAttackingOrInRecovery = IsAttackingOrRecovering;

            // 1. 순간 쳐내기 (Just Parry): 중립/가드 상태에서 적 피격 직전(0.14초 이내) Z키를 타이밍 맞게 입력했을 때만 발동!
            if (!isAttackingOrInRecovery && ParryWindowTimer > 0)
            {
                parried = true;
                JustParried = true;
                JustParryVisualTimer = 0.35f;
                ParryWindowTimer = 0f;
                InvincibleTimer = 0.4f;
                Status.CurrentMp = Math.Min(Status.MaxMp, Status.CurrentMp + 10); // 기력 MP +10 회복!
                SoundSynth.PlayParry();

                // 단전 벼락구슬 유물: 저스트 패링 성공 시 벼락 발동
                if (Status.HasRelic("spark"))
                {
                    SoundSynth.PlaySpecial();
                }
                return;
            }

            // 2. 가드 (Guard): Z키를 길게 홀드하여 방어 자세를 유지하고 있을 때 (공격/후딜 중에는 가드 불가)
            if (!isAttackingOrInRecovery && State == "guard")
            {
                guarded = true;
                damage = Math.Max(1, (int)(damage * 0.30f)); // 피격 데미지 70% 대폭 감소!
                if (Status.HasRelic("cheolgap")) damage = Math.Max(1, damage - 4);
                Status.CurrentHp -= damage;
                SoundSynth.PlayGuard();
                Vx = knockbackDir * 35f; // 약간의 밀림
                InvincibleTimer = 0.28f;
                if (Status.CurrentHp <= 0)
                {
                    Status.CurrentHp = 0;
                    State = "down";
                    StateTimer = 0f;
                    HitStopTimer = 0f;
                    Vz = 120f;
                    Vx = knockbackDir * 100f;
                }
                return;
            }

            // 3. 일반 피격 및 카운터 피격
            DropCarried(); // 피격 시 들고 있던 물체/적을 놓침

            if (isAttackingOrInRecovery)
            {
                counterHit = true;
                damage = (int)(damage * 1.25f); // 공격 중 피격 시 25% 추가 카운터 데미지!
            }

            // 철갑 흉경 유물: 상시 받는 피해 4 감소
            if (Status.HasRelic("cheolgap"))
            {
                damage = Math.Max(1, damage - 4);
            }

            Status.CurrentHp -= damage;
            SoundSynth.PlayHit();

            // 공격 및 딜레이 강제 캔슬
            PostAttackDelayTimer = 0f;
            ParryWindowTimer = 0f;

            if (Status.CurrentHp <= 0)
            {
                Status.CurrentHp = 0;
                State = "down";
                StateTimer = 0f;
                HitStopTimer = 0f;
                Vz = 120f;
                Vx = knockbackDir * (counterHit ? 120f : 100f);
            }
            else
            {
                // 금강역사의 패 유물: 약한 공격 피격 시 경직 무시 (슈퍼아머)
                if (Status.HasRelic("medallion") && damage < 18)
                {
                    InvincibleTimer = 0.25f;
                    return;
                }

                // 20% 확률로 피격 시 넘어짐(down), 80% 확률로 일반 피격 경직(hit)
                bool isKnockedDown = KnockdownRng.NextDouble() < KnockdownChance;
                if (isKnockedDown)
                {
                    State = "down";
                    StateTimer = 0f;
                    HitStopTimer = 0f;
                    Vz = 120f;
                    Vx = knockbackDir * (counterHit ? 120f : 100f);
                }
                else
                {
                    State = "hit";
                    StateTimer = 0f;
                    InvincibleTimer = counterHit ? 0.45f : 0.6f;
                    Vx = knockbackDir * (counterHit ? 110f : 80f);
                }
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
                "guard" => ParryWindowTimer > 0 ? "player_parry" : "player_guard",
                "dragon" => AnimFrame > 0 ? "player_dragon_1" : "player_dragon_0",
                "ex_dragon" => AnimFrame > 0 ? "player_dragon_1" : "player_dragon_0",
                "wave" => AnimFrame > 0 ? "player_wave_1" : "player_wave_0",
                "ex_wave" => AnimFrame > 0 ? "player_wave_1" : "player_wave_0",
                "swing" => "player_swing",
                "knee" => "player_kick_0",
                "giantswing" => $"player_spin_{AnimFrame}",
                "dashpunch" => "player_punch_2",
                "dashkick" => "player_kick_2",
                "punch" => $"player_punch_{AnimFrame}",
                "kick" => $"player_kick_{AnimFrame}",
                "jumpkick" => "player_jumpkick",
                "spin" => $"player_spin_{AnimFrame}",
                "ex_spin" => $"player_spin_{AnimFrame}",
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
            // 삼대 삼한 비기 고해상도 이펙트 렌더링
            // ==========================================
            if (State == "spin" || State == "ex_spin")
            {
                RenderHunminSpinGlow(buffer, cameraX);
            }
            else if (State == "dragon" || State == "ex_dragon")
            {
                RenderDragonStrikeGlow(buffer, cameraX);
            }
            else if (State == "wave" || State == "ex_wave")
            {
                RenderMoonWaveGlow(buffer, cameraX);
            }

            // 순간 쳐내기 (Just Parry) 푸른 섬광 및 충격파 렌더링
            if (JustParryVisualTimer > 0)
            {
                RenderJustParryFlash(buffer, cameraX);
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

        private void RenderDragonStrikeGlow(PixelBuffer buffer, float cameraX)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            bool isEx = State == "ex_dragon";
            float dir = FacingRight ? 1f : -1f;

            // 960x720 고해상도 스크린 좌표계
            float scx = Perspective.ProjectScreenX(X, cameraX, Y);
            float scy = Perspective.ProjectScreenY(Y - 15f, Z);
            float sGroundY = Perspective.ProjectScreenY(Y, 0f);

            // 주먹 위치: 캐릭터 상체 앞쪽 및 높은 상승 타격점
            float fistX = scx + dir * 14f * PixelBuffer.Scale;
            float fistY = scy - 20f * PixelBuffer.Scale;

            // ----------------------------------------------------
            // 1. 지면 용오름 충격파 (Ground Dragon Ascent Blast)
            // ----------------------------------------------------
            float groundRatio = Math.Clamp(StateTimer / 0.40f, 0f, 1f);
            float groundRingW = (35f + groundRatio * 65f) * PixelBuffer.Scale;
            float groundRingH = (12f + groundRatio * 18f) * PixelBuffer.Scale;
            int groundAlpha = (int)(Math.Max(0f, 1f - groundRatio) * 220);

            if (groundAlpha > 0)
            {
                using var pGroundRing = new Pen(isEx ? Color.FromArgb(groundAlpha, 255, 120, 40) : Color.FromArgb(groundAlpha, 80, 230, 255), 4f);
                g.DrawEllipse(pGroundRing, scx - groundRingW / 2f, sGroundY - groundRingH / 2f, groundRingW, groundRingH);

                using var brGroundDust = new SolidBrush(Color.FromArgb(groundAlpha / 3, 255, 230, 150));
                g.FillEllipse(brGroundDust, scx - groundRingW * 0.4f, sGroundY - groundRingH * 0.4f, groundRingW * 0.8f, groundRingH * 0.8f);
            }

            // ----------------------------------------------------
            // 2. 승천 기둥: 지면부터 주먹까지 솟구치는 거대한 용오름 오라 (Dragon Ascension Column)
            // ----------------------------------------------------
            float colHeight = Math.Max(25f, sGroundY - fistY);
            float colWidth = (28f + (float)Math.Sin(StateTimer * 30f) * 6f) * PixelBuffer.Scale;

            // 바깥쪽 반투명 청록/황금 에너지 기둥
            using (var colBrush = new LinearGradientBrush(
                new PointF(scx, sGroundY), new PointF(scx, fistY),
                isEx ? Color.FromArgb(40, 255, 100, 20) : Color.FromArgb(30, 0, 140, 255),
                isEx ? Color.FromArgb(200, 255, 200, 50) : Color.FromArgb(200, 70, 220, 255)))
            {
                g.FillRectangle(colBrush, scx - colWidth / 2f, fistY, colWidth, colHeight);
            }

            // ----------------------------------------------------
            // 3. 나선형으로 용틀임하는 청룡 & 황금 쌍룡 회오리 (Ascending Double Helix Coils)
            // ----------------------------------------------------
            int spiralSegments = 16;
            for (int i = 0; i < spiralSegments; i++)
            {
                float segmentY = fistY + (i / (float)spiralSegments) * colHeight;
                float phase = StateTimer * 22f - i * 0.45f;
                float sway = (float)Math.Sin(phase) * (20f + i * 1.5f) * PixelBuffer.Scale;
                float segSize = (16f + (1f - i / (float)spiralSegments) * 14f) * PixelBuffer.Scale;

                float xA = scx + sway;
                float xB = scx - sway;

                // 나선 A: 비룡 청록 오라 구체
                using var brCyanCoil = new SolidBrush(isEx ? Color.FromArgb(190, 255, 80, 40) : Color.FromArgb(190, 40, 180, 255));
                g.FillEllipse(brCyanCoil, xA - segSize / 2f, segmentY - segSize / 3f, segSize, segSize * 0.65f);

                // 나선 B: 황금 번개 비늘 구체
                using var brGoldCoil = new SolidBrush(Color.FromArgb(210, 255, 225, 70));
                g.FillEllipse(brGoldCoil, xB - segSize * 0.4f, segmentY - segSize * 0.25f, segSize * 0.8f, segSize * 0.5f);

                // 코어 하이라이트
                using var brCore = new SolidBrush(Color.FromArgb(240, 255, 255, 240));
                g.FillEllipse(brCore, xA - segSize * 0.15f, segmentY - segSize * 0.15f, segSize * 0.3f, segSize * 0.3f);
            }

            // ----------------------------------------------------
            // 4. 거대한 비룡의 머리 (Roaring Azure Dragon Avatar / Maw)
            // 주먹 끝에서 하늘을 향해 포효하는 웅장한 용의 머리 실루엣
            // ----------------------------------------------------
            float headX = fistX + dir * 10f * PixelBuffer.Scale;
            float headY = fistY - 15f * PixelBuffer.Scale;
            float scaleD = (isEx ? 1.35f : 1.1f) * PixelBuffer.Scale;

            // 용의 턱, 뿔, 갈기, 주둥이를 그리는 GraphicsPath
            using (var dragonHeadPath = new GraphicsPath())
            {
                // 주둥이, 코끝, 아랫입술, 목, 뒷목 갈기, 큰 뿔, 작은 뿔, 이마
                PointF[] points = new PointF[]
                {
                    new PointF(headX + dir * 35f * scaleD, headY - 10f * scaleD), // 윗입술
                    new PointF(headX + dir * 42f * scaleD, headY - 2f * scaleD),  // 코끝
                    new PointF(headX + dir * 25f * scaleD, headY + 12f * scaleD), // 아랫입술
                    new PointF(headX + dir * 18f * scaleD, headY + 4f * scaleD),  // 입 안쪽
                    new PointF(headX - dir * 10f * scaleD, headY + 22f * scaleD), // 아래턱
                    new PointF(headX - dir * 20f * scaleD, headY + 5f * scaleD),  // 목덜미 갈기
                    new PointF(headX - dir * 28f * scaleD, headY - 24f * scaleD), // 큰 뿔 끝
                    new PointF(headX - dir * 10f * scaleD, headY - 14f * scaleD), // 뿔 분기점
                    new PointF(headX - dir * 16f * scaleD, headY - 32f * scaleD), // 작은 뿔 끝
                    new PointF(headX + dir * 5f * scaleD,  headY - 16f * scaleD), // 이마
                };
                dragonHeadPath.AddPolygon(points);

                // 반투명 용머리 내부 바디 채우기
                using var brDragonSkin = new SolidBrush(isEx ? Color.FromArgb(200, 240, 60, 30) : Color.FromArgb(200, 30, 160, 255));
                g.FillPath(brDragonSkin, dragonHeadPath);

                // 용머리 외곽선
                using var penDragonOutline = new Pen(isEx ? Color.FromArgb(255, 255, 210, 80) : Color.FromArgb(255, 170, 245, 255), 3.5f * PixelBuffer.Scale);
                g.DrawPath(penDragonOutline, dragonHeadPath);
            }

            // 용의 타오르는 안광 (Dragon Eye)
            float eyeX = headX + dir * 14f * scaleD;
            float eyeY = headY - 6f * scaleD;
            using (var brEyeGlow = new SolidBrush(Color.FromArgb(255, 255, 245, 60)))
            {
                g.FillEllipse(brEyeGlow, eyeX - 4f * PixelBuffer.Scale, eyeY - 4f * PixelBuffer.Scale, 8f * PixelBuffer.Scale, 8f * PixelBuffer.Scale);
            }
            using (var brEyePupil = new SolidBrush(Color.FromArgb(255, 255, 40, 20)))
            {
                g.FillEllipse(brEyePupil, eyeX - 2f * PixelBuffer.Scale, eyeY - 2f * PixelBuffer.Scale, 4f * PixelBuffer.Scale, 4f * PixelBuffer.Scale);
            }

            // 용의 긴 수염 (Dragon Whiskers) 2줄기
            using (var penWhisker = new Pen(isEx ? Color.FromArgb(240, 255, 230, 100) : Color.FromArgb(240, 150, 240, 255), 2.5f))
            {
                float wPhase = StateTimer * 18f;
                PointF[] whiskerUpper = new PointF[]
                {
                    new PointF(headX + dir * 38f * scaleD, headY - 4f * scaleD),
                    new PointF(headX + dir * 55f * scaleD, headY + (float)Math.Sin(wPhase) * 12f * scaleD),
                    new PointF(headX + dir * 75f * scaleD, headY - 15f * scaleD + (float)Math.Cos(wPhase) * 15f * scaleD),
                };
                g.DrawCurve(penWhisker, whiskerUpper);

                PointF[] whiskerLower = new PointF[]
                {
                    new PointF(headX + dir * 32f * scaleD, headY + 6f * scaleD),
                    new PointF(headX + dir * 48f * scaleD, headY + 20f * scaleD + (float)Math.Sin(wPhase + 1f) * 10f * scaleD),
                    new PointF(headX + dir * 68f * scaleD, headY + 12f * scaleD + (float)Math.Cos(wPhase + 1f) * 14f * scaleD),
                };
                g.DrawCurve(penWhisker, whiskerLower);
            }

            // ----------------------------------------------------
            // 5. 주먹 끝의 여의주(如意珠) 대폭발 섬광 & 방사형 광선 (Dragon Pearl Sunburst)
            // ----------------------------------------------------
            float pearlRadius = (16f + (float)Math.Sin(StateTimer * 35f) * 4f) * PixelBuffer.Scale;
            using (var brPearlHalo = new SolidBrush(Color.FromArgb(170, 255, 210, 50)))
            {
                g.FillEllipse(brPearlHalo, fistX - pearlRadius * 1.6f, fistY - pearlRadius * 1.6f, pearlRadius * 3.2f, pearlRadius * 3.2f);
            }
            using (var brPearlCore = new SolidBrush(Color.FromArgb(255, 255, 255, 240)))
            {
                g.FillEllipse(brPearlCore, fistX - pearlRadius, fistY - pearlRadius, pearlRadius * 2f, pearlRadius * 2f);
            }
            using (var penFlash = new Pen(Color.FromArgb(240, 255, 240, 160), 3f))
            {
                float rayLen = 42f * PixelBuffer.Scale;
                g.DrawLine(penFlash, fistX - rayLen, fistY, fistX + rayLen, fistY);
                g.DrawLine(penFlash, fistX, fistY - rayLen, fistX, fistY + rayLen);
                g.DrawLine(penFlash, fistX - rayLen * 0.7f, fistY - rayLen * 0.7f, fistX + rayLen * 0.7f, fistY + rayLen * 0.7f);
                g.DrawLine(penFlash, fistX - rayLen * 0.7f, fistY + rayLen * 0.7f, fistX + rayLen * 0.7f, fistY - rayLen * 0.7f);
            }

            // ----------------------------------------------------
            // 6. 장엄한 서예 한자 "龍 飛 御 天" 4자 분산 투사
            // ----------------------------------------------------
            string[] dragonChars = { "龍", "飛", "御", "天" };
            float[] charYOffsets = new float[]
            {
                sGroundY - 45f,                       // 龍 (지면 발구름 부근)
                sGroundY - (colHeight * 0.38f),       // 飛 (도약 중단)
                sGroundY - (colHeight * 0.72f),       // 御 (상승 상단)
                fistY - 35f                           // 天 (하늘 정점, 주먹 위)
            };

            for (int c = 0; c < 4; c++)
            {
                float cY = charYOffsets[c];
                float sideOffset = ((c % 2 == 0) ? -1f : 1f) * (26f * PixelBuffer.Scale);
                float cX = scx + sideOffset + (float)Math.Sin(StateTimer * 16f + c) * 10f;

                using (var brCharHalo = new SolidBrush(Color.FromArgb(190, 15, 25, 45)))
                {
                    g.FillEllipse(brCharHalo, cX - 22f, cY - 14f, 44f, 44f);
                }
                using (var penCharRing = new Pen(Color.FromArgb(230, 255, 215, 60), 2f))
                {
                    g.DrawEllipse(penCharRing, cX - 22f, cY - 14f, 44f, 44f);
                }

                buffer.DrawOutlinedText(dragonChars[c], cX - 11f, cY - 8f,
                    isEx ? Color.FromArgb(255, 240, 120) : Color.FromArgb(255, 235, 75),
                    Color.FromArgb(15, 20, 50),
                    buffer.FontSubtitle, 2.8f);
            }
        }

        private void RenderMoonWaveGlow(PixelBuffer buffer, float cameraX)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            float scx = Perspective.ProjectScreenX(X, cameraX, Y);
            float scy = Perspective.ProjectScreenY(Y - 15f, Z);

            // 1. 은빛 보름달 광륜 (Silver Celestial Moon Halo)
            float moonRadius = 45f * PixelBuffer.Scale;
            using (var moonPen = new Pen(Color.FromArgb(170, 210, 240, 255), 3.5f))
            using (var moonGlow = new SolidBrush(Color.FromArgb(45, 180, 220, 255)))
            {
                g.FillEllipse(moonGlow, scx - moonRadius, scy - moonRadius, moonRadius * 2f, moonRadius * 2f);
                g.DrawEllipse(moonPen, scx - moonRadius, scy - moonRadius, moonRadius * 2f, moonRadius * 2f);
            }

            // 2. 청홍 태극 파동구 (Taegeuk Energy Sphere at Hands)
            float handX = scx + (FacingRight ? 22f : -22f);
            float handY = scy - 5f;
            using (var blueTaegeuk = new SolidBrush(Color.FromArgb(220, 30, 90, 220)))
            using (var redTaegeuk = new SolidBrush(Color.FromArgb(220, 220, 40, 40)))
            {
                g.FillPie(blueTaegeuk, handX - 16f, handY - 16f, 32f, 32f, 0, 180);
                g.FillPie(redTaegeuk, handX - 16f, handY - 16f, 32f, 32f, 180, 180);
            }

            // 3. 150px 일직선 관통 광선 파동포 빔 (Piercing Wave Beam)
            float beamLength = 150f * PixelBuffer.Scale;
            float dir = FacingRight ? 1f : -1f;
            float endX = handX + dir * beamLength;

            // 외곽 푸른 파동 글로우
            using (var outerBeam = new Pen(Color.FromArgb(160, 70, 175, 255), 26f))
            {
                outerBeam.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                outerBeam.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawLine(outerBeam, handX, handY, endX, handY);
            }

            // 내부 순백 코어 빔
            using (var coreBeam = new Pen(Color.FromArgb(255, 255, 255, 230), 12f))
            {
                coreBeam.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                coreBeam.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                g.DrawLine(coreBeam, handX, handY, endX, handY);
            }

            // 4. 파동포 궤적을 타고 나아가는 광채 한자 "月 印 千 江"
            string[] waveChars = { "月", "印", "千", "江" };
            for (int w = 0; w < waveChars.Length; w++)
            {
                float charX = handX + dir * (35f + w * 32f) * PixelBuffer.Scale;
                buffer.DrawOutlinedText(waveChars[w], charX - 10f, handY - 14f, Color.FromArgb(255, 250, 150), Color.FromArgb(20, 30, 70), buffer.FontHeader, 2f);
            }
        }

        private void RenderJustParryFlash(PixelBuffer buffer, float cameraX)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            float scx = Perspective.ProjectScreenX(X, cameraX, Y);
            float scy = Perspective.ProjectScreenY(Y - 15f, Z);

            float ratio = JustParryVisualTimer / 0.35f;
            float ringR = (1f - ratio) * 55f * PixelBuffer.Scale + 15f;

            // 푸른빛 쳐내기 충격파 링
            using (var parryPen = new Pen(Color.FromArgb((int)(255 * ratio), 100, 235, 255), 4f))
            {
                g.DrawEllipse(parryPen, scx - ringR, scy - ringR, ringR * 2f, ringR * 2f);
            }

            // 섬광 십자 광선
            using (var rayPen = new Pen(Color.FromArgb((int)(255 * ratio), 255, 255, 255), 3f))
            {
                g.DrawLine(rayPen, scx - ringR * 1.5f, scy, scx + ringR * 1.5f, scy);
                g.DrawLine(rayPen, scx, scy - ringR * 1.5f, scx, scy + ringR * 1.5f);
            }

            // 텍스트 강조
            buffer.DrawOutlinedTextCentered("★ 순간 쳐내기 (Just Parry)! ★", (int)(scy - 45f), Color.FromArgb(140, 245, 255), Color.Black, buffer.FontSubtitle, 2f);
        }
    }
}
