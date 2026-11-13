using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using HanlangGame.Engine;

namespace HanlangGame.Visuals
{
    /// <summary>
    /// 9개 스테이지별 다채로운 조선 전통 건축물, 정원, 누각, 성곽 및 소품을 렌더링하는 배경 전담 모듈.
    /// 매크로 순환 배치(약 1860~1960px 주기)를 사용하여 화면 내 동일 건물이 반복되지 않도록 구성합니다.
    /// </summary>
    public static class JoseonBackgrounds
    {
        private readonly struct ArchSegment
        {
            public readonly int OffsetX;
            public readonly int Width;
            public readonly int Variant;

            public ArchSegment(int offsetX, int width, int variant)
            {
                OffsetX = offsetX;
                Width = width;
                Variant = variant;
            }
        }

        // 각 장별 건물/구조물 배치 시퀀스 (한 화면 960px 내에 동일한 건축물이 절대 겹치지 않는 1860~1960px 주기)
        private static readonly ArchSegment[] Stage1Layout =
        {
            new(0, 320, 0),    // 2층 비단 포목전
            new(320, 240, 1),  // 초가 주막·떡집 노점
            new(560, 200, 2),  // 기와 토담 + 우물 + 감나무
            new(760, 320, 3),  // 한옥 보제약방
            new(1080, 230, 4), // 솟을대문 & 2층 목조 망루
            new(1310, 310, 5), // 초가 대장간
            new(1620, 300, 6)  // 맞배지붕 미곡상
        };

        private static readonly ArchSegment[] Stage2Layout =
        {
            new(0, 340, 0),    // 청운서당 강학당
            new(340, 220, 1),  // 기와 담장 + 대나무 숲
            new(560, 250, 2),  // 맞배지붕 묵향 서책방
            new(810, 220, 3),  // 솟을대문 + 고목 은행나무 + 하마비
            new(1030, 270, 4), // 초가 훈장 사택
            new(1300, 280, 5), // 2층 장서각 누각
            new(1580, 300, 6)  // 장독대 안마당 & 기와 별채
        };

        private static readonly ArchSegment[] Stage3Layout =
        {
            new(0, 310, 0),    // 광통교 큰 홍예 석교
            new(310, 250, 1),  // 빨래터 돌계단 & 수표석
            new(560, 320, 2),  // 수변 정자 누각 (청풍정)
            new(880, 240, 3),  // 낮은 석축 + 수양버들 + 석등
            new(1120, 340, 4), // 쌍홍예 수문 석축
            new(1460, 190, 5), // 선착장 목책 & 촛대 석등
            new(1650, 210, 6)  // 강변 나루터 기와 쉼터
        };

        private static readonly ArchSegment[] Stage4Layout =
        {
            new(0, 340, 0),    // 남산 치성 & 목조 망루
            new(340, 280, 1),  // 성곽 여장 & 암반 노송
            new(620, 300, 2),  // 홍예 암문 성문 누각
            new(920, 260, 3),  // 남산 봉수대 (타오르는 봉화)
            new(1180, 340, 4), // 이중 화로 여장 & 목책 방패
            new(1520, 340, 5)  // 계단식 성벽 & 초소 정자
        };

        private static readonly ArchSegment[] Stage5Layout =
        {
            new(0, 360, 0),    // 2층 대형 주막 (만월루)
            new(360, 230, 1),  // 국밥 노천 가판 & 김 나는 가마솥
            new(590, 260, 2),  // 초가 막걸리집 (달빛주막)
            new(850, 230, 3),  // 홍등가 장대 & 달빛 버드나무 담장
            new(1080, 290, 4), // 기와 객잔 (안성객잔)
            new(1370, 250, 5), // 야간 전당포 & 촛불 창문
            new(1620, 280, 6)  // 2층 풍류 누각 (취선각)
        };

        private static readonly ArchSegment[] Stage6Layout =
        {
            new(0, 380, 0),    // 병조 3간 솟을삼문 정문
            new(380, 300, 1),  // 백회 관청 행랑채
            new(680, 230, 2),  // 하마비 & 정료대 광장
            new(910, 360, 3),  // 의금부 2층 누각
            new(1270, 300, 4), // 한성부 정청 전각
            new(1570, 350, 5)  // 관아 담장 & 심야 은행나무 가로수
        };

        private static readonly ArchSegment[] Stage7Layout =
        {
            new(0, 310, 0),    // 해태 석상 & 월대 장대석 + 복사꽃
            new(310, 460, 1),  // 광화문 3홍예 성문 (랜드마크)
            new(770, 250, 2),  // 동십자각 각루 망루
            new(1020, 300, 3), // 궁장 담장 & 분홍 벚나무 가로수
            new(1320, 330, 4), // 금위영 수문장청
            new(1650, 310, 5)  // 어도 외삼문 & 횃불 정료대
        };

        private static readonly ArchSegment[] Stage8Layout =
        {
            new(0, 320, 0),    // 서행각 회랑 열주
            new(320, 240, 1),  // 품계석 광장 & 청동 드므
            new(560, 520, 2),  // 근정전 2층 대웅전 + 이중 월대 (랜드마크)
            new(1080, 280, 3), // 동행각 회랑 & 어전 북/향로
            new(1360, 320, 4), // 경회루풍 연못 2층 누각
            new(1680, 280, 5)  // 사정전 측각 & 고목 소나무
        };

        private static readonly ArchSegment[] Stage9Layout =
        {
            new(0, 420, 0),    // 집현전 본관 서고 전각 (집현전)
            new(420, 280, 1),  // 간의대 천문 관측대 & 혼천의
            new(700, 340, 2),  // 2층 천추전 누각 (천추전)
            new(1040, 300, 3), // 금서고 회랑 & 고목 회화나무
            new(1340, 300, 4), // 석연지 궁장 & 쌍석등
            new(1640, 320, 5)  // 만권당 맞배 측각 & 깃발
        };

        private static (ArchSegment[] segments, int cycleW) GetStageLayout(int stage) => stage switch
        {
            1 => (Stage1Layout, 1920),
            2 => (Stage2Layout, 1880),
            3 => (Stage3Layout, 1860),
            4 => (Stage4Layout, 1860),
            5 => (Stage5Layout, 1900),
            6 => (Stage6Layout, 1920),
            7 => (Stage7Layout, 1960),
            8 => (Stage8Layout, 1960),
            9 => (Stage9Layout, 1960),
            _ => (Stage1Layout, 1920)
        };

        public static void RenderMidground(PixelBuffer buffer, float cameraX, int stage, int groundY)
        {
            var g = buffer.Graphics;
            float archCam = cameraX * 0.6f * PixelBuffer.Scale;
            var (segments, cycleW) = GetStageLayout(stage);

            int firstCycle = (int)Math.Floor(archCam / cycleW) - 1;
            int lastCycle = (int)Math.Floor((archCam + PixelBuffer.Width) / cycleW) + 1;

            for (int c = firstCycle; c <= lastCycle; c++)
            {
                int cycleOriginX = c * cycleW - (int)archCam;
                foreach (var seg in segments)
                {
                    int bx = cycleOriginX + seg.OffsetX;
                    // 화면 밖(지붕 처마 여유 ±60px 포함)에 완전히 벗어난 건물은 건너뜀
                    if (bx + seg.Width < -70 || bx > PixelBuffer.Width + 70)
                        continue;

                    int seed = Math.Abs(c * 31 + seg.Variant * 17 + stage * 13);
                    RenderSegment(buffer, g, stage, seg.Variant, bx, seg.Width, groundY, seed);
                }
            }
        }

        private static void RenderSegment(PixelBuffer buffer, Graphics g, int stage, int variant, int bx, int w, int groundY, int seed)
        {
            switch (stage)
            {
                case 1: RenderStage1Market(buffer, g, variant, bx, w, groundY, seed); break;
                case 2: RenderStage2Seodang(buffer, g, variant, bx, w, groundY, seed); break;
                case 3: RenderStage3Cheonggye(buffer, g, variant, bx, w, groundY, seed); break;
                case 4: RenderStage4Fortress(buffer, g, variant, bx, w, groundY, seed); break;
                case 5: RenderStage5Tavern(buffer, g, variant, bx, w, groundY, seed); break;
                case 6: RenderStage6Ministries(buffer, g, variant, bx, w, groundY, seed); break;
                case 7: RenderStage7Gwanghwamun(buffer, g, variant, bx, w, groundY, seed); break;
                case 8: RenderStage8Geunjeong(buffer, g, variant, bx, w, groundY, seed); break;
                case 9: RenderStage9Jiphyeonjeon(buffer, g, variant, bx, w, groundY, seed); break;
                default: RenderStage1Market(buffer, g, variant, bx, w, groundY, seed); break;
            }
        }

