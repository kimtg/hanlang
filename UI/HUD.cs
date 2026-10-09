using System;
using System.Drawing;
using HanlangGame.Engine;
using HanlangGame.Entities;
using HanlangGame.Systems;

namespace HanlangGame.UI
{
    public class HUD
    {
        public void Render(PixelBuffer buffer, Player player, Enemy? boss = null, int currentStage = 1)
        {
            var status = player.Status;

            // 상단 HUD 어두운 반투명 배경 바
            buffer.FillRect(0, 0, PixelBuffer.Width, 24, Color.FromArgb(220, 20, 20, 25));
            buffer.DrawLine(0, 24, PixelBuffer.Width, 24, Color.FromArgb(70, 70, 85));

            // 플레이어 이름 & 레벨
            buffer.DrawText($"한랑 LV.{status.Level}", 6, 4, Color.FromArgb(240, 230, 180));

            // HP 바
            int hpBarX = 72;
            int hpBarY = 5;
            int hpBarW = 54;
            int hpBarH = 6;
            buffer.FillRect(hpBarX, hpBarY, hpBarW, hpBarH, Color.FromArgb(60, 20, 20));
            int fillHp = Math.Max(0, (int)((float)status.CurrentHp / status.MaxHp * hpBarW));
            buffer.FillRect(hpBarX, hpBarY, fillHp, hpBarH, Color.FromArgb(230, 40, 40));
            buffer.DrawRect(hpBarX, hpBarY, hpBarW, hpBarH, Color.FromArgb(30, 30, 30));
            buffer.DrawText("HP", hpBarX - 16, hpBarY - 2, Color.FromArgb(250, 100, 100));

            // MP 바
            int mpBarY = 13;
            buffer.FillRect(hpBarX, mpBarY, hpBarW, hpBarH, Color.FromArgb(20, 30, 60));
            int fillMp = Math.Max(0, (int)((float)status.CurrentMp / status.MaxMp * hpBarW));
            buffer.FillRect(hpBarX, mpBarY, fillMp, hpBarH, Color.FromArgb(40, 120, 240));
            buffer.DrawRect(hpBarX, mpBarY, hpBarW, hpBarH, Color.FromArgb(30, 30, 30));
            buffer.DrawText("MP", hpBarX - 16, mpBarY - 2, Color.FromArgb(100, 180, 255));

            // 엽전 (골드)
            buffer.DrawText($"{status.Gold}냥", 134, 4, Color.FromArgb(255, 215, 0));

            // 스탯 포인트 알림 (남은 포인트 있을 시 점멸) 및 주막 상점 바로가기
            if (status.StatPoints > 0)
            {
                bool blink = ((int)(DateTime.UtcNow.Millisecond / 300) % 2 == 0);
                Color blinkCol = blink ? Color.FromArgb(255, 230, 80) : Color.FromArgb(255, 120, 50);
                buffer.DrawText($"★성장+{status.StatPoints}", 176, 4, blinkCol);
            }
            else
            {
                buffer.DrawText("[Tab:성장]", 176, 4, Color.FromArgb(160, 160, 175));
            }
            buffer.DrawText("[S:주막]", 218, 4, Color.FromArgb(255, 225, 110));

            // 현재 장 표시
            buffer.DrawText($"[제{currentStage}장]", 254, 4, Color.FromArgb(210, 230, 255));

            // 들고 있을 때 조작 가이드 알림
            if (player.IsCarrying)
            {
                buffer.DrawText("들기상태 [Z:때리기 X:던지기]", 145, 14, Color.FromArgb(255, 220, 70));
            }

            // 대형 아케이드 보스 체력바 & 칭호 (보스전 진행 중일 때 하단에 장엄하게 표시)
            if (boss != null && !boss.IsDead && boss.CurrentHp > 0)
            {
                int bPlateX = 24;
                int bPlateY = PixelBuffer.Height - 26;
                int bPlateW = 272;
                int bPlateH = 22;

                // 보스 명패 배경
                buffer.FillRect(bPlateX, bPlateY, bPlateW, bPlateH, Color.FromArgb(235, 15, 15, 22));
                buffer.DrawRect(bPlateX, bPlateY, bPlateW, bPlateH, Color.FromArgb(240, 185, 55));
                buffer.DrawRect(bPlateX + 1, bPlateY + 1, bPlateW - 2, bPlateH - 2, Color.FromArgb(120, 30, 30));

                // 보스 칭호 및 이름
                string bossTitle = boss.Type switch
                {
                    EnemyType.MidBoss1 => "【 제3장 중간보스 : 괴력 건달 마달 】",
                    EnemyType.MidBoss2 => "【 제6장 중간보스 : 배신자 철웅 】",
                    EnemyType.Boss => "【 제9장 최종보스 : 마도학사 흑호 】",
                    _ => $"【 보스 : {boss.Name} 】"
                };

                buffer.DrawText(bossTitle, bPlateX + 6, bPlateY + 2, Color.FromArgb(255, 230, 140));

                // 광폭화 상태 알림
                if (boss.IsRaging)
                {
                    bool blink = ((int)(DateTime.UtcNow.Millisecond / 250) % 2 == 0);
                    Color rageCol = blink ? Color.FromArgb(255, 60, 60) : Color.FromArgb(255, 230, 60);
                    buffer.DrawText("★광폭화(슈퍼아머)!★", bPlateX + bPlateW - 95, bPlateY + 2, rageCol);
                }
                else
                {
                    buffer.DrawText($"{boss.CurrentHp}/{boss.MaxHp}", bPlateX + bPlateW - 55, bPlateY + 2, Color.FromArgb(220, 200, 190));
                }

                // 게이지 바
                int bBarX = bPlateX + 6;
                int bBarY = bPlateY + 12;
                int bBarW = bPlateW - 12;
                int bBarH = 6;

                buffer.FillRect(bBarX, bBarY, bBarW, bBarH, Color.FromArgb(50, 15, 15));
                int bFill = Math.Max(0, (int)((float)boss.CurrentHp / boss.MaxHp * bBarW));
                Color barCol = boss.IsRaging ? Color.FromArgb(255, 50, 40) : Color.FromArgb(235, 100, 35);
                buffer.FillRect(bBarX, bBarY, bFill, bBarH, barCol);
                buffer.DrawRect(bBarX, bBarY, bBarW, bBarH, Color.FromArgb(180, 150, 80));
            }
        }
    }
}
