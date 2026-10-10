using System;

namespace HanlangGame.Systems
{
    public class StatusSystem
    {
        public int Level { get; set; } = 1;
        public int CurrentExp { get; set; } = 0;
        public int MaxExp => Level * 100;
        public int Gold { get; set; } = 100; // 100냥 시작
        public int StatPoints { get; set; } = 3; // 시작 시 보너스 3포인트

        // 4대 스테이터스
        public int Power { get; set; } = 10;      // 힘 (공격력)
        public int Endurance { get; set; } = 10;  // 맷집 (최대HP, 방어력)
        public int Agility { get; set; } = 10;    // 날쌤 (이동속도, 점프력)
        public int Ki { get; set; } = 10;         // 기력 (최대MP, 비기 공격력)

        // RPG 별칭 프로퍼티
        public int Strength { get => Power; set => Power = value; }
        public int Vitality { get => Endurance; set => Endurance = value; }
        public int Speed { get => Agility; set => Agility = value; }
        public int Qi { get => Ki; set => Ki = value; }

        // 파생 능력치
        public int MaxHp => 100 + (Endurance - 10) * 12;
        public int MaxMp => 50 + (Ki - 10) * 8;
        public float MoveSpeed => 80f + (Agility - 10) * 4f;
        public float DashSpeed => 135f + (Agility - 10) * 6f;
        public float JumpPower => 235f + (Agility - 10) * 6f;

        public int PunchDamage => 8 + (Power - 10) * 2;
        public int KickDamage => 14 + (Power - 10) * 3;
        public int SpecialDamage => (int)Math.Round((25 + (Ki - 10) * 4 + (Power - 10) * 2) * 1.5);

        public int CurrentHp { get; set; } = 100;
        public int CurrentMp { get; set; } = 50;

        public bool AddExp(int exp, out bool leveledUp)
        {
            leveledUp = false;
            CurrentExp += exp;
            while (CurrentExp >= MaxExp)
            {
                CurrentExp -= MaxExp;
                Level++;
                StatPoints += 3; // 레벨당 3포인트
                CurrentHp = MaxHp; // 회복
                CurrentMp = MaxMp;
                leveledUp = true;
            }
            return leveledUp;
        }

        public bool UpgradeStat(string statName)
        {
            if (StatPoints <= 0) return false;

            switch (statName)
            {
                case "힘":
                    Power++;
                    StatPoints--;
                    return true;
                case "맷집":
                    Endurance++;
                    StatPoints--;
                    CurrentHp = Math.Min(CurrentHp + 12, MaxHp);
                    return true;
                case "날쌤":
                    Agility++;
                    StatPoints--;
                    return true;
                case "기력":
                    Ki++;
                    StatPoints--;
                    CurrentMp = Math.Min(CurrentMp + 8, MaxMp);
                    return true;
            }
            return false;
        }
    }
}
