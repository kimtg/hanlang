using System;
using System.Collections.Generic;
using System.Drawing;

namespace HanlangGame.Visuals
{
    public enum HeadgearType
    {
        Topknot, // 상투 머리
        Gat      // 갓 쓴 머리
    }

    public static class RetroSprites
    {
        private static readonly Dictionary<string, Bitmap> _cache = new();

        public static Bitmap? GetSprite(string key)
        {
            if (_cache.TryGetValue(key, out var bmp)) return bmp;
            return null;
        }

        public static void Initialize()
        {
            _cache.Clear();

            // 1. 플레이어: 유생 국호 (주인공은 갓 쓴 머리로 고정! 흰 도포, 청색 쾌자, 붉은 띠, 갓, 옆모습 통통한 SD)
            RegisterChibiHeroSet("player", Color.FromArgb(245, 245, 250), Color.FromArgb(40, 85, 190), Color.FromArgb(200, 30, 30), headgear: HeadgearType.Gat);

            // 2. 적 1: 골목 깡패 (상투 머리, 주황색 옷)
            RegisterChibiHeroSet("enemy1", Color.FromArgb(230, 135, 45), Color.FromArgb(175, 60, 20), Color.FromArgb(40, 40, 40), isEnemy: true, headgear: HeadgearType.Topknot);

            // 3. 적 2: 불량 훈도 (갓 쓴 머리, 초록색 도포)
            RegisterChibiHeroSet("enemy2", Color.FromArgb(65, 155, 75), Color.FromArgb(30, 95, 45), Color.FromArgb(140, 100, 30), isEnemy: true, headgear: HeadgearType.Gat);

            // 4. 적 3: 암흑 결사대 (상투 머리, 보라색 무복)
            RegisterChibiHeroSet("enemy3", Color.FromArgb(85, 45, 120), Color.FromArgb(50, 25, 75), Color.FromArgb(190, 40, 40), isEnemy: true, headgear: HeadgearType.Topknot);

            // 5. 중간보스 1: 괴력 건달 마달 (상투 머리, 갈색 옷, 대형)
            RegisterChibiHeroSet("midboss1", Color.FromArgb(160, 85, 45), Color.FromArgb(100, 45, 20), Color.FromArgb(220, 180, 50), isBoss: true, scaleW: 32, scaleH: 36, headgear: HeadgearType.Topknot);

            // 6. 중간보스 2: 무과 배신자 철웅 (갓 쓴 머리, 검붉은 무복)
            RegisterChibiHeroSet("midboss2", Color.FromArgb(140, 35, 35), Color.FromArgb(50, 50, 60), Color.FromArgb(240, 200, 40), isBoss: true, scaleW: 30, scaleH: 35, headgear: HeadgearType.Gat);

            // 7. 최종 보스: 마도학사 흑호 (갓 쓴 머리, 칠흑빛 관복)
            RegisterChibiHeroSet("boss", Color.FromArgb(25, 25, 32), Color.FromArgb(180, 25, 25), Color.FromArgb(255, 215, 0), isBoss: true, scaleW: 32, scaleH: 36, headgear: HeadgearType.Gat);

            // 드롭 아이템
            _cache["item_coin"]   = CreateCoin();
            _cache["item_rice"]   = CreateRiceBall();
            _cache["item_scroll"] = CreateScroll();

            // 주워서 때리거나 던질 수 있는 오브젝트
            _cache["obj_club"]    = CreateClub();     // 몽둥이/목봉
            _cache["obj_crate"]   = CreateCrate();    // 나무 궤짝
            _cache["obj_rock"]    = CreateRock();     // 돌멩이/바위
            _cache["obj_jar"]     = CreateJar();      // 옹기 항아리
            _cache["obj_barrel"]  = CreateBarrel();   // 주막 술통
        }

