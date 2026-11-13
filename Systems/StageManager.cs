using System;
using System.Collections.Generic;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Entities;
using HanlangGame.Visuals;

namespace HanlangGame.Systems
{
    public class StageManager
    {
        public const int MaxStage = 9;
        public int CurrentStage { get; set; } = 1;
        public float StageWidth => 860f; // 횡스크롤 길이

        public List<Enemy> Enemies { get; } = new();
        public List<WorldObject> WorldObjects { get; } = new();
        public bool IsSurvivalMode { get; set; } = false;
        public int SurvivalWave { get; set; } = 1;
        public bool StageCleared => IsSurvivalMode ? (Enemies.Count == 0) : (Enemies.Count == 0 && CurrentStage < MaxStage);
        public bool GameWon { get; set; } = false;

        public Enemy? CurrentBoss => Enemies.Find(e => e.IsBossType);

        public string GetStageName(int stage)
        {
            if (IsSurvivalMode) return $"무한 연무장 : 제 {SurvivalWave} 파도 (Wave {SurvivalWave})";
            return stage switch
            {
                1 => "1장: 한양 저잣거리",
                2 => "2장: 서당 뒷골목",
                3 => "3장: 청계천 다리 (중간보스전)",
                4 => "4장: 남산 성곽길",
                5 => "5장: 도성 주막거리",
                6 => "6장: 육조거리 (중간보스전)",
                7 => "7장: 광화문 앞뜰",
                8 => "8장: 경복궁 근정전",
                9 => "9장: 집현전 천추전 (최종결전)",
                _ => $"{stage}장"
            };
        }

