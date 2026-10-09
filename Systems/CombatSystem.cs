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
        }

        public void CheckCollisions(Player player, List<Enemy> enemies, List<WorldObject>? worldObjects = null)
        {
            // 1. 플레이어의 공격 판정 -> 적 피격
            var playerHitbox = player.GetAttackHitbox();
            if (playerHitbox.HasValue && !player.HasDealtDamage)
            {
                var box = playerHitbox.Value;

                foreach (var enemy in enemies)
                {
                    if (enemy.IsDead || enemy.State == "down") continue;

                    // 2.5D 깊이 Y 판정 (같은 선상 +-15픽셀 내에 있어야 적중)
                    if (Math.Abs(player.Y - enemy.Y) > 15f) continue;

                    var enemyBody = enemy.GetBodyHitbox();
                    if (box.IntersectsWith(enemyBody))
                    {
                        player.HasDealtDamage = true;

                        int damage = player.CurrentAttackType switch
                        {
                            "spin" => player.Status.SpecialDamage,
                            "dashpunch" => player.Status.PunchDamage + 6,
                            "dashkick" => player.Status.KickDamage + 8,
                            "kick" => player.Status.KickDamage,
                            "jumpkick" => player.Status.KickDamage + 4,
                            "swing" => player.Status.PunchDamage + (player.CarriedObject != null
                                ? (player.CarriedObject.Type == ObjectType.Club ? 14 : (player.CarriedObject.Type == ObjectType.Crate ? 18 : 22))
                                : (player.CarriedEnemy != null ? 16 : 10)),
                            _ => player.Status.PunchDamage
                        };

                        // 스윙 공격 시 들고 있던 물체나 적의 내구도/HP 소모
                        if (player.CurrentAttackType == "swing")
                        {
                            if (player.CarriedObject != null)
                            {
                                player.CarriedObject.Durability--;
                                if (player.CarriedObject.Durability <= 0)
                                {
                                    player.CarriedObject.IsDead = true;
                                    player.CarriedObject = null;
                                    SoundSynth.PlayHit();
                                }
                            }
                            else if (player.CarriedEnemy != null)
                            {
                                player.CarriedEnemy.TakeDamage(14, 0f, out _);
                                DamageNumbers.Add(new DamageNumber(14, player.CarriedEnemy.X, player.CarriedEnemy.Y - player.CarriedEnemy.Z - player.CarriedEnemy.Height - 8f, isPlayerHit: false));
                                if (player.CarriedEnemy.CurrentHp <= 0)
                                {
                                    player.CarriedEnemy.IsDead = true;
                                    OnEnemyDefeated(player.CarriedEnemy, player);
                                    player.CarriedEnemy = null;
                                    player.CarriedEnemyTimer = 0f;
                                }
                            }
                        }

                        float knockDir = player.FacingRight ? 1f : -1f;
                        enemy.TakeDamage(damage, knockDir, out bool isKnockedDown);

                        // 타격감 극대화: 히트스톱 역경직, 화면 흔들림, 충격 스파크 방출!
                        bool isHeavy = player.CurrentAttackType == "spin" || player.CurrentAttackType == "dashpunch" ||
                                       player.CurrentAttackType == "dashkick" || player.CurrentAttackType == "swing" || isKnockedDown;
                        OnScreenShake?.Invoke(isHeavy ? 3.6f : 1.8f, isHeavy ? 0.20f : 0.09f);
                        OnHitStop?.Invoke(isHeavy ? 0.05f : 0.025f);
                        AddHitSparks(enemy.X, enemy.Y - enemy.Z - 16f, count: isHeavy ? 8 : 5);

                        // 머리 위에 떠오르는 데미지 숫자 생성!
                        DamageNumbers.Add(new DamageNumber(damage, enemy.X, enemy.Y - enemy.Z - enemy.Height - 8f, isPlayerHit: false));

                        // 한글 피격 대사 말풍선 생성!
                        string quote = enemy.GetRandomHitQuote();
                        Bubbles.Add(new SpeechBubble(quote, enemy.X, enemy.Y - enemy.Z - enemy.Height - 18f));

                        // 적 처치 시 보상 드롭
                        if (enemy.CurrentHp <= 0)
                        {
                            OnEnemyDefeated(enemy, player);
                        }
                        break;
                    }
                }
            }

            // 1-2. 날아가는 투척 오브젝트(목봉, 궤짝, 돌멩이) 판정
            if (worldObjects != null)
            {
                foreach (var obj in worldObjects)
                {
                    if (obj.IsDead || !obj.IsFlying) continue;

                    if (obj.ThrownByPlayer)
                    {
                        // 플레이어가 던진 물체 -> 적 피격 판정
                        foreach (var enemy in enemies)
                        {
                            if (enemy.IsDead || enemy.State == "down") continue;
                            if (Math.Abs(obj.Y - enemy.Y) > 16f) continue;

                            var enemyBody = enemy.GetBodyHitbox();
                            if (obj.GetBodyHitbox().IntersectsWith(enemyBody))
                            {
                                int throwDamage = obj.Type switch
                                {
                                    ObjectType.Rock => 28,
                                    ObjectType.Crate => 24,
                                    _ => 20
                                };
                                float dir = Math.Sign(obj.Vx);
                                if (dir == 0) dir = 1f;

                                enemy.TakeDamage(throwDamage, dir, out _);
                                DamageNumbers.Add(new DamageNumber(throwDamage, enemy.X, enemy.Y - enemy.Z - enemy.Height - 8f, isPlayerHit: false));
                                Bubbles.Add(new SpeechBubble(enemy.GetRandomHitQuote(), enemy.X, enemy.Y - enemy.Z - enemy.Height - 18f));

                                if (enemy.CurrentHp <= 0)
                                {
                                    OnEnemyDefeated(enemy, player);
                                }

                                obj.Durability--;
                                obj.IsFlying = false;
                                obj.Vx = -obj.Vx * 0.2f;
                                obj.Vz = 35f;
                                if (obj.Durability <= 0)
                                {
                                    obj.IsDead = true;
                                }
                                SoundSynth.PlayHit();
                                break;
                            }
                        }
                    }
                    else
                    {
                        // 적이 던진 물체 -> 플레이어 피격 판정!
                        if (player.InvincibleTimer <= 0 && player.State != "down")
                        {
                            if (Math.Abs(obj.Y - player.Y) <= 16f)
                            {
                                var playerBody = player.GetBodyHitbox();
                                if (obj.GetBodyHitbox().IntersectsWith(playerBody))
                                {
                                    int throwDamage = obj.Type switch
                                    {
                                        ObjectType.Rock => 16,
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
                            player.TakeDamage(enemy.AttackPower, knockDir);

                            OnScreenShake?.Invoke(0.24f, 4.2f);
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
                        }
                        item.IsDead = true;
                        Items.RemoveAt(i);
                    }
                }
            }
        }

        private void OnEnemyDefeated(Enemy enemy, Player player)
        {
            // 경험치 및 골드 보상
            player.Status.Gold += enemy.GoldReward;
            bool leveledUp = player.Status.AddExp(enemy.ExpReward, out bool leveled);
            if (leveled)
            {
                SoundSynth.PlayLevelUp();
                Bubbles.Add(new SpeechBubble("★ 레벨 업! (스탯 +3) ★", player.X, player.Y - 50f));
            }

            // 아이템 드롭 (확률)
            var rand = new Random();
            int roll = rand.Next(100);
            if (roll < 45)
            {
                Items.Add(new Item(enemy.X, enemy.Y, ItemType.Coin));
            }
            else if (roll < 75)
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
