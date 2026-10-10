using System;
using System.Drawing;
using System.Windows.Forms;
using HanlangGame.Engine;
using HanlangGame.Systems;

namespace HanlangGame.UI
{
    public enum TitleMenuState
    {
        Main,
        Help
    }

    public class TitleScreen
    {
        private int _selectedIdx = 0;
        private readonly string[] _menuItems = { "새로운 이야기 (시작)", "이어하기 (불러오기)", "무한 연무장 (서바이벌)", "조작법 및 비기 설명" };
        private TitleMenuState _state = TitleMenuState.Main;

        public void HandleInput(Input input, out string action)
        {
            action = "";

            if (_state == TitleMenuState.Help)
            {
                if (input.IsJustPressed(Keys.Z) || input.IsJustPressed(Keys.X) || input.IsJustPressed(Keys.Escape) || input.IsJustPressed(Keys.Enter))
                {
                    _state = TitleMenuState.Main;
                    SoundSynth.PlayJump();
                }
                return;
            }

            if (input.IsJustPressed(Keys.Up))
            {
                _selectedIdx = (_selectedIdx - 1 + _menuItems.Length) % _menuItems.Length;
                SoundSynth.PlayJump();
            }
            else if (input.IsJustPressed(Keys.Down))
            {
                _selectedIdx = (_selectedIdx + 1) % _menuItems.Length;
                SoundSynth.PlayJump();
            }
            else if (input.IsJustPressed(Keys.Z) || input.IsJustPressed(Keys.Enter))
            {
                SoundSynth.PlayCoin();
                if (_selectedIdx == 0)
                {
                    action = "new_game";
                }
                else if (_selectedIdx == 1)
                {
                    if (SaveLoadSystem.HasSaveData())
                    {
                        action = "load_game";
                    }
                    else
                    {
                        SoundSynth.PlayHit(); // 저장 데이터 없음
                    }
                }
                else if (_selectedIdx == 2)
                {
                    action = "survival_mode";
                }
                else if (_selectedIdx == 3)
                {
                    _state = TitleMenuState.Help;
                }
            }
        }

        public void Render(PixelBuffer buffer)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            // 1. 960x720 한양 밤하늘 그라데이션
            using (var skyBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Point(0, 0), new Point(0, PixelBuffer.Height),
                Color.FromArgb(16, 12, 32), Color.FromArgb(32, 24, 52)))
            {
                g.FillRectangle(skyBrush, 0, 0, PixelBuffer.Width, PixelBuffer.Height);
            }

            // 보름달 (세종의 달) & 은은한 달무리
            int moonX = 740;
            int moonY = 110;
            using (var haloBrush = new SolidBrush(Color.FromArgb(40, 255, 240, 170)))
            {
                g.FillEllipse(haloBrush, moonX - 70, moonY - 70, 140, 140);
            }
            using (var moonBrush = new SolidBrush(Color.FromArgb(255, 248, 190)))
            {
                g.FillEllipse(moonBrush, moonX - 42, moonY - 42, 84, 84);
            }

            // 원경 산맥 및 궁궐 전각 실루엣
            Point[] mtnPts =
            {
                new Point(0, 480),
                new Point(280, 360),
                new Point(580, 420),
                new Point(820, 340),
                new Point(PixelBuffer.Width, 440),
                new Point(PixelBuffer.Width, PixelBuffer.Height),
                new Point(0, PixelBuffer.Height)
            };
            using (var mtnBrush = new SolidBrush(Color.FromArgb(18, 14, 30)))
            {
                g.FillPolygon(mtnBrush, mtnPts);
            }

            // 한옥 궁궐 실루엣
            using (var roofBrush = new SolidBrush(Color.FromArgb(10, 8, 18)))
            {
                g.FillRectangle(roofBrush, 60, 420, 840, 150);
                Point[] palaceRoof = { new Point(30, 420), new Point(480, 340), new Point(930, 420) };
                g.FillPolygon(roofBrush, palaceRoof);
            }

            if (_state == TitleMenuState.Help)
            {
                RenderHelp(buffer);
                return;
            }

            // 2. 타이틀 로고 (캘리그래피 명조 대형 타이포그래피)
            buffer.DrawOutlinedTextCentered("조선 무협 벨트스크롤 액션 RPG", 85, Color.FromArgb(255, 220, 120), Color.Black, buffer.FontSubtitle, 2f);
            buffer.DrawOutlinedTextCentered("한랑 (韓浪)", 128, Color.FromArgb(255, 75, 75), Color.Black, buffer.FontHuge, 5f);
            buffer.DrawOutlinedTextCentered("~ 훈민정음 탈환기 ~", 195, Color.FromArgb(255, 235, 160), Color.Black, buffer.FontTitle, 3f);

            // 3. 메뉴 목록 (고해상도 버튼 디자인)
            bool hasSave = SaveLoadSystem.HasSaveData();
            int menuStartY = 295;
            int btnW = 480;
            int btnH = 52;
            int btnX = (PixelBuffer.Width - btnW) / 2;

