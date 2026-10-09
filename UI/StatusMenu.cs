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
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            // 1. 전체 화면 반투명 어두운 딤 처리
            buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(185, 12, 10, 18));

            int winW = 740;
            int winH = 540;
            int winX = (PixelBuffer.Width - winW) / 2;
            int winY = (PixelBuffer.Height - winH) / 2;

            // 2. 한지 두루마리 질감 배경 및 목조 금장 테두리
            buffer.FillRoundedRect(winX, winY, winW, winH, 14f, Color.FromArgb(248, 242, 228));
            buffer.DrawRoundedRect(winX, winY, winW, winH, 14f, Color.FromArgb(65, 42, 22), 4f);
            buffer.DrawRoundedRect(winX + 4, winY + 4, winW - 8, winH - 8, 10f, Color.FromArgb(200, 165, 110), 2f);

            // 3. 상단 헤더
            int headH = 54;
            buffer.FillRoundedRect(winX + 8, winY + 8, winW - 16, headH, 8f, Color.FromArgb(90, 48, 25));
            buffer.DrawOutlinedTextCentered("【 의열 유생 한랑 - 상태 및 성장 】", winY + 16, Color.FromArgb(255, 235, 175), Color.FromArgb(40, 20, 10), buffer.FontTitle, 2.5f);

            // 4. 기본 정보 (레벨, 경험치, 엽전, 남은 스탯포인트)
            int infoY = winY + 74;
            buffer.DrawOutlinedText($"수련 등급 : LV.{status.Level}   (경험치 : {status.CurrentExp} / {status.MaxExp})", winX + 24, infoY, Color.FromArgb(50, 40, 30), Color.Transparent, buffer.FontHeader);
            buffer.DrawOutlinedText($"보유 엽전 : {status.Gold:N0} 냥", winX + 440, infoY, Color.FromArgb(195, 130, 20), Color.Transparent, buffer.FontHeader);

            Color ptColor = status.StatPoints > 0 ? Color.FromArgb(215, 35, 35) : Color.FromArgb(110, 100, 95);
            buffer.DrawOutlinedText($"성장 가능 포인트 : [ {status.StatPoints} ] pt", winX + 24, infoY + 32, ptColor, Color.Transparent, buffer.FontSubtitle);
            buffer.DrawLine(winX + 16, infoY + 70, winX + winW - 16, infoY + 70, Color.FromArgb(195, 175, 145), 2f);

            // 5. 4대 스탯 목록 (행 높이 58px)
            int listY = infoY + 80;
            for (int i = 0; i < _statNames.Length; i++)
            {
                bool isSelected = (i == _selectedIdx);
                int itemY = listY + i * 60;

                if (isSelected)
                {
                    buffer.FillRoundedRect(winX + 16, itemY - 2, winW - 32, 54, 8f, Color.FromArgb(240, 218, 168));
                    buffer.DrawRoundedRect(winX + 16, itemY - 2, winW - 32, 54, 8f, Color.FromArgb(195, 130, 50), 2.5f);
                    buffer.DrawOutlinedText("▶", winX + 26, itemY + 14, Color.FromArgb(180, 45, 20), Color.Transparent, buffer.FontTitle);
                }

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
                    "힘" => $"기본 주먹력 : {status.PunchDamage}  |  발차기력 : {status.KickDamage}",
                    "맷집" => $"최대 체력 : {status.MaxHp}  (피격 경감)",
                    "날쌤" => $"이동 속도 : {(int)status.MoveSpeed}  |  대시 속도 : {(int)status.DashSpeed}",
                    "기력" => $"최대 기력 : {status.MaxMp}  |  비기 훈민선풍각 위력 : {status.SpecialDamage}",
                    _ => ""
                };

                Color nameCol = isSelected ? Color.FromArgb(170, 25, 25) : Color.FromArgb(40, 30, 20);
                buffer.DrawOutlinedText($"{name} ( {val:D2} )", winX + 65, itemY + 13, nameCol, Color.Transparent, buffer.FontTitle);
                buffer.DrawOutlinedText(desc, winX + 215, itemY + 16, Color.FromArgb(90, 75, 60), Color.Transparent, buffer.FontBody);

                if (status.StatPoints > 0 && isSelected)
                {
                    buffer.FillRoundedRect(winX + winW - 165, itemY + 6, 135, 38, 6f, Color.FromArgb(215, 45, 35));
                    buffer.DrawRoundedRect(winX + winW - 165, itemY + 6, 135, 38, 6f, Color.FromArgb(255, 220, 80), 2f);
                    buffer.DrawOutlinedText("[ +1 Z키 투자 ]", winX + winW - 153, itemY + 14, Color.FromArgb(255, 245, 120), Color.Black, buffer.FontBody, 1.5f);
                }
            }

            // 6. 하단 조작 가이드
            int bottomY = winY + winH - 42;
            buffer.DrawLine(winX + 16, bottomY - 6, winX + winW - 16, bottomY - 6, Color.FromArgb(195, 175, 145), 1.5f);
            buffer.DrawOutlinedTextCentered("[ ↑↓ 방향키 : 스탯 선택 | Z / Enter / → : 성장 포인트 투자 | Tab / ESC : 창 닫기 ]", bottomY + 4, Color.FromArgb(100, 90, 80), Color.Transparent, buffer.FontSmall);
        }
    }
}
