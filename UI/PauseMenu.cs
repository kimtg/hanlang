using System;
using System.Drawing;
using System.Windows.Forms;
using HanlangGame.Engine;

namespace HanlangGame.UI
{
    public class PauseMenu
    {
        private int _selectedTab = 0; // 0: 한글 무예 보감, 1: 시스템 설정

        public void HandleInput(Input input, out bool resume, out bool quitToTitle)
        {
            resume = false;
            quitToTitle = false;

            if (input.IsJustPressed(Keys.Escape) || input.IsJustPressed(Keys.Enter) || input.IsJustPressed(Keys.Z))
            {
                SoundSynth.PlayJump();
                resume = true;
                return;
            }

            if (input.IsJustPressed(Keys.Q))
            {
                SoundSynth.PlayJump();
                quitToTitle = true;
                return;
            }

            if (input.IsJustPressed(Keys.Left) || input.IsJustPressed(Keys.Right) || input.IsJustPressed(Keys.Tab))
            {
                _selectedTab = 1 - _selectedTab;
                SoundSynth.PlayJump();
            }
        }

        public void Render(PixelBuffer buffer)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            // 반투명 전통 먹색 암막 배경
            buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(205, 12, 10, 18));

            // 비단 두루마리 창틀 (Traditional Silk Scroll Frame)
            int boxX = 70;
            int boxY = 45;
            int boxW = 820;
            int boxH = 630;

            // 외곽 금박 테두리 및 비단 바탕
            buffer.FillRoundedRect(boxX, boxY, boxW, boxH, 16f, Color.FromArgb(250, 244, 230));
            buffer.DrawRoundedRect(boxX, boxY, boxW, boxH, 16f, Color.FromArgb(170, 120, 45), 4f);
            buffer.DrawRoundedRect(boxX + 6, boxY + 6, boxW - 12, boxH - 12, 12f, Color.FromArgb(225, 195, 140), 2f);

            // 상단 현판 헤더
            int headerH = 60;
            buffer.FillRoundedRect(boxX + 16, boxY + 16, boxW - 32, headerH, 10f, Color.FromArgb(235, 45, 35, 40));
            buffer.DrawRoundedRect(boxX + 16, boxY + 16, boxW - 32, headerH, 10f, Color.FromArgb(245, 210, 80), 2.5f);

            buffer.DrawOutlinedTextCentered("★ 일 시 정 지 ㆍ 한 글 무 예 보 감 (武 藝 寶 鑑) ★", boxY + 28, Color.FromArgb(255, 235, 90), Color.FromArgb(40, 20, 10), buffer.FontTitle, 2.5f);

            // 내용 영역 분할 (3열 그리드 카드)
            int colY = boxY + 95;
            int colH = 460;
            int colW = 248;

            // 1열: 기본 권법 (Basic Martial Arts)
            RenderSectionCard(buffer, boxX + 24, colY, colW, colH, "【 기본 권법 (基本) 】", Color.FromArgb(50, 70, 140), new[]
            {
                ("[ Z 키 (탭) ]", "주먹 3단 콤보", Color.FromArgb(40, 80, 180)),
                ("", "바닥 무기/항아리 줍기", Color.FromArgb(70, 70, 70)),
                ("", "쓰러진 적 메쳐 들기", Color.FromArgb(70, 70, 70)),
                ("", "든 물체/적으로 스윙 타격", Color.FromArgb(70, 70, 70)),
                ("[ X 키 (탭) ]", "앞차기 (넉백 발차기)", Color.FromArgb(180, 50, 50)),
                ("", "들고 있는 물체/적 투척", Color.FromArgb(70, 70, 70)),
                ("[ C 키 ]", "도약 (점프)", Color.FromArgb(40, 140, 60)),
                ("[ 공중 + Z / X ]", "공중 급강하 날아차기", Color.FromArgb(180, 80, 30)),
                ("[ 대시 + Z ]", "질풍 슬라이딩 정권", Color.FromArgb(120, 40, 160)),
                ("[ 대시 + X ]", "비천 돌진 날아차기", Color.FromArgb(160, 40, 80))
            });

