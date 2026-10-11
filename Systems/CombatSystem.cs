using System;
using System.Collections.Generic;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Entities;
using HanlangGame.UI;

namespace HanlangGame.Systems
{
    public class CombatSystem
    {
        public List<SpeechBubble> Bubbles { get; } = new();
        public List<Item> Items { get; } = new();
        public List<DamageNumber> DamageNumbers { get; } = new();
        public List<GameParticle> Particles { get; } = new();

        public Action<float, float>? OnScreenShake;
        public Action<float>? OnHitStop;

        // 콤보 카운터 및 조선풍 스타일 랭크
        public int ComboCount { get; private set; } = 0;
        public float ComboTimer { get; private set; } = 0f;
        public int MaxCombo { get; private set; } = 0;
        public string CurrentStyleRank { get; private set; } = "";
        public float StyleRankDisplayTimer { get; private set; } = 0f;

        // 보스 정면 충돌 '합(合)' 힘겨루기 QTE
        public bool IsClashing { get; private set; } = false;
        public float ClashTimer { get; private set; } = 0f;
        public int ClashPressCount { get; private set; } = 0;
        public int ClashRequiredPresses { get; set; } = 4;
        public Enemy? ClashingBoss { get; private set; } = null;
        public float ClashCooldown { get; set; } = 0f;

        public void AddHitSparks(float x, float y, float z = 0, int count = 5)
        {
            var rand = new Random();
            Color[] sparkColors = { Color.FromArgb(255, 230, 80), Color.FromArgb(255, 255, 180), Color.FromArgb(255, 130, 60), Color.White };
            
            // 960x720 고해상도 충격파 링 생성
            Particles.Add(new GameParticle(x, y, z, 0, 0, 0, 0.24f, Color.FromArgb(255, 235, 100), 10, "ring"));

            for (int i = 0; i < count; i++)
            {
                float angle = (float)(rand.NextDouble() * Math.PI * 2);
                float speed = 50f + (float)rand.NextDouble() * 90f;
                float vx = (float)Math.Cos(angle) * speed;
                float vy = (float)Math.Sin(angle) * speed * 0.4f;
                float vz = 30f + (float)rand.NextDouble() * 70f;
                Color col = sparkColors[rand.Next(sparkColors.Length)];
                Particles.Add(new GameParticle(x, y, z, vx, vy, vz, 0.20f + (float)rand.NextDouble() * 0.15f, col, rand.Next(2, 4), "spark"));
            }
        }

        public void AddDust(float x, float y, float z = 0, int count = 3)
        {
            var rand = new Random();
            for (int i = 0; i < count; i++)
            {
                float vx = (float)(rand.NextDouble() * 30 - 15);
                float vy = (float)(rand.NextDouble() * 10 - 5);
                float vz = 8f + (float)rand.NextDouble() * 18f;
                Color dustCol = Color.FromArgb(175, 155, 130);
                Particles.Add(new GameParticle(x + (rand.Next(10) - 5), y + (rand.Next(6) - 3), z, vx, vy, vz, 0.24f + (float)rand.NextDouble() * 0.12f, dustCol, rand.Next(2, 4), "dust"));
            }
        }

        public void Update(float dt)
        {
            for (int i = Bubbles.Count - 1; i >= 0; i--)
            {
                Bubbles[i].Update(dt);
                if (Bubbles[i].IsExpired) Bubbles.RemoveAt(i);
            }

            for (int i = DamageNumbers.Count - 1; i >= 0; i--)
            {
                DamageNumbers[i].Update(dt);
                if (DamageNumbers[i].IsExpired) DamageNumbers.RemoveAt(i);
            }

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Items[i].Update(dt);
                if (Items[i].IsDead) Items.RemoveAt(i);
            }

            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                Particles[i].Update(dt);
                if (Particles[i].IsExpired) Particles.RemoveAt(i);
            }

            if (ComboTimer > 0)
            {
                ComboTimer -= dt;
                if (ComboTimer <= 0)
                {
                    ComboCount = 0;
                    CurrentStyleRank = "";
                }
            }

            if (StyleRankDisplayTimer > 0)
            {
                StyleRankDisplayTimer -= dt;
            }

            if (ClashCooldown > 0)
            {
                ClashCooldown -= dt;
            }

