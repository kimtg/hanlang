using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using HanlangGame.Engine;
using HanlangGame.Entities;
using HanlangGame.Systems;

namespace HanlangGame.UI
{
    public class HUD
    {
        public void Render(PixelBuffer buffer, Player player, Enemy? boss = null, int currentStage = 1)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            var status = player.Status;

            // ========================================================
            // 1. 960x720 상단 고해상도 상태 패널 (전통 옻칠 금장 패널 디자인)
            // ========================================================
            int hudH = 72;
            using (var hudBrush = new LinearGradientBrush(
                new Point(0, 0), new Point(0, hudH),
                Color.FromArgb(240, 16, 16, 22), Color.FromArgb(235, 26, 24, 34)))
            {
                g.FillRectangle(hudBrush, 0, 0, PixelBuffer.Width, hudH);
            }

            // 하단 금장 분할선
            using (var goldPen = new Pen(Color.FromArgb(215, 175, 65), 2.5f))
            using (var darkLine = new Pen(Color.FromArgb(120, 0, 0, 0), 1.5f))
            {
                g.DrawLine(goldPen, 0, hudH, PixelBuffer.Width, hudH);
                g.DrawLine(darkLine, 0, hudH + 2f, PixelBuffer.Width, hudH + 2f);
            }

            // 플레이어 이름 및 레벨 명패
            buffer.FillRoundedRect(16, 12, 160, 48, 6f, Color.FromArgb(180, 35, 30, 40));
            buffer.DrawRoundedRect(16, 12, 160, 48, 6f, Color.FromArgb(190, 150, 60), 1.5f);

            buffer.DrawOutlinedText("한 랑", 28, 16, Color.FromArgb(255, 240, 180), Color.FromArgb(30, 20, 10), buffer.FontSubtitle, 1.8f);
            buffer.DrawOutlinedText($"LV.{status.Level}", 95, 26, Color.FromArgb(255, 215, 80), Color.FromArgb(30, 20, 10), buffer.FontHeader, 1.5f);

            // ========================================================
            // 2. 고해상도 HP / MP 게이지 바
            // ========================================================
            int barX = 195;
            int hpW = 230;
            int hpH = 20;

            // HP 바
            buffer.DrawOutlinedText("HP", barX - 32, 15, Color.FromArgb(255, 120, 120), Color.Black, buffer.FontBody, 1.5f);
            buffer.FillRoundedRect(barX, 14, hpW, hpH, 4f, Color.FromArgb(200, 45, 15, 15));
            buffer.DrawRoundedRect(barX, 14, hpW, hpH, 4f, Color.FromArgb(90, 30, 30), 1.5f);

            int fillHp = Math.Max(0, (int)((float)status.CurrentHp / status.MaxHp * (hpW - 4)));
            if (fillHp > 0)
            {
                using (var hpBrush = new LinearGradientBrush(
                    new Point(barX + 2, 16), new Point(barX + 2, 16 + hpH - 4),
                    Color.FromArgb(240, 60, 60), Color.FromArgb(170, 20, 20)))
                {
                    buffer.FillRoundedRect(barX + 2, 16, fillHp, hpH - 4, 3f, Color.FromArgb(220, 40, 40));
                }
            }
            string hpStr = $"{status.CurrentHp} / {status.MaxHp}";
            buffer.DrawOutlinedText(hpStr, barX + (hpW - (int)g.MeasureString(hpStr, buffer.FontTiny).Width) / 2f, 17, Color.White, Color.FromArgb(50, 10, 10), buffer.FontTiny, 1.2f);

            // MP 바
            int mpW = 180;
            int mpH = 16;
            int mpY = 40;
            buffer.DrawOutlinedText("MP", barX - 32, mpY - 1, Color.FromArgb(120, 180, 255), Color.Black, buffer.FontBody, 1.5f);
            buffer.FillRoundedRect(barX, mpY, mpW, mpH, 4f, Color.FromArgb(200, 15, 25, 55));
            buffer.DrawRoundedRect(barX, mpY, mpW, mpH, 4f, Color.FromArgb(30, 40, 90), 1.5f);

