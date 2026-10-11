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

            // 1. 플레이어: 유생 한랑 (주인공은 갓 쓴 머리로 고정! 흰 도포, 청색 쾌자, 붉은 띠, 갓, 옆모습 통통한 SD)
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

            // 8. 신규 적: 흑호단 궁수 (상투 머리, 암녹색 무복)
            RegisterChibiHeroSet("archer", Color.FromArgb(45, 95, 60), Color.FromArgb(30, 60, 40), Color.FromArgb(180, 140, 30), isEnemy: true, headgear: HeadgearType.Topknot);

            // 9. 신규 적: 철갑 방패무사 (상투 머리, 묵직한 강철 갑주)
            RegisterChibiHeroSet("shieldguard", Color.FromArgb(85, 90, 100), Color.FromArgb(55, 60, 70), Color.FromArgb(200, 150, 40), isEnemy: true, scaleW: 32, scaleH: 35, headgear: HeadgearType.Topknot);

            // 드롭 아이템
            _cache["item_coin"]   = CreateCoin();
            _cache["item_rice"]   = CreateRiceBall();
            _cache["item_scroll"] = CreateScroll();

            // 주워서 때리거나 던질 수 있는 오브젝트
            _cache["obj_club"]     = CreateClub();      // 몽둥이/목봉
            _cache["obj_crate"]    = CreateCrate();     // 나무 궤짝
            _cache["obj_rock"]     = CreateRock();      // 돌멩이/바위
            _cache["obj_jar"]      = CreateJar();       // 옹기 항아리
            _cache["obj_barrel"]   = CreateBarrel();    // 주막 술통
            _cache["obj_cart"]     = CreateCart();      // 대형 달구지 수레
            _cache["obj_table"]    = CreateTable();     // 대형 주막 탁자 / 서당 책상
            _cache["obj_bench"]    = CreateBench();     // 대형 주막 평상 / 긴 의자
            _cache["obj_ricesack"] = CreateRiceSack();  // 볏짚 쌀가마니
            _cache["obj_arrow"]    = CreateArrow();     // 흑호단 궁수 화살
            _cache["obj_spear"]    = CreateSpear();     // 오랏줄 장창
            _cache["obj_cudgel"]   = CreateCudgel();    // 육각 쇠방망이
            _cache["obj_fan"]      = CreateFan();       // 선비 쇠살 부채
            _cache["obj_hwacha"]   = CreateHwacha();    // 광화문 신기전 화차
        }

        private static void RegisterChibiHeroSet(string prefix, Color robeColor, Color vestColor, Color beltColor, bool isEnemy = false, bool isBoss = false, int scaleW = 30, int scaleH = 34, HeadgearType headgear = HeadgearType.Topknot)
        {
            _cache[$"{prefix}_idle_0"] = CreateSideChubbySD(prefix, 0, "idle", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_idle_1"] = CreateSideChubbySD(prefix, 1, "idle", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            _cache[$"{prefix}_walk_0"] = CreateSideChubbySD(prefix, 0, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_walk_1"] = CreateSideChubbySD(prefix, 1, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_walk_2"] = CreateSideChubbySD(prefix, 2, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_walk_3"] = CreateSideChubbySD(prefix, 3, "walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 4프레임 주먹 지르기 (0: 당기기/준비, 1: 중간찌르기, 2: 최대6px타격/충격파, 3: 회수)
            _cache[$"{prefix}_punch_0"] = CreateSideChubbySD(prefix, 0, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch_1"] = CreateSideChubbySD(prefix, 1, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch_2"] = CreateSideChubbySD(prefix, 2, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch_3"] = CreateSideChubbySD(prefix, 3, "punch", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_punch"]   = _cache[$"{prefix}_punch_2"];

            // 4프레임 발차기 (0: 무릎들기/준비, 1: 중간뻗기, 2: 최대6px앞차기/풍압, 3: 접기/회수)
            _cache[$"{prefix}_kick_0"]  = CreateSideChubbySD(prefix, 0, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick_1"]  = CreateSideChubbySD(prefix, 1, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick_2"]  = CreateSideChubbySD(prefix, 2, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick_3"]  = CreateSideChubbySD(prefix, 3, "kick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_kick"]    = _cache[$"{prefix}_kick_2"];

            _cache[$"{prefix}_jump"]     = CreateSideChubbySD(prefix, 0, "jump", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_jumpkick"] = CreateSideChubbySD(prefix, 0, "jumpkick", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_hit"]      = CreateSideChubbySD(prefix, 0, "hit", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 비기 훈민선풍각 4프레임 회전 회오리 및 화려한 광/빛 효과
            _cache[$"{prefix}_spin_0"]  = CreateSideChubbySD(prefix, 0, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin_1"]  = CreateSideChubbySD(prefix, 1, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin_2"]  = CreateSideChubbySD(prefix, 2, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin_3"]  = CreateSideChubbySD(prefix, 3, "spin", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_spin"]    = _cache[$"{prefix}_spin_0"];

            // 방어(가드) 및 쳐내기(패링) 모션
            _cache[$"{prefix}_guard"]   = CreateSideChubbySD(prefix, 0, "guard", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_parry"]   = CreateSideChubbySD(prefix, 1, "guard", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 제2 비기: 용비어천격 솟구침 승룡 어퍼컷 모션
            _cache[$"{prefix}_dragon_0"] = CreateSideChubbySD(prefix, 0, "dragon", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_dragon_1"] = CreateSideChubbySD(prefix, 1, "dragon", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_dragon"]   = _cache[$"{prefix}_dragon_1"];

            // 제3 비기: 월인천강포 달빛 태극 파동포 모션
            _cache[$"{prefix}_wave_0"]   = CreateSideChubbySD(prefix, 0, "wave", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_wave_1"]   = CreateSideChubbySD(prefix, 1, "wave", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_wave"]     = _cache[$"{prefix}_wave_1"];

            // 물체/적을 들고 있는 모션 (대기, 이동 4프레임, 점프)
            _cache[$"{prefix}_carry"]        = CreateSideChubbySD(prefix, 0, "carry_idle", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_idle"]   = _cache[$"{prefix}_carry"];
            _cache[$"{prefix}_carry_walk_0"] = CreateSideChubbySD(prefix, 0, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_walk_1"] = CreateSideChubbySD(prefix, 1, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_walk_2"] = CreateSideChubbySD(prefix, 2, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_walk_3"] = CreateSideChubbySD(prefix, 3, "carry_walk", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);
            _cache[$"{prefix}_carry_jump"]   = CreateSideChubbySD(prefix, 0, "carry_jump", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // 든 것으로 후려치는 모션
            _cache[$"{prefix}_swing"]    = CreateSideChubbySD(prefix, 0, "swing", robeColor, vestColor, beltColor, isEnemy, isBoss, scaleW, scaleH, headgear);

            // "캐릭터가 쓰러진 모습은 원래 모습을 그대로 -90도 눕혀서 그린다"
            var baseIdle = _cache[$"{prefix}_idle_0"];
            var downBmp = new Bitmap(baseIdle);
            downBmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            _cache[$"{prefix}_down"] = downBmp;
        }

        private static Bitmap CreateSideChubbySD(string charId, int frame, string action, Color robeColor, Color vestColor, Color beltColor, bool isEnemy, bool isBoss, int w, int h, HeadgearType headgear)
        {
            var bmp = new Bitmap(w, h);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            Color skinBase = Color.FromArgb(255, 222, 190);
            Color skinShadow = Color.FromArgb(226, 175, 138);
            Color skinCheek = Color.FromArgb(255, 170, 155); // 통통한 분홍 볼살
            Color skinHi = Color.FromArgb(255, 240, 220);    // 얼굴 윤곽 입체 하이라이트
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
            int legH = 6; // 1.5배 길어진 다리 (6px)
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
            using var brSkinHi = new SolidBrush(skinHi);
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
            // 0. 등 뒤로 휘날리는 도포 뒤자락 / 망토 (Flowing Robe Tails)
            // ==========================================
            int flapOffset = 0;
            if (action == "walk" || action == "carry_walk")
            {
                flapOffset = (frame == 1 || frame == 3) ? -2 : -1;
            }
            else if (action == "jump" || action == "jumpkick" || action == "dragon")
            {
                flapOffset = -3;
            }
            else if (action == "spin")
            {
                flapOffset = (frame == 1 || frame == 2) ? 2 : -3;
            }
            else if (action == "kick" && frame == 2)
            {
                flapOffset = -3;
            }
            else if (action == "punch" && frame == 2)
            {
                flapOffset = -2;
            }

            int tailX = bx - 2 + flapOffset;
            int tailY = bodyY + 6;
            int tailW = 4;
            int tailH = bodyH - 3;

            if (charId == "player")
            {
                // 한랑: 바람결에 펄럭이는 백옥 도포 자락 + 감청색 쾌자 안감
                g.FillRectangle(brOut, tailX, tailY, tailW + 1, tailH + 1);
                g.FillRectangle(brRobeDk, tailX + 1, tailY + 1, tailW - 1, tailH - 1);
                g.FillRectangle(brRobeLt, tailX + 2, tailY + 1, 1, tailH - 2);
                g.FillRectangle(brVest, tailX + 1, tailY + 1, 2, 4);
            }
            else if (charId == "boss")
            {
                // 마도학사 흑호: 위압적인 칠흑 흑도포 자락 + 핏빛 진홍 안감 (Blood-Red Silk Lining)
                g.FillRectangle(brOut, tailX - 1, tailY, tailW + 2, tailH + 2);
                g.FillRectangle(brHair, tailX, tailY + 1, tailW, tailH);
                using var brCrimson = new SolidBrush(Color.FromArgb(195, 25, 25));
                g.FillRectangle(brCrimson, tailX, tailY + tailH - 2, tailW, 2);
            }
            else if (charId == "midboss2")
            {
                // 배신자 철웅: 검붉은 무관 전포 망토
                g.FillRectangle(brOut, tailX, tailY, tailW + 1, tailH + 1);
                using var brCape = new SolidBrush(Color.FromArgb(155, 30, 30));
                g.FillRectangle(brCape, tailX + 1, tailY + 1, tailW - 1, tailH - 1);
            }
            else if (charId == "enemy3")
            {
                // 암흑 결사대: 흩날리는 흑자색 닌자 스카프 자락
                g.FillRectangle(brOut, bx - 3 + flapOffset, headY + 5, 3, 5);
                using var brScarf = new SolidBrush(Color.FromArgb(110, 45, 145));
                g.FillRectangle(brScarf, bx - 2 + flapOffset, headY + 6, 2, 4);
            }

            // ==========================================
            // 1. 다리 & 버선발 & 각반/신발 (2픽셀 더 통통한 9px 너비, 다리 사이 1픽셀 분리선)
            // ==========================================
            int legW = 9;

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
                // 발끝 가죽 코/신발 디테일
                g.FillRectangle(brOut, bx + bodyW + 6, legY + 1, 2, 2);
            }
            else if (action == "kick")
            {
                // 4프레임 지상 발차기
                g.FillRectangle(brOut, bx, legY, legW, legH);
                g.FillRectangle(brRobe, bx + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, bx + 1, legY + 3, legW - 2, 3);

                // 차는 앞다리: 4프레임 역동적 전개
                if (frame == 0)
                {
                    g.FillRectangle(brOut, bx + bodyW - 4, legY - 3, 7, 7);
                    g.FillRectangle(brRobe, bx + bodyW - 3, legY - 2, 4, 5);
                    g.FillRectangle(brWhite, bx + bodyW + 1, legY - 1, 2, 4);
                }
                else if (frame == 1)
                {
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
                    g.FillRectangle(brOut, bx + bodyW - 4, legY - 2, 7, 7);
                    g.FillRectangle(brRobe, bx + bodyW - 3, legY - 1, 4, 5);
                    g.FillRectangle(brWhite, bx + bodyW, legY - 1, 3, 4);
                }
            }
            else if (action == "punch")
            {
                int lunge = (frame == 1) ? 1 : ((frame == 2) ? 2 : 0);
                int backX = bx;
                int frontX = bx + legW - 1 + lunge;

                // 뒷다리
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);

                // 앞다리
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);
                if (charId == "player")
                {
                    // 흑혜 신발 코
                    g.FillRectangle(brOut, frontX + legW - 3, legY + 4, 2, 2);
                }
            }
            else if (action == "spin")
            {
                // 비기 훈민선풍각: 4프레임 회전 회오리 발차기
                if (frame == 0)
                {
                    g.FillRectangle(brOut, bx - 2, legY, 7, legH);
                    g.FillRectangle(brRobeDk, bx - 1, legY, 5, 3);
                    g.FillRectangle(brWhite, bx - 1, legY + 3, 5, 3);

                    g.FillRectangle(brOut, bx + bodyW - 2, legY - 2, 10, 6);
                    g.FillRectangle(brRobe, bx + bodyW - 1, legY - 1, 4, 4);
                    g.FillRectangle(brWhite, bx + bodyW + 3, legY - 1, 5, 4);
                }
                else if (frame == 1)
                {
                    g.FillRectangle(brOut, bx - 5, legY - 1, 9, 6);
                    g.FillRectangle(brWhite, bx - 4, legY, 4, 4);
                    g.FillRectangle(brRobeDk, bx, legY, 3, 4);

                    g.FillRectangle(brOut, bx + bodyW - 4, legY - 1, 9, 6);
                    g.FillRectangle(brRobe, bx + bodyW - 3, legY, 3, 4);
                    g.FillRectangle(brWhite, bx + bodyW, legY, 5, 4);
                }
                else if (frame == 2)
                {
                    g.FillRectangle(brOut, bx - 5, legY - 2, 10, 6);
                    g.FillRectangle(brWhite, bx - 4, legY - 1, 5, 4);
                    g.FillRectangle(brRobe, bx + 1, legY - 1, 3, 4);

                    g.FillRectangle(brOut, bx + bodyW - 5, legY, 7, legH);
                    g.FillRectangle(brRobeDk, bx + bodyW - 4, legY, 5, 3);
                    g.FillRectangle(brWhite, bx + bodyW - 4, legY + 3, 5, 3);
                }
                else // frame == 3
                {
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
                g.FillRectangle(brOut, bx, legY - 1, legW, 5);
                g.FillRectangle(brWhite, bx + 1, legY, legW - 2, 3);
                g.FillRectangle(brOut, bx + legW - 1, legY - 1, legW, 5);
                g.FillRectangle(brWhite, bx + legW, legY, legW - 2, 3);
            }
            else if (action == "dragon")
            {
                if (frame == 0)
                {
                    // 프레임 0: 도약 직전 웅크린 다리
                    g.FillRectangle(brOut, bx, legY - 1, legW, 5);
                    g.FillRectangle(brRobeDk, bx + 1, legY, legW - 2, 2);
                    g.FillRectangle(brWhite, bx + 1, legY + 2, legW - 2, 2);
                    g.FillRectangle(brOut, bx + legW - 1, legY + 1, legW, 5);
                    g.FillRectangle(brRobe, bx + legW, legY + 2, legW - 2, 2);
                    g.FillRectangle(brWhite, bx + legW, legY + 4, legW - 2, 2);
                }
                else
                {
                    // 프레임 1: 하늘로 힘차게 솟구쳐 뻗은 다리
                    g.FillRectangle(brOut, bx, legY - 3, legW, 7);
                    g.FillRectangle(brRobeDk, bx + 1, legY - 2, legW - 2, 3);
                    g.FillRectangle(brWhite, bx + 1, legY + 1, legW - 2, 2);
                    g.FillRectangle(brOut, bx + legW - 2, legY, legW, 8);
                    g.FillRectangle(brRobe, bx + legW - 1, legY + 1, legW - 2, 4);
                    g.FillRectangle(brWhite, bx + legW - 1, legY + 5, legW - 2, 2);
                }
            }
            else if (action == "wave" || action == "guard")
            {
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
                int fOffset = (frame == 0) ? 2 : (frame == 1 ? 1 : (frame == 2 ? -2 : -1));
                int bOffset = -fOffset;

                int backX = bx + bOffset;
                int frontX = bx + legW - 1 + fOffset;

                g.FillRectangle(brRobeDk, bx + 1, legY, bodyW - 2, 2);

                // 뒷다리
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);

                // 앞다리
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);

                // 발끝 가죽 덧신
                if (charId == "player")
                {
                    g.FillRectangle(brOut, frontX + legW - 3, legY + 4, 2, 2);
                }
            }
            else // idle, carry, carry_idle
            {
                int backX = bx;
                int frontX = bx + legW - 1;

                // 뒷다리
                g.FillRectangle(brOut, backX, legY, legW, legH);
                g.FillRectangle(brRobeDk, backX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, backX + 1, legY + 3, legW - 2, 3);

                // 앞다리
                g.FillRectangle(brOut, frontX, legY, legW, legH);
                g.FillRectangle(brRobe, frontX + 1, legY, legW - 2, 3);
                g.FillRectangle(brWhite, frontX + 1, legY + 3, legW - 2, 3);

                if (charId == "player")
                {
                    g.FillRectangle(brOut, frontX + legW - 3, legY + 4, 2, 2);
                }
                else if (charId == "midboss2")
                {
                    // 철웅 강철 정강이 각반
                    using var brGreave = new SolidBrush(Color.FromArgb(185, 195, 210));
                    g.FillRectangle(brGreave, frontX + 2, legY + 1, 3, 3);
                }
            }

            // ==========================================
            // 2. 왼쪽(뒤쪽) 팔
            // ==========================================
            if (action == "carry" || action == "carry_idle" || action == "carry_walk" || action == "carry_jump")
            {
                g.FillRectangle(brOut, bx + 1, headY + 1, 6, 7);
                g.FillRectangle(brRobe, bx + 2, headY + 2, 4, 5);
                g.FillRectangle(brSkin, bx + 2, headY - 1, 4, 3);
            }
            else if (action == "guard")
            {
                g.FillRectangle(brOut, bx + 2, bodyY + 1, 6, 7);
                g.FillRectangle(brRobe, bx + 3, bodyY + 2, 4, 5);
                g.FillRectangle(brBelt, bx + 3, bodyY + 4, 4, 2);
                g.FillRectangle(brSkin, bx + 3, bodyY, 4, 3);
            }
            else if (action == "dragon")
            {
                if (frame == 0)
                {
                    // 프레임 0: 웅크림 - 왼팔 가슴 앞 가드
                    g.FillRectangle(brOut, bx + 1, bodyY + 2, 6, 7);
                    g.FillRectangle(brRobe, bx + 2, bodyY + 3, 4, 5);
                    g.FillRectangle(brSkin, bx + 3, bodyY + 2, 3, 3);
                }
                else
                {
                    // 프레임 1: 승천 - 왼팔 뒤로 젖혀 펄럭임
                    g.FillRectangle(brOut, bx - 2, bodyY + 3, 6, 8);
                    g.FillRectangle(brRobe, bx - 1, bodyY + 4, 4, 6);
                    g.FillRectangle(brSkin, bx, bodyY + 7, 3, 3);
                }
            }
            else if (action == "wave")
            {
                g.FillRectangle(brOut, bx + bodyW - 6, bodyY + 2, 7, 7);
                g.FillRectangle(brRobe, bx + bodyW - 5, bodyY + 3, 4, 5);
                g.FillRectangle(brSkin, bx + bodyW - 3, bodyY + 3, 4, 5);
            }
            else if (action == "spin")
            {
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
                int guardX = (frame == 0) ? (bx + 4) : ((frame == 2) ? (bx - 1) : (bx + 2));
                g.FillRectangle(brOut, guardX, bodyY + 3, 5, 6);
                g.FillRectangle(brSkin, guardX + 1, bodyY + 4, 3, 4);
            }
            else if (action == "kick")
            {
                int kArmX = (frame == 2) ? (bx - 3) : (bx - 1);
                g.FillRectangle(brOut, kArmX, bodyY + 2, 5, 6);
                g.FillRectangle(brSkin, kArmX, bodyY + 4, 3, 3);
            }

            // ==========================================
            // 3. 몸통 & 도포 & 쾌자 & 갑주/장식 & 허리띠
            // ==========================================
            g.FillRectangle(brOut, bx, bodyY, bodyW, bodyH);
            g.FillRectangle(brRobe, bx + 1, bodyY + 1, bodyW - 2, bodyH - 2);
            g.FillRectangle(brRobeDk, bx + 1, bodyY + 1, 3, bodyH - 2); // 등 뒤 깊은 음영
            g.FillRectangle(brRobeLt, bx + bodyW - 4, bodyY + 1, 3, bodyH - 3); // 앞가슴 입체 하이라이트

            // 쾌자 (도포 위 걸치는 조끼)
            g.FillRectangle(brVest, bx + 3, bodyY + 1, bodyW - 6, bodyH - 3);

            // 캐릭터별 특화 의복 및 휘장 디테일
            if (charId == "player")
            {
                // 주인공 한랑: 한복 백색 동정(목깃) & 품격 있는 비단 옷깃
                g.FillRectangle(brWhite, bx + 3, bodyY, bodyW - 6, 1);
                g.FillRectangle(brWhite, bx + bodyW - 4, bodyY + 1, 2, 3); // 단정한 동정 깃
            }
            else if (charId == "boss")
            {
                // 최종 보스 흑호: 가슴 황금 마호 흉배 (Golden Demon Tiger Insignia)
                using var brGoldEmblem = new SolidBrush(Color.FromArgb(255, 215, 60));
                using var brRubyEye = new SolidBrush(Color.FromArgb(230, 20, 20));
                g.FillRectangle(brGoldEmblem, bx + bodyW - 7, bodyY + 2, 5, 5);
                g.FillRectangle(brHair, bx + bodyW - 6, bodyY + 3, 3, 3);
                g.FillRectangle(brRubyEye, bx + bodyW - 5, bodyY + 4, 1, 1);
            }
            else if (charId == "midboss2")
            {
                // 중간보스 2 철웅: 흑철 흉갑 & 어깨 사자 머리 견갑(Lion Pauldrons)
                using var brSteel = new SolidBrush(Color.FromArgb(195, 205, 220));
                using var brGoldRivet = new SolidBrush(Color.FromArgb(240, 200, 50));
                g.FillRectangle(brSteel, bx + 1, bodyY, 4, 4);
                g.FillRectangle(brGoldRivet, bx + 2, bodyY + 1, 2, 2);
                g.FillRectangle(brSteel, bx + bodyW - 5, bodyY, 4, 4);
                g.FillRectangle(brGoldRivet, bx + bodyW - 4, bodyY + 1, 2, 2);
            }
            else if (charId == "midboss1")
            {
                // 중간보스 1 마달: 거친 반라 외투 & 거대한 호골 염주 목걸이
                g.FillRectangle(brSkin, bx + bodyW - 6, bodyY + 1, 4, 5); // 드러난 우측 근육 가슴
                using var brPrayerBeads = new SolidBrush(Color.FromArgb(200, 45, 30));
                g.FillRectangle(brPrayerBeads, bx + 2, bodyY + 2, 2, 2);
                g.FillRectangle(brPrayerBeads, bx + 4, bodyY + 4, 2, 2);
                g.FillRectangle(brPrayerBeads, bx + 6, bodyY + 6, 2, 2);
            }
            else if (charId == "enemy1")
            {
                // 골목 깡패: 거칠게 풀어헤친 V넥 흉부
                g.FillRectangle(brSkin, bx + bodyW - 5, bodyY + 1, 3, 4);
            }
            else if (charId == "enemy3")
            {
                // 암흑 결사대: 가슴을 가로지르는 X자 가죽 하네스 벨트 & 암기 버클
                using var brHarness = new SolidBrush(Color.FromArgb(55, 35, 25));
                using var brSilverBuckle = new SolidBrush(Color.FromArgb(210, 220, 235));
                g.FillRectangle(brHarness, bx + 3, bodyY + 1, 2, bodyH - 3);
                g.FillRectangle(brSilverBuckle, bx + 3, bodyY + 4, 2, 2);
            }

            // 허리띠 및 멋스러운 술띠(Tassel) / 패옥 장식
            g.FillRectangle(brBelt, bx + 1, bodyY + 8, bodyW - 2, 2);
            g.FillRectangle(brOut, bx + bodyW - 2, bodyY + 8, 2, 3); // 매듭
            g.FillRectangle(brBelt, bx + bodyW - 2, bodyY + 11, 2, 3); // 늘어뜨린 술띠
            g.FillRectangle(brWhite, bx + bodyW - 2, bodyY + 14, 2, 1);

            if (charId == "player")
            {
                // 주인공 한랑: 허리춤에 차고 있는 품격 있는 청옥 패옥(Jade Pendant)
                using var brJadePendant = new SolidBrush(Color.FromArgb(80, 225, 185));
                using var brGoldRing = new SolidBrush(Color.FromArgb(250, 215, 60));
                g.FillRectangle(brGoldRing, bx + bodyW - 4, bodyY + 10, 2, 1);
                g.FillRectangle(brJadePendant, bx + bodyW - 4, bodyY + 11, 2, 3);
            }
            else if (charId == "enemy2")
            {
                // 불량 훈도: 허리띠 뒤편의 쇠회초리(Discipline Rod) 손잡이
                using var brRod = new SolidBrush(Color.FromArgb(145, 90, 45));
                g.FillRectangle(brRod, bx, bodyY + 6, 2, 6);
            }

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
            g.FillRectangle(brSkinHi, hx + headW - 4, headY + 6, 2, 2); // 코와 볼의 입체광

            // 옆모습 통통한 볼살 & 콧날
            g.FillRectangle(brCheek, hx + headW - 3, headY + 8, 4, 3);
            g.FillRectangle(brSkin, hx + headW - 2, headY + 7, 2, 2);

            // 눈 & 표정 (캐릭터별 개성 및 카리스마)
            if (action == "hit")
            {
                g.FillRectangle(brWhite, hx + headW - 5, headY + 5, 5, 5);
                g.FillRectangle(brOut, hx + headW - 4, headY + 6, 2, 3);
                g.FillRectangle(brOut, hx + headW - 3, headY + 11, 4, 2);
                g.FillRectangle(new SolidBrush(Color.FromArgb(230, 40, 40)), hx + headW - 2, headY + 11, 2, 1);
            }
            else
            {
                if (charId == "player")
                {
                    // 주인공 유생 한랑: 결의에 찬 강인한 무사 눈썹 + 빛나는 2x2 호걸의 영롱한 눈빛!
                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 2);
                    g.FillRectangle(brOut, hx + headW - 5, headY + 7, 4, 3);
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 7, 2, 2); // 반짝이는 눈빛 하이라이트!
                    g.FillRectangle(brOut, hx + headW - 3, headY + 11, 3, 1);  // 굳게 다문 의지의 입매

                    // 뒤로 휘날리는 청홍 댕기 머리띠 리본
                    g.FillRectangle(brBelt, hx - 2, headY + 5, 3, 2);
                    g.FillRectangle(brBelt, hx - 3, headY + 7, 2, 3);
                }
                else if (charId == "boss")
                {
                    // 마도학사 흑호: 불길하게 타오르는 진홍빛 사안(Crimson Demon Eye) & 황금 동공
                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 2);
                    using var brEvilSclera = new SolidBrush(Color.FromArgb(255, 35, 35));
                    using var brGoldPupil = new SolidBrush(Color.FromArgb(255, 225, 60));
                    g.FillRectangle(brEvilSclera, hx + headW - 5, headY + 7, 4, 3);
                    g.FillRectangle(brGoldPupil, hx + headW - 4, headY + 7, 2, 2);
                    g.FillRectangle(brOut, hx + headW - 3, headY + 11, 4, 1);
                }
                else if (charId == "enemy1")
                {
                    // 골목 깡패: 이마 붉은 두건 + 카리스마 넘치는 뺨의 칼자국 흉터(Battle Scar)
                    using var brBandana = new SolidBrush(Color.FromArgb(215, 35, 35));
                    g.FillRectangle(brBandana, hx + 1, headY + 3, headW - 2, 2);
                    g.FillRectangle(brBandana, hx - 2, headY + 4, 3, 2); // 펄럭이는 두건 꼬리
                    g.FillRectangle(brBandana, hx - 3, headY + 6, 2, 3);

                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 1);
                    g.FillRectangle(brOut, hx + headW - 5, headY + 6, 4, 4);
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 6, 1, 2);

                    // 뺨의 붉은 칼자국 흉터
                    using var brScar = new SolidBrush(Color.FromArgb(195, 65, 55));
                    g.FillRectangle(brScar, hx + headW - 4, headY + 8, 3, 1);
                    g.FillRectangle(brScar, hx + headW - 3, headY + 9, 2, 2);

                    g.FillRectangle(brOut, hx + headW - 3, headY + 11, 3, 1); // 비틀린 미소
                }
                else if (charId == "enemy2")
                {
                    // 불량 훈도: 날카로운 여우 눈매 & 턱밑 뾰족한 염소수염 (Goatee)
                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 1);
                    g.FillRectangle(brOut, hx + headW - 5, headY + 6, 4, 3);
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 6, 1, 2);

                    // 턱밑 염소수염
                    g.FillRectangle(brHair, hx + headW - 2, headY + 11, 2, 3);
                    g.FillRectangle(brHair, hx + headW - 1, headY + 14, 1, 1);

                    g.FillRectangle(brOut, hx + headW - 3, headY + 10, 3, 1);
                }
                else if (charId == "enemy3")
                {
                    // 암흑 결사대: 입과 코를 가린 자객 복면 + 번뜩이는 시안빛 살수의 안광!
                    using var brMask = new SolidBrush(Color.FromArgb(28, 20, 36));
                    using var brCyanGleam = new SolidBrush(Color.FromArgb(100, 235, 255));
                    g.FillRectangle(brMask, hx + headW - 5, headY + 8, 6, 4); // 복면

                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 2);
                    g.FillRectangle(brCyanGleam, hx + headW - 5, headY + 7, 3, 2); // 시안빛 살수 안광
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 7, 1, 1);
                }
                else
                {
                    // 일반/중간보스 기본 호걸 눈매
                    g.FillRectangle(brOut, hx + headW - 5, headY + 6, 4, 4);
                    g.FillRectangle(brWhite, hx + headW - 4, headY + 6, 1, 2);
                    g.FillRectangle(brOut, hx + headW - 6, headY + 5, 5, 1);
                    g.FillRectangle(brOut, hx + headW - 3, headY + 11, 3, 1);
                }
            }

            // ==========================================
            // 5. 헤드기어 (상투, 흑립 갓 & 옥로, 무관 전립)
            // ==========================================
            if (charId == "midboss2")
            {
                // ------------------------------------------
                // 무과 배신자 철웅: 조선 무관 전립 (Jeonrip with Crimson Feather Plume)
                // ------------------------------------------
                Color jeonripDark = Color.FromArgb(20, 18, 24);
                Color jeonripBody = Color.FromArgb(36, 32, 42);
                Color jeonripGold = Color.FromArgb(245, 210, 50);
                Color plumeRed = Color.FromArgb(225, 35, 35);
                Color plumeLt = Color.FromArgb(255, 110, 110);

                using var brJDark = new SolidBrush(jeonripDark);
                using var brJBody = new SolidBrush(jeonripBody);
                using var brJGold = new SolidBrush(jeonripGold);
                using var brPRed = new SolidBrush(plumeRed);
                using var brPLt = new SolidBrush(plumeLt);

                // 벙거지 돔 & 황금 꼭지 정자
                g.FillRectangle(brJGold, hx + 5, headY - 6, 3, 2);
                g.FillRectangle(brJDark, hx + 2, headY - 4, 10, 6);
                g.FillRectangle(brJBody, hx + 3, headY - 4, 8, 5);
                g.FillRectangle(brJGold, hx + 2, headY + 1, 10, 1); // 금박 띠

                // 챙
                int brimX = hx - 3;
                int brimW = headW + 6;
                int brimY = headY + 2;
                g.FillRectangle(brJDark, brimX, brimY, brimW, 2);

                // 뒤로 휘날리는 붉은 꿩 깃털 (Crimson Feather Plume)
                g.FillRectangle(brPRed, hx + 4, headY - 7, 2, 2);
                g.FillRectangle(brPRed, hx + 1, headY - 8, 3, 2);
                g.FillRectangle(brPLt, hx - 3, headY - 8, 4, 2);
                g.FillRectangle(brPRed, hx - 7, headY - 7, 4, 2);
                g.FillRectangle(brPLt, hx - 10, headY - 5, 3, 2);
            }
            else if (headgear == HeadgearType.Gat)
            {
                // ------------------------------------------
                // 갓 (흑립 갓 & 옥로 & 삼색 갓끈)
                // ------------------------------------------
                Color gatDark = Color.FromArgb(14, 12, 18);
                Color gatBody = Color.FromArgb(28, 25, 34);
                Color gatLight = Color.FromArgb(62, 58, 74);
                Color gatBeadRed = Color.FromArgb(210, 45, 45);
                Color gatBeadGold = Color.FromArgb(235, 195, 60);
                Color gatBeadJade = Color.FromArgb(85, 215, 180);

                using var brGatDark = new SolidBrush(gatDark);
                using var brGatBody = new SolidBrush(gatBody);
                using var brGatLt = new SolidBrush(gatLight);
                using var brBeadR = new SolidBrush(gatBeadRed);
                using var brBeadG = new SolidBrush(gatBeadGold);
                using var brBeadJ = new SolidBrush(gatBeadJade);

                // 1) 대우 (Crown)
                g.FillRectangle(brGatDark, hx + 3, headY - 5, 8, 1);
                g.FillRectangle(brGatLt, hx + 4, headY - 5, 6, 1);

                g.FillRectangle(brGatDark, hx + 2, headY - 4, 10, 7);
                g.FillRectangle(brGatBody, hx + 3, headY - 4, 8, 6);
                g.FillRectangle(brGatLt, hx + 4, headY - 4, 2, 5);
                g.FillRectangle(brGatDark, hx + 8, headY - 4, 2, 6);

                if (charId == "player")
                {
                    // 주인공 한랑의 옥로 (White Jade Okro Crane Crest)
                    using var brOkro = new SolidBrush(Color.FromArgb(235, 248, 255));
                    using var brOkroSh = new SolidBrush(Color.FromArgb(160, 215, 235));
                    g.FillRectangle(brOkroSh, hx + 5, headY - 7, 3, 2);
                    g.FillRectangle(brOkro, hx + 6, headY - 8, 2, 2); // 솟아오른 백옥 꼭지
                }
                else if (charId == "boss")
                {
                    // 흑호의 마정석 (Purple Demon Gem)
                    using var brDemonGem = new SolidBrush(Color.FromArgb(195, 55, 255));
                    using var brGemCore = new SolidBrush(Color.FromArgb(255, 160, 255));
                    g.FillRectangle(brDemonGem, hx + 5, headY - 7, 4, 3);
                    g.FillRectangle(brGemCore, hx + 6, headY - 6, 2, 1);
                }

                // 2) 양태 (Brim)
                int brimX = hx - 4;
                int brimW = headW + 7;
                int brimY = headY + 2;
                g.FillRectangle(brGatDark, brimX, brimY, brimW, 2);
                g.FillRectangle(brGatBody, brimX + 1, brimY, brimW - 2, 1);
                g.FillRectangle(brGatLt, brimX + 3, brimY, 6, 1);

                // 3) 휘날리는 유려한 삼색 갓끈 (산호 + 금 + 비취)
                g.FillRectangle(brGatDark, hx + 4, headY + 6, 1, 5);
                g.FillRectangle(brBeadR, hx + 4, headY + 11, 2, 2); // 산호 구슬
                g.FillRectangle(brBeadG, hx + 3, headY + 13, 2, 2); // 금구슬
                g.FillRectangle(brBeadJ, hx + 2, headY + 15, 2, 2); // 비취 구슬
                g.FillRectangle(brGatDark, hx + 1, headY + 17, 1, 3);
                g.FillRectangle(brBeadR, hx, headY + 20, 2, 2);
            }
            else
            {
                // ------------------------------------------
                // 상투 (전통 상투와 비녀 머리)
                // ------------------------------------------
                g.FillRectangle(brOut, hx + 2, headY - 3, 6, 5);
                g.FillRectangle(brHairLt, hx + 3, headY - 2, 4, 3);
                g.FillRectangle(brOut, hx, headY, 9, 2);
                g.FillRectangle(brWhite, hx, headY, 2, 2); // 은/골 비녀

                if (charId == "midboss1")
                {
                    // 마달: 쇠고리 상투 밴드
                    using var brIronRing = new SolidBrush(Color.FromArgb(170, 180, 195));
                    g.FillRectangle(brIronRing, hx + 2, headY - 1, 6, 2);
                }
            }

            // ==========================================
            // 6. 오른쪽(앞쪽) 팔 & 권격 & 무사 아대
            // ==========================================
            if (action == "punch")
            {
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

                if (charId == "player")
                {
                    // 한랑: 손목을 감싼 권투 무사 가죽 아대 & 백색 손등 붕대
                    g.FillRectangle(brBelt, armX + armW - 3, bodyY + 3, 2, 6);
                    g.FillRectangle(brWhite, armX + armW - 1, bodyY + 3, 4, 6);
                }
                else if (charId == "midboss1")
                {
                    // 마달: 쇠징 박힌 거대한 무쇠 주먹 장갑
                    using var brStud = new SolidBrush(Color.FromArgb(235, 235, 245));
                    g.FillRectangle(brOut, armX + armW - 1, bodyY + 2, 6, 7);
                    g.FillRectangle(brStud, armX + armW + 3, bodyY + 3, 2, 2);
                    g.FillRectangle(brStud, armX + armW + 3, bodyY + 6, 2, 2);
                }
                else
                {
                    g.FillRectangle(brSkin, armX + armW - 1, bodyY + 3, 5, 6);
                }

                g.FillRectangle(brSkinSh, armX + armW + 1, bodyY + 5, 2, 3);
                g.FillRectangle(brWhite, armX + armW - 1, bodyY + 3, 2, 2);

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
                int armX = frame switch
                {
                    0 => bx + 4,
                    1 => bx + 6,
                    2 => bx + bodyW - 2,
                    3 => bx + 5,
                    _ => bx + 5
                };
                g.FillRectangle(brOut, armX, bodyY + 2, 6, 7);
                g.FillRectangle(brRobe, armX + 1, bodyY + 3, 4, 5);
                g.FillRectangle(brSkin, armX + 2, bodyY + 5, 3, 3);
                if (charId == "player")
                {
                    g.FillRectangle(brBelt, armX + 1, bodyY + 4, 2, 3); // 손목 아대
                }
            }
            else if (action == "carry" || action == "carry_idle" || action == "carry_walk" || action == "carry_jump")
            {
                g.FillRectangle(brOut, bx + bodyW - 7, headY + 1, 6, 7);
                g.FillRectangle(brRobe, bx + bodyW - 6, headY + 2, 4, 5);
                g.FillRectangle(brSkin, bx + bodyW - 6, headY - 1, 4, 3);
            }
            else if (action == "swing")
            {
                g.FillRectangle(brOut, bx + bodyW - 3, bodyY + 2, 9, 8);
                g.FillRectangle(brRobe, bx + bodyW - 2, bodyY + 3, 3, 6);
                g.FillRectangle(brSkin, bx + bodyW + 1, bodyY + 4, 5, 5);
            }
            else if (action == "spin")
            {
                int rArmX = frame switch
                {
                    0 => bx + bodyW - 2,
                    1 => bx + bodyW,
                    2 => bx + bodyW - 4,
                    3 => bx + bodyW - 1,
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
                    g.FillRectangle(brGold, bx + bodyW + 4, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brCyan, bx + bodyW + 2, bodyY - 2, 3, 2);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 5, bodyY + 2, 1, 6);
                    g.FillRectangle(brCyan, bx - 3, legY + 2, 2, 3);
                }
                else if (frame == 1)
                {
                    g.FillRectangle(brCyan, bx - 6, bodyY + 2, 2, bodyH);
                    g.FillRectangle(brWhiteLt, bx - 5, bodyY + 4, 1, 6);
                    g.FillRectangle(brGold, bx + bodyW + 4, bodyY + 2, 2, bodyH);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 4, bodyY + 4, 1, 6);
                    g.FillRectangle(brCyan, bx + 2, bodyY - 3, bodyW - 4, 2);
                    g.FillRectangle(brGold, bx + 1, legY + 5, bodyW - 2, 2);
                }
                else if (frame == 2)
                {
                    g.FillRectangle(brCyan, bx - 6, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brGold, bx - 4, bodyY - 2, 3, 2);
                    g.FillRectangle(brWhiteLt, bx - 5, bodyY + 2, 1, 6);
                    g.FillRectangle(brGold, bx + bodyW + 2, legY + 2, 2, 3);
                }
                else // frame == 3
                {
                    g.FillRectangle(brGold, bx - 5, bodyY - 3, bodyW + 10, 2);
                    g.FillRectangle(brWhiteLt, bx - 2, bodyY - 2, bodyW + 4, 1);
                    g.FillRectangle(brCyan, bx - 6, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brCyan, bx + bodyW + 4, bodyY, 2, bodyH + 4);
                    g.FillRectangle(brGold, bx - 5, legY + 5, bodyW + 10, 2);
                    g.FillRectangle(brWhiteLt, bx - 2, legY + 5, bodyW + 4, 1);
                    g.FillRectangle(brWhiteLt, bx - 4, bodyY - 1, 2, 2);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 2, bodyY - 1, 2, 2);
                    g.FillRectangle(brWhiteLt, bx - 4, legY + 3, 2, 2);
                    g.FillRectangle(brWhiteLt, bx + bodyW + 2, legY + 3, 2, 2);
                }
            }
            else if (action == "guard")
            {
                g.FillRectangle(brOut, bx + bodyW - 5, bodyY - 1, 7, 9);
                g.FillRectangle(brRobe, bx + bodyW - 4, bodyY, 5, 7);
                g.FillRectangle(brBelt, bx + bodyW - 4, bodyY + 3, 5, 2);
                g.FillRectangle(brSkin, bx + bodyW - 4, bodyY - 1, 5, 4);
                g.FillRectangle(brWhite, bx + bodyW - 3, bodyY, 2, 2);
            }
            else if (action == "dragon")
            {
                if (frame == 0)
                {
                    // 프레임 0: 어퍼컷 끌어올림 장전 (골반/허리 아래에서 힘 모으기)
                    int dArmX = bx + bodyW - 2;
                    int dArmY = bodyY + 3;
                    g.FillRectangle(brOut, dArmX, dArmY, 7, 10);
                    g.FillRectangle(brRobe, dArmX + 1, dArmY + 1, 5, 5);
                    g.FillRectangle(brSkin, dArmX + 1, dArmY + 5, 5, 4);
                    g.FillRectangle(brWhite, dArmX + 2, dArmY + 6, 3, 2); // 백색 붕대 주먹
                    using var brGoldSpark = new SolidBrush(Color.FromArgb(255, 230, 80));
                    g.FillRectangle(brGoldSpark, dArmX - 1, dArmY + 3, 9, 2);
                }
                else
                {
                    // 프레임 1: 하늘 높이 수직으로 뻗은 영웅적 승룡 어퍼컷!
                    int dArmX = bx + bodyW - 4;
                    int dArmY = headY - 10;
                    g.FillRectangle(brOut, dArmX, dArmY, 7, 18);
                    g.FillRectangle(brRobe, dArmX + 1, dArmY + 7, 5, 10);
                    g.FillRectangle(brSkin, dArmX + 1, dArmY + 1, 5, 6);
                    g.FillRectangle(brWhite, dArmX + 2, dArmY + 2, 3, 3); // 붕대 주먹
                    using var brGoldGlow = new SolidBrush(Color.FromArgb(255, 225, 60));
                    using var brCyanGlow = new SolidBrush(Color.FromArgb(120, 240, 255));
                    // 주먹 주변 타오르는 황금 & 청룡 기운
                    g.FillRectangle(brGoldGlow, dArmX - 2, dArmY - 2, 11, 3);
                    g.FillRectangle(brCyanGlow, dArmX - 1, dArmY + 1, 9, 2);
                    g.FillRectangle(brGoldGlow, dArmX - 2, dArmY + 1, 2, 14);
                    g.FillRectangle(brGoldGlow, dArmX + 7, dArmY + 1, 2, 14);
                }
            }
            else if (action == "wave")
            {
                int wArmX = bx + bodyW - 2;
                int wArmY = bodyY;
                g.FillRectangle(brOut, wArmX, wArmY, 9, 8);
                g.FillRectangle(brRobe, wArmX + 1, wArmY + 1, 4, 6);
                g.FillRectangle(brSkin, wArmX + 4, wArmY, 5, 8);
                using var brCyanOrb = new SolidBrush(Color.FromArgb(140, 230, 255));
                g.FillEllipse(brCyanOrb, wArmX + 7, wArmY + 1, 5, 6);
            }
            else
            {
                // 기본 대기 및 보행: 듬직한 앞쪽 팔뚝 & 손목 무사 아대
                int armOffset = (action == "walk") ? ((frame == 0 || frame == 1) ? 2 : -2) : 0;
                g.FillRectangle(brOut, bx + 5 + armOffset, bodyY + 2, 7, 7);
                g.FillRectangle(brRobe, bx + 6 + armOffset, bodyY + 3, 5, 4);
                if (charId == "player")
                {
                    g.FillRectangle(brBelt, bx + 6 + armOffset, bodyY + 5, 5, 2); // 붉은 권투 아대
                    g.FillRectangle(brWhite, bx + 6 + armOffset, bodyY + 7, 5, 2); // 백색 붕대 주먹
                }
                else
                {
                    g.FillRectangle(brSkin, bx + 6 + armOffset, bodyY + 6, 5, 3);
                    g.FillRectangle(brSkinSh, bx + 8 + armOffset, bodyY + 7, 2, 2);
                }
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

        private static Bitmap CreateCart()
        {
            var bmp = new Bitmap(34, 24);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(40, 24, 12));
            using var bWood = new SolidBrush(Color.FromArgb(155, 98, 48));
            using var bWoodDk = new SolidBrush(Color.FromArgb(108, 62, 28));
            using var bWoodLt = new SolidBrush(Color.FromArgb(205, 145, 80));
            using var bIron = new SolidBrush(Color.FromArgb(60, 65, 72));
            using var bIronLt = new SolidBrush(Color.FromArgb(135, 142, 150));

            // 대형 달구지 짐수레 (화물 적재함 + 손잡이 끌채 + 큼직한 바퀴살)
            // 1. 적재함 본체
            g.FillRectangle(bOut, 2, 4, 26, 12);
            g.FillRectangle(bWood, 3, 5, 24, 10);

            // 적재함 내부 나무 판자 살
            g.FillRectangle(bWoodLt, 4, 6, 22, 2);
            g.FillRectangle(bWoodDk, 4, 9, 22, 1);
            g.FillRectangle(bWoodLt, 4, 11, 22, 2);

            // 적재함 세로 지지 기둥
            g.FillRectangle(bOut, 4, 3, 2, 13);
            g.FillRectangle(bOut, 15, 3, 2, 13);
            g.FillRectangle(bOut, 26, 3, 2, 13);

            // 2. 앞쪽 끌채/손잡이
            g.FillRectangle(bOut, 26, 9, 7, 3);
            g.FillRectangle(bWoodLt, 27, 10, 6, 1);

            // 3. 수레바퀴
            // 좌측 뒷바퀴 약간 어둡게
            g.FillEllipse(bOut, 3, 11, 10, 11);
            g.FillEllipse(bWoodDk, 4, 12, 8, 9);

            // 우측 앞바퀴 (중심 주 바퀴)
            g.FillEllipse(bOut, 14, 11, 12, 12);
            g.FillEllipse(bIron, 15, 12, 10, 10);
            g.FillEllipse(bWood, 16, 13, 8, 8);

            // 바퀴살 (Spokes) & 철제 축(Hub)
            using var pSpoke = new Pen(bIronLt, 1);
            g.DrawLine(pSpoke, 20, 13, 20, 21);
            g.DrawLine(pSpoke, 16, 17, 24, 17);
            g.DrawLine(pSpoke, 17, 14, 23, 20);
            g.DrawLine(pSpoke, 17, 20, 23, 14);

            g.FillEllipse(bIronLt, 18, 15, 4, 4);
            g.FillEllipse(bOut, 19, 16, 2, 2);

            return bmp;
        }

        private static Bitmap CreateTable()
        {
            var bmp = new Bitmap(30, 18);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(42, 22, 12));
            using var bWood = new SolidBrush(Color.FromArgb(145, 82, 38));
            using var bWoodDk = new SolidBrush(Color.FromArgb(92, 48, 20));
            using var bWoodLt = new SolidBrush(Color.FromArgb(195, 125, 68));
            using var bShine = new SolidBrush(Color.FromArgb(235, 175, 115));

            // 대형 주막 주안상 / 서당 서책 탁자
            // 1. 넓은 상판
            g.FillRectangle(bOut, 1, 2, 28, 7);
            g.FillRectangle(bWood, 2, 3, 26, 5);
            g.FillRectangle(bShine, 3, 3, 24, 1);
            g.FillRectangle(bWoodLt, 3, 4, 24, 2);
            g.FillRectangle(bWoodDk, 2, 6, 26, 2);

            // 2. 탁자 뒷다리
            g.FillRectangle(bOut, 5, 8, 3, 7);
            g.FillRectangle(bWoodDk, 6, 8, 2, 6);
            g.FillRectangle(bOut, 22, 8, 3, 7);
            g.FillRectangle(bWoodDk, 22, 8, 2, 6);

            // 가로 지지대 쇠목
            g.FillRectangle(bOut, 4, 12, 22, 2);
            g.FillRectangle(bWoodDk, 4, 12, 22, 1);

            // 3. 탁자 앞다리
            g.FillRectangle(bOut, 2, 8, 4, 9);
            g.FillRectangle(bWood, 3, 8, 2, 8);
            g.FillRectangle(bWoodLt, 3, 9, 1, 6);

            g.FillRectangle(bOut, 24, 8, 4, 9);
            g.FillRectangle(bWood, 25, 8, 2, 8);
            g.FillRectangle(bWoodLt, 25, 9, 1, 6);

            return bmp;
        }

        private static Bitmap CreateBench()
        {
            var bmp = new Bitmap(32, 16);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(38, 24, 14));
            using var bWood = new SolidBrush(Color.FromArgb(162, 110, 56));
            using var bWoodDk = new SolidBrush(Color.FromArgb(110, 70, 32));
            using var bWoodLt = new SolidBrush(Color.FromArgb(210, 158, 92));

            // 대형 주막 평상 / 긴 목조 의자
            // 1. 긴 널빤지 평상 판자
            g.FillRectangle(bOut, 1, 2, 30, 6);
            g.FillRectangle(bWood, 2, 3, 28, 4);

            // 판자 틈새 표현
            g.FillRectangle(bWoodLt, 2, 3, 28, 1);
            g.FillRectangle(bWoodDk, 10, 3, 1, 4);
            g.FillRectangle(bWoodDk, 20, 3, 1, 4);

            // 2. 가로 지지 대들보
            g.FillRectangle(bOut, 3, 7, 26, 3);
            g.FillRectangle(bWoodDk, 4, 8, 24, 2);

            // 3. 네 귀퉁이 굵은 다리
            g.FillRectangle(bOut, 3, 9, 4, 6);
            g.FillRectangle(bWood, 4, 9, 2, 5);
            g.FillRectangle(bWoodLt, 4, 10, 1, 4);

            g.FillRectangle(bOut, 25, 9, 4, 6);
            g.FillRectangle(bWood, 26, 9, 2, 5);
            g.FillRectangle(bWoodLt, 26, 10, 1, 4);

            // 중앙 보강 기둥
            g.FillRectangle(bOut, 14, 9, 4, 6);
            g.FillRectangle(bWoodDk, 15, 9, 2, 5);

            return bmp;
        }

        private static Bitmap CreateRiceSack()
        {
            var bmp = new Bitmap(24, 18);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bOut = new SolidBrush(Color.FromArgb(48, 38, 18));
            using var bStraw = new SolidBrush(Color.FromArgb(198, 162, 92));
            using var bStrawLt = new SolidBrush(Color.FromArgb(232, 202, 134));
            using var bStrawDk = new SolidBrush(Color.FromArgb(148, 116, 58));
            using var bRope = new SolidBrush(Color.FromArgb(102, 74, 32));
            using var bRed = new SolidBrush(Color.FromArgb(195, 35, 30));

            // 전통 볏짚 쌀가마니 (곡물 포대)
            // 1. 양쪽 묶음 매듭 술
            g.FillRectangle(bOut, 0, 5, 3, 8);
            g.FillRectangle(bStrawDk, 0, 6, 2, 6);
            g.FillRectangle(bOut, 21, 5, 3, 8);
            g.FillRectangle(bStrawDk, 22, 6, 2, 6);

            // 2. 몸체 본체
            g.FillEllipse(bOut, 2, 1, 20, 16);
            g.FillEllipse(bStraw, 3, 2, 18, 14);

            // 볏짚 격자 질감
            g.FillRectangle(bStrawLt, 5, 4, 14, 3);
            g.FillRectangle(bStrawDk, 4, 8, 16, 1);
            g.FillRectangle(bStrawLt, 5, 10, 14, 2);
            g.FillRectangle(bStrawDk, 4, 13, 16, 1);

            // 3. 새끼줄 동여맴
            g.FillRectangle(bRope, 7, 2, 2, 14);
            g.FillRectangle(bRope, 15, 2, 2, 14);

            // 4. 중앙 붉은 낙관 각인
            g.FillRectangle(bRed, 11, 7, 3, 4);
            g.FillRectangle(bStrawLt, 12, 8, 1, 2);

            return bmp;
        }

        private static Bitmap CreateArrow()
        {
            var bmp = new Bitmap(18, 6);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bShaft = new SolidBrush(Color.FromArgb(180, 140, 70));
            using var bHead = new SolidBrush(Color.FromArgb(210, 215, 225));
            using var bHeadDk = new SolidBrush(Color.FromArgb(90, 95, 105));
            using var bFeather = new SolidBrush(Color.FromArgb(235, 235, 240));
            using var bFeatherDk = new SolidBrush(Color.FromArgb(160, 40, 40));

            // 화살대 (대나무살)
            g.FillRectangle(bShaft, 3, 2, 11, 2);

            // 깃 (꿩 깃털 / 붉은 실)
            g.FillRectangle(bFeather, 0, 1, 3, 4);
            g.FillRectangle(bFeatherDk, 2, 2, 2, 2);

            // 화살촉 (쇠촉 철촉)
            g.FillPolygon(bHead, new Point[] { new Point(14, 0), new Point(18, 3), new Point(14, 5) });
            g.FillPolygon(bHeadDk, new Point[] { new Point(14, 2), new Point(17, 3), new Point(14, 4) });

            return bmp;
        }

        private static Bitmap CreateSpear()
        {
            var bmp = new Bitmap(28, 10);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bWood = new SolidBrush(Color.FromArgb(145, 95, 45));
            using var bBlade = new SolidBrush(Color.FromArgb(220, 225, 235));
            using var bBladeDk = new SolidBrush(Color.FromArgb(110, 115, 125));
            using var bTassel = new SolidBrush(Color.FromArgb(220, 45, 45));

            // 긴 창대 (나무)
            g.FillRectangle(bWood, 0, 4, 20, 2);

            // 붉은 상모/술 (적색 술)
            g.FillRectangle(bTassel, 18, 2, 3, 6);

            // 강철 창날
            g.FillPolygon(bBlade, new Point[] { new Point(20, 1), new Point(28, 5), new Point(20, 9) });
            g.FillPolygon(bBladeDk, new Point[] { new Point(20, 4), new Point(26, 5), new Point(20, 6) });

            return bmp;
        }

        private static Bitmap CreateCudgel()
        {
            var bmp = new Bitmap(22, 10);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bWood = new SolidBrush(Color.FromArgb(120, 75, 35));
            using var bIron = new SolidBrush(Color.FromArgb(70, 75, 85));
            using var bStud = new SolidBrush(Color.FromArgb(200, 205, 215));

            // 굵은 쇠방망이 몸통
            g.FillRectangle(bWood, 0, 3, 20, 4);
            g.FillRectangle(bIron, 10, 2, 10, 6);

            // 쇠징 박힘 (리벳/돌기)
            g.FillRectangle(bStud, 12, 3, 2, 1);
            g.FillRectangle(bStud, 16, 3, 2, 1);
            g.FillRectangle(bStud, 12, 6, 2, 1);
            g.FillRectangle(bStud, 16, 6, 2, 1);

            return bmp;
        }

        private static Bitmap CreateFan()
        {
            var bmp = new Bitmap(18, 12);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bBamboo = new SolidBrush(Color.FromArgb(160, 125, 60));
            using var bPaper = new SolidBrush(Color.FromArgb(245, 240, 225));
            using var bBlue = new SolidBrush(Color.FromArgb(40, 90, 180));
            using var bRed = new SolidBrush(Color.FromArgb(200, 40, 40));

            // 펼친 합죽선 부채살
            g.FillPolygon(bPaper, new Point[] { new Point(2, 2), new Point(16, 2), new Point(13, 10), new Point(5, 10) });

            // 태극 문양
            g.FillEllipse(bRed, 7, 3, 4, 4);
            g.FillEllipse(bBlue, 7, 5, 4, 3);

            // 부채살 뼈대
            g.FillRectangle(bBamboo, 8, 8, 2, 4);

            return bmp;
        }

        private static Bitmap CreateHwacha()
        {
            var bmp = new Bitmap(36, 24);
            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            using var bWood = new SolidBrush(Color.FromArgb(135, 85, 40));
            using var bWoodDk = new SolidBrush(Color.FromArgb(85, 50, 25));
            using var bIron = new SolidBrush(Color.FromArgb(60, 65, 75));
            using var bRockets = new SolidBrush(Color.FromArgb(200, 60, 40));
            using var bGold = new SolidBrush(Color.FromArgb(220, 180, 50));

            // 수레 본체 바퀴
            g.FillEllipse(bWoodDk, 3, 11, 12, 12);
            g.FillEllipse(bIron, 6, 14, 6, 6);

            // 수레 지지대 및 손잡이
            g.FillRectangle(bWood, 10, 14, 22, 3);
            g.FillRectangle(bWoodDk, 28, 11, 4, 11);

            // 신기전 발사대 (로켓 다연장 통)
            g.FillRectangle(bWoodDk, 8, 2, 22, 10);
            g.FillRectangle(bIron, 7, 2, 2, 10);

            // 로켓 화전 머리들 (빨간 화전 촉)
            for (int y = 3; y <= 9; y += 3)
            {
                for (int x = 10; x <= 26; x += 4)
                {
                    g.FillRectangle(bRockets, x, y, 2, 2);
                }
            }
            g.FillRectangle(bGold, 16, 5, 6, 4); // 관청 인장

            return bmp;
        }
    }
}
