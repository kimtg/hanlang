using System;
using System.Collections.Generic;
using System.IO;

namespace HanlangGame.Engine
{
    public static class MidiGenerator
    {
        private sealed class TimedEvent
        {
            public int Tick { get; }
            public byte[] Data { get; }
            public int Priority { get; }

            public TimedEvent(int tick, byte[] data, int priority = 0)
            {
                Tick = tick;
                Data = data;
                Priority = priority;
            }
        }

        public static int NoteToMidi(string note)
        {
            if (string.IsNullOrEmpty(note) || note == "-" || note == "=" || note == ".") return -1;

            char pitchChar = char.ToUpperInvariant(note[0]);
            int semitone = pitchChar switch
            {
                'C' => 0,
                'D' => 2,
                'E' => 4,
                'F' => 5,
                'G' => 7,
                'A' => 9,
                'B' => 11,
                _ => -1
            };
            if (semitone < 0) return -1;

            int octaveIdx = 1;
            if (note.Length > 1 && (note[1] == '#' || note[1] == 's' || note[1] == 'S'))
            {
                semitone += 1;
                octaveIdx = 2;
            }
            else if (note.Length > 1 && (note[1] == 'b' || note[1] == 'f' || note[1] == 'F'))
            {
                semitone -= 1;
                octaveIdx = 2;
            }

            if (octaveIdx < note.Length && int.TryParse(note.Substring(octaveIdx), out int octave))
            {
                return (octave + 1) * 12 + semitone;
            }
            return -1;
        }

        private static void WriteVarLen(BinaryWriter bw, int value)
        {
            int buffer = value & 0x7F;
            while ((value >>= 7) > 0)
            {
                buffer <<= 8;
                buffer |= 0x80;
                buffer += (value & 0x7F);
            }
            while (true)
            {
                bw.Write((byte)(buffer & 0xFF));
                if ((buffer & 0x80) != 0) buffer >>= 8;
                else break;
            }
        }

        public static string GetStageBgmTrack(int stage) => stage switch
        {
            1 or 2 => "stage_market",
            3 or 4 => "stage_fortress",
            5 or 6 => "stage_night",
            7 or 8 => "stage_palace",
            9 => "stage_palace",
            _ => "stage_market"
        };

