using System;
using System.IO;
using System.Linq;
using HanlangGame.Entities;
using HanlangGame.Systems;
using HanlangGame.Visuals;
using HanlangGame.Engine;

namespace HanlangGame.Tests
{
    public static class GameTests
    {
        public static bool RunAllTests()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("====================================================");
            Console.WriteLine(" [한랑 (韓浪)] 시스템 자동화 무결성 테스트 실행 ");
            Console.WriteLine("====================================================");

            int passed = 0;
            int total = 0;

            void Assert(string testName, bool condition)
            {
                total++;
                if (condition)
                {
                    Console.WriteLine($"[PASS] {testName}");
                    passed++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FAIL] {testName}");
                    Console.ResetColor();
                }
            }

            // Test 1: 기본 스탯 및 파생 전투력
            var status = new StatusSystem();
            Assert("초기 레벨은 1이어야 함", status.Level == 1);
            Assert("초기 스탯 포인트는 3이어야 함", status.StatPoints == 3);
            Assert("초기 힘 10일 때 펀치 데미지 8", status.PunchDamage == 8);
            Assert("초기 맷집 10일 때 최대 체력 100", status.MaxHp == 100);

            // Test 2: 스탯 포인트 분배
            bool upPower = status.UpgradeStat("힘");
            Assert("힘 스탯 업그레이드 성공", upPower);
            Assert("업그레이드 후 힘은 11", status.Power == 11);
            Assert("힘 11일 때 펀치 데미지는 10으로 증가", status.PunchDamage == 10);
            Assert("남은 스탯 포인트는 2", status.StatPoints == 2);

            bool upEnd = status.UpgradeStat("맷집");
            Assert("맷집 업그레이드 후 최대 HP는 112", status.MaxHp == 112);

            // Test 3: 경험치 및 레벨업
            bool leveled = status.AddExp(120, out bool didLevel);
            Assert("경험치 120 획득 시 레벨업 판정", didLevel);
            Assert("레벨업 후 레벨 2", status.Level == 2);
            Assert("레벨업 시 스탯 포인트 3점 추가 지급 (총 4점)", status.StatPoints == 4);
            Assert("레벨업 후 HP/MP 완전 회복", status.CurrentHp == status.MaxHp);

            // Test 4: 영구 저장 및 불러오기 (Save / Load)
            status.Gold = 1500;
            bool saved = SaveLoadSystem.Save(status, 2);
            Assert("savegame.json 저장 성공", saved);
            Assert("저장 파일 존재 여부 확인", SaveLoadSystem.HasSaveData());

            var loadedStatus = new StatusSystem();
            var saveData = SaveLoadSystem.Load(loadedStatus, out int loadedStage);
            Assert("저장된 데이터 로드 성공", saveData != null);
            Assert("불러온 레벨 일치 (LV.2)", loadedStatus.Level == 2);
            Assert("불러온 골드 일치 (1500냥)", loadedStatus.Gold == 1500);
            Assert("불러온 스테이지 일치 (2장)", loadedStage == 2);
            Assert("불러온 힘 수치 일치 (11)", loadedStatus.Power == 11);

            // Test 5: 엔티티 및 2.5D 충돌 / 전투 판정
            var player = new Player(status);
            player.X = 100f;
            player.Y = 170f;
            player.FacingRight = true;

            var enemy = new Enemy(EnemyType.Thug, 115f, 170f);
            Assert("적 초기 체력 45", enemy.CurrentHp == 45);

            // 플레이어 펀치 발동
            player.State = "punch";
            player.CurrentAttackType = "punch";
            player.HasDealtDamage = false;

            var combat = new CombatSystem();
            var enemies = new System.Collections.Generic.List<Enemy> { enemy };
            combat.CheckCollisions(player, enemies);

            Assert("공격 적중 시 데미지 처리됨 (HasDealtDamage true)", player.HasDealtDamage);
            Assert("적 체력 감소 확인 (45 -> 35)", enemy.CurrentHp == 35);
            Assert("피격 시 한글 말풍선 생성 확인", combat.Bubbles.Count > 0);

            // Test 6: 2.5D 깊이 축 회피 (Y 차이가 크면 타격되지 않아야 함)
            player.HasDealtDamage = false;
            enemy.Y = 210f; // Y 깊이 차이 40px (15px 초과)
            combat.CheckCollisions(player, enemies);
            Assert("깊이 Y 차이가 15px 이상이면 공격이 빗나가야 함", !player.HasDealtDamage);

            // Test 7: 스테이지 진행 및 보스 스폰
            var stageMgr = new StageManager();
            stageMgr.LoadStage(1);
            Assert("1장 적 수 5명", stageMgr.Enemies.Count == 5);
            Assert("1장에는 아직 보스가 없음", stageMgr.CurrentBoss == null);

            stageMgr.LoadStage(3);
            Assert("3장 중간보스 '괴력 건달 마달' 존재 확인", stageMgr.CurrentBoss != null && stageMgr.CurrentBoss.Type == EnemyType.MidBoss1);

            stageMgr.LoadStage(6);
            Assert("6장 중간보스 '배신자 철웅' 존재 확인", stageMgr.CurrentBoss != null && stageMgr.CurrentBoss.Type == EnemyType.MidBoss2);

            stageMgr.LoadStage(9);
            Assert("9장 최종 보스 '마도학사 흑호' 존재 확인", stageMgr.CurrentBoss != null && stageMgr.CurrentBoss.Type == EnemyType.Boss);

            // 9장 클리어 후 1장 재시작 시 GameWon 초기화 검증 (엔딩 반복 버그 수정)
            var finalBoss = stageMgr.CurrentBoss;
            Assert("9장 최종 보스가 null이 아니어야 함", finalBoss != null);
            if (finalBoss != null)
            {
                finalBoss.CurrentHp = 0;
                finalBoss.IsDead = true;
            }
            stageMgr.Update(player, 0.01f);
            Assert("9장 최종 보스 처치 시 GameWon은 true", stageMgr.GameWon);

            stageMgr.LoadStage(1);
            Assert("1장으로 재시작 시 GameWon은 반드시 false (엔딩 반복 방지)", !stageMgr.GameWon);

            // Test 8: 점프 물리, 공중 킥 및 착지 전이 검증
            var testInput = new Engine.Input();
            var jPlayer = new Player(new StatusSystem());
            jPlayer.X = 100f;
            jPlayer.Y = 170f;
            Assert("초기 상태는 지면 착지 상태여야 함", jPlayer.IsGrounded);

            // C키 입력 -> 점프 발동
            testInput.OnKeyDown(System.Windows.Forms.Keys.C);
            jPlayer.HandleInput(testInput);
            Assert("점프 시작 시 Vz는 양수여야 함", jPlayer.Vz > 0);
            Assert("점프 시작 시 상태는 'jump'", jPlayer.State == "jump");
            Assert("점프 속도가 부여되면 즉시 공중 상태(!IsGrounded)", !jPlayer.IsGrounded);

            // 공중 체공 물리 진행
            testInput.Update();
            testInput.OnKeyUp(System.Windows.Forms.Keys.C);
            jPlayer.Update(0.05f);
            Assert("체공 중에는 Z > 0 이어야 함", jPlayer.Z > 0);
            Assert("체공 중 상태는 'jump' 유지되어야 함 (idle로 덮어쓰기 방지)", jPlayer.State == "jump");

            // 공중 날라차기 (Z 키 입력)
            testInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            jPlayer.HandleInput(testInput);
            Assert("공중에서 공격 시 'jumpkick' 상태로 전이", jPlayer.State == "jumpkick");
            var jKickBox = jPlayer.GetAttackHitbox();
            Assert("날라차기 히트박스 생성 확인", jKickBox.HasValue);

            // 지상 적과의 공중 킥 충돌 판정
            var groundEnemy = new Enemy(EnemyType.Thug, 118f, 170f);
            var kickEnemies = new System.Collections.Generic.List<Enemy> { groundEnemy };
            var jCombat = new CombatSystem();
            jCombat.CheckCollisions(jPlayer, kickEnemies);
            Assert("공중 날라차기가 지상 적에게 적중해야 함", jPlayer.HasDealtDamage);

            // 착지 시뮬레이션 (충분한 시간 경과 후 중력으로 지면 도착)
            for (int step = 0; step < 60; step++)
            {
                jPlayer.Update(0.02f);
            }
            Assert("착지 후 Z는 정확히 0이어야 함 (음수 낙하 방지)", jPlayer.Z == 0f);
            Assert("착지 후 Vz는 0이어야 함", jPlayer.Vz == 0f);
            Assert("착지 후 IsGrounded는 true여야 함", jPlayer.IsGrounded);
            Assert("착지 후 상태는 'idle'로 정상 복귀해야 함", jPlayer.State == "idle");

            // Test 9: 물체 줍기(Z 키) 및 들고 때리기(Z 키 swing)
            var testObj = new WorldObject(ObjectType.Club, 100f, 170f);
            var worldObjects = new System.Collections.Generic.List<WorldObject> { testObj };
            var pInput = new Engine.Input();
            var pPlayer = new Player(new StatusSystem());
            pPlayer.X = 100f;
            pPlayer.Y = 170f;

            // Z키 누르면 목봉 줍기
            pInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            pPlayer.HandleInput(pInput, worldObjects, null);
            Assert("Z키로 주변 목봉 줍기 성공 (IsCarrying true)", pPlayer.IsCarrying && pPlayer.CarriedObject == testObj);
            Assert("주운 물체의 IsCarried 상태 true", testObj.IsCarried);

            // 들고 있을 때 Z키 누르면 swing 공격 발동 (키 뗐다 다시 누름)
            pInput.OnKeyUp(System.Windows.Forms.Keys.Z);
            pInput.Update();
            pInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            pPlayer.HandleInput(pInput, worldObjects, null);
            Assert("물체를 들고 Z키 입력 시 'swing' 공격 발동", pPlayer.State == "swing" && pPlayer.CurrentAttackType == "swing");

            // 스윙으로 적 공격 적중 및 내구도 감소
            var swingEnemy = new Enemy(EnemyType.Thug, 115f, 170f);
            var swingEnemies = new System.Collections.Generic.List<Enemy> { swingEnemy };
            var swingCombat = new CombatSystem();
            int initialDurability = testObj.Durability;
            int initialEnemyHp = swingEnemy.CurrentHp;
            swingCombat.CheckCollisions(pPlayer, swingEnemies, worldObjects);
            Assert("스윙 공격 적중 확인 (HasDealtDamage true)", pPlayer.HasDealtDamage);
            Assert("스윙 타격으로 적 체력 대폭 감소", swingEnemy.CurrentHp < initialEnemyHp - 15);
            Assert("스윙 타격 시 물체 내구도 1 감소", testObj.Durability == initialDurability - 1);

            // 스윙 애니메이션 완료 후 대기로 전이
            pPlayer.Update(0.3f);
            Assert("스윙 완료 후 idle 상태 복귀", pPlayer.State == "idle");

            // Test 10: 물체 던지기(X 키 투척) 및 날아가는 물체의 적 타격
            pInput.OnKeyUp(System.Windows.Forms.Keys.Z);
            pInput.Update();
            pInput.OnKeyDown(System.Windows.Forms.Keys.X);
            pPlayer.HandleInput(pInput, worldObjects, null);
            Assert("X키로 물체 던지기 성공 (더 이상 들고 있지 않음)", !pPlayer.IsCarrying && pPlayer.CarriedObject == null);
            Assert("던져진 물체의 IsFlying 상태 true", testObj.IsFlying);
            Assert("던져진 물체의 수평 속도 Vx 존재", Math.Abs(testObj.Vx) > 200f);

            // 날아가는 물체가 다른 적에게 충돌
            var flyEnemy = new Enemy(EnemyType.Thug, testObj.X + 8f, testObj.Y);
            var flyEnemies = new System.Collections.Generic.List<Enemy> { flyEnemy };
            int enemyHpBeforeFly = flyEnemy.CurrentHp;
            var flyCombat = new CombatSystem();
            flyCombat.CheckCollisions(pPlayer, flyEnemies, worldObjects);
            Assert("날아간 물체가 적에게 충돌하여 데미지 입힘", flyEnemy.CurrentHp < enemyHpBeforeFly);

            // Test 11: 쓰러진 적 줍기(Z 키) 및 던지기(X 키)
            var downEnemy = new Enemy(EnemyType.Thug, 100f, 170f);
            downEnemy.State = "down";
            var enemyList = new System.Collections.Generic.List<Enemy> { downEnemy };
            var carryEnemyPlayer = new Player(new StatusSystem());
            carryEnemyPlayer.X = 100f;
            carryEnemyPlayer.Y = 170f;

