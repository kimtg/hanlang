using System;
using System.IO;
using System.Text.Json;

namespace HanlangGame.Systems
{
    public class SaveData
    {
        public int Level { get; set; } = 1;
        public int CurrentExp { get; set; } = 0;
        public int Gold { get; set; } = 100;
        public int StatPoints { get; set; } = 3;

        public int Power { get; set; } = 10;
        public int Endurance { get; set; } = 10;
        public int Agility { get; set; } = 10;
        public int Ki { get; set; } = 10;

        public int CurrentHp { get; set; } = 100;
        public int CurrentMp { get; set; } = 50;
        public int StageIndex { get; set; } = 1;
        public string SaveDate { get; set; } = "";
    }

    public static class SaveLoadSystem
    {
        private static readonly string SaveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

        public static bool HasSaveData()
        {
            return File.Exists(SaveFilePath);
        }

        public static bool Save(StatusSystem status, int stageIndex)
        {
            try
            {
                var data = new SaveData
                {
                    Level = status.Level,
                    CurrentExp = status.CurrentExp,
                    Gold = status.Gold,
                    StatPoints = status.StatPoints,
                    Power = status.Power,
                    Endurance = status.Endurance,
                    Agility = status.Agility,
                    Ki = status.Ki,
                    CurrentHp = status.CurrentHp,
                    CurrentMp = status.CurrentMp,
                    StageIndex = stageIndex,
                    SaveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SaveFilePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static SaveData? Load(StatusSystem status, out int stageIndex)
        {
            stageIndex = 1;
            try
            {
                if (!HasSaveData()) return null;

                string json = File.ReadAllText(SaveFilePath);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                if (data != null)
                {
                    status.Level = data.Level;
                    status.CurrentExp = data.CurrentExp;
                    status.Gold = data.Gold;
                    status.StatPoints = data.StatPoints;
                    status.Power = data.Power;
                    status.Endurance = data.Endurance;
                    status.Agility = data.Agility;
                    status.Ki = data.Ki;
                    status.CurrentHp = data.CurrentHp;
                    status.CurrentMp = data.CurrentMp;
                    stageIndex = data.StageIndex;
                    return data;
                }
            }
            catch
            {
                // load failed
            }
            return null;
        }
    }
}
