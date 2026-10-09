using System;
using System.Collections.Generic;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Entities;

namespace HanlangGame.Systems
{
    public class StageManager
    {
        public const int MaxStage = 9;
        public int CurrentStage { get; set; } = 1;
        public float StageWidth => 860f; // 횡스크롤 길이

        public List<Enemy> Enemies { get; } = new();
        public List<WorldObject> WorldObjects { get; } = new();
        public bool StageCleared => Enemies.Count == 0 && CurrentStage < MaxStage;
        public bool GameWon { get; set; } = false;

        public Enemy? CurrentBoss => Enemies.Find(e => e.IsBossType);

        public string GetStageName(int stage) => stage switch
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

        public void LoadStage(int stage)
        {
            CurrentStage = stage;
            GameWon = false; // 재시작 시 엔딩이 다시 뜨지 않도록 완벽 리셋!
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
                    // 4장: 남산 성곽길 - 암흑 결사대 난입
                    Enemies.Add(new Enemy(EnemyType.Ninja, 260f, 160f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 400f, 190f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 540f, 175f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 680f, 205f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 790f, 180f, stage));
                    break;

                case 5:
                    // 5장: 도성 주막거리 - 야간 습격
                    Enemies.Add(new Enemy(EnemyType.Thug, 240f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 380f, 200f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 520f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 660f, 185f, stage));
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
                    // 7장: 광화문 앞뜰 - 정예 결사대 대공세
                    Enemies.Add(new Enemy(EnemyType.Ninja, 250f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 390f, 205f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 530f, 160f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 670f, 190f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 790f, 175f, stage));
                    break;

                case 8:
                    // 8장: 경복궁 근정전 - 궁궐 침투
                    Enemies.Add(new Enemy(EnemyType.Ninja, 260f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.Scholar, 380f, 200f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 500f, 170f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 640f, 205f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 760f, 180f, stage));
                    break;

                case 9:
                    // 9장: 집현전 천추전 - 최종 보스 마도학사 흑호 결전!
                    Enemies.Add(new Enemy(EnemyType.Ninja, 280f, 165f, stage));
                    Enemies.Add(new Enemy(EnemyType.Ninja, 450f, 200f, stage));
                    Enemies.Add(new Enemy(EnemyType.Boss, 680f, 185f, stage));
                    break;
            }