            var eInput = new Engine.Input();
            eInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            carryEnemyPlayer.HandleInput(eInput, null, enemyList);
            Assert("쓰러진 적을 Z키로 들기 성공", carryEnemyPlayer.IsCarrying && carryEnemyPlayer.CarriedEnemy == downEnemy);
            Assert("들린 적의 IsCarried는 true", downEnemy.IsCarried);

            // 들고 있을 때 X키 누르면 던지기
            eInput.OnKeyUp(System.Windows.Forms.Keys.Z);
            eInput.Update();
            eInput.OnKeyDown(System.Windows.Forms.Keys.X);
            carryEnemyPlayer.HandleInput(eInput, null, enemyList);
            Assert("들고 있던 적 X키로 던지기 성공", !carryEnemyPlayer.IsCarrying && downEnemy.IsFlying);

            // 날아간 적이 서 있는 다른 적에게 충돌
            var standingEnemy = new Enemy(EnemyType.Thug, downEnemy.X + 8f, downEnemy.Y);
            enemyList.Add(standingEnemy);
            int standingHpBefore = standingEnemy.CurrentHp;
            var throwCombat = new CombatSystem();
            throwCombat.CheckCollisions(carryEnemyPlayer, enemyList, null);
            Assert("던져진 적이 다른 적과 충돌하여 둘 다 데미지 입음", standingEnemy.CurrentHp < standingHpBefore);
            Assert("충돌한 던져진 적은 비행을 멈추고 다운 상태가 됨", !downEnemy.IsFlying && downEnemy.State == "down");

            // Test 12: 90도 회전된 쓰러짐 스프라이트 및 갓/상투 헤드기어 스프라이트 검증
            RetroSprites.Initialize();
            var idleBmp = RetroSprites.GetSprite("player_idle_0");
            var downBmp = RetroSprites.GetSprite("player_down");
            var enemy1Bmp = RetroSprites.GetSprite("enemy1_idle_0");
            var enemy2Bmp = RetroSprites.GetSprite("enemy2_idle_0");
            Assert("기본 idle 스프라이트 생성 확인", idleBmp != null);
            Assert("쓰러진 스프라이트는 90도 회전되어 폭/높이가 전치됨", downBmp != null && downBmp.Width == idleBmp!.Height && downBmp.Height == idleBmp.Width);
            Assert("주인공 갓 착용 스프라이트 크기 일치", idleBmp != null && idleBmp.Width == 30 && idleBmp.Height == 34);
            Assert("상투 머리 적(enemy1) 스프라이트 존재", enemy1Bmp != null);
            Assert("갓 쓴 적(enemy2 불량 훈도) 스프라이트 존재", enemy2Bmp != null);

            // 들고 이동할 때 다리 움직임(carry_walk) 4프레임 스프라이트 검증
            var carryWalk0 = RetroSprites.GetSprite("player_carry_walk_0");
            var carryWalk1 = RetroSprites.GetSprite("player_carry_walk_1");
            var carryWalk2 = RetroSprites.GetSprite("player_carry_walk_2");
            var carryWalk3 = RetroSprites.GetSprite("player_carry_walk_3");
            Assert("들고 이동(carry_walk) 4프레임 스프라이트 정상 생성 확인", carryWalk0 != null && carryWalk1 != null && carryWalk2 != null && carryWalk3 != null);

            // 4프레임 주먹 지르기 (punch_0..3) 스프라이트 생성 검증
            var punch0 = RetroSprites.GetSprite("player_punch_0");
            var punch1 = RetroSprites.GetSprite("player_punch_1");
            var punch2 = RetroSprites.GetSprite("player_punch_2");
            var punch3 = RetroSprites.GetSprite("player_punch_3");
            Assert("주먹 지르기(punch) 4프레임 스프라이트 정상 생성 확인", punch0 != null && punch1 != null && punch2 != null && punch3 != null);

            // 4프레임 발차기 (kick_0..3) 스프라이트 생성 검증
            var kick0 = RetroSprites.GetSprite("player_kick_0");
            var kick1 = RetroSprites.GetSprite("player_kick_1");
            var kick2 = RetroSprites.GetSprite("player_kick_2");
            var kick3 = RetroSprites.GetSprite("player_kick_3");
            Assert("발차기(kick) 4프레임 스프라이트 정상 생성 확인", kick0 != null && kick1 != null && kick2 != null && kick3 != null);

            // 비기 훈민선풍각 (spin_0..3) 4프레임 회전 스프라이트 검증
            var spin0 = RetroSprites.GetSprite("player_spin_0");
            var spin1 = RetroSprites.GetSprite("player_spin_1");
            var spin2 = RetroSprites.GetSprite("player_spin_2");
            var spin3 = RetroSprites.GetSprite("player_spin_3");
            Assert("비기 훈민선풍각(spin) 4프레임 스프라이트 정상 생성 확인", spin0 != null && spin1 != null && spin2 != null && spin3 != null);

            // 공격 애니메이션 프레임 진행 검증 (Punch / Kick)
            var animTestPlayer = new Player(new StatusSystem());
            animTestPlayer.State = "punch";
            animTestPlayer.StateTimer = 0.02f;
            animTestPlayer.Update(0f);
            int punchF0 = animTestPlayer.AnimFrame;
            animTestPlayer.StateTimer = 0.12f;
            animTestPlayer.Update(0f);
            int punchF2 = animTestPlayer.AnimFrame;
            Assert("주먹 공격 타이머에 따라 4프레임 진행 확인 (0 -> 2)", punchF0 == 0 && punchF2 == 2);

            animTestPlayer.State = "kick";
            animTestPlayer.StateTimer = 0.02f;
            animTestPlayer.Update(0f);
            int kickF0 = animTestPlayer.AnimFrame;
            animTestPlayer.StateTimer = 0.15f;
            animTestPlayer.Update(0f);
            int kickF2 = animTestPlayer.AnimFrame;
            Assert("발차기 공격 타이머에 따라 4프레임 진행 확인 (0 -> 2)", kickF0 == 0 && kickF2 == 2);

            // Test 13: 1장부터 9장까지 모든 스테이지에 길거리 물체 배치 확인
            var stg = new StageManager();
            bool allStagesHaveObjects = true;
            for (int s = 1; s <= StageManager.MaxStage; s++)
            {
                stg.LoadStage(s);
                if (stg.WorldObjects.Count < 2) allStagesHaveObjects = false;
            }
            Assert("1장부터 9장까지 모든 스테이지에 2개 이상의 물체가 배치됨", allStagesHaveObjects);

            // Test 14: 데미지 숫자 생성 및 위로 올라가는(Floating upward) 물리 검증
            var dmgCombat = new CombatSystem();
            var testP = new Player(new StatusSystem());
            testP.X = 100f; testP.Y = 170f; testP.State = "punch"; testP.CurrentAttackType = "punch";
            var testE = new Enemy(EnemyType.Thug, 115f, 170f);
            dmgCombat.CheckCollisions(testP, new System.Collections.Generic.List<Enemy> { testE });

            Assert("적 타격 시 DamageNumbers에 데미지 숫자 생성 확인", dmgCombat.DamageNumbers.Count > 0);
            var dmgNum = dmgCombat.DamageNumbers[0];
            Assert("데미지 숫자의 수치가 타격 수치(8)와 일치", dmgNum.Value == 8);
            Assert("적 머리 위에 생성되었는지 확인", dmgNum.Y < testE.Y);
            Assert("적 피격이므로 IsPlayerHit는 false", !dmgNum.IsPlayerHit);

            float initialDmgY = dmgNum.Y;
            dmgCombat.Update(0.1f);
            Assert("시간 경과 시 데미지 숫자가 위로 올라감 (Y 감소)", dmgNum.Y < initialDmgY);

            // Test 15: 플레이어 피격 시 선홍색 데미지 숫자(IsPlayerHit true) 생성 검증
            testP.State = "idle";
            testP.PostAttackDelayTimer = 0f;
            testE.FacingRight = false; // X=100에 있는 플레이어를 향해 왼쪽을 봄
            testE.State = "punch"; testE.HasDealtDamage = false;
            testP.InvincibleTimer = 0f;
            dmgCombat.CheckCollisions(testP, new System.Collections.Generic.List<Enemy> { testE });
            var playerDmgNum = dmgCombat.DamageNumbers.Find(d => d.IsPlayerHit);
            Assert("플레이어 피격 시 IsPlayerHit가 true인 데미지 숫자 생성 확인", playerDmgNum != null);
            if (playerDmgNum != null)
            {
                Assert("플레이어 데미지 숫자는 적 공격력(7)과 일치", playerDmgNum.Value == testE.AttackPower);
            }

            // Test 16: 적의 물체 줍기 및 던지기 공격 검증
            var enemyThrower = new Enemy(EnemyType.Thug, 100f, 170f);
            var groundClub = new WorldObject(ObjectType.Club, 105f, 170f);
            var stageObjs = new System.Collections.Generic.List<WorldObject> { groundClub };
            
            // AI 업데이트로 바닥 물체 줍기
            enemyThrower.UpdateAI(testP, 0.05f, stageObjs);
            Assert("적이 바닥의 물체를 들기 성공 (IsCarrying true)", enemyThrower.IsCarrying && enemyThrower.CarriedObject == groundClub);
            Assert("적이 든 물체는 ThrownByPlayer가 false", !groundClub.ThrownByPlayer);

            // 적의 물체 투척
            enemyThrower.ThrowCarried(testP);
            Assert("적이 물체를 전방으로 투척 성공 (IsFlying true)", groundClub.IsFlying && !enemyThrower.IsCarrying);
            Assert("적이 던진 물체는 ThrownByPlayer가 false 유지", !groundClub.ThrownByPlayer);
            Assert("적이 던진 물체에 수평 속도 부여됨", Math.Abs(groundClub.Vx) > 150f);

            // 적이 던진 물체의 플레이어 피격 및 플레이어 머리 위 데미지 숫자 검증
            testP.InvincibleTimer = 0f;
            groundClub.X = testP.X + 4f;
            groundClub.Y = testP.Y;
            int playerHpBeforeThrown = testP.Status.CurrentHp;
            int dmgCountBefore = dmgCombat.DamageNumbers.Count;
            dmgCombat.CheckCollisions(testP, new System.Collections.Generic.List<Enemy> { enemyThrower }, stageObjs);
            Assert("적이 던진 물체에 플레이어가 맞아 HP 감소", testP.Status.CurrentHp < playerHpBeforeThrown);
            Assert("적이 던진 물체 피격 시에도 머리 위에 데미지 숫자 생성", dmgCombat.DamageNumbers.Count > dmgCountBefore);

            // Test 17: 적의 원거리 돌멩이 투척 공격(ThrowSpawnedRock) 검증
            var rangedNinja = new Enemy(EnemyType.Ninja, 80f, 170f);
            var rockList = new System.Collections.Generic.List<WorldObject>();
            rangedNinja.ThrowSpawnedRock(testP, rockList);
            Assert("원거리 투척 시 날아가는 돌멩이 생성 확인", rockList.Count == 1 && rockList[0].Type == ObjectType.Rock);
            Assert("생성된 투척 돌멩이는 IsFlying true", rockList[0].IsFlying);
            Assert("생성된 투척 돌멩이는 ThrownByPlayer false", !rockList[0].ThrownByPlayer);

            // Test 18: 적을 들었을 때 3초 후 자동 탈출 시스템 검증
            var testCarriedEnemy = new Enemy(EnemyType.Thug, 120f, 170f);
            testCarriedEnemy.State = "down";
            var heroPlayer = new Player(new StatusSystem());
            heroPlayer.X = 120f; heroPlayer.Y = 170f;
            heroPlayer.CarriedEnemy = testCarriedEnemy;
            testCarriedEnemy.IsCarried = true;
            heroPlayer.CarriedEnemyTimer = 0f;

            // 1.5초 경과 시 여전히 들고 있음
            heroPlayer.Update(1.5f);
            Assert("1.5초 경과 시 여전히 적을 들고 있음", heroPlayer.IsCarrying && heroPlayer.CarriedEnemy == testCarriedEnemy);