        private static void RegisterChibiHeroSet(string prefix, Color robeColor, Color vestColor, Color beltColor, bool isEnemy = false, bool isBoss = false, int scaleW = 30, int scaleH = 34, HeadgearType headgear = HeadgearType.Topknot)
        {
            _cache[$"{prefix}_idle_0"] = CreateSideChubbySD(0, "idle", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_idle_1"] = CreateSideChubbySD(1, "idle", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            _cache[$"{prefix}_walk_0"] = CreateSideChubbySD(0, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_walk_1"] = CreateSideChubbySD(1, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_walk_2"] = CreateSideChubbySD(2, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_walk_3"] = CreateSideChubbySD(3, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 4프레임 주먹 지르기 (0: 당기기/준비, 1: 중간찌르기, 2: 최대6px타격/충격파, 3: 회수)
            _cache[$"{prefix}_punch_0"] = CreateSideChubbySD(0, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch_1"] = CreateSideChubbySD(1, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch_2"] = CreateSideChubbySD(2, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch_3"] = CreateSideChubbySD(3, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch"]   = _cache[$"{prefix}_punch_2"];

            // 4프레임 발차기 (0: 무릎들기/준비, 1: 중간뻗기, 2: 최대6px앞차기/풍압, 3: 접기/회수)
            _cache[$"{prefix}_kick_0"]  = CreateSideChubbySD(0, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick_1"]  = CreateSideChubbySD(1, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick_2"]  = CreateSideChubbySD(2, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick_3"]  = CreateSideChubbySD(3, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick"]    = _cache[$"{prefix}_kick_2"];

            _cache[$"{prefix}_jump"]     = CreateSideChubbySD(0, "jump", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_jumpkick"] = CreateSideChubbySD(0, "jumpkick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_hit"]      = CreateSideChubbySD(0, "hit", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 비기 훈민선풍각 4프레임 회전 회오리 및 화려한 광/빛 효과
            _cache[$"{prefix}_spin_0"]  = CreateSideChubbySD(0, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin_1"]  = CreateSideChubbySD(1, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin_2"]  = CreateSideChubbySD(2, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin_3"]  = CreateSideChubbySD(3, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin"]    = _cache[$"{prefix}_spin_0"];

            // 방어(가드) 및 쳐내기(패링) 모션
            _cache[$"{prefix}_guard"]   = CreateSideChubbySD(0, "guard", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_parry"]   = CreateSideChubbySD(1, "guard", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 제2 비기: 용비어천격 솟구침 승룡 어퍼컷 모션
            _cache[$"{prefix}_dragon_0"] = CreateSideChubbySD(0, "dragon", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_dragon_1"] = CreateSideChubbySD(1, "dragon", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_dragon"]   = _cache[$"{prefix}_dragon_1"];

            // 제3 비기: 월인천강포 달빛 태극 파동포 모션
            _cache[$"{prefix}_wave_0"]   = CreateSideChubbySD(0, "wave", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_wave_1"]   = CreateSideChubbySD(1, "wave", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_wave"]     = _cache[$"{prefix}_wave_1"];

            // 물체/적을 들고 있는 모션 (대기, 이동 4프레임, 점프)
            _cache[$"{prefix}_carry"]        = CreateSideChubbySD(0, "carry_idle", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_idle"]   = _cache[$"{prefix}_carry"];
            _cache[$"{prefix}_carry_walk_0"] = CreateSideChubbySD(0, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_walk_1"] = CreateSideChubbySD(1, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_walk_2"] = CreateSideChubbySD(2, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_walk_3"] = CreateSideChubbySD(3, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_jump"]   = CreateSideChubbySD(0, "carry_jump", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 든 것으로 후려치는 모션
            _cache[$"{prefix}_swing"]    = CreateSideChubbySD(0, "swing", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // "캐릭터가 쓰러진 모습은 원래 모습을 그대로 -90도 눕혀서 그린다"
            var baseIdle = _cache[$"{prefix}_idle_0"];
            var downBmp = new Bitmap(baseIdle);
            downBmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            _cache[$"{prefix}_down"] = downBmp;
        }

        private static Bitmap CreateSideChubbySD(int frame, string action, Color robeColor, Color vestColor, Color beltColor, bool isEnemy, bool isBoss, int w, int h, HeadgearType headgear)
        {
            var bmp = new Bitmap(w, h);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            Color skinBase = Color.FromArgb(255, 222, 190);
            Color skinShadow = Color.FromArgb(226, 175, 138);
            Color skinCheek = Color.FromArgb(255, 170, 155); // 통통한 분홍 볼살
            Color hairColor = isBoss ? Color.FromArgb(40, 20, 50) : Color.FromArgb(30, 26, 28);
            Color hairLight = isBoss ? Color.FromArgb(70, 40, 85) : Color.FromArgb(55, 48, 52);
            Color outline = Color.FromArgb(12, 10, 16);
            Color white = Color.White;

            Color robeDark = Color.FromArgb(Math.Max(0, robeColor.R - 45), Math.Max(0, robeColor.G - 45), Math.Max(0, robeColor.B - 45));
            Color robeLight = Color.FromArgb(Math.Min(255, robeColor.R + 35), Math.Min(255, robeColor.G + 35), Math.Min(255, robeColor.B + 35));

            int cx = w / 2;

            // ==========================================
            // 비율 계산: 팔다리를 1.5배로 길게, 3배로 통통하게
            // ==========================================
            int legH = 6; // 1.5배 길어진 다리 (4px -> 6px)
            int legY = h - legH;
            int bodyH = isBoss ? 14 : 13;
            int bodyY = legY - bodyH + 1;
            int bodyW = isBoss ? 18 : 16; // 통통하고 듬직한 몸통
            int bx = cx - (bodyW / 2);

            int headW = 15;
            int headH = 12;
            int headY = bodyY - headH + 2;

            // 모션별 호흡 및 보행 바운스
            int bounce = 0;
            if (action == "idle" || action == "carry" || action == "carry_idle")
            {
                bounce = (frame == 1) ? 1 : 0;
            }
            else if (action == "walk" || action == "carry_walk")
            {
                bounce = (frame == 0 || frame == 2) ? 1 : 0;
            }
            else if (action == "jump" || action == "jumpkick" || action == "carry_jump")
            {
                bounce = -1;
            }
            headY += bounce;

            int hx = cx - 7; // 거북목 교정: 머리/얼굴을 1픽셀 뒤로 이동하여 바르고 꼿꼿한 자세 유지

            using var brOut = new SolidBrush(outline);
            using var brSkin = new SolidBrush(skinBase);
            using var brSkinSh = new SolidBrush(skinShadow);
            using var brCheek = new SolidBrush(skinCheek);
            using var brHair = new SolidBrush(hairColor);
            using var brHairLt = new SolidBrush(hairLight);
            using var brRobe = new SolidBrush(robeColor);
            using var brRobeDk = new SolidBrush(robeDark);
            using var brRobeLt = new SolidBrush(robeLight);
            using var brVest = new SolidBrush(vestColor);
            using var brBelt = new SolidBrush(beltColor);
            using var brWhite = new SolidBrush(white);

            // ==========================================
            // 1. 다리 & 버선발 (2픽셀 더 통통한 9px 너비, 다리 사이 1픽셀 분리선)
            // ==========================================
            int legW = 9; // 2픽셀 더 통통해진 듬직한 다리 (기존 7px -> 9px)

            if (action == "jumpkick")
            {
                // 지탱하는 뒷다리: 9px 너비 기둥
                g.FillRectangle(brOut, bx, legY - 1, legW, legH);
                g.FillRectangle(brRobe, bx + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, bx + 1, legY + 3, legW - 2, 3);

                // 공중으로 뻗은 3배 통통한 앞차기 다리 (돌출 도달거리 6px)
                g.FillRectangle(brOut, bx + bodyW - 3, legY - 3, 11, 7);
                g.FillRectangle(brRobe, bx + bodyW - 2, legY - 2, 4, 5);
                g.FillRectangle(brWhite, bx + bodyW + 2, legY - 2, 6, 5);
            }
            else if (action == "kick")
            {
                // 4프레임 지상 발차기
                // 지탱하는 뒷다리: 9px 너비 기둥
                g.FillRectangle(brOut, bx, legY, legW, legH);
                g.FillRectangle(brRobe, bx + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, bx + 1, legY + 3, legW - 2, 3);

                // 차는 앞다리: 4프레임 역동적 전개
                if (frame == 0)
                {
                    // Frame 0: 무릎 높이 들어올리기 (발차기 시동/충전)
                    g.FillRectangle(brOut, bx + bodyW - 4, legY - 3, 7, 7);
                    g.FillRectangle(brRobe, bx + bodyW - 3, legY - 2, 4, 5);
                    g.FillRectangle(brWhite, bx + bodyW + 1, legY - 1, 2, 4);
                }
                else if (frame == 1)
                {
                    // Frame 1: 중간 뻗기 (3px 전개)
                    g.FillRectangle(brOut, bx + bodyW - 4, legY - 1, 8, 6);
                    g.FillRectangle(brRobe, bx + bodyW - 3, legY, 3, 4);
                    g.FillRectangle(brWhite, bx + bodyW, legY, 4, 4);
                }
                else if (frame == 2)
                {
                    // Frame 2: 최대 6px 도달거리 통통한 앞차기 (풍압 충격파)
                    g.FillRectangle(brOut, bx + bodyW - 3, legY - 1, 10, 6);
                    g.FillRectangle(brRobe, bx + bodyW - 2, legY, 3, 4);
                    g.FillRectangle(brWhite, bx + bodyW + 1, legY, 6, 4);
                    // 타격 풍압선 (White & Cyan wind slash)
                    g.FillRectangle(brWhite, bx + bodyW + 7, legY - 1, 1, 6);
                    using var brCyanWind = new SolidBrush(Color.FromArgb(140, 220, 255));
                    g.FillRectangle(brCyanWind, bx + bodyW + 8, legY, 1, 4);
                }
                else // frame == 3
                {
                    // Frame 3: 타격 후 무릎 접어 회수
                    g.FillRectangle(brOut, bx + bodyW - 4, legY - 2, 7, 7);
                    g.FillRectangle(brRobe, bx + bodyW - 3, legY - 1, 4, 5);
                    g.FillRectangle(brWhite, bx + bodyW, legY - 1, 3, 4);
                }
            }
            else if (action == "punch")
            {
                // 4프레임 주먹 지르기 시 하체 밸런스 및 전진 스텝
                int lunge = (frame == 1) ? 1 : ((frame == 2) ? 2 : 0);
                int backX = bx;
                int frontX = bx + legW - 1 + lunge;

                // 뒷다리 (폭 9px)
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);

                // 앞다리 (폭 9px, lunge)
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);
            }
            else if (action == "spin")
            {
                // 비기 훈민선풍각: 4프레임 회전 회오리 발차기
                if (frame == 0)
                {
                    // Frame 0: 우측 전방으로 호쾌하게 뻗은 회전 선풍각
                    g.FillRectangle(brOut, bx - 2, legY, 7, legH);
                    g.FillRectangle(brRobeDk, bx - 1, legY, 5, 3);
                    g.FillRectangle(brWhite, bx - 1, legY + 3, 5, 3);

                    g.FillRectangle(brOut, bx + bodyW - 2, legY - 2, 10, 6);
                    g.FillRectangle(brRobe, bx + bodyW - 1, legY - 1, 4, 4);
                    g.FillRectangle(brWhite, bx + bodyW + 3, legY - 1, 5, 4);
                }
                else if (frame == 1)
                {
                    // Frame 1: 뒷모습 양방향 회오리 스플릿 킥
                    g.FillRectangle(brOut, bx - 5, legY - 1, 9, 6);
                    g.FillRectangle(brWhite, bx - 4, legY, 4, 4);
                    g.FillRectangle(brRobeDk, bx, legY, 3, 4);

                    g.FillRectangle(brOut, bx + bodyW - 4, legY - 1, 9, 6);
                    g.FillRectangle(brRobe, bx + bodyW - 3, legY, 3, 4);
                    g.FillRectangle(brWhite, bx + bodyW, legY, 5, 4);
                }
                else if (frame == 2)
                {
                    // Frame 2: 좌측 역선회 회전각
                    g.FillRectangle(brOut, bx - 5, legY - 2, 10, 6);
                    g.FillRectangle(brWhite, bx - 4, legY - 1, 5, 4);
                    g.FillRectangle(brRobe, bx + 1, legY - 1, 3, 4);

                    g.FillRectangle(brOut, bx + bodyW - 5, legY, 7, legH);
                    g.FillRectangle(brRobeDk, bx + bodyW - 4, legY, 5, 3);
                    g.FillRectangle(brWhite, bx + bodyW - 4, legY + 3, 5, 3);
                }
                else // frame == 3
                {
                    // Frame 3: 공중 회오리 절정 턱 스핀 (Dual Sweep)
                    g.FillRectangle(brOut, bx - 3, legY - 3, 11, 6);
                    g.FillRectangle(brWhite, bx - 2, legY - 2, 5, 4);
                    g.FillRectangle(brRobe, bx + 3, legY - 2, 4, 4);

                    g.FillRectangle(brOut, bx + bodyW - 5, legY - 2, 10, 6);
                    g.FillRectangle(brRobe, bx + bodyW - 4, legY - 1, 4, 4);
                    g.FillRectangle(brWhite, bx + bodyW, legY - 1, 5, 4);
                }
            }
            else if (action == "jump" || action == "carry_jump")
            {
                // 점프: 웅크린 9px 통통한 발
                g.FillRectangle(brOut, bx, legY - 1, legW, 5);
                g.FillRectangle(brWhite, bx + 1, legY, legW - 2, 3);
                g.FillRectangle(brOut, bx + legW - 1, legY - 1, legW, 5);
                g.FillRectangle(brWhite, bx + legW, legY, legW - 2, 3);
            }
            else if (action == "dragon")
            {
                // 용비어천격 솟구침 도약 다리 (승룡 자세)
                g.FillRectangle(brOut, bx, legY - 2, legW, 6);
                g.FillRectangle(brRobeDk, bx + 1, legY - 1, legW - 2, 3);
                g.FillRectangle(brWhite, bx + 1, legY + 2, legW - 2, 2);
                g.FillRectangle(brOut, bx + legW - 2, legY + 1, legW, 6);
                g.FillRectangle(brRobe, bx + legW - 1, legY + 2, legW - 2, 3);
                g.FillRectangle(brWhite, bx + legW - 1, legY + 5, legW - 2, 2);
            }
            else if (action == "wave")
            {
                // 월인천강포 단단한 기마자세 다리
                int backX = bx - 2;
                int frontX = bx + legW;
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);
            }
            else if (action == "guard")
            {
                // 방어/쳐내기 견고한 수비 보폭
                int backX = bx - 1;
                int frontX = bx + legW - 2;
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);
            }
            else if (action == "walk" || action == "carry_walk")
            {
                // 보행 및 들고 이동 시 4프레임 보행 다리 (다리 사이 1픽셀 외곽선)
                int fOffset = (frame == 0) ? 2 : (frame == 1 ? 1 : (frame == 2 ? -2 : -1));
                int bOffset = -fOffset;

                int backX = bx + bOffset;
                int frontX = bx + legW - 1 + fOffset;

                // 보행 시 다리 틈새 안쪽 도포 그림자
                g.FillRectangle(brRobeDk, bx + 1, legY, bodyW - 2, 2);

                // 뒷다리
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);

                // 앞다리 (1픽셀 외곽선으로 뒷다리와 겹쳐도 항상 1픽셀 구분선 유지)
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);
            }
            else // idle, carry, carry_idle
            {
                // 기본 서기 / 들고 서기: 다리 사이 검은 선이 정확히 1픽셀(frontX가 backX + legW - 1에 맞닿음)
                int backX = bx;
                int frontX = bx + legW - 1; // 1픽셀만 겹쳐서 공유

                // 뒷다리 (폭 9px)
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);

                // 앞다리 (폭 9px, 다리 사이 경계선은 1픽셀!)
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);
            }

