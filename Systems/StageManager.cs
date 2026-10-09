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

            if (stage == 1 || stage == 2)
            {
                // 따스한 태양 & 은은한 방사형 빛무리
                int sunX = 720;
                int sunY = 90;
                using (var haloBrush = new SolidBrush(Color.FromArgb(40, 255, 245, 180)))
                {
                    g.FillEllipse(haloBrush, sunX - 60, sunY - 60, 120, 120);
                }
                using (var sunBrush = new SolidBrush(Color.FromArgb(255, 250, 180)))
                {
                    g.FillEllipse(sunBrush, sunX - 35, sunY - 35, 70, 70);
                }
            }
            else if (stage == 3 || stage == 4)
            {
                // 석양의 거대한 붉은 해 & 황금빛 노을 후광
                int sunX = 660;
                int sunY = 160;
                using (var haloBrush = new SolidBrush(Color.FromArgb(60, 255, 120, 60)))
                {
                    g.FillEllipse(haloBrush, sunX - 80, sunY - 80, 160, 160);
                }
                using (var sunBrush = new SolidBrush(Color.FromArgb(255, 105, 65)))
                {
                    g.FillEllipse(sunBrush, sunX - 45, sunY - 45, 90, 90);
                }
            }
            else if (stage == 5 || stage == 6)
            {
                // 은은한 상현달 & 달무리
                int moonX = 700;
                int moonY = 85;
                using (var haloBrush = new SolidBrush(Color.FromArgb(30, 255, 245, 200)))
                {
                    g.FillEllipse(haloBrush, moonX - 45, moonY - 45, 90, 90);
                }
                using (var moonBrush = new SolidBrush(Color.FromArgb(255, 248, 210)))
                {
                    g.FillEllipse(moonBrush, moonX - 25, moonY - 25, 50, 50);
                }
            }
            else if (stage == 8)
            {
                // 붉은 핏빛 보름달 & 불길한 붉은 안개
                int moonX = 680;
                int moonY = 95;
                using (var haloBrush = new SolidBrush(Color.FromArgb(70, 255, 40, 50)))
                {
                    g.FillEllipse(haloBrush, moonX - 70, moonY - 70, 140, 140);
                }
                using (var moonBrush = new SolidBrush(Color.FromArgb(255, 60, 60)))
                {
                    g.FillEllipse(moonBrush, moonX - 38, moonY - 38, 76, 76);
                }
            }
            else if (stage == 9)
            {
                // 간헐적 번개 플래시 및 번개 줄기
                bool flash = (DateTime.UtcNow.Millisecond < 100);
                if (flash)
                {
                    using (var flashBrush = new SolidBrush(Color.FromArgb(160, 200, 220, 255)))
                    {
                        g.FillRectangle(flashBrush, 0, 0, PixelBuffer.Width, groundY);
                    }
                    using (var boltPen = new Pen(Color.FromArgb(255, 255, 255), 3.5f))
                    {
                        g.DrawLine(boltPen, 480, 0, 460, 90);
                        g.DrawLine(boltPen, 460, 90, 510, 170);
                        g.DrawLine(boltPen, 510, 170, 475, 280);
                        g.DrawLine(boltPen, 475, 280, 530, groundY);
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
            Color farColor = Color.FromArgb(Math.Min(255, mtnColor.R + 25), Math.Min(255, mtnColor.G + 25), Math.Min(255, mtnColor.B + 25));

            // 1. 원거리 원경 산맥 (패럴랙스: 0.15배)
            float farCam = cameraX * 0.15f * PixelBuffer.Scale;
            for (int x = -300; x < PixelBuffer.Width + 300; x += 320)
            {
                int mx = x - (int)(farCam % 320);
                Point[] pts =
                {
                    new Point(mx, groundY),
                    new Point(mx + 160, groundY - 210),
                    new Point(mx + 320, groundY)
                };
                using var brush = new SolidBrush(farColor);
                g.FillPolygon(brush, pts);
            }

            // 2. 근거리 원경 산맥 (패럴랙스: 0.28배)
            float nearCam = cameraX * 0.28f * PixelBuffer.Scale;
            for (int x = -260; x < PixelBuffer.Width + 260; x += 260)
            {
                int mx = x - (int)(nearCam % 260);
                Point[] pts =
                {
                    new Point(mx, groundY),
                    new Point(mx + 130, groundY - 150),
                    new Point(mx + 260, groundY)
                };
                using var brush = new SolidBrush(mtnColor);
                g.FillPolygon(brush, pts);
            }
        }

        private void RenderMidgroundArchitecture(PixelBuffer buffer, float cameraX, int stage, int groundY)
        {
            var g = buffer.Graphics;
            float archCam = cameraX * 0.6f * PixelBuffer.Scale;

            if (stage <= 2)
            {
                // 1~2장: 저잣거리 한옥 지붕 & 주막 기둥
                int unitW = 340;
                for (int i = 0; i < 6; i++)
                {
                    int bx = (int)(i * unitW - (archCam % unitW));
                    int by = groundY - 180;

                    // 벽체 (황토벽)
                    using (var wallBrush = new SolidBrush(Color.FromArgb(230, 218, 195)))
                    {
                        g.FillRectangle(wallBrush, bx + 20, by + 45, unitW - 40, 135);
                    }

                    // 목조 기둥 & 서까래
                    using (var woodBrush = new SolidBrush(Color.FromArgb(115, 65, 25)))
                    {
                        g.FillRectangle(woodBrush, bx + 25, by + 45, 18, 135);
                        g.FillRectangle(woodBrush, bx + unitW - 43, by + 45, 18, 135);
                        g.FillRectangle(woodBrush, bx + 160, by + 45, 18, 135);
                        g.FillRectangle(woodBrush, bx + 20, by + 40, unitW - 40, 12);
                    }

                    // 기와지붕 (전통 곡선 팔작지붕 느낌)
                    Point[] roofPts =
                    {
                        new Point(bx - 15, by + 45),
                        new Point(bx + unitW / 2, by),
                        new Point(bx + unitW + 15, by + 45)
                    };
                    using (var roofBrush = new SolidBrush(Color.FromArgb(45, 50, 62)))
                    {
                        g.FillPolygon(roofBrush, roofPts);
                    }
                    using (var ridgePen = new Pen(Color.FromArgb(65, 70, 85), 3f))
                    {
                        g.DrawLines(ridgePen, roofPts);
                    }

                    // 청사초롱 (홍사/청사)
                    int lanternX = bx + unitW - 25;
                    int lanternY = by + 65;
                    using (var redBrush = new SolidBrush(Color.FromArgb(225, 45, 45)))
                    using (var blueBrush = new SolidBrush(Color.FromArgb(40, 70, 210)))
                    using (var glowBrush = new SolidBrush(Color.FromArgb(50, 255, 180, 50)))
                    {
                        g.FillEllipse(glowBrush, lanternX - 25, lanternY - 10, 50, 60);
                        g.FillRectangle(redBrush, lanternX - 10, lanternY, 20, 18);
                        g.FillRectangle(blueBrush, lanternX - 10, lanternY + 18, 20, 18);
                    }
                }
            }
            else if (stage == 3)
            {
                // 3장: 청계천 돌다리 석축 & 늘어진 수양버들
                int unitW = 280;
                for (int i = 0; i < 6; i++)
                {
                    int px = (int)(i * unitW - (archCam % unitW));
                    int py = groundY - 130;

                    // 돌다리 난간 및 석축
                    using (var stoneBrush = new SolidBrush(Color.FromArgb(145, 150, 158)))
                    using (var capBrush = new SolidBrush(Color.FromArgb(175, 180, 188)))
                    {
                        g.FillRectangle(stoneBrush, px + 20, py + 20, unitW - 40, 25);
                        g.FillRectangle(stoneBrush, px + 30, py, 26, 110);
                        g.FillRectangle(capBrush, px + 24, py - 10, 38, 16);

                        g.FillRectangle(stoneBrush, px + unitW - 56, py, 26, 110);
                        g.FillRectangle(capBrush, px + unitW - 62, py - 10, 38, 16);
                    }

                    // 수양버들 나무
                    using (var leafBrush = new SolidBrush(Color.FromArgb(75, 135, 70)))
                    using (var trunkBrush = new SolidBrush(Color.FromArgb(90, 60, 35)))
                    {
                        g.FillRectangle(trunkBrush, px + 130, py - 60, 16, 150);
                        g.FillEllipse(leafBrush, px + 90, py - 110, 100, 75);
                    }
                }
            }
            else if (stage == 4)
            {
                // 4장: 남산 성곽길 (여장 성벽과 불타는 횃불)
                int unitW = 270;
                for (int i = 0; i < 6; i++)
                {
                    int wx = (int)(i * unitW - (archCam % unitW));
                    int wy = groundY - 150;

                    using (var wallBrush = new SolidBrush(Color.FromArgb(95, 90, 85)))
                    using (var stonePen = new Pen(Color.FromArgb(65, 60, 55), 1.5f))
                    {
                        g.FillRectangle(wallBrush, wx, wy + 30, unitW, 120);

                        // 여장 성벽 요철
                        g.FillRectangle(wallBrush, wx + 15, wy, 80, 35);
                        g.FillRectangle(wallBrush, wx + 145, wy, 80, 35);

                        // 성벽 화강암 벽돌 줄눈
                        for (int lineY = wy + 40; lineY < groundY; lineY += 28)
                        {
                            g.DrawLine(stonePen, wx, lineY, wx + unitW, lineY);
                        }
                    }

                    // 성벽 위 타오르는 횃불
                    int torchX = wx + 115;
                    int torchY = wy + 40;
                    using (var glowBrush = new SolidBrush(Color.FromArgb(60, 255, 140, 30)))
                    using (var flameBrush = new SolidBrush(Color.FromArgb(255, 170, 40)))
                    using (var coreBrush = new SolidBrush(Color.FromArgb(255, 240, 120)))
                    {
                        g.FillEllipse(glowBrush, torchX - 25, torchY - 35, 50, 50);
                        g.FillEllipse(flameBrush, torchX - 10, torchY - 20, 20, 26);
                        g.FillEllipse(coreBrush, torchX - 5, torchY - 15, 10, 16);
                    }
                }
            }
            else if (stage <= 6)
            {
                // 5~6장: 관아 대문 및 웅장한 2층 누각
                int unitW = 380;
                for (int i = 0; i < 5; i++)
                {
                    int kx = (int)(i * unitW - (archCam % unitW));
                    int ky = groundY - 210;

                    // 1층 및 2층 단청 벽체
                    using (var dancheongBrush = new SolidBrush(Color.FromArgb(175, 50, 45)))
                    using (var colBrush = new SolidBrush(Color.FromArgb(115, 25, 25)))
                    {
                        g.FillRectangle(dancheongBrush, kx + 25, ky + 55, unitW - 50, 155);
                        g.FillRectangle(colBrush, kx + 35, ky + 55, 24, 155);
                        g.FillRectangle(colBrush, kx + unitW - 59, ky + 55, 24, 155);
                        g.FillRectangle(colBrush, kx + 175, ky + 55, 24, 155);
                    }

                    // 웅장한 기와지붕
                    Point[] roofPts =
                    {
                        new Point(kx - 20, ky + 55),
                        new Point(kx + unitW / 2, ky),
                        new Point(kx + unitW + 20, ky + 55)
                    };
                    using (var roofBrush = new SolidBrush(Color.FromArgb(30, 35, 48)))
                    using (var roofPen = new Pen(Color.FromArgb(220, 180, 50), 3.5f))
                    {
                        g.FillPolygon(roofBrush, roofPts);
                        g.DrawLines(roofPen, roofPts);
                    }
                }
            }
            else
            {
                // 7~9장: 광화문 홍예문, 근정전 월대 및 집현전 황금 전각
                int unitW = 420;
                for (int i = 0; i < 5; i++)
                {
                    int gx = (int)(i * unitW - (archCam % unitW));
                    int gy = groundY - 230;

                    // 궁궐 단청 붉은 벽
                    using (var wallBrush = new SolidBrush(Color.FromArgb(145, 25, 25)))
                    using (var colBrush = new SolidBrush(Color.FromArgb(95, 12, 12)))
                    {
                        g.FillRectangle(wallBrush, gx + 25, gy + 65, unitW - 50, 165);
                        g.FillRectangle(colBrush, gx + 40, gy + 65, 26, 165);
                        g.FillRectangle(colBrush, gx + unitW - 66, gy + 65, 26, 165);
                    }

                    // 궁궐 다포계 곡선 팔작지붕
                    Point[] roofPts =
                    {
                        new Point(gx - 25, gy + 65),
                        new Point(gx + unitW / 2, gy),
                        new Point(gx + unitW + 25, gy + 65)
                    };
                    using (var roofBrush = new SolidBrush(Color.FromArgb(22, 22, 30)))
                    using (var goldPen = new Pen(Color.FromArgb(245, 200, 60), 4f))
                    {
                        g.FillPolygon(roofBrush, roofPts);
                        g.DrawLines(goldPen, roofPts);
                    }

                    // 중앙 홍예문 (아치형 대문)
                    int archX = gx + unitW / 2 - 45;
                    using (var archBrush = new SolidBrush(Color.FromArgb(15, 12, 18)))
                    {
                        g.FillRectangle(archBrush, archX, gy + 110, 90, 120);
                        g.FillPie(archBrush, archX, gy + 75, 90, 70, 180, 180);
                    }
                }
            }
        }

        private void RenderHighResFloor(PixelBuffer buffer, float cameraX, int stage, int groundY, Color groundCol, Color groundLine)
        {
            var g = buffer.Graphics;

            // 바닥 수직 깊이 그라데이션 (시점 원근감)
            Color darkGround = Color.FromArgb(
                Math.Max(0, groundCol.R - 35),
                Math.Max(0, groundCol.G - 35),
                Math.Max(0, groundCol.B - 35));

            using (var groundBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(0, groundY), new Point(0, PixelBuffer.Height), groundCol, darkGround))
            {
                g.FillRectangle(groundBrush, 0, groundY, PixelBuffer.Width, PixelBuffer.Height - groundY);
            }

            // 도로 경계석 엣지 라인
            using (var curbPen = new Pen(groundLine, 3f))
            using (var highlightPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1.5f))
            {
                g.DrawLine(curbPen, 0, groundY, PixelBuffer.Width, groundY);
                g.DrawLine(highlightPen, 0, groundY + 1.5f, PixelBuffer.Width, groundY + 1.5f);
            }

            float floorCam = cameraX * PixelBuffer.Scale;

            if (stage == 1 || stage == 2 || stage == 5)
            {
                // 저잣거리 흙길 (풍부한 질감의 자갈과 수레바퀴 자국)
                using (var trackPen = new Pen(Color.FromArgb(40, groundLine.R, groundLine.G, groundLine.B), 6f))
                {
                    g.DrawLine(trackPen, 0, groundY + 70, PixelBuffer.Width, groundY + 70);
                    g.DrawLine(trackPen, 0, groundY + 190, PixelBuffer.Width, groundY + 190);
                }

                // 바닥 자갈 디테일
                int step = 75;
                int offset = (int)(floorCam % step);
                using (var pebbleBrush = new SolidBrush(Color.FromArgb(70, groundLine.R, groundLine.G, groundLine.B)))
                {
                    for (int x = -step; x < PixelBuffer.Width + step; x += step)
                    {
                        int px = x - offset;
                        g.FillEllipse(pebbleBrush, px + 20, groundY + 45, 8, 5);
                        g.FillEllipse(pebbleBrush, px + 55, groundY + 120, 11, 7);
                        g.FillEllipse(pebbleBrush, px + 35, groundY + 210, 14, 8);
                    }
                }
            }
            else
            {
                // 돌길 및 궁궐 박석 (원근감이 살아있는 정교한 석재 타일)
                using (var stonePen = new Pen(Color.FromArgb(80, groundLine.R, groundLine.G, groundLine.B), 1.8f))
                {
                    // 가로 줄눈
                    for (int y = groundY + 45; y < PixelBuffer.Height; y += 45)
                    {
                        g.DrawLine(stonePen, 0, y, PixelBuffer.Width, y);
                    }

                    // 세로 엇갈림 줄눈
                    int tileW = 90;
                    int tileOffset = (int)(floorCam % (tileW * 2));
                    int row = 0;
                    for (int y = groundY; y < PixelBuffer.Height; y += 45)
                    {
                        int shift = (row % 2 == 0) ? 0 : tileW;
                        for (int x = -tileW * 2; x < PixelBuffer.Width + tileW * 2; x += tileW)
                        {
                            int sx = x - tileOffset + shift;
                            g.DrawLine(stonePen, sx, y, sx, y + 45);
                        }
                        row++;
                    }
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
                    buffer.DrawOutlinedTextCentered("다음 장으로! GO ▶", y + 10, Color.FromArgb(255, 240, 80), Color.FromArgb(30, 20, 10), buffer.FontHeader, 2f);
                }
            }
        }
    }
}