        public static byte[] GenerateBgmMid(string trackName)
        {
            float bpm;
            int totalSteps;
            int ch0Prog = 107; // Koto (가야금/거문고)
            int ch1Prog = 73;  // Flute (대금/피리)
            int ch2Prog = 48;  // String Ensemble 1 (궁중 현악 합주)
            int ch3Prog = 32;  // Acoustic Bass (콘트라베이스)
            string[] melody;
            string[] bass;
            string[] drums;

            switch (trackName.ToLowerInvariant())
            {
                case "midboss":
                    // 「숙명의 대결」 (168 BPM, D 계면조, 128 스텝 16비트 배틀 비트) - 중간보스 마달 & 철웅 테마
                    bpm = 168f;
                    totalSteps = 128;
                    ch0Prog = 107; // Koto (맹렬한 배틀 가야금 아르페지오)
                    ch1Prog = 77;  // Shakuhachi (날카로운 살수 피리 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (배틀 스트링)
                    ch3Prog = 32;  // Acoustic Bass (전투 베이스)
                    melody = new string[]
                    {
                        "D4", "F4", "A4", "D5", "Eb5", "D5", "Bb4", "A4", "F4", "G4", "A4", "Bb4", "A4", "G4", "F4", "Eb4",
                        "D4", "D4", "F4", "G4", "A4", "Bb4", "C5", "D5", "Eb5", "D5", "C5", "Bb4", "A4", "G4", "F4", "Eb4",
                        "D5", "D5", "Eb5", "D5", "C5", "Bb4", "A4", "G4", "F4", "G4", "A4", "F4", "Eb4", "D4", "C4", "D4",
                        "A4", "Bb4", "C5", "D5", "Eb5", "F5", "G5", "F5", "Eb5", "D5", "C5", "Bb4", "A4", "Bb4", "A4", "-",
                        "D5", "F5", "A5", "D6", "C6", "Bb5", "A5", "G5", "F5", "G5", "A5", "Bb5", "A5", "G5", "F5", "Eb5",
                        "D5", "Eb5", "D5", "C5", "Bb4", "C5", "D5", "Eb5", "F5", "G5", "A5", "Bb5", "C6", "Bb5", "A5", "G5",
                        "F5", "G5", "A5", "F5", "Eb5", "F5", "G5", "Eb5", "D5", "Eb5", "F5", "D5", "C5", "D5", "Eb5", "C5",
                        "D4", "F4", "A4", "D5", "Eb5", "D5", "A4", "F4", "D4", "D4", "F4", "A4", "D5", "-", "D5", "-"
                    };
                    bass = new string[]
                    {
                        "D2", "D2", "D2", "D2", "Eb2", "Eb2", "D2", "D2", "F2", "F2", "G2", "G2", "A2", "G2", "F2", "Eb2",
                        "D2", "D2", "D2", "D2", "F2", "F2", "D2", "D2", "G2", "G2", "A2", "A2", "Bb2", "A2", "G2", "F2",
                        "D2", "D2", "D2", "D2", "Eb2", "Eb2", "D2", "D2", "C2", "C2", "C2", "C2", "D2", "D2", "Eb2", "D2",
                        "A1", "A1", "Bb1", "Bb1", "C2", "C2", "D2", "D2", "Eb2", "Eb2", "F2", "F2", "G2", "G2", "A2", "A2",
                        "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "Eb2", "Eb2", "Eb2", "Eb2", "D2", "D2", "D2", "D2",
                        "Bb1", "Bb1", "Bb1", "Bb1", "C2", "C2", "C2", "C2", "D2", "D2", "D2", "D2", "Eb2", "Eb2", "Eb2", "Eb2",
                        "F2", "F2", "F2", "F2", "Eb2", "Eb2", "Eb2", "Eb2", "D2", "D2", "D2", "D2", "C2", "C2", "C2", "C2",
                        "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "-", "D2", "-"
                    };
                    drums = new string[]
                    {
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "C", "C"
                    };
                    break;

                case "finalboss":
                case "boss":
                    // 「마도학사 흑호의 강림」 (180 BPM, C Phrygian 다크 배틀 모드, 128 스텝) - 9장 최종결전 테마
                    bpm = 180f;
                    totalSteps = 128;
                    ch0Prog = 107; // Koto (질풍노도의 배틀 가야금)
                    ch1Prog = 19;  // Church Organ (마도학사 흑호의 웅장한 파이프오르간)
                    ch2Prog = 48;  // String Ensemble 1 (어둠의 장엄한 풀 오케스트라)
                    ch3Prog = 32;  // Acoustic Bass (위압적인 배틀 베이스)
                    melody = new string[]
                    {
                        "C4", "Eb4", "G4", "C5", "Db5", "C5", "Ab4", "G4", "Eb4", "F4", "G4", "Ab4", "G4", "F4", "Eb4", "Db4",
                        "C4", "C4", "Eb4", "F4", "G4", "Ab4", "Bb4", "C5", "Db5", "C5", "Bb4", "Ab4", "G4", "F4", "Eb4", "Db4",
                        "C5", "C5", "Db5", "C5", "Bb4", "Ab4", "G4", "F4", "Eb4", "F4", "G4", "Eb4", "Db4", "C4", "Bb3", "C4",
                        "G4", "Ab4", "Bb4", "C5", "Db5", "Eb5", "F5", "Eb5", "Db5", "C5", "Bb4", "Ab4", "G4", "Ab4", "G4", "-",
                        "C5", "Eb5", "G5", "C6", "Bb5", "Ab5", "G5", "F5", "Eb5", "F5", "G5", "Ab5", "G5", "F5", "Eb5", "Db5",
                        "C5", "Db5", "C5", "Bb4", "Ab4", "Bb4", "C5", "Db5", "Eb5", "F5", "G5", "Ab5", "Bb5", "Ab5", "G5", "F5",
                        "Eb5", "F5", "G5", "Eb5", "Db5", "Eb5", "F5", "Db5", "C5", "Db5", "Eb5", "C5", "Bb4", "C5", "Db5", "Bb4",
                        "C4", "Eb4", "G4", "C5", "Db5", "C5", "G4", "Eb4", "C4", "C4", "Eb4", "G4", "C5", "-", "C5", "-"
                    };
                    bass = new string[]
                    {
                        "C2", "C2", "C2", "C2", "Db2", "Db2", "C2", "C2", "Eb2", "Eb2", "F2", "F2", "G2", "F2", "Eb2", "Db2",
                        "C2", "C2", "C2", "C2", "Eb2", "Eb2", "C2", "C2", "F2", "F2", "G2", "G2", "Ab2", "G2", "F2", "Eb2",
                        "C2", "C2", "C2", "C2", "Db2", "Db2", "C2", "C2", "Bb1", "Bb1", "Bb1", "Bb1", "C2", "C2", "Db2", "C2",
                        "G1", "G1", "Ab1", "Ab1", "Bb1", "Bb1", "C2", "C2", "Db2", "Db2", "Eb2", "Eb2", "F2", "F2", "G2", "G2",
                        "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "Db2", "Db2", "Db2", "Db2", "C2", "C2", "C2", "C2",
                        "Ab1", "Ab1", "Ab1", "Ab1", "Bb1", "Bb1", "Bb1", "Bb1", "C2", "C2", "C2", "C2", "Db2", "Db2", "Db2", "Db2",
                        "Eb2", "Eb2", "Eb2", "Eb2", "Db2", "Db2", "Db2", "Db2", "C2", "C2", "C2", "C2", "Bb1", "Bb1", "Bb1", "Bb1",
                        "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "-", "C2", "-"
                    };
                    drums = new string[]
                    {
                        "C", "T", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "T", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "C",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "T", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "C",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C"
                    };
                    break;

                case "stage_fortress":
                    // 「남산 성곽의 황혼」 (128 BPM, A 계면조 굿거리/타령) - 3장 청계천 & 4장 남산 성곽길
                    bpm = 128f;
                    totalSteps = 128;
                    ch0Prog = 40;  // Violin (해금 질감의 애잔하고 비장한 선율)
                    ch1Prog = 77;  // Shakuhachi (성곽의 황혼을 가르는 피리 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (따스하고 비장한 현악 패드)
                    ch3Prog = 32;  // Acoustic Bass
                    melody = new string[]
                    {
                        "A4", "C5", "D5", "E5", "G5", "E5", "D5", "C5", "A4", "C5", "D5", "E5", "D5", "C5", "A4", "G4",
                        "A4", "A4", "C5", "D5", "E5", "=", "D5", "C5", "D5", "E5", "G5", "A5", "G5", "E5", "D5", "-",
                        "E5", "G5", "A5", "C6", "A5", "G5", "E5", "D5", "C5", "D5", "E5", "G5", "E5", "D5", "C5", "A4",
                        "D5", "E5", "D5", "C5", "A4", "G4", "E4", "G4", "A4", "C5", "D5", "C5", "A4", "-", "A4", "-",
                        "A5", "G5", "E5", "D5", "C5", "D5", "E5", "G5", "A5", "C6", "D6", "C6", "A5", "G5", "E5", "D5",
                        "E5", "G5", "A5", "G5", "E5", "D5", "C5", "D5", "E5", "D5", "C5", "A4", "G4", "A4", "C5", "D5",
                        "C5", "A4", "G4", "E4", "G4", "A4", "C5", "D5", "E5", "G5", "E5", "D5", "C5", "A4", "G4", "E4",
                        "A4", "C5", "D5", "E5", "G5", "E5", "D5", "C5", "A4", "A4", "C5", "E5", "A5", "-", "A5", "-"
                    };
                    bass = new string[]
                    {
                        "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "D3", "-", "D3", "D3", "E3", "-", "D3", "C3",
                        "A2", "-", "A2", "A2", "E3", "-", "E3", "E3", "D3", "-", "D3", "D3", "C3", "-", "D3", "E3",
                        "A2", "-", "C3", "-", "D3", "-", "E3", "-", "G3", "-", "E3", "-", "D3", "-", "C3", "-",
                        "D3", "-", "C3", "-", "A2", "-", "G2", "-", "A2", "-", "C3", "-", "A2", "-", "A2", "-",
                        "A2", "A2", "C3", "C3", "D3", "D3", "E3", "E3", "G3", "G3", "E3", "E3", "D3", "D3", "C3", "C3",
                        "E3", "E3", "G3", "G3", "A3", "A3", "G3", "G3", "E3", "E3", "D3", "D3", "C3", "C3", "D3", "D3",
                        "C3", "C3", "A2", "A2", "G2", "G2", "E2", "E2", "G2", "G2", "A2", "A2", "C3", "C3", "D3", "D3",
                        "A2", "-", "C3", "-", "D3", "-", "E3", "-", "A2", "-", "E3", "-", "A2", "-", "A2", "-"
                    };
                    drums = new string[]
                    {
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", "H", "S", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", "H", "S", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", "H", "K", "K", "S", "S", "C", "C", "C", "-"
                    };
                    break;

                case "stage_night":
                    // 「달빛 아래 육조거리」 (144 BPM, D 계면조 동살풀이 첩보 비트) - 5장 주막거리 & 6장 육조거리
                    bpm = 144f;
                    totalSteps = 128;
                    ch0Prog = 107; // Koto (거문고/가야금의 탄력 있는 심야 리프)
                    ch1Prog = 73;  // Flute (대금 날카로운 대선율)
                    ch2Prog = 45;  // Pizzicato Strings (긴장감 넘치는 피치카토 반주)
                    ch3Prog = 32;  // Acoustic Bass (긴박한 워킹 베이스)
                    melody = new string[]
                    {
                        "D4", "F4", "G4", "A4", "D5", "C5", "A4", "G4", "F4", "G4", "A4", "C5", "A4", "G4", "F4", "D4",
                        "D4", "D4", "F4", "F4", "G4", "G4", "A4", "A4", "C5", "D5", "C5", "A4", "G4", "F4", "D4", "-",
                        "A4", "C5", "D5", "F5", "D5", "C5", "A4", "G4", "F4", "A4", "C5", "D5", "C5", "A4", "G4", "F4",
                        "G4", "A4", "C5", "D5", "F5", "G5", "F5", "D5", "C5", "D5", "C5", "A4", "F4", "G4", "A4", "-",
                        "D5", "D5", "F5", "G5", "A5", "A5", "G5", "F5", "D5", "F5", "G5", "A5", "C6", "A5", "G5", "F5",
                        "G5", "A5", "C6", "D6", "C6", "A5", "G5", "F5", "D5", "F5", "G5", "F5", "D5", "C5", "D5", "-",
                        "A5", "G5", "F5", "D5", "C5", "D5", "F5", "G5", "A5", "G5", "F5", "D5", "C5", "A4", "C5", "D5",
                        "D4", "F4", "G4", "A4", "C5", "A4", "G4", "F4", "D4", "D4", "F4", "A4", "D5", "-", "D5", "-"
                    };
                    bass = new string[]
                    {
                        "D2", "-", "D2", "F2", "G2", "-", "A2", "-", "D2", "-", "F2", "-", "G2", "-", "A2", "-",
                        "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "G2", "-", "G2", "G2", "A2", "G2", "F2", "D2",
                        "D2", "-", "F2", "-", "G2", "-", "A2", "-", "Bb2", "-", "A2", "-", "G2", "-", "F2", "-",
                        "G2", "-", "A2", "-", "C3", "-", "D3", "-", "G2", "-", "F2", "-", "D2", "-", "D2", "-",
                        "D2", "D2", "F2", "F2", "G2", "G2", "A2", "A2", "D2", "D2", "F2", "F2", "G2", "G2", "A2", "A2",
                        "G2", "G2", "A2", "A2", "C3", "C3", "D3", "D3", "G2", "G2", "F2", "F2", "D2", "D2", "D2", "D2",
                        "D2", "-", "F2", "-", "G2", "-", "A2", "-", "Bb2", "-", "A2", "-", "G2", "-", "F2", "-",
                        "D2", "-", "D2", "F2", "G2", "-", "A2", "-", "D2", "-", "A2", "-", "D2", "-", "D2", "-"
                    };
                    drums = new string[]
                    {
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", "H", "S", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "C", "C"
                    };
                    break;

                case "stage_palace":
                    // 「궁궐의 폭풍전야」 (160 BPM, G 우조 휘모리 장단) - 7장 광화문 & 8장 근정전 & 9장 집현전
                    bpm = 160f;
                    totalSteps = 128;
                    ch0Prog = 59;  // Muted Trumpet (태평소 당당한 궁중 취타 팡파르)
                    ch1Prog = 73;  // Flute (대금 화음 고음 선율)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 궁중 오케스트라)
                    ch3Prog = 32;  // Acoustic Bass (위엄 있는 베이스)
                    melody = new string[]
                    {
                        "G4", "G4", "D5", "D5", "E5", "E5", "G5", "E5", "D5", "E5", "D5", "B4", "A4", "B4", "D5", "E5",
                        "G5", "G5", "A5", "G5", "E5", "D5", "B4", "A4", "G4", "A4", "B4", "D5", "B4", "A4", "G4", "-",
                        "D5", "E5", "G5", "A5", "B5", "A5", "G5", "E5", "D5", "E5", "G5", "E5", "D5", "B4", "A4", "B4",
                        "D5", "D5", "E5", "G5", "A5", "B5", "D6", "B5", "A5", "G5", "E5", "D5", "B4", "A4", "G4", "-",
                        "G5", "D6", "B5", "A5", "G5", "A5", "B5", "D6", "E6", "D6", "B5", "A5", "G5", "E5", "D5", "E5",
                        "G5", "A5", "B5", "D6", "B5", "A5", "G5", "E5", "D5", "E5", "D5", "B4", "A4", "B4", "D5", "-",
                        "B5", "A5", "G5", "E5", "D5", "B4", "A4", "G4", "A4", "B4", "D5", "E5", "G5", "E5", "D5", "B4",
                        "G4", "B4", "D5", "G5", "E5", "D5", "B4", "A4", "G4", "G4", "B4", "D5", "G5", "-", "G5", "-"
                    };
                    bass = new string[]
                    {
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "E3", "-", "D3", "B2",
                        "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "-", "B2", "B2", "D3", "B2", "A2", "G2",
                        "D2", "-", "D2", "D2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "-", "A2", "G2",
                        "G2", "-", "A2", "A2", "B2", "-", "D3", "D3", "B2", "-", "A2", "G2", "D2", "-", "D2", "-",
                        "G2", "G2", "B2", "B2", "D3", "D3", "E3", "E3", "G3", "G3", "E3", "E3", "D3", "D3", "B2", "B2",
                        "G2", "G2", "A2", "A2", "B2", "B2", "D3", "D3", "B2", "B2", "A2", "A2", "G2", "G2", "D2", "D2",
                        "E2", "E2", "G2", "G2", "A2", "A2", "B2", "B2", "D3", "D3", "B2", "B2", "A2", "A2", "G2", "G2",
                        "G2", "-", "B2", "-", "D3", "-", "G3", "-", "G2", "-", "D3", "-", "G2", "-", "G2", "-"
                    };
                    drums = new string[]
                    {
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "C", "C"
                    };
                    break;

                case "title":
                    // 「대취타 유생」 (122 BPM, C 우조 대취타 궁중 행진곡, 64 스텝)
                    bpm = 122f;
                    totalSteps = 64;
                    ch0Prog = 56;  // Trumpet (태평소 당당한 팡파르 선율)
                    ch1Prog = 73;  // Flute (대금 고음 화음)
                    ch2Prog = 48;  // String Ensemble 1 (장엄한 궁중 대취타 관현악)
                    ch3Prog = 32;  // Acoustic Bass (위풍당당 행진 베이스)
                    melody = new string[]
                    {
                        "C4", "C4", "G4", "G4", "A4", "A4", "C5", "A4", "G4", "G4", "F4", "D4", "C4", "D4", "F4", "G4",
                        "A4", "A4", "C5", "D5", "C5", "A4", "G4", "F4", "G4", "A4", "G4", "F4", "D4", "C4", "D4", "-",
                        "C5", "C5", "D5", "C5", "A4", "G4", "A4", "C5", "D5", "D5", "C5", "A4", "G4", "F4", "D4", "C4",
                        "F4", "G4", "A4", "C5", "D5", "C5", "A4", "G4", "F4", "D4", "C4", "D4", "C4", "-", "C4", "-"
                    };
                    bass = new string[]
                    {
                        "C2", "-", "C2", "C2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "G2", "-", "F2", "G2",
                        "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "G2", "-", "G2", "G2", "D2", "-", "D2", "G2",
                        "C2", "-", "C2", "C2", "F2", "-", "F2", "F2", "G2", "-", "G2", "G2", "A2", "-", "G2", "F2",
                        "F2", "-", "G2", "G2", "A2", "-", "C3", "C3", "G2", "-", "F2", "D2", "C2", "-", "C2", "-"
                    };
                    drums = new string[]
                    {
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", ".", "K", "K", "S", ".", "C", ".", ".", "."
                    };
                    break;

                case "shop":
                    // 「도성 주막 풍류」 (130 BPM, A 경기민요 / 타령 스윙, 64 스텝)
                    bpm = 130f;
                    totalSteps = 64;
                    ch0Prog = 107; // Koto (가야금 타령 가락)
                    ch1Prog = 74;  // Recorder (단소 구성진 목관)
                    ch2Prog = 45;  // Pizzicato Strings (통통 튀는 피치카토 반주)
                    ch3Prog = 32;  // Acoustic Bass (유쾌한 스윙 베이스)
                    melody = new string[]
                    {
                        "A4", "C5", "D5", "E5", "D5", "C5", "A4", "G4", "A4", "C5", "D5", "C5", "A4", "G4", "E4", "G4",
                        "A4", "A4", "C5", "D5", "E5", "G5", "E5", "D5", "C5", "D5", "C5", "A4", "G4", "A4", "A4", "-",
                        "E5", "E5", "G5", "E5", "D5", "C5", "D5", "E5", "G5", "E5", "D5", "C5", "A4", "C5", "D5", "E5",
                        "D5", "C5", "A4", "G4", "A4", "C5", "D5", "C5", "A4", "G4", "E4", "G4", "A4", "-", "A4", "-"
                    };
                    bass = new string[]
                    {
                        "A2", "-", "C3", "-", "D3", "-", "E3", "-", "A2", "-", "C3", "-", "D3", "-", "E3", "-",
                        "A2", "-", "A2", "C3", "D3", "-", "E3", "G3", "E3", "-", "D3", "C3", "A2", "-", "A2", "-",
                        "E2", "-", "G2", "-", "A2", "-", "C3", "-", "D3", "-", "E3", "-", "C3", "-", "A2", "-",
                        "D3", "-", "C3", "-", "A2", "-", "G2", "-", "A2", "-", "C3", "-", "A2", "-", "A2", "-"
                    };
                    drums = new string[]
                    {
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", "K", "H", "H", "S", ".", "H", "C"
                    };
                    break;

                case "victory":
                    // 「훈민정음 만세」 (140 BPM, D 평조 환희의 피날레, 64 스텝)
                    bpm = 140f;
                    totalSteps = 64;
                    ch0Prog = 107; // Koto (가야금 환희 주선율)
                    ch1Prog = 73;  // Flute (대금 화려한 멜로디)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 승리 오케스트레이션)
                    ch3Prog = 32;  // Acoustic Bass (축제 베이스)
                    melody = new string[]
                    {
                        "D4", "D4", "G4", "A4", "B4", "B4", "D5", "B4", "A4", "B4", "D5", "E5", "D5", "B4", "A4", "G4",
                        "A4", "B4", "D5", "E5", "G5", "G5", "E5", "D5", "B4", "D5", "E5", "D5", "B4", "A4", "B4", "-",
                        "D5", "D5", "E5", "D5", "B4", "A4", "B4", "D5", "E5", "E5", "D5", "B4", "A4", "G4", "E4", "D4",
                        "G4", "A4", "B4", "D5", "E5", "G5", "A5", "G5", "E5", "D5", "B4", "A4", "G4", "-", "G4", "-"
                    };
                    bass = new string[]
                    {
                        "D2", "-", "D2", "D2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "G2", "-", "A2", "G2",
                        "A2", "-", "A2", "A2", "D3", "-", "D3", "D3", "G2", "-", "G2", "G2", "E2", "-", "G2", "A2",
                        "D2", "-", "D2", "D2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "-", "A2", "G2",
                        "G2", "-", "A2", "A2", "B2", "-", "D3", "D3", "B2", "-", "A2", "G2", "D2", "-", "D2", "-"
                    };
                    drums = new string[]
                    {
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "S", "S", "C", ".", ".", "."
                    };
                    break;

                default: // "stage_market" / "stage" - 「한양 저잣거리 활극」 (148 BPM, E 계면조 / G 평조 자진모리 장단, 128 스텝)
                    bpm = 148f;
                    totalSteps = 128;
                    ch0Prog = 107; // Koto (가야금 청명한 활극 리프)
                    ch1Prog = 73;  // Flute (대금 화음 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (풍성한 궁중 현악 패드)
                    ch3Prog = 32;  // Acoustic Bass (묵직한 리듬 베이스)
                    melody = new string[]
                    {
                        "E4", "E4", "G4", "A4", "B4", "B4", "A4", "G4", "E4", "G4", "A4", "B4", "D5", "B4", "A4", "G4",
                        "A4", "A4", "B4", "D5", "E5", "E5", "D5", "B4", "A4", "B4", "A4", "G4", "E4", "E4", "E4", "-",
                        "B4", "D5", "E5", "G5", "E5", "D5", "B4", "A4", "B4", "D5", "B4", "A4", "G4", "E4", "D4", "E4",
                        "G4", "A4", "B4", "D5", "E5", "G5", "A5", "G5", "E5", "D5", "B4", "A4", "G4", "A4", "B4", "-",
                        "E5", "E5", "D5", "B4", "A4", "B4", "D5", "B4", "A4", "G4", "E4", "D4", "E4", "G4", "A4", "B4",
                        "D5", "D5", "B4", "A4", "G4", "A4", "B4", "D5", "E5", "D5", "B4", "A4", "G4", "E4", "G4", "A4",
                        "B4", "B4", "D5", "E5", "G5", "G5", "E5", "D5", "B4", "D5", "E5", "D5", "B4", "A4", "G4", "E4",
                        "G4", "A4", "B4", "D5", "B4", "A4", "G4", "E4", "D4", "E4", "G4", "E4", "E4", "-", "E4", "-"
                    };
                    bass = new string[]
                    {
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "A2", "G2", "E2",
                        "A2", "-", "A2", "A2", "B2", "-", "B2", "B2", "D3", "-", "B2", "A2", "E2", "E2", "G2", "E2",
                        "E2", "E2", "G2", "E2", "A2", "A2", "B2", "A2", "G2", "E2", "D2", "E2", "G2", "A2", "B2", "G2",
                        "E2", "-", "G2", "A2", "B2", "-", "D3", "B2", "A2", "G2", "E2", "D2", "E2", "E2", "B2", "E2",
                        "E2", "E2", "B2", "E2", "G2", "G2", "D3", "G2", "A2", "A2", "E3", "A2", "B2", "B2", "A2", "G2",
                        "D2", "D2", "A2", "D2", "G2", "G2", "D3", "G2", "E2", "E2", "B2", "E2", "A2", "B2", "D3", "B2",
                        "E2", "-", "E2", "G2", "A2", "-", "A2", "B2", "D3", "-", "B2", "A2", "G2", "E2", "D2", "E2",
                        "G2", "A2", "B2", "D3", "B2", "A2", "G2", "E2", "D2", "E2", "G2", "E2", "E2", "-", "E2", "-"
                    };
                    drums = new string[]
                    {
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", ".", "S", "H", "S", "O",
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "S", "C"
                    };
                    break;
            }

            // BGM 연주 속도를 기존의 80% 수준으로 조절 (BPM * 0.8)
            bpm *= 0.8f;

            var events = new List<TimedEvent>();
            const int stepTicks = 24; // 96 ticks per quarter note -> 24 ticks per 16th note step

            // 1. Tick 0: Program Changes & Controllers for each channel
            // Ch 0 (Lead)
            events.Add(new TimedEvent(0, new byte[] { 0xC0, (byte)ch0Prog }));
            events.Add(new TimedEvent(0, new byte[] { 0xB0, 0x07, 110 })); // Volume 110
            events.Add(new TimedEvent(0, new byte[] { 0xB0, 0x5B, 64 }));  // Reverb 64
            events.Add(new TimedEvent(0, new byte[] { 0xB0, 0x0A, 56 }));  // Pan slightly Left

            // Ch 1 (Woodwind Counter-Melody)
            events.Add(new TimedEvent(0, new byte[] { 0xC1, (byte)ch1Prog }));
            events.Add(new TimedEvent(0, new byte[] { 0xB1, 0x07, 95 }));  // Volume 95
            events.Add(new TimedEvent(0, new byte[] { 0xB1, 0x5B, 80 }));  // Reverb 80
            events.Add(new TimedEvent(0, new byte[] { 0xB1, 0x0A, 72 }));  // Pan slightly Right

            // Ch 2 (Strings Ensemble Harmony Pad)
            events.Add(new TimedEvent(0, new byte[] { 0xC2, (byte)ch2Prog }));
            events.Add(new TimedEvent(0, new byte[] { 0xB2, 0x07, 80 }));  // Volume 80
            events.Add(new TimedEvent(0, new byte[] { 0xB2, 0x5B, 90 }));  // Reverb 90
            events.Add(new TimedEvent(0, new byte[] { 0xB2, 0x0A, 64 }));  // Pan Center

            // Ch 3 (Acoustic Bass)
            events.Add(new TimedEvent(0, new byte[] { 0xC3, (byte)ch3Prog }));
            events.Add(new TimedEvent(0, new byte[] { 0xB3, 0x07, 105 })); // Volume 105
            events.Add(new TimedEvent(0, new byte[] { 0xB3, 0x5B, 30 }));  // Reverb 30
            events.Add(new TimedEvent(0, new byte[] { 0xB3, 0x0A, 64 }));  // Pan Center

            // Ch 9 (GM Percussion / Drums)
            events.Add(new TimedEvent(0, new byte[] { 0xB9, 0x07, 115 })); // Volume 115
            events.Add(new TimedEvent(0, new byte[] { 0xB9, 0x5B, 60 }));  // Reverb 60

            // 2. Generate Notes across steps
            for (int s = 0; s < totalSteps; s++)
            {
                int stepTick = s * stepTicks;

                // --- Channel 0: Melody (Koto / 가야금 / 해금 / 태평소) ---
                string mStr = melody[s % melody.Length];
                int mNote = NoteToMidi(mStr);
                if (mNote >= 0)
                {
                    // Check ties or rests for duration
                    int durTicks = stepTicks;
                    int lookAhead = s + 1;
                    while (lookAhead < totalSteps && (melody[lookAhead % melody.Length] == "=" || melody[lookAhead % melody.Length] == "-"))
                    {
                        durTicks += stepTicks;
                        lookAhead++;
                    }

                    int gateTicks = Math.Max(12, durTicks - 4);
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x90, (byte)mNote, 105 }, 1));
                    events.Add(new TimedEvent(stepTick + gateTicks, new byte[] { 0x80, (byte)mNote, 0 }, 0));