            if (IsClashing)
            {
                ClashTimer -= dt;
                if (ClashTimer <= 0)
                {
                    IsClashing = false;
                    ClashingBoss = null;
                }
            }
        }

        public void RegisterHit(Player player, Enemy enemy, int damage)
        {
            ComboCount++;
            ComboTimer = player.Status.HasRelic("mapae") ? 4.5f : 3.0f;
            if (ComboCount > MaxCombo) MaxCombo = ComboCount;

            // 콤보 보너스: 10콤보 이상 시 타격마다 MP 회복
            if (ComboCount >= 35) player.Status.CurrentMp = Math.Min(player.Status.MaxMp, player.Status.CurrentMp + 3);
            else if (ComboCount >= 20) player.Status.CurrentMp = Math.Min(player.Status.MaxMp, player.Status.CurrentMp + 2);
            else if (ComboCount >= 10) player.Status.CurrentMp = Math.Min(player.Status.MaxMp, player.Status.CurrentMp + 1);

            // 마패 유물: 10콤보 주기마다 MP +10 추가 보너스
            if (player.Status.HasRelic("mapae") && ComboCount % 10 == 0)
            {
                player.Status.CurrentMp = Math.Min(player.Status.MaxMp, player.Status.CurrentMp + 10);
            }

            // 스타일 랭크 승급 체크
            string newRank = ComboCount switch
            {
                >= 50 => "천명각성 (天命覺醒)",
                >= 35 => "일기당천 (一騎當千)",
                >= 20 => "쾌도난마 (快刀亂麻)",
                >= 10 => "질풍 (疾風)",
                >= 5 => "초출 (初出)",
                _ => ""
            };

            if (!string.IsNullOrEmpty(newRank) && newRank != CurrentStyleRank)
            {
                CurrentStyleRank = newRank;
                StyleRankDisplayTimer = 2.2f;
                SoundSynth.PlaySpecial();
            }
        }

        public void RegisterClashInput(Player player)
        {
            if (!IsClashing || ClashingBoss == null) return;

            ClashPressCount++;
            SoundSynth.PlayPunch();
            AddHitSparks(player.X + (player.FacingRight ? 18f : -18f), player.Y - 18f, count: 5);

            if (ClashPressCount >= ClashRequiredPresses)
            {
                // 합(合) 승리: 보스 튕겨나감 및 그로기 유발!
                var boss = ClashingBoss;
                IsClashing = false;
                ClashingBoss = null;
                ClashCooldown = 7.0f;

                boss.TakeDamage(35, player.FacingRight ? 1f : -1f);
                boss.State = "hit";
                boss.StateTimer = -2.2f; // 2.5초 긴 그로기 (Hit 상태 유지)
                boss.Vx = (player.FacingRight ? 1f : -1f) * 160f;
                boss.Vz = 100f;

                SoundSynth.PlayLevelUp();
                OnScreenShake?.Invoke(6.5f, 0.45f);
                OnHitStop?.Invoke(0.12f);
                DamageNumbers.Add(new DamageNumber(35, boss.X, boss.Y - boss.Height - 12f, false));
                Bubbles.Add(new SpeechBubble("격파! (그로기 상태)", boss.X, boss.Y - boss.Height - 20f));
            }
        }

        public void CheckCollisions(Player player, List<Enemy> enemies, List<WorldObject>? worldObjects = null)
        {
            // 0. 무릎치기 (Knee Strike): 포박한 적에게 직접 15 피해 타격
            if (player.CurrentAttackType == "knee" && player.CarriedEnemy != null && !player.HasDealtDamage)
            {
                player.HasDealtDamage = true;
                player.CarriedEnemy.TakeDamage(15, 0f, out _);
                DamageNumbers.Add(new DamageNumber(15, player.CarriedEnemy.X, player.CarriedEnemy.Y - player.CarriedEnemy.Z - player.CarriedEnemy.Height - 8f, isPlayerHit: false));
                AddHitSparks(player.CarriedEnemy.X, player.CarriedEnemy.Y - player.CarriedEnemy.Z - 14f, count: 8);
                Bubbles.Add(new SpeechBubble(player.CarriedEnemy.GetRandomHitQuote(), player.CarriedEnemy.X, player.CarriedEnemy.Y - player.CarriedEnemy.Z - player.CarriedEnemy.Height - 18f));
                SoundSynth.PlayHit();
                if (player.CarriedEnemy.CurrentHp <= 0)
                {
                    player.CarriedEnemy.IsDead = true;
                    OnEnemyDefeated(player.CarriedEnemy, player);
                    player.CarriedEnemy = null;
                    player.CarriedEnemyTimer = 0f;
                }
            }

            // 보스 정면 충돌 '합(合)' 힘겨루기 QTE 체크
            if (ClashCooldown <= 0f && !IsClashing)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy.IsDead || enemy.State == "down") continue;
                    if (enemy.IsBossType && (enemy.State == "punch" || enemy.State == "kick") &&
                        (player.IsAttackingOrRecovering || player.ParryWindowTimer > 0f) &&
                        Math.Abs(player.X - enemy.X) < 42f && Math.Abs(player.Y - enemy.Y) < 14f)
                    {
                        IsClashing = true;
                        ClashingBoss = enemy;
                        ClashTimer = 2.5f;
                        ClashPressCount = 0;
                        ClashRequiredPresses = 8;
                        ClashCooldown = 6.0f;
                        SoundSynth.PlaySpecial();
                        OnScreenShake?.Invoke(5.0f, 0.35f);
                        OnHitStop?.Invoke(0.1f);
                        Bubbles.Add(new SpeechBubble("합(合)!", (player.X + enemy.X) * 0.5f, player.Y - player.Height - 20f));
                        return;
                    }
                }
            }

            // 1. 플레이어의 공격 판정 -> 적 피격
            var playerHitbox = player.GetAttackHitbox();
            if (playerHitbox.HasValue && !player.HasDealtDamage)
            {
                var box = playerHitbox.Value;

                // 부채질(fan_wave) 또는 풍압(wave/ex_wave)으로 날아오는 적 화살 반사!
                if (player.CurrentAttackType == "fan_wave" || player.CurrentAttackType == "wave" || player.CurrentAttackType == "ex_wave")
                {
                    if (worldObjects != null)
                    {
                        foreach (var wObj in worldObjects)
                        {
                            if (wObj.IsDead || !wObj.IsFlying || wObj.ThrownByPlayer || wObj.Type != ObjectType.Arrow) continue;
                            if (Math.Abs(player.Y - wObj.Y) <= 18f && box.IntersectsWith(wObj.GetBodyHitbox()))
                            {
                                wObj.ThrownByPlayer = true;
                                wObj.Vx = (player.FacingRight ? 1f : -1f) * 360f;
                                wObj.Vz = 10f;
                                SoundSynth.PlayWhoosh();
                                AddHitSparks(wObj.X, wObj.Y - 10f, count: 8);
                                Bubbles.Add(new SpeechBubble("화살 반사!", player.X, player.Y - player.Height - 16f));
                            }
                        }
                    }
                }

                bool isMultiHit = (player.CurrentAttackType == "wave" || player.CurrentAttackType == "ex_wave" || player.CurrentAttackType == "ex_spin" || player.CurrentAttackType == "fan_wave");
                bool hasHitAny = false;

                foreach (var enemy in enemies)
                {
                    if (enemy.IsDead || enemy.State == "down") continue;

                    // 2.5D 깊이 Y 판정 (같은 선상 +-15픽셀 내에 있어야 적중)
                    if (Math.Abs(player.Y - enemy.Y) > 15f) continue;

                    var enemyBody = enemy.GetBodyHitbox();
                    if (box.IntersectsWith(enemyBody))
                    {
                        hasHitAny = true;
                        if (!isMultiHit)
                        {
                            player.HasDealtDamage = true;
                        }

                        int damage = player.CurrentAttackType switch
                        {
                            "spin" => player.Status.SpecialDamage,
                            "dragon" => (int)(player.Status.SpecialDamage * 1.15f),
                            "wave" => (int)(player.Status.SpecialDamage * 1.25f),
                            "ex_spin" => (int)(player.Status.SpecialDamage * 1.8f),
                            "ex_dragon" => (int)(player.Status.SpecialDamage * 2.0f),
                            "ex_wave" => (int)(player.Status.SpecialDamage * 2.2f),
                            "spear_stab" => player.Status.PunchDamage + 18,
                            "cudgel_swing" => player.Status.PunchDamage + 22,
                            "fan_wave" => player.Status.PunchDamage + 14,
                            "dashpunch" => player.Status.PunchDamage + 6,
                            "dashkick" => player.Status.KickDamage + 8,
                            "kick" => player.Status.KickDamage,
                            "jumpkick" => player.Status.KickDamage + 4,
                            "swing" => player.Status.PunchDamage + (player.CarriedObject != null
                                ? player.CarriedObject.Type switch
                                {
                                    ObjectType.Cart => 30,     // 대형 수레로 통쾌하게 후려치기
                                    ObjectType.Table => 26,    // 대형 탁자로 내려찍기
                                    ObjectType.Bench => 25,    // 평상으로 광역 휩쓸기
                                    ObjectType.Barrel => 24,   // 주막 술통
                                    ObjectType.RiceSack => 22, // 묵직한 쌀가마니 강타
                                    ObjectType.Crate => 18,    // 나무 궤짝
                                    ObjectType.Rock => 16,     // 돌멩이
                                    ObjectType.Club => 14,     // 목봉
                                    _ => 16
                                }
                                : (player.CarriedEnemy != null ? 16 : 10)),
                            "knee" => 15,
                            "giantswing" => 25,
                            _ => player.Status.PunchDamage
                        };

                        // 백호의 발톱 유물: 20% 확률 치명타 및 엽전 드롭
                        if (player.Status.HasRelic("tiger") && new Random().Next(100) < 20)
                        {
                            damage = (int)(damage * 1.5f);
                            Items.Add(new Item(enemy.X, enemy.Y, ItemType.Coin));
                            float sparkY = (player.CurrentAttackType == "jumpkick") ? (player.Y - player.Z - 12f) : (enemy.Y - enemy.Z - 16f);
                            AddHitSparks(enemy.X, sparkY, count: 8);
                            Bubbles.Add(new SpeechBubble("백호 일격!", enemy.X, enemy.Y - enemy.Height - 16f));
                        }

                        // 스윙 공격 시 들고 있던 물체의 내구도 소모
                        if (player.CurrentAttackType == "swing")
                        {
                            if (player.CarriedObject != null)
                            {
                                player.CarriedObject.Durability--;
                                if (player.CarriedObject.Durability <= 0 || player.CarriedObject.Type == ObjectType.RiceSack)
                                {
                                    if (player.CarriedObject.Type == ObjectType.RiceSack)
                                    {
                                        var dropType = new Random().Next(100) < 70 ? ItemType.RiceBall : ItemType.Coin;
                                        Items.Add(new Item(player.CarriedObject.X, player.CarriedObject.Y, dropType));
                                        SoundSynth.PlayShatter();
                                    }
                                    else if (player.CarriedObject.Type == ObjectType.Table || player.CarriedObject.Type == ObjectType.Jar)
                                    {
                                        SoundSynth.PlayShatter();
                                    }
                                    else
                                    {
                                        SoundSynth.PlayHit();
                                    }
                                    player.CarriedObject.IsDead = true;
                                    player.CarriedObject = null;
                                }
                            }
                        }

                        // 손에 든 장착 무기 내구도 소모
                        if (player.EquippedWeapon != null && (player.CurrentAttackType == "spear_stab" || player.CurrentAttackType == "cudgel_swing" || player.CurrentAttackType == "fan_wave"))
                        {
                            player.EquippedWeapon.Durability--;
                            if (player.EquippedWeapon.Durability <= 0)
                            {
                                SoundSynth.PlayShatter();
                                player.EquippedWeapon.IsDead = true;
                                player.EquippedWeapon = null;
                                Bubbles.Add(new SpeechBubble("무기 파손!", player.X, player.Y - player.Height - 16f));
                            }
                        }

                        // 공중 추격타 (Airborne Juggle Spike): 공중 날라차기로 체공 중인 적 강타 시 추가 데미지
                        bool isAirJuggle = (player.CurrentAttackType == "jumpkick" && enemy.Z > 8f);
                        if (isAirJuggle)
                        {
                            damage += 16;
                        }

                        float knockDir = player.FacingRight ? 1f : -1f;
                        enemy.TakeDamage(damage, knockDir, out bool isKnockedDown);

                        // 콤보 및 스타일 랭크 등록
                        RegisterHit(player, enemy, damage);

                        // 제2 비기 용비어천격: 적을 공중으로 솟구쳐 띄움 (Airborne Juggle)
                        if (player.CurrentAttackType == "dragon")
                        {
                            enemy.State = "down";
                            enemy.StateTimer = 0f;
                            enemy.Vz = 240f;
                            enemy.Vx = knockDir * 60f;
                        }
                        else if (player.CurrentAttackType == "ex_dragon")
                        {
                            enemy.State = "down";
                            enemy.StateTimer = 0f;
                            enemy.Vz = 340f;
                            enemy.Vx = knockDir * 120f;
                            OnScreenShake?.Invoke(5.5f, 0.3f);
                        }
                        else if (player.CurrentAttackType == "ex_spin")
                        {
                            float pullDir = Math.Sign(player.X - enemy.X);
                            enemy.Vx = pullDir * 160f;
                            enemy.State = "hit";
                            enemy.StateTimer = 0f;
                        }
                        else if (player.CurrentAttackType == "ex_wave")
                        {
                            enemy.State = "down";
                            enemy.StateTimer = 0f;
                            enemy.Vx = knockDir * 200f;
                            enemy.Vz = 90f;
                        }
                        else if (player.CurrentAttackType == "cudgel_swing")
                        {
                            enemy.State = "down";
                            enemy.StateTimer = 0f;
                            enemy.Vx = knockDir * 140f;
                            enemy.Vz = 60f;
                        }

                        // 공중 추격타 (Airborne Juggle Spike): 급강하 슬램!
                        if (isAirJuggle)
                        {
                            enemy.State = "down";
                            enemy.StateTimer = 0f;
                            enemy.Vz = -280f; // 지면으로 급강하 슬램!
                            enemy.Vx = knockDir * 110f;
                            OnScreenShake?.Invoke(4.6f, 0.22f);
                            OnHitStop?.Invoke(0.065f);
                            AddHitSparks(enemy.X, enemy.Y - enemy.Z - 16f, count: 12);
                            AddDust(enemy.X, enemy.Y, count: 6);
                            Bubbles.Add(new SpeechBubble("공중 강타!", enemy.X, enemy.Y - enemy.Z - enemy.Height - 18f));
                        }

                        // 타격감 극대화: 히트스톱 역경직, 화면 흔들림, 충격 스파크 방출!
                        bool isHeavy = player.CurrentAttackType == "spin" || player.CurrentAttackType == "dragon" ||
                                       player.CurrentAttackType == "wave" || player.CurrentAttackType == "ex_spin" ||
                                       player.CurrentAttackType == "ex_dragon" || player.CurrentAttackType == "ex_wave" ||
                                       player.CurrentAttackType == "cudgel_swing" || player.CurrentAttackType == "spear_stab" ||
                                       player.CurrentAttackType == "dashpunch" || player.CurrentAttackType == "dashkick" ||
                                       player.CurrentAttackType == "swing" || isKnockedDown;
                        OnScreenShake?.Invoke(isHeavy ? 3.8f : 1.8f, isHeavy ? 0.22f : 0.09f);
                        OnHitStop?.Invoke(isHeavy ? 0.055f : 0.025f);
                        float hitSparkY = (player.CurrentAttackType == "jumpkick") ? (player.Y - player.Z - 12f) : (enemy.Y - enemy.Z - 16f);
                        AddHitSparks(enemy.X, hitSparkY, count: isHeavy ? 9 : 5);

                        // 머리 위에 떠오르는 데미지 숫자 생성!
                        DamageNumbers.Add(new DamageNumber(damage, enemy.X, enemy.Y - enemy.Z - enemy.Height - 8f, isPlayerHit: false));

                        // 한글 피격 대사 말풍선 생성!
                        string quote = enemy.GetRandomHitQuote();
                        Bubbles.Add(new SpeechBubble(quote, enemy.X, enemy.Y - enemy.Z - enemy.Height - 18f));

                        // 3장 청계천 다리 밖 수장 판정: 다리 아래 물가로 튕겨나간 적 즉사
                        if (enemy.Y > 218f)
                        {
                            enemy.CurrentHp = 0;
                            enemy.IsDead = true;
                            SoundSynth.PlayShatter();
                            AddDust(enemy.X, enemy.Y, count: 8);
                            Bubbles.Add(new SpeechBubble("풍덩! (수장)", enemy.X, enemy.Y - 20f));
                            OnEnemyDefeated(enemy, player);
                        }
                        else if (enemy.CurrentHp <= 0)
                        {
                            // 적 처치 시 보상 드롭
                            OnEnemyDefeated(enemy, player);
                        }

                        if (!isMultiHit)
                        {
                            break;
                        }
                    }
                }

                if (isMultiHit && hasHitAny)
                {
                    player.HasDealtDamage = true;
                }

                // 플레이어 공격 -> 바닥 항아리/술통/화차 타격
                if (worldObjects != null && !player.HasDealtDamage)
                {
                    foreach (var obj in worldObjects)
                    {
                        if (obj.IsDead || obj.IsCarried || obj.IsFlying) continue;
                        if (Math.Abs(player.Y - obj.Y) > 16f) continue;

                        if (box.IntersectsWith(obj.GetBodyHitbox()))
                        {
                            player.HasDealtDamage = true;
                            if (obj.Type == ObjectType.Hwacha)
                            {
                                // 신기전 화차 타격 시 전방 다연장 로켓 화살 8발 일제 발사!
                                SoundSynth.PlaySpecial();
                                OnScreenShake?.Invoke(4.5f, 0.3f);
                                float objSparkY = (player.CurrentAttackType == "jumpkick") ? (player.Y - player.Z - 12f) : (obj.Y - 14f);
                                AddHitSparks(obj.X, objSparkY, count: 12);
                                Bubbles.Add(new SpeechBubble("신기전 일제 발사!", obj.X, obj.Y - 32f));
                                float fireDir = player.FacingRight ? 1f : -1f;
                                for (int k = 0; k < 8; k++)
                                {
                                    var arrow = new WorldObject(ObjectType.Arrow, obj.X + fireDir * (24f + k * 8f), obj.Y + (k % 3 - 1) * 6f)
                                    {
                                        IsFlying = true,
                                        ThrownByPlayer = true,
                                        Vx = fireDir * (300f + k * 22f),
                                        Vz = 10f + (k % 3) * 5f
                                    };
                                    worldObjects.Add(arrow);
                                }
                            }
                            else if (obj.Type == ObjectType.Jar || obj.Type == ObjectType.RiceSack)
                            {
                                obj.Durability = 0;
                                obj.IsDead = true;
                                SoundSynth.PlayShatter();
                                float objSparkY = (player.CurrentAttackType == "jumpkick") ? (player.Y - player.Z - 12f) : (obj.Y - 10f);
                                AddHitSparks(obj.X, objSparkY, count: 8);
                                AddDust(obj.X, obj.Y, count: 6);

                                // 옹기 항아리나 쌀가마니가 깨지며 아이템 드롭
                                var rand = new Random();
                                var dropType = (obj.Type == ObjectType.RiceSack)
                                    ? (rand.Next(100) < 75 ? ItemType.RiceBall : ItemType.Coin)
                                    : (rand.Next(100) < 65 ? ItemType.Coin : ItemType.RiceBall);
                                Items.Add(new Item(obj.X, obj.Y, dropType));
                            }
                            else if (obj.Type == ObjectType.Barrel || obj.Type == ObjectType.Cart)
                            {
                                obj.IsFlying = true;
                                obj.ThrownByPlayer = true;
                                obj.Vx = (player.FacingRight ? 1f : -1f) * (obj.Type == ObjectType.Cart ? 250f : 230f);
                                obj.Vz = 25f;
                                SoundSynth.PlayWhoosh();
                                AddDust(obj.X, obj.Y, count: 5);
                            }
                            else
                            {
                                obj.Durability--;
                                float objSparkY = (player.CurrentAttackType == "jumpkick") ? (player.Y - player.Z - 12f) : (obj.Y - 10f);
                                AddHitSparks(obj.X, objSparkY, count: 5);
                                if (obj.Durability <= 0)
                                {
                                    obj.IsDead = true;
                                    SoundSynth.PlayShatter();
                                    AddDust(obj.X, obj.Y, count: 6);
                                    if (new Random().Next(100) < 35)
                                    {
                                        string[] rPool = { "풍", "철", "화", "어", "가" };
                                        Items.Add(new Item(obj.X, obj.Y, ItemType.Rune) { RuneWord = rPool[new Random().Next(rPool.Length)] });
                                    }
                                }
                                else
                                {
                                    SoundSynth.PlayHit();
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // 1-2. 날아가거나 굴러가는 투척 오브젝트(수레, 탁자, 평상, 쌀가마니, 목봉, 궤짝, 돌멩이, 항아리, 술통) 판정
            if (worldObjects != null)
            {
                foreach (var obj in worldObjects)
                {
                    if (obj.IsDead || (!obj.IsFlying && !obj.IsRolling)) continue;

                    if (obj.ThrownByPlayer)
                    {
                        // 플레이어가 던지거나 굴린 물체 -> 적 피격 판정
                        foreach (var enemy in enemies)
                        {
                            if (enemy.IsDead || enemy.State == "down") continue;
                            if (Math.Abs(obj.Y - enemy.Y) > 18f) continue;

                            var enemyBody = enemy.GetBodyHitbox();
                            if (obj.GetBodyHitbox().IntersectsWith(enemyBody))
                            {
                                int throwDamage = obj.Type switch
                                {
                                    ObjectType.Cart => 36,     // 대형 수레 충돌 (최대 데미지)
                                    ObjectType.Arrow => 35,    // 반사된 화살 저격
                                    ObjectType.Table => 32,    // 대형 탁자 투척
                                    ObjectType.Bench => 30,    // 대형 평상 투척
                                    ObjectType.Barrel => 30,   // 술통
                                    ObjectType.Rock => 28,     // 돌멩이
                                    ObjectType.RiceSack => 26, // 볏짚 쌀가마니
                                    ObjectType.Crate => 24,    // 궤짝
                                    ObjectType.Jar => 18,      // 항아리
                                    _ => 20
                                };
                                float dir = Math.Sign(obj.Vx);
                                if (dir == 0) dir = 1f;

                                enemy.TakeDamage(throwDamage, dir, out _);
                                if (obj.Type == ObjectType.Barrel || obj.Type == ObjectType.Cart || obj.Type == ObjectType.Bench)
                                {
                                    // 술통, 대형 수레, 평상에 부딪힌 적은 바닥으로 굴러넘어짐!
                                    enemy.State = "down";
                                    enemy.StateTimer = 0f;
                                }

                                DamageNumbers.Add(new DamageNumber(throwDamage, enemy.X, enemy.Y - enemy.Z - enemy.Height - 8f, isPlayerHit: false));
                                Bubbles.Add(new SpeechBubble(enemy.GetRandomHitQuote(), enemy.X, enemy.Y - enemy.Z - enemy.Height - 18f));

                                if (enemy.CurrentHp <= 0)
                                {
                                    OnEnemyDefeated(enemy, player);
                                }

                                obj.Durability--;
                                if (obj.Type == ObjectType.Jar || obj.Type == ObjectType.RiceSack)
                                {
                                    obj.Durability = 0;
                                    obj.IsDead = true;
                                    SoundSynth.PlayShatter();
                                    var dropType = (obj.Type == ObjectType.RiceSack)
                                        ? (new Random().Next(100) < 75 ? ItemType.RiceBall : ItemType.Coin)
                                        : ItemType.Coin;
                                    Items.Add(new Item(obj.X, obj.Y, dropType));
                                }
                                else if (obj.Type == ObjectType.Barrel || obj.Type == ObjectType.Cart)
                                {
                                    obj.Vx *= (obj.Type == ObjectType.Cart ? 0.78f : 0.65f);
                                    SoundSynth.PlayHit();
                                    if (obj.Durability <= 0)
                                    {
                                        obj.IsDead = true;
                                        SoundSynth.PlayShatter();
                                    }
                                }
                                else if (obj.Type == ObjectType.Arrow)
                                {
                                    obj.IsDead = true;
                                    SoundSynth.PlayHit();
                                }
                                else
                                {
                                    obj.IsFlying = false;
                                    obj.Vx = -obj.Vx * 0.2f;
                                    obj.Vz = 35f;
                                    if (obj.Durability <= 0)
                                    {
                                        obj.IsDead = true;
                                        if (obj.Type == ObjectType.Table) SoundSynth.PlayShatter();
                                    }
                                    SoundSynth.PlayHit();
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 적이 던진 물체 및 발사한 화살 -> 플레이어 피격 판정!
                        if (player.InvincibleTimer <= 0 && player.State != "down")
                        {
                            if (Math.Abs(obj.Y - player.Y) <= 18f)
                            {
                                var playerBody = player.GetBodyHitbox();
                                if (obj.GetBodyHitbox().IntersectsWith(playerBody))
                                {
                                    if (obj.Type == ObjectType.Arrow)
                                    {
                                        float arrowDir = Math.Sign(obj.Vx);
                                        if (arrowDir == 0) arrowDir = 1f;

                                        player.TakeDamage(12, arrowDir, out bool parried, out bool guarded, out bool counterHit);
                                        if (parried)
                                        {
                                            // 화살 저스트 패링 성공! 화살 반사 (Arrow Reflect)!
                                            obj.ThrownByPlayer = true;
                                            obj.Vx = (player.FacingRight ? 1f : -1f) * 360f;
                                            obj.Vz = 10f;
                                            OnScreenShake?.Invoke(4.0f, 0.20f);
                                            OnHitStop?.Invoke(0.07f);
                                            AddHitSparks(player.X, player.Y - 14f, count: 12);
                                            Bubbles.Add(new SpeechBubble("반사!", player.X, player.Y - player.Height - 18f));
                                            return;
                                        }
                                        else if (guarded)
                                        {
                                            obj.IsDead = true;
                                            OnScreenShake?.Invoke(1.5f, 0.08f);
                                            AddHitSparks(player.X, player.Y - 14f, count: 5);
                                            DamageNumbers.Add(new DamageNumber(4, player.X, player.Y - player.Height - 8f, isPlayerHit: true));
                                            Bubbles.Add(new SpeechBubble("가드!", player.X, player.Y - player.Height - 18f));
                                            return;
                                        }
                                        else
                                        {
                                            obj.IsDead = true;
                                            int arrowDmg = counterHit ? 16 : 12;
                                            OnScreenShake?.Invoke(2.4f, 0.14f);
                                            AddHitSparks(player.X, player.Y - 14f, count: 7);
                                            DamageNumbers.Add(new DamageNumber(arrowDmg, player.X, player.Y - player.Height - 8f, isPlayerHit: true));
                                            Bubbles.Add(new SpeechBubble(counterHit ? "카운터!" : "화살 피격!", player.X, player.Y - player.Height - 18f));
                                            return;
                                        }
                                    }

                                    int throwDamage = obj.Type switch
                                    {
                                        ObjectType.Cart => 24,
                                        ObjectType.Table => 20,
                                        ObjectType.Bench => 18,
                                        ObjectType.Rock => 16,
                                        ObjectType.RiceSack => 16,
                                        ObjectType.Crate => 14,
                                        _ => 12
                                    };
                                    float dir = Math.Sign(obj.Vx);
                                    if (dir == 0) dir = 1f;

                                    player.TakeDamage(throwDamage, dir);
                                    DamageNumbers.Add(new DamageNumber(throwDamage, player.X, player.Y - player.Z - player.Height - 8f, isPlayerHit: true));
                                    Bubbles.Add(new SpeechBubble("따끔!", player.X, player.Y - player.Z - player.Height - 18f));

                                    obj.Durability--;
                                    obj.IsFlying = false;
                                    obj.Vx = -obj.Vx * 0.2f;
                                    obj.Vz = 30f;
                                    if (obj.Durability <= 0)
                                    {
                                        obj.IsDead = true;
                                    }
                                    SoundSynth.PlayHit();
                                }
                            }
                        }
                    }
                }
            }

            // 1-3. 던져져서 날아가는 적 -> 다른 적 충돌 판정
            foreach (var thrownEnemy in enemies)
            {
                if (thrownEnemy.IsDead || !thrownEnemy.IsFlying) continue;

                foreach (var target in enemies)
                {
                    if (target == thrownEnemy || target.IsDead || target.State == "down") continue;
                    if (Math.Abs(thrownEnemy.Y - target.Y) > 16f) continue;

                    if (thrownEnemy.GetBodyHitbox().IntersectsWith(target.GetBodyHitbox()))
                    {
                        float dir = Math.Sign(thrownEnemy.Vx);
                        if (dir == 0) dir = 1f;

                        target.TakeDamage(25, dir, out _);
                        thrownEnemy.TakeDamage(18, -dir, out _);

                        DamageNumbers.Add(new DamageNumber(25, target.X, target.Y - target.Z - target.Height - 8f, isPlayerHit: false));
                        DamageNumbers.Add(new DamageNumber(18, thrownEnemy.X, thrownEnemy.Y - thrownEnemy.Z - thrownEnemy.Height - 8f, isPlayerHit: false));

                        thrownEnemy.IsFlying = false;
                        thrownEnemy.State = "down";
                        thrownEnemy.StateTimer = 0f;

                        Bubbles.Add(new SpeechBubble(target.GetRandomHitQuote(), target.X, target.Y - target.Z - target.Height - 18f));
                        Bubbles.Add(new SpeechBubble(thrownEnemy.GetRandomHitQuote(), thrownEnemy.X, thrownEnemy.Y - thrownEnemy.Z - thrownEnemy.Height - 18f));

                        if (target.CurrentHp <= 0) OnEnemyDefeated(target, player);
                        if (thrownEnemy.CurrentHp <= 0) OnEnemyDefeated(thrownEnemy, player);

                        SoundSynth.PlayHit();
                        break;
                    }
                }
            }

            // 2. 적의 공격 판정 -> 플레이어 피격
            foreach (var enemy in enemies)
            {
                if (enemy.OnWallBounce == null)
                {
                    enemy.OnWallBounce = (bx, by) =>
                    {
                        OnScreenShake?.Invoke(3.4f, 0.16f);
                        AddDust(bx, by, count: 6);
                        AddHitSparks(bx, by - 16f, count: 8);
                        Bubbles.Add(new SpeechBubble("벽 쿵!", bx, by - 24f));
                    };
                }

                if (enemy.IsDead || enemy.State == "down") continue;

                var enemyHitbox = enemy.GetAttackHitbox();
                if (enemyHitbox.HasValue && !enemy.HasDealtDamage)
                {
                    if (Math.Abs(player.Y - enemy.Y) <= 15f)
                    {
                        var playerBody = player.GetBodyHitbox();
                        if (enemyHitbox.Value.IntersectsWith(playerBody))
                        {
                            enemy.HasDealtDamage = true;
                            float knockDir = enemy.FacingRight ? 1f : -1f;
                            player.TakeDamage(enemy.AttackPower, knockDir, out bool parried, out bool guarded, out bool counterHit);

                            if (parried)
                            {
                                // 순간 쳐내기 (Just Parry): 적 1.25초 스턴, 큰 화면 흔들림 & 역경직, 푸른빛 스파크 방출
                                enemy.State = "hit";
                                enemy.StateTimer = -0.9f;
                                enemy.Vx = (player.FacingRight ? 1f : -1f) * 90f;
                                OnScreenShake?.Invoke(4.2f, 0.22f);
                                OnHitStop?.Invoke(0.08f);
                                AddHitSparks(player.X, player.Y - player.Z - 16f, count: 12);
                                Bubbles.Add(new SpeechBubble("쳐내기!", player.X, player.Y - player.Z - player.Height - 18f));
                                Bubbles.Add(new SpeechBubble("크윽!", enemy.X, enemy.Y - enemy.Z - enemy.Height - 18f));

                                // 단전 벼락구슬 유물: 패링 시 적에게 낙뢰 28 피해!
                                if (player.Status.HasRelic("spark"))
                                {
                                    enemy.TakeDamage(28, 0f, out _);
                                    DamageNumbers.Add(new DamageNumber(28, enemy.X, enemy.Y - enemy.Z - enemy.Height - 12f, isPlayerHit: false));
                                    AddHitSparks(enemy.X, enemy.Y - 18f, count: 15);
                                    SoundSynth.PlaySpecial();
                                    Bubbles.Add(new SpeechBubble("낙뢰!", enemy.X, enemy.Y - enemy.Height - 20f));
                                    if (enemy.CurrentHp <= 0)
                                    {
                                        OnEnemyDefeated(enemy, player);
                                    }
                                }
                            }
                            else if (guarded)
                            {
                                // 철벽 가드 (Guard): 70% 피해 감소, 경직/넉다운 방지
                                int reducedDmg = Math.Max(1, (int)(enemy.AttackPower * 0.30f));
                                OnScreenShake?.Invoke(1.6f, 0.10f);
                                OnHitStop?.Invoke(0.03f);
                                AddHitSparks(player.X, player.Y - player.Z - 16f, count: 5);
                                DamageNumbers.Add(new DamageNumber(reducedDmg, player.X, player.Y - player.Z - player.Height - 8f, isPlayerHit: true));
                                Bubbles.Add(new SpeechBubble("가드!", player.X, player.Y - player.Z - player.Height - 18f));
                            }
                            else if (counterHit)
                            {
                                // 카운터 피격! (공격 동작 중 또는 공격 후딜레이 중 피격 시 +25% 증폭 데미지, 큰 화면 흔들림)
                                int counterDmg = (int)(enemy.AttackPower * 1.25f);
                                OnScreenShake?.Invoke(4.5f, 0.24f);
                                OnHitStop?.Invoke(0.065f);
                                AddHitSparks(player.X, player.Y - player.Z - 16f, count: 10);
                                DamageNumbers.Add(new DamageNumber(counterDmg, player.X, player.Y - player.Z - player.Height - 8f, isPlayerHit: true));
                                Bubbles.Add(new SpeechBubble("카운터!", player.X, player.Y - player.Z - player.Height - 18f));
                            }
                            else
                            {
                                // 통상 피격
                                OnScreenShake?.Invoke(2.4f, 0.14f);
                                OnHitStop?.Invoke(0.045f);
                                AddHitSparks(player.X, player.Y - player.Z - 16f, count: 7);

                                // 플레이어 머리 위에 선홍색 피격 데미지 숫자 생성!
                                DamageNumbers.Add(new DamageNumber(enemy.AttackPower, player.X, player.Y - player.Z - player.Height - 8f, isPlayerHit: true));

                                // 플레이어 피격 말풍선
                                Bubbles.Add(new SpeechBubble("따끔!", player.X, player.Y - player.Z - player.Height - 18f));
                            }
                        }
                    }
                }
            }

            // 3. 플레이어의 아이템 습득 판정
            var playerBase = player.GetBaseHitbox();
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var item = Items[i];
                if (Math.Abs(player.Y - item.Y) <= 14f)
                {
                    var itemBase = item.GetBaseHitbox();
                    if (playerBase.IntersectsWith(itemBase))
                    {
                        // 아이템 획득
                        switch (item.Type)
                        {
                            case ItemType.Coin:
                                player.Status.Gold += 20;
                                SoundSynth.PlayCoin();
                                Bubbles.Add(new SpeechBubble("+20냥!", player.X, player.Y - 35f));
                                break;
                            case ItemType.RiceBall:
                                player.Status.CurrentHp = Math.Min(player.Status.MaxHp, player.Status.CurrentHp + 35);
                                SoundSynth.PlayCoin();
                                Bubbles.Add(new SpeechBubble("주먹밥! (HP+35)", player.X, player.Y - 35f));
                                break;
                            case ItemType.Scroll:
                                bool lvUp = player.Status.AddExp(45, out bool leveled);
                                SoundSynth.PlayCoin();
                                Bubbles.Add(new SpeechBubble("훈민비급! (EXP+45)", player.X, player.Y - 35f));
                                if (leveled)
                                {
                                    SoundSynth.PlayLevelUp();
                                    Bubbles.Add(new SpeechBubble("★ 레벨 업! ★", player.X, player.Y - 50f));
                                }
                                break;
                            case ItemType.Rune:
                                bool added = player.Status.AddRune(item.RuneWord, out string? unlocked);
                                SoundSynth.PlaySpecial();
                                if (added)
                                {
                                    Bubbles.Add(new SpeechBubble($"[{item.RuneWord}] 룬 획득!", player.X, player.Y - 35f));
                                    if (!string.IsNullOrEmpty(unlocked))
                                    {
                                        SoundSynth.PlayLevelUp();
                                        Bubbles.Add(new SpeechBubble($"★ {unlocked} ★", player.X, player.Y - 55f));
                                    }
                                }
                                break;
                        }
                        item.IsDead = true;
                        Items.RemoveAt(i);
                    }
                }
            }
        }

        private void OnEnemyDefeated(Enemy enemy, Player player)
        {
            // 경험치 및 골드 보상 (콤보 수치에 따른 골드 보너스 가산)
            int comboBonus = (int)(ComboCount * 1.5f);
            player.Status.Gold += enemy.GoldReward + comboBonus;
            bool leveledUp = player.Status.AddExp(enemy.ExpReward, out bool leveled);
            if (leveled)
            {
                SoundSynth.PlayLevelUp();
                Bubbles.Add(new SpeechBubble("★ 레벨 업! (스탯 +3) ★", player.X, player.Y - 50f));
            }

            // 주모의 약주단지 유물: 적 처치 시 35% 확률로 주먹밥 즉시 드롭
            if (player.Status.HasRelic("gourd") && new Random().Next(100) < 35)
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.RiceBall));
                Bubbles.Add(new SpeechBubble("약주단지 발동!", enemy.X, enemy.Y - 25f));
            }

            // 보스는 훈민정음 신기 문자 룬 확정 드롭!
            if (enemy.Type == EnemyType.MidBoss1)
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.Rune) { RuneWord = "용" });
                return;
            }
            else if (enemy.Type == EnemyType.MidBoss2)
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.Rune) { RuneWord = "비" });
                return;
            }
            else if (enemy.Type == EnemyType.Boss)
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.Rune) { RuneWord = "천" });
                return;
            }

            // 일반 적 처치 시 룬 또는 소모 아이템 드롭 (확률)
            var rand = new Random();
            int roll = rand.Next(100);
            if (roll < 18)
            {
                string[] runes = { "풍", "철", "화", "어", "가" };
                string dropRune = runes[rand.Next(runes.Length)];
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.Rune) { RuneWord = dropRune });
            }
            else if (roll < 50)
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.Coin));
            }
            else if (roll < 78)
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.Scroll));
            }
            else
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.RiceBall));
            }
        }

        public void Render(PixelBuffer buffer, float cameraX)
        {
            // 아이템은 GameScene의 2.5D 깊이 정렬에서 렌더링되므로 여기서는 파티클, 데미지, 말풍선 출력
            foreach (var p in Particles)
            {
                p.Render(buffer, cameraX);
            }
            foreach (var num in DamageNumbers)
            {
                num.Render(buffer, cameraX);
            }
            foreach (var b in Bubbles)
            {
                b.Render(buffer, cameraX);
            }
        }
    }

    public class GameParticle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Vx { get; set; }
        public float Vy { get; set; }
        public float Vz { get; set; }
        public float Life { get; set; }
        public float MaxLife { get; set; }
        public Color Color { get; set; }
        public int Size { get; set; }
        public string Type { get; set; } // "spark", "dust", "ring"

        public bool IsExpired => Life <= 0f;

        public GameParticle(float x, float y, float z, float vx, float vy, float vz, float life, Color color, int size, string type)
        {
            X = x; Y = y; Z = z;
            Vx = vx; Vy = vy; Vz = vz;
            Life = MaxLife = life;
            Color = color;
            Size = size;
            Type = type;
        }

        public void Update(float dt)
        {
            X += Vx * dt;
            Y += Vy * dt;
            Z += Vz * dt;
            Life -= dt;
            if (Type == "dust")
            {
                Vx *= 0.82f;
                Vy *= 0.82f;
            }
            else if (Type == "spark")
            {
                Vz -= 180f * dt;
            }
        }

        public void Render(PixelBuffer buffer, float cameraX)
        {
            float p = Perspective.GetScale(Y);
            float scx = Perspective.ProjectScreenX(X, cameraX, Y);
            float scy = Perspective.ProjectScreenY(Y, Z);
            if (scx < -60 || scx > PixelBuffer.Width + 60 || scy < -60 || scy > PixelBuffer.Height + 60) return;

            var g = buffer.Graphics;
            float lifeRatio = Math.Clamp(Life / MaxLife, 0f, 1f);

            if (Type == "spark")
            {
                // 960x720 고해상도 섬광 스파크: 빛나는 4점 다이아몬드 별 및 광채 (원근 스케일 p 반영)
                float radius = Size * PixelBuffer.Scale * 1.6f * lifeRatio * p;
                int alpha = (int)(255 * lifeRatio);
                Color glowCol = Color.FromArgb(alpha, Color);

                using var coreBrush = new SolidBrush(Color.FromArgb(alpha, Color.White));
                g.FillEllipse(coreBrush, scx - radius * 0.5f, scy - radius * 0.5f, radius, radius);

                using var rayPen = new Pen(glowCol, 2.2f * p);
                using var whiteRayPen = new Pen(Color.FromArgb(alpha, Color.White), 1.2f * p);
                g.DrawLine(rayPen, scx - radius * 2.2f, scy, scx + radius * 2.2f, scy);
                g.DrawLine(rayPen, scx, scy - radius * 2.2f, scx, scy + radius * 2.2f);
                g.DrawLine(whiteRayPen, scx - radius * 1.1f, scy, scx + radius * 1.1f, scy);
                g.DrawLine(whiteRayPen, scx, scy - radius * 1.1f, scx, scy + radius * 1.1f);
            }
            else if (Type == "ring")
            {
                // 960x720 고해상도 충격파 링 (Shockwave Ring, 원근 스케일 p 반영)
                float radius = ((1f - lifeRatio) * 65f + 14f) * p;
                int alpha = (int)(220 * lifeRatio);
                using var ringPen = new Pen(Color.FromArgb(alpha, Color), 3.2f * p);
                g.DrawEllipse(ringPen, scx - radius, scy - radius, radius * 2f, radius * 2f);
            }
            else // dust
            {
                // 960x720 고해상도 흙먼지: 부드러운 안티앨리어싱 반투명 구름 (원근 스케일 p 반영)
                float radius = ((Size * PixelBuffer.Scale * 1.8f) + (1f - lifeRatio) * 16f) * p;
                int alpha = (int)(110 * lifeRatio);
                using var dustBrush = new SolidBrush(Color.FromArgb(alpha, Color));
                g.FillEllipse(dustBrush, scx - radius, scy - radius, radius * 2f, radius * 2f);
            }
        }
    }
}