            // ==========================================
            // 2. 왼쪽(뒤쪽) 팔 (비기 회전 및 들기 시 뒤편 팔)
            // ==========================================
            if (action == "carry" || action == "carry_idle" || action == "carry_walk" || action == "carry_jump")
            {
                g.FillRectangle(brOut, bx + 1, headY + 1, 6, 7);
                g.FillRectangle(brRobe, bx + 2, headY + 2, 4, 5);
                g.FillRectangle(brSkin, bx + 2, headY - 1, 4, 3);
            }
            else if (action == "guard")
            {
                // 방어 자세: 가슴 앞을 든든하게 받치는 뒷손 가드 팔 (가죽 아대)
                g.FillRectangle(brOut, bx + 2, bodyY + 1, 6, 7);
                g.FillRectangle(brRobe, bx + 3, bodyY + 2, 4, 5);
                g.FillRectangle(brBelt, bx + 3, bodyY + 4, 4, 2); // 손목 아대
                g.FillRectangle(brSkin, bx + 3, bodyY, 4, 3);     // 주먹
            }
            else if (action == "dragon")
            {
                // 용비어천격 솟구침 시 허리춤을 단단히 받치는 뒷손
                g.FillRectangle(brOut, bx - 1, bodyY + 4, 6, 6);
                g.FillRectangle(brRobe, bx, bodyY + 5, 4, 4);
                g.FillRectangle(brSkin, bx + 3, bodyY + 5, 3, 3);
            }
            else if (action == "wave")
            {
                // 월인천강포 전방 장풍 방출 뒷손 손바닥
                g.FillRectangle(brOut, bx + bodyW - 6, bodyY + 2, 7, 7);
                g.FillRectangle(brRobe, bx + bodyW - 5, bodyY + 3, 4, 5);
                g.FillRectangle(brSkin, bx + bodyW - 3, bodyY + 3, 4, 5);
            }
            else if (action == "spin")
            {
                // 회전 비기 시 좌측(뒤편) 팔의 역동적인 4프레임 회전
                int leftArmX = frame switch
                {
                    0 => bx - 4,
                    1 => bx - 6,
                    2 => bx + 2,
                    3 => bx - 5,
                    _ => bx - 4
                };
                int leftArmY = bodyY + ((frame == 1 || frame == 3) ? 1 : 3);
                g.FillRectangle(brOut, leftArmX, leftArmY, 6, 6);
                g.FillRectangle(brRobe, leftArmX + 1, leftArmY + 1, 4, 4);
                g.FillRectangle(brSkin, leftArmX, leftArmY + 1, 3, 4);
            }
            else if (action == "punch")
            {
                // 주먹 지를 때 반대쪽 가드 팔 (4프레임)
                int guardX = (frame == 0) ? (bx + 4) : ((frame == 2) ? (bx - 1) : (bx + 2));
                g.FillRectangle(brOut, guardX, bodyY + 3, 5, 6);
                g.FillRectangle(brSkin, guardX + 1, bodyY + 4, 3, 4);
            }
            else if (action == "kick")
            {
                // 발차기 시 반대편 균형 팔
                int kArmX = (frame == 2) ? (bx - 3) : (bx - 1);
                g.FillRectangle(brOut, kArmX, bodyY + 2, 5, 6);
                g.FillRectangle(brSkin, kArmX, bodyY + 4, 3, 3);
            }

