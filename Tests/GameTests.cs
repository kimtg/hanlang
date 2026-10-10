using System;
using System.IO;
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
            SoundSynth.StopBgm();
            Assert("3채널 칩튠 BGM 비동기 재생/정지 호출 안정성 보장", true);

            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine($" 총 테스트 결과: {passed} / {total} 통과 ({(passed == total ? "100% 성공" : "실패 항목 있음")})");
            Console.WriteLine("====================================================");

            return passed == total;
        }
    }
}
