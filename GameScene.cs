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
        GameWon,
        RelicSelect
    }

    public record RelicOption(string Id, string Name, string Hanja, string Effect, Color Color);

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
        private float _gameOverTimer = 0f;

        public static readonly List<RelicOption> AllRelics = new()
        {
            new RelicOption("cheolgap", "철갑 흉경", "鐵甲胸鏡", "방어력 +6, 상시 받는 피해 4 감소", Color.FromArgb(70, 130, 180)),
            new RelicOption("spark", "단전 벼락구슬", "丹田雷珠", "비기 피해 +40%, 저스트 패링 시 적에게 낙뢰 28 방출", Color.FromArgb(255, 215, 0)),
            new RelicOption("shoes", "풍운의 짚신", "風雲草履", "공중 2단 점프 활성화 및 도약력 대폭 상승", Color.FromArgb(144, 238, 144)),
            new RelicOption("dokkaebi", "도깨비 감투", "鬼之冠", "대시 속도 +20%, 대시 돌진 중 완전 무적", Color.FromArgb(186, 85, 211)),
            new RelicOption("tiger", "백호의 발톱", "白虎之爪", "모든 공격 20% 확률 치명타(1.5배) & 적 타격 시 엽전 드롭", Color.FromArgb(255, 140, 0)),
            new RelicOption("gourd", "주모의 약주단지", "酒母藥酒壺", "적 처치 시 35% 확률로 주먹밥 즉시 드롭", Color.FromArgb(210, 105, 30)),
            new RelicOption("medallion", "금강역사의 패", "金剛力士牌", "최대 HP +30 증가, 피격 경직에 완전 면역 (슈퍼아머)", Color.FromArgb(220, 20, 60)),
            new RelicOption("mapae", "암행어사 마패", "暗行御史馬牌", "콤보 지속시간 +1.5초 연장, 10콤보 주기마다 MP +10 즉시 회복", Color.FromArgb(50, 205, 50))
        };

        private List<RelicOption> _currentRelicChoices = new();
        private int _selectedRelicIdx = 0;

        public float ScreenShakeTimer { get; private set; } = 0f;
        public float ScreenShakeIntensity { get; private set; } = 0f;
        private float _hitStopTimer = 0f;

        public PixelBuffer Buffer => _buffer;
        public Input Input => _input;
        public Player Player => _player;
        public StatusSystem Status => _status;
        public StageManager StageManager => _stageManager;

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

            // 공격/피격 시 화면 흔들림을 없애고, 공격자와 맞은 캐릭터의 개별 Hit-Stop 프리즈로 타격감 구현
            _combatSystem.OnScreenShake = (intensity, duration) => { /* 공격이나 피격 시 화면 흔들림 비활성화 */ };
            _combatSystem.OnHitStop = (duration) => { /* 공격자 및 피격자 개별 HitStop으로 타격감 구현 */ };

            SoundSynth.PlayBgm("title");
        }

        public void TriggerScreenShake(float intensity, float duration)
        {
            // 공격/피격 시 화면 흔들림 비활성화 (화면 안정화)
            ScreenShakeIntensity = 0f;
            ScreenShakeTimer = 0f;
        }

        public void TriggerHitStop(float duration)
        {
            _hitStopTimer = Math.Max(_hitStopTimer, duration);
        }

        public string GetCurrentGameplayBgmTrack()
        {
            if (_stageManager.CurrentBoss != null && _stageManager.CurrentBoss.IsAlive)
            {
                return (_stageManager.CurrentStage == StageManager.MaxStage) ? "finalboss" : "midboss";
            }
            return MidiGenerator.GetStageBgmTrack(_stageManager.CurrentStage);
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
            SoundSynth.PlayBgm(GetCurrentGameplayBgmTrack());
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
                SoundSynth.PlayBgm(GetCurrentGameplayBgmTrack());
                ShowNotification($"기록을 불러왔습니다! (LV.{_status.Level}, {_stageManager.GetStageName(_stageManager.CurrentStage)})");
            }
            else
            {
                StartNewGame();
            }
        }

        public void RespawnAtPreviousStage()
        {
            if (_stageManager.IsSurvivalMode)
            {
                StartSurvivalMode();
                return;
            }

            int originalStage = _stageManager.CurrentStage;
            int prevStage = Math.Max(1, originalStage - 1);
            _stageManager.LoadStage(prevStage);

            // 체력과 기력 완전 회복
            _status.CurrentHp = _status.MaxHp;
            _status.CurrentMp = _status.MaxMp;

            // 플레이어 위치 및 상태 초기화 (이전 스테이지 입구)
            _player.X = 35f;
            _player.Y = 180f;
            _player.Z = 0f;
            _player.Vx = 0f;
            _player.Vy = 0f;
            _player.Vz = 0f;
            _player.State = "idle";
            _player.StateTimer = 0f;
            _player.InvincibleTimer = 2.0f; // 부활 후 2초간 안전 무적

            // 카메라 및 필드 오브젝트 리셋
            _cameraX = 0f;
            _combatSystem.Items.Clear();
            _combatSystem.Bubbles.Clear();
            _combatSystem.DamageNumbers.Clear();
            _dialogueBox.Reset(prevStage);

            _gameOverTimer = 0f;
            State = GameState.Playing;

            SoundSynth.PlayBgm(GetCurrentGameplayBgmTrack());

            if (originalStage > 1)
            {
                ShowNotification($"패배하여 이전 구역({_stageManager.GetStageName(prevStage)})으로 후퇴했습니다.");
            }
            else
            {
                ShowNotification("1장에서 다시 기운을 차리고 일어섰습니다!");
            }
        }

        public void StartSurvivalMode()
        {
            _status = new StatusSystem();
            _player = new Player(_status);
            _stageManager.LoadSurvivalWave(1);
            _combatSystem.Items.Clear();
            _combatSystem.Bubbles.Clear();
            _combatSystem.DamageNumbers.Clear();
            _cameraX = 0f;
            State = GameState.Playing;
            SoundSynth.PlayBgm("boss");
            ShowNotification("무한 연무장 개시! 끝없이 몰려드는 적을 섬멸하시오!");
        }

        private void TriggerRelicSelect()
        {
            var unobtained = AllRelics.FindAll(r => !_status.HasRelic(r.Id));
            if (unobtained.Count == 0)
            {
                ProceedToNextStageOrWave();
                return;
            }

            var rng = new Random();
            var shuffled = new List<RelicOption>(unobtained);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int k = rng.Next(i + 1);
                (shuffled[i], shuffled[k]) = (shuffled[k], shuffled[i]);
            }
            _currentRelicChoices = shuffled.GetRange(0, Math.Min(3, shuffled.Count));
            _selectedRelicIdx = 0;
            State = GameState.RelicSelect;
            SoundSynth.PlaySpecial();
        }

        private void ProceedToNextStageOrWave()
        {
            if (_stageManager.IsSurvivalMode)
            {
                int nextWave = _stageManager.SurvivalWave + 1;
                _stageManager.LoadSurvivalWave(nextWave, _player);
                _player.X = 35f;
                _cameraX = 0f;
                _player.Vx = 0f;
                _player.Vy = 0f;
                _player.State = "idle";
                _player.SyncCarriedPositions();

                // 들고 온 기물 및 장착 무기가 필드 오브젝트 리스트에 누락되지 않도록 보장
                if (_player.CarriedObject != null && !_stageManager.WorldObjects.Contains(_player.CarriedObject))
                    _stageManager.WorldObjects.Add(_player.CarriedObject);
                if (_player.EquippedWeapon != null && !_stageManager.WorldObjects.Contains(_player.EquippedWeapon))
                    _stageManager.WorldObjects.Add(_player.EquippedWeapon);
                if (_player.CarriedEnemy != null && !_stageManager.Enemies.Contains(_player.CarriedEnemy))
                    _stageManager.Enemies.Add(_player.CarriedEnemy);

                _status.CurrentHp = Math.Min(_status.MaxHp, _status.CurrentHp + 45);
                _status.CurrentMp = _status.MaxMp;
                SoundSynth.PlayLevelUp();
                ShowNotification($"연무장 제 {nextWave} 파도가 시작되었습니다!");
                State = GameState.Playing;
            }
            else
            {
                int nextStage = _stageManager.CurrentStage + 1;
                _stageManager.LoadStage(nextStage, _player);
                _player.X = 35f;
                _cameraX = 0f;
                _player.Vx = 0f;
                _player.Vy = 0f;
                _player.State = "idle";
                _player.SyncCarriedPositions();

                // 들고 온 기물 및 장착 무기가 필드 오브젝트 리스트에 누락되지 않도록 보장
                if (_player.CarriedObject != null && !_stageManager.WorldObjects.Contains(_player.CarriedObject))
                    _stageManager.WorldObjects.Add(_player.CarriedObject);
                if (_player.EquippedWeapon != null && !_stageManager.WorldObjects.Contains(_player.EquippedWeapon))
                    _stageManager.WorldObjects.Add(_player.EquippedWeapon);
                if (_player.CarriedEnemy != null && !_stageManager.Enemies.Contains(_player.CarriedEnemy))
                    _stageManager.Enemies.Add(_player.CarriedEnemy);

                SoundSynth.PlayLevelUp();
                ShowNotification(_stageManager.GetStageName(nextStage) + "에 진입했습니다!");
                State = GameState.Playing;
            }
        }

        private void UpdateRelicSelect()
        {
            if (_input.IsJustPressed(Keys.Left))
            {
                _selectedRelicIdx = (_selectedRelicIdx - 1 + _currentRelicChoices.Count) % _currentRelicChoices.Count;
                SoundSynth.PlayJump();
            }
            else if (_input.IsJustPressed(Keys.Right))
            {
                _selectedRelicIdx = (_selectedRelicIdx + 1) % _currentRelicChoices.Count;
                SoundSynth.PlayJump();
            }
            else if (_input.IsJustPressed(Keys.Z) || _input.IsJustPressed(Keys.Enter))
            {
                if (_currentRelicChoices.Count > 0)
                {
                    var chosen = _currentRelicChoices[_selectedRelicIdx];
                    _status.AddRelic(chosen.Id);
                    SoundSynth.PlayLevelUp();
                    ShowNotification($"기연 유물 [ {chosen.Name} ] 을(를) 획득하였소!");
                }
                ProceedToNextStageOrWave();
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
                    else if (titleAction == "survival_mode") StartSurvivalMode();
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
                        SoundSynth.PlayBgm(GetCurrentGameplayBgmTrack());
                    }
                    break;

                case GameState.RelicSelect:
                    UpdateRelicSelect();
                    break;

                case GameState.GameOver:
                    SoundSynth.StopBgm();
                    _gameOverTimer += dt;
                    if (_input.IsJustPressed(Keys.Z) || _input.IsJustPressed(Keys.Enter) || _gameOverTimer >= 2.5f)
                    {
                        RespawnAtPreviousStage();
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

            // 보스 격파 피니셔 배너 검사 (완전소탕 / 격파 슬로우모션 연출)
            if (_stageManager.CurrentBoss != null && _stageManager.CurrentBoss.CurrentHp <= 0 && !_dialogueBox.FinisherActive)
            {
                string finisherTitle = (_stageManager.CurrentStage == StageManager.MaxStage)
                    ? "완 전 소 탕 (完 全 掃 蕩)"
                    : "격 파 (擊破)";
                _dialogueBox.TriggerFinisher(finisherTitle);
                _hitStopTimer = 0.60f; // 보스 격파 순간의 극적인 역경직/슬로우모션
                _player.TriggerHitStop(0.60f);
                _stageManager.CurrentBoss.TriggerHitStop(0.60f);
                ScreenShakeTimer = 0f;
                ScreenShakeIntensity = 0f;
            }

            _dialogueBox.Update(dt);

            // BGM 전환 (보스 출현 여부 및 스테이지별 테마곡)
            SoundSynth.PlayBgm(GetCurrentGameplayBgmTrack());

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

            // 보스 합(合) 힘겨루기 QTE 연타 처리
            if (_combatSystem.IsClashing)
            {
                if (_input.IsJustPressed(Keys.Z) || _input.IsJustPressed(Keys.X) || _input.IsJustPressed(Keys.Space))
                {
                    _combatSystem.RegisterClashInput(_player);
                }
                _combatSystem.Update(dt);
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

            // 다음 구역 진입 또는 연무장 파도 진행 판정 (기연 유물 3지선다 선택 연계)
            if (_stageManager.IsSurvivalMode)
            {
                if (_stageManager.StageCleared)
                {
                    TriggerRelicSelect();
                    return;
                }
            }
            else
            {
                if (_stageManager.StageCleared && _player.X >= _stageManager.StageWidth - 24f)
                {
                    TriggerRelicSelect();
                    return;
                }
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
                case GameState.RelicSelect:
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
                    else if (State == GameState.RelicSelect)
                    {
                        RenderRelicSelect();
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

            // 6. 960x720 고해상도 상단 HUD & 하단 보스 게이지 & 콤보/스타일 랭크/합 QTE
            _hud.Render(_buffer, _player, _stageManager.CurrentBoss, _stageManager.CurrentStage, _combatSystem);

            // 7. 보스전 대화창 및 KO 격파 배너
            _dialogueBox.Render(_buffer);
        }

        private void RenderRelicSelect()
        {
            _buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(210, 12, 10, 22));

            _buffer.DrawOutlinedTextCentered("【 기 연 (奇 緣) : 조 선 8 대 유 물 전 승 】", 100, Color.FromArgb(255, 225, 60), Color.Black, _buffer.FontHuge, 4f);
            _buffer.DrawOutlinedTextCentered("강호의 기운이 깃든 신비한 보구를 하나 선택하여 능력을 전승받으시오.", 175, Color.FromArgb(235, 230, 200), Color.Black, _buffer.FontSubtitle, 1.8f);

            int count = _currentRelicChoices.Count;
            if (count == 0) return;

            int cardW = 270;
            int cardH = 350;
            int gap = 24;
            int totalW = count * cardW + (count - 1) * gap;
            int startX = (PixelBuffer.Width - totalW) / 2;
            int startY = 225;

            for (int i = 0; i < count; i++)
            {
                var relic = _currentRelicChoices[i];
                bool isSelected = (i == _selectedRelicIdx);
                int cx = startX + i * (cardW + gap);
                int cy = startY;

                if (isSelected)
                {
                    cy -= 10;
                    _buffer.FillRoundedRect(cx, cy, cardW, cardH, 14f, Color.FromArgb(245, 45, 35, 52));
                    _buffer.DrawRoundedRect(cx, cy, cardW, cardH, 14f, Color.FromArgb(255, 230, 80), 3.5f);
                }
                else
                {
                    _buffer.FillRoundedRect(cx, cy, cardW, cardH, 14f, Color.FromArgb(190, 25, 22, 35));
                    _buffer.DrawRoundedRect(cx, cy, cardW, cardH, 14f, Color.FromArgb(120, 110, 130), 2f);
                }

                float cardCenterX = cx + (cardW / 2f);

                // 카드 상단 한자 뱃지
                _buffer.FillRoundedRect(cx + 15, cy + 16, cardW - 30, 56, 8f, Color.FromArgb(220, 18, 16, 26));
                _buffer.DrawRoundedRect(cx + 15, cy + 16, cardW - 30, 56, 8f, relic.Color, 1.5f);
                _buffer.DrawOutlinedTextCenteredAt(relic.Hanja, cardCenterX, cy + 24, relic.Color, Color.Black, _buffer.FontTitle, 2f);

                // 유물 이름
                Color nameCol = isSelected ? Color.FromArgb(255, 240, 100) : Color.FromArgb(240, 240, 240);
                _buffer.DrawOutlinedTextCenteredAt(relic.Name, cardCenterX, cy + 90, nameCol, Color.Black, _buffer.FontSubtitle, 1.8f);

                // 분할선
                _buffer.DrawLine(cx + 25, cy + 124, cx + cardW - 25, cy + 124, Color.FromArgb(150, 130, 90), 1.5f);

                // 효과 설명 패널 (안정적인 다크 비단 배경 박스)
                int descBoxX = cx + 14;
                int descBoxY = cy + 130;
                int descBoxW = cardW - 28;
                int descBoxH = cardH - 192;
                _buffer.FillRoundedRect(descBoxX, descBoxY, descBoxW, descBoxH, 6f, Color.FromArgb(140, 15, 12, 22));
                _buffer.DrawRoundedRect(descBoxX, descBoxY, descBoxW, descBoxH, 6f, Color.FromArgb(70, 140, 120, 80), 1f);

                // 효과 설명 자동 줄바꿈 렌더링 (최대 가용 폭: cardW - 40 = 230px, 화면 우측 잘림 완벽 방지)
                float maxTextWidth = cardW - 40f;
                var font = _buffer.FontBody;
                var effectLines = WrapRelicEffectText(relic.Effect, font, maxTextWidth, _buffer.Graphics);

                int lineCount = effectLines.Count;
                int lineSpacing = (lineCount >= 4) ? 22 : (lineCount == 3 ? 24 : 26);
                int totalTextH = lineCount * lineSpacing;
                int effStartY = descBoxY + Math.Max(8, (descBoxH - totalTextH) / 2);

                for (int l = 0; l < effectLines.Count; l++)
                {
                    float lineY = effStartY + l * lineSpacing;
                    _buffer.DrawOutlinedTextCenteredAt(effectLines[l], cardCenterX, lineY, Color.FromArgb(235, 230, 215), Color.Black, font, 1.2f);
                }

                // 하단 선택 표시
                if (isSelected)
                {
                    _buffer.FillRoundedRect(cx + 30, cy + cardH - 52, cardW - 60, 36, 6f, Color.FromArgb(230, 200, 50));
                    _buffer.DrawOutlinedTextCenteredAt("★ 전 승 ★", cardCenterX, cy + cardH - 46, Color.FromArgb(40, 20, 10), Color.Transparent, _buffer.FontHeader, 1.5f);
                }
            }

            _buffer.DrawOutlinedTextCentered("[ ← → 방향키 : 선택  |  Z / Enter : 기연 전승 ]", 615, Color.FromArgb(255, 235, 140), Color.Black, _buffer.FontHeader, 1.8f);
        }

        /// <summary>
        /// 유물 카드의 폭(270px) 내부 안전 영역(230px)에 맞춰 긴 유물 설명을 단어/구문 단위로 자동 줄바꿈합니다.
        /// </summary>
        public static List<string> WrapRelicEffectText(string effect, Font font, float maxLineWidth, Graphics g)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(effect)) return result;

            // 콤마(,) 단위로 주요 효과 구문 분할
            var clauses = effect.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            foreach (var clause in clauses)
            {
                if (g.MeasureString(clause, font).Width <= maxLineWidth)
                {
                    result.Add(clause);
                }
                else
                {
                    // 구문이 maxLineWidth보다 길 경우 공백(단어) 단위로 추가 줄바꿈
                    string[] words = clause.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string curLine = "";
                    foreach (var word in words)
                    {
                        string testLine = string.IsNullOrEmpty(curLine) ? word : curLine + " " + word;
                        if (g.MeasureString(testLine, font).Width <= maxLineWidth)
                        {
                            curLine = testLine;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(curLine))
                            {
                                result.Add(curLine);
                                curLine = word;
                            }
                            else
                            {
                                // 단어 하나가 이미 maxLineWidth를 초과하는 경우 음절 단위 분할
                                string sub = "";
                                foreach (char c in word)
                                {
                                    if (g.MeasureString(sub + c, font).Width <= maxLineWidth)
                                    {
                                        sub += c;
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(sub)) result.Add(sub);
                                        sub = c.ToString();
                                    }
                                }
                                curLine = sub;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(curLine))
                    {
                        result.Add(curLine);
                    }
                }
            }
            return result;
        }

        private void RenderGameOver()
        {
            _buffer.FillRect(0, 0, PixelBuffer.Width, PixelBuffer.Height, Color.FromArgb(195, 10, 5, 15));
            _buffer.DrawOutlinedTextCentered("패 배 하 였 소", 180, Color.FromArgb(255, 65, 65), Color.Black, _buffer.FontHuge, 4f);

            int currentStage = _stageManager.CurrentStage;
            int prevStage = Math.Max(1, currentStage - 1);
            string currentStageName = _stageManager.GetStageName(currentStage);

            if (_stageManager.IsSurvivalMode)
            {
                _buffer.DrawOutlinedTextCentered($"유생 한랑이 연무장 제 {_stageManager.SurvivalWave} 파도에서 장렬히 쓰러졌습니다.", 270, Color.FromArgb(230, 230, 230), Color.Black, _buffer.FontSubtitle, 2f);
            }
            else
            {
                _buffer.DrawOutlinedTextCentered($"유생 한랑이 {currentStageName}에서 쓰러졌습니다.", 270, Color.FromArgb(230, 230, 230), Color.Black, _buffer.FontSubtitle, 2f);
            }

            string respawnLabel = _stageManager.IsSurvivalMode
                ? "[ Z 키 : 제 1 파도부터 재도전 ]"
                : (currentStage > 1
                    ? $"[ Z 키 : {_stageManager.GetStageName(prevStage)}에서 부활 ]"
                    : "[ Z 키 : 1장에서 다시 도전 ]");

            int btnW = 540;
            int btnY = 380;
            _buffer.FillRoundedRect((PixelBuffer.Width - btnW) / 2, btnY, btnW, 50, 8f, Color.FromArgb(200, 40, 25, 25));
            _buffer.DrawRoundedRect((PixelBuffer.Width - btnW) / 2, btnY, btnW, 50, 8f, Color.FromArgb(255, 215, 60), 2f);
            _buffer.DrawOutlinedTextCentered(respawnLabel, btnY + 11, Color.FromArgb(255, 240, 90), Color.Black, _buffer.FontHeader, 2f);

            int btn2Y = 450;
            _buffer.FillRoundedRect((PixelBuffer.Width - btnW) / 2, btn2Y, btnW, 45, 8f, Color.FromArgb(160, 25, 25, 30));
            _buffer.DrawRoundedRect((PixelBuffer.Width - btnW) / 2, btn2Y, btnW, 45, 8f, Color.FromArgb(150, 150, 170), 1.5f);
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
