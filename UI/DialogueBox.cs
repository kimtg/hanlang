using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HanlangGame.Engine;

namespace HanlangGame.UI
{
    public class DialogueBox
    {
        public bool IsActive { get; private set; } = false;

        private readonly List<(string speaker, string line, Color nameCol)> _dialogueQueue = new();
        private int _currentIndex = 0;
        private float _charRevealTimer = 0f;

        // 보스전 조우 플래그 (중복 실행 방지)
        private readonly HashSet<int> _triggeredStages = new();

        // KO 격파 배너 연출
        public bool FinisherActive { get; private set; } = false;
        private float _finisherTimer = 0f;
        private string _finisherText = "격 파 (擊破)";

        public bool HasTriggered(int stage) => _triggeredStages.Contains(stage);

        public void Reset(int? upToStage = null)
        {
            _dialogueQueue.Clear();
            _currentIndex = 0;
            _charRevealTimer = 0f;
            FinisherActive = false;
            _finisherTimer = 0f;
            if (upToStage.HasValue)
            {
                _triggeredStages.RemoveWhere(s => s >= upToStage.Value);
            }
        }

        public void TriggerStageDialogue(int stage)
        {
            if (_triggeredStages.Contains(stage)) return;
            _triggeredStages.Add(stage);

            _dialogueQueue.Clear();
            _currentIndex = 0;
            _charRevealTimer = 0f;

            switch (stage)
            {
                case 3:
                    _dialogueQueue.Add(("괴력 건달 마달", "어이 유생 양반, 감히 내 청계천 다리를 지나가겠다고? 뼈도 못 추릴 줄 알아라!", Color.FromArgb(235, 120, 50)));
                    _dialogueQueue.Add(("유생 한랑", "길을 비키시오! 백성의 문자를 되찾기 전까진 한 걸음도 물러설 수 없소!", Color.FromArgb(70, 160, 255)));
                    _dialogueQueue.Add(("괴력 건달 마달", "글자 따위가 밥을 먹여주냐? 내 무쇠 주먹 맛을 보아라!", Color.FromArgb(235, 120, 50)));
                    break;

                case 6:
                    _dialogueQueue.Add(("배신자 철웅", "육조거리까지 기어오다니 용기는 가상하구나. 무과 급제자의 검술을 넘을 수 있겠느냐?", Color.FromArgb(220, 60, 60)));
                    _dialogueQueue.Add(("유생 한랑", "나라와 백성을 지키라 배운 무예를 사리사욕에 쓰다니, 부끄러운 줄 아시오!", Color.FromArgb(70, 160, 255)));
                    _dialogueQueue.Add(("배신자 철웅", "약자의 헛소리는 검 아래서 사라질 뿐이다. 각오해라!", Color.FromArgb(220, 60, 60)));
                    break;

                case 9:
                    _dialogueQueue.Add(("마도학사 흑호", "크크크... 여기까지 도달했단 말인가. 허나 백성 따위가 문자를 알아서 무엇하겠는가?", Color.FromArgb(210, 40, 40)));
                    _dialogueQueue.Add(("유생 한랑", "모든 사람이 제 뜻을 펴게 하려는 세종대왕의 뜻, 내 목숨을 바쳐 증명하겠소!", Color.FromArgb(70, 160, 255)));
                    _dialogueQueue.Add(("마도학사 흑호", "천추전의 어둠 속에서 훈민정음과 함께 영원히 사라져라!", Color.FromArgb(210, 40, 40)));
                    break;
            }

            if (_dialogueQueue.Count > 0)
            {
                IsActive = true;
                SoundSynth.PlayLevelUp();
            }
        }

        public void TriggerFinisher(string text = "격 파 (擊破)")
        {
            FinisherActive = true;
            _finisherTimer = 2.4f;
            _finisherText = text;
            SoundSynth.PlaySpecial();
        }

        public void Update(float dt)
        {
            if (IsActive)
            {
                _charRevealTimer += dt * 35f;
            }

            if (FinisherActive)
            {
                _finisherTimer -= dt;
                if (_finisherTimer <= 0)
                {
                    FinisherActive = false;
                }
            }
        }

        public void HandleInput(Input input)
        {
            if (!IsActive) return;

            if (input.IsJustPressed(Keys.Escape))
            {
                // 대화 스킵
                IsActive = false;
                SoundSynth.PlayJump();
                return;
            }

            if (input.IsJustPressed(Keys.Enter) || input.IsJustPressed(Keys.Space))
            {
                _currentIndex++;
                _charRevealTimer = 0f;
                SoundSynth.PlayPunch();

                if (_currentIndex >= _dialogueQueue.Count)
                {
                    IsActive = false;
                }
            }
        }

