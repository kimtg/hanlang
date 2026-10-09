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
        private readonly string[] _menuItems = { "새로운 이야기 (시작)", "이어하기 (불러오기)", "조작법 및 비기 설명" };
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
                    _state = TitleMenuState.Help;
                }
            }
        }

        public void Render(PixelBuffer buffer)
        {
            // 한양 밤하늘 및 기와집 실루엣
            buffer.FillRect(0, 0, PixelBuffer.Width, 140, Color.FromArgb(20, 15, 35));
            buffer.FillRect(0, 140, PixelBuffer.Width, 100, Color.FromArgb(30, 25, 45));

            // 보름달 (세종의 달)
            using var moonBrush = new SolidBrush(Color.FromArgb(255, 245, 180));
            buffer.Graphics.FillEllipse(moonBrush, 230, 20, 48, 48);

            // 한옥 실루엣
            buffer.FillRect(20, 95, 280, 45, Color.FromArgb(12, 10, 25));
            buffer.FillRect(10, 85, 300, 12, Color.FromArgb(8, 6, 18));

            if (_state == TitleMenuState.Help)
            {
                RenderHelp(buffer);
                return;
            }

            // 타이틀 로고
            buffer.DrawTextCentered("조선 무협 벨트스크롤 액션", 30, Color.FromArgb(255, 215, 120));
            buffer.DrawTextCentered("한 랑 (韓浪)", 50, Color.FromArgb(255, 60, 60), large: true);
            buffer.DrawTextCentered("~ 훈민정음 탈환기 ~", 72, Color.FromArgb(255, 230, 150));

            // 메뉴 목록
            bool hasSave = SaveLoadSystem.HasSaveData();
            int menuStartY = 125;

            for (int i = 0; i < _menuItems.Length; i++)
            {
                bool isSelected = (i == _selectedIdx);
                string prefix = isSelected ? "▶ " : "   ";
                string label = prefix + _menuItems[i];

                Color col;
                if (i == 1 && !hasSave)
                {
                    col = Color.FromArgb(100, 100, 110); // 비활성화 회색
                    label += " (기록 없음)";
                }
                else if (isSelected)
                {
                    col = Color.FromArgb(255, 240, 80);
                }
                else
                {
                    col = Color.FromArgb(210, 210, 220);
                }

                buffer.DrawText(label, 75, menuStartY + i * 20, col);
            }

            // 하단 조작 가이드
            buffer.DrawTextCentered("조작: ↑↓ 선택 | Z 키: 결정", 205, Color.FromArgb(160, 160, 180));
            buffer.DrawTextCentered("320 x 240 레트로 픽셀 모드", 220, Color.FromArgb(120, 120, 140));
        }

        private void RenderHelp(PixelBuffer buffer)
        {
            int hX = 8;
            int hY = 12;
            int hW = 304;
            int hH = 216;

            buffer.FillRect(hX, hY, hW, hH, Color.FromArgb(245, 240, 225));
            buffer.DrawRect(hX, hY, hW, hH, Color.FromArgb(60, 40, 20));

            buffer.FillRect(hX + 3, hY + 3, hW - 6, 20, Color.FromArgb(80, 45, 25));
            buffer.DrawTextCentered("【 조작법 및 한글 비기 비급 】", hY + 5, Color.FromArgb(255, 235, 170));

            int textY = hY + 28;
            buffer.DrawText("● 기본 이동: 방향키 이동 (좌우 연속 2회 입력 시 대시) / C 점프", hX + 8, textY, Color.FromArgb(40, 40, 40), tiny: true);
            buffer.DrawText("● 기본 타격: Z 정권 찌르기 / X 돌려차기 (점프 중 입력 시 점프 킥)", hX + 8, textY + 16, Color.FromArgb(40, 40, 40), tiny: true);
            buffer.DrawText("● 물체/적 들기: Z키 (주변의 목봉, 궤짝, 돌멩이 또는 쓰러진 적 들기)", hX + 8, textY + 32, Color.FromArgb(170, 35, 20), tiny: true);
            buffer.DrawText("● 들고 공격: Z 들고 때리기 / X 시원한 전방 투척 공격!", hX + 8, textY + 48, Color.FromArgb(170, 35, 20), tiny: true);
            buffer.DrawText("● 들기 탈출: 쓰러진 적을 들면 3초 후 스스로 풀려납니다!", hX + 8, textY + 64, Color.FromArgb(160, 40, 100), tiny: true);
            buffer.DrawText("● 한글 비기: Space키 (훈민선풍각 - 광역 타격 비기)", hX + 8, textY + 80, Color.FromArgb(30, 80, 180), tiny: true);
            buffer.DrawText("● 메뉴/기록: Tab 능력치 분배 / F5 저장 / F9 불러오기", hX + 8, textY + 96, Color.FromArgb(40, 40, 40), tiny: true);
            buffer.DrawText("● 전투 팁: 적 머리 위 체력바를 확인하고 던지기로 다수를 제압하시오!", hX + 8, textY + 114, Color.FromArgb(140, 80, 10), tiny: true);

            buffer.DrawTextCentered("돌아가려면 Z 또는 ESC 키를 누르시오", hY + hH - 16, Color.FromArgb(80, 60, 40), tiny: true);
        }
    }
}