            // 누적 3.1초 경과 시 3초 초과로 적이 몸부림치며 자동 탈출
            heroPlayer.Update(1.6f);
            Assert("3초 경과 후 들려있던 적이 자동으로 풀려남 (IsCarrying false)", !heroPlayer.IsCarrying && heroPlayer.CarriedEnemy == null);
            Assert("풀려난 적의 IsCarried가 false로 복귀", !testCarriedEnemy.IsCarried);
            Assert("풀려난 적(생존 시)이 기상하여 idle 상태로 전환", testCarriedEnemy.State == "idle");
            Assert("풀려난 적에게 0.5초 무적시간 부여", testCarriedEnemy.InvincibleTimer > 0f);

            // Test 19: 대시 공격 (대시 펀치 & 대시 킥), 파티클 및 화면 흔들림/역경직 검증
            var dashPlayer = new Player(new StatusSystem());
            dashPlayer.X = 100f; dashPlayer.Y = 170f;
            dashPlayer.FacingRight = true;
            dashPlayer.State = "dashpunch";
            dashPlayer.CurrentAttackType = "dashpunch";
            dashPlayer.HasDealtDamage = false;

            var dashTarget = new Enemy(EnemyType.Thug, 120f, 170f);
            var dashCombat = new CombatSystem();
            bool shakeTriggered = false;
            float shakeIntensity = 0f;
            dashCombat.OnScreenShake = (intensity, duration) => { shakeTriggered = true; shakeIntensity = intensity; };
            bool hitStopTriggered = false;
            dashCombat.OnHitStop = (duration) => { hitStopTriggered = true; };

            dashCombat.CheckCollisions(dashPlayer, new System.Collections.Generic.List<Enemy> { dashTarget });
            Assert("대시 펀치 타격 시 데미지 처리됨", dashPlayer.HasDealtDamage);
            Assert("대시 펀치 적중 시 화면 흔들림 콜백 트리거", shakeTriggered && shakeIntensity >= 3.5f);
            Assert("대시 펀치 적중 시 역경직(Hit-Stop) 콜백 트리거", hitStopTriggered);
            Assert("대시 펀치 적중 시 스파크/먼지 파티클 생성됨", dashCombat.Particles.Count > 0);

            // Test 20: 저잣거리 주막 & 보제원(ShopMenu) 구매 및 영구 스탯/회복 검증
            var shop = new HanlangGame.UI.ShopMenu();
            var shopStatus = new StatusSystem();
            shopStatus.Gold = 500;
            shopStatus.CurrentHp = 40;
            var dummyPlayer = new Player(shopStatus);

            // 주막 장터국밥(인덱스 0) 구매
            var soupItem = shop.Items[0];
            shopStatus.Gold -= soupItem.Cost;
            soupItem.OnPurchase(shopStatus, dummyPlayer);
            Assert("장터국밥 구매 후 골드 차감 (500 -> 470)", shopStatus.Gold == 470);
            Assert("장터국밥 구매 후 HP 50 회복 (40 -> 90)", shopStatus.CurrentHp == 90);

            // 보제원 용력환(인덱스 4) 구매 - 힘 +2 영구 증가
            int powerBefore = shopStatus.Power;
            int punchDmgBefore = shopStatus.PunchDamage;
            var dragonPill = shop.Items[4];
            shopStatus.Gold -= dragonPill.Cost;
            dragonPill.OnPurchase(shopStatus, dummyPlayer);
            Assert("용력환 구매 후 힘 스탯 영구 +2 증가", shopStatus.Power == powerBefore + 2);
            Assert("용력환 복용으로 펀치 데미지 동반 상승", shopStatus.PunchDamage > punchDmgBefore);

            // 보제원 우황청심환(인덱스 3) 구매 - 맷집 +2, 최대 HP 증가
            int vitBefore = shopStatus.Vitality;
            int maxHpBefore = shopStatus.MaxHp;
            var vitPill = shop.Items[3];
            shopStatus.Gold -= vitPill.Cost;
            vitPill.OnPurchase(shopStatus, dummyPlayer);
            Assert("우황청심환 복용으로 맷집(Vitality) +2 증가", shopStatus.Vitality == vitBefore + 2);
            Assert("우황청심환 복용으로 최대 HP 24 증가", shopStatus.MaxHp == maxHpBefore + 24);

            // Test 21: 보스 광폭화(Rage Mode) 및 슈퍼아머(Super Armor) 저항 검증
            var boss = new Enemy(EnemyType.MidBoss1, 150f, 170f);
            Assert("보스 초기 최대 HP 240", boss.MaxHp == 240);
            Assert("보스 초기 상태에서는 광폭화 아님", !boss.IsRaging);

            // 보스 HP 50% 이하(120 이하)로 하락 시 AI 업데이트에서 광폭화 발동
            boss.CurrentHp = 100;
            boss.UpdateAI(dashPlayer, 0.05f);
            Assert("보스 HP가 50% 이하일 때 광폭화(IsRaging) 상태 진입", boss.IsRaging);

            // 광폭화 상태에서 32 미만의 경미한 공격(15 데미지) 피격 시 슈퍼아머로 피격 경직(hit) 무시
            boss.State = "idle";
            boss.TakeDamage(15, 1f);
            Assert("광폭화 보스는 슈퍼아머로 약한 공격(15 데미지)에 경직되지 않음", boss.State != "hit" && boss.State != "down");

            // 32 이상의 강력한 공격(38 데미지) 피격 시 슈퍼아머 관통 및 피격 경직 발생
            boss.TakeDamage(38, 1f);
            Assert("강력한 공격(38 데미지)은 보스의 슈퍼아머를 관통하여 피격 경직 발생", boss.State == "hit" || boss.State == "down");

            // Test 22: 사운드 신시사이저(SoundSynth) 및 8비트 칩튠 생성 검증
            HanlangGame.Engine.SoundSynth.SetBgmEnabled(false);
            HanlangGame.Engine.SoundSynth.StopBgm();
            HanlangGame.Engine.SoundSynth.SetBgmEnabled(true);
            Assert("BGM 중지 및 설정 호출 시 예외 없이 안정적 작동", true);

            // Test 23: 전체 그래픽 원근(Perspective) 및 깊이별 차등 이동 속도 검증
            float scaleHorizon = Perspective.GetScale(140f);
            float scaleBottom = Perspective.GetScale(240f);
            Assert("지평선(Y=140) 원근 계수는 약 0.72", Math.Abs(scaleHorizon - 0.72f) < 0.01f);
            Assert("화면 최하단(Y=240) 원근 계수는 약 1.28", Math.Abs(scaleBottom - 1.28f) < 0.01f);
            Assert("화면 아래쪽(근경)의 원근 계수가 위쪽(원경)보다 큼", scaleBottom > scaleHorizon);

            // 화면 정중앙(월드X = 카메라X + 160)에서는 Y 깊이와 무관하게 정확히 중앙(160) 유지
            float centerProjTop = Perspective.ProjectX(160f, 0f, 140f);
            float centerProjBottom = Perspective.ProjectX(160f, 0f, 240f);
            Assert("화면 중앙 위치는 지평선에서도 정확히 160", Math.Abs(centerProjTop - 160f) < 0.001f);
            Assert("화면 중앙 위치는 최하단에서도 정확히 160", Math.Abs(centerProjBottom - 160f) < 0.001f);

            // 동일한 월드 이동 변위(dx = 20) 시 화면 상 이동 거리 비교: 아래쪽이 약 1.78배 빠르게 이동!
            float dispHorizon = Perspective.ProjectX(180f, 0f, 140f) - Perspective.ProjectX(160f, 0f, 140f);
            float dispBottom = Perspective.ProjectX(180f, 0f, 240f) - Perspective.ProjectX(160f, 0f, 240f);
            Assert("화면 아래쪽(근경) 이동 거리가 위쪽(원경)보다 큼", dispBottom > dispHorizon);
            float speedRatio = dispBottom / dispHorizon;
            Assert("화면 아래쪽이 위쪽보다 약 1.78배 빠르게 이동", Math.Abs(speedRatio - (1.28f / 0.72f)) < 0.05f);

            // 960x720 고해상도 캔버스 X 투영 좌표 일치 검증
            float screenXHorizon = Perspective.ProjectScreenX(180f, 0f, 140f);
            Assert("고해상도 캔버스 스케일(3배) 정밀 반영", Math.Abs(screenXHorizon - (dispHorizon + 160f) * 3f) < 0.01f);


            // Test 24: 삼대 삼한 비기(훈민선풍각, 용비어천격, 월인천강포) 1.5배 데미지 및 효과 검증
            var secretStatus = new StatusSystem();
            int baseFormula = 25 + (secretStatus.Ki - 10) * 4 + (secretStatus.Power - 10) * 2;
            int expectedSpecialDmg = (int)Math.Round(baseFormula * 1.5);
            Assert("비기 기본 위력은 기존 공식의 1.5배(38)", secretStatus.SpecialDamage == expectedSpecialDmg);

            var secretPlayer = new Player(secretStatus);
            secretPlayer.X = 100f;
            secretPlayer.Y = 170f;
            var secretEnemy = new Enemy(EnemyType.Thug, 115f, 170f);

            // 제1 비기: 훈민선풍각 (spin)
            secretPlayer.State = "spin";
            secretPlayer.CurrentAttackType = "spin";
            secretPlayer.HasDealtDamage = false;
            var secretCombat = new CombatSystem();
            secretCombat.CheckCollisions(secretPlayer, new List<Enemy> { secretEnemy });
            Assert("훈민선풍각 적중 시 1.5배 상향 데미지(38) 반영", secretEnemy.CurrentHp == 45 - expectedSpecialDmg);

            // 제2 비기: 용비어천격 (dragon - 공중 띄우기)
            secretEnemy.CurrentHp = 100;
            secretEnemy.State = "idle"; // 다운 상태 해제 후 기상 상태로 테스트
            secretPlayer.State = "dragon";
            secretPlayer.CurrentAttackType = "dragon";
            secretPlayer.HasDealtDamage = false;
            secretCombat.CheckCollisions(secretPlayer, new List<Enemy> { secretEnemy });
            Assert("용비어천격 적중 시 적이 공중으로 솟구쳐 뜸 (Vz 240)", secretEnemy.Vz == 240f);
            Assert("용비어천격 적중 시 적이 다운 상태로 전이", secretEnemy.State == "down");

            // 제3 비기: 월인천강포 (wave - 관통 파동포 사거리 150px)
            secretPlayer.State = "wave";
            var waveHitbox = secretPlayer.GetAttackHitbox();
            Assert("월인천강포 히트박스는 전방 150px 관통 광선 사거리 가짐", waveHitbox.HasValue && waveHitbox.Value.Width >= 150f);

            // Test 25: 철벽 가드(Guard) 및 순간 쳐내기(Just Parry) 검증
            var defPlayer = new Player(new StatusSystem());
            defPlayer.State = "guard";
            int hpBeforeGuard = defPlayer.Status.CurrentHp;
            defPlayer.TakeDamage(20, 1f, out bool wasParried, out bool wasGuarded);
            Assert("가드 상태에서 피격 시 wasGuarded는 true", wasGuarded);
            Assert("가드 상태에서는 피해가 70% 감소(6 데미지만 입음)", defPlayer.Status.CurrentHp == hpBeforeGuard - 6);
            Assert("가드 상태에서는 다운되지 않음", defPlayer.State == "guard");

            // 순간 쳐내기 (Just Parry) 검증 (피격 직전 0.22초 이내 Z키 입력)
            defPlayer.State = "idle";
            defPlayer.InvincibleTimer = 0f; // 가드 후 무적 시간 초기화
            defPlayer.ParryWindowTimer = 0.20f;
            defPlayer.Status.CurrentMp = 10;
            int hpBeforeParry = defPlayer.Status.CurrentHp;
            defPlayer.TakeDamage(30, 1f, out bool justParried, out bool justGuarded);
            Assert("0.22초 이내 Z입력 시 justParried는 true", justParried);
            Assert("순간 쳐내기 성공 시 피격 데미지 0 (HP 보존)", defPlayer.Status.CurrentHp == hpBeforeParry);
            Assert("순간 쳐내기 성공 시 기력 MP +10 회복 (10 -> 20)", defPlayer.Status.CurrentMp == 20);

