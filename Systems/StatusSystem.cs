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

        // 훈민정음 신기 문자 룬 (Hangul Syllable Runes)
        public System.Collections.Generic.HashSet<string> Runes { get; } = new();
        public System.Collections.Generic.List<string> ActiveResonances { get; } = new();

        public bool HasRune(string rune) => Runes.Contains(rune);

        public bool AddRune(string rune, out string? unlockedResonance)
        {
            unlockedResonance = null;
            if (!Runes.Add(rune)) return false;

            CheckResonances(out unlockedResonance);
            return true;
        }

        private void CheckResonances(out string? newResonance)
        {
            newResonance = null;
            // 1. 용풍(龍風): 질풍용격 (용 + 풍) -> 이동/대시 속도 15% 증가, 비기 MP 소모 25% 경감
            if (Runes.Contains("용") && Runes.Contains("풍") && !ActiveResonances.Contains("용풍"))
            {
                ActiveResonances.Add("용풍");
                newResonance = "용풍(龍風) - 질풍용격";
            }
            // 2. 철화(鐵火): 강철화염 (철 + 화) -> 최대HP +30, 기본 주먹/발차기 위력 +4
            if (Runes.Contains("철") && Runes.Contains("화") && !ActiveResonances.Contains("철화"))
            {
                ActiveResonances.Add("철화");
                newResonance = "철화(鐵火) - 강철화염";
            }
            // 3. 용비어천가(龍飛御天歌): 천명각성 (용 + 비 + 어 + 천 + 가) -> 최대MP +50, 비기 MP 소모 50% 절감, 비기 공격력 40% 대폭 증폭!
            if (Runes.Contains("용") && Runes.Contains("비") && Runes.Contains("어") && Runes.Contains("천") && Runes.Contains("가") && !ActiveResonances.Contains("용비어천가"))
            {
                ActiveResonances.Add("용비어천가");
                newResonance = "용비어천가(龍飛御天歌) - 천명각성";
            }
        }

        public int GetSpecialMpCost(int baseCost)
        {
            if (ActiveResonances.Contains("용비어천가")) return Math.Max(1, baseCost / 2);
            if (ActiveResonances.Contains("용풍") || Runes.Contains("용")) return Math.Max(1, (int)(baseCost * 0.75f));
            return baseCost;
        }

        // 파생 능력치 (룬 및 공명 보너스 반영)
        public int MaxHp => 100 + (Endurance - 10) * 12 + (Runes.Contains("철") ? 20 : 0) + (ActiveResonances.Contains("철화") ? 30 : 0);
        public int MaxMp => 50 + (Ki - 10) * 8 + (Runes.Contains("천") ? 20 : 0) + (ActiveResonances.Contains("용비어천가") ? 50 : 0);
        public float MoveSpeed => (80f + (Agility - 10) * 4f) * (Runes.Contains("풍") ? 1.15f : 1.0f) * (ActiveResonances.Contains("용풍") ? 1.15f : 1.0f);
        public float DashSpeed => (135f + (Agility - 10) * 6f) * (Runes.Contains("풍") ? 1.15f : 1.0f) * (ActiveResonances.Contains("용풍") ? 1.15f : 1.0f);
        public float JumpPower => 235f + (Agility - 10) * 6f;

        public int PunchDamage => 8 + (Power - 10) * 2 + (Runes.Contains("화") ? 4 : 0) + (ActiveResonances.Contains("철화") ? 4 : 0);
        public int KickDamage => 14 + (Power - 10) * 3 + (Runes.Contains("화") ? 4 : 0) + (ActiveResonances.Contains("철화") ? 4 : 0);
        public int SpecialDamage => (int)Math.Round(((25 + (Ki - 10) * 4 + (Power - 10) * 2) * 1.5) * (ActiveResonances.Contains("용비어천가") ? 1.4f : (Runes.Contains("용") ? 1.15f : 1.0f)));

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
