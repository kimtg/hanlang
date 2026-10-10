using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using HanlangGame.Engine;
using HanlangGame.Entities;
using HanlangGame.Systems;
using HanlangGame.UI;

namespace HanlangGame
{
    public enum GameState
    {
        Title,
        Playing,
        Paused,
        StatusMenu,
        ShopMenu,
        GameOver,
        GameWon
    }

    public class GameScene
    {
        public GameState State { get; private set; } = GameState.Title;

        private readonly PixelBuffer _buffer = new();
        private readonly Input _input = new();

        private Player _player = null!;
        private StatusSystem _status = null!;
        private StageManager _stageManager = null!;
        private CombatSystem _combatSystem = null!;

        private HUD _hud = null!;
        private StatusMenu _statusMenu = null!;
        private ShopMenu _shopMenu = null!;
        private TitleScreen _titleScreen = null!;
        private PauseMenu _pauseMenu = null!;
        private DialogueBox _dialogueBox = null!;

        private float _cameraX = 0f;
        private string _notifyMessage = "";
        private float _notifyTimer = 0f;

        public float ScreenShakeTimer { get; private set; } = 0f;
        public float ScreenShakeIntensity { get; private set; } = 0f;
        private float _hitStopTimer = 0f;

        public PixelBuffer Buffer => _buffer;
        public Input Input => _input;

        public void Initialize()
        {
            _status = new StatusSystem();
            _player = new Player(_status);
            _stageManager = new StageManager();
            _combatSystem = new CombatSystem();

            _hud = new HUD();
            _statusMenu = new StatusMenu();
            _shopMenu = new ShopMenu();
            _titleScreen = new TitleScreen();
            _pauseMenu = new PauseMenu();
            _dialogueBox = new DialogueBox();

            _combatSystem.OnScreenShake = (intensity, duration) => TriggerScreenShake(intensity, duration);
            _combatSystem.OnHitStop = (duration) => TriggerHitStop(duration);

            SoundSynth.PlayBgm("title");
        }

        public void TriggerScreenShake(float intensity, float duration)
        {
            ScreenShakeIntensity = intensity;
            ScreenShakeTimer = duration;
        }

        public void TriggerHitStop(float duration)
        {
            _hitStopTimer = Math.Max(_hitStopTimer, duration);
        }

        public void StartNewGame()
        {
            _status = new StatusSystem();
            _player = new Player(_status);
            _stageManager.GameWon = false;
            _stageManager.LoadStage(1);
            _combatSystem.Items.Clear();
            _combatSystem.Bubbles.Clear();
            _combatSystem.DamageNumbers.Clear();
            _cameraX = 0f;
            State = GameState.Playing;
            SoundSynth.PlayBgm("stage");
            ShowNotification(_stageManager.GetStageName(1) + "에 도달했습니다!");
        }

        public void LoadSavedGame()
        {
            _status = new StatusSystem();
            var data = SaveLoadSystem.Load(_status, out int stageIndex);
            if (data != null)
            {
                _player = new Player(_status);
                _stageManager.GameWon = false;
                _stageManager.LoadStage(stageIndex);
                _combatSystem.Items.Clear();
                _combatSystem.Bubbles.Clear();
                _combatSystem.DamageNumbers.Clear();
                _cameraX = 0f;
                State = GameState.Playing;
                SoundSynth.PlayBgm("stage");
                ShowNotification($"기록을 불러왔습니다! (LV.{_status.Level}, {_stageManager.GetStageName(_stageManager.CurrentStage)})");
            }
            else
            {
                StartNewGame();
            }
        }

        public void ShowNotification(string msg)
        {
            _notifyMessage = msg;
            _notifyTimer = 2.5f;
        }

