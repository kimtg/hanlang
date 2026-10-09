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

                case GameState.StatusMenu:
                    _statusMenu.HandleInput(_input, _status, out bool shouldClose);
                    if (shouldClose) State = GameState.Playing;
                    break;

                case GameState.ShopMenu:
                    _shopMenu.HandleInput(_input, _status, _player, out bool closeShop);
                    if (closeShop) State = GameState.Playing;
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
                    SoundSynth.PlayBgm("title");
                    if (_input.IsJustPressed(Keys.Z) || _input.IsJustPressed(Keys.Enter) || _input.IsJustPressed(Keys.Escape))
                    {
                        _stageManager.GameWon = false;
                        State = GameState.Title;
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
            float targetCamX = _player.X - PixelBuffer.Width / 2f;
            float maxCamX = Math.Max(0, _stageManager.StageWidth - PixelBuffer.Width);
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
                case GameState.StatusMenu:
                case GameState.ShopMenu:
                    RenderPlaying();
                    if (State == GameState.StatusMenu)
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

            // 상단 알림 배너 팝업
            if (_notifyTimer > 0 && !string.IsNullOrEmpty(_notifyMessage))
            {
                int notiW = _notifyMessage.Length * 12 + 16;
                int notiX = (PixelBuffer.Width - notiW) / 2;
                int notiY = 32;

                _buffer.FillRect(notiX, notiY, notiW, 16, Color.FromArgb(220, 10, 10, 20));
                _buffer.DrawRect(notiX, notiY, notiW, 16, Color.FromArgb(240, 200, 60));
                _buffer.DrawTextCentered(_notifyMessage, notiY + 1, Color.FromArgb(255, 230, 120));
            }

            // 레트로 CRT 스캔라인 효과
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

            // 1. 배경
            _stageManager.RenderBackground(_buffer, renderCamX);

            // 2. 2.5D 깊이 정렬 렌더링 (Y 좌표 기준 오름차순, 동일 깊이는 Z 기준 오름차순으로 그리기)
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

            foreach (var entity in renderList)
            {
                entity.Render(_buffer, renderCamX);
            }

            // 3. 말풍선 및 파티클 렌더링
            _combatSystem.Render(_buffer, renderCamX);

            // 4. 다음 구역 화살표
            _stageManager.RenderStageClearArrow(_buffer);

            // 5. 상단 HUD & 보스 게이지
            _hud.Render(_buffer, _player, _stageManager.CurrentBoss, _stageManager.CurrentStage);
        }

        private void RenderGameOver()
        {
            _buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(170, 0, 0, 0));
            _buffer.DrawTextCentered("패 배 하 였 소", 70, Color.FromArgb(255, 60, 60), large: true);
            _buffer.DrawTextCentered("유생 한랑이 저잣거리에서 쓰러졌습니다.", 100, Color.FromArgb(220, 220, 220));

            _buffer.DrawTextCentered("[ Z 키: 다시 도전 ]", 140, Color.FromArgb(255, 240, 80));
            _buffer.DrawTextCentered("[ ESC 키: 타이틀로 복귀 ]", 160, Color.FromArgb(180, 180, 190));
        }

        private void RenderGameWon()
        {
            _buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(20, 15, 30));

            _buffer.DrawTextCentered("★ 훈민정음 탈환 대성공! ★", 35, Color.FromArgb(255, 220, 40), large: true);
            _buffer.DrawTextCentered("10월 9일 한글날, 세종대왕의 뜻을 지켜냈소!", 65, Color.FromArgb(255, 240, 180));

            int boxX = 30;
            int boxY = 90;
            int boxW = 260;
            int boxH = 95;
            _buffer.FillRect(boxX, boxY, boxW, boxH, Color.FromArgb(245, 240, 225));
            _buffer.DrawRect(boxX, boxY, boxW, boxH, Color.FromArgb(180, 130, 60));

            _buffer.DrawTextCentered("『나랏말싸미 듕귁에 달아 문자와로 서르 사맛디 아니할ᄊᆡ", boxY + 10, Color.FromArgb(40, 30, 20));
            _buffer.DrawTextCentered("이런 젼차로 어린 백셩이 니르고져 홇 배 이셔도", boxY + 28, Color.FromArgb(40, 30, 20));
            _buffer.DrawTextCentered("마ᄎᆞᆷ내 제 ᛯ들 시러 펴디 몯ᄒᆞᇙ 노미 하니라.", boxY + 46, Color.FromArgb(40, 30, 20));
            _buffer.DrawTextCentered("내 이ᄅᆞᆯ 위ᄒᆞ야 어엿비 너겨 새로 스믈여듧 字ᄅᆞᆯ ᄆᆡᇰᄀᆞ노니...』", boxY + 64, Color.FromArgb(180, 20, 20));

            _buffer.DrawTextCentered("유생 한랑의 활약으로 온 백성이 한글을 누리게 되었도다!", 195, Color.FromArgb(100, 230, 120));
            _buffer.DrawTextCentered("[ Z 키를 눌러 타이틀로 복귀 ]", 215, Color.FromArgb(200, 200, 200));
        }
    }
}