            // ==========================================
            // 3. 몸통 & 도포 & 허리띠 (다리 윗부분을 덮음)
            // ==========================================
            g.FillRectangle(brOut, bx, bodyY, bodyW, bodyH);
            g.FillRectangle(brRobe, bx + 1, bodyY + 1, bodyW - 2, bodyH - 2);
            g.FillRectangle(brRobeDk, bx + 1, bodyY + 1, 3, bodyH - 2); // 등 뒤 그림자
            g.FillRectangle(brRobeLt, bx + bodyW - 4, bodyY + 1, 3, bodyH - 3); // 앞가슴 빛

            // 쾌자 (도포 위 걸치는 조끼)
            g.FillRectangle(brVest, bx + 3, bodyY + 1, bodyW - 6, bodyH - 3);

            // 보스/중간보스/특수 적 고유 장식
            if (isBoss)
            {
                // 최종 보스 흑호: 가슴 황금 호랑이 흉배(Tiger Insignia)
                using var brGoldEmblem = new SolidBrush(Color.FromArgb(255, 215, 60));
                g.FillRectangle(brGoldEmblem, bx + bodyW - 6, bodyY + 3, 4, 4);
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 25, 25)), bx + bodyW - 5, bodyY + 4, 2, 2);
            }
            else if (isEnemy && w >= 30)
            {
                // 중간보스 2 철웅: 강철 어깨 견갑(Pauldrons)
                using var brSteel = new SolidBrush(Color.FromArgb(195, 205, 220));
                g.FillRectangle(brSteel, bx + 1, bodyY, 3, 4);
                g.FillRectangle(brSteel, bx + bodyW - 4, bodyY, 3, 4);
            }

            // 허리띠 및 멋스러운 술띠(Tassel)
            g.FillRectangle(brBelt, bx + 1, bodyY + 8, bodyW - 2, 2);
            g.FillRectangle(brOut, bx + bodyW - 2, bodyY + 8, 2, 3); // 앞쪽 매듭
            g.FillRectangle(brBelt, bx + bodyW - 2, bodyY + 11, 2, 3); // 늘어뜨려진 술띠
            g.FillRectangle(brWhite, bx + bodyW - 2, bodyY + 14, 2, 1);

            // ==========================================
            // 4. 머리 & 얼굴 표현 (몸통 다음에 그려 몸통에 가려지지 않음!)
            // ==========================================
            // 귀 (얼굴 뒤쪽 중앙)
            g.FillRectangle(brOut, hx + 2, headY + 7, 2, 3);
            g.FillRectangle(brSkinSh, hx + 3, headY + 7, 1, 2);

            // 뒤통수 & 두개골 베이스
            g.FillRectangle(brOut, hx, headY + 1, headW, headH);
            g.FillRectangle(brHair, hx + 1, headY + 2, headW - 2, headH - 2);
            g.FillRectangle(brHairLt, hx + 2, headY + 2, 7, 2);

            // 망건 (이마 머리띠)
            g.FillRectangle(brOut, hx + 1, headY + 4, headW - 2, 2);

            // 옆모습 얼굴 피부 & 턱선 (몸통 옷깃 위에 깔끔하게 얹힘!)
            g.FillRectangle(brSkinSh, hx + 4, headY + 5, headW - 4, headH - 4);
            g.FillRectangle(brSkin, hx + 5, headY + 5, headW - 4, headH - 5);

            // 옆모습 통통한 볼살
            g.FillRectangle(brCheek, hx + headW - 3, headY + 8, 4, 3);
            g.FillRectangle(brSkin, hx + headW - 2, headY + 7, 2, 2); // 코

            // 눈 & 표정 (캐릭터별 개성 및 멋진 호걸 눈매)
            if (action == "hit")
            {
                g.FillRectangle(brWhite, hx + headW - 5, headY + 5, 5, 5);
                g.FillRectangle(brOut, hx + headW - 4, headY + 6, 2, 3);
                g.FillRectangle(brOut, hx + headW - 3, headY + 11, 4, 2);
                g.FillRectangle(new SolidBrush(Color.FromArgb(230, 40, 40)), hx + headW - 2, headY + 11, 2, 1);
            }
            else
            {
                if (!isEnemy)
                {
                    // 주인공 유생 한랑: 결의에 찬 날카로운 호걸의 눈매와 눈빛 하이라이트!
                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 2); // 날렵한 무사 눈썹
                    g.FillRectangle(brOut, hx + headW - 5, headY + 7, 4, 3); // 눈
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 7, 2, 2); // 반짝이는 눈빛 하이라이트!
                    g.FillRectangle(brOut, hx + headW - 3, headY + 11, 3, 1); // 다문 입
                }
                else if (isBoss)
                {
                    // 마도학사 흑호: 불길하게 빛나는 붉은 안광(Evil Crimson Eyes)
                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 2);
                    using var brEvilEye = new SolidBrush(Color.FromArgb(255, 50, 50));
                    g.FillRectangle(brEvilEye, hx + headW - 5, headY + 7, 4, 3);
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 7, 1, 2);
                    g.FillRectangle(brOut, hx + headW - 3, headY + 11, 4, 1);
                }
                else
                {
                    // 일반 적: 상투 깡패(흉터), 불량 훈도(수염), 결사대(복면)
                    g.FillRectangle(brOut, hx + headW - 5, headY + 6, 4, 4);
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 6, 1, 2);
                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 1); // 눈썹
                    g.FillRectangle(brOut, hx + headW - 3, headY + 11, 3, 1); // 입

                    if (vestColor.R == 50 && vestColor.G == 25)
                    {
                        // 결사대: 닌자 복면
                        g.FillRectangle(brOut, hx + headW - 4, headY + 9, 5, 3);
                    }
                }
            }

            // ==========================================
            // 5. 헤드기어 (상투 또는 갓 쓴 머리)
            // ==========================================
            if (headgear == HeadgearType.Gat)
            {
                // ------------------------------------------
                // 갓 (조선 선비/학사의 흑립 갓)
                // ------------------------------------------
                Color gatDark = Color.FromArgb(14, 12, 18);
                Color gatBody = Color.FromArgb(28, 25, 34);
                Color gatLight = Color.FromArgb(62, 58, 74);
                Color gatBeadRed = Color.FromArgb(210, 45, 45);
                Color gatBeadGold = Color.FromArgb(235, 195, 60);

                using var brGatDark = new SolidBrush(gatDark);
                using var brGatBody = new SolidBrush(gatBody);
                using var brGatLt = new SolidBrush(gatLight);
                using var brBeadR = new SolidBrush(gatBeadRed);
                using var brBeadG = new SolidBrush(gatBeadGold);

                // 1) 대우 (갓 모자 기둥 / Crown)
                g.FillRectangle(brGatDark, hx + 3, headY - 5, 8, 1);
                g.FillRectangle(brGatLt, hx + 4, headY - 5, 6, 1);

                g.FillRectangle(brGatDark, hx + 2, headY - 4, 10, 7);
                g.FillRectangle(brGatBody, hx + 3, headY - 4, 8, 6);
                g.FillRectangle(brGatLt, hx + 4, headY - 4, 2, 5);
                g.FillRectangle(brGatDark, hx + 8, headY - 4, 2, 6);

                // 2) 양태 (갓의 둥글고 넓은 챙 / Brim)
                int brimX = hx - 4;
                int brimW = headW + 7;
                int brimY = headY + 2;
                g.FillRectangle(brGatDark, brimX, brimY, brimW, 2);
                g.FillRectangle(brGatBody, brimX + 1, brimY, brimW - 2, 1);
                g.FillRectangle(brGatLt, brimX + 3, brimY, 6, 1);

                // 3) 휘날리는 갓끈 (산호/금구슬 장식과 함께 바람에 유려하게 곡선을 그리며 휘날림!)
                g.FillRectangle(brGatDark, hx + 4, headY + 6, 1, 5);
                g.FillRectangle(brBeadR, hx + 4, headY + 11, 2, 2); // 산호 구슬
                g.FillRectangle(brBeadG, hx + 3, headY + 13, 2, 2); // 금구슬
                g.FillRectangle(brGatDark, hx + 2, headY + 15, 1, 3); // 바람에 뒤로 날리는 갓끈
                g.FillRectangle(brBeadR, hx + 1, headY + 18, 2, 2);
            }
            else
            {
                // ------------------------------------------
                // 상투 (전통 상투와 비녀 머리)
                // ------------------------------------------
                g.FillRectangle(brOut, hx + 2, headY - 3, 6, 5);
                g.FillRectangle(brHairLt, hx + 3, headY - 2, 4, 3);
                g.FillRectangle(brOut, hx, headY, 9, 2); // 비녀
                g.FillRectangle(brWhite, hx, headY, 2, 2);
            }

            // ==========================================
            // 6. 오른쪽(앞쪽) 팔 & 주먹
            // ==========================================
            if (action == "punch")
            {
                // 4프레임 타격 도달거리 (frame 0: 준비 0px, frame 1: 중간 3px, frame 2: 최대 6px 타격, frame 3: 회수 2px)
                int punchReach = frame switch
                {
                    0 => 0,
                    1 => 3,
                    2 => 6,
                    3 => 2,
                    _ => 4
                };

                int armW = 4 + punchReach;
                int armX = bx + bodyW - 3;
                g.FillRectangle(brOut, armX, bodyY + 2, armW + 5, 8);
                g.FillRectangle(brRobe, armX + 1, bodyY + 3, Math.Max(2, armW - 2), 6);
                g.FillRectangle(brSkin, armX + armW - 1, bodyY + 3, 5, 6); // 3배 통통한 왕주먹
                g.FillRectangle(brSkinSh, armX + armW + 1, bodyY + 5, 2, 3);
                g.FillRectangle(brWhite, armX + armW - 1, bodyY + 3, 2, 2); // 주먹 하이라이트

                if (frame == 2)
                {
                    // 최대 도달 거리 6px 타격 순간 강렬한 타격 충격파
                    g.FillRectangle(brWhite, armX + armW + 4, bodyY + 2, 1, 7);
                    using var brHitGlow = new SolidBrush(Color.FromArgb(255, 230, 90));
                    g.FillRectangle(brHitGlow, armX + armW + 5, bodyY + 3, 1, 5);
                }
            }
            else if (action == "kick")
            {
                // 발차기 시 앞쪽 팔의 균형 동작 (4프레임)
                int armX = frame switch
                {
                    0 => bx + 4,          // 시동: 가드 올림
                    1 => bx + 6,          // 전개
                    2 => bx + bodyW - 2,  // 풀 킥: 앞손 전개
                    3 => bx + 5,          // 회수
                    _ => bx + 5
                };
                g.FillRectangle(brOut, armX, bodyY + 2, 6, 7);
                g.FillRectangle(brRobe, armX + 1, bodyY + 3, 4, 5);
                g.FillRectangle(brSkin, armX + 2, bodyY + 5, 3, 3);
            }
            else if (action == "carry" || action == "carry_idle" || action == "carry_walk" || action == "carry_jump")
            {
                // 물체/적을 머리 위로 든 오른쪽 팔
                g.FillRectangle(brOut, bx + bodyW - 7, headY + 1, 6, 7);
                g.FillRectangle(brRobe, bx + bodyW - 6, headY + 2, 4, 5);
                g.FillRectangle(brSkin, bx + bodyW - 6, headY - 1, 4, 3);
            }
            else if (action == "swing")
            {
                // 든 물건/적을 내리치는 도톰한 스윙 팔 (도달거리 6px)
                g.FillRectangle(brOut, bx + bodyW - 3, bodyY + 2, 9, 8);
                g.FillRectangle(brRobe, bx + bodyW - 2, bodyY + 3, 3, 6);
                g.FillRectangle(brSkin, bx + bodyW + 1, bodyY + 4, 5, 5);
            }
            else if (action == "spin")
            {
                // 회전 비기: 프레임별 호쾌한 팔 동작 및 내장 광선 효과!
                int rArmX = frame switch
                {
                    0 => bx + bodyW - 2, // 우측 전방으로 호쾌하게 뻗음
                    1 => bx + bodyW,     // 회전 반경 최대로 펼친 팔
                    2 => bx + bodyW - 4, // 역방향 선회
                    3 => bx + bodyW - 1, // 천상 선풍 절정
                    _ => bx + bodyW - 2
                };
                int rArmY = bodyY + ((frame == 1 || frame == 3) ? 1 : 3);
                g.FillRectangle(brOut, rArmX, rArmY, 6, 6);
                g.FillRectangle(brRobe, rArmX + 1, rArmY + 1, 4, 4);
                g.FillRectangle(brSkin, rArmX + 2, rArmY + 2, 4, 4);

                // ==========================================
                // 훈민선풍각 비기 고유의 화려한 선풍 광륜 (빛/광 효과)
                // ==========================================
                using var brGold = new SolidBrush(Color.FromArgb(255, 235, 70));
                using var brCyan = new SolidBrush(Color.FromArgb(80, 220, 255));
                using var brWhiteLt = new SolidBrush(Color.White);

                if (frame == 0)
                {
                    // 우측 전방으로 휘감기는 황금빛 회오리 광륜
                    g.FillRectangle(brGold, bx + bodyW + 4, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brCyan, bx + bodyW + 2, bodyY - 2, 3, 2);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 5, bodyY + 2, 1, 6);
                    g.FillRectangle(brCyan, bx - 3, legY + 2, 2, 3);
                }
                else if (frame == 1)
                {
                    // 양옆으로 퍼져나가는 청백색 쌍두 선풍 광익 (Cyclone Wings)
                    g.FillRectangle(brCyan, bx - 6, bodyY + 2, 2, bodyH);
                    g.FillRectangle(brWhiteLt, bx - 5, bodyY + 4, 1, 6);
                    g.FillRectangle(brGold, bx + bodyW + 4, bodyY + 2, 2, bodyH);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 4, bodyY + 4, 1, 6);
                    // 상하 에너지 고리
                    g.FillRectangle(brCyan, bx + 2, bodyY - 3, bodyW - 4, 2);
                    g.FillRectangle(brGold, bx + 1, legY + 5, bodyW - 2, 2);
                }
                else if (frame == 2)
                {
                    // 좌측으로 휘몰아치는 역풍 황금/하늘빛 광륜
                    g.FillRectangle(brCyan, bx - 6, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brGold, bx - 4, bodyY - 2, 3, 2);
                    g.FillRectangle(brWhiteLt, bx - 5, bodyY + 2, 1, 6);
                    g.FillRectangle(brGold, bx + bodyW + 2, legY + 2, 2, 3);
                }
                else if (frame == 3)
                {
                    // 절정: 전방위를 감싸는 천상 훈민 선풍 광환 (Full Halo Storm)
                    g.FillRectangle(brGold, bx - 5, bodyY - 3, bodyW + 10, 2);
                    g.FillRectangle(brWhiteLt, bx - 2, bodyY - 2, bodyW + 4, 1);
                    g.FillRectangle(brCyan, bx - 6, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brCyan, bx + bodyW + 4, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brGold, bx - 5, legY + 5, bodyW + 10, 2);
                    g.FillRectangle(brWhiteLt, bx - 2, legY + 5, bodyW + 4, 1);
                    // 대각선 광선 스파크
                    g.FillRectangle(brWhiteLt, bx - 4, bodyY - 1, 2, 2);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 2, bodyY - 1, 2, 2);
                    g.FillRectangle(brWhiteLt, bx - 4, legY + 3, 2, 2);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 2, legY + 3, 2, 2);
                }
            }
            else if (action == "guard")
            {
                // 방어/쳐내기: 가슴과 얼굴 앞을 단단히 가로막는 앞손 가드 (철벽 방어)
                g.FillRectangle(brOut, bx + bodyW - 5, bodyY - 1, 7, 9);
                g.FillRectangle(brRobe, bx + bodyW - 4, bodyY, 5, 7);
                g.FillRectangle(brBelt, bx + bodyW - 4, bodyY + 3, 5, 2); // 튼튼한 가죽 아대
                g.FillRectangle(brSkin, bx + bodyW - 4, bodyY - 1, 5, 4); // 주먹
                g.FillRectangle(brWhite, bx + bodyW - 3, bodyY, 2, 2);
            }
            else if (action == "dragon")
            {
                // 용비어천격: 하늘을 찌르는 수직 승룡 어퍼컷 주먹!
                int dArmX = bx + bodyW - 4;
                int dArmY = headY - 6; // 머리 위로 높이 솟구친 주먹
                g.FillRectangle(brOut, dArmX, dArmY, 7, 14);
                g.FillRectangle(brRobe, dArmX + 1, dArmY + 5, 5, 8);
                g.FillRectangle(brSkin, dArmX + 1, dArmY, 5, 6);
                g.FillRectangle(brWhite, dArmX + 2, dArmY + 1, 2, 2); // 주먹 광채
                // 승룡 황금빛 궤적선
                using var brGoldGlow = new SolidBrush(Color.FromArgb(255, 220, 60));
                g.FillRectangle(brGoldGlow, dArmX - 1, dArmY + 1, 1, 12);
                g.FillRectangle(brGoldGlow, dArmX + 7, dArmY + 1, 1, 12);
            }
            else if (action == "wave")
            {
                // 월인천강포: 전방을 향해 뻗은 강력한 기공 파동 손바닥!
                int wArmX = bx + bodyW - 2;
                int wArmY = bodyY;
                g.FillRectangle(brOut, wArmX, wArmY, 9, 8);
                g.FillRectangle(brRobe, wArmX + 1, wArmY + 1, 4, 6);
                g.FillRectangle(brSkin, wArmX + 4, wArmY, 5, 8);
                // 손바닥 중심의 태극 기운 광채
                using var brCyanOrb = new SolidBrush(Color.FromArgb(140, 230, 255));
                g.FillEllipse(brCyanOrb, wArmX + 7, wArmY + 1, 5, 6);
            }
            else
            {
                // 기본 대기 및 보행: 1.5배 길고(7px) 3배 통통한(7px) 듬직한 앞쪽 팔뚝
                int armOffset = (action == "walk") ? ((frame == 0 || frame == 1) ? 2 : -2) : 0;
                g.FillRectangle(brOut, bx + 5 + armOffset, bodyY + 2, 7, 7);
                g.FillRectangle(brRobe, bx + 6 + armOffset, bodyY + 3, 5, 4);
                g.FillRectangle(brSkin, bx + 6 + armOffset, bodyY + 6, 5, 3); // 통통한 주먹
                g.FillRectangle(brSkinSh, bx + 8 + armOffset, bodyY + 7, 2, 2);
            }

            return bmp;
        }

        // ==========================================
        // 무기 및 들 수 있는 길거리 오브젝트 스프라이트
        // ==========================================

        private static Bitmap CreateClub()
        {
            var bmp = new Bitmap(20, 10);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(60, 35, 15));
            using var bWood = new SolidBrush(Color.FromArgb(170, 110, 50));
            using var bWoodLt = new SolidBrush(Color.FromArgb(210, 150, 80));

            // 가로로 놓인 묵직한 몽둥이/목봉
            g.FillRectangle(bOut, 1, 3, 18, 5);
            g.FillRectangle(bWood, 2, 4, 16, 3);
            g.FillRectangle(bWoodLt, 10, 4, 7, 2); // 타격부
            return bmp;
        }

        private static Bitmap CreateCrate()
        {
            var bmp = new Bitmap(18, 16);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(50, 30, 15));
            using var bWood = new SolidBrush(Color.FromArgb(160, 105, 55));
            using var bWoodDk = new SolidBrush(Color.FromArgb(120, 70, 30));

            // 나무 궤짝 (X자 띠가 있는 상자)
            g.FillRectangle(bOut, 1, 1, 16, 14);
            g.FillRectangle(bWood, 2, 2, 14, 12);
            g.DrawLine(new Pen(bWoodDk, 2), 2, 2, 15, 13);
            g.DrawLine(new Pen(bWoodDk, 2), 15, 2, 2, 13);
            return bmp;
        }

        private static Bitmap CreateRock()
        {
            var bmp = new Bitmap(16, 14);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(40, 40, 45));
            using var bStone = new SolidBrush(Color.FromArgb(130, 135, 140));
            using var bStoneLt = new SolidBrush(Color.FromArgb(170, 175, 180));

            // 둥글고 단단한 돌멩이/바위
            g.FillEllipse(bOut, 1, 1, 14, 12);
            g.FillEllipse(bStone, 2, 2, 12, 10);
            g.FillRectangle(bStoneLt, 4, 3, 4, 3);
            return bmp;
        }

        private static Bitmap CreateCoin()
        {
            var bmp = new Bitmap(10, 10);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOutline = new SolidBrush(Color.FromArgb(120, 80, 20));
            using var bGold = new SolidBrush(Color.FromArgb(250, 205, 50));
            using var bLight = new SolidBrush(Color.FromArgb(255, 240, 150));

            g.FillEllipse(bOutline, 0, 0, 10, 10);
            g.FillEllipse(bGold, 1, 1, 8, 8);
            g.FillRectangle(bLight, 2, 2, 2, 2);
            g.FillRectangle(bOutline, 4, 4, 2, 2);
            return bmp;
        }

        private static Bitmap CreateRiceBall()
        {
            var bmp = new Bitmap(12, 12);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(40, 40, 40));
            using var bRice = new SolidBrush(Color.FromArgb(250, 250, 250));
            using var bSeaweed = new SolidBrush(Color.FromArgb(25, 35, 25));

            Point[] pts = { new Point(6, 1), new Point(1, 10), new Point(11, 10) };
            g.FillPolygon(bOut, pts);
            Point[] ptsInner = { new Point(6, 2), new Point(2, 9), new Point(10, 9) };
            g.FillPolygon(bRice, ptsInner);
            g.FillRectangle(bSeaweed, 4, 6, 4, 4);
            return bmp;
        }

        private static Bitmap CreateScroll()
        {
            var bmp = new Bitmap(14, 12);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(90, 50, 20));
            using var bPaper = new SolidBrush(Color.FromArgb(250, 240, 205));
            using var bRed = new SolidBrush(Color.FromArgb(200, 30, 30));

            g.FillRectangle(bOut, 1, 1, 12, 10);
            g.FillRectangle(bPaper, 2, 2, 10, 8);
            g.FillRectangle(bRed, 4, 4, 6, 4);
            return bmp;
        }

        private static Bitmap CreateJar()
        {
            var bmp = new Bitmap(18, 18);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(40, 22, 12));
            using var bClay = new SolidBrush(Color.FromArgb(95, 52, 28));
            using var bClayDk = new SolidBrush(Color.FromArgb(68, 36, 18));
            using var bClayLt = new SolidBrush(Color.FromArgb(145, 82, 45));
            using var bSheen = new SolidBrush(Color.FromArgb(200, 140, 90));

            // 전통 옹기 항아리 (배가 부르고 뚜껑이 있는 질그릇)
            g.FillRectangle(bOut, 7, 0, 4, 3);
            g.FillRectangle(bClayLt, 8, 0, 2, 2);
            g.FillRectangle(bOut, 4, 2, 10, 3);
            g.FillRectangle(bClay, 5, 3, 8, 2);

            g.FillEllipse(bOut, 1, 4, 16, 13);
            g.FillEllipse(bClay, 2, 5, 14, 11);
            g.FillEllipse(bClayDk, 2, 5, 5, 11);
            g.FillEllipse(bClayLt, 7, 6, 7, 8);
            g.FillRectangle(bSheen, 10, 7, 2, 4);

            g.FillRectangle(bOut, 4, 16, 10, 2);
            g.FillRectangle(bClayDk, 5, 16, 8, 1);
            return bmp;
        }

        private static Bitmap CreateBarrel()
        {
            var bmp = new Bitmap(18, 16);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(35, 20, 10));
            using var bWood = new SolidBrush(Color.FromArgb(145, 95, 50));
            using var bWoodDk = new SolidBrush(Color.FromArgb(105, 65, 30));
            using var bWoodLt = new SolidBrush(Color.FromArgb(185, 125, 70));
            using var bIron = new SolidBrush(Color.FromArgb(65, 70, 75));
            using var bIronLt = new SolidBrush(Color.FromArgb(120, 130, 140));

            // 주막 나무 술통 (철제 띠와 나무 결)
            g.FillRectangle(bOut, 1, 1, 16, 14);
            g.FillRectangle(bWood, 2, 2, 14, 12);
            g.FillRectangle(bWoodDk, 2, 2, 4, 12);
            g.FillRectangle(bWoodLt, 8, 2, 6, 12);

            g.FillRectangle(bIron, 1, 4, 16, 2);
            g.FillRectangle(bIronLt, 6, 4, 4, 1);
            g.FillRectangle(bIron, 1, 10, 16, 2);
            g.FillRectangle(bIronLt, 6, 10, 4, 1);

            g.FillRectangle(bOut, 8, 7, 3, 3);
            g.FillRectangle(bWoodDk, 9, 8, 1, 1);
            return bmp;
        }
    }
}