        public void Update(float dt)
        {
            if (_notifyTimer > 0)
            {
                _notifyTimer -= dt;
            }

            switch (State)
            {
                case GameState.Title:
                    SoundSynth.PlayBgm("title");
                    _titleScreen.HandleInput(_input, out string titleAction);
                    if (titleAction == "new_game") StartNewGame();
                    else if (titleAction == "load_game") LoadSavedGame();
                    break;

                case GameState.Paused:
                    _pauseMenu.HandleInput(_input, out bool resume, out bool quitToTitle);
                    if (resume) State = GameState.Playing;
                    else if (quitToTitle)
                    {
                        State = GameState.Title;
                        SoundSynth.PlayBgm("title");
                    }
                    break;

                case GameState.StatusMenu:
                    _statusMenu.HandleInput(_input, _status, out bool shouldClose);
                    if (shouldClose) State = GameState.Playing;
                    break;

                case GameState.ShopMenu:
                    SoundSynth.PlayBgm("shop");
                    _shopMenu.HandleInput(_input, _status, _player, out bool closeShop);
                    if (closeShop)
                    {
                        State = GameState.Playing;
                        SoundSynth.PlayBgm((_stageManager.CurrentBoss != null && _stageManager.CurrentBoss.IsAlive) ? "boss" : "stage");
                    }
                    break;

                case GameState.GameOver:
                    SoundSynth.StopBgm();
                    if (_input.IsJustPressed(Keys.Z) || _input.IsJustPressed(Keys.Enter))
                    {
                        StartNewGame();
                    }
                    else if (_input.IsJustPressed(Keys.Escape))
                    {
                        State = GameState.Title;
                        SoundSynth.PlayBgm("title");
                    }
                    break;

                case GameState.GameWon:
                    SoundSynth.PlayBgm("victory");
                    if (_input.IsJustPressed(Keys.Z) || _input.IsJustPressed(Keys.Enter) || _input.IsJustPressed(Keys.Escape))
                    {
                        _stageManager.GameWon = false;
                        State = GameState.Title;
                        SoundSynth.PlayBgm("title");
                    }
                    break;

                case GameState.Playing:
                    UpdatePlaying(dt);
                    break;
            }

            _input.Update();
        }

        private void UpdatePlaying(float dt)
        {
            // 보스 조우 대화창 활성화 상태일 때는 대화 진행만 처리
            if (_dialogueBox.IsActive)
            {
                _dialogueBox.HandleInput(_input);
                _dialogueBox.Update(dt);
                return;
            }

            // ESC 키로 일시 정지 및 무예 보감 열기
            if (_input.IsJustPressed(Keys.Escape))
            {
                SoundSynth.PlayJump();
                State = GameState.Paused;
                return;
            }

            // 보스 조우 대화 검사 (플레이어가 보스 반경 180px 이내로 접근 시)
            if (_stageManager.CurrentBoss != null && _stageManager.CurrentBoss.IsAlive && !_dialogueBox.HasTriggered(_stageManager.CurrentStage))
            {
                if (Math.Abs(_player.X - _stageManager.CurrentBoss.X) < 180f)
                {
                    _dialogueBox.TriggerStageDialogue(_stageManager.CurrentStage);
                    return;
                }
            }

            // 보스 격파 피니셔 배너 검사
            if (_stageManager.CurrentBoss != null && _stageManager.CurrentBoss.CurrentHp <= 0 && !_dialogueBox.FinisherActive)
            {
                _dialogueBox.TriggerFinisher("격 파 (擊破)");
            }

            _dialogueBox.Update(dt);

            // BGM 전환 (보스 출현 여부)
            if (_stageManager.CurrentBoss != null && _stageManager.CurrentBoss.IsAlive)
            {
                SoundSynth.PlayBgm("boss");
            }
            else
            {
                SoundSynth.PlayBgm("stage");
            }

            // 화면 흔들림 감쇠
            if (ScreenShakeTimer > 0)
            {
                ScreenShakeTimer -= dt;
            }

            // 역경직(Hit-Stop) 처리: 타격 순간 찰나의 정지 효과
            if (_hitStopTimer > 0)
            {
                _hitStopTimer -= dt;
                _combatSystem.Update(dt);
                return;
            }

            // 빠른 저장 (F5) / 빠른 불러오기 (F9)
            if (_input.IsJustPressed(Keys.F5))
            {
                if (SaveLoadSystem.Save(_status, _stageManager.CurrentStage))
                {
                    SoundSynth.PlayCoin();
                    ShowNotification("기록(Save)을 무사히 마쳤습니다!");
                }
            }
            else if (_input.IsJustPressed(Keys.F9))
            {
                LoadSavedGame();
                return;
            }

            // 상태 메뉴 열기 (Tab)
            if (_input.IsJustPressed(Keys.Tab))
            {
                SoundSynth.PlayJump();
                State = GameState.StatusMenu;
                return;
            }

            // 주막(상점) 열기 (S)
            if (_input.IsJustPressed(Keys.S))
            {
                SoundSynth.PlayCoin();
                State = GameState.ShopMenu;
                return;
            }

            // 플레이어 조작 & 업데이트
            _player.HandleInput(_input, _stageManager.WorldObjects, _stageManager.Enemies);
            _player.Update(dt);

            // 카메라 추적 (좌우 횡스크롤 한계)
            float targetCamX = _player.X - PixelBuffer.VirtualWidth / 2f;
            float maxCamX = Math.Max(0, _stageManager.StageWidth - PixelBuffer.VirtualWidth);
            _cameraX = Math.Clamp(targetCamX, 0, maxCamX);

            // 스테이지 및 적 AI 업데이트
            _stageManager.Update(_player, dt);

            // 전투 판정
            _combatSystem.CheckCollisions(_player, _stageManager.Enemies, _stageManager.WorldObjects);
            _combatSystem.Update(dt);

            // 다음 구역 진입 판정
            if (_stageManager.StageCleared && _player.X >= _stageManager.StageWidth - 24f)
            {
                int nextStage = _stageManager.CurrentStage + 1;
                _stageManager.LoadStage(nextStage);
                _player.X = 35f;
                _cameraX = 0f;
                SoundSynth.PlayLevelUp();
                ShowNotification(_stageManager.GetStageName(nextStage) + "에 진입했습니다!");
            }

            // 게임 오버 검사
            if (_status.CurrentHp <= 0 && _player.State == "down" && _player.StateTimer > 0.8f)
            {
                State = GameState.GameOver;
            }

            // 보스 클리어 검사
            if (_stageManager.GameWon)
            {
                State = GameState.GameWon;
            }
        }