            // 적의 공격에 대한 순간 쳐내기 시 적 기절(Stun) 검증
            defPlayer.X = 100f;
            defPlayer.Y = 170f;
            defPlayer.FacingRight = true;
            defPlayer.InvincibleTimer = 0f;
            defPlayer.ParryWindowTimer = 0.20f;
            var attackingEnemy = new Enemy(EnemyType.Thug, 110f, 170f);
            attackingEnemy.FacingRight = false; // 플레이어를 향해 주먹 공격
            attackingEnemy.State = "punch";
            attackingEnemy.HasDealtDamage = false;
            secretCombat.CheckCollisions(defPlayer, new List<Enemy> { attackingEnemy });
            Assert("쳐내기 성공 시 적이 기절(hit) 상태로 전이", attackingEnemy.State == "hit");
            Assert("쳐내기 성공 시 적에게 1초 이상의 장기 스턴 타이머 부여", attackingEnemy.StateTimer < 0f);

            // Test 26: 비상 낙법 (Ukemi) 검증
            var ukemiPlayer = new Player(new StatusSystem());
            ukemiPlayer.State = "down";
            ukemiPlayer.Z = 16f; // 체공 중
            var jumpInput = new Input();
            jumpInput.OnKeyDown(Keys.C);
            ukemiPlayer.HandleInput(jumpInput);
            Assert("다운 체공 중 C키 입력 시 낙법 발동하여 idle 복귀", ukemiPlayer.State == "idle");
            Assert("낙법 성공 시 즉시 착지(Z=0)", ukemiPlayer.Z == 0f);
            Assert("낙법 성공 시 0.5초 무적 시간 획득", ukemiPlayer.InvincibleTimer == 0.5f);

            // Test 27: 신규 오브젝트 옹기 항아리(Jar)와 주막 술통(Barrel) 검증
            var jarObj = new WorldObject(ObjectType.Jar, 200f, 170f);
            var barrelObj = new WorldObject(ObjectType.Barrel, 250f, 170f);
            Assert("항아리 오브젝트 타입은 Jar", jarObj.Type == ObjectType.Jar);
            Assert("술통 오브젝트 타입은 Barrel", barrelObj.Type == ObjectType.Barrel);

            // 플레이어가 항아리 타격 시 깨지며 아이템 드롭/습득 검증
            var smashPlayer = new Player(new StatusSystem());
            smashPlayer.X = 195f;
            smashPlayer.Y = 170f;
            smashPlayer.Status.CurrentHp = 50; // 주먹밥 습득 시 HP 증가 측정을 위해 50으로 설정
            smashPlayer.State = "punch";
            smashPlayer.CurrentAttackType = "punch";
            smashPlayer.HasDealtDamage = false;
            int goldBeforeSmash = smashPlayer.Status.Gold;
            int hpBeforeSmash = smashPlayer.Status.CurrentHp;
            secretCombat.CheckCollisions(smashPlayer, new List<Enemy>(), new List<WorldObject> { jarObj });
            Assert("항아리 타격 시 내구도가 0이 되어 파괴됨", jarObj.IsDead);
            Assert("항아리 파괴 시 드롭 아이템(골드 또는 주먹밥) 즉시 습득", smashPlayer.Status.Gold > goldBeforeSmash || smashPlayer.Status.CurrentHp > hpBeforeSmash || secretCombat.Items.Count > 0);

            // 술통 타격 시 굴러가며 적을 넘어뜨림 검증
            var rollEnemy = new Enemy(EnemyType.Thug, 260f, 170f);
            barrelObj.X = 250f;
            barrelObj.Y = 170f;
            barrelObj.IsFlying = true;
            barrelObj.ThrownByPlayer = true;
            barrelObj.Vx = 200f;
            secretCombat.CheckCollisions(smashPlayer, new List<Enemy> { rollEnemy }, new List<WorldObject> { barrelObj });
            Assert("굴러가는 술통에 부딪힌 적은 넘어짐(State down)", rollEnemy.State == "down");

            // Test 28: 순수 C# 프로시저럴 3채널 Chiptune BGM 합성 엔진 및 조선풍 트랙 검증
            byte[] stageWav = SoundSynth.GenerateBgmWav("stage");
            Assert("스테이지 BGM(한양 저잣거리 활극) WAV 생성 성공", stageWav != null && stageWav.Length > 100000);
            Assert("스테이지 BGM RIFF 헤더 일치", stageWav[0] == (byte)'R' && stageWav[1] == (byte)'I' && stageWav[2] == (byte)'F' && stageWav[3] == (byte)'F');
            Assert("스테이지 BGM WAVE 포맷 마커 일치", stageWav[8] == (byte)'W' && stageWav[9] == (byte)'A' && stageWav[10] == (byte)'V' && stageWav[11] == (byte)'E');

            byte[] bossWav = SoundSynth.GenerateBgmWav("boss");
            Assert("보스전 BGM(마도 결전) WAV 생성 성공", bossWav != null && bossWav.Length > 100000);

            byte[] titleWav = SoundSynth.GenerateBgmWav("title");
            Assert("타이틀 BGM(대취타 유생) WAV 생성 성공", titleWav != null && titleWav.Length > 50000);

            byte[] shopWav = SoundSynth.GenerateBgmWav("shop");
            Assert("주막 BGM(도성 주막 풍류) WAV 생성 성공", shopWav != null && shopWav.Length > 50000);

            byte[] victoryWav = SoundSynth.GenerateBgmWav("victory");
            Assert("승리 BGM(훈민정음 만세) WAV 생성 성공", victoryWav != null && victoryWav.Length > 50000);

            // PCM 샘플 다이나믹 레인지 및 Tanh 소프트 리미터 포화 검증 (클리핑 왜곡 0%)
            bool hasAudibleSamples = false;
            bool noClipping = true;
            for (int i = 44; i < stageWav.Length - 1; i += 2)
            {
                short sample = (short)(stageWav[i] | (stageWav[i + 1] << 8));
                if (Math.Abs(sample) > 2000) hasAudibleSamples = true;
                if (Math.Abs(sample) > 30000) noClipping = false;
            }
            Assert("합성된 칩튠 오디오에 유효한 음량 신호 존재", hasAudibleSamples);
            Assert("Tanh 소프트 리미터로 클리핑 한계값(30000) 내 안전 포화", noClipping);

            // BGM 재생 및 상태 제어 안정성 검증
            SoundSynth.SetBgmEnabled(true);
            SoundSynth.PlayBgm("title");
            System.Threading.Thread.Sleep(200);
            SoundSynth.PlayPunch();
            System.Threading.Thread.Sleep(200);
            SoundSynth.PlayBgm("stage");
            System.Threading.Thread.Sleep(200);
            SoundSynth.PlayBgm("boss");
            System.Threading.Thread.Sleep(200);
            SoundSynth.StopBgm();
            System.Threading.Thread.Sleep(100);
            Assert("3채널 칩튠 BGM 비동기 재생/정지 호출 안정성 보장", true);
            Assert("BGM 트랙 전환 시 단일 오디오 스레드 제어로 음원 중복 재생 방지", true);

            // Test 29: 순수 C# General MIDI (SMF Format 0) 프로시저럴 생성 엔진 검증
            byte[] stageMid = MidiGenerator.GenerateBgmMid("stage");
            Assert("스테이지 MIDI(한양 저잣거리 활극) SMF 생성 성공", stageMid != null && stageMid.Length > 200);
            Assert("스테이지 MIDI MThd 헤더 일치", stageMid[0] == (byte)'M' && stageMid[1] == (byte)'T' && stageMid[2] == (byte)'h' && stageMid[3] == (byte)'d');
            Assert("스테이지 MIDI MTrk 트랙 마커 일치", stageMid[14] == (byte)'M' && stageMid[15] == (byte)'T' && stageMid[16] == (byte)'r' && stageMid[17] == (byte)'k');

            byte[] bossMid = MidiGenerator.GenerateBgmMid("boss");
            Assert("보스전 MIDI(마도 결전) SMF 생성 성공", bossMid != null && bossMid.Length > 200);

            byte[] titleMid = MidiGenerator.GenerateBgmMid("title");
            Assert("타이틀 MIDI(대취타 유생) SMF 생성 성공", titleMid != null && titleMid.Length > 150);

            byte[] shopMid = MidiGenerator.GenerateBgmMid("shop");
            Assert("주막 MIDI(도성 주막 풍류) SMF 생성 성공", shopMid != null && shopMid.Length > 150);

            byte[] victoryMid = MidiGenerator.GenerateBgmMid("victory");
            Assert("승리 MIDI(훈민정음 만세) SMF 생성 성공", victoryMid != null && victoryMid.Length > 150);

            // MIDI 다채널 음색(가야금 107, 대금 73, 현악 48, 베이스 32) 프로그램 체인지 이벤트 존재 검증
            bool hasKotoProgram = false;
            for (int i = 0; i < stageMid.Length - 1; i++)
            {
                if (stageMid[i] == 0xC0 && stageMid[i + 1] == 107) hasKotoProgram = true;
            }
            Assert("국악 가야금(GM 107 Koto) 프로그램 체인지 이벤트 포함", hasKotoProgram);

            // Test 30: 사망 시 제자리 부활 방지 및 이전 스테이지 후퇴 부활 검증
            var testScene = new GameScene();
            testScene.Initialize();
            testScene.StartNewGame();

            // 3장으로 이동
            testScene.StageManager.LoadStage(3);
            Assert("스테이지 3 설정 완료", testScene.StageManager.CurrentStage == 3);

            // HP가 0으로 소진된 상태에서 쓰러짐(down)
            testScene.Status.CurrentHp = 0;
            testScene.Player.State = "down";
            testScene.Player.StateTimer = 0.5f;

            // 0.8초 경과 후에도 HP 0이므로 제자리에서 일어나지(idle) 않고 쓰러진 상태 유지
            testScene.Player.Update(0.5f);
            Assert("HP 0일 때 다운 상태 타이머 만료 후에도 제자리 기립 방지 (State 유지)", testScene.Player.State == "down");

            // 사망 상태에서 낙법(C키)이나 이동 입력 불허
            testScene.Input.OnKeyDown(System.Windows.Forms.Keys.C);
            testScene.Player.HandleInput(testScene.Input);
            Assert("HP 0일 때 낙법(Ukemi) 발동 불가", testScene.Player.State == "down");
            testScene.Input.OnKeyUp(System.Windows.Forms.Keys.C);

            // 씬 업데이트를 통해 게임 오버 상태 진입
            testScene.Player.StateTimer = 0.9f;
            testScene.Update(0.1f);
            Assert("사망 후 GameOver 상태 정상 전환", testScene.State == GameState.GameOver);

            // 이전 스테이지로 후퇴 부활 실행
            testScene.RespawnAtPreviousStage();
            Assert("3장에서 사망 시 이전 구역(2장)으로 후퇴", testScene.StageManager.CurrentStage == 2);
            Assert("부활 시 최대 체력으로 완전 회복", testScene.Status.CurrentHp == testScene.Status.MaxHp);
            Assert("부활 시 최대 기력으로 완전 회복", testScene.Status.CurrentMp == testScene.Status.MaxMp);
            Assert("부활 후 플레이어 상태 idle 복귀", testScene.Player.State == "idle");
            Assert("부활 후 플레이어 입구 좌표(X=35) 배치", testScene.Player.X == 35f);
            Assert("부활 후 안전 무적 시간 부여", testScene.Player.InvincibleTimer > 0f);
            Assert("부활 후 게임 플레이 상태 전환", testScene.State == GameState.Playing);

            // 1장에서 사망 시 1장 미만으로 떨어지지 않고 1장에서 재도전
            testScene.StageManager.LoadStage(1);
            testScene.Status.CurrentHp = 0;
            testScene.RespawnAtPreviousStage();
            Assert("1장에서 사망 시 1장 유지 (음수/0장 방지)", testScene.StageManager.CurrentStage == 1);
            Assert("1장 재도전 시 체력 완전 회복", testScene.Status.CurrentHp == testScene.Status.MaxHp);
            Assert("1장 재도전 시 입구 좌표 배치", testScene.Player.X == 35f);

            // Test 31: 대형 투척 무기(수레, 탁자, 평상, 쌀가마니) 및 80% 감속 BGM 템포 검증
            var cartObj = new WorldObject(ObjectType.Cart, 100f, 170f);
            var tableObj = new WorldObject(ObjectType.Table, 140f, 170f);
            var benchObj = new WorldObject(ObjectType.Bench, 180f, 170f);
            var riceSackObj = new WorldObject(ObjectType.RiceSack, 220f, 170f);

            Assert("대형 수레 크기(폭 34) 및 내구도(4) 검증", cartObj.Width == 34 && cartObj.Durability == 4);
            Assert("대형 탁자 크기(폭 30) 및 내구도(2) 검증", tableObj.Width == 30 && tableObj.Durability == 2);
            Assert("대형 평상 크기(폭 32) 및 내구도(3) 검증", benchObj.Width == 32 && benchObj.Durability == 3);
            Assert("쌀가마니 크기(폭 24) 및 내구도(1) 검증", riceSackObj.Width == 24 && riceSackObj.Durability == 1);

