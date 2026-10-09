using System;
using System.Drawing;
using System.Windows.Forms;
using HanlangGame.Engine;
using HanlangGame.Entities;
using HanlangGame.Systems;

namespace HanlangGame.UI
{
    public class ShopItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public string Category { get; set; } // "주막" or "보제원"
        public Action<StatusSystem, Player> OnPurchase { get; set; }

        public ShopItem(string name, string desc, int cost, string category, Action<StatusSystem, Player> onPurchase)
        {
            Name = name;
            Description = desc;
            Cost = cost;
            Category = category;
            OnPurchase = onPurchase;
        }
    }

    public class ShopMenu
    {
        private int _selectedIdx = 0;
        private string _feedbackMessage = "원하시는 음식이나 영약, 비급서를 고르시오.";
        private Color _feedbackColor = Color.FromArgb(70, 50, 30);

        public int SelectedIndex { get => _selectedIdx; set => _selectedIdx = value; }
        public string FeedbackMessage => _feedbackMessage;
        public ShopItem[] Items => _items;

        private readonly ShopItem[] _items = new ShopItem[]
        {
            new ShopItem("장터국밥", "따끈한 소머리 국밥으로 체력 50을 회복합니다.", 30, "주막", (st, p) =>
            {
                st.CurrentHp = Math.Min(st.MaxHp, st.CurrentHp + 50);
            }),
            new ShopItem("인절미와 동동주", "달콤한 떡과 술로 HP와 MP를 완전 회복합니다.", 60, "주막", (st, p) =>
            {
                st.CurrentHp = st.MaxHp;
                st.CurrentMp = st.MaxMp;
            }),
            new ShopItem("수라상 삼계탕", "귀한 인삼 백숙으로 전신 회복 및 힘(STR) 영구 +2 증가!", 150, "주막", (st, p) =>
            {
                st.CurrentHp = st.MaxHp;
                st.CurrentMp = st.MaxMp;
                st.Strength += 2;
            }),
            new ShopItem("우황청심환", "속을 다스려 맷집(VIT)을 영구히 +2 증가시킵니다. (HP 증가)", 100, "보제원", (st, p) =>
            {
                st.Vitality += 2;
                st.CurrentHp = Math.Min(st.MaxHp, st.CurrentHp + 24);
            }),
            new ShopItem("용력환 (龍力丸)", "비전의 선약으로 힘(STR)을 영구히 +2 증가시킵니다.", 120, "보제원", (st, p) =>
            {
                st.Strength += 2;
            }),
            new ShopItem("천리마 환약", "다리 힘을 길러 날쌤(SPD)을 영구히 +2 증가시킵니다.", 100, "보제원", (st, p) =>
            {
                st.Speed += 2;
            }),
            new ShopItem("단전호흡 비급서", "단전을 틔워 기력(QI)을 영구히 +2 증가시킵니다. (MP 증가)", 150, "보제원", (st, p) =>
            {
                st.Qi += 2;
                st.CurrentMp = Math.Min(st.MaxMp, st.CurrentMp + 20);
            })
        };

        public void HandleInput(Input input, StatusSystem status, Player player, out bool closeRequested)
        {
            closeRequested = false;

            if (input.IsJustPressed(Keys.Escape) || input.IsJustPressed(Keys.X) || input.IsJustPressed(Keys.S) || input.IsJustPressed(Keys.Tab))
            {
                closeRequested = true;
                SoundSynth.PlayJump();
                return;
            }

            if (input.IsJustPressed(Keys.Up))
            {
                _selectedIdx = (_selectedIdx - 1 + _items.Length) % _items.Length;
                SoundSynth.PlayPunch();
            }
            else if (input.IsJustPressed(Keys.Down))
            {
                _selectedIdx = (_selectedIdx + 1) % _items.Length;
                SoundSynth.PlayPunch();
            }

            if (input.IsJustPressed(Keys.Z) || input.IsJustPressed(Keys.Enter))
            {
                var item = _items[_selectedIdx];
                if (status.Gold >= item.Cost)
                {
                    status.Gold -= item.Cost;
                    item.OnPurchase(status, player);
                    SoundSynth.PlayLevelUp();
                    _feedbackMessage = $"[{item.Name}]을(를) 구매하여 효험을 보았소!";
                    _feedbackColor = Color.FromArgb(20, 140, 40);
                }
                else
                {
                    SoundSynth.PlayHit();
                    _feedbackMessage = $"엽전이 부족하오! ({item.Cost - status.Gold}냥 부족)";
                    _feedbackColor = Color.FromArgb(200, 30, 30);
                }
            }
        }

        public void Render(PixelBuffer buffer, StatusSystem status)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            // 1. 전체 화면 반투명 어두운 딤 처리
            buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(195, 12, 10, 18));

            int winW = 800;
            int winH = 580;
            int winX = (PixelBuffer.Width - winW) / 2;
            int winY = (PixelBuffer.Height - winH) / 2;

            // 2. 한지 두루마리 질감 배경 및 목조 금장 테두리
            buffer.FillRoundedRect(winX, winY, winW, winH, 14f, Color.FromArgb(248, 242, 228));
            buffer.DrawRoundedRect(winX, winY, winW, winH, 14f, Color.FromArgb(65, 42, 22), 4f);
            buffer.DrawRoundedRect(winX + 4, winY + 4, winW - 8, winH - 8, 10f, Color.FromArgb(200, 165, 110), 2f);

            // 3. 상단 헤더 배너
            int headH = 54;
            buffer.FillRoundedRect(winX + 8, winY + 8, winW - 16, headH, 8f, Color.FromArgb(95, 45, 25));
            buffer.DrawOutlinedTextCentered("【 저잣거리 주막 & 보제원 한약방 】", winY + 16, Color.FromArgb(255, 235, 175), Color.FromArgb(40, 20, 10), buffer.FontTitle, 2.5f);

            // 4. 보유 엽전 및 현재 HP/MP 상태 바
            int infoY = winY + 70;
            buffer.DrawOutlinedText($"보유 엽전 : {status.Gold:N0} 냥", winX + 24, infoY, Color.FromArgb(205, 140, 20), Color.Transparent, buffer.FontSubtitle);
            buffer.DrawOutlinedText($"[ 체력 HP : {status.CurrentHp} / {status.MaxHp} ]   [ 기력 MP : {status.CurrentMp} / {status.MaxMp} ]", winX + 380, infoY + 2, Color.FromArgb(70, 65, 85), Color.Transparent, buffer.FontHeader);
            buffer.DrawLine(winX + 16, infoY + 34, winX + winW - 16, infoY + 34, Color.FromArgb(195, 175, 145), 2f);

            // 5. 아이템 목록 (행 높이 42px)
            int listY = infoY + 42;
            for (int i = 0; i < _items.Length; i++)
            {
                var it = _items[i];
                bool isSel = (i == _selectedIdx);
                int itemY = listY + i * 44;

                if (isSel)
                {
                    buffer.FillRoundedRect(winX + 16, itemY - 2, winW - 32, 40, 6f, Color.FromArgb(238, 215, 165));
                    buffer.DrawRoundedRect(winX + 16, itemY - 2, winW - 32, 40, 6f, Color.FromArgb(190, 130, 50), 2f);
                    buffer.DrawOutlinedText("▶", winX + 22, itemY + 8, Color.FromArgb(180, 50, 20), Color.Transparent, buffer.FontHeader);
                }

                Color catBg = it.Category == "주막" ? Color.FromArgb(195, 65, 25) : Color.FromArgb(35, 110, 185);
                buffer.FillRoundedRect(winX + 46, itemY + 4, 90, 26, 4f, catBg);
                buffer.DrawOutlinedText($"[{it.Category}]", winX + 58, itemY + 8, Color.White, Color.Transparent, buffer.FontSmall);

                Color nameCol = (status.Gold >= it.Cost) ? Color.FromArgb(45, 30, 20) : Color.FromArgb(150, 140, 140);
                buffer.DrawOutlinedText(it.Name, winX + 155, itemY + 7, nameCol, Color.Transparent, buffer.FontHeader);

                Color costCol = (status.Gold >= it.Cost) ? Color.FromArgb(190, 125, 15) : Color.FromArgb(200, 45, 45);
                buffer.DrawOutlinedText($"{it.Cost:N0} 냥", winX + winW - 140, itemY + 7, costCol, Color.Transparent, buffer.FontSubtitle);
            }

            // 6. 하단 선택된 아이템 상세 설명 박스
            int descY = winY + 458;
            buffer.FillRoundedRect(winX + 16, descY, winW - 32, 54, 8f, Color.FromArgb(240, 232, 215));
            buffer.DrawRoundedRect(winX + 16, descY, winW - 32, 54, 8f, Color.FromArgb(180, 150, 115), 1.5f);

            var selItem = _items[_selectedIdx];
            buffer.DrawOutlinedText(selItem.Description, winX + 28, descY + 16, Color.FromArgb(60, 45, 30), Color.Transparent, buffer.FontHeader);

            // 7. 피드백 메시지 및 조작 안내
            buffer.DrawOutlinedTextCentered(_feedbackMessage, winY + 524, _feedbackColor, Color.Transparent, buffer.FontHeader, 1f);
            buffer.DrawOutlinedTextCentered("[ 방향키 ↑↓ : 선택 | Z / Enter : 구매 | ESC / X / S : 닫기 ]", winY + 550, Color.FromArgb(120, 110, 100), Color.Transparent, buffer.FontSmall);
        }
    }
}
