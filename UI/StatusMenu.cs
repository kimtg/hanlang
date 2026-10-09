using System;
using System.Drawing;
using System.Windows.Forms;
using HanlangGame.Engine;
using HanlangGame.Systems;

namespace HanlangGame.UI
{
    public class StatusMenu
    {
        private int _selectedIdx = 0;
        private readonly string[] _statNames = { "힘", "맷집", "날쌤", "기력" };

        public void HandleInput(Input input, StatusSystem status, out bool shouldClose)
        {
            shouldClose = false;

            if (input.IsJustPressed(Keys.Tab) || input.IsJustPressed(Keys.Escape))
            {
                shouldClose = true;
                return;
            }

            if (input.IsJustPressed(Keys.Up))
            {
                _selectedIdx = (_selectedIdx - 1 + _statNames.Length) % _statNames.Length;
                SoundSynth.PlayJump();
            }
            else if (input.IsJustPressed(Keys.Down))
            {
                _selectedIdx = (_selectedIdx + 1) % _statNames.Length;
                SoundSynth.PlayJump();
            }
            else if (input.IsJustPressed(Keys.Z) || input.IsJustPressed(Keys.Enter) || input.IsJustPressed(Keys.Right))
            {
                // 스탯 포인트 투자
                if (status.UpgradeStat(_statNames[_selectedIdx]))
                {
                    SoundSynth.PlayLevelUp();
                }
            }
        }

        public void Render(PixelBuffer buffer, StatusSystem status)
        {
            // 반투명 딤(Dim) 배경
            buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(140, 0, 0, 0));

            // 상태창 메인 윈도우 박스 (가로 240, 세로 180)
            int winX = 40;
            int winY = 28;
            int winW = 240;
            int winH = 184;

            buffer.FillRect(winX, winY, winW, winH, Color.FromArgb(245, 240, 225));
            buffer.DrawRect(winX, winY, winW, winH, Color.FromArgb(60, 40, 20));
            buffer.DrawRect(winX + 2, winY + 2, winW - 4, winH - 4, Color.FromArgb(180, 150, 100));

            // 헤더
            buffer.FillRect(winX + 4, winY + 4, winW - 8, 20, Color.FromArgb(80, 45, 25));
            buffer.DrawTextCentered("【 의혈 유생 한랑 - 상태 및 성장 】", winY + 6, Color.FromArgb(255, 235, 170));

            // 기본 정보 (레벨, 경험치, 엽전, 남은 스탯포인트)
            int infoY = winY + 28;
            buffer.DrawText($"레벨: LV.{status.Level} (경험치: {status.CurrentExp}/{status.MaxExp})", winX + 12, infoY, Color.FromArgb(50, 40, 30));
            buffer.DrawText($"보유 엽전: {status.Gold} 냥", winX + 12, infoY + 14, Color.FromArgb(180, 120, 20));

            Color ptColor = status.StatPoints > 0 ? Color.FromArgb(220, 30, 30) : Color.FromArgb(100, 100, 100);
            buffer.DrawText($"남은 성장 점수: [ {status.StatPoints} ] pt", winX + 12, infoY + 28, ptColor);

            buffer.DrawLine(winX + 8, infoY + 44, winX + winW - 8, infoY + 44, Color.FromArgb(180, 160, 130));

            // 스탯 목록
            int listY = infoY + 48;
            for (int i = 0; i < _statNames.Length; i++)
            {
                bool isSelected = (i == _selectedIdx);
                int itemY = listY + i * 20;

                if (isSelected)
                {
                    buffer.FillRect(winX + 8, itemY - 1, winW - 16, 17, Color.FromArgb(255, 220, 150));
                    buffer.DrawRect(winX + 8, itemY - 1, winW - 16, 17, Color.FromArgb(200, 120, 30));
                }

                string cursor = isSelected ? "▶" : "  ";
                string name = _statNames[i];
                int val = name switch
                {
                    "힘" => status.Power,
                    "맷집" => status.Endurance,
                    "날쌤" => status.Agility,
                    "기력" => status.Ki,
                    _ => 10
                };

                string desc = name switch
                {
                    "힘" => $"(공격력: {status.PunchDamage} / 킥: {status.KickDamage})",
                    "맷집" => $"(최대HP: {status.MaxHp})",
                    "날쌤" => $"(이동속도: {(int)status.MoveSpeed})",
                    "기력" => $"(최대MP: {status.MaxMp} / 비기: {status.SpecialDamage})",
                    _ => ""
                };

                Color textColor = isSelected ? Color.FromArgb(160, 20, 20) : Color.FromArgb(40, 40, 40);
                buffer.DrawText($"{cursor} {name} (수치: {val:D2}) {desc}", winX + 10, itemY, textColor);

                if (status.StatPoints > 0 && isSelected)
                {
                    buffer.DrawText("[+1 Z키]", winX + winW - 60, itemY, Color.FromArgb(200, 30, 30));
                }
            }

            // 하단 안내
            int bottomY = winY + winH - 22;
            buffer.DrawLine(winX + 8, bottomY - 3, winX + winW - 8, bottomY - 3, Color.FromArgb(180, 160, 130));
            buffer.DrawText("↑↓: 선택 | Z: 포인트 투자 | Tab: 닫기", winX + 12, bottomY, Color.FromArgb(80, 70, 60));
        }
    }
}