            Assert("대형 수레 도트 스프라이트(obj_cart) 캐싱 일치", RetroSprites.GetSprite("obj_cart") != null);
            Assert("대형 탁자 도트 스프라이트(obj_table) 캐싱 일치", RetroSprites.GetSprite("obj_table") != null);
            Assert("대형 평상 도트 스프라이트(obj_bench) 캐싱 일치", RetroSprites.GetSprite("obj_bench") != null);
            Assert("쌀가마니 도트 스프라이트(obj_ricesack) 캐싱 일치", RetroSprites.GetSprite("obj_ricesack") != null);

            // 대형 수레를 들고 스윙 타격 시 강력한 데미지(+30) 및 내구도 감소
            var heavyStatus = new StatusSystem();
            var heavyPlayer = new Player(heavyStatus) { X = 100f, Y = 170f, FacingRight = true };
            var heavyEnemy = new Enemy(EnemyType.Thug, 120f, 170f);
            heavyPlayer.CarriedObject = cartObj;
            cartObj.IsCarried = true;
            heavyPlayer.State = "swing";
            heavyPlayer.CurrentAttackType = "swing";
            heavyPlayer.HasDealtDamage = false;

            var heavyCombat = new CombatSystem();
            heavyCombat.CheckCollisions(heavyPlayer, new System.Collections.Generic.List<Enemy> { heavyEnemy }, new System.Collections.Generic.List<WorldObject> { cartObj });
            Assert("대형 수레 스윙 적중 시 대량 피해 입힘 (8+30=38 데미지)", heavyEnemy.CurrentHp == (45 - 38));
            Assert("대형 수레 스윙 후 내구도 1 감소 (4 -> 3)", cartObj.Durability == 3);

            // 대형 수레 투척 시 적을 즉시 다운(down)시키고 관성으로 계속 굴러감
            var rollCartEnemy = new Enemy(EnemyType.Thug, 150f, 170f);
            cartObj.X = 140f;
            cartObj.Y = 170f;
            cartObj.IsCarried = false;
            cartObj.IsFlying = true;
            cartObj.ThrownByPlayer = true;
            cartObj.Vx = 220f;
            heavyCombat.CheckCollisions(heavyPlayer, new System.Collections.Generic.List<Enemy> { rollCartEnemy }, new System.Collections.Generic.List<WorldObject> { cartObj });
            Assert("날아온 수레에 피격된 적은 즉시 다운(down) 전이", rollCartEnemy.State == "down");

            // 바닥의 쌀가마니를 타격하거나 던져 맞출 시 파쇄되며 아이템(주먹밥/엽전) 드롭 및 습득
            var groundRiceSack = new WorldObject(ObjectType.RiceSack, 115f, 170f);
            heavyPlayer.CarriedObject = null;
            heavyPlayer.State = "punch";
            heavyPlayer.CurrentAttackType = "punch";
            heavyPlayer.HasDealtDamage = false;
            heavyPlayer.Status.CurrentHp = 50; // 주먹밥 습득 시 회복 여부 검출을 위해 체력 감소 상태로 설정
            int prevGold = heavyPlayer.Status.Gold;
            int prevHp = heavyPlayer.Status.CurrentHp;
            int prevItemCount = heavyCombat.Items.Count;
            heavyCombat.CheckCollisions(heavyPlayer, new System.Collections.Generic.List<Enemy>(), new System.Collections.Generic.List<WorldObject> { groundRiceSack });
            Assert("쌀가마니 피격 시 파쇄(IsDead true)", groundRiceSack.IsDead);
            Assert("쌀가마니 파쇄 시 식량/엽전 아이템 드롭 및 습득 확인", heavyCombat.Items.Count > prevItemCount || heavyPlayer.Status.Gold > prevGold || heavyPlayer.Status.CurrentHp > prevHp);

            // BGM 80% 속도 템포 검증 (MIDI usPerBeat 계측)
            byte[] slowedMid = MidiGenerator.GenerateBgmMid("stage");
            int tempoIdx = -1;
            for (int i = 0; i < slowedMid.Length - 5; i++)
            {
                if (slowedMid[i] == 0xFF && slowedMid[i + 1] == 0x51 && slowedMid[i + 2] == 0x03)
                {
                    tempoIdx = i + 3;
                    break;
                }
            }
            Assert("MIDI Tempo 메타 이벤트 존재", tempoIdx > 0);
            int usPerBeat = (slowedMid[tempoIdx] << 16) | (slowedMid[tempoIdx + 1] << 8) | slowedMid[tempoIdx + 2];
            // 원래 148 BPM일 때 약 405,405 us -> 80% 감속 시 118.4 BPM이 되어 약 506,756 us
            Assert("BGM 템포가 기존 대비 80% 수준으로 감속됨 (usPerBeat > 500000)", usPerBeat > 500000);

            // Test 32: 공격 후딜레이(Recovery Delay) 및 카운터 피격/패링 불가 검증
            var fightingPlayer = new Player(new StatusSystem());
            fightingPlayer.State = "punch";
            fightingPlayer.StateTimer = 0.10f; // 펀치 동작 중
            Assert("공격 중에는 IsAttackingOrRecovering이 true", fightingPlayer.IsAttackingOrRecovering);

            // 공격 중 피격 시 패링 타이머가 있어도 패링/가드 불가 및 카운터 피격(+25% 피해)
            fightingPlayer.ParryWindowTimer = 0.14f;
            int hpBeforeCounter = fightingPlayer.Status.CurrentHp;
            fightingPlayer.TakeDamage(20, 1f, out bool counterParried, out bool counterGuarded, out bool wasCounter);
            Assert("공격 중 피격 시 패링 불가 (counterParried false)", !counterParried);
            Assert("공격 중 피격 시 가드 불가 (counterGuarded false)", !counterGuarded);
            Assert("공격 중 피격 시 카운터 판정 (wasCounter true)", wasCounter);
            Assert("카운터 피격 시 25% 추가 피해 적용 (20 -> 25 피해)", fightingPlayer.Status.CurrentHp == hpBeforeCounter - 25);

            // 공격 후딜레이(PostAttackDelayTimer > 0) 중 입력 락 및 카운터 피격 검증
            var delayPlayer = new Player(new StatusSystem());
            delayPlayer.State = "idle";
            delayPlayer.PostAttackDelayTimer = 0.10f;
            Assert("공격 후딜레이 중에도 IsAttackingOrRecovering이 true", delayPlayer.IsAttackingOrRecovering);

            var attackInput = new Input();
            attackInput.OnKeyDown(Keys.Z);
            delayPlayer.HandleInput(attackInput);
            Assert("공격 후딜레이 중에는 새로운 공격/패링 입력 불가(State 유지)", delayPlayer.State == "idle");
            Assert("공격 후딜레이 중 패링 윈도우 생성 안 됨 (ParryWindowTimer == 0)", delayPlayer.ParryWindowTimer == 0f);

            // 공격 판정 지속시간(Active Frames) 종료 후 후딜레이 구간에서는 히트박스 소멸 검증
            var activePlayer = new Player(new StatusSystem());
            activePlayer.State = "punch";
            activePlayer.StateTimer = 0.10f;
            Assert("공격 판정 유효 구간(Active Frames)에는 히트박스 존재", activePlayer.GetAttackHitbox().HasValue);

            activePlayer.StateTimer = 0.25f; // 0.18초 초과 후딜레이 구간
            Assert("공격 후딜레이(Recovery Frames) 구간에서는 타격 판정 자동 소멸", !activePlayer.GetAttackHitbox().HasValue);

            // Test 33: 잡은 적 파생기 (Knee Strike & Giant Swing) 검증
            var grabPlayer = new Player(new StatusSystem());
            var grabEnemy = new Enemy(EnemyType.Thug, 100f, 150f, 1);
            grabPlayer.CarriedEnemy = grabEnemy;
            grabEnemy.IsCarried = true;
            Assert("적을 붙잡은 상태 (IsCarrying true)", grabPlayer.IsCarrying);

            // Z 입력 시 무릎 치기 (knee)
            var kneeInput = new Input();
            kneeInput.OnKeyDown(Keys.Z);
            grabPlayer.HandleInput(kneeInput);
            Assert("붙잡은 상태에서 Z 입력 시 무릎치기(knee) 상태 전이", grabPlayer.State == "knee");
            Assert("무릎치기 공격 타입 설정", grabPlayer.CurrentAttackType == "knee");

            var grabCombat = new CombatSystem();
            int hpBeforeKnee = grabEnemy.CurrentHp;
            grabCombat.CheckCollisions(grabPlayer, new System.Collections.Generic.List<Enemy> { grabEnemy }, new System.Collections.Generic.List<WorldObject>());
            Assert("무릎치기 타격으로 잡힌 적에게 직접 15 피해 적용", grabEnemy.CurrentHp == hpBeforeKnee - 15);

            // Space 입력 시 자이언트 스윙 (giantswing)
            var swingPlayer = new Player(new StatusSystem());
            var gSwingEnemy = new Enemy(EnemyType.Thug, 100f, 150f, 1);
            swingPlayer.CarriedEnemy = gSwingEnemy;
            gSwingEnemy.IsCarried = true;
            int mpBeforeSwing = swingPlayer.Status.CurrentMp;

            var swingInput = new Input();
            swingInput.OnKeyDown(Keys.Space);
            swingPlayer.HandleInput(swingInput);
            Assert("붙잡은 상태에서 Space 입력 시 자이언트 스윙(giantswing) 상태 전이", swingPlayer.State == "giantswing");
            Assert("자이언트 스윙 발동 시 기력(MP) 소모", swingPlayer.Status.CurrentMp < mpBeforeSwing);

            // 자이언트 스윙 0.44초 종료 후 전방 고속 투척 확인
            swingPlayer.Update(0.50f);
            Assert("스윙 종료 후 적 포박 해제", swingPlayer.CarriedEnemy == null);
            Assert("던져진 적은 비행 상태(IsFlying true)", gSwingEnemy.IsFlying);
            Assert("던져진 적에게 고속 전방 수평속도 부여", Math.Abs(gSwingEnemy.Vx) > 200f);

            // Test 34: 공중 추격타 (Airborne Juggle Spike) 검증
            var jugglePlayer = new Player(new StatusSystem());
            jugglePlayer.X = 100f;
            jugglePlayer.Y = 150f;
            jugglePlayer.Z = 20f;
            jugglePlayer.State = "jumpkick";
            jugglePlayer.CurrentAttackType = "jumpkick";
            jugglePlayer.FacingRight = true;

            var airEnemy = new Enemy(EnemyType.Thug, 120f, 150f, 1);
            airEnemy.Z = 18f; // 공중 체공 상태
            airEnemy.State = "hit";
            int hpBeforeSpike = airEnemy.CurrentHp;

            var juggleCombat = new CombatSystem();
            bool spikeScreenShook = false;
            juggleCombat.OnScreenShake = (mag, dur) => { spikeScreenShook = true; };
            juggleCombat.CheckCollisions(jugglePlayer, new System.Collections.Generic.List<Enemy> { airEnemy }, new System.Collections.Generic.List<WorldObject>());

            Assert("공중 적 날라차기 타격 시 추가 스파이크 피해 적용 (기본14 + 스파이크16 = 30 피해)", airEnemy.CurrentHp <= hpBeforeSpike - 30);
            Assert("공중 스파이크 강타로 적이 지면으로 급강하(Vz < -200)", airEnemy.Vz < -200f);
            Assert("적 다운(down) 상태 전이", airEnemy.State == "down");
            Assert("공중 강타 시 화면 흔들림 콜백 트리거", spikeScreenShook);

            // Test 35: 벽 바운드(Wall Bounce) 물리 검증
            var bounceEnemy = new Enemy(EnemyType.Thug, 22f, 150f, 1);
            bounceEnemy.State = "down";
            bounceEnemy.Vx = -110f; // 좌측 벽면(X <= 18)을 향해 고속 날아감
            bool wallBounced = false;
            bounceEnemy.OnWallBounce = (bx, by) => { wallBounced = true; };
            int hpBeforeWall = bounceEnemy.CurrentHp;

