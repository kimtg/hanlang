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
            // 반투명 딤 처리
            buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(180, 0, 0, 0));

            int winX = 25;
            int winY = 16;
            int winW = 270;
            int winH = 208;

            // 한지 두루마리 질감 배경
            buffer.FillRect(winX, winY, winW, winH, Color.FromArgb(246, 240, 226));
            buffer.DrawRect(winX, winY, winW, winH, Color.FromArgb(60, 40, 20));
            buffer.DrawRect(winX + 2, winY + 2, winW - 4, winH - 4, Color.FromArgb(190, 160, 110));

            // 헤더
            buffer.FillRect(winX + 4, winY + 4, winW - 8, 22, Color.FromArgb(90, 45, 25));
            buffer.DrawTextCentered("【 저잣거리 주막 & 보제원 한약방 】", winY + 6, Color.FromArgb(255, 235, 170));

            // 보유 엽전 표시
            buffer.DrawText($"보유 엽전: {status.Gold} 냥", winX + 12, winY + 28, Color.FromArgb(190, 130, 20));
            buffer.DrawText($"[HP {status.CurrentHp}/{status.MaxHp}] [MP {status.CurrentMp}/{status.MaxMp}]", winX + 140, winY + 28, Color.FromArgb(70, 70, 90));
            buffer.DrawLine(winX + 8, winY + 42, winX + winW - 8, winY + 42, Color.FromArgb(180, 160, 130));

            // 아이템 목록
            int listY = winY + 46;
            for (int i = 0; i < _items.Length; i++)
            {
                var it = _items[i];
                bool isSel = (i == _selectedIdx);
                int itemY = listY + i * 16;

                if (isSel)
                {
                    buffer.FillRect(winX + 8, itemY - 1, winW - 16, 15, Color.FromArgb(235, 210, 160));
                    buffer.DrawRect(winX + 8, itemY - 1, winW - 16, 15, Color.FromArgb(180, 120, 60));
                }

                Color catCol = it.Category == "주막" ? Color.FromArgb(180, 60, 20) : Color.FromArgb(30, 100, 170);
                buffer.DrawText($"[{it.Category}]", winX + 10, itemY, catCol);

                Color nameCol = (status.Gold >= it.Cost) ? Color.FromArgb(40, 30, 20) : Color.FromArgb(140, 130, 130);
                buffer.DrawText(it.Name, winX + 60, itemY, nameCol);

                Color costCol = (status.Gold >= it.Cost) ? Color.FromArgb(180, 120, 10) : Color.FromArgb(180, 40, 40);
                buffer.DrawText($"{it.Cost}냥", winX + 215, itemY, costCol);
            }

            // 하단 선택된 아이템 설명 박스
            int descY = winY + 162;
            buffer.FillRect(winX + 8, descY, winW - 16, 22, Color.FromArgb(238, 230, 212));
            buffer.DrawRect(winX + 8, descY, winW - 16, 22, Color.FromArgb(170, 140, 100));

            var selItem = _items[_selectedIdx];
            buffer.DrawText(selItem.Description, winX + 12, descY + 4, Color.FromArgb(50, 40, 30));

            // 피드백 메시지 / 조작 안내
            buffer.DrawTextCentered(_feedbackMessage, winY + 188, _feedbackColor);
        }
    }
}