        public void Render()
        {
            _buffer.Clear(Color.Black);

            switch (State)
            {
                case GameState.Title:
                    _titleScreen.Render(_buffer);
                    break;

                case GameState.Playing:
                case GameState.Paused:
                case GameState.StatusMenu:
                case GameState.ShopMenu:
                    RenderPlaying();
                    if (State == GameState.Paused)
                    {
                        _pauseMenu.Render(_buffer);
                    }
                    else if (State == GameState.StatusMenu)
                    {
                        _statusMenu.Render(_buffer, _status);
                    }
                    else if (State == GameState.ShopMenu)
                    {
                        _shopMenu.Render(_buffer, _status);
                    }
                    break;

                case GameState.GameOver:
                    RenderPlaying();
                    RenderGameOver();
                    break;

                case GameState.GameWon:
                    RenderGameWon();
                    break;
            }

            // 상단 알림 배너 팝업 (960x720 고해상도)
            if (_notifyTimer > 0 && !string.IsNullOrEmpty(_notifyMessage))
            {
                var size = _buffer.Graphics.MeasureString(_notifyMessage, _buffer.FontSubtitle);
                int notiW = (int)size.Width + 48;
                int notiH = 46;
                int notiX = (PixelBuffer.Width - notiW) / 2;
                int notiY = 82;

                _buffer.FillRoundedRect(notiX, notiY, notiW, notiH, 10f, Color.FromArgb(235, 20, 20, 30));
                _buffer.DrawRoundedRect(notiX, notiY, notiW, notiH, 10f, Color.FromArgb(255, 215, 70), 2f);
                _buffer.DrawOutlinedTextCentered(_notifyMessage, notiY + 9, Color.FromArgb(255, 240, 160), Color.FromArgb(40, 25, 10), _buffer.FontSubtitle, 1.8f);
            }

            // 960x720 고해상도 모드 유지
            _buffer.DrawScanlines();
        }

        private void RenderPlaying()
        {
            // 화면 흔들림 오프셋 계산
            float renderCamX = _cameraX;
            if (ScreenShakeTimer > 0)
            {
                renderCamX += (float)(Math.Sin(ScreenShakeTimer * 65f) * ScreenShakeIntensity);
            }

            // 1. 960x720 고해상도 배경 & 바닥 렌더링
            _stageManager.RenderBackground(_buffer, renderCamX);

            // 2. 2.5D 깊이 정렬 렌더링 (Y 좌표 기준 오름차순, 동일 깊이는 Z 기준 오름차순)
            var renderList = new List<Entity>();
            renderList.Add(_player);
            renderList.AddRange(_stageManager.Enemies);
            renderList.AddRange(_stageManager.WorldObjects);
            renderList.AddRange(_combatSystem.Items);

            renderList.Sort((a, b) =>
            {
                int cmp = a.Y.CompareTo(b.Y);
                if (cmp != 0) return cmp;
                return a.Z.CompareTo(b.Z);
            });

            // 오브젝트는 도트 스프라이트 그대로 3배 정수 확대 출력
            foreach (var entity in renderList)
            {
                entity.Render(_buffer, renderCamX);
            }

            // 3. 960x720 고해상도 타격 이펙트, 말풍선, 데미지 플로팅 텍스트
            _combatSystem.Render(_buffer, renderCamX);

            // 4. 동적 날씨 파티클 및 환경 기믹 (단풍, 안개, 비, 번개, 화살)
            _stageManager.RenderWeatherAndHazards(_buffer, renderCamX);

            // 5. 960x720 고해상도 다음 구역 화살표
            _stageManager.RenderStageClearArrow(_buffer);

            // 6. 960x720 고해상도 상단 HUD & 하단 보스 게이지
            _hud.Render(_buffer, _player, _stageManager.CurrentBoss, _stageManager.CurrentStage);

            // 7. 보스전 대화창 및 KO 격파 배너
            _dialogueBox.Render(_buffer);
        }