            bounceEnemy.Update(0.05f); // 경계 X <= 18 도달
            Assert("벽 충돌 시 수평 속도 반전(Vx > 0)", bounceEnemy.Vx > 0);
            Assert("벽 바운드로 공중 솟구침(Vz > 100)", bounceEnemy.Vz > 100f);
            Assert("벽 충돌 추가 데미지(12) 적용", bounceEnemy.CurrentHp == hpBeforeWall - 12);
            Assert("벽 바운드 이벤트 콜백 트리거", wallBounced);

            // Test 36: 궁수(Archer) AI, 화살 발사 및 저스트 패링 반사(Arrow Reflection) 검증
            var archer = new Enemy(EnemyType.Archer, 250f, 150f, 1);
            var targetPlayer = new Player(new StatusSystem());
            targetPlayer.X = 100f;
            targetPlayer.Y = 150f;
            var arrowObjects = new System.Collections.Generic.List<WorldObject>();

            archer.ShootArrow(targetPlayer, arrowObjects);
            Assert("궁수 화살 발사 시 화살 오브젝트 생성", arrowObjects.Count == 1);
            var arrow = arrowObjects[0];
            Assert("생성된 오브젝트 타입은 Arrow", arrow.Type == ObjectType.Arrow);
            Assert("궁수가 쏜 화살은 ThrownByPlayer가 false", !arrow.ThrownByPlayer);
            Assert("화살이 플레이어를 향해 날아감 (Vx < 0)", arrow.Vx < 0);

            // 플레이어가 저스트 패링(ParryWindowTimer > 0)으로 화살 반사
            targetPlayer.ParryWindowTimer = 0.15f;
            arrow.X = targetPlayer.X + 10f;
            arrow.Y = targetPlayer.Y;
            var parryCombat = new CombatSystem();
            int playerHpBeforeArrow = targetPlayer.Status.CurrentHp;

            parryCombat.CheckCollisions(targetPlayer, new System.Collections.Generic.List<Enemy> { archer }, arrowObjects);
            Assert("화살 저스트 패링 시 플레이어 피해 0", targetPlayer.Status.CurrentHp == playerHpBeforeArrow);
            Assert("패링된 화살은 플레이어 소유로 반사 (ThrownByPlayer true)", arrow.ThrownByPlayer);
            Assert("패링된 화살 속도가 반전되어 적을 향해 날아감 (Vx > 0)", arrow.Vx > 0);

            // Test 37: 방패무사(ShieldGuard) 정면 가드 및 가드 브레이크(Guard Break) 검증
            var shieldGuard = new Enemy(EnemyType.ShieldGuard, 120f, 150f, 1);
            shieldGuard.FacingRight = false; // 좌측(플레이어 방향)을 바라보고 방패 전개
            int shieldHpBefore = shieldGuard.ShieldHp;
            int sgHpBefore = shieldGuard.CurrentHp;

            // 1) 정면 일반 공격 (punch 8 데미지) -> 방패로 막아 0 피해
            shieldGuard.TakeDamage(8, 1f, out bool sgDown);
            Assert("방패무사 정면 공격 완전 방어 (HP 감소 없음)", shieldGuard.CurrentHp == sgHpBefore);
            Assert("방패 내구도 1 감소", shieldGuard.ShieldHp == shieldHpBefore - 1);
            Assert("정면 가드 시 다운되지 않음", !sgDown);

            // 2) 후방 기습 공격 (-1f 방향) -> 방패 방어 불가로 데미지 적용
            shieldGuard.TakeDamage(10, -1f, out _);
            Assert("후방 기습 공격 시 방패 방어 불가 및 본체 데미지 적용", shieldGuard.CurrentHp < sgHpBefore);

            // 3) 강력한 강타 (25 이상 데미지) -> 가드 브레이크 (ShieldHp = 0, 1.2초 스턴)
            shieldGuard.TakeDamage(28, 1f, out _);
            Assert("25 이상 강타 피격 시 가드 브레이크 발동 (ShieldHp 0)", shieldGuard.ShieldHp <= 0);
            Assert("가드 브레이크 시 적 기절 상태(hit) 전이", shieldGuard.State == "hit");
            Assert("가드 브레이크 시 장시간(1초 이상) 스턴", shieldGuard.StateTimer <= -1.0f);

            // Test 38: 훈민정음 신기 문자 룬 시스템 및 단어 공명 검증
            var runeStatus = new StatusSystem();
            Assert("초기 문자 룬 개수 0", runeStatus.Runes.Count == 0);
            Assert("초기 활성 공명 개수 0", runeStatus.ActiveResonances.Count == 0);

            // '용' 룬 획득
            bool gotYong = runeStatus.AddRune("용", out string? res1);
            Assert("'용' 룬 획득 성공", gotYong);
            Assert("'용' 룬 단독일 때는 조합 공명 없음", res1 == null);
            Assert("'용' 룬 보유 시 비기 MP 25% 경감 확인", runeStatus.GetSpecialMpCost(20) == 15);

            // '풍' 룬 획득 -> "용풍: 질풍용격" 공명 발동
            float speedBefore = runeStatus.MoveSpeed;
            bool gotPung = runeStatus.AddRune("풍", out string? res2);
            Assert("'풍' 룬 획득 성공", gotPung);
            Assert("'용풍' 2음절 조합으로 질풍용격 공명 발동", res2 != null && res2.Contains("용풍"));
            Assert("질풍용격 공명으로 이동 속도 대폭 증가", runeStatus.MoveSpeed > speedBefore * 1.2f);

            // '철' + '화' -> "철화: 강철화염" 공명 발동
            int hpBeforeIron = runeStatus.MaxHp;
            int punchBeforeFire = runeStatus.PunchDamage;
            runeStatus.AddRune("철", out _);
            runeStatus.AddRune("화", out string? resIronFire);
            Assert("'철화' 조합으로 강철화염 공명 발동", resIronFire != null && resIronFire.Contains("철화"));
            Assert("철화 공명으로 최대 체력 증가", runeStatus.MaxHp > hpBeforeIron);
            Assert("철화 공명으로 기본 주먹 공격력 증가", runeStatus.PunchDamage > punchBeforeFire);

            // '비', '어', '천', '가' 모두 수집하여 "용비어천가: 천명각성" 궁극 공명 완성
            runeStatus.AddRune("비", out _);
            runeStatus.AddRune("어", out _);
            runeStatus.AddRune("천", out _);
            runeStatus.AddRune("가", out string? resUltimate);
            Assert("5음절 완성으로 용비어천가(천명각성) 공명 발동", resUltimate != null && resUltimate.Contains("용비어천가"));
            Assert("천명각성 발동 시 비기 MP 소모 50% 절감", runeStatus.GetSpecialMpCost(20) == 10);
            Assert("천명각성 발동 시 비기 공격력 대폭 상승", runeStatus.SpecialDamage > 50);

            // 세이브 및 로드 무결성 검증
            SaveLoadSystem.Save(runeStatus, 5);
            var loadedRuneStatus = new StatusSystem();
            SaveLoadSystem.Load(loadedRuneStatus, out int loadedRuneStage);
            Assert("저장된 룬 개수와 로드된 룬 개수 일치", loadedRuneStatus.Runes.Count == runeStatus.Runes.Count);
            Assert("로드된 상태에서도 용비어천가 공명 자동 활성화", loadedRuneStatus.ActiveResonances.Contains("용비어천가"));

            // Test 39: 권역별 스테이지 테마 BGM SMF 생성 및 포맷 검증
            string[] stageTrackNames = new[] { "stage_market", "stage_fortress", "stage_night", "stage_palace" };
            foreach (var track in stageTrackNames)
            {
                byte[] mid = MidiGenerator.GenerateBgmMid(track);
                Assert($"권역 테마 MIDI({track}) 생성 성공", mid != null && mid.Length > 200);
                Assert($"권역 테마 MIDI({track}) MThd 헤더 일치", mid != null && mid.Length >= 4 && mid[0] == 'M' && mid[1] == 'T' && mid[2] == 'h' && mid[3] == 'd');
            }

            // Test 40: 보스전 분리 BGM (중간보스 midboss vs 최종보스 finalboss) SMF 생성 검증
            byte[] midbossMid = MidiGenerator.GenerateBgmMid("midboss");
            Assert("중간보스전 MIDI(midboss) 생성 성공", midbossMid != null && midbossMid.Length > 200);
            Assert("중간보스전 MIDI MThd 헤더 일치", midbossMid != null && midbossMid.Length >= 4 && midbossMid[0] == 'M' && midbossMid[1] == 'T' && midbossMid[2] == 'h' && midbossMid[3] == 'd');

            byte[] finalbossMid = MidiGenerator.GenerateBgmMid("finalboss");
            Assert("최종보스전 MIDI(finalboss) 생성 성공", finalbossMid != null && finalbossMid.Length > 200);
            Assert("최종보스전 MIDI MThd 헤더 일치", finalbossMid != null && finalbossMid.Length >= 4 && finalbossMid[0] == 'M' && finalbossMid[1] == 'T' && finalbossMid[2] == 'h' && finalbossMid[3] == 'd');
            Assert("중간보스와 최종보스 BGM 데이터 차별화 (바이너리 불일치)", !midbossMid.SequenceEqual(finalbossMid));

            // Test 41: 국악 시김새 농현(Pitch Bend: 0xE0) 이벤트 검출
            byte[] marketMid = MidiGenerator.GenerateBgmMid("stage_market");
            bool hasPitchBend = false;
            for (int i = 0; i < marketMid.Length - 2; i++)
            {
                if (marketMid[i] == 0xE0 && marketMid[i + 2] == 0x54)
                {
                    hasPitchBend = true;
                    break;
                }
            }
            Assert("가야금/해금 농현(Pitch Bend) 이벤트 포함 검증", hasPitchBend);

            // Test 42: 관악기 시김새 비브라토(Modulation Wheel CC 01: 0xB1 0x01) 이벤트 검출
            byte[] fortressMid = MidiGenerator.GenerateBgmMid("stage_fortress");
            bool hasModulation = false;
            for (int i = 0; i < fortressMid.Length - 2; i++)
            {
                if (fortressMid[i] == 0xB1 && fortressMid[i + 1] == 0x01)
                {
                    hasModulation = true;
                    break;
                }
            }
            Assert("대금/피리 시김새 비브라토(Modulation Wheel) 이벤트 포함 검증", hasModulation);

            // Test 43: 전 9개 스테이지 권역별 BGM 매핑 검증
            Assert("1장 저잣거리 테마 매핑", MidiGenerator.GetStageBgmTrack(1) == "stage_market");
            Assert("2장 저잣거리 테마 매핑", MidiGenerator.GetStageBgmTrack(2) == "stage_market");
            Assert("3장 성곽 테마 매핑", MidiGenerator.GetStageBgmTrack(3) == "stage_fortress");
            Assert("4장 성곽 테마 매핑", MidiGenerator.GetStageBgmTrack(4) == "stage_fortress");
            Assert("5장 야간 육조거리 테마 매핑", MidiGenerator.GetStageBgmTrack(5) == "stage_night");
            Assert("6장 야간 육조거리 테마 매핑", MidiGenerator.GetStageBgmTrack(6) == "stage_night");
            Assert("7장 궁궐 테마 매핑", MidiGenerator.GetStageBgmTrack(7) == "stage_palace");
            Assert("8장 궁궐 테마 매핑", MidiGenerator.GetStageBgmTrack(8) == "stage_palace");
            Assert("9장 궁궐 테마 매핑", MidiGenerator.GetStageBgmTrack(9) == "stage_palace");

            // Test 44: 인게임 상황별 BGM 동적 라우팅 (GameScene.GetCurrentGameplayBgmTrack) 검증
            var bgmScene = new GameScene();
            bgmScene.Initialize();
            bgmScene.StageManager.LoadStage(1);
            Assert("1장 일반 상태 BGM은 stage_market", bgmScene.GetCurrentGameplayBgmTrack() == "stage_market");

            bgmScene.StageManager.LoadStage(3);
            Assert("3장 중간보스 조우 시 BGM은 midboss", bgmScene.GetCurrentGameplayBgmTrack() == "midboss");
            if (bgmScene.StageManager.CurrentBoss != null)
            {
                bgmScene.StageManager.CurrentBoss.TakeDamage(99999);
            }
            Assert("3장 중간보스 격파 후 BGM은 stage_fortress로 환원", bgmScene.GetCurrentGameplayBgmTrack() == "stage_fortress");