        public void LoadStage(int stage, Player? player = null)
        {
            IsSurvivalMode = false;
            CurrentStage = stage;
            GameWon = false; // 재시작 시 엔딩이 다시 뜨지 않도록 완벽 리셋!

            // 플레이어가 들고 있는 기물, 장착 무기, 포박한 적 보존
            WorldObject? carriedObj = (player != null && player.CarriedObject != null && !player.CarriedObject.IsDead) ? player.CarriedObject : null;
            WorldObject? equippedWp = (player != null && player.EquippedWeapon != null && !player.EquippedWeapon.IsDead) ? player.EquippedWeapon : null;
            Enemy? carriedEnemy = (player != null && player.CarriedEnemy != null && !player.CarriedEnemy.IsDead) ? player.CarriedEnemy : null;

            Enemies.Clear();
            WorldObjects.Clear();

            switch (stage)
            {
                case 1:
                    // 1장: 한양 저잣거리 - 골목 깡패 무리
                    Enemies.Add(new Enemy(EnemyType.Thug, 230f, 160f, stage));
                    Enemies.Add(new Enemy(EnemyType.Thug, 360f, 195f, stage));
                    Enemies.Add(new Enemy(EnemyType.Thug, 500f, 175f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 660f, 180f, stage));
                    Enemies.Add(new Enemy(EnemyType.Thug, 780f, 210f, stage));
                    break;

                case 2:
                    // 2장: 서당 뒷골목 - 깡패와 불량 훈도 연합
                    Enemies.Add(new Enemy(EnemyType.Scholar, 240f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.Thug, 380f, 185f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 520f, 205f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 660f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.Thug, 780f, 195f, stage));
                    break;

                case 3:
                    // 3장: 청계천 다리 - 중간보스 괴력 건달 마달 등장!
                    Enemies.Add(new Enemy(EnemyType.Thug, 250f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 420f, 195f, stage));
                    Enemies.Add(new Enemy(EnemyType.Thug, 560f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.MidBoss1, 720f, 185f, stage));
                    break;

                case 4:
                    // 4장: 남산 성곽길 - 암흑 결사대 & 성곽 궁수 난입
                    Enemies.Add(new Enemy(EnemyType.Ninja, 260f, 160f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 400f, 190f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 540f, 175f, stage));
                    Enemies.Add(new Enemy(EnemyType.Archer, 680f, 205f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 790f, 180f, stage));
                    break;

                case 5:
                    // 5장: 도성 주막거리 - 야간 습격 & 철갑 방패무사
                    Enemies.Add(new Enemy(EnemyType.Thug, 240f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 380f, 200f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 520f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.ShieldGuard, 660f, 185f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 780f, 210f, stage));
                    break;

                case 6:
                    // 6장: 육조거리 - 중간보스 배신자 철웅 결전!
                    Enemies.Add(new Enemy(EnemyType.Scholar, 260f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 430f, 195f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 580f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.MidBoss2, 730f, 185f, stage));
                    break;

                case 7:
                    // 7장: 광화문 앞뜰 - 정예 결사대 대공세 (궁수 & 방패무사 협공)
                    Enemies.Add(new Enemy(EnemyType.Ninja, 250f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 390f, 205f, stage));
                    Enemies.Add(new Enemy(EnemyType.Archer, 530f, 160f, stage));
                    Enemies.Add(new Enemy(EnemyType.ShieldGuard, 670f, 190f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 790f, 175f, stage));
                    break;

                case 8:
                    // 8장: 경복궁 근정전 - 궁궐 정예 친위대 결전
                    Enemies.Add(new Enemy(EnemyType.Ninja, 260f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 380f, 200f, stage));
                    Enemies.Add(new Enemy(EnemyType.Archer, 500f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.ShieldGuard, 640f, 205f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 760f, 180f, stage));
                    break;

                case 9:
                    // 9장: 집현전 천추전 - 최종 보스 마도학사 흑호 결전!
                    Enemies.Add(new Enemy(EnemyType.Ninja, 280f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 450f, 200f, stage));
                    Enemies.Add(new Enemy(EnemyType.Boss, 680f, 185f, stage));
                    break;
            }

            // 각 장별 길거리 투척/타격 오브젝트 및 손에 쥐는 무기(장창, 쇠방망이, 부채), 신기전 화차 배치
            switch (stage)
            {
                case 1:
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 270f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 430f, 210f));
                    WorldObjects.Add(new WorldObject(ObjectType.Spear, 480f, 175f)); // 장창
                    WorldObjects.Add(new WorldObject(ObjectType.RiceSack, 540f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.Jar, 650f, 165f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 760f, 195f));
                    break;
                case 2:
                    WorldObjects.Add(new WorldObject(ObjectType.Table, 270f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Barrel, 410f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Cudgel, 480f, 185f)); // 육각 쇠방망이
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 540f, 195f));
                    WorldObjects.Add(new WorldObject(ObjectType.Jar, 670f, 205f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 770f, 180f));
                    break;
                case 3:
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 290f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 440f, 190f));
                    WorldObjects.Add(new WorldObject(ObjectType.Fan, 480f, 180f)); // 선비 쇠살부채
                    WorldObjects.Add(new WorldObject(ObjectType.Barrel, 570f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Jar, 680f, 200f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 770f, 175f));
                    break;
                case 4:
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 280f, 165f));
                    WorldObjects.Add(new WorldObject(ObjectType.Spear, 370f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.RiceSack, 420f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 540f, 210f));
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 660f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 760f, 195f));
                    break;
                case 5:
                    WorldObjects.Add(new WorldObject(ObjectType.Table, 260f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 390f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Cudgel, 450f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.Barrel, 510f, 200f));
                    WorldObjects.Add(new WorldObject(ObjectType.RiceSack, 630f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 750f, 190f));
                    break;
                case 6:
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 290f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Fan, 380f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Table, 440f, 190f));
                    WorldObjects.Add(new WorldObject(ObjectType.Barrel, 580f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 700f, 185f));
                    break;
                case 7:
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 270f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 410f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Hwacha, 480f, 185f)); // 광화문 신기전 화차!
                    WorldObjects.Add(new WorldObject(ObjectType.Spear, 540f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Table, 590f, 195f));
                    WorldObjects.Add(new WorldObject(ObjectType.Jar, 670f, 205f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 770f, 175f));
                    break;
                case 8:
                    WorldObjects.Add(new WorldObject(ObjectType.Table, 280f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Spear, 350f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 420f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Jar, 550f, 195f));
                    WorldObjects.Add(new WorldObject(ObjectType.Cudgel, 620f, 190f));
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 670f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 770f, 205f));
                    break;
                case 9:
                    WorldObjects.Add(new WorldObject(ObjectType.Table, 290f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.Fan, 360f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Cart, 440f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Spear, 510f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Bench, 580f, 195f));
                    WorldObjects.Add(new WorldObject(ObjectType.Hwacha, 640f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.RiceSack, 700f, 170f));
                    break;
            }

            // 플레이어가 들고 온 기물/무기/적 복원
            if (carriedObj != null && !WorldObjects.Contains(carriedObj))
            {
                carriedObj.IsCarried = true;
                carriedObj.IsFlying = false;
                carriedObj.IsRolling = false;
                carriedObj.Vx = 0f;
                carriedObj.Vy = 0f;
                carriedObj.Vz = 0f;
                WorldObjects.Add(carriedObj);
            }
            if (equippedWp != null && !WorldObjects.Contains(equippedWp))
            {
                equippedWp.IsCarried = true;
                equippedWp.IsFlying = false;
                equippedWp.Vx = 0f;
                equippedWp.Vy = 0f;
                equippedWp.Vz = 0f;
                WorldObjects.Add(equippedWp);
            }
            if (carriedEnemy != null && !Enemies.Contains(carriedEnemy))
            {
                carriedEnemy.IsCarried = true;
                carriedEnemy.IsFlying = false;
                carriedEnemy.Vx = 0f;
                carriedEnemy.Vy = 0f;
                carriedEnemy.Vz = 0f;
                Enemies.Add(carriedEnemy);
            }

            InitWeather(stage);
        }

        public void LoadSurvivalWave(int wave, Player? player = null)
        {
            IsSurvivalMode = true;
            SurvivalWave = wave;
            CurrentStage = ((wave - 1) % MaxStage) + 1;
            GameWon = false;

            // 플레이어가 들고 있는 기물, 장착 무기, 포박한 적 보존
            WorldObject? carriedObj = (player != null && player.CarriedObject != null && !player.CarriedObject.IsDead) ? player.CarriedObject : null;
            WorldObject? equippedWp = (player != null && player.EquippedWeapon != null && !player.EquippedWeapon.IsDead) ? player.EquippedWeapon : null;
            Enemy? carriedEnemy = (player != null && player.CarriedEnemy != null && !player.CarriedEnemy.IsDead) ? player.CarriedEnemy : null;

            Enemies.Clear();
            WorldObjects.Clear();

            int enemyCount = 3 + wave;
            var rand = new Random(wave * 7919);

            for (int i = 0; i < enemyCount; i++)
            {
                float x = 200f + (i * 115f) % (StageWidth - 250f);
                float y = 160f + (i % 3) * 20f;
                EnemyType t;
                if (wave % 5 == 0 && i == enemyCount - 1)
                {
                    t = (wave % 10 == 0) ? EnemyType.Boss : EnemyType.MidBoss2;
                }
                else if (wave % 3 == 0 && i == enemyCount - 1)
                {
                    t = EnemyType.MidBoss1;
                }
                else
                {
                    int roll = rand.Next(100);
                    t = roll switch
                    {
                        < 35 => EnemyType.Thug,
                        < 60 => EnemyType.Scholar,
                        < 75 => EnemyType.Ninja,
                        < 90 => EnemyType.Archer,
                        _ => EnemyType.ShieldGuard
                    };
                }
                Enemies.Add(new Enemy(t, x, y, CurrentStage));
            }

            // 무기 및 기물 배치
            WorldObjects.Add(new WorldObject(ObjectType.Spear, 320f, 175f));
            WorldObjects.Add(new WorldObject(ObjectType.Cudgel, 520f, 185f));
            WorldObjects.Add(new WorldObject(ObjectType.Fan, 700f, 180f));
            WorldObjects.Add(new WorldObject(ObjectType.Cart, 420f, 170f));
            if (wave >= 3)
            {
                WorldObjects.Add(new WorldObject(ObjectType.Hwacha, 600f, 185f));
            }

            // 복원
            if (carriedObj != null && !WorldObjects.Contains(carriedObj))
            {
                carriedObj.IsCarried = true;
                carriedObj.IsFlying = false;
                carriedObj.IsRolling = false;
                carriedObj.Vx = 0f;
                carriedObj.Vy = 0f;
                carriedObj.Vz = 0f;
                WorldObjects.Add(carriedObj);
            }
            if (equippedWp != null && !WorldObjects.Contains(equippedWp))
            {
                equippedWp.IsCarried = true;
                equippedWp.IsFlying = false;
                equippedWp.Vx = 0f;
                equippedWp.Vy = 0f;
                equippedWp.Vz = 0f;
                WorldObjects.Add(equippedWp);
            }
            if (carriedEnemy != null && !Enemies.Contains(carriedEnemy))
            {
                carriedEnemy.IsCarried = true;
                carriedEnemy.IsFlying = false;
                carriedEnemy.Vx = 0f;
                carriedEnemy.Vy = 0f;
                carriedEnemy.Vz = 0f;
                Enemies.Add(carriedEnemy);
            }

            InitWeather(CurrentStage);
        }

        // 동적 날씨 및 환경 기믹 상태
        public float WeatherTimer { get; private set; } = 0f;
        public float HazardTimer { get; private set; } = 0f;
        public float HazardWarningTimer { get; private set; } = 0f;
        public float HazardX { get; private set; } = 0f;
        public bool HazardActive { get; private set; } = false;
        private readonly List<(float x, float y, float vx, float vy, float size, Color col)> _weatherParticles = new();

        private void InitWeather(int stage)
        {
            HazardTimer = 0f;
            HazardWarningTimer = 0f;
            HazardActive = false;
            _weatherParticles.Clear();

            var rand = new Random();
            if (stage == 1 || stage == 2)
            {
                // 1, 2장: 흩날리는 가을 단풍잎
                for (int i = 0; i < 35; i++)
                {
                    float px = (float)rand.NextDouble() * PixelBuffer.Width;
                    float py = (float)rand.NextDouble() * PixelBuffer.Height;
                    float vx = 25f + (float)rand.NextDouble() * 35f;
                    float vy = 20f + (float)rand.NextDouble() * 25f;
                    Color col = (i % 2 == 0) ? Color.FromArgb(220, 200, 70, 30) : Color.FromArgb(220, 230, 160, 40);
                    _weatherParticles.Add((px, py, vx, vy, 4f + (float)rand.NextDouble() * 4f, col));
                }
            }
            else if (stage == 3)
            {
                // 3장: 청계천 강물 안개 구름
                for (int i = 0; i < 18; i++)
                {
                    float px = (float)rand.NextDouble() * PixelBuffer.Width;
                    float py = 380f + (float)rand.NextDouble() * 180f;
                    float vx = 12f + (float)rand.NextDouble() * 18f;
                    _weatherParticles.Add((px, py, vx, 0f, 25f + (float)rand.NextDouble() * 20f, Color.FromArgb(50, 210, 225, 235)));
                }
            }
            else if (stage == 4)
            {
                // 4장: 남산 성곽길 황혼 - 은은하게 피어오르는 도깨비불 & 횃불 불씨
                for (int i = 0; i < 32; i++)
                {
                    float px = (float)rand.NextDouble() * PixelBuffer.Width;
                    float py = (float)rand.NextDouble() * PixelBuffer.Height;
                    float vx = 12f + (float)rand.NextDouble() * 22f;
                    float vy = -18f - (float)rand.NextDouble() * 26f;
                    Color col = (i % 3 == 0) ? Color.FromArgb(210, 255, 140, 50) : (i % 3 == 1 ? Color.FromArgb(190, 255, 210, 80) : Color.FromArgb(170, 240, 80, 60));
                    _weatherParticles.Add((px, py, vx, vy, 2.5f + (float)rand.NextDouble() * 3.5f, col));
                }
            }
            else if (stage == 5)
            {
                // 5장: 도성 주막거리 - 밤바람 모래먼지 & 주막 등불 빛무리
                for (int i = 0; i < 36; i++)
                {
                    float px = (float)rand.NextDouble() * PixelBuffer.Width;
                    float py = (float)rand.NextDouble() * PixelBuffer.Height;
                    float vx = -28f - (float)rand.NextDouble() * 32f;
                    float vy = 10f + (float)rand.NextDouble() * 16f;
                    Color col = (i % 2 == 0) ? Color.FromArgb(160, 225, 185, 110) : Color.FromArgb(140, 240, 200, 130);
                    _weatherParticles.Add((px, py, vx, vy, 3f + (float)rand.NextDouble() * 3f, col));
                }
            }
            else if (stage == 6)
            {
                // 6장: 육조거리 - 심야 황금 은행잎 & 서리
                for (int i = 0; i < 38; i++)
                {
                    float px = (float)rand.NextDouble() * PixelBuffer.Width;
                    float py = (float)rand.NextDouble() * PixelBuffer.Height;
                    float vx = 22f + (float)rand.NextDouble() * 28f;
                    float vy = 28f + (float)rand.NextDouble() * 32f;
                    Color col = (i % 2 == 0) ? Color.FromArgb(220, 240, 205, 50) : Color.FromArgb(180, 220, 235, 255);
                    _weatherParticles.Add((px, py, vx, vy, 3.5f + (float)rand.NextDouble() * 3.5f, col));
                }
            }
            else if (stage == 7)
            {
                // 7장: 광화문 앞뜰 - 흩날리는 복사꽃 / 벚꽃잎
                for (int i = 0; i < 42; i++)
                {
                    float px = (float)rand.NextDouble() * PixelBuffer.Width;
                    float py = (float)rand.NextDouble() * PixelBuffer.Height;
                    float vx = 30f + (float)rand.NextDouble() * 35f;
                    float vy = 16f + (float)rand.NextDouble() * 22f;
                    Color col = (i % 2 == 0) ? Color.FromArgb(220, 255, 180, 200) : Color.FromArgb(200, 255, 215, 230);
                    _weatherParticles.Add((px, py, vx, vy, 4f + (float)rand.NextDouble() * 4f, col));
                }
            }
            else if (stage == 8 || stage == 9)
            {
                // 8, 9장: 궁궐 폭풍우 빗줄기
                for (int i = 0; i < 65; i++)
                {
                    float px = (float)rand.NextDouble() * PixelBuffer.Width;
                    float py = (float)rand.NextDouble() * PixelBuffer.Height;
                    float vx = -40f - (float)rand.NextDouble() * 30f;
                    float vy = 280f + (float)rand.NextDouble() * 120f;
                    _weatherParticles.Add((px, py, vx, vy, 2f, Color.FromArgb(140, 180, 210, 255)));
                }
            }
        }

        public void Update(Player player, float dt)
        {
            WeatherTimer += dt;

            // 날씨 파티클 갱신
            for (int p = 0; p < _weatherParticles.Count; p++)
            {
                var pt = _weatherParticles[p];
                pt.x += pt.vx * dt;
                pt.y += pt.vy * dt;
                if (pt.y > PixelBuffer.Height + 20f || pt.y < -20f || pt.x < -40f || pt.x > PixelBuffer.Width + 40f)
                {
                    pt.y = pt.vy < 0 ? PixelBuffer.Height + 10f : -10f;
                    pt.x = (float)new Random().NextDouble() * PixelBuffer.Width;
                }
                _weatherParticles[p] = pt;
            }

            // 7장: 광화문 앞뜰 화살 세례 기믹 (Arrow Rain Hazard)
            if (CurrentStage == 7)
            {
                HazardTimer += dt;
                if (HazardWarningTimer > 0)
                {
                    HazardWarningTimer -= dt;
                    if (HazardWarningTimer <= 0)
                    {
                        HazardActive = true;
                        SoundSynth.PlayWhoosh();
                        float minH = HazardX - 60f;
                        float maxH = HazardX + 60f;
                        if (player.X >= minH && player.X <= maxH && player.InvincibleTimer <= 0 && player.State != "down")
                        {
                            player.TakeDamage(15, 0f);
                        }
                        foreach (var enemy in Enemies)
                        {
                            if (!enemy.IsDead && enemy.State != "down" && enemy.X >= minH && enemy.X <= maxH)
                            {
                                enemy.TakeDamage(20, 0f, out _);
                            }
                        }
                    }
                }
                else if (HazardActive)
                {
                    if (HazardTimer > 1.8f) HazardActive = false;
                }
                else if (HazardTimer > 9.0f)
                {
                    HazardTimer = 0f;
                    HazardWarningTimer = 1.4f;
                    HazardX = Math.Clamp(player.X + (new Random().Next(100) - 50), 120f, StageWidth - 120f);
                }
            }

            // 9장: 천추전 낙뢰 기믹 (Thunder Strike Hazard)
            if (CurrentStage == 9)
            {
                HazardTimer += dt;
                if (HazardWarningTimer > 0)
                {
                    HazardWarningTimer -= dt;
                    if (HazardWarningTimer <= 0)
                    {
                        HazardActive = true;
                        SoundSynth.PlayThunder();
                        float minH = HazardX - 45f;
                        float maxH = HazardX + 45f;
                        if (player.X >= minH && player.X <= maxH && player.InvincibleTimer <= 0 && player.State != "down")
                        {
                            player.TakeDamage(22, 0f);
                        }
                        foreach (var enemy in Enemies)
                        {
                            if (!enemy.IsDead && enemy.State != "down" && enemy.X >= minH && enemy.X <= maxH)
                            {
                                enemy.TakeDamage(30, 0f, out _);
                                enemy.State = "down";
                                enemy.Vz = 180f;
                            }
                        }
                    }
                }
                else if (HazardActive)
                {
                    if (HazardTimer > 0.8f) HazardActive = false;
                }
                else if (HazardTimer > 10.5f)
                {
                    HazardTimer = 0f;
                    HazardWarningTimer = 1.1f;
                    HazardX = Math.Clamp(player.X + (new Random().Next(80) - 40), 100f, StageWidth - 100f);
                }
            }

            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = Enemies[i];
                enemy.UpdateAI(player, dt, WorldObjects);
                enemy.Update(dt);

                if (enemy.IsDead)
                {
                    if (CurrentStage == MaxStage && enemy.Type == EnemyType.Boss)
                    {
                        GameWon = true; // 9장 최종 보스를 쓰러뜨려야 엔딩!
                    }
                    Enemies.RemoveAt(i);
                }
            }

            for (int i = WorldObjects.Count - 1; i >= 0; i--)
            {
                var obj = WorldObjects[i];
                obj.Update(dt);
                if (obj.IsDead)
                {
                    WorldObjects.RemoveAt(i);
                }
            }
        }

        public void RenderBackground(PixelBuffer buffer, float cameraX)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            // 각 장마다 특색 있는 하늘 팔레트 및 천체
            (Color skyTop, Color skyBottom, Color groundCol, Color groundLine) = GetStagePalette(CurrentStage);

            int groundY = 420; // 960x720 해상도에서 지면 시작 Y (140 * 3)

            // 1. 960x720 하늘 수직 그라데이션 채색
            using (var skyBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(0, 0), new Point(0, groundY), skyTop, skyBottom))
            {
                g.FillRectangle(skyBrush, 0, 0, PixelBuffer.Width, groundY);
            }

            // 1-1. 대기 원근법 수평 연무 (Atmospheric Horizon Haze: 지평선과 원경을 융합하는 대기 산란)
            using (var hazeBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(0, groundY - 75), new Point(0, groundY),
                Color.FromArgb(0, skyBottom.R, skyBottom.G, skyBottom.B),
                Color.FromArgb(90, skyBottom.R, skyBottom.G, skyBottom.B)))
            {
                g.FillRectangle(hazeBrush, 0, groundY - 75, PixelBuffer.Width, 75);
            }

            // 2. 장별 특수 천체 및 원경 (달, 해, 먹구름, 번개 등)
            RenderSkyFeatures(buffer, cameraX, CurrentStage, groundY);

            // 3. 960x720 원경 산 및 숲 (패럴랙스 스크롤: 원경 0.15배, 근경 0.3배)
            RenderDistantMountains(buffer, cameraX, CurrentStage, groundY);

            // 4. 960x720 중경 조선 전통 건축물 / 성곽 / 전각 (패럴랙스 스크롤: 0.6배)
            RenderMidgroundArchitecture(buffer, cameraX, CurrentStage, groundY);

            // 5. 960x720 고해상도 바닥 도로 (저잣거리 흙길, 돌다리, 성곽길, 궁궐 박석 등)
            RenderHighResFloor(buffer, cameraX, CurrentStage, groundY, groundCol, groundLine);
        }

        private (Color top, Color bot, Color ground, Color line) GetStagePalette(int stage) => stage switch
        {
            1 => (Color.FromArgb(85, 155, 235), Color.FromArgb(235, 215, 175), Color.FromArgb(195, 160, 115), Color.FromArgb(145, 115, 75)), // 1장: 맑은 낮
            2 => (Color.FromArgb(115, 135, 210), Color.FromArgb(245, 190, 140), Color.FromArgb(185, 150, 110), Color.FromArgb(135, 105, 70)), // 2장: 오후 골목
            3 => (Color.FromArgb(190, 75, 85), Color.FromArgb(255, 165, 105), Color.FromArgb(135, 140, 145), Color.FromArgb(85, 90, 95)),     // 3장: 석양 청계천
            4 => (Color.FromArgb(75, 55, 125), Color.FromArgb(195, 115, 135), Color.FromArgb(155, 145, 135), Color.FromArgb(95, 85, 80)),    // 4장: 황혼 성곽
            5 => (Color.FromArgb(20, 20, 55), Color.FromArgb(55, 40, 90), Color.FromArgb(145, 115, 90), Color.FromArgb(90, 70, 50)),          // 5장: 야간 주막
            6 => (Color.FromArgb(15, 18, 40), Color.FromArgb(35, 45, 80), Color.FromArgb(120, 125, 135), Color.FromArgb(70, 75, 85)),         // 6장: 심야 육조거리
            7 => (Color.FromArgb(45, 25, 55), Color.FromArgb(115, 55, 65), Color.FromArgb(145, 135, 140), Color.FromArgb(90, 80, 85)),        // 7장: 폭풍전야 광화문
            8 => (Color.FromArgb(40, 10, 25), Color.FromArgb(135, 30, 40), Color.FromArgb(125, 95, 95), Color.FromArgb(75, 55, 55)),          // 8장: 붉은 달 궁궐
            9 => (Color.FromArgb(12, 8, 22), Color.FromArgb(60, 20, 75), Color.FromArgb(95, 80, 110), Color.FromArgb(55, 40, 70)),           // 9장: 집현전 최종결전
            _ => (Color.FromArgb(80, 120, 180), Color.FromArgb(200, 180, 150), Color.FromArgb(170, 140, 100), Color.FromArgb(130, 100, 70))
        };

        private Color GetMountainColor(int stage) => stage switch
        {
            1 => Color.FromArgb(95, 135, 175),
            2 => Color.FromArgb(105, 128, 155),
            3 => Color.FromArgb(140, 80, 95),
            4 => Color.FromArgb(75, 55, 100),
            5 => Color.FromArgb(22, 22, 50),
            6 => Color.FromArgb(20, 26, 52),
            7 => Color.FromArgb(40, 25, 48),
            8 => Color.FromArgb(42, 18, 28),
            9 => Color.FromArgb(18, 12, 32),
            _ => Color.FromArgb(70, 100, 140)
        };

        private void RenderSkyFeatures(PixelBuffer buffer, float cameraX, int stage, int groundY)
        {
            var g = buffer.Graphics;

            // 1. 주간/황혼 하늘의 부드러운 층운 및 권운 구름층 (Soft Layered Clouds)
            if (stage <= 4)
            {
                float cloudCam = cameraX * 0.08f * PixelBuffer.Scale;
                int cloudUnit = 360;
                Color cloudCol = (stage <= 2)
                    ? Color.FromArgb(45, 255, 255, 255)
                    : Color.FromArgb(55, 255, 160, 110);

                using (var cloudBrush = new SolidBrush(cloudCol))
                {
                    for (int x = -cloudUnit; x < PixelBuffer.Width + cloudUnit; x += cloudUnit)
                    {
                        int cx = x - (int)(cloudCam % cloudUnit);
                        g.FillEllipse(cloudBrush, cx + 20, 45, 160, 32);
                        g.FillEllipse(cloudBrush, cx + 80, 35, 130, 38);
                        g.FillEllipse(cloudBrush, cx + 180, 60, 140, 28);
                        g.FillEllipse(cloudBrush, cx + 220, 50, 110, 34);
                    }
                }
            }

            // 2. 야간 하늘의 반짝이는 북두칠성과 밤하늘 성단 (Twinkling Starfield)
            if (stage == 5 || stage == 6 || stage == 8 || stage == 9)
            {
                for (int s = 0; s < 45; s++)
                {
                    int starBaseX = (s * 47 + 23) % PixelBuffer.Width;
                    int starBaseY = (s * 31 + 17) % (groundY - 140);
                    float twinkle = (float)(Math.Sin(DateTime.UtcNow.Millisecond * 0.005f + s * 1.7f) * 0.5 + 0.5);
                    int starAlpha = 80 + (int)(twinkle * 160);
                    Color starCol = (stage == 8)
                        ? Color.FromArgb(starAlpha, 255, 180, 190)
                        : Color.FromArgb(starAlpha, 230, 240, 255);

                    using var starBrush = new SolidBrush(starCol);
                    int starSize = (s % 5 == 0) ? 3 : 2;
                    g.FillRectangle(starBrush, starBaseX, starBaseY, starSize, starSize);
                }
            }

            if (stage == 1 || stage == 2)
            {
                // 따스한 태양 & 은은한 다층 방사형 빛무리
                int sunX = 720;
                int sunY = 90;
                using (var outerHalo = new SolidBrush(Color.FromArgb(25, 255, 235, 160)))
                using (var haloBrush = new SolidBrush(Color.FromArgb(50, 255, 245, 180)))
                using (var sunBrush = new SolidBrush(Color.FromArgb(255, 255, 235)))
                {
                    g.FillEllipse(outerHalo, sunX - 90, sunY - 90, 180, 180);
                    g.FillEllipse(haloBrush, sunX - 55, sunY - 55, 110, 110);
                    g.FillEllipse(sunBrush, sunX - 32, sunY - 32, 64, 64);
                }
            }
            else if (stage == 3 || stage == 4)
            {
                // 석양의 거대한 붉은 해 & 황금빛 노을 후광
                int sunX = 660;
                int sunY = 160;
                using (var outerHalo = new SolidBrush(Color.FromArgb(35, 255, 90, 40)))
                using (var haloBrush = new SolidBrush(Color.FromArgb(65, 255, 140, 60)))
                using (var sunBrush = new SolidBrush(Color.FromArgb(255, 110, 60)))
                {
                    g.FillEllipse(outerHalo, sunX - 110, sunY - 110, 220, 220);
                    g.FillEllipse(haloBrush, sunX - 70, sunY - 70, 140, 140);
                    g.FillEllipse(sunBrush, sunX - 42, sunY - 42, 84, 84);
                }
            }
            else if (stage == 5 || stage == 6)
            {
                // 은은한 상현달 & 달의 바다(Maria) 음영 질감
                int moonX = 700;
                int moonY = 85;
                using (var haloBrush = new SolidBrush(Color.FromArgb(30, 255, 245, 200)))
                using (var moonBrush = new SolidBrush(Color.FromArgb(255, 250, 215)))
                using (var craterBrush = new SolidBrush(Color.FromArgb(40, 90, 85, 75)))
                {
                    g.FillEllipse(haloBrush, moonX - 45, moonY - 45, 90, 90);
                    g.FillEllipse(moonBrush, moonX - 25, moonY - 25, 50, 50);
                    g.FillEllipse(craterBrush, moonX - 16, moonY - 14, 14, 12);
                    g.FillEllipse(craterBrush, moonX - 6, moonY + 2, 10, 9);
                    g.FillEllipse(craterBrush, moonX - 18, moonY + 4, 8, 8);
                }
            }
            else if (stage == 8)
            {
                // 붉은 핏빛 보름달 & 불길한 붉은 안개
                int moonX = 680;
                int moonY = 95;
                using (var haloBrush = new SolidBrush(Color.FromArgb(65, 255, 40, 50)))
                using (var moonBrush = new SolidBrush(Color.FromArgb(255, 75, 70)))
                using (var darkBrush = new SolidBrush(Color.FromArgb(65, 80, 10, 15)))
                {
                    g.FillEllipse(haloBrush, moonX - 75, moonY - 75, 150, 150);
                    g.FillEllipse(moonBrush, moonX - 38, moonY - 38, 76, 76);
                    g.FillEllipse(darkBrush, moonX - 22, moonY - 18, 22, 18);
                    g.FillEllipse(darkBrush, moonX - 8, moonY + 5, 16, 14);
                }
            }
            else if (stage == 9)
            {
                // 간헐적 번개 플래시 및 지그재그 번개 줄기
                bool flash = (DateTime.UtcNow.Millisecond < 120);
                if (flash)
                {
                    using (var flashBrush = new SolidBrush(Color.FromArgb(140, 190, 215, 255)))
                    {
                        g.FillRectangle(flashBrush, 0, 0, PixelBuffer.Width, groundY);
                    }
                    using (var boltPen = new Pen(Color.FromArgb(255, 255, 255), 3.5f))
                    using (var subBoltPen = new Pen(Color.FromArgb(210, 230, 255), 1.8f))
                    {
                        g.DrawLine(boltPen, 480, 0, 460, 90);
                        g.DrawLine(boltPen, 460, 90, 510, 170);
                        g.DrawLine(boltPen, 510, 170, 475, 280);
                        g.DrawLine(boltPen, 475, 280, 530, groundY);

                        g.DrawLine(subBoltPen, 460, 90, 430, 150);
                        g.DrawLine(subBoltPen, 510, 170, 555, 240);
                    }
                }
                int moonX = 720;
                int moonY = 100;
                using (var moonBrush = new SolidBrush(Color.FromArgb(215, 195, 255)))
                {
                    g.FillEllipse(moonBrush, moonX - 30, moonY - 30, 60, 60);
                }
            }
        }

        private void RenderDistantMountains(PixelBuffer buffer, float cameraX, int stage, int groundY)
        {
            var g = buffer.Graphics;
            Color mtnColor = GetMountainColor(stage);
            Color farColor = Color.FromArgb(Math.Min(255, mtnColor.R + 28), Math.Min(255, mtnColor.G + 28), Math.Min(255, mtnColor.B + 32));
            Color cliffColor = Color.FromArgb(Math.Max(0, mtnColor.R - 25), Math.Max(0, mtnColor.G - 25), Math.Max(0, mtnColor.B - 25));

            // 1. 원거리 원경 산맥 (패럴랙스: 0.12배 - 능선 타일 인덱스별 봉우리 높이/실루엣 자연 변주)
            float farCam = cameraX * 0.12f * PixelBuffer.Scale;
            int farUnit = 420;
            int firstFar = (int)Math.Floor(farCam / farUnit) - 1;
            int lastFar = (int)Math.Floor((farCam + PixelBuffer.Width) / farUnit) + 1;
            for (int k = firstFar; k <= lastFar; k++)
            {
                int mx = k * farUnit - (int)farCam;
                int v1 = (int)(Math.Sin(k * 1.7 + stage) * 28);
                int v2 = (int)(Math.Cos(k * 2.3 + stage * 0.5) * 32);
                int v3 = (int)(Math.Sin(k * 3.1 + 1.2) * 24);
                Point[] farRidge =
                {
                    new Point(mx, groundY),
                    new Point(mx + 42, groundY - 115 - v1 / 2),
                    new Point(mx + 95, groundY - 178 - v2 / 2),
                    new Point(mx + 145, groundY - 142 - v3 / 2),
                    new Point(mx + 205 + v1 / 2, groundY - 235 - v2), // 주봉 변주
                    new Point(mx + 265, groundY - 182 - v3),
                    new Point(mx + 325 + v2 / 3, groundY - 210 - v1), // 제2봉 변주
                    new Point(mx + 375, groundY - 125 - v3 / 2),
                    new Point(mx + farUnit, groundY)
                };
                using var farBrush = new SolidBrush(farColor);
                g.FillPolygon(farBrush, farRidge);
            }

            // 산골짜기 운무 띠 (Valley Fog / Mountain Mist)
            using (var mistBrush = new SolidBrush(Color.FromArgb(40, 235, 240, 250)))
            {
                g.FillRectangle(mistBrush, 0, groundY - 145, PixelBuffer.Width, 32);
            }

            // 2. 근거리 원경 산맥 (패럴랙스: 0.25배 - 능선별 기암괴석 높이 변주)
            float nearCam = cameraX * 0.25f * PixelBuffer.Scale;
            int nearUnit = 360;
            int firstNear = (int)Math.Floor(nearCam / nearUnit) - 1;
            int lastNear = (int)Math.Floor((nearCam + PixelBuffer.Width) / nearUnit) + 1;
            for (int k = firstNear; k <= lastNear; k++)
            {
                int mx = k * nearUnit - (int)nearCam;
                int nv1 = (int)(Math.Sin(k * 2.1 + stage * 1.3) * 26);
                int nv2 = (int)(Math.Cos(k * 1.5 + stage * 0.7) * 24);
                int peakX = mx + 175 + nv1 / 2;
                int peakY = groundY - 185 - nv2;

                Point[] nearRidge =
                {
                    new Point(mx, groundY),
                    new Point(mx + 40, groundY - 92 - nv1 / 3),
                    new Point(mx + 92, groundY - 152 - nv2 / 2),
                    new Point(mx + 135, groundY - 126 - nv1 / 2),
                    new Point(peakX, peakY),
                    new Point(mx + 230, groundY - 138 + nv1 / 3),
                    new Point(mx + 280, groundY - 172 - nv1 / 2),
                    new Point(mx + 328, groundY - 106),
                    new Point(mx + nearUnit, groundY)
                };
                using var nearBrush = new SolidBrush(mtnColor);
                g.FillPolygon(nearBrush, nearRidge);

                // 화강암 암벽 그늘 음영면 (Cliff Shadow Facet)
                Point[] cliffPts =
                {
                    new Point(peakX, peakY),
                    new Point(mx + 230, groundY - 138 + nv1 / 3),
                    new Point(mx + 205, groundY - 70),
                    new Point(mx + 158, groundY - 110)
                };
                using var cBrush = new SolidBrush(cliffColor);
                g.FillPolygon(cBrush, cliffPts);

                // 산등성이 소나무 숲 텍스처 (Pine Skyline Texture)
                using var pinePen = new Pen(cliffColor, 2f);
                for (int px = mx + 22; px < mx + nearUnit - 22; px += 22)
                {
                    int py = groundY - 102 - (int)(Math.Sin((px - mx) * 0.02f + k) * 55);
                    g.DrawLine(pinePen, px, py, px, py - 6);
                    g.DrawLine(pinePen, px - 3, py - 3, px + 3, py - 3);
                }
            }
        }

        private void RenderMidgroundArchitecture(PixelBuffer buffer, float cameraX, int stage, int groundY)
        {
            JoseonBackgrounds.RenderMidground(buffer, cameraX, stage, groundY);
        }

        private void RenderHighResFloor(PixelBuffer buffer, float cameraX, int stage, int groundY, Color groundCol, Color groundLine)
        {
            var g = buffer.Graphics;

            // 바닥 수직 깊이 그라데이션 (시점 원근감: 전면으로 올수록 짙고 중후한 명암)
            Color darkGround = Color.FromArgb(
                Math.Max(0, groundCol.R - 40),
                Math.Max(0, groundCol.G - 40),
                Math.Max(0, groundCol.B - 40));

            using (var groundBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(0, groundY), new Point(0, PixelBuffer.Height), groundCol, darkGround))
            {
                g.FillRectangle(groundBrush, 0, groundY, PixelBuffer.Width, PixelBuffer.Height - groundY);
            }

            // 지평선 도로 경계석 엣지 라인
            using (var curbPen = new Pen(groundLine, 3.5f))
            using (var highlightPen = new Pen(Color.FromArgb(140, 255, 255, 255), 1.5f))
            {
                g.DrawLine(curbPen, 0, groundY, PixelBuffer.Width, groundY);
                g.DrawLine(highlightPen, 0, groundY + 1.5f, PixelBuffer.Width, groundY + 1.5f);
            }

            if (stage == 1 || stage == 2 || stage == 5)
            {
                // 1, 2, 5장: 저잣거리 흙길 (원근 패럴랙스 차등 스크롤)
                // 1) 지평선 경계 들풀(Wild Grass) 클러스터 (패럴랙스 스크롤: 0.85배)
                float grassCam = cameraX * 0.85f * PixelBuffer.Scale;
                using (var grassDark = new SolidBrush(Color.FromArgb(90, 75, 115, 55)))
                using (var grassLight = new SolidBrush(Color.FromArgb(120, 110, 155, 75)))
                {
                    for (int gx = -40; gx < PixelBuffer.Width + 40; gx += 32)
                    {
                        int posX = gx - (int)(grassCam % 32);
                        float sway = (float)Math.Sin(gx * 0.3f);
                        g.FillPolygon(grassDark, new Point[] {
                            new Point(posX - 4, groundY),
                            new Point(posX + (int)(sway * 3), groundY - 9),
                            new Point(posX + 4, groundY)
                        });
                        g.FillPolygon(grassLight, new Point[] {
                            new Point(posX, groundY),
                            new Point(posX + 3 + (int)(sway * 2), groundY - 12),
                            new Point(posX + 6, groundY)
                        });
                    }
                }

                // 2) 수레바퀴 자국 (깊이 Y = 168f, 212f): 소실점 원근 트랙
                float trackY1 = 168f;
                float trackY2 = 212f;
                float sTrackY1 = trackY1 * PixelBuffer.Scale;
                float sTrackY2 = trackY2 * PixelBuffer.Scale;

                using (var trackPen1 = new Pen(Color.FromArgb(35, groundLine.R, groundLine.G, groundLine.B), 4.5f))
                using (var trackPen2 = new Pen(Color.FromArgb(50, groundLine.R, groundLine.G, groundLine.B), 7.5f))
                using (var rutHighlight = new Pen(Color.FromArgb(25, 255, 255, 255), 1.5f))
                {
                    g.DrawLine(trackPen1, 0, sTrackY1, PixelBuffer.Width, sTrackY1);
                    g.DrawLine(rutHighlight, 0, sTrackY1 - 2, PixelBuffer.Width, sTrackY1 - 2);
                    g.DrawLine(trackPen2, 0, sTrackY2, PixelBuffer.Width, sTrackY2);
                    g.DrawLine(rutHighlight, 0, sTrackY2 - 3, PixelBuffer.Width, sTrackY2 - 3);
                }

                // 수레바퀴 바큇자국 마디 텍스처 (하단 바큇자국이 더 빠르게 스크롤됨!)
                using (var rutPen = new Pen(Color.FromArgb(45, groundLine.R, groundLine.G, groundLine.B), 2f))
                {
                    float rutStep1 = 28f;
                    float minX1 = cameraX - 40f;
                    float maxX1 = cameraX + PixelBuffer.VirtualWidth + 40f;
                    for (float wx = (float)Math.Floor(minX1 / rutStep1) * rutStep1; wx <= maxX1; wx += rutStep1)
                    {
                        float rx = Perspective.ProjectScreenX(wx, cameraX, trackY1);
                        g.DrawLine(rutPen, rx - 3, sTrackY1 - 2, rx + 3, sTrackY1 + 2);
                    }

                    float rutStep2 = 36f;
                    float minX2 = cameraX - 60f;
                    float maxX2 = cameraX + PixelBuffer.VirtualWidth + 60f;
                    for (float wx = (float)Math.Floor(minX2 / rutStep2) * rutStep2; wx <= maxX2; wx += rutStep2)
                    {
                        float rx = Perspective.ProjectScreenX(wx, cameraX, trackY2);
                        g.DrawLine(rutPen, rx - 5, sTrackY2 - 3, rx + 5, sTrackY2 + 3);
                    }
                }

                // 3) 4단계 깊이별 자갈/모래알 (화면 아래쪽 자갈이 더 크고 훨씬 빠르게 이동!)
                (float bandY, float step, int w, int h, int alpha)[] bands =
                {
                    (152f, 45f, 6, 4, 60),   // 원경 자갈: 작고 느리게 스크롤
                    (175f, 60f, 10, 6, 75),  // 중경 자갈: 보통 속도
                    (198f, 75f, 14, 8, 90),  // 근경 자갈: 빠르고 큼
                    (225f, 90f, 18, 10, 110) // 최전면 자갈: 가장 크고 초고속 스크롤
                };

                foreach (var (by, bStep, bw, bh, bAlpha) in bands)
                {
                    float sy = by * PixelBuffer.Scale;
                    float minWx = cameraX - 50f;
                    float maxWx = cameraX + PixelBuffer.VirtualWidth + 50f;
                    using var bandBrush = new SolidBrush(Color.FromArgb(bAlpha, groundLine.R, groundLine.G, groundLine.B));

                    for (float wx = (float)Math.Floor(minWx / bStep) * bStep; wx <= maxWx; wx += bStep)
                    {
                        float seed = (float)Math.Sin(wx * 12.3f + by * 7.7f);
                        float px = Perspective.ProjectScreenX(wx + seed * 8f, cameraX, by);
                        g.FillEllipse(bandBrush, px - bw / 2f, sy - bh / 2f, bw, bh);
                    }
                }
            }
            else
            {
                // 3, 4, 6, 7, 8, 9장: 돌길 및 궁궐 박석 (원근 소실점 방사형 줄눈 & 행별 차등 스크롤)
                using (var stonePen = new Pen(Color.FromArgb(85, groundLine.R, groundLine.G, groundLine.B), 1.8f))
                using (var shadowPen = new Pen(Color.FromArgb(40, 0, 0, 0), 1.2f))
                using (var bevelHighlight = new Pen(Color.FromArgb(35, 255, 255, 255), 1.2f))
                {
                    // 1) 가로 줄눈: 전면으로 올수록 간격이 기하급수적으로 넓어지는 원근 깊이 배열
                    float[] depthRatios = { 0.0f, 0.08f, 0.18f, 0.31f, 0.47f, 0.66f, 0.84f, 1.0f };
                    float totalFloorH = PixelBuffer.Height - groundY; // 300px
                    float[] rowY = new float[depthRatios.Length];

                    for (int i = 0; i < depthRatios.Length; i++)
                    {
                        rowY[i] = groundY + totalFloorH * depthRatios[i];
                        if (i > 0 && i < depthRatios.Length - 1)
                        {
                            g.DrawLine(shadowPen, 0, rowY[i] + 1f, PixelBuffer.Width, rowY[i] + 1f);
                            g.DrawLine(stonePen, 0, rowY[i], PixelBuffer.Width, rowY[i]);
                            g.DrawLine(bevelHighlight, 0, rowY[i] - 1f, PixelBuffer.Width, rowY[i] - 1f);
                        }
                    }

                    // 2) 세로 줄눈: 소실점을 향해 뻗어가는 방사형 라인 및 각 행별 차등 스크롤
                    float tileWidthLogic = 34f; // 가상 좌표 기준 타일 폭
                    float minX = cameraX - 100f;
                    float maxX = cameraX + PixelBuffer.VirtualWidth + 100f;

                    for (int r = 0; r < depthRatios.Length - 1; r++)
                    {
                        float yTop = rowY[r];
                        float yBot = rowY[r + 1];
                        float logicYTop = yTop / PixelBuffer.Scale;
                        float logicYBot = yBot / PixelBuffer.Scale;

                        // 벽돌식 엇갈림(running bond) 패턴
                        float shift = (r % 2 == 1) ? (tileWidthLogic * 0.5f) : 0f;

                        for (float wx = (float)Math.Floor(minX / tileWidthLogic) * tileWidthLogic; wx <= maxX; wx += tileWidthLogic)
                        {
                            float tileWorldX = wx + shift;
                            float sxTop = Perspective.ProjectScreenX(tileWorldX, cameraX, logicYTop);
                            float sxBot = Perspective.ProjectScreenX(tileWorldX, cameraX, logicYBot);

                            // 화면 내에 보이는 선만 렌더링
                            if ((sxTop >= -20 && sxTop <= PixelBuffer.Width + 20) || (sxBot >= -20 && sxBot <= PixelBuffer.Width + 20))
                            {
                                g.DrawLine(stonePen, sxTop, yTop, sxBot, yBot);
                            }
                        }
                    }

                    // 3) 7, 8, 9장 궁궐: 중앙 어도(御道, 삼도 三道) - 임금이 걷는 중앙 특별 화강암 어도 라인 & 박석 요철
                    if (stage >= 7)
                    {
                        float eodoCenterWorldX = StageWidth * 0.5f; // 스테이지 중앙
                        float eodoHalfW = 42f; // 어도 반폭

                        float eodoL_Top = Perspective.ProjectScreenX(eodoCenterWorldX - eodoHalfW, cameraX, 140f);
                        float eodoL_Bot = Perspective.ProjectScreenX(eodoCenterWorldX - eodoHalfW, cameraX, 240f);
                        float eodoR_Top = Perspective.ProjectScreenX(eodoCenterWorldX + eodoHalfW, cameraX, 140f);
                        float eodoR_Bot = Perspective.ProjectScreenX(eodoCenterWorldX + eodoHalfW, cameraX, 240f);

                        // 어도 중앙 장대석 경계선
                        using var eodoBorderPen = new Pen(Color.FromArgb(120, 215, 185, 95), 2.5f);
                        using var eodoFillBrush = new SolidBrush(Color.FromArgb(28, 255, 235, 180));
                        PointF[] eodoPoly =
                        {
                            new PointF(eodoL_Top, groundY),
                            new PointF(eodoR_Top, groundY),
                            new PointF(eodoR_Bot, PixelBuffer.Height),
                            new PointF(eodoL_Bot, PixelBuffer.Height)
                        };
                        g.FillPolygon(eodoFillBrush, eodoPoly);
                        g.DrawLine(eodoBorderPen, eodoL_Top, groundY, eodoL_Bot, PixelBuffer.Height);
                        g.DrawLine(eodoBorderPen, eodoR_Top, groundY, eodoR_Bot, PixelBuffer.Height);

                        // 거친 박석(薄石) 자연 요철 스펙클 (Granite grain flecks)
                        using var fleckBrush = new SolidBrush(Color.FromArgb(30, 20, 15, 25));
                        for (int fl = 0; fl < 30; fl++)
                        {
                            float fwX = (fl * 43 + 17) % StageWidth;
                            float fy = 145f + (fl * 7) % 90;
                            float fx = Perspective.ProjectScreenX(fwX, cameraX, fy);
                            float sy = Perspective.ProjectScreenY(fy, 0f);
                            g.FillRectangle(fleckBrush, fx, sy, 3, 2);
                        }
                    }
                }
            }
        }

        public void RenderWeatherAndHazards(PixelBuffer buffer, float cameraX)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            // 1. 환경 기믹 렌더링 (7장 화살 세례 & 9장 천추전 낙뢰)
            if (HazardWarningTimer > 0)
            {
                float scx = Perspective.ProjectScreenX(HazardX, cameraX, 185f);
                float scy = Perspective.ProjectScreenY(185f, 0f);
                bool blink = ((int)(HazardWarningTimer * 10) % 2 == 0);

                if (CurrentStage == 7)
                {
                    // 7장 화살 낙하 위험 경고 구역
                    float zoneW = 120f * PixelBuffer.Scale;
                    using var warnBrush = new SolidBrush(Color.FromArgb(blink ? 120 : 60, 240, 40, 40));
                    using var warnPen = new Pen(Color.FromArgb(255, 255, 60, 60), 2.5f);
                    g.FillEllipse(warnBrush, scx - zoneW / 2f, scy - 18f, zoneW, 36f);
                    g.DrawEllipse(warnPen, scx - zoneW / 2f, scy - 18f, zoneW, 36f);

                    buffer.DrawOutlinedTextCentered("▲ 화살 세례 경고! ▲", (int)(scy - 45f), Color.FromArgb(255, 230, 80), Color.Black, buffer.FontSubtitle, 2f);
                }
                else if (CurrentStage == 9)
                {
                    // 9장 천추전 낙뢰 룬 서클 경고
                    float zoneW = 90f * PixelBuffer.Scale;
                    using var warnBrush = new SolidBrush(Color.FromArgb(blink ? 140 : 70, 160, 40, 240));
                    using var warnPen = new Pen(Color.FromArgb(255, 140, 230, 255), 3f);
                    g.FillEllipse(warnBrush, scx - zoneW / 2f, scy - 16f, zoneW, 32f);
                    g.DrawEllipse(warnPen, scx - zoneW / 2f, scy - 16f, zoneW, 32f);

                    buffer.DrawOutlinedTextCentered("⚡ 천둥 낙뢰 경고! ⚡", (int)(scy - 45f), Color.FromArgb(160, 240, 255), Color.Black, buffer.FontSubtitle, 2f);
                }
            }

            if (HazardActive)
            {
                float scx = Perspective.ProjectScreenX(HazardX, cameraX, 185f);
                float scy = Perspective.ProjectScreenY(185f, 0f);

                if (CurrentStage == 7)
                {
                    // 화살비 세례 쏟아짐
                    using var arrowPen = new Pen(Color.FromArgb(240, 60, 30), 2.5f);
                    using var flamePen = new Pen(Color.FromArgb(255, 230, 80), 1.5f);
                    for (int a = -4; a <= 4; a++)
                    {
                        float ax = scx + a * 14f;
                        g.DrawLine(arrowPen, ax - 15f, scy - 120f, ax, scy);
                        g.DrawLine(flamePen, ax - 8f, scy - 60f, ax, scy);
                    }
                }
                else if (CurrentStage == 9)
                {
                    // 천추전 거대 벼락 강타 & 화면 섬광
                    buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(60, 200, 240, 255));

                    using var boltOuter = new Pen(Color.FromArgb(200, 90, 180, 255), 18f);
                    using var boltInner = new Pen(Color.White, 7f);
                    g.DrawLine(boltOuter, scx, 0, scx, scy);
                    g.DrawLine(boltInner, scx, 0, scx, scy);

                    // 바닥 충격파 링
                    using var shockPen = new Pen(Color.FromArgb(255, 160, 240, 255), 4f);
                    g.DrawEllipse(shockPen, scx - 65f, scy - 20f, 130f, 40f);
                }
            }

            // 2. 동적 날씨 파티클 렌더링
            foreach (var pt in _weatherParticles)
            {
                if (CurrentStage == 1 || CurrentStage == 2)
                {
                    // 흩날리는 가을 단풍잎
                    using var leafBrush = new SolidBrush(pt.col);
                    g.FillEllipse(leafBrush, pt.x, pt.y, pt.size, pt.size * 0.7f);
                }
                else if (CurrentStage == 3)
                {
                    // 청계천 수면을 덮는 자욱한 안개 구름
                    using var mistBrush = new SolidBrush(pt.col);
                    g.FillEllipse(mistBrush, pt.x - pt.size, pt.y - pt.size * 0.4f, pt.size * 2f, pt.size * 0.8f);
                }
                else if (CurrentStage == 4)
                {
                    // 남산 성곽길 황혼 도깨비불 / 은은한 횃불 불씨
                    using var emberBrush = new SolidBrush(pt.col);
                    g.FillEllipse(emberBrush, pt.x, pt.y, pt.size, pt.size);
                    using var haloBrush = new SolidBrush(Color.FromArgb(40, pt.col.R, pt.col.G, pt.col.B));
                    g.FillEllipse(haloBrush, pt.x - pt.size, pt.y - pt.size, pt.size * 3f, pt.size * 3f);
                }
                else if (CurrentStage == 5)
                {
                    // 도성 주막거리 밤바람 모래먼지 & 주막 등불 빛무리
                    using var dustBrush = new SolidBrush(pt.col);
                    g.FillEllipse(dustBrush, pt.x, pt.y, pt.size * 1.4f, pt.size * 0.8f);
                }
                else if (CurrentStage == 6)
                {
                    // 육조거리 심야 황금 은행잎 & 서리
                    using var ginkgoBrush = new SolidBrush(pt.col);
                    g.FillEllipse(ginkgoBrush, pt.x, pt.y, pt.size, pt.size * 0.75f);
                }
                else if (CurrentStage == 7)
                {
                    // 광화문 앞뜰 흩날리는 연분홍 복사꽃 / 벚꽃잎
                    using var petalBrush = new SolidBrush(pt.col);
                    g.FillEllipse(petalBrush, pt.x, pt.y, pt.size * 1.2f, pt.size * 0.6f);
                }
                else if (CurrentStage == 8 || CurrentStage == 9)
                {
                    // 사선으로 거세게 내리는 궁궐 폭풍우 빗줄기
                    using var rainPen = new Pen(pt.col, 1.5f);
                    g.DrawLine(rainPen, pt.x, pt.y, pt.x - 6f, pt.y + 16f);
                }
            }
        }

        public void RenderStageClearArrow(PixelBuffer buffer)
        {
            if (StageCleared)
            {
                bool blink = ((int)(DateTime.UtcNow.Millisecond / 220) % 2 == 0);
                if (blink)
                {
                    int btnW = 260;
                    int btnH = 50;
                    int x = PixelBuffer.Width - btnW - 35;
                    int y = 90;

                    buffer.FillRoundedRect(x, y, btnW, btnH, 12f, Color.FromArgb(235, 20, 20, 30));
                    buffer.DrawRoundedRect(x, y, btnW, btnH, 12f, Color.FromArgb(255, 215, 60), 2.5f);
                    buffer.DrawOutlinedText("다음 장으로! GO ▶", x + 20, y + 10, Color.FromArgb(255, 240, 80), Color.FromArgb(30, 20, 10), buffer.FontHeader, 2f);
                }
            }
        }
    }
}