            // 2열: 방어 및 심리전 (Defense & Parry)
            RenderSectionCard(buffer, boxX + 286, colY, colW, colH, "【 방어 & 반격 (防禦) 】", Color.FromArgb(40, 120, 100), new[]
            {
                ("[ Z 키 (길게 유지) ]", "철벽 가드 (Iron Guard)", Color.FromArgb(30, 130, 140)),
                ("", "ㆍ받는 피해 70% 대폭 감소!", Color.FromArgb(170, 40, 40)),
                ("", "ㆍ피격 경직 및 쓰러짐 방지", Color.FromArgb(70, 70, 70)),
                ("[ 피격 직전 Z ]", "순간 쳐내기 (Just Parry)", Color.FromArgb(20, 80, 200)),
                ("", "ㆍ피격 직전 0.14초 정밀 탭!", Color.FromArgb(200, 40, 40)),
                ("", "ㆍ적 1.25초 스턴 & 기력+10", Color.FromArgb(30, 130, 40)),
                ("", "ㆍ공격/후딜 중 불가(연타 방지)", Color.FromArgb(190, 50, 40)),
                ("[ 공격 후딜레이 ]", "격투 공방 빈틈 (Delay)", Color.FromArgb(160, 50, 50)),
                ("", "ㆍ공격 후 빈틈엔 가드/패링 불가", Color.FromArgb(70, 70, 70)),
                ("", "ㆍ빈틈 피격 시 카운터 피해(+25%)", Color.FromArgb(180, 40, 40)),
                ("[ 피격/다운 중 C ]", "비상 낙법 (Ukemi)", Color.FromArgb(160, 90, 20)),
                ("", "ㆍ착지 전 즉시 기상 & 무적", Color.FromArgb(60, 60, 60))
            });

            // 3열: 삼대 삼한 비기 (Three Secret Arts)
            RenderSectionCard(buffer, boxX + 548, colY, colW, colH, "【 삼대 비기 (秘技) 】", Color.FromArgb(140, 40, 40), new[]
            {
                ("[ Space 키 ]", "제1 비기: 훈민선풍각", Color.FromArgb(180, 40, 40)),
                ("", "ㆍ소모 기력: MP 15", Color.FromArgb(100, 100, 100)),
                ("", "ㆍ360도 8괘 자모 회오리", Color.FromArgb(70, 70, 70)),
                ("", "ㆍ주변 적 다단히트 소탕", Color.FromArgb(70, 70, 70)),
                ("[ ↑ + Space ]", "제2 비기: 용비어천격", Color.FromArgb(30, 80, 190)),
                ("", "ㆍ소모 기력: MP 20", Color.FromArgb(100, 100, 100)),
                ("", "ㆍ청룡 기운 승룡 어퍼컷!", Color.FromArgb(70, 70, 70)),
                ("", "ㆍ적을 높이 띄워 공중 콤보", Color.FromArgb(70, 70, 70)),
                ("[ ↓ + Space ]", "제3 비기: 월인천강포", Color.FromArgb(160, 40, 160)),
                ("", "ㆍ소모 기력: MP 25", Color.FromArgb(100, 100, 100)),
                ("", "ㆍ달빛 태극 파동포 관통 빔!", Color.FromArgb(170, 30, 30)),
                ("", "ㆍ전방 150px 일직선 적 관통", Color.FromArgb(70, 70, 70))
            });

            // 하단 조작 안내 띠
            int footerY = boxY + colH + 105;
            buffer.FillRoundedRect(boxX + 20, footerY, boxW - 40, 45, 8f, Color.FromArgb(240, 35, 30, 40));
            buffer.DrawRoundedRect(boxX + 20, footerY, boxW - 40, 45, 8f, Color.FromArgb(255, 215, 70), 1.5f);

            buffer.DrawOutlinedTextCentered("[ ESC / ENTER / Z : 게임으로 복귀 ]     ㆍ     [ Q : 타이틀 화면으로 나가기 ]", footerY + 12, Color.FromArgb(255, 240, 170), Color.Black, buffer.FontBody, 1.8f);
        }

        private void RenderSectionCard(PixelBuffer buffer, int x, int y, int w, int h, string title, Color themeCol, (string header, string desc, Color headerCol)[] items)
        {
            // 카드 배경
            buffer.FillRoundedRect(x, y, w, h, 8f, Color.FromArgb(242, 238, 224));
            buffer.DrawRoundedRect(x, y, w, h, 8f, Color.FromArgb(200, 180, 150), 1.5f);

            // 카드 타이틀 바
            buffer.FillRoundedRect(x, y, w, 36, 8f, themeCol);
            buffer.DrawOutlinedTextCenteredAt(title, x + (w / 2f), y + 8, Color.FromArgb(255, 245, 210), Color.FromArgb(20, 15, 25), buffer.FontSubtitle, 1.8f);

            int itemY = y + 46;
            foreach (var (header, desc, headerCol) in items)
            {
                if (!string.IsNullOrEmpty(header))
                {
                    buffer.DrawOutlinedText(header, x + 12, itemY, headerCol, Color.White, buffer.FontSubtitle, 1.2f);
                    itemY += 21;
                }
                if (!string.IsNullOrEmpty(desc))
                {
                    buffer.DrawText(desc, x + 16, itemY, Color.FromArgb(50, 45, 45), large: false);
                    itemY += 18;
                }
            }
        }
    }
}