            int fillMp = Math.Max(0, (int)((float)status.CurrentMp / status.MaxMp * (mpW - 4)));
            if (fillMp > 0)
            {
                using (var mpBrush = new LinearGradientBrush(
                    new Point(barX + 2, mpY + 2), new Point(barX + 2, mpY + mpH - 2),
                    Color.FromArgb(50, 150, 255), Color.FromArgb(25, 75, 180)))
                {
                    buffer.FillRoundedRect(barX + 2, mpY + 2, fillMp, mpH - 4, 3f, Color.FromArgb(40, 130, 245));
                }
            }
            string mpStr = $"{status.CurrentMp} / {status.MaxMp}";
            buffer.DrawOutlinedText(mpStr, barX + (mpW - (int)g.MeasureString(mpStr, buffer.FontTiny).Width) / 2f, mpY + 1, Color.White, Color.FromArgb(10, 20, 50), buffer.FontTiny, 1.2f);

            // ========================================================
            // 3. 엽전 (골드) 및 단축키 안내 뱃지
            // ========================================================
            int goldX = 450;
            buffer.FillRoundedRect(goldX, 16, 125, 40, 6f, Color.FromArgb(160, 45, 35, 15));
            buffer.DrawRoundedRect(goldX, 16, 125, 40, 6f, Color.FromArgb(220, 180, 50), 1.5f);
            buffer.DrawOutlinedText($"엽전 {status.Gold:N0} 냥", goldX + 12, 26, Color.FromArgb(255, 225, 60), Color.FromArgb(40, 30, 10), buffer.FontHeader, 1.5f);

            // 스탯 포인트 알림 (남은 포인트 있을 시 점멸)
            int btnX = 590;
            if (status.StatPoints > 0)
            {
                bool blink = ((int)(DateTime.UtcNow.Millisecond / 260) % 2 == 0);
                Color blinkBg = blink ? Color.FromArgb(220, 180, 40, 20) : Color.FromArgb(180, 130, 30, 15);
                buffer.FillRoundedRect(btnX, 16, 110, 40, 6f, blinkBg);
                buffer.DrawRoundedRect(btnX, 16, 110, 40, 6f, Color.FromArgb(255, 230, 70), 2f);
                buffer.DrawOutlinedText($"★성장 +{status.StatPoints}", btnX + 12, 26, Color.FromArgb(255, 240, 90), Color.Black, buffer.FontHeader, 1.5f);
            }
            else
            {
                buffer.FillRoundedRect(btnX, 16, 100, 40, 6f, Color.FromArgb(160, 35, 35, 45));
                buffer.DrawRoundedRect(btnX, 16, 100, 40, 6f, Color.FromArgb(120, 120, 140), 1.5f);
                buffer.DrawOutlinedText("[Tab:성장]", btnX + 14, 26, Color.FromArgb(200, 200, 215), Color.Black, buffer.FontBody, 1.2f);
            }

            // 주막 상점 바로가기 뱃지
            int shopBtnX = 705;
            buffer.FillRoundedRect(shopBtnX, 16, 100, 40, 6f, Color.FromArgb(180, 50, 30, 15));
            buffer.DrawRoundedRect(shopBtnX, 16, 100, 40, 6f, Color.FromArgb(235, 170, 50), 1.5f);
            buffer.DrawOutlinedText("[S: 주막]", shopBtnX + 18, 26, Color.FromArgb(255, 230, 120), Color.FromArgb(40, 25, 10), buffer.FontHeader, 1.5f);

            // 현재 장 표시
            int stageX = 820;
            buffer.FillRoundedRect(stageX, 16, 120, 40, 6f, Color.FromArgb(200, 25, 30, 50));
            buffer.DrawRoundedRect(stageX, 16, 120, 40, 6f, Color.FromArgb(100, 140, 220), 1.5f);
            buffer.DrawOutlinedText($"제 {currentStage} 장", stageX + 28, 26, Color.FromArgb(220, 235, 255), Color.FromArgb(20, 20, 40), buffer.FontHeader, 1.5f);