                    // 한국 전통 가야금/해금 농현(弄絃, Pitch Bend 시김새)
                    // 지속음(2스텝 이상)일 때 줄을 눌렀다 놓는 피치 벤드 진동 삽입
                    if (durTicks >= stepTicks * 2)
                    {
                        int bendTick1 = stepTick + (durTicks / 2);
                        int bendTick2 = stepTick + (durTicks * 3 / 4);
                        events.Add(new TimedEvent(bendTick1, new byte[] { 0xE0, 0x00, 0x54 }, 1)); // 미세 올림
                        events.Add(new TimedEvent(bendTick2, new byte[] { 0xE0, 0x00, 0x34 }, 1)); // 미세 내림
                        events.Add(new TimedEvent(stepTick + gateTicks, new byte[] { 0xE0, 0x00, 0x40 }, 0)); // 원위치 복귀
                    }

                    // --- Channel 1: Woodwind / Counter-melody (대금 / 피리 시김새 & 비브라토) ---
                    // Flute joins in unison or 1 octave higher for sustained/strong notes
                    if (durTicks >= stepTicks * 2 || (s % 4 == 0))
                    {
                        int fNote = (mNote <= 72) ? (mNote + 12) : mNote;
                        int fGate = Math.Max(16, durTicks - 2);
                        events.Add(new TimedEvent(stepTick, new byte[] { 0x91, (byte)fNote, 90 }, 1));
                        events.Add(new TimedEvent(stepTick + fGate, new byte[] { 0x81, (byte)fNote, 0 }, 0));

                        // 대금/피리 지속음 비브라토 모듈레이션 (CC 0x01)
                        if (durTicks >= stepTicks * 2)
                        {
                            int vibTick = stepTick + (durTicks / 2);
                            events.Add(new TimedEvent(vibTick, new byte[] { 0xB1, 0x01, 64 }, 1)); // Vibrato ON
                            events.Add(new TimedEvent(stepTick + fGate, new byte[] { 0xB1, 0x01, 0 }, 0)); // Vibrato OFF
                        }
                    }
                }