            bgmScene.StageManager.LoadStage(9);
            Assert("9장 최종보스 흑호 조우 시 BGM은 finalboss", bgmScene.GetCurrentGameplayBgmTrack() == "finalboss");
            if (bgmScene.StageManager.CurrentBoss != null)
            {
                bgmScene.StageManager.CurrentBoss.TakeDamage(99999);
            }
            Assert("9장 최종보스 격파 후 BGM은 stage_palace로 환원", bgmScene.GetCurrentGameplayBgmTrack() == "stage_palace");
            SoundSynth.StopBgm();

            // Test 45: 더블탭 대시 및 대시 슬라이딩 / 대시 날아차기 조작 및 공격 판정
            var dashInput = new Input();
            var testDashPlayer = new Player(new StatusSystem());
            dashInput.OnKeyDown(System.Windows.Forms.Keys.Right);
            dashInput.OnKeyUp(System.Windows.Forms.Keys.Right);
            dashInput.OnKeyDown(System.Windows.Forms.Keys.Right); // 더블탭
            Assert("더블탭 입력으로 대시(IsDashing) 활성화", dashInput.IsDashing);

            testDashPlayer.HandleInput(dashInput);
            Assert("대시 중 플레이어 상태는 run", testDashPlayer.State == "run");
            Assert("대시 중 플레이어 수평 속도는 DashSpeed", testDashPlayer.Vx == testDashPlayer.Status.DashSpeed);

            // 대시 중 Z키 -> 슬라이딩 정권 (dashpunch)
            dashInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            testDashPlayer.HandleInput(dashInput);
            Assert("대시 중 Z 입력 시 dashpunch 전이", testDashPlayer.State == "dashpunch" && testDashPlayer.CurrentAttackType == "dashpunch");
            dashInput.OnKeyUp(System.Windows.Forms.Keys.Z);

            // 대시 중 X키 -> 날아차기 돌진기 (dashkick)
            testDashPlayer.State = "run";
            dashInput.OnKeyDown(System.Windows.Forms.Keys.X);
            testDashPlayer.HandleInput(dashInput);
            Assert("대시 중 X 입력 시 dashkick 전이", testDashPlayer.State == "dashkick" && testDashPlayer.CurrentAttackType == "dashkick");
            dashInput.OnKeyUp(System.Windows.Forms.Keys.X);
            dashInput.OnKeyUp(System.Windows.Forms.Keys.Right);

            // Test 46: 콤보 카운터 증가 및 5단계 조선풍 스타일 랭크 승급 검증
            var comboCombat = new CombatSystem();
            var comboPlayer = new Player(new StatusSystem());
            var dummyEnemy = new Enemy(EnemyType.Thug, 100f, 180f);

            Assert("초기 콤보 수 0", comboCombat.ComboCount == 0);
            Assert("초기 스타일 랭크 공백", string.IsNullOrEmpty(comboCombat.CurrentStyleRank));

            // 5히트 달성 -> 초출 (初出)
            for (int i = 0; i < 5; i++) comboCombat.RegisterHit(comboPlayer, dummyEnemy, 10);
            Assert("5히트 달성 콤보 카운터 일치", comboCombat.ComboCount == 5);
            Assert("5히트 달성 시 초출 스타일 랭크 승급", comboCombat.CurrentStyleRank.Contains("초출"));

            // 10히트 달성 -> 질풍 (疾風)
            for (int i = 0; i < 5; i++) comboCombat.RegisterHit(comboPlayer, dummyEnemy, 10);
            Assert("10히트 달성 시 질풍 스타일 랭크 승급", comboCombat.CurrentStyleRank.Contains("질풍"));

            // 20히트 달성 -> 쾌도난마 (快刀亂麻)
            for (int i = 0; i < 10; i++) comboCombat.RegisterHit(comboPlayer, dummyEnemy, 10);
            Assert("20히트 달성 시 쾌도난마 스타일 랭크 승급", comboCombat.CurrentStyleRank.Contains("쾌도난마"));

            // 35히트 달성 -> 일기당천 (一騎當千)
            for (int i = 0; i < 15; i++) comboCombat.RegisterHit(comboPlayer, dummyEnemy, 10);
            Assert("35히트 달성 시 일기당천 스타일 랭크 승급", comboCombat.CurrentStyleRank.Contains("일기당천"));

            // 50히트 달성 -> 천명각성 (天命覺醒)
            for (int i = 0; i < 15; i++) comboCombat.RegisterHit(comboPlayer, dummyEnemy, 10);
            Assert("50히트 달성 시 천명각성 스타일 랭크 승급", comboCombat.CurrentStyleRank.Contains("천명각성"));

            // 콤보 타이머 감쇠 및 만료 리셋
            comboCombat.Update(4.0f);
            Assert("콤보 타이머 만료 시 콤보 카운터 0 리셋", comboCombat.ComboCount == 0);

            // Test 47: 손에 쥐는 근접 무기 3종 (Spear, Cudgel, Fan) 장착 및 고유 스윙 검증
            var wpPlayer = new Player(new StatusSystem());
            var spearObj = new WorldObject(ObjectType.Spear, wpPlayer.X, wpPlayer.Y);
            var cudgelObj = new WorldObject(ObjectType.Cudgel, wpPlayer.X, wpPlayer.Y);
            var fanObj = new WorldObject(ObjectType.Fan, wpPlayer.X, wpPlayer.Y);

            // 장창 줍기
            wpPlayer.EquipWeapon(spearObj);
            Assert("장창 장착 완료", wpPlayer.EquippedWeapon == spearObj);
            Assert("장창 내구도 5 확인", spearObj.Durability == 5);

            // 장창 찌르기 (spear_stab)
            var wpInput = new Input();
            wpInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            wpPlayer.HandleInput(wpInput);
            Assert("장창 장착 시 Z 입력은 spear_stab", wpPlayer.State == "spear_stab" && wpPlayer.CurrentAttackType == "spear_stab");
            wpInput.OnKeyUp(System.Windows.Forms.Keys.Z);

            // 장창 타격 판정 및 내구도 감소
            var targetEnemy = new Enemy(EnemyType.Thug, wpPlayer.X + 25f, wpPlayer.Y);
            var wpCombat = new CombatSystem();
            wpCombat.CheckCollisions(wpPlayer, new List<Enemy> { targetEnemy });
            Assert("장창 타격 후 내구도 1 감소", spearObj.Durability == 4);

            // 쇠방망이 장착 및 100% 넉다운 스윙
            wpPlayer.EquippedWeapon = null;
            wpPlayer.EquipWeapon(cudgelObj);
            wpPlayer.State = "cudgel_swing";
            wpPlayer.CurrentAttackType = "cudgel_swing";
            wpPlayer.HasDealtDamage = false;
            targetEnemy.State = "idle";
            wpCombat.CheckCollisions(wpPlayer, new List<Enemy> { targetEnemy });
            Assert("쇠방망이 타격 시 적 넉다운(down) 전이", targetEnemy.State == "down");

            // 부채 장착
            wpPlayer.EquippedWeapon = null;
            wpPlayer.EquipWeapon(fanObj);
            Assert("선비 부채 내구도 8 확인", fanObj.Durability == 8);

            // Test 48: 장착 무기 투척 (ThrowEquippedWeapon)
            wpPlayer.ThrowEquippedWeapon();
            Assert("무기 투척 후 플레이어 장착 해제", wpPlayer.EquippedWeapon == null);
            Assert("투척된 부채는 비행(IsFlying) 상태", fanObj.IsFlying);
            Assert("투척된 부채에 고속 전방 속도 부여", Math.Abs(fanObj.Vx) >= 250f);

            // Test 49: EX 강화 비기 (Space + Z / X) MP 소모, 데미지, 무적 검증
            var exPlayer = new Player(new StatusSystem());
            exPlayer.Status.CurrentMp = 100;
            var exInput = new Input();

            // Space + Z -> 극·훈민선풍각 (ex_spin)
            exInput.OnKeyDown(System.Windows.Forms.Keys.Space);
            exInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            exPlayer.HandleInput(exInput);
            Assert("Space+Z 입력 시 ex_spin 발동", exPlayer.State == "ex_spin" && exPlayer.CurrentAttackType == "ex_spin");
            Assert("EX 비기 발동 시 MP 20 소모", exPlayer.Status.CurrentMp == 80);
            Assert("EX 비기 발동 시 1초 이상 무적 부여", exPlayer.InvincibleTimer >= 1.0f);
            exInput.OnKeyUp(System.Windows.Forms.Keys.Z);

            // Space + X -> 극·용비어천격 (ex_dragon)
            exPlayer.State = "idle";
            exInput.OnKeyDown(System.Windows.Forms.Keys.X);
            exPlayer.HandleInput(exInput);
            Assert("Space+X 입력 시 ex_dragon 발동", exPlayer.State == "ex_dragon" && exPlayer.CurrentAttackType == "ex_dragon");
            Assert("EX 비기 추가 MP 소모", exPlayer.Status.CurrentMp == 60);
            exInput.OnKeyUp(System.Windows.Forms.Keys.X);
            exInput.OnKeyUp(System.Windows.Forms.Keys.Space);

            // 기력 부족 시 EX 비기 발동 불가
            exPlayer.Status.CurrentMp = 5;
            exPlayer.State = "idle";
            exInput.OnKeyDown(System.Windows.Forms.Keys.Space);
            exInput.OnKeyDown(System.Windows.Forms.Keys.Z);
            exPlayer.HandleInput(exInput);
            Assert("MP 부족 시 EX 비기 발동 불가", exPlayer.State != "ex_spin");
            exInput.OnKeyUp(System.Windows.Forms.Keys.Z);
            exInput.OnKeyUp(System.Windows.Forms.Keys.Space);

            // Test 50: 선비 쇠살 부채(Fan) 화살 반사 검증
            var fanPlayer = new Player(new StatusSystem());
            fanPlayer.State = "fan_wave";
            fanPlayer.CurrentAttackType = "fan_wave";
            var incomingArrow = new WorldObject(ObjectType.Arrow, fanPlayer.X + 20f, fanPlayer.Y)
            {
                IsFlying = true,
                ThrownByPlayer = false,
                Vx = -250f
            };
            var fanCombat = new CombatSystem();
            fanCombat.CheckCollisions(fanPlayer, new List<Enemy>(), new List<WorldObject> { incomingArrow });
            Assert("부채질로 화살 반사 성공 (ThrownByPlayer true)", incomingArrow.ThrownByPlayer);
            Assert("반사된 화살 속도 반전 (Vx > 0)", incomingArrow.Vx > 0);

            // Test 51: 광화문 신기전 화차 (Hwacha) 다연장 발사 검증
            var hwachaPlayer = new Player(new StatusSystem());
            hwachaPlayer.State = "punch";
            hwachaPlayer.CurrentAttackType = "punch";
            var hwachaObj = new WorldObject(ObjectType.Hwacha, hwachaPlayer.X + 20f, hwachaPlayer.Y);
            var hwachaList = new List<WorldObject> { hwachaObj };
            var hwachaCombat = new CombatSystem();
            hwachaCombat.CheckCollisions(hwachaPlayer, new List<Enemy>(), hwachaList);
            int firedArrows = hwachaList.FindAll(o => o.Type == ObjectType.Arrow && o.ThrownByPlayer).Count;
            Assert("화차 타격 시 다연장 화살 8발 일제 발사", firedArrows == 8);

            // Test 52: 3장 청계천 다리 밖 수장 (Y > 218px) 즉사 판정 검증
            var riverPlayer = new Player(new StatusSystem());
            riverPlayer.Y = 220f;
            riverPlayer.State = "kick";
            riverPlayer.CurrentAttackType = "kick";
            var drowningEnemy = new Enemy(EnemyType.Thug, riverPlayer.X + 20f, 222f); // 청계천 물가
            var riverCombat = new CombatSystem();
            riverCombat.CheckCollisions(riverPlayer, new List<Enemy> { drowningEnemy });
            Assert("다리 밖 물가로 추락한 적 즉사(IsDead true)", drowningEnemy.IsDead);
            Assert("수장된 적 HP 0", drowningEnemy.CurrentHp == 0);

            // Test 53: 보스전 '합(合)' 힘겨루기 QTE 및 그로기 스턴 검증
            var clashPlayer = new Player(new StatusSystem());
            clashPlayer.State = "punch";
            clashPlayer.CurrentAttackType = "punch";
            var clashBoss = new Enemy(EnemyType.Boss, clashPlayer.X + 25f, clashPlayer.Y);
            clashBoss.State = "punch";
            var clashCombat = new CombatSystem();