            // 들기 상태 조작 안내
            if (player.IsCarrying)
            {
                int guideW = 340;
                int guideX = (PixelBuffer.Width - guideW) / 2;
                buffer.FillRoundedRect(guideX, 82, guideW, 32, 6f, Color.FromArgb(230, 25, 20, 35));
                buffer.DrawRoundedRect(guideX, 82, guideW, 32, 6f, Color.FromArgb(255, 210, 60), 1.5f);
                buffer.DrawOutlinedText("들기 상태 [ Z: 들고 때리기 | X: 던지기 ]", guideX + 16, 88, Color.FromArgb(255, 235, 130), Color.Black, buffer.FontBody, 1.2f);
            }

            // ========================================================
            // 4. 960x720 하단 아케이드 스타일 대형 보스 게이지 & 칭호
            // ========================================================
            if (boss != null && !boss.IsDead && boss.CurrentHp > 0)
            {
                int bPlateW = 740;
                int bPlateH = 68;
                int bPlateX = (PixelBuffer.Width - bPlateW) / 2;
                int bPlateY = PixelBuffer.Height - bPlateH - 14;

                // 보스 명패 배경
                buffer.FillRoundedRect(bPlateX, bPlateY, bPlateW, bPlateH, 8f, Color.FromArgb(235, 20, 15, 25));
                buffer.DrawRoundedRect(bPlateX, bPlateY, bPlateW, bPlateH, 8f, Color.FromArgb(235, 185, 60), 2.5f);

                // 보스 칭호
                string bossTitle = boss.Type switch
                {
                    EnemyType.MidBoss1 => "【 제3장 중간보스 : 괴력 건달 마달 】",
                    EnemyType.MidBoss2 => "【 제6장 중간보스 : 배신자 철웅 】",
                    EnemyType.Boss => "【 제9장 최종보스 : 마도학사 흑호 】",
                    _ => $"【 보스 : {boss.Name} 】"
                };

                buffer.DrawOutlinedText(bossTitle, bPlateX + 18, bPlateY + 10, Color.FromArgb(255, 235, 140), Color.Black, buffer.FontSubtitle, 1.8f);

                // 광폭화 상태 알림
                if (boss.IsRaging)
                {
                    bool blink = ((int)(DateTime.UtcNow.Millisecond / 200) % 2 == 0);
                    Color rageCol = blink ? Color.FromArgb(255, 60, 60) : Color.FromArgb(255, 230, 60);
                    buffer.DrawOutlinedText("★ 광폭화 (슈퍼아머 발동!) ★", bPlateX + bPlateW - 250, bPlateY + 10, rageCol, Color.Black, buffer.FontHeader, 1.8f);
                }
                else
                {
                    buffer.DrawOutlinedText($"{boss.CurrentHp} / {boss.MaxHp}", bPlateX + bPlateW - 130, bPlateY + 12, Color.FromArgb(220, 210, 210), Color.Black, buffer.FontBody, 1.2f);
                }

                // 대형 체력 바 게이지
                int bBarX = bPlateX + 18;
                int bBarY = bPlateY + 40;
                int bBarW = bPlateW - 36;
                int bBarH = 16;

                buffer.FillRoundedRect(bBarX, bBarY, bBarW, bBarH, 4f, Color.FromArgb(220, 45, 15, 15));
                buffer.DrawRoundedRect(bBarX, bBarY, bBarW, bBarH, 4f, Color.FromArgb(120, 40, 40), 1.5f);

                int bFill = Math.Max(0, (int)((float)boss.CurrentHp / boss.MaxHp * (bBarW - 4)));
                if (bFill > 0)
                {
                    Color gradTop = boss.IsRaging ? Color.FromArgb(255, 60, 50) : Color.FromArgb(240, 110, 35);
                    Color gradBot = boss.IsRaging ? Color.FromArgb(180, 20, 20) : Color.FromArgb(170, 45, 15);

                    using (var bBrush = new LinearGradientBrush(
                        new Point(bBarX + 2, bBarY + 2), new Point(bBarX + 2, bBarY + bBarH - 2), gradTop, gradBot))
                    {
                        buffer.FillRoundedRect(bBarX + 2, bBarY + 2, bFill, bBarH - 4, 3f, gradTop);
                    }
                }
            }
        }
    }
}