                // --- Channel 3: Bass (Acoustic Bass) ---
                string bStr = bass[s % bass.Length];
                int bNote = NoteToMidi(bStr);
                if (bNote >= 0)
                {
                    int durTicks = stepTicks;
                    int lookAhead = s + 1;
                    while (lookAhead < totalSteps && (bass[lookAhead % bass.Length] == "=" || bass[lookAhead % bass.Length] == "-"))
                    {
                        durTicks += stepTicks;
                        lookAhead++;
                    }
                    int gateTicks = Math.Max(12, durTicks - 4);
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x93, (byte)bNote, 100 }, 1));
                    events.Add(new TimedEvent(stepTick + gateTicks, new byte[] { 0x83, (byte)bNote, 0 }, 0));

                    // --- Channel 2: Strings Ensemble Pad on measure beats ---
                    // Every 4 steps (quarter note beat), if bass exists, create warm string harmony
                    if (s % 4 == 0)
                    {
                        int root = bNote + 12; // Octave up from bass
                        while (root < 48) root += 12;
                        while (root > 60) root -= 12;
                        int fifth = root + 7;

                        int chordDur = stepTicks * 4 - 4; // 1 beat pad
                        events.Add(new TimedEvent(stepTick, new byte[] { 0x92, (byte)root, 75 }, 1));
                        events.Add(new TimedEvent(stepTick + chordDur, new byte[] { 0x82, (byte)root, 0 }, 0));
                        events.Add(new TimedEvent(stepTick, new byte[] { 0x92, (byte)fifth, 68 }, 1));
                        events.Add(new TimedEvent(stepTick + chordDur, new byte[] { 0x82, (byte)fifth, 0 }, 0));
                    }
                }

                // --- Channel 9 (MIDI Ch 10): Percussion & Drums (장구/북/징 다이내믹스) ---
                string dStr = drums[s % drums.Length];
                bool isAccent = (s % 8 == 0 || s % 8 == 4);
                byte kVel = (byte)(isAccent ? 115 : 85);
                byte sVel = (byte)(isAccent ? 105 : 75);
                byte hVel = (byte)(isAccent ? 80 : 60);

                if (dStr.Contains('K')) // Taiko / Bass Drum (Note 35 / 36)
                {
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x99, 35, kVel }, 1));
                    events.Add(new TimedEvent(stepTick + 12, new byte[] { 0x89, 35, 0 }, 0));
                }
                if (dStr.Contains('S')) // Janggu Chae / Snare (Note 38 / 40)
                {
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x99, 38, sVel }, 1));
                    events.Add(new TimedEvent(stepTick + 12, new byte[] { 0x89, 38, 0 }, 0));
                }
                if (dStr.Contains('H')) // Closed Hi-Hat (Note 42)
                {
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x99, 42, hVel }, 1));
                    events.Add(new TimedEvent(stepTick + 8, new byte[] { 0x89, 42, 0 }, 0));
                }
                if (dStr.Contains('O')) // Open Hi-Hat (Note 46)
                {
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x99, 46, 85 }, 1));
                    events.Add(new TimedEvent(stepTick + 20, new byte[] { 0x89, 46, 0 }, 0));
                }
                if (dStr.Contains('C')) // Jing / Crash Cymbal (Note 49)
                {
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x99, 49, 115 }, 1));
                    events.Add(new TimedEvent(stepTick + 48, new byte[] { 0x89, 49, 0 }, 0));
                }
                if (dStr.Contains('T')) // Floor Tom / Buk (Note 41)
                {
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x99, 41, 105 }, 1));
                    events.Add(new TimedEvent(stepTick + 16, new byte[] { 0x89, 41, 0 }, 0));
                }
            }

            // 3. Sort Events strictly by Tick, then Priority (Note-Off before Note-On at same tick)
            events.Sort((a, b) =>
            {
                int c = a.Tick.CompareTo(b.Tick);
                if (c != 0) return c;
                return a.Priority.CompareTo(b.Priority);
            });

            // 4. Build Standard MIDI File Format 0 binary
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // MThd Header
            bw.Write(new char[] { 'M', 'T', 'h', 'd' });
            bw.Write(new byte[] { 0, 0, 0, 6 }); // Header size = 6
            bw.Write(new byte[] { 0, 0 });       // Format 0
            bw.Write(new byte[] { 0, 1 });       // 1 Track
            bw.Write(new byte[] { 0, 96 });      // 96 division (ticks per quarter note)

            // MTrk Track
            using (var tMs = new MemoryStream())
            {
                using (var tbw = new BinaryWriter(tMs))
                {
                    // Tempo Meta Event
                    int usPerBeat = (int)(60000000.0 / bpm);
                    WriteVarLen(tbw, 0);
                    tbw.Write(new byte[]
                    {
                        0xFF, 0x51, 0x03,
                        (byte)((usPerBeat >> 16) & 0xFF),
                        (byte)((usPerBeat >> 8) & 0xFF),
                        (byte)(usPerBeat & 0xFF)
                    });

                    // Time Signature 4/4
                    WriteVarLen(tbw, 0);
                    tbw.Write(new byte[] { 0xFF, 0x58, 0x04, 0x04, 0x02, 0x18, 0x08 });

                    int currentTick = 0;
                    foreach (var ev in events)
                    {
                        int delta = ev.Tick - currentTick;
                        if (delta < 0) delta = 0;
                        WriteVarLen(tbw, delta);
                        tbw.Write(ev.Data);
                        currentTick = ev.Tick;
                    }

                    // End of Track Meta Event
                    WriteVarLen(tbw, 0);
                    tbw.Write(new byte[] { 0xFF, 0x2F, 0x00 });

                    byte[] trackBytes = tMs.ToArray();
                    bw.Write(new char[] { 'M', 'T', 'r', 'k' });
                    byte[] lenBytes = BitConverter.GetBytes(trackBytes.Length);
                    Array.Reverse(lenBytes); // Big Endian
                    bw.Write(lenBytes);
                    bw.Write(trackBytes);
                }
            }

            return ms.ToArray();
        }
    }
}