        private void RenderGameOver()
        {
            _buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(195, 10, 5, 15));
            _buffer.DrawOutlinedTextCentered("패 배 하 였 소", 180, Color.FromArgb(255, 65, 65), Color.Black, _buffer.FontHuge, 4f);
            _buffer.DrawOutlinedTextCentered("유생 한랑이 저잣거리에서 쓰러졌습니다.", 270, Color.FromArgb(230, 230, 230), Color.Black, _buffer.FontSubtitle, 2f);

            int btnY = 380;
            _buffer.FillRoundedRect((PixelBuffer.Width - 360) / 2, btnY, 360, 50, 8f, Color.FromArgb(200, 40, 25, 25));
            _buffer.DrawRoundedRect((PixelBuffer.Width - 360) / 2, btnY, 360, 50, 8f, Color.FromArgb(255, 215, 60), 2f);
            _buffer.DrawOutlinedTextCentered("[ Z 키 : 다시 도전 ]", btnY + 11, Color.FromArgb(255, 240, 90), Color.Black, _buffer.FontHeader, 2f);

            int btn2Y = 450;
            _buffer.FillRoundedRect((PixelBuffer.Width - 360) / 2, btn2Y, 360, 45, 8f, Color.FromArgb(160, 25, 25, 30));
            _buffer.DrawRoundedRect((PixelBuffer.Width - 360) / 2, btn2Y, 360, 45, 8f, Color.FromArgb(150, 150, 170), 1.5f);
            _buffer.DrawOutlinedTextCentered("[ ESC 키 : 타이틀로 복귀 ]", btn2Y + 11, Color.FromArgb(200, 200, 210), Color.Black, _buffer.FontBody, 1.8f);
        }

        private void RenderGameWon()
        {
            _buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(245, 15, 15, 30));

            _buffer.DrawOutlinedTextCentered("★ 훈민정음 탈환 대성공! ★", 50, Color.FromArgb(255, 225, 50), Color.FromArgb(60, 40, 10), _buffer.FontHuge, 4f);
            _buffer.DrawOutlinedTextCentered("10월 9일 한글날, 세종대왕의 뜻을 온전히 지켜냈소!", 120, Color.FromArgb(255, 245, 190), Color.Black, _buffer.FontSubtitle, 2f);

            int boxX = 80;
            int boxY = 180;
            int boxW = 800;
            int boxH = 340;
            _buffer.FillRoundedRect(boxX, boxY, boxW, boxH, 12f, Color.FromArgb(248, 244, 230));
            _buffer.DrawRoundedRect(boxX, boxY, boxW, boxH, 12f, Color.FromArgb(160, 110, 40), 3f);
            _buffer.DrawRoundedRect(boxX + 5, boxY + 5, boxW - 10, boxH - 10, 8f, Color.FromArgb(215, 185, 130), 1.5f);

            _buffer.DrawTextCentered("『나랏말ᄊᆞ미中듀ᇰ國귁에달아文문字ᄍᆞᆼ와로서르ᄉᆞᄆᆞᆺ디아니ᄒᆞᆯᄊᆡ", boxY + 35, Color.FromArgb(30, 20, 10), large: true);
            _buffer.DrawTextCentered("이런젼ᄎᆞ로어린百ᄇᆡᆨ姓셔ᇰ이니르고져호ᇙ배이셔도", boxY + 95, Color.FromArgb(30, 20, 10), large: true);
            _buffer.DrawTextCentered("ᄆᆞᄎᆞᆷ내제ᄠᅳ들시러펴디몯ᄒᆞᇙ노미하니라", boxY + 155, Color.FromArgb(30, 20, 10), large: true);
            _buffer.DrawTextCentered("내이ᄅᆞᆯ為윙ᄒᆞ야어엿비너겨새로스믈여듧字ᄍᆞᆼᄅᆞᆯᄆᆡᇰᄀᆞ노니...』", boxY + 225, Color.FromArgb(170, 20, 20), large: true);

            _buffer.DrawOutlinedTextCentered("유생 한랑의 쾌거로 온 백성이 자유로이 뜻을 펴게 되었도다!", 560, Color.FromArgb(120, 240, 140), Color.Black, _buffer.FontSubtitle, 2f);
            _buffer.DrawOutlinedTextCentered("[ Z 키 또는 ESC 키를 눌러 타이틀로 복귀 ]", 620, Color.FromArgb(220, 220, 220), Color.Black, _buffer.FontHeader, 1.8f);
        }
    }
}