            for (int i = 0; i < _menuItems.Length; i++)
            {
                bool isSelected = (i == _selectedIdx);
                int btnY = menuStartY + i * 66;

                if (isSelected)
                {
                    buffer.FillRoundedRect(btnX, btnY, btnW, btnH, 10f, Color.FromArgb(220, 50, 40, 35));
                    buffer.DrawRoundedRect(btnX, btnY, btnW, btnH, 10f, Color.FromArgb(255, 215, 65), 3f);
                }
                else
                {
                    buffer.FillRoundedRect(btnX, btnY, btnW, btnH, 10f, Color.FromArgb(160, 25, 22, 35));
                    buffer.DrawRoundedRect(btnX, btnY, btnW, btnH, 10f, Color.FromArgb(120, 110, 130), 1.5f);
                }

                string label = _menuItems[i];
                Color col;
                if (i == 1 && !hasSave)
                {
                    col = Color.FromArgb(130, 130, 140);
                    label += " (기록 없음)";
                }
                else if (isSelected)
                {
                    col = Color.FromArgb(255, 240, 90);
                    label = "▶  " + label + "  ◀";
                }
                else
                {
                    col = Color.FromArgb(220, 220, 230);
                }

                buffer.DrawOutlinedTextCentered(label, btnY + 12, col, Color.Black, buffer.FontSubtitle, 2f);
            }

            // 하단 조작 가이드
            buffer.DrawOutlinedTextCentered("조작 : ↑↓ 방향키 선택  |  Z / Enter : 결정", 575, Color.FromArgb(200, 200, 220), Color.Black, buffer.FontHeader, 1.5f);
            buffer.DrawOutlinedTextCentered("세종대왕의 뜻을 받들어 훈민정음을 탈환하라!", 615, Color.FromArgb(150, 225, 160), Color.Black, buffer.FontBody, 1.2f);
        }

        private void RenderHelp(PixelBuffer buffer)
        {
            int hW = 890;
            int hH = 650;
            int hX = (PixelBuffer.Width - hW) / 2;
            int hY = (PixelBuffer.Height - hH) / 2;

            buffer.FillRoundedRect(hX, hY, hW, hH, 14f, Color.FromArgb(248, 242, 228));
            buffer.DrawRoundedRect(hX, hY, hW, hH, 14f, Color.FromArgb(65, 42, 22), 4f);
            buffer.DrawRoundedRect(hX + 4, hY + 4, hW - 8, hH - 8, 10f, Color.FromArgb(200, 165, 110), 2f);

            int headH = 50;
            buffer.FillRoundedRect(hX + 8, hY + 8, hW - 16, headH, 8f, Color.FromArgb(90, 48, 25));
            buffer.DrawOutlinedTextCentered("【 조작법 및 한글 비기 비급서 】", hY + 13, Color.FromArgb(255, 235, 175), Color.FromArgb(40, 20, 10), buffer.FontTitle, 2.5f);

            int textY = hY + 70;
            int lineGap = 37;

            void DrawHelpLine(string title, string desc, Color titleCol)
            {
                buffer.DrawOutlinedText(title, hX + 28, textY, titleCol, Color.Transparent, buffer.FontHeader);
                buffer.DrawOutlinedText(desc, hX + 225, textY + 1, Color.FromArgb(50, 40, 35), Color.Transparent, buffer.FontBody);
                textY += lineGap;
            }

            DrawHelpLine("● 기동 및 회피", "더블탭 대시 / C : 점프 / 공중 피격 중 C : 비상 낙법(Ukemi) 즉시 기상", Color.FromArgb(45, 85, 175));
            DrawHelpLine("● 저스트 패링 & 가드", "적 공격 직전 Z : 쳐내기(스턴+역경직) / Z 길게 누름 : 철벽 가드(피해 70% 감소)", Color.FromArgb(30, 120, 180));
            DrawHelpLine("● 손에 쥐는 무기 3종", "장창(원거리 찌르기) / 쇠방망이(100% 넉다운) / 부채(화살 반사) / X : 무기 투척", Color.FromArgb(195, 55, 25));
            DrawHelpLine("● 대시 공격 & 콤보", "대시 중 Z : 슬라이딩 / 대시 중 X : 날아차기 / 연속 타격 시 5단계 스타일 랭크 승급", Color.FromArgb(195, 55, 25));
            DrawHelpLine("● 들기 & 적 포박", "Z : 수레/탁자/적 들기 / X : 내던지기 (3초 후 풀려남) / Space : 대차륜 스윙", Color.FromArgb(180, 50, 20));
            DrawHelpLine("● EX 강화 비기", "Space + Z / X : MP 20 소모하여 극·선풍각 / 극·용비어천격 / 극·월인천강포 발동", Color.FromArgb(210, 40, 110));
            DrawHelpLine("● 보스전 합(合) 겨루기", "보스와 정면 격돌 시 컷인 발생! Z 키를 맹렬히 연타하여 보스를 그로기 격파", Color.FromArgb(220, 130, 20));
            DrawHelpLine("● 기연 8대 유물 3지선다", "각 구역 돌파 시 철갑흉경, 도깨비감투, 백호발톱, 단전벼락구슬 등 기연 유물 획득", Color.FromArgb(120, 60, 170));
            DrawHelpLine("● 주막 & 성장 & 기록", "S : 주막 상점 / Tab : 능력치 분배 / F5 : 저장 / F9 : 불러오기", Color.FromArgb(50, 130, 60));

            buffer.DrawLine(hX + 20, textY + 6, hX + hW - 20, textY + 6, Color.FromArgb(195, 175, 145), 2f);
            buffer.DrawOutlinedTextCentered("[ Z 키 또는 ESC 키를 눌러 타이틀로 복귀 ]", textY + 16, Color.FromArgb(90, 75, 65), Color.Transparent, buffer.FontHeader);
        }
    }
}