        // =========================================================================
        // 1장: 한양 저잣거리 (다채로운 상가, 초가 노점, 우물 정원, 망루, 대장간)
        // =========================================================================
        private static void RenderStage1Market(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 2층 비단 포목전
                {
                    int by = gy - 210;
                    DrawStonePlinth(g, bx + 12, gy, w - 24, 16);
                    DrawWallAndPillars(g, bx + 16, by + 88, w - 32, 108, Color.FromArgb(230, 212, 182), Color.FromArgb(110, 58, 24), 3);
                    // 1층 열린 진열대 & 오색 비단 필목
                    DrawDisplayShelf(g, bx + 42, gy - 68, 95, 52, true);
                    DrawChanghojiWindow(g, bx + 180, gy - 84, 72, 62, false, false);
                    // 1층 차양 처마
                    DrawTileRoof(g, bx + 6, by + 76, w - 12, 34, Color.FromArgb(46, 50, 62), Color.FromArgb(210, 170, 55), true);
                    // 2층 본체 & 세살문
                    DrawWallAndPillars(g, bx + 34, by + 26, w - 68, 60, Color.FromArgb(236, 222, 196), Color.FromArgb(100, 52, 20), 3);
                    DrawChanghojiWindow(g, bx + 66, by + 40, 56, 40, false, false);
                    DrawChanghojiWindow(g, bx + 190, by + 40, 56, 40, false, false);
                    DrawTileRoof(g, bx + 18, by - 4, w - 36, 46, Color.FromArgb(40, 44, 56), Color.FromArgb(215, 175, 55), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 34, by + 88, 68, 22, "포목전", Color.FromArgb(72, 38, 16), Color.FromArgb(255, 232, 155));
                    DrawCheongsachorong(g, bx + w - 28, by + 114, false);
                    break;
                }
                case 1: // 초가 떡집·주막 노점
                {
                    int by = gy - 146;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 12);
                    DrawWallAndPillars(g, bx + 18, by + 42, w - 36, 94, Color.FromArgb(218, 198, 162), Color.FromArgb(108, 64, 30), 2);
                    // 노천 차양 & 떡 시루/항아리
                    DrawClothAwning(g, bx + 28, by + 56, w - 56, 20, Color.FromArgb(175, 65, 50), Color.FromArgb(235, 220, 190));
                    DrawJarCluster(g, bx + 38, gy - 12, 3);
                    DrawWoodenBench(g, bx + 130, gy - 28, 68, 16);
                    DrawThatchedRoof(g, bx + 4, by, w - 8, 54, Color.FromArgb(186, 152, 88), Color.FromArgb(148, 114, 58));
                    // 처마 밑 말린 고추 꾸러미
                    DrawHangingPeppers(g, bx + 44, by + 50, 4);
                    DrawSignboard(buffer, g, bx + w / 2 - 28, by + 34, 56, 18, "주막집", Color.FromArgb(90, 52, 22), Color.FromArgb(245, 225, 180));
                    break;
                }
                case 2: // 기와 토담 + 우물 + 가을 감나무
                {
                    DrawNaturalTree(g, bx + 142, gy - 20, TreeKind.Persimmon, 1.05f, false);
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 72, Color.FromArgb(196, 178, 146), Color.FromArgb(52, 56, 68));
                    DrawStoneWell(g, bx + 36, gy, 54);
                    break;
                }
                case 3: // 한옥 보제약방 (넓은 약방)
                {
                    int by = gy - 178;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 16);
                    DrawWallAndPillars(g, bx + 20, by + 48, w - 40, 114, Color.FromArgb(235, 224, 200), Color.FromArgb(98, 50, 22), 3);
                    DrawChanghojiWindow(g, bx + 46, by + 68, 70, 66, false, false);
                    DrawDisplayShelf(g, bx + 188, gy - 84, 92, 68, false); // 약재함
                    DrawTileRoof(g, bx - 4, by + 4, w + 8, 56, Color.FromArgb(44, 52, 64), Color.FromArgb(210, 175, 65), true);
                    DrawSignboard(buffer, g, bx + 126, by + 52, 66, 22, "보제약방", Color.FromArgb(35, 65, 48), Color.FromArgb(245, 235, 195));
                    DrawCheongsachorong(g, bx + 30, by + 62, false);
                    break;
                }
                case 4: // 솟을대문 & 2층 목조 망루 누각
                {
                    int by = gy - 232;
                    // 하단 기단 & 홍살/대문 기둥
                    DrawStonePlinth(g, bx + 24, gy, w - 48, 22);
                    using (var colBrush = new SolidBrush(Color.FromArgb(105, 52, 22)))
                    using (var gateBrush = new SolidBrush(Color.FromArgb(132, 34, 30)))
                    {
                        g.FillRectangle(colBrush, bx + 36, by + 98, 18, 114);
                        g.FillRectangle(colBrush, bx + w - 54, by + 98, 18, 114);
                        g.FillRectangle(gateBrush, bx + 54, gy - 104, w - 108, 84);
                    }
                    // 2층 목조 망루 및 계자난간
                    DrawPavilionUpper(g, bx + 28, by + 34, w - 56, 74, Color.FromArgb(128, 66, 28), Color.FromArgb(88, 42, 16), false);
                    DrawTileRoof(g, bx + 10, by - 4, w - 20, 48, Color.FromArgb(38, 42, 54), Color.FromArgb(220, 180, 60), true);
                    break;
                }
                case 5: // 초가 대장간
                {
                    int by = gy - 154;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 14);
                    DrawWallAndPillars(g, bx + 20, by + 44, w - 40, 96, Color.FromArgb(165, 152, 138), Color.FromArgb(72, 45, 25), 3);
                    // 대장간 아궁이 화로 불빛
                    using (var hearth = new SolidBrush(Color.FromArgb(42, 36, 34)))
                    using (var ember = new SolidBrush(Color.FromArgb(255, 120, 30)))
                    using (var core = new SolidBrush(Color.FromArgb(255, 225, 95)))
                    {
                        g.FillRectangle(hearth, bx + 56, gy - 68, 64, 54);
                        g.FillEllipse(ember, bx + 68, gy - 50, 40, 28);
                        g.FillEllipse(core, bx + 78, gy - 42, 20, 16);
                    }
                    DrawChanghojiWindow(g, bx + 186, by + 64, 72, 54, false, false);
                    DrawThatchedRoof(g, bx + 2, by, w - 4, 56, Color.FromArgb(170, 138, 80), Color.FromArgb(130, 102, 54));
                    DrawSignboard(buffer, g, bx + w / 2 - 28, by + 38, 56, 20, "대장간", Color.FromArgb(50, 35, 25), Color.FromArgb(255, 210, 120));
                    break;
                }
                default: // 6: 맞배지붕 미곡상
                {
                    int by = gy - 172;
                    DrawStonePlinth(g, bx + 16, gy, w - 32, 16);
                    DrawWallAndPillars(g, bx + 22, by + 46, w - 44, 110, Color.FromArgb(228, 214, 185), Color.FromArgb(95, 52, 24), 3);
                    DrawClothAwning(g, bx + 38, gy - 88, 120, 22, Color.FromArgb(55, 95, 135), Color.FromArgb(230, 228, 218));
                    DrawChanghojiWindow(g, bx + 186, by + 66, 68, 60, false, false);
                    DrawGableRoof(g, bx + 6, by + 6, w - 12, 48, Color.FromArgb(46, 50, 62), Color.FromArgb(120, 74, 34));
                    DrawSignboard(buffer, g, bx + w / 2 - 30, by + 44, 60, 20, "미곡상", Color.FromArgb(80, 45, 18), Color.FromArgb(255, 235, 175));
                    break;
                }
            }
        }

        // =========================================================================
        // 2장: 서당 뒷골목 (서당, 대나무 담장, 서책방, 은행나무 대문, 초가 사택, 장서각, 장독대)
        // =========================================================================
        private static void RenderStage2Seodang(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 청운서당 강학당
                {
                    int by = gy - 188;
                    DrawStonePlinth(g, bx + 12, gy, w - 24, 24);
                    // 대청마루 나무 난간
                    using (var maruBrush = new SolidBrush(Color.FromArgb(148, 92, 44)))
                    {
                        g.FillRectangle(maruBrush, bx + 18, gy - 34, w - 36, 12);
                    }
                    DrawWallAndPillars(g, bx + 20, by + 48, w - 40, 108, Color.FromArgb(238, 228, 204), Color.FromArgb(98, 52, 22), 4);
                    DrawChanghojiWindow(g, bx + 44, by + 66, 74, 68, false, false);
                    DrawChanghojiWindow(g, bx + 134, by + 66, 74, 68, false, false);
                    DrawChanghojiWindow(g, bx + 224, by + 66, 74, 68, false, false);
                    DrawTileRoof(g, bx - 6, by + 2, w + 12, 58, Color.FromArgb(42, 46, 58), Color.FromArgb(205, 170, 65), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 42, by + 48, 84, 24, "청운서당", Color.FromArgb(30, 48, 62), Color.FromArgb(250, 235, 180));
                    break;
                }
                case 1: // 기와 담장 & 푸른 대나무 숲
                {
                    DrawNaturalTree(g, bx + 42, gy - 18, TreeKind.Bamboo, 1.0f, false);
                    DrawNaturalTree(g, bx + 130, gy - 18, TreeKind.Bamboo, 1.15f, false);
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 84, Color.FromArgb(215, 206, 188), Color.FromArgb(48, 52, 64));
                    break;
                }
                case 2: // 맞배지붕 묵향 서책방
                {
                    int by = gy - 174;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 16);
                    DrawWallAndPillars(g, bx + 18, by + 44, w - 36, 114, Color.FromArgb(228, 218, 195), Color.FromArgb(88, 46, 20), 3);
                    DrawDisplayShelf(g, bx + 42, gy - 88, 88, 68, false); // 서책 진열
                    DrawChanghojiWindow(g, bx + 150, by + 66, 66, 62, false, false);
                    DrawGableRoof(g, bx + 4, by + 4, w - 8, 48, Color.FromArgb(45, 50, 62), Color.FromArgb(128, 78, 38));
                    DrawSignboard(buffer, g, bx + w / 2 - 30, by + 42, 60, 20, "서책방", Color.FromArgb(55, 42, 26), Color.FromArgb(245, 230, 185));
                    break;
                }
                case 3: // 솟을대문 & 고목 은행나무 & 하마비
                {
                    DrawNaturalTree(g, bx + 156, gy - 12, TreeKind.Ginkgo, 1.15f, false);
                    // 솟을대문
                    int gx = bx + 16;
                    int gw = 120;
                    int by = gy - 184;
                    DrawStonePlinth(g, gx, gy, gw, 16);
                    using (var gateWall = new SolidBrush(Color.FromArgb(196, 182, 152)))
                    using (var doorBrush = new SolidBrush(Color.FromArgb(105, 46, 24)))
                    {
                        g.FillRectangle(gateWall, gx + 10, by + 48, gw - 20, 120);
                        g.FillRectangle(doorBrush, gx + 28, gy - 102, gw - 56, 86);
                    }
                    DrawGableRoof(g, gx - 6, by + 4, gw + 12, 48, Color.FromArgb(42, 46, 58), Color.FromArgb(115, 65, 28));
                    DrawStoneStele(g, bx + 168, gy, 24, 58);
                    break;
                }
                case 4: // 초가 훈장 사택
                {
                    int by = gy - 148;
                    DrawStonePlinth(g, bx + 12, gy, w - 24, 14);
                    DrawWallAndPillars(g, bx + 18, by + 42, w - 36, 92, Color.FromArgb(224, 208, 178), Color.FromArgb(102, 58, 26), 3);
                    DrawChanghojiWindow(g, bx + 48, by + 58, 64, 52, false, false);
                    DrawChanghojiWindow(g, bx + 158, by + 58, 64, 52, false, false);
                    DrawThatchedRoof(g, bx + 2, by, w - 4, 52, Color.FromArgb(182, 150, 90), Color.FromArgb(142, 112, 60));
                    break;
                }
                case 5: // 2층 장서각 누각
                {
                    int by = gy - 224;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 20);
                    DrawWallAndPillars(g, bx + 24, by + 102, w - 48, 102, Color.FromArgb(182, 64, 48), Color.FromArgb(95, 28, 22), 3);
                    DrawChanghojiWindow(g, bx + 64, gy - 88, 62, 62, false, false);
                    DrawChanghojiWindow(g, bx + 156, gy - 88, 62, 62, false, false);
                    DrawTileRoof(g, bx + 6, by + 82, w - 12, 34, Color.FromArgb(40, 45, 58), Color.FromArgb(205, 175, 65), true);
                    DrawPavilionUpper(g, bx + 30, by + 32, w - 60, 58, Color.FromArgb(158, 48, 38), Color.FromArgb(88, 22, 18), false);
                    DrawTileRoof(g, bx + 12, by - 4, w - 24, 46, Color.FromArgb(36, 40, 52), Color.FromArgb(215, 180, 60), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 32, by + 84, 64, 20, "장서각", Color.FromArgb(35, 45, 65), Color.FromArgb(255, 228, 140));
                    break;
                }
                default: // 6: 장독대 안마당 & 기와 별채
                {
                    DrawCourtyardWall(g, bx + 6, gy, 150, 68, Color.FromArgb(208, 194, 165), Color.FromArgb(48, 52, 64));
                    DrawJarCluster(g, bx + 26, gy - 18, 4); // 장독대 항아리들
                    int ax = bx + 152;
                    int aw = w - 158;
                    int by = gy - 166;
                    DrawStonePlinth(g, ax + 6, gy, aw - 12, 14);
                    DrawWallAndPillars(g, ax + 10, by + 44, aw - 20, 108, Color.FromArgb(232, 220, 195), Color.FromArgb(96, 52, 22), 2);
                    DrawChanghojiWindow(g, ax + 34, by + 64, 68, 60, false, false);
                    DrawTileRoof(g, ax - 4, by + 4, aw + 8, 50, Color.FromArgb(42, 46, 58), Color.FromArgb(205, 170, 60), false);
                    break;
                }
            }
        }

        // =========================================================================
        // 3장: 청계천 다리 (석교, 빨래터, 정자, 수양버들, 쌍홍예, 나루터)
        // =========================================================================
        private static void RenderStage3Cheonggye(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 광통교 큰 홍예 석교
                {
                    int py = gy - 142;
                    DrawStoneEmbankment(g, bx, py + 24, w, 118, true, true, 92);
                    DrawBridgeCoping(g, bx, py + 10, w, true);
                    break;
                }
                case 1: // 빨래터 돌계단 & 수표석 (낮은 석축)
                {
                    int py = gy - 102;
                    DrawStoneEmbankment(g, bx, py + 16, w, 86, false, false, 0);
                    // 내려가는 돌계단 실루엣
                    using (var stepBrush = new SolidBrush(Color.FromArgb(165, 170, 178)))
                    using (var linePen = new Pen(Color.FromArgb(95, 100, 108), 1.5f))
                    {
                        for (int s = 0; s < 5; s++)
                        {
                            int sy = gy - 75 + s * 15;
                            int sx = bx + 28 + s * 16;
                            g.FillRectangle(stepBrush, sx, sy, 90 - s * 10, 15);
                            g.DrawRectangle(linePen, sx, sy, 90 - s * 10, 15);
                        }
                    }
                    // 수표석 돌기둥
                    DrawStoneStele(g, bx + w - 72, gy, 22, 92);
                    break;
                }
                case 2: // 수변 정자 누각 (청풍정)
                {
                    int py = gy - 122;
                    DrawStoneEmbankment(g, bx, py + 18, w, 104, false, false, 0);
                    DrawBridgeCoping(g, bx, py + 6, w, false);
                    // 정자 상부
                    int px = bx + 36;
                    int pw = w - 72;
                    int topY = gy - 236;
                    DrawPavilionUpper(g, px, topY + 46, pw, 72, Color.FromArgb(148, 52, 42), Color.FromArgb(96, 30, 22), false);
                    DrawTileRoof(g, px - 18, topY + 2, pw + 36, 54, Color.FromArgb(38, 44, 56), Color.FromArgb(225, 185, 65), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 32, topY + 48, 64, 20, "청풍정", Color.FromArgb(30, 42, 65), Color.FromArgb(255, 228, 145));
                    break;
                }
                case 3: // 낮은 석축 + 수양버들 + 석등
                {
                    DrawNaturalTree(g, bx + 110, gy - 74, TreeKind.Willow, 1.15f, false);
                    int py = gy - 96;
                    DrawStoneEmbankment(g, bx, py + 14, w, 82, false, false, 0);
                    DrawBridgeCoping(g, bx, py + 2, w, false);
                    DrawStoneLantern(g, bx + w - 48, py + 2, true);
                    break;
                }
                case 4: // 쌍홍예 수문 석축
                {
                    int py = gy - 134;
                    DrawStoneEmbankment(g, bx, py + 22, w, 112, false, false, 0);
                    // 2개의 작은 수문 아치 & 쇠창살
                    for (int a = 0; a < 2; a++)
                    {
                        int ax = bx + 54 + a * 162;
                        using var archB = new SolidBrush(Color.FromArgb(26, 32, 42));
                        using var barP = new Pen(Color.FromArgb(70, 74, 82), 2f);
                        g.FillRectangle(archB, ax, py + 64, 64, 70);
                        g.FillPie(archB, ax, py + 36, 64, 56, 180, 180);
                        for (int barX = ax + 14; barX < ax + 60; barX += 14)
                            g.DrawLine(barP, barX, py + 48, barX, gy);
                    }
                    DrawBridgeCoping(g, bx, py + 10, w, true);
                    break;
                }
                case 5: // 선착장 목책 & 촛대 석등
                {
                    int py = gy - 92;
                    DrawStoneEmbankment(g, bx, py + 12, w, 80, false, false, 0);
                    using (var woodB = new SolidBrush(Color.FromArgb(105, 68, 34)))
                    {
                        for (int px = bx + 20; px < bx + w - 20; px += 28)
                            g.FillRectangle(woodB, px, gy - 58, 10, 58);
                        g.FillRectangle(woodB, bx + 14, gy - 44, w - 28, 8);
                    }
                    DrawStoneLantern(g, bx + w / 2, py + 12, true);
                    break;
                }
                default: // 6: 강변 나루터 기와 쉼터
                {
                    int by = gy - 172;
                    DrawStonePlinth(g, bx + 10, gy, w - 20, 22);
                    DrawPavilionUpper(g, bx + 22, by + 48, w - 44, 102, Color.FromArgb(128, 68, 30), Color.FromArgb(88, 42, 18), false);
                    DrawTileRoof(g, bx, by + 6, w, 50, Color.FromArgb(44, 48, 60), Color.FromArgb(215, 175, 65), false);
                    DrawCheongsachorong(g, bx + w - 34, by + 60, false);
                    break;
                }
            }
        }

        // =========================================================================
        // 4장: 남산 성곽길 (치성 망루, 노송 암반, 암문 성문, 봉수대, 이중 화로, 계단 성벽)
        // =========================================================================
        private static void RenderStage4Fortress(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 남산 치성 & 2층 목조 망루
                {
                    int wy = gy - 155;
                    DrawFortressWallBase(g, bx, wy, w, 155, true, false);
                    // 상부 목조 감시 망루
                    int tx = bx + 58;
                    int tw = w - 116;
                    DrawPavilionUpper(g, tx, wy - 72, tw, 72, Color.FromArgb(108, 52, 26), Color.FromArgb(68, 32, 16), true);
                    DrawTileRoof(g, tx - 16, wy - 110, tw + 32, 46, Color.FromArgb(36, 40, 52), Color.FromArgb(205, 165, 60), true);
                    DrawTorchBrazier(g, bx + 32, wy + 22, seed);
                    break;
                }
                case 1: // 낮은 성곽 여장 & 암반 노송 군락
                {
                    DrawNaturalTree(g, bx + 85, gy - 90, TreeKind.Pine, 1.15f, false);
                    DrawNaturalTree(g, bx + 205, gy - 84, TreeKind.Pine, 0.95f, false);
                    int wy = gy - 122;
                    DrawFortressWallBase(g, bx, wy, w, 122, true, true);
                    break;
                }
                case 2: // 홍예 암문 성문 누각
                {
                    int wy = gy - 175;
                    DrawFortressWallBase(g, bx, wy + 18, w, 157, false, false);
                    // 중앙 홍예 성문 & 쇠징 박힌 대문
                    int ax = bx + w / 2 - 46;
                    using (var archB = new SolidBrush(Color.FromArgb(22, 18, 20)))
                    using (var doorB = new SolidBrush(Color.FromArgb(128, 32, 28)))
                    using (var studB = new SolidBrush(Color.FromArgb(185, 175, 160)))
                    {
                        g.FillRectangle(archB, ax, gy - 96, 92, 96);
                        g.FillPie(archB, ax, gy - 134, 92, 76, 180, 180);
                        g.FillRectangle(doorB, ax + 10, gy - 92, 72, 92);
                        for (int sx = ax + 22; sx < ax + 78; sx += 18)
                            for (int sy = gy - 78; sy < gy - 14; sy += 22)
                                g.FillEllipse(studB, sx, sy, 4, 4);
                    }
                    DrawTileRoof(g, bx + 16, wy - 28, w - 32, 52, Color.FromArgb(34, 38, 50), Color.FromArgb(215, 175, 55), true);
                    DrawTorchBrazier(g, bx + 38, wy + 44, seed + 1);
                    DrawTorchBrazier(g, bx + w - 38, wy + 44, seed + 2);
                    break;
                }
                case 3: // 남산 봉수대 (연기와 맹렬한 봉화)
                {
                    int wy = gy - 140;
                    DrawFortressWallBase(g, bx, wy + 25, w, 115, false, false);
                    // 계단식 돌 연대 & 거대 화구 불꽃
                    using (var stoneB = new SolidBrush(Color.FromArgb(118, 110, 102)))
                    using (var darkPen = new Pen(Color.FromArgb(65, 58, 52), 2f))
                    {
                        g.FillRectangle(stoneB, bx + 44, wy - 18, w - 88, 44);
                        g.DrawRectangle(darkPen, bx + 44, wy - 18, w - 88, 44);
                        g.FillRectangle(stoneB, bx + 78, wy - 62, w - 156, 44);
                        g.DrawRectangle(darkPen, bx + 78, wy - 62, w - 156, 44);
                    }
                    // 피어오르는 연기 & 강렬한 봉화
                    float flk = (float)(Math.Sin(DateTime.UtcNow.Millisecond * 0.015f + seed) * 5);
                    using (var smoke = new SolidBrush(Color.FromArgb(80, 140, 130, 145)))
                    using (var outerF = new SolidBrush(Color.FromArgb(145, 255, 95, 25)))
                    using (var innerF = new SolidBrush(Color.FromArgb(255, 225, 85)))
                    {
                        g.FillEllipse(smoke, bx + w / 2 - 42, wy - 148 + flk, 84, 52);
                        g.FillEllipse(smoke, bx + w / 2 - 26, wy - 186 - flk, 68, 44);
                        g.FillEllipse(outerF, bx + w / 2 - 32, wy - 102 + flk, 64, 50);
                        g.FillEllipse(innerF, bx + w / 2 - 16, wy - 90 + flk, 32, 34);
                    }
                    break;
                }
                case 4: // 이중 화로 여장 & 방패 목책
                {
                    int wy = gy - 152;
                    DrawFortressWallBase(g, bx, wy, w, 152, true, false);
                    DrawTorchBrazier(g, bx + 84, wy + 46, seed + 3);
                    DrawTorchBrazier(g, bx + w - 84, wy + 46, seed + 4);
                    break;
                }
                default: // 5: 경사 여장 & 경비 초소
                {
                    int wy = gy - 138;
                    DrawFortressWallBase(g, bx, wy, w, 138, true, true);
                    DrawPavilionUpper(g, bx + w - 148, wy - 62, 118, 62, Color.FromArgb(100, 56, 28), Color.FromArgb(62, 34, 16), true);
                    DrawGableRoof(g, bx + w - 160, wy - 96, 142, 42, Color.FromArgb(38, 42, 54), Color.FromArgb(95, 55, 24));
                    break;
                }
            }
        }

        // =========================================================================
        // 5장: 도성 주막거리 (만월루 2층, 국밥 노천 가판, 초가 주막, 홍등가 담장, 객잔, 전당포, 풍류 누각)
        // =========================================================================
        private static void RenderStage5Tavern(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 만월루 2층 대형 기와 주막
                {
                    int by = gy - 222;
                    DrawStonePlinth(g, bx + 12, gy, w - 24, 16);
                    DrawWallAndPillars(g, bx + 18, by + 98, w - 36, 108, Color.FromArgb(165, 52, 44), Color.FromArgb(95, 26, 22), 4);
                    DrawChanghojiWindow(g, bx + 48, gy - 90, 74, 62, true, false);
                    DrawChanghojiWindow(g, bx + 144, gy - 90, 74, 62, true, false);
                    DrawChanghojiWindow(g, bx + 240, gy - 90, 74, 62, true, false);
                    DrawTileRoof(g, bx + 2, by + 84, w - 4, 34, Color.FromArgb(30, 34, 46), Color.FromArgb(220, 178, 62), true);
                    DrawPavilionUpper(g, bx + 32, by + 32, w - 64, 58, Color.FromArgb(172, 58, 46), Color.FromArgb(90, 24, 20), false);
                    DrawChanghojiWindow(g, bx + 76, by + 38, 66, 34, true, false);
                    DrawChanghojiWindow(g, bx + 216, by + 38, 66, 34, true, false);
                    DrawTileRoof(g, bx + 14, by - 4, w - 28, 46, Color.FromArgb(26, 30, 42), Color.FromArgb(228, 185, 65), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 34, by + 88, 68, 22, "만월루", Color.FromArgb(55, 18, 16), Color.FromArgb(255, 225, 120));
                    DrawCheongsachorong(g, bx + 26, by + 114, true);
                    DrawCheongsachorong(g, bx + w - 26, by + 114, true);
                    break;
                }
                case 1: // 국밥 노천 가판 & 모락모락 가마솥 굴뚝
                {
                    int by = gy - 142;
                    // 뒤쪽 흙벽 & 굴뚝
                    using (var wallB = new SolidBrush(Color.FromArgb(135, 118, 98)))
                    using (var chimB = new SolidBrush(Color.FromArgb(95, 82, 68)))
                    {
                        g.FillRectangle(wallB, bx + 22, by + 34, w - 44, 108);
                        g.FillRectangle(chimB, bx + w - 58, by - 28, 24, 168);
                    }
                    // 모락모락 김 & 굴뚝 연기
                    using (var steam = new SolidBrush(Color.FromArgb(55, 240, 245, 255)))
                    {
                        g.FillEllipse(steam, bx + 68, gy - 82, 46, 32);
                        g.FillEllipse(steam, bx + w - 66, by - 64, 42, 34);
                    }
                    DrawClothAwning(g, bx + 16, by + 36, w - 60, 22, Color.FromArgb(158, 46, 40), Color.FromArgb(225, 200, 145));
                    // 가마솥 & 화덕 불빛
                    using (var potB = new SolidBrush(Color.FromArgb(35, 34, 38)))
                    using (var fireB = new SolidBrush(Color.FromArgb(255, 145, 35)))
                    {
                        g.FillEllipse(potB, bx + 54, gy - 44, 62, 30);
                        g.FillEllipse(fireB, bx + 72, gy - 20, 26, 14);
                    }
                    DrawWoodenBench(g, bx + 128, gy - 28, 56, 16);
                    DrawSignboard(buffer, g, bx + 48, by + 16, 62, 18, "국밥집", Color.FromArgb(72, 36, 18), Color.FromArgb(255, 228, 150));
                    break;
                }
                case 2: // 초가 달빛주막
                {
                    int by = gy - 150;
                    DrawStonePlinth(g, bx + 12, gy, w - 24, 14);
                    DrawWallAndPillars(g, bx + 18, by + 44, w - 36, 92, Color.FromArgb(172, 150, 122), Color.FromArgb(84, 46, 22), 3);
                    DrawChanghojiWindow(g, bx + 44, by + 62, 66, 54, true, false);
                    DrawChanghojiWindow(g, bx + 148, by + 62, 66, 54, true, false);
                    DrawThatchedRoof(g, bx + 2, by, w - 4, 54, Color.FromArgb(142, 114, 70), Color.FromArgb(108, 86, 52));
                    DrawJarCluster(g, bx + 24, gy - 14, 2);
                    DrawCheongsachorong(g, bx + w - 28, by + 56, true);
                    DrawSignboard(buffer, g, bx + w / 2 - 32, by + 38, 64, 20, "달빛주막", Color.FromArgb(62, 34, 16), Color.FromArgb(255, 220, 130));
                    break;
                }
                case 3: // 홍등가 장대 & 달빛 버드나무 담장
                {
                    DrawNaturalTree(g, bx + 115, gy - 22, TreeKind.Willow, 1.05f, true);
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 74, Color.FromArgb(132, 120, 112), Color.FromArgb(32, 36, 48));
                    // 양쪽 나무 장대 + 주렁주렁 홍등 줄
                    using (var poleB = new SolidBrush(Color.FromArgb(68, 38, 20)))
                    using (var wireP = new Pen(Color.FromArgb(40, 35, 35), 1.5f))
                    {
                        g.FillRectangle(poleB, bx + 18, gy - 178, 8, 178);
                        g.FillRectangle(poleB, bx + w - 26, gy - 178, 8, 178);
                        g.DrawLine(wireP, bx + 22, gy - 165, bx + w - 22, gy - 165);
                    }
                    for (int lx = bx + 48; lx <= bx + w - 48; lx += 44)
                        DrawPaperLantern(g, lx, gy - 165, true);
                    break;
                }
                case 4: // 기와 안성객잔
                {
                    int by = gy - 184;
                    DrawStonePlinth(g, bx + 12, gy, w - 24, 16);
                    DrawWallAndPillars(g, bx + 18, by + 48, w - 36, 120, Color.FromArgb(182, 168, 146), Color.FromArgb(88, 42, 20), 3);
                    DrawChanghojiWindow(g, bx + 46, by + 68, 72, 66, true, false);
                    DrawChanghojiWindow(g, bx + 172, by + 68, 72, 66, true, false);
                    DrawTileRoof(g, bx - 4, by + 4, w + 8, 54, Color.FromArgb(32, 36, 48), Color.FromArgb(205, 168, 60), true);
                    DrawSignboard(buffer, g, bx + 114, by + 54, 62, 22, "안성객잔", Color.FromArgb(62, 26, 20), Color.FromArgb(255, 226, 145));
                    break;
                }
                case 5: // 야간 전당포 (어두운 셔터 + 촛불 작은 창)
                {
                    int by = gy - 168;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 16);
                    DrawWallAndPillars(g, bx + 20, by + 44, w - 40, 108, Color.FromArgb(118, 114, 122), Color.FromArgb(58, 36, 22), 3);
                    DrawChanghojiWindow(g, bx + 48, by + 60, 58, 36, true, false);
                    DrawGableRoof(g, bx + 4, by + 4, w - 8, 46, Color.FromArgb(30, 34, 46), Color.FromArgb(85, 52, 28));
                    DrawCheongsachorong(g, bx + w - 38, by + 56, true);
                    DrawSignboard(buffer, g, bx + 132, by + 54, 62, 20, "전당포", Color.FromArgb(40, 35, 45), Color.FromArgb(225, 210, 160));
                    break;
                }
                default: // 6: 2층 풍류 누각 (취선각)
                {
                    int by = gy - 216;
                    DrawStonePlinth(g, bx + 18, gy, w - 36, 18);
                    DrawPavilionUpper(g, bx + 24, by + 98, w - 48, 100, Color.FromArgb(112, 48, 28), Color.FromArgb(68, 26, 18), false);
                    DrawPavilionUpper(g, bx + 32, by + 32, w - 64, 66, Color.FromArgb(132, 54, 30), Color.FromArgb(74, 28, 18), false);
                    DrawTileRoof(g, bx + 10, by - 6, w - 20, 48, Color.FromArgb(28, 32, 44), Color.FromArgb(215, 175, 65), true);
                    DrawPaperLantern(g, bx + 54, by + 38, true);
                    DrawPaperLantern(g, bx + w - 54, by + 38, true);
                    break;
                }
            }
        }

        // =========================================================================
        // 6장: 육조거리 (솟을삼문 정문, 백회 행랑채, 하마비/정료대, 의금부 2층, 한성부 정청, 은행나무 담장)
        // =========================================================================
        private static void RenderStage6Ministries(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 병조 3간 솟을삼문 관아 정문
                {
                    int by = gy - 216;
                    DrawStonePlinth(g, bx + 12, gy, w - 24, 22);
                    // 좌우 측문 + 중앙 솟을대문
                    using (var wallB = new SolidBrush(Color.FromArgb(148, 38, 34)))
                    using (var colB = new SolidBrush(Color.FromArgb(95, 18, 16)))
                    using (var doorB = new SolidBrush(Color.FromArgb(118, 24, 22)))
                    {
                        g.FillRectangle(wallB, bx + 20, by + 84, w - 40, 110);
                        g.FillRectangle(doorB, bx + 48, gy - 96, 68, 74);
                        g.FillRectangle(doorB, bx + w / 2 - 48, gy - 116, 96, 94);
                        g.FillRectangle(doorB, bx + w - 116, gy - 96, 68, 74);
                        g.FillRectangle(colB, bx + 26, by + 64, 18, 130);
                        g.FillRectangle(colB, bx + 124, by + 40, 20, 154);
                        g.FillRectangle(colB, bx + w - 144, by + 40, 20, 154);
                        g.FillRectangle(colB, bx + w - 44, by + 64, 18, 130);
                    }
                    DrawTileRoof(g, bx + 4, by + 46, 126, 36, Color.FromArgb(26, 30, 42), Color.FromArgb(205, 170, 60), false);
                    DrawTileRoof(g, bx + w - 130, by + 46, 126, 36, Color.FromArgb(26, 30, 42), Color.FromArgb(205, 170, 60), false);
                    DrawTileRoof(g, bx + 102, by - 6, w - 204, 52, Color.FromArgb(24, 28, 40), Color.FromArgb(230, 190, 65), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 30, by + 46, 60, 22, "병 조", Color.FromArgb(30, 24, 28), Color.FromArgb(255, 225, 120));
                    break;
                }
                case 1: // 관청 백회 행랑채
                {
                    int by = gy - 166;
                    DrawStonePlinth(g, bx + 8, gy, w - 16, 18);
                    DrawWallAndPillars(g, bx + 14, by + 44, w - 28, 104, Color.FromArgb(195, 198, 206), Color.FromArgb(88, 28, 24), 4);
                    for (int kx = bx + 38; kx < bx + w - 58; kx += 78)
                        DrawChanghojiWindow(g, kx, by + 64, 48, 42, (kx / 78) % 2 == 0, false);
                    DrawTileRoof(g, bx - 4, by + 4, w + 8, 48, Color.FromArgb(28, 32, 44), Color.FromArgb(190, 165, 70), true);
                    break;
                }
                case 2: // 하마비 & 쌍정료대 광장
                {
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 72, Color.FromArgb(182, 184, 190), Color.FromArgb(28, 32, 44));
                    DrawStoneStele(g, bx + w / 2 - 14, gy, 28, 78);
                    DrawTorchBrazier(g, bx + 46, gy - 12, seed);
                    DrawTorchBrazier(g, bx + w - 46, gy - 12, seed + 1);
                    break;
                }
                case 3: // 의금부 2층 누각
                {
                    int by = gy - 224;
                    DrawStonePlinth(g, bx + 16, gy, w - 32, 20);
                    DrawWallAndPillars(g, bx + 22, by + 100, w - 44, 104, Color.FromArgb(148, 36, 32), Color.FromArgb(88, 18, 16), 3);
                    DrawChanghojiWindow(g, bx + 66, gy - 90, 74, 62, true, false);
                    DrawChanghojiWindow(g, bx + 220, gy - 90, 74, 62, true, false);
                    DrawTileRoof(g, bx + 4, by + 84, w - 8, 34, Color.FromArgb(24, 28, 40), Color.FromArgb(220, 180, 60), true);
                    DrawPavilionUpper(g, bx + 34, by + 32, w - 68, 58, Color.FromArgb(138, 34, 30), Color.FromArgb(80, 16, 16), false);
                    DrawTileRoof(g, bx + 14, by - 4, w - 28, 46, Color.FromArgb(22, 26, 38), Color.FromArgb(230, 190, 65), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 34, by + 86, 68, 22, "의금부", Color.FromArgb(30, 22, 24), Color.FromArgb(255, 228, 130));
                    break;
                }
                case 4: // 한성부 정청 전각
                {
                    int by = gy - 196;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 26);
                    DrawWallAndPillars(g, bx + 22, by + 50, w - 44, 120, Color.FromArgb(162, 40, 36), Color.FromArgb(92, 20, 18), 3);
                    DrawChanghojiWindow(g, bx + 56, by + 72, 68, 66, true, false);
                    DrawChanghojiWindow(g, bx + w - 124, by + 72, 68, 66, true, false);
                    DrawTileRoof(g, bx - 6, by + 4, w + 12, 56, Color.FromArgb(26, 30, 42), Color.FromArgb(225, 185, 65), true);
                    DrawSignboard(buffer, g, bx + w / 2 - 34, by + 52, 68, 22, "한성부", Color.FromArgb(34, 26, 28), Color.FromArgb(255, 230, 140));
                    break;
                }
                default: // 5: 관아 담장 & 심야 은행나무 가로수
                {
                    DrawNaturalTree(g, bx + 95, gy - 20, TreeKind.Ginkgo, 1.15f, true);
                    DrawNaturalTree(g, bx + 245, gy - 20, TreeKind.Ginkgo, 1.05f, true);
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 78, Color.FromArgb(158, 158, 168), Color.FromArgb(26, 30, 42));
                    DrawStoneLantern(g, bx + w / 2, gy - 12, true);
                    break;
                }
            }
        }

        // =========================================================================
        // 7장: 광화문 앞뜰 (해태 월대, 광화문 3홍예 성문, 동십자각, 궁장+벚꽃, 수문장청, 외삼문)
        // =========================================================================
        private static void RenderStage7Gwanghwamun(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 해태 석상 & 월대 장대석 + 복사꽃 나무
                {
                    DrawNaturalTree(g, bx + 215, gy - 20, TreeKind.Blossom, 1.15f, false);
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 76, Color.FromArgb(165, 148, 152), Color.FromArgb(24, 28, 38));
                    DrawHaetaePedestal(g, bx + 78, gy, 54, 78);
                    break;
                }
                case 1: // 광화문 3홍예 성문 (대형 랜드마크 460w)
                {
                    int by = gy - 252;
                    // 거대 화강암 성벽 기단 + 3개의 홍예문 아치
                    using (var stoneB = new SolidBrush(Color.FromArgb(158, 152, 156)))
                    using (var lineP = new Pen(Color.FromArgb(95, 88, 92), 1.6f))
                    using (var archB = new SolidBrush(Color.FromArgb(16, 12, 18)))
                    {
                        g.FillRectangle(stoneB, bx + 14, gy - 118, w - 28, 118);
                        for (int ly = gy - 96; ly < gy; ly += 22)
                            g.DrawLine(lineP, bx + 14, ly, bx + w - 14, ly);

                        int[] archCenters = { bx + 108, bx + w / 2, bx + w - 108 };
                        for (int i = 0; i < 3; i++)
                        {
                            int aw = (i == 1) ? 78 : 60;
                            int ah = (i == 1) ? 84 : 70;
                            int ax = archCenters[i] - aw / 2;
                            g.FillRectangle(archB, ax, gy - ah + 16, aw, ah - 16);
                            g.FillPie(archB, ax, gy - ah - 16, aw, 64, 180, 180);
                        }
                    }
                    // 2층 궁궐 누각 본체 + 오방색 단청 + 이중 팔작지붕 + 잡상
                    DrawPalaceTieredHall(g, bx + 42, by, w - 84, 134, true, true);
                    DrawSignboard(buffer, g, bx + w / 2 - 36, by + 64, 72, 22, "광화문", Color.FromArgb(28, 18, 22), Color.FromArgb(255, 225, 110));
                    break;
                }
                case 2: // 동십자각 각루 망루
                {
                    int by = gy - 228;
                    DrawStonePlinth(g, bx + 20, gy, w - 40, 82);
                    DrawPavilionUpper(g, bx + 32, by + 68, w - 64, 80, Color.FromArgb(148, 28, 30), Color.FromArgb(92, 12, 14), false);
                    DrawTileRoof(g, bx + 10, by + 26, w - 20, 48, Color.FromArgb(22, 24, 34), Color.FromArgb(235, 195, 60), true, 4);
                    break;
                }
                case 3: // 궁장 담장 & 분홍 벚꽃/복사꽃 가로수
                {
                    DrawNaturalTree(g, bx + 84, gy - 20, TreeKind.Blossom, 1.15f, false);
                    DrawNaturalTree(g, bx + 216, gy - 20, TreeKind.Blossom, 1.05f, false);
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 84, Color.FromArgb(154, 82, 82), Color.FromArgb(24, 26, 36));
                    break;
                }
                case 4: // 금위영 수문장청
                {
                    int by = gy - 188;
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 24);
                    DrawWallAndPillars(g, bx + 20, by + 50, w - 40, 114, Color.FromArgb(145, 26, 28), Color.FromArgb(88, 12, 14), 3);
                    DrawChanghojiWindow(g, bx + 52, by + 72, 68, 60, true, false);
                    DrawChanghojiWindow(g, bx + w - 120, by + 72, 68, 60, true, false);
                    DrawTileRoof(g, bx - 6, by + 4, w + 12, 54, Color.FromArgb(22, 24, 34), Color.FromArgb(235, 195, 65), true, 4);
                    DrawSignboard(buffer, g, bx + w / 2 - 36, by + 52, 72, 22, "수문장청", Color.FromArgb(30, 18, 20), Color.FromArgb(255, 225, 130));
                    break;
                }
                default: // 5: 어도 외삼문 & 정료대
                {
                    int by = gy - 196;
                    DrawStonePlinth(g, bx + 18, gy, w - 36, 22);
                    DrawWallAndPillars(g, bx + 26, by + 56, w - 52, 118, Color.FromArgb(138, 24, 28), Color.FromArgb(84, 10, 12), 3);
                    DrawTileRoof(g, bx + 6, by + 8, w - 12, 54, Color.FromArgb(22, 24, 34), Color.FromArgb(230, 190, 60), true);
                    DrawTorchBrazier(g, bx + 36, gy - 10, seed);
                    DrawTorchBrazier(g, bx + w - 36, gy - 10, seed + 1);
                    break;
                }
            }
        }

        // =========================================================================
        // 8장: 경복궁 근정전 (행각 회랑, 품계석 광장, 근정전 2층 대웅전, 경회루, 사정전)
        // =========================================================================
        private static void RenderStage8Geunjeong(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 서행각 회랑 열주
                case 3: // 동행각 회랑 열주
                {
                    int by = gy - 172;
                    DrawStonePlinth(g, bx + 6, gy, w - 12, 22);
                    DrawWallAndPillars(g, bx + 12, by + 46, w - 24, 104, Color.FromArgb(132, 22, 24), Color.FromArgb(82, 10, 12), 4);
                    for (int kx = bx + 36; kx < bx + w - 56; kx += 74)
                        DrawChanghojiWindow(g, kx, by + 66, 46, 52, false, true); // 붉은 달빛 창살
                    DrawTileRoof(g, bx - 4, by + 4, w + 8, 48, Color.FromArgb(20, 20, 28), Color.FromArgb(225, 175, 55), true);
                    if (v == 3) DrawTorchBrazier(g, bx + w / 2, gy - 14, seed);
                    break;
                }
                case 1: // 품계석 광장 & 청동 드므 (탁 트인 붉은 달 시야)
                {
                    DrawCourtyardWall(g, bx + 4, gy, w - 8, 52, Color.FromArgb(160, 152, 156), Color.FromArgb(24, 22, 30));
                    // 품계석 돌기둥 3열
                    for (int px = bx + 38; px <= bx + w - 58; px += 72)
                        DrawStoneStele(g, px, gy, 16, 44);
                    // 청동 드므 화로
                    using (var bronze = new SolidBrush(Color.FromArgb(62, 54, 48)))
                    using (var rim = new Pen(Color.FromArgb(155, 120, 65), 2f))
                    {
                        g.FillEllipse(bronze, bx + w / 2 - 22, gy - 26, 44, 26);
                        g.DrawEllipse(rim, bx + w / 2 - 22, gy - 26, 44, 26);
                    }
                    break;
                }
                case 2: // 근정전 중층 대웅전 + 이중 월대 (520w 궁궐 랜드마크)
                {
                    int by = gy - 268;
                    // 이중 월대 기단 & 중앙 어도 답도
                    DrawStonePlinth(g, bx + 14, gy, w - 28, 54);
                    DrawStonePlinth(g, bx + 44, gy - 54, w - 88, 28);
                    using (var ramp = new SolidBrush(Color.FromArgb(195, 188, 180)))
                    {
                        g.FillRectangle(ramp, bx + w / 2 - 42, gy - 54, 84, 54);
                    }
                    // 2층 중층 전각
                    DrawPalaceTieredHall(g, bx + 44, by, w - 88, 186, true, true);
                    DrawSignboard(buffer, g, bx + w / 2 - 38, by + 68, 76, 24, "근정전", Color.FromArgb(24, 14, 18), Color.FromArgb(255, 220, 95));
                    break;
                }
                case 4: // 경회루풍 연못 2층 누각
                {
                    int by = gy - 228;
                    // 돌기둥 교각 아래 연못 석축
                    DrawStonePlinth(g, bx + 16, gy, w - 32, 36);
                    using (var pierB = new SolidBrush(Color.FromArgb(152, 146, 152)))
                    {
                        for (int px = bx + 36; px < bx + w - 36; px += 62)
                            g.FillRectangle(pierB, px, gy - 88, 18, 52);
                    }
                    DrawPavilionUpper(g, bx + 28, by + 62, w - 56, 78, Color.FromArgb(136, 22, 24), Color.FromArgb(84, 10, 12), false);
                    DrawTileRoof(g, bx + 8, by + 16, w - 16, 52, Color.FromArgb(22, 22, 32), Color.FromArgb(235, 190, 58), true, 5);
                    DrawSignboard(buffer, g, bx + w / 2 - 32, by + 62, 64, 20, "경회루", Color.FromArgb(28, 16, 20), Color.FromArgb(255, 225, 115));
                    break;
                }
                default: // 5: 사정전 측각 & 고목 소나무
                {
                    DrawNaturalTree(g, bx + 225, gy - 18, TreeKind.Pine, 1.15f, false);
                    int ax = bx + 12;
                    int aw = 185;
                    int by = gy - 178;
                    DrawStonePlinth(g, ax, gy, aw, 20);
                    DrawWallAndPillars(g, ax + 6, by + 48, aw - 12, 110, Color.FromArgb(136, 22, 24), Color.FromArgb(82, 10, 12), 2);
                    DrawChanghojiWindow(g, ax + 34, by + 68, 62, 58, false, true);
                    DrawTileRoof(g, ax - 10, by + 4, aw + 20, 50, Color.FromArgb(20, 22, 30), Color.FromArgb(225, 180, 58), true);
                    break;
                }
            }
        }

        // =========================================================================
        // 9장: 집현전 천추전 (집현전 서고, 간의대 혼천의, 2층 천추전, 금서고 회화나무, 석연지, 만권당)
        // =========================================================================
        private static void RenderStage9Jiphyeonjeon(PixelBuffer buffer, Graphics g, int v, int bx, int w, int gy, int seed)
        {
            switch (v)
            {
                case 0: // 집현전 본관 서고 전각 (420w)
                {
                    int by = gy - 224;
                    DrawStonePlinth(g, bx + 16, gy, w - 32, 28);
                    DrawWallAndPillars(g, bx + 24, by + 56, w - 48, 140, Color.FromArgb(128, 22, 34), Color.FromArgb(74, 12, 22), 4);
                    // 황금빛으로 은은하게 빛나는 서고 창호지
                    DrawChanghojiWindow(g, bx + 54, by + 84, 72, 74, true, false);
                    DrawChanghojiWindow(g, bx + 174, by + 84, 72, 74, true, false);
                    DrawChanghojiWindow(g, bx + 294, by + 84, 72, 74, true, false);
                    DrawTileRoof(g, bx - 4, by + 6, w + 8, 58, Color.FromArgb(20, 18, 32), Color.FromArgb(245, 205, 65), true, 5);
                    DrawSignboard(buffer, g, bx + w / 2 - 42, by + 56, 84, 24, "집현전", Color.FromArgb(24, 14, 34), Color.FromArgb(255, 230, 105));
                    break;
                }
                case 1: // 간의대 천문 관측대 & 혼천의 (유니크 천문 랜드마크)
                {
                    // 계단식 석조 관측대
                    DrawStonePlinth(g, bx + 24, gy, w - 48, 86);
                    DrawStonePlinth(g, bx + 52, gy - 86, w - 104, 28);
                    // 황금빛/청동 혼천의(Armillary Sphere) 실루엣
                    int cx = bx + w / 2;
                    int cy = gy - 164;
                    using (var baseB = new SolidBrush(Color.FromArgb(70, 56, 48)))
                    using (var brassP = new Pen(Color.FromArgb(230, 190, 75), 3f))
                    using (var innerP = new Pen(Color.FromArgb(185, 145, 255), 1.8f))
                    {
                        g.FillRectangle(baseB, cx - 22, gy - 126, 44, 12);
                        g.DrawLine(brassP, cx - 16, gy - 126, cx, cy);
                        g.DrawLine(brassP, cx + 16, gy - 126, cx, cy);
                        g.DrawEllipse(brassP, cx - 40, cy - 40, 80, 80);
                        g.DrawEllipse(brassP, cx - 40, cy - 16, 80, 32);
                        g.DrawEllipse(brassP, cx - 16, cy - 40, 32, 80);
                        g.DrawEllipse(innerP, cx - 10, cy - 10, 20, 20);
                    }
                    break;
                }
                case 2: // 2층 천추전 누각
                {
                    int by = gy - 248;
                    DrawStonePlinth(g, bx + 16, gy, w - 32, 32);
                    DrawPalaceTieredHall(g, bx + 24, by, w - 48, 216, true, false);
                    DrawSignboard(buffer, g, bx + w / 2 - 36, by + 64, 72, 22, "천추전", Color.FromArgb(26, 16, 38), Color.FromArgb(255, 225, 100));
                    DrawTorchBrazier(g, bx + 34, gy - 16, seed);
                    DrawTorchBrazier(g, bx + w - 34, gy - 16, seed + 1);
                    break;
                }
                case 3: // 금서고 회랑 & 고목 회화나무
                {
                    DrawNaturalTree(g, bx + 225, gy - 20, TreeKind.Pine, 1.2f, false);
                    int by = gy - 168;
                    DrawStonePlinth(g, bx + 8, gy, 210, 20);
                    DrawWallAndPillars(g, bx + 14, by + 46, 198, 102, Color.FromArgb(118, 24, 34), Color.FromArgb(68, 12, 20), 3);
                    DrawChanghojiWindow(g, bx + 40, by + 66, 56, 54, true, false);
                    DrawChanghojiWindow(g, bx + 122, by + 66, 56, 54, true, false);
                    DrawTileRoof(g, bx, by + 4, 222, 48, Color.FromArgb(20, 18, 32), Color.FromArgb(235, 195, 60), true);
                    break;
                }
                case 4: // 석연지 궁장 & 쌍석등
                {
                    DrawCourtyardWall(g, bx + 6, gy, w - 12, 82, Color.FromArgb(112, 92, 120), Color.FromArgb(22, 20, 34));
                    DrawStoneLantern(g, bx + 68, gy - 12, true);
                    DrawStoneLantern(g, bx + w - 68, gy - 12, true);
                    break;
                }
                default: // 5: 만권당 맞배 측각 & 왕실 깃발
                {
                    int by = gy - 186;
                    DrawStonePlinth(g, bx + 18, gy, w - 36, 22);
                    DrawWallAndPillars(g, bx + 24, by + 48, w - 48, 116, Color.FromArgb(124, 24, 34), Color.FromArgb(72, 14, 22), 3);
                    DrawChanghojiWindow(g, bx + 54, by + 70, 68, 60, true, false);
                    DrawChanghojiWindow(g, bx + w - 122, by + 70, 68, 60, true, false);
                    DrawGableRoof(g, bx + 6, by + 6, w - 12, 50, Color.FromArgb(22, 20, 34), Color.FromArgb(125, 42, 48));
                    DrawSignboard(buffer, g, bx + w / 2 - 32, by + 50, 64, 20, "만권당", Color.FromArgb(26, 16, 36), Color.FromArgb(255, 228, 120));
                    break;
                }
            }
        }

        // =========================================================================
        // 공용 조선 건축 컴포넌트 (중복 없는 파라미터화 헬퍼)
        // =========================================================================

        private static void DrawStonePlinth(Graphics g, int x, int groundY, int w, int h)
        {
            int y = groundY - h;
            using var baseB = new SolidBrush(Color.FromArgb(162, 156, 148));
            using var lineP = new Pen(Color.FromArgb(108, 102, 96), 1.3f);
            g.FillRectangle(baseB, x, y, w, h);
            g.DrawRectangle(lineP, x, y, w, h);
            for (int sx = x + 36; sx < x + w - 10; sx += 42)
                g.DrawLine(lineP, sx, y, sx, groundY);
        }

        private static void DrawWallAndPillars(Graphics g, int x, int y, int w, int h, Color wallCol, Color woodCol, int pillarCount)
        {
            using (var wallB = new SolidBrush(wallCol))
            {
                g.FillRectangle(wallB, x, y, w, h);
            }
            using (var woodB = new SolidBrush(woodCol))
            using (var darkWood = new SolidBrush(Color.FromArgb(Math.Max(0, woodCol.R - 30), Math.Max(0, woodCol.G - 20), Math.Max(0, woodCol.B - 15))))
            {
                g.FillRectangle(woodB, x - 2, y - 4, w + 4, 11);
                g.FillRectangle(darkWood, x - 2, y + 4, w + 4, 4);
                int pw = 16;
                for (int p = 0; p < pillarCount; p++)
                {
                    int px = (pillarCount <= 1) ? x : x + (w - pw) * p / (pillarCount - 1);
                    g.FillRectangle(woodB, px, y, pw, h);
                }
            }
        }

        private static void DrawTileRoof(Graphics g, int x, int y, int w, int h, Color roofCol, Color eaveGold, bool hasDancheong, int japsangCount = 0)
        {
            if (hasDancheong)
            {
                using var gB = new SolidBrush(Color.FromArgb(52, 128, 88));
                using var rB = new SolidBrush(Color.FromArgb(172, 46, 44));
                for (int rx = x + 8; rx <= x + w - 14; rx += 14)
                    g.FillRectangle(((rx / 14) % 2 == 0) ? gB : rB, rx, y + h - 8, 8, 8);
            }

            Point[] pts =
            {
                new Point(x - 10, y + h - 6),
                new Point(x + w / 7, y + h / 2),
                new Point(x + w / 2, y),
                new Point(x + w - w / 7, y + h / 2),
                new Point(x + w + 10, y + h - 6),
                new Point(x + w, y + h + 4),
                new Point(x + w / 2, y + h / 2 - 4),
                new Point(x, y + h + 4)
            };

            using (var roofB = new SolidBrush(roofCol))
            {
                g.FillPolygon(roofB, pts);
            }

            // 입체 기와골
            using (var fluteP = new Pen(Color.FromArgb(Math.Max(0, roofCol.R - 14), Math.Max(0, roofCol.G - 14), Math.Max(0, roofCol.B - 14)), 1.4f))
            using (var hiP = new Pen(Color.FromArgb(Math.Min(255, roofCol.R + 24), Math.Min(255, roofCol.G + 24), Math.Min(255, roofCol.B + 28)), 1f))
            {
                for (int tx = x + 14; tx < x + w - 14; tx += 13)
                {
                    float r = Math.Abs((tx - x) / (float)w - 0.5f);
                    float topY = y + 3 + r * (h * 0.9f);
                    float botY = y + h * 0.52f + r * (h * 0.72f);
                    g.DrawLine(fluteP, tx, topY, tx, botY);
                    g.DrawLine(hiP, tx + 1, topY, tx + 1, botY);
                }
            }

            // 용마루 & 금빛 처마 선
            using (var ridgeP = new Pen(Color.FromArgb(Math.Min(255, roofCol.R + 40), Math.Min(255, roofCol.G + 40), Math.Min(255, roofCol.B + 44)), 3.2f))
            using (var eaveP = new Pen(eaveGold, 2f))
            {
                g.DrawLine(ridgeP, x + w / 6, y + 3, x + w - w / 6, y + 3);
                g.DrawLines(eaveP, new Point[] { pts[0], pts[6], pts[4] });
            }

            // 잡상(어처구니) 장식
            if (japsangCount > 0)
            {
                using var jB = new SolidBrush(Color.FromArgb(35, 38, 48));
                for (int j = 1; j <= japsangCount; j++)
                {
                    int jxL = x + j * 14;
                    int jyL = y + h - 8 - j * 6;
                    g.FillRectangle(jB, jxL, jyL, 5, 7);
                    int jxR = x + w - j * 14 - 5;
                    g.FillRectangle(jB, jxR, jyL, 5, 7);
                }
            }
        }

        private static void DrawGableRoof(Graphics g, int x, int y, int w, int h, Color roofCol, Color woodGableCol)
        {
            Point[] roofPts =
            {
                new Point(x - 6, y + h),
                new Point(x + w / 2, y),
                new Point(x + w + 6, y + h)
            };
            using (var roofB = new SolidBrush(roofCol))
            using (var ridgeP = new Pen(Color.FromArgb(180, 175, 165), 2.5f))
            {
                g.FillPolygon(roofB, roofPts);
                g.DrawLine(ridgeP, x + w / 2 - 18, y + 2, x + w / 2 + 18, y + 2);
            }
            // 박공 삼각 목조 장식
            Point[] gablePts =
            {
                new Point(x + 12, y + h),
                new Point(x + w / 2, y + 8),
                new Point(x + w - 12, y + h)
            };
            using (var gableB = new SolidBrush(woodGableCol))
            using (var trimP = new Pen(Color.FromArgb(55, 30, 14), 2f))
            {
                g.FillPolygon(gableB, gablePts);
                g.DrawPolygon(trimP, gablePts);
            }
        }

        private static void DrawThatchedRoof(Graphics g, int x, int y, int w, int h, Color strawCol, Color shadowCol)
        {
            using (var path = new GraphicsPath())
            {
                path.AddBezier(x, y + h, x + w * 0.12f, y + 4, x + w * 0.88f, y + 4, x + w, y + h);
                path.AddLine(x + w, y + h, x, y + h);
                path.CloseFigure();

                using var strawB = new SolidBrush(strawCol);
                g.FillPath(strawB, path);
            }
            // 짚결 줄눈 및 용마름 상단 엮음
            using (var lineP = new Pen(shadowCol, 1.5f))
            using (var ridgeB = new SolidBrush(shadowCol))
            {
                g.FillRectangle(ridgeB, x + w / 5, y + 6, w * 3 / 5, 7);
                for (int sx = x + 16; sx < x + w - 16; sx += 14)
                    g.DrawLine(lineP, sx, y + 14, sx - 4, y + h);
                g.DrawLine(lineP, x, y + h, x + w, y + h);
            }
        }

        private static void DrawPalaceTieredHall(Graphics g, int x, int y, int w, int h, bool nightLit, bool redMoon)
        {
            int midY = y + h / 2;
            DrawWallAndPillars(g, x + 8, midY + 4, w - 16, h - (midY - y) - 4, Color.FromArgb(142, 22, 24), Color.FromArgb(88, 10, 12), 4);
            // 중앙 홍예/궁궐 대문 또는 대청 창살
            int winSpan = (w - 48) / 3;
            for (int i = 0; i < 3; i++)
                DrawChanghojiWindow(g, x + 24 + i * winSpan, midY + 24, winSpan - 18, h / 2 - 34, nightLit, redMoon);

            DrawTileRoof(g, x - 14, midY - 12, w + 28, 36, Color.FromArgb(20, 22, 32), Color.FromArgb(238, 196, 60), true, 4);
            // 2층 상단
            DrawWallAndPillars(g, x + 36, y + 44, w - 72, midY - y - 48, Color.FromArgb(148, 24, 28), Color.FromArgb(86, 10, 12), 3);
            DrawTileRoof(g, x + 12, y - 4, w - 24, 52, Color.FromArgb(22, 24, 34), Color.FromArgb(245, 205, 65), true, 5);
        }

        private static void DrawPavilionUpper(Graphics g, int x, int y, int w, int h, Color woodCol, Color railCol, bool hasShutters)
        {
            using (var colB = new SolidBrush(woodCol))
            using (var railB = new SolidBrush(railCol))
            {
                g.FillRectangle(colB, x, y, 12, h);
                g.FillRectangle(colB, x + w - 12, y, 12, h);
                g.FillRectangle(colB, x + w / 2 - 6, y, 12, h);

                if (hasShutters)
                {
                    using var shB = new SolidBrush(Color.FromArgb(42, 28, 20));
                    g.FillRectangle(shB, x + 12, y + 10, w - 24, h - 22);
                }

                // 하단 계자난간
                g.FillRectangle(railB, x - 4, y + h - 18, w + 8, 6);
                for (int rx = x + 6; rx < x + w - 6; rx += 14)
                    g.FillRectangle(railB, rx, y + h - 22, 4, 18);
            }
        }

        private static void DrawChanghojiWindow(Graphics g, int x, int y, int w, int h, bool nightGlow, bool redMoonTint)
        {
            Color paperCol = nightGlow
                ? Color.FromArgb(215, 255, 205, 95)
                : (redMoonTint ? Color.FromArgb(175, 165, 95, 100) : Color.FromArgb(190, 252, 232, 165));

            using var frameB = new SolidBrush(Color.FromArgb(88, 46, 18));
            using var paperB = new SolidBrush(paperCol);
            using var gridP = new Pen(Color.FromArgb(110, 62, 24), 1.2f);
            g.FillRectangle(frameB, x - 3, y - 3, w + 6, h + 6);
            g.FillRectangle(paperB, x, y, w, h);
            for (int gx = x + 14; gx < x + w; gx += 14) g.DrawLine(gridP, gx, y, gx, y + h);
            for (int gy = y + 14; gy < y + h; gy += 14) g.DrawLine(gridP, x, gy, x + w, gy);
        }

        private static void DrawDisplayShelf(Graphics g, int x, int y, int w, int h, bool silkBolts)
        {
            using var bg = new SolidBrush(Color.FromArgb(52, 34, 20));
            using var shelfP = new Pen(Color.FromArgb(128, 76, 34), 3f);
            g.FillRectangle(bg, x, y, w, h);
            g.DrawLine(shelfP, x, y + h / 2, x + w, y + h / 2);

            if (silkBolts)
            {
                Color[] bolts = { Color.FromArgb(205, 55, 55), Color.FromArgb(55, 105, 190), Color.FromArgb(230, 185, 55), Color.FromArgb(75, 150, 90) };
                for (int i = 0; i < 4; i++)
                {
                    using var bB = new SolidBrush(bolts[i]);
                    g.FillRectangle(bB, x + 8 + i * 20, y + 6, 14, h / 2 - 10);
                }
            }
            else
            {
                // 서책 / 약재 서랍 격자
                using var drawerB = new SolidBrush(Color.FromArgb(125, 78, 38));
                using var lineP = new Pen(Color.FromArgb(65, 38, 18), 1.2f);
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        int dx = x + 4 + c * ((w - 8) / 4);
                        int dy = y + 4 + r * ((h - 8) / 3);
                        g.FillRectangle(drawerB, dx, dy, (w - 16) / 4, (h - 14) / 3);
                        g.DrawRectangle(lineP, dx, dy, (w - 16) / 4, (h - 14) / 3);
                    }
                }
            }
        }

        private static void DrawClothAwning(Graphics g, int x, int y, int w, int h, Color c1, Color c2)
        {
            int stripeW = 14;
            for (int sx = x, idx = 0; sx < x + w; sx += stripeW, idx++)
            {
                int cw = Math.Min(stripeW, x + w - sx);
                using var b = new SolidBrush((idx % 2 == 0) ? c1 : c2);
                g.FillRectangle(b, sx, y, cw, h);
            }
        }

        private static void DrawCourtyardWall(Graphics g, int x, int groundY, int w, int h, Color wallCol, Color capRoofCol)
        {
            int y = groundY - h;
            using var stoneB = new SolidBrush(Color.FromArgb(145, 140, 134));
            using var wallB = new SolidBrush(wallCol);
            using var roofB = new SolidBrush(capRoofCol);
            using var lineP = new Pen(Color.FromArgb(88, 82, 76), 1.2f);

            // 하단 기단 석축 + 상단 토담
            int stoneH = Math.Min(28, h / 2);
            g.FillRectangle(stoneB, x, groundY - stoneH, w, stoneH);
            g.FillRectangle(wallB, x, y + 10, w, h - stoneH - 10);
            g.DrawLine(lineP, x, groundY - stoneH, x + w, groundY - stoneH);

            // 기와 얹은 담장 지붕
            g.FillRectangle(roofB, x - 3, y, w + 6, 12);
        }

        private static void DrawSignboard(PixelBuffer buffer, Graphics g, int x, int y, int w, int h, string text, Color bgCol, Color textCol)
        {
            using var bgB = new SolidBrush(bgCol);
            using var borderP = new Pen(Color.FromArgb(200, 165, 70), 1.5f);
            g.FillRectangle(bgB, x, y, w, h);
            g.DrawRectangle(borderP, x, y, w, h);

            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var tb = new SolidBrush(textCol);
            g.DrawString(text, buffer.FontTiny, tb, new RectangleF(x, y, w, h), sf);
        }

        private static void DrawCheongsachorong(Graphics g, int x, int y, bool brightNight)
        {
            int alpha = brightNight ? 90 : 45;
            using var glow = new SolidBrush(Color.FromArgb(alpha, 255, 185, 65));
            using var rB = new SolidBrush(Color.FromArgb(228, 45, 45));
            using var bB = new SolidBrush(Color.FromArgb(38, 78, 215));
            using var tB = new SolidBrush(Color.FromArgb(215, 180, 50));
            using var hP = new Pen(Color.FromArgb(65, 65, 65), 1.5f);

            g.FillEllipse(glow, x - 22, y - 8, 44, 52);
            g.DrawLine(hP, x, y - 10, x, y);
            g.FillRectangle(rB, x - 8, y, 16, 14);
            g.FillRectangle(bB, x - 8, y + 14, 16, 14);
            g.FillRectangle(tB, x - 3, y + 28, 6, 9);
        }

        private static void DrawPaperLantern(Graphics g, int x, int y, bool bright)
        {
            using var glow = new SolidBrush(Color.FromArgb(bright ? 85 : 40, 255, 140, 60));
            using var body = new SolidBrush(Color.FromArgb(240, 75, 55));
            using var lineP = new Pen(Color.FromArgb(60, 25, 20), 1.2f);
            g.FillEllipse(glow, x - 18, y - 4, 36, 42);
            g.DrawLine(lineP, x, y, x, y + 6);
            g.FillEllipse(body, x - 8, y + 6, 16, 22);
            g.DrawEllipse(lineP, x - 8, y + 6, 16, 22);
        }

        private static void DrawTorchBrazier(Graphics g, int x, int y, int seed)
        {
            float flk = (float)(Math.Sin(DateTime.UtcNow.Millisecond * 0.02f + seed * 1.9f) * 3.5f);
            using var outerG = new SolidBrush(Color.FromArgb(45, 255, 120, 20));
            using var innerG = new SolidBrush(Color.FromArgb(75, 255, 165, 40));
            using var flameB = new SolidBrush(Color.FromArgb(255, 175, 40));
            using var coreB = new SolidBrush(Color.FromArgb(255, 245, 145));
            using var ironB = new SolidBrush(Color.FromArgb(45, 42, 40));

            g.FillEllipse(outerG, x - 32, y - 42 + flk, 64, 64);
            g.FillEllipse(innerG, x - 20, y - 32 + flk, 40, 40);
            g.FillEllipse(flameB, x - 10, y - 22 + flk, 20, 24);
            g.FillEllipse(coreB, x - 5, y - 16 + flk, 10, 15);
            g.FillRectangle(ironB, x - 3, y - 2, 6, 24);
            g.FillRectangle(ironB, x - 8, y - 5, 16, 5);
        }

        private static void DrawStoneLantern(Graphics g, int x, int y, bool lit)
        {
            using var stB = new SolidBrush(Color.FromArgb(142, 142, 148));
            using var glow = new SolidBrush(Color.FromArgb(lit ? 75 : 30, 255, 200, 90));
            using var fire = new SolidBrush(Color.FromArgb(255, 215, 100));
            g.FillRectangle(stB, x - 6, y - 46, 12, 46);
            g.FillRectangle(stB, x - 12, y - 62, 24, 18);
            g.FillPolygon(stB, new Point[] { new(x - 18, y - 62), new(x, y - 76), new(x + 18, y - 62) });
            if (lit)
            {
                g.FillEllipse(glow, x - 24, y - 72, 48, 42);
                g.FillRectangle(fire, x - 5, y - 58, 10, 10);
            }
        }

        private static void DrawStoneWell(Graphics g, int x, int groundY, int w)
        {
            using var stB = new SolidBrush(Color.FromArgb(148, 146, 142));
            using var darkP = new Pen(Color.FromArgb(88, 86, 82), 1.5f);
            using var woodB = new SolidBrush(Color.FromArgb(95, 54, 24));
            g.FillRectangle(stB, x, groundY - 28, w, 28);
            g.DrawRectangle(darkP, x, groundY - 28, w, 28);
            g.FillRectangle(woodB, x + 6, groundY - 64, 6, 36);
            g.FillRectangle(woodB, x + w - 12, groundY - 64, 6, 36);
            g.FillRectangle(woodB, x + 2, groundY - 68, w - 4, 6);
        }

        private static void DrawJarCluster(Graphics g, int x, int baseY, int count)
        {
            using var jarB = new SolidBrush(Color.FromArgb(82, 42, 22));
            using var lidB = new SolidBrush(Color.FromArgb(52, 26, 14));
            for (int i = 0; i < count; i++)
            {
                int jx = x + i * 24;
                int jh = (i % 2 == 0) ? 24 : 18;
                g.FillEllipse(jarB, jx, baseY - jh, 20, jh);
                g.FillRectangle(lidB, jx + 3, baseY - jh - 3, 14, 5);
            }
        }

        private static void DrawWoodenBench(Graphics g, int x, int y, int w, int h)
        {
            using var wb = new SolidBrush(Color.FromArgb(118, 68, 32));
            g.FillRectangle(wb, x, y, w, 6);
            g.FillRectangle(wb, x + 6, y + 6, 6, h);
            g.FillRectangle(wb, x + w - 12, y + 6, 6, h);
        }

        private static void DrawHangingPeppers(Graphics g, int x, int y, int count)
        {
            using var pepB = new SolidBrush(Color.FromArgb(205, 42, 34));
            for (int i = 0; i < count; i++)
                g.FillEllipse(pepB, x + i * 16, y + (i % 2) * 3, 6, 12);
        }

        private static void DrawStoneStele(Graphics g, int x, int groundY, int w, int h)
        {
            using var stB = new SolidBrush(Color.FromArgb(156, 158, 162));
            using var baseB = new SolidBrush(Color.FromArgb(124, 126, 130));
            using var pen = new Pen(Color.FromArgb(88, 90, 94), 1.4f);
            g.FillRectangle(baseB, x - 5, groundY - 12, w + 10, 12);
            g.FillRectangle(stB, x, groundY - h, w, h - 12);
            g.DrawRectangle(pen, x, groundY - h, w, h - 12);
        }

        private static void DrawHaetaePedestal(Graphics g, int x, int groundY, int w, int h)
        {
            using var pedB = new SolidBrush(Color.FromArgb(168, 164, 160));
            using var statB = new SolidBrush(Color.FromArgb(142, 138, 134));
            g.FillRectangle(pedB, x, groundY - 36, w, 36);
            g.FillEllipse(statB, x + 8, groundY - h, w - 16, h - 32);
            g.FillEllipse(statB, x + 10, groundY - h - 14, 24, 24);
        }

        private static void DrawStoneEmbankment(Graphics g, int x, int y, int w, int h, bool hasMainArch, bool darkWater, int archWidth)
        {
            using var stB = new SolidBrush(Color.FromArgb(142, 148, 156));
            using var mP = new Pen(Color.FromArgb(92, 98, 106), 1.4f);
            g.FillRectangle(stB, x, y, w, h);

            for (int ly = y + 22; ly < y + h; ly += 22)
                g.DrawLine(mP, x, ly, x + w, ly);
            for (int r = 0; r < h / 22 + 1; r++)
            {
                int rY = y + r * 22;
                int shift = (r % 2 == 0) ? 0 : 26;
                for (int lx = x + shift; lx < x + w; lx += 52)
                    g.DrawLine(mP, lx, rY, lx, Math.Min(y + h, rY + 22));
            }

            if (hasMainArch)
            {
                int ax = x + w / 2 - archWidth / 2;
                using var archB = new SolidBrush(Color.FromArgb(darkWater ? 26 : 34, 34, 44));
                g.FillRectangle(archB, ax, y + 48, archWidth, h - 48);
                g.FillPie(archB, ax, y + 14, archWidth, 68, 180, 180);
            }
        }

        private static void DrawBridgeCoping(Graphics g, int x, int y, int w, bool cornerPosts)
        {
            using var capB = new SolidBrush(Color.FromArgb(184, 188, 196));
            g.FillRectangle(capB, x, y, w, 13);
            if (cornerPosts)
            {
                g.FillRectangle(capB, x + 22, y - 22, 32, 22);
                g.FillRectangle(capB, x + w - 54, y - 22, 32, 22);
            }
        }

        private static void DrawFortressWallBase(Graphics g, int x, int y, int w, int h, bool merlons, bool pineRock)
        {
            using var wallB = new SolidBrush(Color.FromArgb(108, 100, 94));
            using var stP = new Pen(Color.FromArgb(68, 62, 58), 1.6f);
            using var hiP = new Pen(Color.FromArgb(138, 130, 122), 1f);
            using var slitB = new SolidBrush(Color.FromArgb(24, 20, 18));

            g.FillRectangle(wallB, x, y + 30, w, h - 30);
            if (merlons)
            {
                int step = 112;
                for (int mx = x + 14; mx < x + w - 70; mx += step)
                {
                    g.FillRectangle(wallB, mx, y, 78, 34);
                    g.FillRectangle(slitB, mx + 32, y + 12, 12, 18);
                }
            }

            for (int ly = y + 32; ly < y + h; ly += 26)
            {
                g.DrawLine(stP, x, ly, x + w, ly);
                g.DrawLine(hiP, x, ly + 1, x + w, ly + 1);
            }
            for (int r = 0; r < h / 26; r++)
            {
                int rY = y + 32 + r * 26;
                int shift = (r % 2 == 0) ? 0 : 28;
                for (int lx = x + shift; lx < x + w; lx += 56)
                    g.DrawLine(stP, lx, rY, lx, Math.Min(y + h, rY + 26));
            }

            if (pineRock)
            {
                using var rockB = new SolidBrush(Color.FromArgb(84, 78, 74));
                g.FillPolygon(rockB, new Point[] {
                    new(x, y + h), new(x + 28, y + h - 36), new(x + 74, y + h - 18), new(x + 115, y + h)
                });
            }
        }

        private enum TreeKind { Pine, Willow, Bamboo, Persimmon, Ginkgo, Blossom }

        private static void DrawNaturalTree(Graphics g, int x, int groundY, TreeKind kind, float scale, bool nightDark)
        {
            int trunkH = (int)(135 * scale);
            int topY = groundY - trunkH;
            Color trunkCol = nightDark ? Color.FromArgb(46, 32, 22) : Color.FromArgb(84, 52, 28);

            if (kind == TreeKind.Bamboo)
            {
                using var bamP = new Pen(nightDark ? Color.FromArgb(42, 78, 44) : Color.FromArgb(72, 132, 62), 5f);
                using var leafB = new SolidBrush(nightDark ? Color.FromArgb(52, 92, 54) : Color.FromArgb(92, 160, 76));
                for (int b = -2; b <= 2; b++)
                {
                    int bx = x + b * 14;
                    g.DrawLine(bamP, bx, groundY, bx + b * 5, topY - 35);
                    g.FillEllipse(leafB, bx - 16 + b * 5, topY - 40, 38, 16);
                }
                return;
            }

            using (var trB = new SolidBrush(trunkCol))
            {
                g.FillRectangle(trB, x - 7, topY + 28, 14, trunkH - 28);
            }

            if (kind == TreeKind.Willow)
            {
                Color dCol = nightDark ? Color.FromArgb(35, 68, 42) : Color.FromArgb(62, 122, 58);
                Color lCol = nightDark ? Color.FromArgb(50, 92, 58) : Color.FromArgb(92, 162, 78);
                using var dB = new SolidBrush(dCol);
                using var lB = new SolidBrush(lCol);
                using var vP = new Pen(lCol, 2f);
                g.FillEllipse(dB, x - 56, topY - 24, 118, 76);
                g.FillEllipse(lB, x - 44, topY - 14, 96, 58);
                for (int vx = x - 42; vx <= x + 52; vx += 13)
                    g.DrawLine(vP, vx, topY + 24, vx - 4, topY + 72 + (Math.Abs(vx) % 18));
            }
            else if (kind == TreeKind.Pine)
            {
                Color pDark = nightDark ? Color.FromArgb(24, 48, 32) : Color.FromArgb(36, 82, 46);
                Color pLight = nightDark ? Color.FromArgb(36, 70, 44) : Color.FromArgb(54, 112, 62);
                using var dB = new SolidBrush(pDark);
                using var lB = new SolidBrush(pLight);
                g.FillEllipse(dB, x - 58, topY + 4, 72, 34);
                g.FillEllipse(dB, x - 8, topY - 16, 78, 38);
                g.FillEllipse(lB, x - 44, topY - 28, 86, 40);
            }
            else if (kind == TreeKind.Persimmon)
            {
                using var dB = new SolidBrush(Color.FromArgb(145, 78, 32));
                using var lB = new SolidBrush(Color.FromArgb(186, 112, 44));
                using var frB = new SolidBrush(Color.FromArgb(245, 115, 32));
                g.FillEllipse(dB, x - 52, topY - 22, 108, 74);
                g.FillEllipse(lB, x - 38, topY - 12, 82, 54);
                for (int f = 0; f < 5; f++)
                    g.FillEllipse(frB, x - 30 + f * 15, topY + 4 + (f % 2) * 14, 8, 8);
            }
            else if (kind == TreeKind.Ginkgo)
            {
                Color gDark = nightDark ? Color.FromArgb(118, 98, 32) : Color.FromArgb(205, 162, 38);
                Color gLight = nightDark ? Color.FromArgb(152, 128, 45) : Color.FromArgb(242, 202, 58);
                using var dB = new SolidBrush(gDark);
                using var lB = new SolidBrush(gLight);
                g.FillEllipse(dB, x - 54, topY - 24, 112, 78);
                g.FillEllipse(lB, x - 40, topY - 14, 84, 58);
            }
            else // Blossom
            {
                using var dB = new SolidBrush(Color.FromArgb(215, 125, 145));
                using var lB = new SolidBrush(Color.FromArgb(250, 185, 202));
                g.FillEllipse(dB, x - 56, topY - 24, 114, 78);
                g.FillEllipse(lB, x - 40, topY - 14, 86, 56);
            }
        }
    }
}