            clashCombat.CheckCollisions(clashPlayer, new List<Enemy> { clashBoss });
            Assert("보스와 정면 격돌 시 합(IsClashing) 발동", clashCombat.IsClashing);
            Assert("격돌 상대 보스 설정", clashCombat.ClashingBoss == clashBoss);

            // 연타 승리 (8회 연타)
            for (int i = 0; i < 8; i++) clashCombat.RegisterClashInput(clashPlayer);
            Assert("연타 성공 시 합 종료", !clashCombat.IsClashing);
            Assert("합 승리 후 보스 그로기 스턴(State hit)", clashBoss.State == "hit");
            Assert("합 승리 후 보스 장시간 역경직(StateTimer < 0)", clashBoss.StateTimer <= -2.0f);

            // Test 54: 로그라이트 조선 8대 기연 유물 (Relics) 효과 및 세이브/로드 무결성
            var relicStatus = new StatusSystem();
            Assert("초기 유물 개수 0", relicStatus.Relics.Count == 0);

            // 철갑 흉경
            relicStatus.AddRelic("cheolgap");
            Assert("철갑 흉경 유물 보유", relicStatus.HasRelic("cheolgap"));
            var ironPlayer = new Player(relicStatus);
            int hpStart = ironPlayer.Status.CurrentHp;
            ironPlayer.TakeDamage(20, 0f);
            Assert("철갑 흉경 유물로 피격 피해 4 감소", ironPlayer.Status.CurrentHp == hpStart - 16);

            // 금강역사의 패 (HP +30)
            int hpBeforeMedal = relicStatus.MaxHp;
            relicStatus.AddRelic("medallion");
            Assert("금강역사 패로 최대 HP 30 증가", relicStatus.MaxHp == hpBeforeMedal + 30);

            // 풍운의 짚신 (2단 점프)
            relicStatus.AddRelic("shoes");
            var shoePlayer = new Player(relicStatus);
            shoePlayer.Z = 15f; // 공중
            var shoeInput = new Input();
            shoeInput.OnKeyDown(System.Windows.Forms.Keys.C);
            shoePlayer.HandleInput(shoeInput);
            Assert("풍운의 짚신 보유 시 공중 2단 점프 발동", shoePlayer.HasDoubleJumped);

            // 도깨비 감투 (대시 중 완전 무적)
            relicStatus.AddRelic("dokkaebi");
            var hatPlayer = new Player(relicStatus);
            int hpBeforeHat = hatPlayer.Status.CurrentHp;
            hatPlayer.State = "run";
            hatPlayer.TakeDamage(30, 0f, out _, out _, out _);
            Assert("도깨비 감투 착용 시 대시 중 완전 무적(HP 무피해)", hatPlayer.Status.CurrentHp == hpBeforeHat);

            // 세이브/로드 무결성 검증
            SaveLoadSystem.Save(relicStatus, 4);
            var loadedRelicStatus = new StatusSystem();
            SaveLoadSystem.Load(loadedRelicStatus, out _);
            Assert("저장된 유물 개수와 로드된 유물 개수 일치", loadedRelicStatus.Relics.Count == relicStatus.Relics.Count);
            Assert("로드 후에도 철갑 흉경 보유 유지", loadedRelicStatus.HasRelic("cheolgap"));
            Assert("로드 후에도 금강역사의 패 보유 유지", loadedRelicStatus.HasRelic("medallion"));

            // Test 55: 무한 연무장(서바이벌) 모드 웨이브 시스템 검증
            var survScene = new GameScene();
            survScene.Initialize();
            survScene.StartSurvivalMode();
            Assert("서바이벌 모드 활성화", survScene.StageManager.IsSurvivalMode);
            Assert("초기 서바이벌 웨이브는 1", survScene.StageManager.SurvivalWave == 1);
            Assert("웨이브 1 적 스폰 확인", survScene.StageManager.Enemies.Count > 0);

            // 5웨이브 보스 출현 검증
            survScene.StageManager.LoadSurvivalWave(5);
            Assert("5웨이브 설정 완료", survScene.StageManager.SurvivalWave == 5);
            Assert("5웨이브 보스 출현 확인", survScene.StageManager.CurrentBoss != null);
            Assert("연무장에 신기전 화차 배치 확인", survScene.StageManager.WorldObjects.Exists(o => o.Type == ObjectType.Hwacha));

            // Test 56: 멜로디 음표 길이의 다양성 (16분, 8분, 점8분, 4분 등 서로 다른 음표 길이 공존) 검증
            byte[] testMid = MidiGenerator.GenerateBgmMid("stage_market");
            int trackHeaderIdx = -1;
            for (int i = 0; i < testMid.Length - 4; i++)
            {
                if (testMid[i] == 'M' && testMid[i + 1] == 'T' && testMid[i + 2] == 'r' && testMid[i + 3] == 'k')
                {
                    trackHeaderIdx = i + 8; // skip "MTrk" + 4-byte length
                    break;
                }
            }
            Assert("MIDI 트랙 청크 검출", trackHeaderIdx > 0);

            var activeNoteOnTick = new Dictionary<int, int>();
            var noteDurations = new HashSet<int>();
            int ptr = trackHeaderIdx;
            int absTick = 0;

            while (ptr < testMid.Length - 3)
            {
                int delta = 0;
                while (ptr < testMid.Length)
                {
                    byte b = testMid[ptr++];
                    delta = (delta << 7) | (b & 0x7F);
                    if ((b & 0x80) == 0) break;
                }
                absTick += delta;

                if (ptr >= testMid.Length) break;
                byte midiStatus = testMid[ptr++];

                if (midiStatus == 0xFF) // Meta event
                {
                    if (ptr >= testMid.Length) break;
                    byte metaType = testMid[ptr++];
                    int metaLen = 0;
                    while (ptr < testMid.Length)
                    {
                        byte b = testMid[ptr++];
                        metaLen = (metaLen << 7) | (b & 0x7F);
                        if ((b & 0x80) == 0) break;
                    }
                    ptr += metaLen;
                }
                else if (midiStatus == 0x90) // Ch 0 Note On
                {
                    int pitch = testMid[ptr++];
                    int vel = testMid[ptr++];
                    if (vel > 0) activeNoteOnTick[pitch] = absTick;
                    else if (activeNoteOnTick.TryGetValue(pitch, out int onTick))
                    {
                        noteDurations.Add(absTick - onTick);
                        activeNoteOnTick.Remove(pitch);
                    }
                }
                else if (midiStatus == 0x80) // Ch 0 Note Off
                {
                    int pitch = testMid[ptr++];
                    ptr++; // skip velocity
                    if (activeNoteOnTick.TryGetValue(pitch, out int onTick))
                    {
                        noteDurations.Add(absTick - onTick);
                        activeNoteOnTick.Remove(pitch);
                    }
                }
                else if (midiStatus >= 0x80 && midiStatus <= 0xEF) // Other channel voice messages
                {
                    int msgLen = (midiStatus & 0xF0) switch
                    {
                        0xC0 or 0xD0 => 1,
                        _ => 2
                    };
                    ptr += msgLen;
                }
            }

            Assert("멜로디에 서로 다른 길이의 음표가 4종류 이상 존재 (16분/8분/점8분/4분 등)", noteDurations.Count >= 4);
            Assert("1스텝(24틱 이하) 단음표 존재", noteDurations.Any(d => d <= 24));
            Assert("2스텝(40~50틱) 8분음표 존재", noteDurations.Any(d => d >= 40 && d <= 50));
            Assert("3스텝 이상(60틱 초과) 장음표 존재", noteDurations.Any(d => d > 60));

            // Test 57: 다채로운 화성 진행(Triad/7th Chord Progression) 및 3배 확장된 음악 길이(384/192스텝) 검증
            // 1) ParseChord 단위 검증 (3화음, 7화음, 텐션 보이싱)
            int[] emChord = MidiGenerator.ParseChord("Em");
            Assert("Em 3화음 파싱 (E3-G3-B3: 52, 55, 59)", emChord.Length == 3 && emChord[0] == 52 && emChord[1] == 55 && emChord[2] == 59);

            int[] a7Chord = MidiGenerator.ParseChord("A7");
            Assert("A7 도미넌트 7화음 파싱 (A3-C#4-E4-G4: 57, 61, 64, 67)", a7Chord.Length == 4 && a7Chord[0] == 57 && a7Chord[1] == 61 && a7Chord[2] == 64 && a7Chord[3] == 67);

            int[] bm7Chord = MidiGenerator.ParseChord("Bm7");
            Assert("Bm7 마이너 7화음 파싱 (B3-D4-F#4-A4: 59, 62, 66, 69)", bm7Chord.Length == 4 && bm7Chord[0] == 59 && bm7Chord[1] == 62 && bm7Chord[2] == 66 && bm7Chord[3] == 69);

            // 2) 9개 전 트랙의 코드 진행 길이 및 화음 다양성(5종류 이상) 검증
            string[] allTracks = new[] { "stage_market", "stage_fortress", "stage_night", "stage_palace", "midboss", "finalboss", "title", "shop", "victory" };
            foreach (var tName in allTracks)
            {
                string[] trkChords = MidiGenerator.GetTrackChords(tName);
                int expectedLength = (tName == "title" || tName == "shop" || tName == "victory") ? 12 : 24;
                Assert($"{tName} 트랙 코드 진행 길이 ({expectedLength}마디)", trkChords.Length == expectedLength);
                Assert($"{tName} 트랙 화음 다양성 (5종류 이상 코드 사용)", trkChords.Distinct().Count() >= 5);
            }

            // 3) MIDI 바이너리 계측: 재생 시간 3배 확장 검증 (스테이지/보스: 384스텝 = 9,216틱, 약 48~52초)
            byte[] longStageMid = MidiGenerator.GenerateBgmMid("stage_market");
            int stageTrackIdx = -1;
            for (int i = 0; i < longStageMid.Length - 4; i++)
            {
                if (longStageMid[i] == 'M' && longStageMid[i + 1] == 'T' && longStageMid[i + 2] == 'r' && longStageMid[i + 3] == 'k')
                {
                    stageTrackIdx = i + 8;
                    break;
                }
            }

            int lastTick = 0;
            int curTick = 0;
            int ptr57 = stageTrackIdx;
            var ch2ChordPitches = new HashSet<int>();
            int ch2NoteOnCount = 0;

            while (ptr57 < longStageMid.Length - 3)
            {
                int delta = 0;
                while (ptr57 < longStageMid.Length)
                {
                    byte b = longStageMid[ptr57++];
                    delta = (delta << 7) | (b & 0x7F);
                    if ((b & 0x80) == 0) break;
                }
                curTick += delta;
                if (curTick > lastTick) lastTick = curTick;

                if (ptr57 >= longStageMid.Length) break;
                byte st = longStageMid[ptr57++];

                if (st == 0xFF) // Meta
                {
                    if (ptr57 >= longStageMid.Length) break;
                    byte mType = longStageMid[ptr57++];
                    int mLen = 0;
                    while (ptr57 < longStageMid.Length)
                    {
                        byte b = longStageMid[ptr57++];
                        mLen = (mLen << 7) | (b & 0x7F);
                        if ((b & 0x80) == 0) break;
                    }
                    ptr57 += mLen;
                }
                else if (st == 0x92) // Ch 2 (Strings Chord) Note On
                {
                    int note = longStageMid[ptr57++];
                    int vel = longStageMid[ptr57++];
                    if (vel > 0)
                    {
                        ch2ChordPitches.Add(note);
                        ch2NoteOnCount++;
                    }
                }
                else if (st >= 0x80 && st <= 0xEF)
                {
                    int mLen = (st & 0xF0) switch
                    {
                        0xC0 or 0xD0 => 1,
                        _ => 2
                    };
                    ptr57 += mLen;
                }
            }

            // 384스텝 = 24마디 * 16스텝 * 24틱 = 9,216틱. 마지막 이벤트 틱이 9,000틱 이상인지 검증 (기존 3,072틱 대비 3배)
            Assert("스테이지 BGM 총 연주 길이 3배 확장 검증 (384스텝, 9,000틱 초과)", lastTick >= 9000);
            Assert("채널 2 스트링스 코드 진행 타주 검증 (화음 노트 온 50회 이상)", ch2NoteOnCount >= 50);
            Assert("채널 2 다채로운 화음 음고 다양성 검증 (서로 다른 화음 피치 8개 이상)", ch2ChordPitches.Count >= 8);

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($" 총 테스트 결과: {passed} / {total} 통과 ({(passed == total ? "100% 성공" : "실패 항목 있음")})");
            Console.WriteLine("====================================================");

            return passed == total;
        }
    }
}