            // 각 장별 길거리 투척/타격 오브젝트 배치
            switch (stage)
            {
                case 1:
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 290f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 440f, 210f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 610f, 165f));
                    break;
                case 2:
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 280f, 190f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 460f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 610f, 205f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 730f, 180f));
                    break;
                case 3:
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 310f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 490f, 200f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 640f, 170f));
                    break;
                case 4:
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 300f, 165f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 470f, 210f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 620f, 180f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 740f, 195f));
                    break;
                case 5:
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 290f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 450f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 610f, 205f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 710f, 165f));
                    break;
                case 6:
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 310f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 500f, 195f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 660f, 180f));
                    break;
                case 7:
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 290f, 190f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 470f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 610f, 205f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 730f, 175f));
                    break;
                case 8:
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 300f, 175f));
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 480f, 195f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 640f, 165f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 740f, 205f));
                    break;
                case 9:
                    WorldObjects.Add(new WorldObject(ObjectType.Rock, 320f, 185f));
                    WorldObjects.Add(new WorldObject(ObjectType.Club, 510f, 170f));
                    WorldObjects.Add(new WorldObject(ObjectType.Crate, 610f, 200f));
                    break;
            }
        }

        public void Update(Player player, float dt)
        {
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
            // 각 장마다 특색 있는 하늘 팔레트 및 천체
            (Color skyTop, Color skyBottom, Color groundCol, Color groundLine) = GetStagePalette(CurrentStage);

            // 1. 하늘 채색
            buffer.FillRect(0, 0, PixelBuffer.Width, 140, skyTop);

            // 2. 장별 특수 천체 및 원경 (달, 해, 먹구름, 번개 등)
            RenderSkyFeatures(buffer, cameraX, CurrentStage);

            // 3. 원경 산 및 성곽/숲 (패럴랙스 스크롤: 0.2배)
            float mountainCam = cameraX * 0.2f;
            Color mtnColor = GetMountainColor(CurrentStage);
            for (int x = -100; x < PixelBuffer.Width + 100; x += 90)
            {
                int mx = x - (int)(mountainCam % 90);
                Point[] pts = { new Point(mx, 120), new Point(mx + 45, 60), new Point(mx + 90, 120) };
                using var brush = new SolidBrush(mtnColor);
                buffer.Graphics.FillPolygon(brush, pts);
            }

            // 4. 중경 건축물 / 오브젝트 (패럴랙스 스크롤: 0.6배)
            RenderMidgroundArchitecture(buffer, cameraX, CurrentStage);

            // 5. 바닥 도로 (저잣거리 흙길, 돌다리, 성곽 바닥, 궁궐 박석 등)
            buffer.FillRect(0, 140, PixelBuffer.Width, 100, groundCol);
            buffer.DrawLine(0, 140, PixelBuffer.Width, 140, groundLine);

            // 도로 텍스처 디테일
            int groundOffset = (int)(cameraX % 40);
            for (int gx = 0; gx < PixelBuffer.Width + 40; gx += 32)
            {
                int rx = gx - groundOffset;
                buffer.SetPixel(rx + 8, 165, groundLine);
                buffer.SetPixel(rx + 22, 190, groundLine);
                buffer.SetPixel(rx + 14, 215, groundLine);
            }
        }

        private (Color top, Color bot, Color ground, Color line) GetStagePalette(int stage) => stage switch
        {
            1 => (Color.FromArgb(95, 155, 230), Color.FromArgb(225, 205, 160), Color.FromArgb(190, 155, 110), Color.FromArgb(145, 115, 80)), // 1장: 맑은 낮
            2 => (Color.FromArgb(120, 140, 200), Color.FromArgb(235, 185, 130), Color.FromArgb(180, 145, 105), Color.FromArgb(135, 105, 75)), // 2장: 오후 골목
            3 => (Color.FromArgb(180, 80, 90), Color.FromArgb(245, 160, 100), Color.FromArgb(130, 135, 140), Color.FromArgb(90, 95, 100)),     // 3장: 석양 청계천
            4 => (Color.FromArgb(80, 60, 120), Color.FromArgb(190, 110, 130), Color.FromArgb(150, 140, 130), Color.FromArgb(100, 90, 85)),    // 4장: 황혼 성곽
            5 => (Color.FromArgb(25, 25, 60), Color.FromArgb(60, 45, 95), Color.FromArgb(140, 110, 85), Color.FromArgb(95, 75, 55)),          // 5장: 야간 주막
            6 => (Color.FromArgb(18, 20, 45), Color.FromArgb(40, 50, 85), Color.FromArgb(115, 120, 130), Color.FromArgb(75, 80, 90)),         // 6장: 심야 육조거리
            7 => (Color.FromArgb(50, 30, 60), Color.FromArgb(120, 60, 70), Color.FromArgb(140, 130, 135), Color.FromArgb(95, 85, 90)),        // 7장: 폭풍전야 광화문
            8 => (Color.FromArgb(45, 15, 30), Color.FromArgb(140, 35, 45), Color.FromArgb(120, 90, 90), Color.FromArgb(80, 60, 60)),          // 8장: 붉은 달 궁궐
            9 => (Color.FromArgb(15, 10, 25), Color.FromArgb(65, 25, 80), Color.FromArgb(90, 75, 105), Color.FromArgb(60, 45, 75)),           // 9장: 집현전 최종결전
            _ => (Color.FromArgb(80, 120, 180), Color.FromArgb(200, 180, 150), Color.FromArgb(170, 140, 100), Color.FromArgb(130, 100, 70))
        };

        private Color GetMountainColor(int stage) => stage switch
        {
            1 => Color.FromArgb(85, 125, 165),
            2 => Color.FromArgb(95, 120, 145),
            3 => Color.FromArgb(130, 75, 90),
            4 => Color.FromArgb(70, 50, 95),
            5 => Color.FromArgb(20, 20, 45),
            6 => Color.FromArgb(18, 24, 48),
            7 => Color.FromArgb(35, 22, 42),
            8 => Color.FromArgb(38, 15, 25),
            9 => Color.FromArgb(15, 10, 28),
            _ => Color.FromArgb(70, 100, 140)
        };

        private void RenderSkyFeatures(PixelBuffer buffer, float cameraX, int stage)
        {
            if (stage == 1 || stage == 2)
            {
                // 따스한 태양
                using var sunBrush = new SolidBrush(Color.FromArgb(255, 235, 140));
                buffer.Graphics.FillEllipse(sunBrush, 240, 20, 32, 32);
            }
            else if (stage == 3 || stage == 4)
            {
                // 석양 붉은 해
                using var sunBrush = new SolidBrush(Color.FromArgb(255, 110, 80));
                buffer.Graphics.FillEllipse(sunBrush, 220, 45, 36, 36);
            }
            else if (stage == 5 || stage == 6)
            {
                // 은은한 상현달
                using var moonBrush = new SolidBrush(Color.FromArgb(255, 245, 190));
                buffer.Graphics.FillEllipse(moonBrush, 230, 20, 26, 26);
            }
            else if (stage == 8)
            {
                // 붉은 핏빛 보름달
                using var moonBrush = new SolidBrush(Color.FromArgb(255, 75, 75));
                buffer.Graphics.FillEllipse(moonBrush, 220, 22, 38, 38);
            }
            else if (stage == 9)
            {
                // 간헐적 번개 플래시 (0.8초마다 한번씩 번쩍)
                bool flash = (DateTime.UtcNow.Millisecond < 80);
                if (flash)
                {
                    buffer.FillRect(0, 0, PixelBuffer.Width, 140, Color.FromArgb(180, 210, 255));
                }
                using var moonBrush = new SolidBrush(Color.FromArgb(210, 190, 255));
                buffer.Graphics.FillEllipse(moonBrush, 230, 25, 32, 32);
            }
        }

        private void RenderMidgroundArchitecture(PixelBuffer buffer, float cameraX, int stage)
        {
            float buildingCam = cameraX * 0.6f;

            if (stage <= 2)
            {
                // 1~2장: 저잣거리 한옥 및 주막
                for (int i = 0; i < 8; i++)
                {
                    int bx = (int)(i * 120 - (buildingCam % 120));
                    int by = 68;

                    buffer.FillRect(bx, by, 110, 18, Color.FromArgb(45, 50, 60)); // 기와
                    buffer.FillRect(bx + 10, by - 6, 90, 8, Color.FromArgb(35, 40, 50));
                    buffer.FillRect(bx + 6, by + 18, 98, 54, Color.FromArgb(225, 215, 190)); // 벽
                    buffer.FillRect(bx + 10, by + 18, 8, 54, Color.FromArgb(120, 70, 30));  // 기둥
                    buffer.FillRect(bx + 92, by + 18, 8, 54, Color.FromArgb(120, 70, 30));

                    if (i % 2 == 0)
                    {
                        // 청사초롱
                        buffer.FillRect(bx + 102, by + 20, 4, 30, Color.FromArgb(100, 60, 20));
                        buffer.FillRect(bx + 98, by + 26, 12, 16, Color.FromArgb(210, 45, 45));
                        buffer.FillRect(bx + 98, by + 34, 12, 8, Color.FromArgb(45, 65, 190));
                    }
                }
            }
            else if (stage == 3)
            {
                // 3장: 청계천 다리 돌난간 및 버드나무
                for (int i = 0; i < 9; i++)
                {
                    int px = (int)(i * 100 - (buildingCam % 100));
                    // 돌다리 난간 기둥
                    buffer.FillRect(px, 100, 12, 40, Color.FromArgb(145, 150, 155));
                    buffer.FillRect(px - 2, 94, 16, 8, Color.FromArgb(170, 175, 180));
                    buffer.FillRect(px + 12, 108, 88, 8, Color.FromArgb(125, 130, 135));

                    // 늘어진 버드나무
                    buffer.FillRect(px + 45, 50, 8, 50, Color.FromArgb(80, 50, 25));
                    buffer.FillRect(px + 30, 40, 38, 20, Color.FromArgb(60, 120, 60));
                }
            }
            else if (stage == 4)
            {
                // 4장: 남산 성곽길 여장(성벽) 및 횃불
                for (int i = 0; i < 9; i++)
                {
                    int wx = (int)(i * 95 - (buildingCam % 95));
                    buffer.FillRect(wx, 85, 85, 55, Color.FromArgb(90, 85, 80));
                    buffer.FillRect(wx + 5, 75, 30, 12, Color.FromArgb(110, 105, 100)); // 성벽 요철
                    buffer.FillRect(wx + 50, 75, 30, 12, Color.FromArgb(110, 105, 100));

                    // 횃불
                    buffer.FillRect(wx + 42, 90, 4, 25, Color.FromArgb(80, 50, 20));
                    buffer.FillRect(wx + 40, 84, 8, 8, Color.FromArgb(250, 140, 30));
                    buffer.FillRect(wx + 42, 82, 4, 4, Color.FromArgb(255, 230, 80));
                }
            }
            else if (stage <= 6)
            {
                // 5~6장: 관아 대문 및 2층 누각
                for (int i = 0; i < 7; i++)
                {
                    int kx = (int)(i * 135 - (buildingCam % 135));
                    buffer.FillRect(kx, 55, 125, 22, Color.FromArgb(30, 35, 45)); // 큰 누각 지붕
                    buffer.FillRect(kx + 10, 45, 105, 12, Color.FromArgb(20, 25, 35));
                    buffer.FillRect(kx + 8, 77, 110, 63, Color.FromArgb(170, 55, 50)); // 단청 붉은 벽
                    buffer.FillRect(kx + 12, 77, 12, 63, Color.FromArgb(120, 30, 30)); // 굵은 기둥
                    buffer.FillRect(kx + 102, 77, 12, 63, Color.FromArgb(120, 30, 30));
                }
            }
            else
            {
                // 7~9장: 광화문 홍예문, 근정전 월대 및 집현전 황금 전각
                for (int i = 0; i < 6; i++)
                {
                    int gx = (int)(i * 150 - (buildingCam % 150));
                    buffer.FillRect(gx, 45, 140, 26, Color.FromArgb(18, 18, 25)); // 궁궐 처마
                    buffer.FillRect(gx + 12, 35, 116, 12, Color.FromArgb(220, 180, 40)); // 황금 단청 선
                    buffer.FillRect(gx + 6, 71, 128, 69, Color.FromArgb(135, 25, 25));   // 궁궐 붉은 벽
                    buffer.FillRect(gx + 10, 71, 14, 69, Color.FromArgb(90, 15, 15));    // 열주 기둥
                    buffer.FillRect(gx + 116, 71, 14, 69, Color.FromArgb(90, 15, 15));

                    // 아치형 문/창문
                    buffer.FillRect(gx + 48, 85, 44, 55, Color.FromArgb(20, 15, 20));
                }
            }
        }

        public void RenderStageClearArrow(PixelBuffer buffer)
        {
            if (StageCleared)
            {
                bool blink = ((int)(DateTime.UtcNow.Millisecond / 250) % 2 == 0);
                if (blink)
                {
                    int x = PixelBuffer.Width - 116;
                    int y = 50;
                    buffer.FillRect(x - 4, y - 2, 112, 18, Color.FromArgb(210, 20, 20, 25));
                    buffer.DrawRect(x - 4, y - 2, 112, 18, Color.FromArgb(250, 200, 40));
                    buffer.DrawText("다음 장으로! GO ▶", x, y, Color.FromArgb(255, 230, 60), large: false);
                }
            }
        }
    }
}