        public void Render(PixelBuffer buffer)
        {
            var g = buffer.Graphics;
            buffer.SetHighQualityModes();

            // 1. 대화창 렌더링
            if (IsActive && _currentIndex < _dialogueQueue.Count)
            {
                var (speaker, line, nameCol) = _dialogueQueue[_currentIndex];

                int boxX = 60;
                int boxY = PixelBuffer.Height - 165;
                int boxW = PixelBuffer.Width - 120;
                int boxH = 135;

                // 대화창 비단 바탕 및 전통 문양 테두리
                buffer.FillRoundedRect(boxX, boxY, boxW, boxH, 12f, Color.FromArgb(240, 18, 16, 26));
                buffer.DrawRoundedRect(boxX, boxY, boxW, boxH, 12f, Color.FromArgb(220, 180, 70), 3f);
                buffer.DrawRoundedRect(boxX + 4, boxY + 4, boxW - 8, boxH - 8, 8f, Color.FromArgb(140, 110, 50), 1.5f);

                // 화자 명패 (Name Tag)
                int tagW = 200;
                int tagH = 36;
                int tagX = boxX + 24;
                int tagY = boxY - 18;

                buffer.FillRoundedRect(tagX, tagY, tagW, tagH, 8f, Color.FromArgb(245, 30, 25, 35));
                buffer.DrawRoundedRect(tagX, tagY, tagW, tagH, 8f, nameCol, 2.5f);
                buffer.DrawOutlinedTextCenteredAt(speaker, tagX + (tagW / 2f), tagY + 8, nameCol, Color.Black, buffer.FontSubtitle, 2f);

                // 텍스트 출력 (글자 타이핑 효과)
                int charCount = Math.Min(line.Length, (int)_charRevealTimer);
                string revealedText = line.Substring(0, charCount);

                buffer.DrawOutlinedText(revealedText, boxX + 35, boxY + 40, Color.FromArgb(250, 245, 235), Color.FromArgb(20, 15, 10), buffer.FontTitle, 2f);

                // 다음 진행 안내 깜빡임
                if (((int)(DateTime.UtcNow.Millisecond / 300) % 2 == 0))
                {
                    buffer.DrawOutlinedText("[ ENTER : 계속 ]", boxX + boxW - 170, boxY + boxH - 28, Color.FromArgb(255, 225, 90), Color.Black, buffer.FontBody, 1.5f);
                }
            }

            // 2. KO 격파 배너 렌더링
            if (FinisherActive)
            {
                RenderFinisherBanner(buffer);
            }
        }

        private void RenderFinisherBanner(PixelBuffer buffer)
        {
            var g = buffer.Graphics;
            float ratio = Math.Clamp(_finisherTimer / 2.4f, 0f, 1f);

            int bannerY = PixelBuffer.Height / 2 - 80;
            int bannerH = 130;

            // 붉은 먹빛 수평 띠
            using (var bannerBrush = new SolidBrush(Color.FromArgb(215, 25, 15, 20)))
            {
                g.FillRectangle(bannerBrush, 0, bannerY, PixelBuffer.Width, bannerH);
            }

            using (var goldLinePen = new Pen(Color.FromArgb(255, 215, 60), 3f))
            {
                g.DrawLine(goldLinePen, 0, bannerY, PixelBuffer.Width, bannerY);
                g.DrawLine(goldLinePen, 0, bannerY + bannerH, PixelBuffer.Width, bannerY + bannerH);
            }

            // 중앙 웅장한 서예 텍스트 "격 파 (擊破)"
            buffer.DrawOutlinedTextCentered("★ " + _finisherText + " ★", bannerY + 22, Color.FromArgb(255, 240, 75), Color.FromArgb(50, 15, 10), buffer.FontHuge, 5f);

            // 붉은 낙관 도장 (Crimson Seal)
            int stampSize = 64;
            int stampX = PixelBuffer.Width / 2 + 190;
            int stampY = bannerY + 32;

            buffer.FillRoundedRect(stampX, stampY, stampSize, stampSize, 6f, Color.FromArgb(220, 180, 25, 25));
            buffer.DrawRoundedRect(stampX, stampY, stampSize, stampSize, 6f, Color.FromArgb(255, 230, 200), 2f);
            buffer.DrawOutlinedText("韓", stampX + 16, stampY + 12, Color.FromArgb(255, 240, 230), Color.FromArgb(80, 10, 10), buffer.FontHeader, 2f);
        }
    }
}
