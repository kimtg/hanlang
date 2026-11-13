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
            if (string.IsNullOrEmpty(note) || note == "-" || note == "=" || note == "." || note == "~" || note.Equals("r", StringComparison.OrdinalIgnoreCase)) return -1;

            string pitchStr = note;
            int sepIdx = note.IndexOfAny(new[] { ':', '_', '/' });
            if (sepIdx >= 0) pitchStr = note.Substring(0, sepIdx);

            char pitchChar = char.ToUpperInvariant(pitchStr[0]);
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
            if (pitchStr.Length > 1 && (pitchStr[1] == '#' || pitchStr[1] == 's' || pitchStr[1] == 'S'))
            {
                semitone += 1;
                octaveIdx = 2;
            }
            else if (pitchStr.Length > 1 && (pitchStr[1] == 'b' || pitchStr[1] == 'f' || pitchStr[1] == 'F'))
            {
                semitone -= 1;
                octaveIdx = 2;
            }

            if (octaveIdx < pitchStr.Length && int.TryParse(pitchStr.Substring(octaveIdx), out int octave))
            {
                return (octave + 1) * 12 + semitone;
            }
            return -1;
        }

        public static int[] ParseChord(string chord)
        {
            if (string.IsNullOrWhiteSpace(chord)) return Array.Empty<int>();

            chord = chord.Trim();
            char rootChar = char.ToUpperInvariant(chord[0]);
            int rootSemi = rootChar switch
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
            if (rootSemi < 0) return Array.Empty<int>();

            int idx = 1;
            if (idx < chord.Length && (chord[idx] == '#' || chord[idx] == 's' || chord[idx] == 'S'))
            {
                rootSemi = (rootSemi + 1) % 12;
                idx++;
            }
            else if (idx < chord.Length && (chord[idx] == 'b' || chord[idx] == 'f' || chord[idx] == 'F'))
            {
                rootSemi = (rootSemi + 11) % 12;
                idx++;
            }

            // Root in comfortable tenor string pad octave 3 (MIDI 48 ~ 59)
            int rootMidi = 48 + rootSemi;
            string suffix = chord.Substring(idx).ToLowerInvariant();

            int thirdInterval = 4; // default Major
            int fifthInterval = 7; // default Perfect 5th
            int? seventhInterval = null;

            if (suffix == "m" || suffix == "min")
            {
                thirdInterval = 3;
            }
            else if (suffix == "m7" || suffix == "min7")
            {
                thirdInterval = 3;
                seventhInterval = 10;
            }
            else if (suffix == "maj7" || suffix == "m7+")
            {
                thirdInterval = 4;
                seventhInterval = 11;
            }
            else if (suffix == "7" || suffix == "dom7")
            {
                thirdInterval = 4;
                seventhInterval = 10;
            }
            else if (suffix == "dim" || suffix == "dim7")
            {
                thirdInterval = 3;
                fifthInterval = 6;
                seventhInterval = 9;
            }
            else if (suffix == "aug")
            {
                thirdInterval = 4;
                fifthInterval = 8;
            }
            else if (suffix == "sus4")
            {
                thirdInterval = 5;
            }

            int r = rootMidi;
            int t = rootMidi + thirdInterval;
            int f = rootMidi + fifthInterval;

            if (seventhInterval.HasValue)
            {
                int sev = rootMidi + seventhInterval.Value;
                return new int[] { r, t, f, sev };
            }
            return new int[] { r, t, f };
        }

        public static string[] GetTrackChords(string trackName) => trackName.ToLowerInvariant() switch
        {
            "midboss" => new string[]
            {
                // Part A: 1~8 measures
                "Dm", "Bb", "Gm", "A7", "Dm", "C", "Bb", "A7",
                // Part B: 9~16 measures
                "Bb", "C", "Dm", "F", "Gm", "Dm", "C", "A7",
                // Part C: 17~24 measures
                "Dm", "Gm", "C", "F", "Bb", "Gm", "A7", "Dm"
            },
            "finalboss" or "boss" => new string[]
            {
                // Part A: 1~8 measures
                "Cm", "Ab", "Fm", "G7", "Cm", "Eb", "Fm", "G7",
                // Part B: 9~16 measures
                "Ab", "Bb", "Cm", "Eb", "Fm", "Cm", "Ab", "G7",
                // Part C: 17~24 measures
                "Cm", "Fm", "Bb", "Eb", "Ab", "Fm", "G7", "Cm"
            },
            "stage_fortress" => new string[]
            {
                // Part A: 1~8 measures
                "Am", "Em", "F", "G", "Dm", "Em", "F", "E7",
                // Part B: 9~16 measures
                "C", "G", "Am", "Em", "F", "Dm", "Em7", "E7",
                // Part C: 17~24 measures
                "Am", "F", "G", "C", "Dm", "F", "Bm7", "E7"
            },
            "stage_night" => new string[]
            {
                // Part A: 1~8 measures
                "Dm", "Gm", "C", "A7", "Dm", "Bb", "Gm", "A7",
                // Part B: 9~16 measures
                "Bb", "C", "Am7", "Dm", "Gm", "C", "F", "A7",
                // Part C: 17~24 measures
                "Dm", "Bb", "C", "F", "Gm", "Dm", "Bb", "A7"
            },
            "stage_palace" => new string[]
            {
                // Part A: 1~8 measures
                "G", "D", "Em", "Bm", "C", "G", "Am", "D7",
                // Part B: 9~16 measures
                "Em", "Bm", "C", "G", "Am", "Em", "C", "D7",
                // Part C: 17~24 measures
                "G", "D", "Em", "C", "Am7", "Bm7", "D7", "G"
            },
            "title" => new string[]
            {
                // Part A: 1~4 measures
                "C", "G", "Am", "F",
                // Part B: 5~8 measures
                "Dm", "G", "Em", "Am",
                // Part C: 9~12 measures
                "F", "G", "G7", "C"
            },
            "shop" => new string[]
            {
                // Part A: 1~4 measures
                "Am", "Dm", "G", "C",
                // Part B: 5~8 measures
                "F", "Dm", "E7", "Am",
                // Part C: 9~12 measures
                "Dm", "G", "C", "E7"
            },
            "victory" => new string[]
            {
                // Part A: 1~4 measures
                "D", "A", "Bm", "G",
                // Part B: 5~8 measures
                "Em", "A", "F#m", "Bm",
                // Part C: 9~12 measures
                "G", "A", "D", "D"
            },
            _ => new string[] // "stage_market" / "stage"
            {
                // Part A: 1~8 measures
                "Em", "D", "C", "B7", "Em", "G", "Am7", "B7",
                // Part B: 9~16 measures
                "C", "D", "G", "Em", "Am7", "D", "C", "B7",
                // Part C: 17~24 measures
                "Em", "C", "D", "G", "C", "Am7", "B7", "Em"
            }
        };

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
            string[] chords = GetTrackChords(trackName);
            string[] melody;
            string[] bass;
            string[] drums;

            switch (trackName.ToLowerInvariant())
            {
                case "midboss":
                    // 「숙명의 대결」 (172 BPM, D minor 하드록/K-퓨전 배틀 슈레드, 384 스텝 24마디)
                    bpm = 172f;
                    totalSteps = 384;
                    ch0Prog = 29;  // Overdriven Guitar (맹렬한 아케이드 보스 리드 기타)
                    ch1Prog = 81;  // Lead 2 Sawtooth (트윈 리드 신스 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (긴박한 배틀 스트링)
                    ch3Prog = 34;  // Pick Bass (공격적인 피크 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 시그니처 페달 포인트 & 블루스 슈레드 리프)
                        "D5", "D5", "F5", "D5", "A5", "=", "Ab5", "G5", "F5", "D5", "C5", "D5", "F5", "G5", "Ab5", "A5",
                        "Bb5", ".", "F5", "D5", "Bb5", "=", "A5", "G5", "F5", "D5", "C5", "D5", "F5", "=", "D5", ".",
                        "G5", "G5", "Bb5", "G5", "D6", "=", "C6", "Bb5", "A5", "G5", "F5", "E5", "D5", "=", "E5", "F5",
                        "E5", "A5", "C#6", "E6", "D6", "C#6", "Bb5", "A5", "G5", "F5", "E5", "C#5", "A4", "=", ".", ".",
                        "D5", ".", "A5", ".", "G5", "F5", "E5", "D5", "F5", "=", "Ab5", "A5", "C6", "=", "Bb5", "A5",
                        "G5", "=", "E5", "C5", "G5", "A5", "Bb5", "C6", "B5", "A5", "G5", "E5", "C5", "=", ".", ".",
                        "Bb4", "D5", "F5", "Bb5", "A5", "G5", "F5", "D5", "G5", "=", "F5", "E5", "D5", "E5", "F5", "G5",
                        "A5", "=", "G5", "F5", "E5", "=", "C#5", "E5", "A5", "=", "C#6", "=", "E6", "=", ".", ".",

                        // Part B (Measures 9~16 - 비장미가 폭발하는 질풍 멜로디 코러스)
                        "Bb5", "=", "=", "A5", "F5", "=", "D5", "F5", "Bb5", "=", "A5", "G5", "F5", "=", "D5", ".",
                        "C6", "=", "=", "Bb5", "G5", "=", "E5", "G5", "C6", "=", "B5", "A5", "G5", "=", "E5", ".",
                        "D6", "=", "A5", "F5", "D5", "F5", "A5", "D6", "C6", "=", "Bb5", "A5", "G5", "=", "F5", "E5",
                        "F5", "=", "C6", "A5", "F5", "=", "C5", "F5", "A5", "=", "Bb5", "C6", "A5", "=", ".", ".",
                        "G5", ".", "Bb5", "D6", "F6", "=", "E6", "D6", "C6", "Bb5", "A5", "G5", "F5", "=", "D5", ".",
                        "A5", ".", "F5", "D5", "A5", "=", "Bb5", "C6", "D6", "=", "A5", "F5", "D5", "=", ".", ".",
                        "E5", "G5", "C6", "E6", "D6", "C6", "B5", "A5", "G5", "E5", "C5", "E5", "G5", "=", "E5", ".",
                        "A5", "=", "C#6", "E6", "A5", "=", "Bb5", "A5", "G5", "F5", "E5", "C#5", "A4", "=", "C#5", "E5",

                        // Part C (Measures 17~24 - 최종 스퍼트 네오클래시컬 아케이드 솔로)
                        "D5", "E5", "F5", "G5", "A5", "Bb5", "C6", "D6", "C6", "Bb5", "A5", "G5", "F5", "E5", "F5", "A5",
                        "G5", ".", "D6", ".", "Bb5", "A5", "G5", "F5", "G5", "A5", "Bb5", "C6", "D6", "=", "Bb5", ".",
                        "C6", ".", "G6", ".", "E6", "D6", "C6", "Bb5", "A5", "G5", "F5", "E5", "G5", "=", "E5", ".",
                        "F5", "A5", "C6", "F6", "E6", "D6", "C6", "Bb5", "A5", "G5", "F5", "E5", "F5", "=", "G5", "A5",
                        "Bb5", "=", "F5", "D5", "Bb5", "C6", "D6", "F6", "E6", "D6", "C6", "Bb5", "A5", "=", "G5", ".",
                        "G5", "Bb5", "D6", "F6", "E6", "D6", "C6", "Bb5", "A5", "G5", "Ab5", "A5", "Bb5", "=", "G5", ".",
                        "A5", "C#6", "E6", "G6", "F6", "E6", "D6", "C#6", "Bb5", "A5", "G5", "F5", "E5", "C#5", "A4", ".",
                        "D5", "=", "F5", "A5", "D6", "=", "=", "=", "D5", "=", ".", ".", "D5", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8) - 8비트/16비트 드라이빙 피크 베이스
                        "D2", "D2", "D3", "D2", "D2", "D2", "F2", "D2", "D2", "D2", "C2", "C2", "D2", "D2", "F2", "G2",
                        "Bb1", "Bb1", "Bb2", "Bb1", "Bb1", "Bb1", "D2", "F2", "Bb1", "Bb1", "F2", "Bb1", "C2", "D2", "F2", "D2",
                        "G1", "G1", "G2", "G1", "G1", "G1", "Bb1", "D2", "G1", "G1", "D2", "G1", "Bb1", "D2", "F2", "G2",
                        "A1", "A1", "A2", "A1", "E2", "E2", "G2", "E2", "A1", "A1", "C#2", "E2", "A2", "G2", "E2", "C#2",
                        "D2", "D2", "D3", "D2", "D2", "D2", "A2", "D2", "D2", "D2", "F2", "D2", "A2", "G2", "F2", "E2",
                        "C2", "C2", "C3", "C2", "C2", "C2", "G2", "C2", "C2", "C2", "E2", "G2", "C3", "Bb2", "G2", "E2",
                        "Bb1", "Bb1", "Bb2", "Bb1", "Bb1", "Bb1", "F2", "Bb1", "G1", "G1", "G2", "G1", "Bb1", "C2", "D2", "G2",
                        "A1", "A1", "A2", "A1", "C#2", "C#2", "E2", "E2", "A2", "A2", "G2", "E2", "C#2", "A1", "A1", "-",

                        // Part B (9~16)
                        "Bb1", "Bb1", "F2", "Bb1", "Bb1", "Bb1", "D2", "F2", "Bb1", "Bb1", "Bb2", "Bb1", "A2", "F2", "D2", "Bb1",
                        "C2", "C2", "G2", "C2", "C2", "C2", "E2", "G2", "C2", "C2", "C3", "C2", "Bb2", "G2", "E2", "C2",
                        "D2", "D2", "A2", "D2", "D2", "D2", "F2", "A2", "D2", "D2", "D3", "D2", "C3", "A2", "F2", "D2",
                        "F2", "F2", "C3", "F2", "F2", "F2", "A2", "C3", "F2", "F2", "F3", "F2", "E3", "C3", "A2", "F2",
                        "G2", "G2", "D3", "G2", "G2", "G2", "Bb2", "D3", "G2", "G2", "G3", "G2", "F3", "D3", "Bb2", "G2",
                        "D2", "D2", "A2", "D2", "D2", "D2", "F2", "A2", "D2", "D2", "D3", "D2", "C3", "A2", "F2", "D2",
                        "C2", "C2", "G2", "C2", "C2", "C2", "E2", "G2", "C2", "C2", "C3", "C2", "G2", "E2", "C2", "C2",
                        "A1", "A1", "C#2", "C#2", "E2", "E2", "G2", "G2", "A2", "A2", "G2", "E2", "C#2", "A1", "A1", "-",

                        // Part C (17~24)
                        "D2", "D2", "D3", "D2", "F2", "F2", "A2", "A2", "D3", "D3", "C3", "C3", "Bb2", "Bb2", "A2", "A2",
                        "G2", "G2", "G3", "G2", "Bb2", "Bb2", "D3", "D3", "G3", "G3", "F3", "F3", "Eb3", "Eb3", "D3", "D3",
                        "C2", "C2", "C3", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "Bb2", "Bb2", "A2", "A2", "G2", "G2",
                        "F2", "F2", "F3", "F2", "A2", "A2", "C3", "C3", "F3", "F3", "E3", "E3", "D3", "D3", "C3", "C3",
                        "Bb1", "Bb1", "Bb2", "Bb1", "D2", "D2", "F2", "F2", "Bb2", "Bb2", "A2", "A2", "G2", "G2", "F2", "F2",
                        "G1", "G1", "G2", "G1", "Bb1", "Bb1", "D2", "D2", "G2", "G2", "F2", "F2", "D2", "D2", "Bb1", "Bb1",
                        "A1", "A1", "C#2", "C#2", "E2", "E2", "G2", "G2", "A2", "A2", "G2", "G2", "E2", "E2", "C#2", "A1",
                        "D2", "-", "D2", "D2", "F2", "-", "A2", "-", "D3", "-", "A2", "-", "D2", "-", "D2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~8)
                        "C", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "C", "C",

                        // Part B (9~16)
                        "C", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", ".", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", ".", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C",

                        // Part C (17~24)
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "T", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C"
                    };
                    break;

                case "finalboss":
                case "boss":
                    // 「마도학사 흑호의 강림」 (184 BPM, C minor 고딕 심포닉 메탈, 384 스텝 24마디)
                    bpm = 184f;
                    totalSteps = 384;
                    ch0Prog = 81;  // Lead 2 Sawtooth (예리한 네오클래시컬 최종보스 리드)
                    ch1Prog = 19;  // Church Organ (어둠의 파이프오르간 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (장엄한 고딕 오케스트라)
                    ch3Prog = 38;  // Synth Bass 1 (묵직한 신스 배틀 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 고딕 오르간 모티프 + 바로크 16분음 질주 + Gb 블루노트)
                        "C5", "=", "G5", "Gb5", "F5", "=", "Eb5", "D5", "C5", "D5", "Eb5", "F5", "G5", "=", "B4", ".",
                        "Ab5", ".", "Eb5", "C5", "Ab5", "=", "G5", "F5", "Eb5", "D5", "C5", "Eb5", "G5", "=", "Eb5", ".",
                        "F5", "G5", "Ab5", "Bb5", "C6", "=", "Bb5", "Ab5", "G5", "F5", "Eb5", "D5", "C5", "=", "D5", "Eb5",
                        "D5", "G5", "B5", "D6", "C6", "B5", "Ab5", "G5", "F5", "Eb5", "D5", "B4", "G4", "=", ".", ".",
                        "C5", "Eb5", "G5", "C6", "Gb5", "=", "F5", "Eb5", "D5", "Eb5", "F5", "Gb5", "G5", "=", "C6", ".",
                        "Eb6", ".", "Bb5", "G5", "Eb5", "G5", "Bb5", "Eb6", "D6", "C6", "Bb5", "Ab5", "G5", "=", ".", ".",
                        "F5", "Ab5", "C6", "F6", "Eb6", "D6", "C6", "Bb5", "Ab5", "G5", "F5", "Eb5", "D5", "=", "F5", ".",
                        "G5", "=", "B5", "D6", "F6", "=", "Eb6", "D6", "C6", "B5", "Ab5", "G5", "D5", "=", "B4", ".",

                        // Part B (Measures 9~16 - 웅장한 합창 & 운명의 대결 코러스)
                        "Ab5", "=", "=", "G5", "Eb5", "=", "C5", "Eb5", "Ab5", "=", "G5", "F5", "Eb5", "=", "C5", ".",
                        "Bb5", "=", "=", "Ab5", "F5", "=", "D5", "F5", "Bb5", "=", "A5", "G5", "F5", "=", "D5", ".",
                        "C6", "=", "G5", "Eb5", "C5", "Eb5", "G5", "C6", "Bb5", "=", "Ab5", "G5", "F5", "=", "Eb5", "D5",
                        "Eb5", "=", "Bb5", "G5", "Eb5", "=", "Bb4", "Eb5", "G5", "=", "Ab5", "Bb5", "G5", "=", ".", ".",
                        "F5", ".", "Ab5", "C6", "Eb6", "=", "D6", "C6", "Bb5", "Ab5", "G5", "F5", "Eb5", "=", "C5", ".",
                        "C6", "=", "G5", "Eb5", "C5", "D5", "Eb5", "G5", "C6", "=", "Bb5", "Ab5", "G5", "=", "Eb5", ".",
                        "Ab5", ".", "Eb5", "C5", "Ab5", "=", "Bb5", "C6", "B5", "Ab5", "G5", "F5", "Eb5", "=", "D5", ".",
                        "G5", "B5", "D6", "G6", "F6", "Eb6", "D6", "C6", "B5", "Ab5", "G5", "F5", "Eb5", "D5", "C5", "B4",

                        // Part C (Measures 17~24 - 흑호 마도 총력전 클라이맥스)
                        "C5", "D5", "Eb5", "F5", "G5", "Ab5", "B5", "C6", "G5", "C6", "Eb6", "C6", "G6", "=", "Eb6", ".",
                        "F5", "Ab5", "C6", "F6", "Eb6", "D6", "C6", "Bb5", "Ab5", "G5", "F5", "Eb5", "D5", "=", "F5", ".",
                        "Bb4", "D5", "F5", "Bb5", "Ab5", "G5", "F5", "Eb5", "D5", "F5", "Bb5", "D6", "C6", "=", "Bb5", ".",
                        "Eb5", "G5", "Bb5", "Eb6", "D6", "C6", "Bb5", "Ab5", "G5", "F5", "Eb5", "D5", "Eb5", "=", "F5", "G5",
                        "Ab5", "=", "Eb5", "C5", "Ab5", "Bb5", "C6", "Eb6", "D6", "C6", "Bb5", "Ab5", "G5", "=", "F5", ".",
                        "F5", "Ab5", "C6", "F6", "Eb6", "D6", "C6", "B5", "Ab5", "G5", "Gb5", "G5", "Ab5", "=", "F5", ".",
                        "G5", "B5", "D6", "G6", "F6", "Eb6", "D6", "C6", "B5", "Ab5", "G5", "F5", "Eb5", "D5", "B4", ".",
                        "C5", "=", "Eb5", "G5", "C6", "=", "=", "=", "C5", "=", ".", ".", "C5", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "C2", "C2", "C3", "C2", "C2", "C2", "Eb2", "G2", "C2", "C2", "Bb1", "Bb1", "C2", "C2", "G2", "C2",
                        "Ab1", "Ab1", "Ab2", "Ab1", "Ab1", "Ab1", "C2", "Eb2", "Ab1", "Ab1", "Eb2", "Ab1", "Bb1", "C2", "Eb2", "C2",
                        "F1", "F1", "F2", "F1", "F1", "F1", "Ab1", "C2", "F1", "F1", "C2", "F1", "Ab1", "Bb1", "C2", "F2",
                        "G1", "G1", "G2", "G1", "B1", "B1", "D2", "D2", "G2", "G2", "F2", "D2", "B1", "G1", "G1", "-",
                        "C2", "C2", "C3", "C2", "C2", "C2", "G2", "C2", "C2", "C2", "Eb2", "C2", "G2", "F2", "Eb2", "D2",
                        "Eb2", "Eb2", "Eb3", "Eb2", "Eb2", "Eb2", "Bb2", "Eb2", "Eb2", "Eb2", "G2", "Bb2", "Eb3", "D3", "Bb2", "G2",
                        "F2", "F2", "F3", "F2", "F2", "F2", "C3", "F2", "F2", "F2", "Ab2", "C3", "F3", "Eb3", "C3", "Ab2",
                        "G2", "G2", "G3", "G2", "B2", "B2", "D3", "D3", "G3", "G3", "F3", "D3", "B2", "G2", "G2", "-",

                        // Part B (9~16)
                        "Ab1", "Ab1", "Eb2", "Ab1", "Ab1", "Ab1", "C2", "Eb2", "Ab1", "Ab1", "Ab2", "Ab1", "G2", "Eb2", "C2", "Ab1",
                        "Bb1", "Bb1", "F2", "Bb1", "Bb1", "Bb1", "D2", "F2", "Bb1", "Bb1", "Bb2", "Bb1", "Ab2", "F2", "D2", "Bb1",
                        "C2", "C2", "G2", "C2", "C2", "C2", "Eb2", "G2", "C2", "C2", "C3", "C2", "Bb2", "G2", "Eb2", "C2",
                        "Eb2", "Eb2", "Bb2", "Eb2", "Eb2", "Eb2", "G2", "Bb2", "Eb2", "Eb2", "Eb3", "Eb2", "D3", "Bb2", "G2", "Eb2",
                        "F2", "F2", "C3", "F2", "F2", "F2", "Ab2", "C3", "F2", "F2", "F3", "F2", "Eb3", "C3", "Ab2", "F2",
                        "C2", "C2", "G2", "C2", "C2", "C2", "Eb2", "G2", "C2", "C2", "C3", "C2", "Bb2", "G2", "Eb2", "C2",
                        "Ab1", "Ab1", "Eb2", "Ab1", "Ab1", "Ab1", "C2", "Eb2", "Ab1", "Ab1", "Ab2", "Ab1", "Eb2", "C2", "Ab1", "Ab1",
                        "G1", "G1", "B1", "B1", "D2", "D2", "F2", "F2", "G2", "G2", "F2", "D2", "B1", "G1", "G1", "-",

                        // Part C (17~24)
                        "C2", "C2", "C3", "C2", "Eb2", "Eb2", "G2", "G2", "C3", "C3", "Bb2", "Bb2", "Ab2", "Ab2", "G2", "G2",
                        "F2", "F2", "F3", "F2", "Ab2", "Ab2", "C3", "C3", "F3", "F3", "Eb3", "Eb3", "Db3", "Db3", "C3", "C3",
                        "Bb1", "Bb1", "Bb2", "Bb1", "D2", "D2", "F2", "F2", "Bb2", "Bb2", "Ab2", "Ab2", "G2", "G2", "F2", "F2",
                        "Eb2", "Eb2", "Eb3", "Eb2", "G2", "G2", "Bb2", "Bb2", "Eb3", "Eb3", "D3", "D3", "C3", "C3", "Bb2", "Bb2",
                        "Ab1", "Ab1", "Ab2", "Ab1", "C2", "C2", "Eb2", "Eb2", "Ab2", "Ab2", "G2", "G2", "F2", "F2", "Eb2", "Eb2",
                        "F1", "F1", "F2", "F1", "Ab1", "Ab1", "C2", "C2", "F2", "F2", "Eb2", "Eb2", "C2", "C2", "Ab1", "Ab1",
                        "G1", "G1", "B1", "B1", "D2", "D2", "F2", "F2", "G2", "G2", "F2", "F2", "D2", "D2", "B1", "G1",
                        "C2", "-", "C2", "C2", "Eb2", "-", "G2", "-", "C3", "-", "G2", "-", "C2", "-", "C2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~8)
                        "C", "T", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "T", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "C",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "T", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "C",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C",

                        // Part B (9~16)
                        "C", "K", "T", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "C",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", ".", "H", "H", "S", "H", "S", "C",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C",

                        // Part C (17~24)
                        "C", "K", "T", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "C",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "T", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "C",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C"
                    };
                    break;

                case "stage_fortress":
                    // 「남산 성곽의 황혼」 (132 BPM, A minor 비장하고 드라마틱한 검무 발라드록, 384 스텝 24마디)
                    bpm = 132f;
                    totalSteps = 384;
                    ch0Prog = 40;  // Violin (비장한 해금풍 바이올린 주선율)
                    ch1Prog = 77;  // Shakuhachi (성곽 바람을 가르는 대금/단소 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 현악 합주)
                    ch3Prog = 32;  // Acoustic Bass
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 가슴 울리는 도약 5도 모티프 & 블루스 슬라이드)
                        "A4", "=", "=", "E5", "F5", "=", "E5", "D5", "C5", "=", "B4", "C5", "A4", "=", ".", ".",
                        "E5", ".", "G5", "B5", "A5", "=", "G5", "E5", "D5", "=", "E5", "G5", "E5", "=", ".", ".",
                        "F5", "=", "=", "C6", "B5", "=", "A5", "G5", "F5", "=", "E5", "D5", "C5", "=", "A4", ".",
                        "B4", "D5", "G5", "=", "B5", "=", "A5", "G5", "D6", "=", "B5", "G5", "B5", "=", ".", ".",
                        "D5", ".", "F5", "A5", "G5", "=", "F5", "E5", "D5", "=", "Eb5", "E5", "F5", "=", "A5", ".",
                        "E5", ".", "G5", "B5", "E6", "=", "D6", "B5", "G5", "=", "B5", "A5", "G5", "=", "E5", ".",
                        "F5", "=", "A5", "C6", "F6", "=", "E6", "D6", "C6", "=", "A5", "F5", "E5", "=", "D5", ".",
                        "E5", "=", "G#5", "B5", "E6", "=", "D6", "B5", "G#5", "=", "B5", "=", "E5", "=", ".", ".",

                        // Part B (Measures 9~16 - 장엄하게 솟아오르는 영웅의 서사시)
                        "C5", "=", "G5", "=", "E5", "=", "=", "D5", "C5", "D5", "E5", "G5", "E5", "=", ".", ".",
                        "B4", "=", "G5", "=", "D5", "=", "=", "C5", "B4", "C5", "D5", "F5", "D5", "=", ".", ".",
                        "A4", "B4", "C5", "E5", "A5", "=", "=", "G5", "E5", "=", "C5", "E5", "A5", "=", ".", ".",
                        "G5", "=", "E5", "B4", "G5", "=", "F#5", "E5", "B4", "=", "E5", "G5", "B5", "=", "=", ".",
                        "F5", "=", "A5", "C6", "B5", "=", "A5", "G5", "F5", "=", "C5", "F5", "A5", "=", ".", ".",
                        "D5", "=", "F5", "A5", "G5", "=", "F5", "E5", "D5", "Eb5", "E5", "F5", "A5", "=", ".", ".",
                        "G5", "F5", "E5", "D5", "C5", "B4", "A4", "G4", "E5", "=", "G5", "=", "B5", "=", "=", ".",
                        "G#5", "=", "E5", "B4", "G#5", "=", "B5", "E6", "B5", "=", "=", ".", "E5", "=", ".", ".",

                        // Part C (Measures 17~24 - 절정의 검무 협주곡 클라이맥스)
                        "A5", ".", "A5", "C6", "E6", "=", "D6", "C6", "B5", "C6", "B5", "A5", "E5", "=", "A5", ".",
                        "F5", ".", "F5", "A5", "C6", "=", "B5", "A5", "G5", "A5", "G5", "F5", "C5", "=", "F5", ".",
                        "G5", ".", "G5", "B5", "D6", "=", "C6", "B5", "A5", "B5", "A5", "G5", "D5", "=", "G5", ".",
                        "C6", "=", "G5", "E5", "C6", "=", "E6", "G6", "E6", "=", "C6", "G5", "E5", "=", ".", ".",
                        "D5", "F5", "A5", "C6", "Bb5", "A5", "G5", "F5", "E5", "D5", "C5", "Bb4", "A4", "=", "D5", ".",
                        "F5", ".", "A5", "C6", "F6", "=", "E6", "D6", "C6", "=", "A5", "F5", "C5", "=", "A4", ".",
                        "B4", "D5", "F#5", "A5", "G#5", "F#5", "E5", "D#5", "E5", "=", "B5", "=", "G#5", "=", "E5", ".",
                        "A5", "=", "E5", "C5", "A4", "=", "=", ".", "A5", "=", "=", "=", "A4", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "D3", "-", "D3", "D3", "E3", "-", "D3", "C3",
                        "A2", "-", "A2", "A2", "E3", "-", "E3", "E3", "D3", "-", "D3", "D3", "C3", "-", "D3", "E3",
                        "A2", "-", "C3", "-", "D3", "-", "E3", "-", "G3", "-", "E3", "-", "D3", "-", "C3", "-",
                        "D3", "-", "C3", "-", "A2", "-", "G2", "-", "A2", "-", "C3", "-", "A2", "-", "A2", "-",
                        "A2", "A2", "C3", "C3", "D3", "D3", "E3", "E3", "G3", "G3", "E3", "E3", "D3", "D3", "C3", "C3",
                        "E3", "E3", "G3", "G3", "A3", "A3", "G3", "G3", "E3", "E3", "D3", "D3", "C3", "C3", "D3", "D3",
                        "C3", "C3", "A2", "A2", "G2", "G2", "E2", "E2", "G2", "G2", "A2", "A2", "C3", "C3", "D3", "D3",
                        "A2", "-", "C3", "-", "D3", "-", "E3", "-", "A2", "-", "E3", "-", "A2", "-", "A2", "-",

                        // Part B (9~16)
                        "C2", "-", "C2", "C2", "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "C3", "-", "B2", "G2",
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "G3", "-", "F3", "D3",
                        "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "E3", "-", "E3", "E3", "A3", "-", "G3", "E3",
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "E3", "-", "D3", "B2",
                        "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "F3", "-", "E3", "C3",
                        "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "D3", "-", "C3", "A2",
                        "E2", "E2", "G2", "G2", "B2", "B2", "D3", "D3", "E3", "E3", "D3", "D3", "B2", "B2", "G2", "G2",
                        "E2", "E2", "G#2", "G#2", "B2", "B2", "E3", "E3", "E2", "E2", "B2", "B2", "E2", "-", "E2", "-",

                        // Part C (17~24)
                        "A2", "A2", "C3", "C3", "D3", "D3", "E3", "E3", "A3", "A3", "G3", "G3", "F3", "F3", "E3", "E3",
                        "F2", "F2", "A2", "A2", "C3", "C3", "F3", "F3", "E3", "E3", "D3", "D3", "C3", "C3", "A2", "A2",
                        "G2", "G2", "B2", "B2", "D3", "D3", "G3", "G3", "F3", "F3", "E3", "E3", "D3", "D3", "B2", "B2",
                        "C2", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "B2", "B2", "A2", "A2", "G2", "G2", "E2", "E2",
                        "D2", "D2", "F2", "F2", "A2", "A2", "D3", "D3", "C3", "C3", "B2", "B2", "A2", "A2", "F2", "F2",
                        "A2", "A2", "C3", "C3", "E3", "E3", "A3", "A3", "G3", "G3", "F3", "F3", "E3", "E3", "C3", "C3",
                        "F2", "F2", "A2", "A2", "D3", "D3", "F3", "F3", "E3", "E3", "D3", "D3", "C3", "C3", "B2", "B2",
                        "A2", "-", "C3", "-", "E3", "-", "A3", "-", "A2", "-", "E3", "-", "A2", "-", "A2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~8)
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", "H", "S", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", "H", "S", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", "H", "K", "K", "S", "S", "C", "C", "C", "-",

                        // Part B (9~16)
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", "H", "S", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", "H", "S", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "H", "S", "S", "H", "C",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "C", "C", "C", "-",

                        // Part C (17~24)
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
                    // 「달빛 아래 육조거리」 (142 BPM, D minor 심야 시티팝·재즈 퓨전 활극, 384 스텝 24마디)
                    bpm = 142f;
                    totalSteps = 384;
                    ch0Prog = 26;  // Electric Guitar Jazz (스타일리시한 심야 네오재즈 리드)
                    ch1Prog = 73;  // Flute (대금 선율 응답)
                    ch2Prog = 45;  // Pizzicato Strings (긴장감 있는 피치카토)
                    ch3Prog = 33;  // Finger Bass (그루비한 워킹 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 시크하고 그루비한 16비트 오프비트 재즈 리프 + Ab 블루노트)
                        "D5", ".", "F5", "A5", ".", "C6", "Bb5", "A5", ".", "Ab5", "G5", "F5", "D5", "=", ".", ".",
                        "G5", ".", "Bb5", "D6", ".", "C6", "Bb5", "G5", "A5", "=", "F5", "D5", "C5", "=", ".", ".",
                        "E5", ".", "G5", "Bb5", "C6", "=", "Bb5", "G5", "E5", "G5", "Bb5", "C6", "E6", "=", ".", ".",
                        "C#6", "=", "A5", "G5", "E5", "G5", "Ab5", "A5", "D6", "=", "A5", "F5", "D5", "=", ".", ".",
                        "D5", "F5", "A5", ".", "D6", "=", "C6", "A5", "Ab5", "G5", "F5", "D5", "F5", "=", "G5", ".",
                        "Bb4", ".", "D5", "F5", "Bb5", "=", "A5", "G5", "F5", "D5", "F5", "A5", "C6", "=", "Bb5", ".",
                        "G5", ".", "D5", "Bb4", "G5", "A5", "Bb5", "D6", "C6", "=", "Bb5", "A5", "G5", "=", "F5", ".",
                        "E5", "=", "G5", "A5", "C#6", "=", "E6", ".", "D6", "=", "A5", "F5", "D5", "=", ".", ".",

                        // Part B (Measures 9~16 - 세련된 7음 도시 야경 퓨전 코러스)
                        "Bb5", "=", "=", "F5", "D5", "F5", "Bb5", "C6", "D6", "=", "C6", "Bb5", "A5", "=", "G5", ".",
                        "C6", "=", "=", "G5", "E5", "G5", "C6", "D6", "E6", "=", "D6", "C6", "B5", "=", "A5", ".",
                        "A5", ".", "E5", "C5", "E5", "G5", "A5", "C6", "B5", "A5", "G5", "E5", "D5", "=", "C5", ".",
                        "D5", "F5", "A5", "D6", "C6", "=", "Bb5", "A5", "G5", "F5", "E5", "D5", "F5", "=", "A5", ".",
                        "G5", ".", "Bb5", "D6", "F6", "=", "E6", "D6", "C6", "Bb5", "A5", "G5", "D5", "=", "G5", ".",
                        "C6", ".", "G5", "E5", "G5", "A5", "Bb5", "C6", "E6", "=", "D6", "C6", "Bb5", "=", "G5", ".",
                        "F5", "G5", "A5", "C6", "E6", "=", "D6", "C6", "A5", "=", "F5", "C5", "F5", "=", "G5", "A5",
                        "E5", "=", "G5", "A5", "C#6", "=", "E6", "G6", "E6", "=", "C#6", "A5", "E5", "=", ".", ".",

                        // Part C (Measures 17~24 - 달빛 추격전 펑키 클라이맥스)
                        "D5", "F5", "Ab5", "A5", "D6", ".", "C6", "A5", "Ab5", "G5", "F5", "D5", "F5", "=", "D5", ".",
                        "Bb5", ".", "D6", "F6", "D6", "=", "C6", "Bb5", "A5", "G5", "F5", "D5", "Bb4", "=", "D5", ".",
                        "C6", ".", "E6", "G6", "E6", "=", "D6", "C6", "Bb5", "A5", "G5", "E5", "C5", "=", "E5", ".",
                        "F5", "A5", "C6", "F6", "E6", "D6", "C6", "Bb5", "A5", "G5", "F5", "Eb5", "D5", "=", "F5", ".",
                        "G5", "Bb5", "D6", "G6", "F6", "E6", "D6", "C6", "Bb5", "A5", "G5", "F5", "D5", "=", "G5", ".",
                        "A5", "D6", "F6", "A6", "G6", "F6", "E6", "D6", "C6", "Bb5", "A5", "G5", "F5", "=", "D5", ".",
                        "Bb4", "D5", "F5", "Bb5", "A5", "G5", "F5", "E5", "D5", "=", "F5", "=", "Ab5", "=", "A5", ".",
                        "D5", "=", "A5", "F5", "D5", "=", "=", ".", "D6", "=", "=", "=", "D5", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8) - 탄력 있는 재즈 펑크 옥타브 핑거 베이스
                        "D2", "-", "D3", "D2", "-", "D2", "F2", "A2", "D2", "-", "C3", "D2", "Ab2", "A2", "F2", "D2",
                        "G1", "-", "G2", "G1", "-", "G1", "Bb1", "D2", "G2", "-", "F2", "G1", "D2", "F2", "G2", "Bb2",
                        "C2", "-", "C3", "C2", "-", "C2", "E2", "G2", "C3", "-", "Bb2", "C2", "G2", "E2", "C2", "Bb1",
                        "A1", "-", "A2", "A1", "C#2", "E2", "G2", "A2", "G2", "F2", "E2", "C#2", "A1", "-", "A1", "-",
                        "D2", "-", "D3", "D2", "-", "F2", "A2", "C3", "D3", "-", "C3", "A2", "F2", "D2", "C2", "D2",
                        "Bb1", "-", "Bb2", "Bb1", "-", "Bb1", "D2", "F2", "Bb2", "-", "A2", "F2", "D2", "C2", "Bb1", "A1",
                        "G1", "-", "G2", "G1", "-", "Bb1", "D2", "F2", "G2", "-", "F2", "D2", "Bb1", "G1", "Bb1", "D2",
                        "A1", "A2", "G2", "E2", "C#2", "E2", "G2", "A2", "D2", "-", "A2", "-", "D2", "-", "D2", "-",

                        // Part B (9~16)
                        "Bb1", "-", "F2", "Bb1", "D2", "-", "F2", "Bb2", "Bb1", "-", "F2", "Bb1", "A2", "F2", "D2", "Bb1",
                        "C2", "-", "G2", "C2", "E2", "-", "G2", "C3", "C2", "-", "G2", "C2", "Bb2", "G2", "E2", "C2",
                        "A1", "-", "E2", "A1", "C2", "-", "E2", "G2", "A2", "-", "G2", "E2", "C2", "A1", "E2", "A1",
                        "D2", "-", "A2", "D2", "F2", "-", "A2", "C3", "D3", "-", "C3", "A2", "F2", "D2", "C2", "D2",
                        "G1", "-", "D2", "G1", "Bb1", "-", "D2", "F2", "G2", "-", "F2", "D2", "Bb1", "G1", "A1", "Bb1",
                        "C2", "-", "G2", "C2", "E2", "-", "G2", "Bb2", "C3", "-", "Bb2", "G2", "E2", "C2", "E2", "G2",
                        "F2", "-", "C3", "F2", "A2", "-", "C3", "Eb3", "F3", "-", "Eb3", "C3", "A2", "F2", "C2", "F2",
                        "A1", "C#2", "E2", "G2", "A2", "G2", "E2", "C#2", "A1", "-", "E2", "-", "A1", "-", "A1", "-",

                        // Part C (17~24)
                        "D2", "D2", "D3", "D2", "F2", "F2", "A2", "A2", "D3", "D3", "C3", "C3", "A2", "A2", "F2", "F2",
                        "Bb1", "Bb1", "Bb2", "Bb1", "D2", "D2", "F2", "F2", "Bb2", "Bb2", "A2", "A2", "F2", "F2", "D2", "D2",
                        "C2", "C2", "C3", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "Bb2", "Bb2", "G2", "G2", "E2", "E2",
                        "F2", "F2", "F3", "F2", "A2", "A2", "C3", "C3", "F3", "F3", "Eb3", "Eb3", "C3", "C3", "A2", "A2",
                        "G2", "G2", "G3", "G2", "Bb2", "Bb2", "D3", "D3", "G3", "G3", "F3", "F3", "D3", "D3", "Bb2", "Bb2",
                        "D2", "D2", "D3", "D2", "F2", "F2", "A2", "A2", "D3", "D3", "C3", "C3", "A2", "A2", "F2", "F2",
                        "Bb1", "Bb1", "D2", "D2", "F2", "F2", "Ab2", "Ab2", "Bb2", "Bb2", "A2", "A2", "G2", "G2", "F2", "F2",
                        "A1", "-", "C#2", "-", "E2", "-", "A2", "-", "D2", "-", "A2", "-", "D2", "-", "D2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~8)
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", "H", "S", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "C", "C",

                        // Part B (9~16)
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "H", "C",
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", "H", "S", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C",

                        // Part C (17~24)
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
                    // 「궁궐의 폭풍전야」 (158 BPM, G major 위풍당당한 황궁 대취타 행진곡, 384 스텝 24마디)
                    bpm = 158f;
                    totalSteps = 384;
                    ch0Prog = 56;  // Trumpet (장엄하고 우렁찬 황궁 팡파르 트럼펫)
                    ch1Prog = 73;  // Flute (대금 화려한 고음 화음)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 궁중 관현악)
                    ch3Prog = 32;  // Acoustic Bass (위엄 있는 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 당당한 대취타 팡파르 훅 & 영웅적 행진 테마)
                        "D5", ".", "G5", "G5", "A5", "=", "B5", ".", "D6", "=", "B5", "G5", "A5", "=", ".", ".",
                        "A5", ".", "F#5", "D5", "A5", "=", "B5", "C6", "B5", "=", "A5", "F#5", "D5", "=", ".", ".",
                        "E5", ".", "G5", "B5", "E6", "=", "D6", "B5", "C6", "=", "B5", "A5", "G5", "=", "F#5", ".",
                        "B5", ".", "F#5", "D5", "F#5", "=", "B5", ".", "A5", "G5", "F#5", "E5", "D5", "=", ".", ".",
                        "C5", "E5", "G5", "C6", "B5", "=", "A5", "G5", "E5", "=", "C5", "E5", "G5", "=", ".", ".",
                        "D5", "G5", "B5", "D6", "B5", "=", "G5", "D5", "G5", "A5", "B5", "D6", "G6", "=", ".", ".",
                        "E6", ".", "C6", "A5", "E5", "A5", "C6", "E6", "D6", "C6", "B5", "A5", "G5", "=", "E5", ".",
                        "D5", "F#5", "A5", "D6", "C6", "=", "B5", "A5", "F#5", "=", "A5", "C6", "D6", "=", ".", ".",

                        // Part B (Measures 9~16 - 결전의 전야, 긴장과 낭만이 교차하는 서사 코러스)
                        "E5", ".", "B5", "B5", "G5", "=", "E5", "G5", "B5", "A5", "G5", "F#5", "E5", "=", ".", ".",
                        "D5", ".", "B5", "B5", "F#5", "=", "D5", "F#5", "B5", "A5", "G5", "F#5", "D5", "=", ".", ".",
                        "C5", "E5", "G5", "C6", "E6", "=", "D6", "C6", "B5", "=", "G5", "E5", "C5", "=", "E5", ".",
                        "G5", "B5", "D6", "G6", "F#6", "=", "E6", "D6", "B5", "=", "G5", "D5", "B4", "=", ".", ".",
                        "A4", "C5", "E5", "A5", "G5", "F#5", "E5", "D5", "C5", "E5", "A5", "C6", "B5", "=", "A5", ".",
                        "B4", "E5", "G5", "B5", "E6", "=", "D6", "B5", "G5", "E5", "G5", "B5", "E6", "=", ".", ".",
                        "C6", ".", "G5", "E5", "C6", "B5", "A5", "G5", "E5", "G5", "C6", "E6", "D6", "=", "C6", ".",
                        "D6", "=", "A5", "F#5", "D5", "F#5", "A5", "D6", "C6", "B5", "A5", "F#5", "D5", "=", "F#5", "A5",

                        // Part C (Measures 17~24 - 역모 토벌 대취타 총진격 피날레)
                        "G5", ".", "D6", "D6", "B5", "=", "G5", "B5", "D6", "=", "C6", "B5", "A5", "=", "G5", ".",
                        "F#5", ".", "D6", "D6", "A5", "=", "F#5", "A5", "D6", "=", "C#6", "B5", "A5", "=", ".", ".",
                        "E5", "G5", "B5", "E6", "G6", "=", "F#6", "E6", "D6", "C6", "B5", "A5", "G5", "=", "E5", ".",
                        "C6", "G5", "E5", "C5", "E5", "G5", "C6", "E6", "G6", "=", "E6", "C6", "G5", "=", ".", ".",
                        "A5", ".", "C6", "E6", "G6", "=", "F#6", "E6", "D6", "C6", "B5", "A5", "G5", "=", "E5", ".",
                        "B5", ".", "F#5", "D#5", "B5", "C6", "D6", "F#6", "E6", "D6", "C6", "B5", "F#5", "=", ".", ".",
                        "D5", "F#5", "A5", "C6", "D6", "=", "E6", "F#6", "G6", "=", "F#6", "E6", "D6", "C6", "B5", "A5",
                        "G5", "=", "B5", "D6", "G6", "=", "=", "=", "G5", "=", ".", ".", "G5", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "G2", "-", "G3", "G2", "D3", "-", "B2", "G2", "G2", "G2", "A2", "B2", "D3", "B2", "A2", "G2",
                        "D2", "-", "D3", "D2", "A2", "-", "F#2", "D2", "D2", "D2", "E2", "F#2", "A2", "F#2", "E2", "D2",
                        "E2", "-", "E3", "E2", "B2", "-", "G2", "E2", "E2", "E2", "F#2", "G2", "B2", "G2", "F#2", "E2",
                        "B1", "-", "B2", "B1", "F#2", "-", "D2", "B1", "B1", "C#2", "D2", "E2", "F#2", "E2", "D2", "B1",
                        "C2", "C2", "G2", "C2", "E2", "E2", "G2", "C3", "C3", "B2", "A2", "G2", "E2", "D2", "C2", "B1",
                        "G2", "G2", "D3", "G2", "B2", "B2", "D3", "G3", "G3", "F#3", "E3", "D3", "B2", "A2", "G2", "F#2",
                        "A1", "A1", "E2", "A1", "C2", "C2", "E2", "A2", "A2", "G2", "F#2", "E2", "C2", "B1", "A1", "G1",
                        "D2", "D2", "F#2", "A2", "D3", "C3", "B2", "A2", "F#2", "D2", "E2", "F#2", "A2", "F#2", "D2", "-",

                        // Part B (9~16)
                        "E2", "-", "E3", "E2", "G2", "A2", "B2", "E3", "E3", "D3", "C3", "B2", "G2", "F#2", "E2", "D2",
                        "B1", "-", "B2", "B1", "D2", "E2", "F#2", "B2", "B2", "A2", "G2", "F#2", "D2", "C2", "B1", "A1",
                        "C2", "-", "C3", "C2", "E2", "G2", "C3", "E3", "C3", "B2", "A2", "G2", "E2", "D2", "C2", "B1",
                        "G2", "-", "G3", "G2", "B2", "D3", "G3", "B3", "G3", "F#3", "E3", "D3", "B2", "A2", "G2", "F#2",
                        "A1", "-", "A2", "A1", "C2", "E2", "A2", "C3", "A2", "G2", "F#2", "E2", "C2", "B1", "A1", "G1",
                        "E2", "-", "E3", "E2", "G2", "B2", "E3", "G3", "E3", "D3", "C3", "B2", "G2", "F#2", "E2", "D2",
                        "C2", "C2", "E2", "G2", "C3", "B2", "A2", "G2", "E2", "C2", "E2", "G2", "C3", "G2", "E2", "C2",
                        "D2", "D2", "F#2", "A2", "D3", "C3", "B2", "A2", "F#2", "D2", "E2", "F#2", "A2", "F#2", "D2", "-",

                        // Part C (17~24)
                        "G2", "G2", "G3", "G2", "B2", "B2", "D3", "D3", "G3", "G3", "F#3", "F#3", "D3", "D3", "B2", "B2",
                        "D2", "D2", "D3", "D2", "F#2", "F#2", "A2", "A2", "D3", "D3", "C3", "C3", "A2", "A2", "F#2", "F#2",
                        "E2", "E2", "E3", "E2", "G2", "G2", "B2", "B2", "E3", "E3", "D3", "D3", "B2", "B2", "G2", "G2",
                        "C2", "C2", "C3", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "B2", "B2", "G2", "G2", "E2", "E2",
                        "A1", "A1", "C2", "C2", "E2", "E2", "G2", "G2", "A2", "A2", "G2", "G2", "E2", "E2", "C2", "C2",
                        "B1", "B1", "D#2", "D#2", "F#2", "F#2", "B2", "B2", "A2", "A2", "F#2", "F#2", "D#2", "D#2", "B1", "B1",
                        "D2", "D2", "F#2", "F#2", "A2", "A2", "C3", "C3", "D3", "D3", "C3", "C3", "B2", "B2", "A2", "A2",
                        "G2", "-", "B2", "-", "D3", "-", "G3", "-", "G2", "-", "D3", "-", "G2", "-", "G2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~8)
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "C", "C",

                        // Part B (9~16)
                        "C", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", ".", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "K", "H", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", "H", "S", "H", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C",

                        // Part C (17~24)
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "S", "C",
                        "C", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "H", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "S", "O",
                        "K", "H", "K", "H", "S", "H", "H", "H", "K", "K", "H", "H", "S", "H", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C"
                    };
                    break;

                case "title":
                    // 「대취타 유생」 (128 BPM, C major 영웅 서사 메인 테마, 192 스텝 12마디)
                    bpm = 128f;
                    totalSteps = 192;
                    ch0Prog = 56;  // Trumpet (영웅의 출사표 팡파르)
                    ch1Prog = 73;  // Flute (대금 상쾌한 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 현악 오케스트라)
                    ch3Prog = 32;  // Acoustic Bass (힘찬 행진 베이스)
                    melody = new string[]
                    {
                        // Part A (1~4 measures - 한 번 들으면 기억나는 영웅 메인 훅)
                        "G4", ".", "C5", "D5", "E5", "=", "G5", ".", "C6", "=", "=", "B5", "G5", "=", "E5", ".",
                        "D5", ".", "G5", "A5", "B5", "=", "G5", ".", "D6", "=", "=", "B5", "G5", "=", ".", ".",
                        "E5", ".", "A5", "B5", "C6", "=", "B5", "A5", "E5", "=", "C5", "E5", "A5", "=", "G5", ".",
                        "F5", "G5", "A5", "B5", "C6", "=", "A5", "F5", "G5", "=", "=", ".", "G4", "A4", "B4", ".",

                        // Part B (5~8 measures - 낭만과 결의의 브리지)
                        "F5", ".", "A5", "C6", "B5", "=", "A5", "G5", "F5", "=", "D5", "F5", "A5", "=", "G5", ".",
                        "D5", "G5", "B5", "D6", "C6", "=", "B5", "A5", "G5", "=", "D5", "G5", "B5", "=", ".", ".",
                        "E5", ".", "B5", "G5", "E5", "G5", "B5", "E6", "D6", "C6", "B5", "A5", "G5", "=", "E5", ".",
                        "A5", "=", "E5", "C5", "E5", "G5", "A5", "C6", "B5", "=", "A5", "G5", "E5", "=", ".", ".",

                        // Part C (9~12 measures - 장엄한 총력 출진 클라이맥스)
                        "F5", ".", "C6", "A5", "F5", "G5", "A5", "B5", "C6", "=", "A5", "F5", "C5", "=", "F5", ".",
                        "G5", ".", "D6", "B5", "G5", "A5", "B5", "C6", "D6", "=", "B5", "G5", "D5", "=", "G5", ".",
                        "B5", "C6", "D6", "E6", "F6", "=", "E6", "D6", "C6", "=", "B5", "A5", "G5", "=", "B5", ".",
                        "C6", "=", "G5", "E5", "C6", "=", "=", "=", "C5", "=", ".", ".", "C5", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~4)
                        "C2", "-", "C3", "C2", "G2", "-", "E2", "C2", "C2", "C2", "D2", "E2", "G2", "E2", "D2", "C2",
                        "G1", "-", "G2", "G1", "D2", "-", "B1", "G1", "G1", "A1", "B1", "D2", "G2", "D2", "B1", "G1",
                        "A1", "-", "A2", "A1", "E2", "-", "C2", "A1", "A1", "B1", "C2", "E2", "A2", "E2", "C2", "A1",
                        "F1", "-", "F2", "F1", "C2", "-", "A1", "F1", "G1", "-", "D2", "B1", "G1", "A1", "B1", "G1",

                        // Part B (5~8)
                        "D2", "-", "A2", "D2", "F2", "-", "A2", "C3", "D3", "-", "C3", "A2", "F2", "D2", "C2", "D2",
                        "G1", "-", "D2", "G1", "B1", "-", "D2", "F2", "G2", "-", "F2", "D2", "B1", "G1", "A1", "B1",
                        "E2", "-", "B2", "E2", "G2", "-", "B2", "D3", "E3", "-", "D3", "B2", "G2", "E2", "D2", "E2",
                        "A1", "-", "E2", "A1", "C2", "-", "E2", "G2", "A2", "-", "G2", "E2", "C2", "A1", "E2", "A1",

                        // Part C (9~12)
                        "F1", "F1", "C2", "F1", "A1", "A1", "C2", "C2", "F2", "F2", "E2", "E2", "C2", "C2", "A1", "A1",
                        "G1", "G1", "D2", "G1", "B1", "B1", "D2", "D2", "G2", "G2", "F2", "F2", "D2", "D2", "B1", "B1",
                        "G1", "G1", "B1", "B1", "D2", "D2", "F2", "F2", "G2", "G2", "F2", "F2", "D2", "D2", "B1", "G1",
                        "C2", "-", "E2", "-", "G2", "-", "C3", "-", "C2", "-", "G2", "-", "C2", "-", "C2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~4)
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", ".", "K", "K", "S", ".", "C", ".", ".", ".",

                        // Part B (5~8)
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", ".", "K", "K", "S", ".", "C", ".", ".", ".",

                        // Part C (9~12)
                        "C", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", ".", "S", ".", "H", ".", "K", "K", "S", "S", "C", "C", "C", "-"
                    };
                    break;

                case "shop":
                    // 「도성 주막 풍류」 (130 BPM, A minor 통통 튀는 해학 재즈 스윙, 192 스텝 12마디)
                    bpm = 130f;
                    totalSteps = 192;
                    ch0Prog = 107; // Koto (경쾌하고 구성진 가야금 스윙)
                    ch1Prog = 74;  // Recorder (정겨운 단소 대선율)
                    ch2Prog = 45;  // Pizzicato Strings (통통 튀는 피치카토 반주)
                    ch3Prog = 32;  // Acoustic Bass (워킹 스윙 베이스)
                    melody = new string[]
                    {
                        // Part A (1~4 measures - 흥겨운 주막 엇박 스윙 훅 + Eb 블루스 슬라이드)
                        "E5", ".", "C5", "A4", "C5", "D5", "Eb5", "E5", "A5", ".", "G5", "E5", "C5", "=", "A4", ".",
                        "D5", ".", "F5", "A5", ".", "F5", "D5", ".", "E5", "F5", "E5", "D5", "C5", "=", ".", ".",
                        "B4", ".", "D5", "G5", ".", "F5", "D5", ".", "B4", "C5", "D5", "F5", "E5", "=", "D5", ".",
                        "C5", ".", "E5", "G5", "C6", ".", "B5", "A5", "G5", "E5", "C5", "D5", "E5", "=", ".", ".",

                        // Part B (5~8 measures - 얼씨구 술잔 기울이는 해학 7음 브리지)
                        "F5", ".", "A5", "C6", ".", "A5", "F5", ".", "E5", "F5", "G5", "A5", "F5", "=", ".", ".",
                        "D5", ".", "F5", "A5", ".", "G5", "F5", ".", "E5", "D5", "C5", "B4", "D5", "=", ".", ".",
                        "E5", ".", "G#5", "B5", "D6", "=", "C6", "B5", "A5", "G#5", "F#5", "E5", "B4", "=", ".", ".",
                        "A4", "C5", "E5", "A5", "G5", "=", "E5", "C5", "B4", "C5", "Eb5", "E5", "A4", "=", ".", ".",

                        // Part C (9~12 measures - 흥겨운 난장 대합주 스윙 피날레)
                        "D5", ".", "A5", "F5", "D5", "F5", "A5", "C6", "B5", "A5", "G5", "F5", "D5", "=", ".", ".",
                        "G5", ".", "D6", "B5", "G5", "A5", "B5", "D6", "C6", "B5", "A5", "G5", "D5", "=", ".", ".",
                        "C6", ".", "G5", "E5", "C5", "E5", "G5", "C6", "B5", "A5", "G5", "E5", "C5", "=", ".", ".",
                        "B4", "E5", "G#5", "B5", "D6", "=", "C6", "B5", "E5", "=", "G#5", "=", "A5", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~4) - 진짜 재즈 워킹 베이스라인!
                        "A2", "-", "C3", "-", "D3", "-", "Eb3", "-", "E3", "-", "D3", "-", "C3", "-", "B2", "-",
                        "D2", "-", "F2", "-", "A2", "-", "C3", "-", "Bb2", "-", "A2", "-", "F2", "-", "D2", "-",
                        "G2", "-", "B2", "-", "D3", "-", "F3", "-", "E3", "-", "D3", "-", "B2", "-", "G2", "-",
                        "C2", "-", "E2", "-", "G2", "-", "A2", "-", "G2", "-", "E2", "-", "D2", "-", "B1", "-",

                        // Part B (5~8)
                        "F2", "-", "A2", "-", "C3", "-", "E3", "-", "D3", "-", "C3", "-", "A2", "-", "F2", "-",
                        "D2", "-", "F2", "-", "A2", "-", "C3", "-", "B2", "-", "A2", "-", "G2", "-", "F2", "-",
                        "E2", "-", "G#2", "-", "B2", "-", "D3", "-", "C3", "-", "B2", "-", "G#2", "-", "E2", "-",
                        "A2", "-", "C3", "-", "E3", "-", "G3", "-", "E3", "-", "C3", "-", "A2", "-", "E2", "-",

                        // Part C (9~12)
                        "D2", "-", "F2", "-", "A2", "-", "C3", "-", "B2", "-", "A2", "-", "F2", "-", "D2", "-",
                        "G2", "-", "B2", "-", "D3", "-", "F3", "-", "E3", "-", "D3", "-", "B2", "-", "G2", "-",
                        "C2", "-", "E2", "-", "G2", "-", "B2", "-", "A2", "-", "G2", "-", "E2", "-", "C2", "-",
                        "E2", "-", "G#2", "-", "B2", "-", "E3", "-", "A2", "-", "E2", "-", "A2", "-", "A2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~4)
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", "K", "H", "H", "S", ".", "H", "C",

                        // Part B (5~8)
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", "K", "H", "H", "S", ".", "H", "C",

                        // Part C (9~12)
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", "K", "S", ".", "H", ".", "K", ".", "H", "K", "S", ".", "H", "O",
                        "K", "K", "H", "K", "S", "S", "H", ".", "K", "K", "H", "H", "S", "S", "C", "C"
                    };
                    break;

                case "victory":
                    // 「훈민정음 만세」 (142 BPM, D major 환희의 개선 대관식 팡파르, 192 스텝 12마디)
                    bpm = 142f;
                    totalSteps = 192;
                    ch0Prog = 56;  // Trumpet (환희의 개선 트럼펫 팡파르)
                    ch1Prog = 73;  // Flute (청아한 대금 선율)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 승리 오케스트라)
                    ch3Prog = 32;  // Acoustic Bass (축제 베이스)
                    melody = new string[]
                    {
                        // Part A (1~4 measures - 눈부신 승리 팡파르 훅)
                        "D5", ".", "D5", "D5", "A5", "=", "F#5", ".", "G5", "A5", "B5", "C#6", "D6", "=", ".", ".",
                        "C#6", ".", "A5", "E5", "A5", "=", "B5", ".", "C#6", "=", "B5", "A5", "E5", "=", ".", ".",
                        "F#5", ".", "D5", "B4", "F#5", "=", "G5", "A5", "B5", "=", "A5", "G5", "F#5", "=", "D5", ".",
                        "G5", "A5", "B5", "C#6", "D6", "=", "B5", "G5", "A5", "=", "=", "=", "D5", "F#5", "A5", ".",

                        // Part B (5~8 measures - 백성의 환호와 감동의 찬가)
                        "G5", ".", "B5", "E6", "D6", "=", "C#6", "B5", "G5", "=", "E5", "G5", "B5", "=", ".", ".",
                        "A5", ".", "C#6", "E6", "A6", "=", "G6", "F#6", "E6", "=", "C#6", "A5", "E5", "=", ".", ".",
                        "F#5", "A5", "C#6", "F#6", "E6", "=", "D6", "C#6", "B5", "A5", "G#5", "F#5", "C#6", "=", ".", ".",
                        "D6", "=", "B5", "F#5", "D5", "F#5", "B5", "D6", "C#6", "B5", "A5", "F#5", "D5", "=", ".", ".",

                        // Part C (9~12 measures - 태평성대 대합주 피날레)
                        "G5", ".", "B5", "D6", "G6", "=", "F#6", "E6", "D6", "C#6", "B5", "A5", "G5", "=", "B5", ".",
                        "A5", ".", "C#6", "E6", "A6", "=", "G6", "F#6", "E6", "D6", "C#6", "B5", "A5", "=", "E5", ".",
                        "D5", "F#5", "A5", "D6", "E6", "=", "F#6", ".", "G6", "=", "F#6", "E6", "D6", "=", "A5", ".",
                        "D6", "=", "A5", "F#5", "D6", "=", "=", "=", "D5", "=", ".", ".", "D6", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~4)
                        "D2", "-", "D3", "D2", "A2", "-", "F#2", "D2", "D2", "E2", "F#2", "A2", "D3", "A2", "F#2", "D2",
                        "A1", "-", "A2", "A1", "E2", "-", "C#2", "A1", "A1", "B1", "C#2", "E2", "A2", "E2", "C#2", "A1",
                        "B1", "-", "B2", "B1", "F#2", "-", "D2", "B1", "B1", "C#2", "D2", "F#2", "B2", "F#2", "D2", "B1",
                        "G1", "-", "D2", "G1", "B1", "D2", "G2", "B2", "A2", "-", "E2", "C#2", "A1", "B1", "C#2", "A1",

                        // Part B (5~8)
                        "E2", "-", "B2", "E2", "G2", "-", "B2", "D3", "E3", "-", "D3", "B2", "G2", "E2", "D2", "E2",
                        "A1", "-", "E2", "A1", "C#2", "-", "E2", "G2", "A2", "-", "G2", "E2", "C#2", "A1", "C#2", "E2",
                        "F#2", "-", "C#3", "F#2", "A2", "-", "C#3", "E3", "F#3", "-", "E3", "C#3", "A2", "F#2", "C#2", "F#2",
                        "B1", "-", "F#2", "B1", "D2", "-", "F#2", "A2", "B2", "-", "A2", "F#2", "D2", "B1", "F#2", "B1",

                        // Part C (9~12)
                        "G1", "G1", "D2", "G1", "B1", "B1", "D2", "D2", "G2", "G2", "F#2", "F#2", "D2", "D2", "B1", "B1",
                        "A1", "A1", "E2", "A1", "C#2", "C#2", "E2", "E2", "A2", "A2", "G2", "G2", "E2", "E2", "C#2", "A1",
                        "D2", "D2", "F#2", "F#2", "A2", "A2", "D3", "D3", "A2", "A2", "F#2", "F#2", "D2", "D2", "A1", "A1",
                        "D2", "-", "A2", "-", "D3", "-", "A2", "-", "D2", "-", "A2", "-", "D2", "-", "D2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~4)
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "S", "S", "C", ".", ".", ".",

                        // Part B (5~8)
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "S", "S", "C", ".", ".", ".",

                        // Part C (9~12)
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "C",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "S", "S", "C", "C", "C", "-"
                    };
                    break;

                default: // "stage_market" / "stage" - 「한양 저잣거리 활극」 (146 BPM, E minor 쾌활한 K-펑크 스트리트 브롤, 384 스텝 24마디)
                    bpm = 146f;
                    totalSteps = 384;
                    ch0Prog = 107; // Koto (강렬하고 청명한 가야금 스트리트 리드)
                    ch1Prog = 73;  // Flute (대금 시원한 화음 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (풍성한 현악 패드)
                    ch3Prog = 36;  // Slap Bass 1 (탄력 있는 펑크 슬랩 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 쾌활하고 중독적인 E Blues 펑크 스트리트 훅!)
                        "E5", ".", "G5", "E5", "B5", "=", "A5", ".", "G5", "=", "E5", "G5", "B5", "=", "=", ".",
                        "A5", ".", "F#5", "D5", "A5", "=", "G5", ".", "F#5", "=", "E5", "D5", "F#5", "=", "=", ".",
                        "E5", ".", "G5", "A5", "Bb5", "=", "B5", "A5", "G5", "=", "E5", "D5", "C5", "=", "D5", ".",
                        "B4", "D#5", "F#5", "=", "A5", "=", "G5", "F#5", "E5", "D#5", "B4", "=", ".", ".", "B4", "D5",
                        "E5", "=", "G5", "A5", "B5", "=", "D6", ".", "B5", "=", "A5", "G5", "E5", "=", "G5", "A5",
                        "B5", ".", "D6", "B5", "G5", "=", "A5", "B5", "D6", "=", "B5", "G5", "E5", "=", "D5", ".",
                        "C5", "D5", "E5", "=", "G5", "=", "A5", "Bb5", "A5", "G5", "E5", "=", "D5", "E5", "G5", ".",
                        "F#5", "=", "A5", "G5", "F#5", "=", "D#5", "=", "B4", "=", "D#5", "F#5", "B5", "=", "=", ".",

                        // Part B (Measures 9~16 - 영웅미가 터지는 시원한 후렴구 코러스)
                        "C6", "=", "=", "B5", "G5", "=", "E5", "G5", "A5", "=", "G5", "E5", "D5", "=", "C5", ".",
                        "D6", "=", "=", "C6", "A5", "=", "F#5", "A5", "B5", "=", "A5", "F#5", "D5", "=", ".", "D5",
                        "G5", "A5", "B5", "=", "D6", "=", "=", "B5", "G5", "=", "A5", "B5", "D6", "=", "=", ".",
                        "E6", "=", "D6", "B5", "G5", "=", "B5", "A5", "G5", "E5", "D5", "=", "E5", "=", "=", ".",
                        "A5", ".", "G5", "E5", "C6", "=", "B5", "A5", "G5", "=", "E5", "C5", "D5", "=", "E5", ".",
                        "F#5", ".", "G5", "A5", "D6", "=", "C6", "B5", "A5", "=", "F#5", "D5", "E5", "=", "F#5", ".",
                        "G5", "A5", "Bb5", "B5", "C6", "=", "D6", "=", "E6", "=", "D6", "C6", "B5", "=", "A5", "G5",
                        "F#5", "=", "=", "=", "D#5", "=", "F#5", "=", "B5", "=", "=", ".", "B4", "D5", "E5", "G5",

                        // Part C (Measures 17~24 - 16비트 쾌감 폭발 클라이맥스 잼!)
                        "B5", ".", "B5", "G5", "E5", "G5", "B5", "=", "D6", ".", "C6", "B5", "A5", "=", "G5", ".",
                        "C6", ".", "C6", "A5", "G5", "A5", "C6", "=", "E6", ".", "D6", "C6", "B5", "=", "A5", ".",
                        "D6", ".", "D6", "B5", "A5", "B5", "D6", "=", "F#6", "=", "E6", "D6", "C6", "=", "B5", "A5",
                        "G5", "=", "B5", "D6", "G6", "=", "=", "=", "F#6", "E6", "D6", "B5", "G5", "=", ".", ".",
                        "E6", "=", "C6", "G5", "E6", "=", "D6", "C6", "B5", "=", "A5", "G5", "E5", "=", "G5", ".",
                        "A5", "G5", "E5", "D5", "C5", "D5", "E5", "G5", "A5", "Bb5", "B5", "D6", "C6", "=", "A5", ".",
                        "B5", "=", "F#5", "D#5", "B5", "=", "A5", "G5", "F#5", "=", "D#5", "E5", "F#5", "=", "D#5", ".",
                        "E5", "=", "G5", "B5", "E6", "=", "=", "=", "E5", "=", ".", ".", "E5", "=", ".", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8) - 펑키 슬랩 & 옥타브 펌핑 베이스
                        "E2", "-", "E3", "E2", "-", "E2", "G2", "A2", "B2", "-", "B1", "E2", "-", "G2", "A2", "B2",
                        "D2", "-", "D3", "D2", "-", "D2", "F#2", "A2", "D3", "-", "C3", "D2", "A2", "F#2", "E2", "D2",
                        "C2", "-", "C3", "C2", "-", "C2", "E2", "G2", "C3", "-", "B2", "C2", "G2", "E2", "D2", "C2",
                        "B1", "-", "B2", "B1", "D#2", "F#2", "A2", "B2", "A2", "G2", "F#2", "D#2", "B1", "D#2", "F#2", "B2",
                        "E2", "-", "E3", "E2", "-", "G2", "B2", "D3", "E3", "-", "D3", "B2", "G2", "E2", "D2", "E2",
                        "G2", "-", "G3", "G2", "-", "B2", "D3", "F#3", "G3", "-", "F#3", "D3", "B2", "G2", "A2", "B2",
                        "A1", "-", "A2", "A1", "C2", "E2", "G2", "A2", "G2", "E2", "C2", "A1", "G1", "A1", "C2", "A1",
                        "B1", "D#2", "F#2", "A2", "B2", "A2", "F#2", "D#2", "B1", "-", "F#2", "-", "B1", "-", "B1", "-",

                        // Part B (9~16)
                        "C2", "-", "C3", "C2", "E2", "-", "G2", "C3", "C2", "-", "G2", "C2", "B2", "G2", "E2", "C2",
                        "D2", "-", "D3", "D2", "F#2", "-", "A2", "D3", "D2", "-", "A2", "D2", "C3", "A2", "F#2", "D2",
                        "G2", "-", "G3", "G2", "B2", "-", "D3", "G3", "G2", "-", "D3", "G2", "F#3", "D3", "B2", "G2",
                        "E2", "-", "E3", "E2", "G2", "-", "B2", "E3", "E2", "-", "B2", "E2", "D3", "B2", "G2", "E2",
                        "A1", "-", "A2", "A1", "C2", "-", "E2", "G2", "A2", "-", "G2", "E2", "C2", "A1", "C2", "E2",
                        "D2", "-", "D3", "D2", "F#2", "-", "A2", "C3", "D3", "-", "C3", "A2", "F#2", "D2", "F#2", "A2",
                        "C2", "C2", "E2", "G2", "C3", "B2", "A2", "G2", "E2", "C2", "E2", "G2", "C3", "G2", "E2", "C2",
                        "B1", "D#2", "F#2", "A2", "B2", "A2", "F#2", "D#2", "B1", "-", "F#2", "-", "B1", "-", "B1", "-",

                        // Part C (17~24)
                        "E2", "E2", "E3", "E2", "G2", "G2", "B2", "B2", "E3", "E3", "D3", "D3", "C3", "C3", "B2", "B2",
                        "C2", "C2", "C3", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "B2", "B2", "A2", "A2", "G2", "G2",
                        "D2", "D2", "D3", "D2", "F#2", "F#2", "A2", "A2", "D3", "D3", "C3", "C3", "A2", "A2", "F#2", "F#2",
                        "G2", "G2", "G3", "G2", "B2", "B2", "D3", "D3", "G3", "G3", "F#3", "F#3", "D3", "D3", "B2", "B2",
                        "C2", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "B2", "B2", "A2", "A2", "G2", "G2", "E2", "E2",
                        "A1", "A1", "C2", "C2", "E2", "E2", "G2", "G2", "A2", "A2", "G2", "G2", "E2", "E2", "C2", "C2",
                        "B1", "B1", "D#2", "D#2", "F#2", "F#2", "B2", "B2", "A2", "A2", "F#2", "F#2", "D#2", "D#2", "B1", "B1",
                        "E2", "-", "G2", "-", "A2", "-", "B2", "-", "E2", "-", "B2", "-", "E2", "-", "E2", "-"
                    };
                    drums = new string[]
                    {
                        // Part A (1~8)
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", ".", "S", "H", "S", "O",
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", "H", "S", "S", "S", "C",

                        // Part B (9~16)
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", ".", "S", "H", "S", "O",
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "T", "T", "S", "S", "C", "C",

                        // Part C (17~24)
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "H", "K", "H", "S", ".", "H", "H", "K", "K", "H", ".", "S", "H", "S", "O",
                        "C", ".", "H", ".", "S", ".", "H", ".", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", ".", "K", "K", "H", ".", "S", ".", "H", "O",
                        "K", ".", "H", ".", "S", ".", "H", "H", "K", ".", "H", ".", "S", ".", "H", "O",
                        "K", "K", "K", "K", "S", "S", "S", "S", "K", "K", "H", "H", "S", "S", "S", "C"
                    };
                    break;
            }

            // BGM 연주 속도를 기존의 80% 수준으로 조절 (BPM * 0.8)
            bpm *= 0.8f;

            TrackMixConfig mix = GetTrackMixConfig(trackName, ref ch3Prog);

            var events = new List<TimedEvent>();
            const int stepTicks = 24; // 96 ticks per quarter note -> 24 ticks per 16th note step
            int totalTicks = totalSteps * stepTicks;
            var channelLastOff = new Dictionary<(int ch, int pitch), int>();

            // 1. Tick 0: 8채널 스테레오 믹싱, 프로그램 체인지 & 리버브/코러스 컨트롤러 초기화
            InitChannel(events, 0, ch0Prog, 118, 62, 54, 15);           // Ch 0: Lead Melody (Center-L)
            InitChannel(events, 1, ch1Prog, 92, 82, 76, 25);            // Ch 1: Counter Woodwind/Organ (Center-R)
            InitChannel(events, 2, ch2Prog, 74, 92, 64, 35);            // Ch 2: Harmony Strings/Pizz (Center)
            InitChannel(events, 3, ch3Prog, 106, 28, 64, 0);            // Ch 3: Groove/Battle Bass (Center)
            InitChannel(events, 4, mix.Ch4Prog, mix.Ch4Vol, 76, 36, 30);// Ch 4: Harp/Guitar/EP Arp (Left)
            InitChannel(events, 5, mix.Ch5Prog, mix.Ch5Vol, 88, 92, 25);// Ch 5: Brass/Choir/Horn Punches (Right)
            InitChannel(events, 6, mix.Ch6Prog, mix.Ch6Vol, 84, 24, 35);// Ch 6: Timpani/Bells/Vibe Sparkle (Wide-L)
            events.Add(new TimedEvent(0, new byte[] { 0xB9, 0x07, 114 })); // Ch 9: Percussion Volume
            events.Add(new TimedEvent(0, new byte[] { 0xB9, 0x5B, 64 }));  // Ch 9: Percussion Hall Reverb

            // 2. 스텝별 다층 오케스트레이션 & 다이내믹 그루브 생성
            int partLen = totalSteps / 3;
            for (int s = 0; s < totalSteps; s++)
            {
                // 오프비트 16분음표 미세 스윙(+SwingTicks)으로 펑크/스윙 그루브 부여
                int swingOffset = (mix.SwingTicks > 0 && (s % 2 == 1)) ? mix.SwingTicks : 0;
                int stepTick = s * stepTicks + swingOffset;
                int partIdx = Math.Min(2, s / Math.Max(1, partLen)); // 0=Part A, 1=Part B, 2=Part C
                int barStep = s % 16;
                int phraseStep = s % 64;
                bool isFillBar = (phraseStep >= 56);

                int[] currentChordNotes = Array.Empty<int>();
                if (chords != null && chords.Length > 0)
                {
                    int stepsPerChord = Math.Max(1, totalSteps / chords.Length);
                    int chordIndex = (s / stepsPerChord) % chords.Length;
                    currentChordNotes = ParseChord(chords[chordIndex]);
                }

                // --- Channel 0: Lead Melody + 시김새 농현 벤드 + 다이내믹 프레이징 ---
                string mStr = melody[s % melody.Length];
                int mNote = NoteToMidi(mStr);
                if (mNote >= 0)
                {
                    int durTicks = stepTicks;
                    int colonIdx = mStr.IndexOfAny(new[] { ':', '_' });
                    if (colonIdx >= 0 && int.TryParse(mStr.Substring(colonIdx + 1), out int explicitSteps) && explicitSteps > 0)
                    {
                        durTicks = explicitSteps * stepTicks;
                    }
                    else
                    {
                        int lookAhead = s + 1;
                        while (lookAhead < totalSteps && (melody[lookAhead % melody.Length] == "=" || melody[lookAhead % melody.Length] == "-"))
                        {
                            durTicks += stepTicks;
                            lookAhead++;
                        }
                    }

                    int gateTicks = Math.Max(12, durTicks - 4);
                    int partBoost = partIdx * 5;
                    int baseVel = (durTicks >= stepTicks * 3 ? 112 : (durTicks >= stepTicks * 2 ? 105 : (s % 4 == 0 ? 101 : 90)));
                    byte mVel = (byte)Math.Clamp(baseVel + partBoost, 70, 125);
                    EmitNoteSafe(events, channelLastOff, 0, mNote, mVel, stepTick, gateTicks, totalTicks);

                    // 가야금/해금 전통 시김새: 장음 진입 스쿠프(Scoop) + 농현 피치 벤드 비브라토
                    if (durTicks >= stepTicks * 2)
                    {
                        int bendStart = Math.Min(totalTicks - 4, stepTick + 2);
                        int bendReset = Math.Min(totalTicks - 3, stepTick + 8);
                        int bendUp = Math.Min(totalTicks - 2, stepTick + (durTicks / 2));
                        int bendDn = Math.Min(totalTicks - 2, stepTick + (durTicks * 3 / 4));
                        int bendEnd = Math.Min(totalTicks - 1, stepTick + gateTicks);

                        events.Add(new TimedEvent(stepTick, new byte[] { 0xE0, 0x00, 0x32 }, 1)); // 미세 하향 스쿠프 시작
                        events.Add(new TimedEvent(bendReset, new byte[] { 0xE0, 0x00, 0x40 }, 1)); // 정음 복귀
                        events.Add(new TimedEvent(bendUp, new byte[] { 0xE0, 0x00, 0x54 }, 1));   // 농현 올림
                        events.Add(new TimedEvent(bendDn, new byte[] { 0xE0, 0x00, 0x36 }, 1));   // 농현 내림
                        events.Add(new TimedEvent(bendEnd, new byte[] { 0xE0, 0x00, 0x40 }, 0));  // 원위치 복귀
                    }

                    // --- Channel 1: 다성부 대선율 (Part A 메아리 응답, Part B/C 3도·6도 하모니 + 장식 리프) ---
                    RenderCounterMelody(events, channelLastOff, mNote, durTicks, s, stepTick, partIdx, currentChordNotes, totalTicks, stepTicks);
                }
                else if (mStr == "." && partIdx >= 1 && currentChordNotes.Length >= 3 && barStep >= 12)
                {
                    // 프레이즈 끝 쉼표 구간에서 대금/피리가 짧게 응답하는 Call-and-Response 장식구
                    int respNote = currentChordNotes[(s % 2 == 0) ? 2 : 1] + 24;
                    while (respNote > 88) respNote -= 12;
                    EmitNoteSafe(events, channelLastOff, 1, respNote, (byte)(76 + partIdx * 6), stepTick, stepTicks - 4, totalTicks);
                }

                // --- Channel 3: 그루브/옥타브 펌핑/워킹 베이스 엔진 ---
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
                    byte bVel = (byte)Math.Clamp((barStep % 4 == 0 ? 108 : 94) + partIdx * 4, 75, 122);
                    EmitNoteSafe(events, channelLastOff, 3, bNote, bVel, stepTick, gateTicks, totalTicks);

                    // 2스텝 이상 지속 베이스 구간에 옥타브 팝/5도 픽업 삽입 (전투감 & 펑크 그루브 강화)
                    if (durTicks >= stepTicks * 2 && mix.BassStyle != BassStyleKind.SymphonicHold)
                    {
                        int popTick = stepTick + stepTicks;
                        if (mix.BassStyle == BassStyleKind.FunkSlap && (barStep % 4 == 2 || isFillBar))
                        {
                            EmitNoteSafe(events, channelLastOff, 3, bNote + 12, 92, popTick, stepTicks - 6, totalTicks);
                        }
                        else if ((mix.BassStyle == BassStyleKind.PickedMetal || mix.BassStyle == BassStyleKind.Driving8th) && partIdx >= 1)
                        {
                            int pickupPitch = (barStep == 14 && currentChordNotes.Length >= 3) ? currentChordNotes[2] - 12 : bNote;
                            EmitNoteSafe(events, channelLastOff, 3, Math.Max(28, pickupPitch), 96, popTick, stepTicks - 4, totalTicks);
                        }
                    }
                }

                // --- Channel 2: 확장 화성(오픈 보이싱 & 9th/7th) + Expression 스웰 & Part B/C 리듬 리프트 ---
                if (currentChordNotes.Length > 0)
                {
                    int stepsPerChord = Math.Max(1, totalSteps / chords!.Length);
                    int[] voiced = BuildRichChordVoicing(currentChordNotes, mix.StringStyle, partIdx);

                    if (s % stepsPerChord == 0)
                    {
                        int chordDur = stepsPerChord * stepTicks - 6;
                        // 현악 앙상블/피치카토 공통 Expression 스웰(CC 11): 프레이즈 강약 호흡 부여
                        events.Add(new TimedEvent(stepTick, new byte[] { 0xB2, 0x0B, 84 }, 1));
                        int midSwell = Math.Min(totalTicks - 4, stepTick + chordDur / 2);
                        int endSwell = Math.Min(totalTicks - 2, stepTick + chordDur);
                        events.Add(new TimedEvent(midSwell, new byte[] { 0xB2, 0x0B, (byte)(108 + partIdx * 8) }, 1));
                        events.Add(new TimedEvent(endSwell, new byte[] { 0xB2, 0x0B, 92 }, 0));
                    }

                    if (ch2Prog == 45) // Pizzicato Strings: 4분음 + 2/3부 엇박 스윙 추가
                    {
                        if (s % 4 == 0 || (partIdx >= 1 && s % 8 == 6))
                        {
                            int pDur = stepTicks * 2;
                            byte pVel = (byte)(s % 8 == 0 ? 84 : (s % 4 == 0 ? 74 : 62));
                            foreach (int cn in voiced)
                                EmitNoteSafe(events, channelLastOff, 2, cn, pVel, stepTick, pDur, totalTicks);
                        }
                    }
                    else
                    {
                        if (s % stepsPerChord == 0)
                        {
                            int padDur = stepsPerChord * stepTicks - 6;
                            byte padVel = (byte)(70 + partIdx * 6);
                            foreach (int cn in voiced)
                                EmitNoteSafe(events, channelLastOff, 2, cn, padVel, stepTick, padDur, totalTicks);
                        }
                    }
                }

                // --- Channel 4: 아르페지오 / 파워코드 기타 / 일렉피아노 컴핑 레이어 ---
                RenderArpeggioLayer(events, channelLastOff, mix, currentChordNotes, s, barStep, partIdx, stepTick, stepTicks, totalTicks);

                // --- Channel 5: 브라스 펀치 / 호른 대위 / 고딕 합창 성가대 레이어 ---
                RenderBrassChoirLayer(events, channelLastOff, mix, currentChordNotes, s, barStep, phraseStep, partIdx, stepTick, stepTicks, totalTicks);

                // --- Channel 6: 팀파니 / 튜블러 벨 / 비브라폰 / 클라비넷 스파클 레이어 ---
                RenderSparkleLayer(events, channelLastOff, mix, currentChordNotes, s, barStep, phraseStep, partIdx, stepTick, stepTicks, totalTicks);

                // --- Channel 9 (GM Drums): 다층 국악·모던 퍼커션 + 고스트 스네어 + 4마디 필인 ---
                RenderDrumStep(events, drums[s % drums.Length], mix, s, barStep, phraseStep, partIdx, stepTick, totalTicks);
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

        private enum ArpStyleKind { HarpWave, HarpCascade, EpianoJazz, PowerGuitar, NylonSwing }
        private enum AccentStyleKind { BrassFunk, HornSwell, BrassFanfare, ChoirGothic, VibeSwing }
        private enum SparkleStyleKind { ClavFunk, TimpaniBattle, VibeNight, BellsRoyal, GlockVictory }
        private enum BassStyleKind { FunkSlap, Driving8th, WalkingSwing, SymphonicHold, PickedMetal }
        private enum StringStyleKind { WarmOpen, TightBattle, JazzColor }

        private sealed record TrackMixConfig(
            int Ch4Prog, byte Ch4Vol, ArpStyleKind ArpStyle,
            int Ch5Prog, byte Ch5Vol, AccentStyleKind AccentStyle,
            int Ch6Prog, byte Ch6Vol, SparkleStyleKind SparkleStyle,
            BassStyleKind BassStyle, StringStyleKind StringStyle,
            int SwingTicks, bool UseRideInChorus);

        private static TrackMixConfig GetTrackMixConfig(string trackName, ref int ch3Prog)
        {
            switch (trackName.ToLowerInvariant())
            {
                case "midboss":
                    ch3Prog = 34; // Electric Bass (Pick)
                    return new TrackMixConfig(
                        30, 66, ArpStyleKind.PowerGuitar,
                        61, 78, AccentStyleKind.BrassFunk,
                        47, 86, SparkleStyleKind.TimpaniBattle,
                        BassStyleKind.PickedMetal, StringStyleKind.TightBattle,
                        0, true);

                case "finalboss":
                case "boss":
                    ch3Prog = 38; // Synth Bass 1
                    return new TrackMixConfig(
                        30, 68, ArpStyleKind.PowerGuitar,
                        52, 76, AccentStyleKind.ChoirGothic,
                        47, 90, SparkleStyleKind.TimpaniBattle,
                        BassStyleKind.PickedMetal, StringStyleKind.TightBattle,
                        0, true);

                case "stage_fortress":
                    ch3Prog = 32; // Acoustic Bass
                    return new TrackMixConfig(
                        46, 74, ArpStyleKind.HarpWave,
                        60, 76, AccentStyleKind.HornSwell,
                        47, 78, SparkleStyleKind.TimpaniBattle,
                        BassStyleKind.SymphonicHold, StringStyleKind.WarmOpen,
                        0, true);

                case "stage_night":
                    ch3Prog = 33; // Electric Bass (Finger)
                    return new TrackMixConfig(
                        4, 72, ArpStyleKind.EpianoJazz,
                        61, 68, AccentStyleKind.BrassFunk,
                        11, 76, SparkleStyleKind.VibeNight,
                        BassStyleKind.WalkingSwing, StringStyleKind.JazzColor,
                        3, true);

                case "stage_palace":
                    ch3Prog = 32; // Acoustic Bass
                    return new TrackMixConfig(
                        46, 76, ArpStyleKind.HarpCascade,
                        61, 80, AccentStyleKind.BrassFanfare,
                        14, 80, SparkleStyleKind.BellsRoyal,
                        BassStyleKind.Driving8th, StringStyleKind.WarmOpen,
                        0, true);

                case "title":
                    ch3Prog = 32;
                    return new TrackMixConfig(
                        46, 74, ArpStyleKind.HarpCascade,
                        61, 80, AccentStyleKind.BrassFanfare,
                        14, 76, SparkleStyleKind.BellsRoyal,
                        BassStyleKind.Driving8th, StringStyleKind.WarmOpen,
                        0, true);

                case "shop":
                    ch3Prog = 32;
                    return new TrackMixConfig(
                        24, 74, ArpStyleKind.NylonSwing,
                        11, 72, AccentStyleKind.VibeSwing,
                        9, 64, SparkleStyleKind.VibeNight,
                        BassStyleKind.WalkingSwing, StringStyleKind.JazzColor,
                        4, false);

                case "victory":
                    ch3Prog = 32;
                    return new TrackMixConfig(
                        46, 78, ArpStyleKind.HarpCascade,
                        61, 82, AccentStyleKind.BrassFanfare,
                        9, 76, SparkleStyleKind.GlockVictory,
                        BassStyleKind.Driving8th, StringStyleKind.WarmOpen,
                        0, true);

                default: // "stage_market" / "stage"
                    ch3Prog = 36; // Slap Bass 1
                    return new TrackMixConfig(
                        46, 72, ArpStyleKind.HarpWave,
                        61, 74, AccentStyleKind.BrassFunk,
                        7, 68, SparkleStyleKind.ClavFunk,
                        BassStyleKind.FunkSlap, StringStyleKind.WarmOpen,
                        2, true);
            }
        }

        private static void InitChannel(List<TimedEvent> events, int ch, int prog, byte vol, byte reverb, byte pan, byte chorus)
        {
            events.Add(new TimedEvent(0, new byte[] { (byte)(0xC0 | ch), (byte)prog }));
            events.Add(new TimedEvent(0, new byte[] { (byte)(0xB0 | ch), 0x07, vol }));
            events.Add(new TimedEvent(0, new byte[] { (byte)(0xB0 | ch), 0x5B, reverb }));
            events.Add(new TimedEvent(0, new byte[] { (byte)(0xB0 | ch), 0x0A, pan }));
            if (chorus > 0) events.Add(new TimedEvent(0, new byte[] { (byte)(0xB0 | ch), 0x5D, chorus }));
        }

        private static void EmitNoteSafe(List<TimedEvent> events, Dictionary<(int ch, int pitch), int> lastOff, int ch, int pitch, byte vel, int startTick, int durTicks, int maxTicks)
        {
            int p = Math.Clamp(pitch, 24, 108);
            int sTick = Math.Clamp(startTick, 0, maxTicks - 6);
            int eTick = Math.Clamp(sTick + Math.Max(8, durTicks), sTick + 6, maxTicks - 1);

            if (lastOff.TryGetValue((ch, p), out int prevOff) && sTick < prevOff)
            {
                // 동일 채널/음고 겹침 방지: 이전 음의 Note-Off가 이미 지났을 때만 겹치지 않게 시작 보정
                sTick = prevOff;
                if (sTick >= eTick) return;
            }

            events.Add(new TimedEvent(sTick, new byte[] { (byte)(0x90 | ch), (byte)p, Math.Clamp(vel, (byte)1, (byte)127) }, 1));
            events.Add(new TimedEvent(eTick, new byte[] { (byte)(0x80 | ch), (byte)p, 0 }, 0));
            lastOff[(ch, p)] = eTick;
        }

        private static int[] BuildRichChordVoicing(int[] rawChord, StringStyleKind style, int partIdx)
        {
            if (rawChord.Length < 3) return rawChord;
            int r = rawChord[0];
            int t = rawChord[1];
            int f = rawChord[2];

            // 오픈 보이싱(3음을 한 옥타브 올려 부드러운 입체 화성 구성) + 9th/7th 텐션 확장
            int openThird = t + 12;
            if (style == StringStyleKind.TightBattle)
            {
                return new[] { r - 12, f, r + 12, openThird };
            }
            if (style == StringStyleKind.JazzColor)
            {
                int ninth = r + 14; // 9th 텐션
                return rawChord.Length >= 4
                    ? new[] { r, f, rawChord[3], openThird, ninth }
                    : new[] { r, f, openThird, ninth };
            }
            // WarmOpen: Part B/C에서 상성 옥타브/9th 확장
            if (partIdx >= 1 && rawChord.Length >= 4)
                return new[] { r, f, rawChord[3], openThird };
            if (partIdx == 2)
                return new[] { r, f, r + 12, openThird };
            return new[] { r, f, openThird };
        }

        private static void RenderCounterMelody(List<TimedEvent> events, Dictionary<(int ch, int pitch), int> lastOff,
            int mNote, int durTicks, int s, int stepTick, int partIdx, int[] chord, int maxTicks, int stepTicks)
        {
            if (!(durTicks >= stepTicks * 2 || (s % 4 == 0))) return;

            int fNote = (mNote <= 72) ? (mNote + 12) : mNote;
            if (partIdx >= 1 && chord.Length >= 2)
            {
                // Part B/C: 코드 3도/5도 기반 3/6도 대위 화음 배치
                int idx = (partIdx == 2 && chord.Length >= 3) ? 2 : 1;
                int targetHarm = chord[idx] + 12;
                while (targetHarm < 65) targetHarm += 12;
                while (targetHarm > 86) targetHarm -= 12;
                if (Math.Abs(targetHarm - mNote) >= 2) fNote = targetHarm;
            }

            int fGate = Math.Max(14, durTicks - 2);
            byte fVel = (byte)(partIdx == 2 ? 94 : 86);
            EmitNoteSafe(events, lastOff, 1, fNote, fVel, stepTick, fGate, maxTicks);

            // 대금/피리 지속음 비브라토 모듈레이션 (CC 0x01)
            if (durTicks >= stepTicks * 2)
            {
                int vibTick = Math.Min(maxTicks - 3, stepTick + (durTicks / 2));
                int vibOff = Math.Min(maxTicks - 1, stepTick + fGate);
                events.Add(new TimedEvent(vibTick, new byte[] { 0xB1, 0x01, 64 }, 1));
                events.Add(new TimedEvent(vibOff, new byte[] { 0xB1, 0x01, 0 }, 0));
            }
        }

        private static void RenderArpeggioLayer(List<TimedEvent> events, Dictionary<(int ch, int pitch), int> lastOff,
            TrackMixConfig mix, int[] chord, int s, int barStep, int partIdx, int stepTick, int stepTicks, int maxTicks)
        {
            if (chord.Length < 3) return;
            int r = chord[0], t = chord[1], f = chord[2];
            int seventh = chord.Length >= 4 ? chord[3] : r + 12;

            switch (mix.ArpStyle)
            {
                case ArpStyleKind.PowerGuitar:
                {
                    // 강렬한 8분음표 파워코드 팜뮤트 리프 + Part B/C 16분 더블 크런치
                    bool hit = (barStep % 2 == 0) || (partIdx == 2 && (barStep == 7 || barStep == 15));
                    if (!hit) return;
                    bool accented = (barStep % 4 == 0);
                    byte vel = (byte)(accented ? 94 : 72);
                    int dur = accented ? stepTicks + 8 : stepTicks - 6;
                    EmitNoteSafe(events, lastOff, 4, r - 12, vel, stepTick, dur, maxTicks);
                    EmitNoteSafe(events, lastOff, 4, f - 12, (byte)(vel - 6), stepTick, dur, maxTicks);
                    if (accented && partIdx >= 1)
                        EmitNoteSafe(events, lastOff, 4, r, (byte)(vel - 12), stepTick, dur, maxTicks);
                    break;
                }
                case ArpStyleKind.EpianoJazz:
                {
                    // 세련된 네오재즈 일렉피아노 컴핑 (오프비트 2, 4, & 박자 + 아르페지오)
                    if (barStep == 0 || barStep == 6 || barStep == 10 || (partIdx >= 1 && barStep == 14))
                    {
                        byte vel = (byte)(barStep == 0 ? 84 : 72);
                        int dur = (barStep == 0) ? stepTicks * 3 : stepTicks * 2;
                        foreach (int n in new[] { t, f, seventh })
                            EmitNoteSafe(events, lastOff, 4, n + 12, vel, stepTick, dur, maxTicks);
                    }
                    break;
                }
                case ArpStyleKind.NylonSwing:
                {
                    // 주막 기타 2 & 4 스윙 스트럼
                    if (barStep == 4 || barStep == 7 || barStep == 12 || barStep == 15)
                    {
                        byte vel = (byte)((barStep % 8 == 4) ? 84 : 66);
                        EmitNoteSafe(events, lastOff, 4, r, vel, stepTick, stepTicks, maxTicks);
                        EmitNoteSafe(events, lastOff, 4, t + 12, vel, stepTick + 2, stepTicks, maxTicks);
                        EmitNoteSafe(events, lastOff, 4, f + 12, vel, stepTick + 4, stepTicks, maxTicks);
                    }
                    break;
                }
                case ArpStyleKind.HarpCascade:
                {
                    // 웅장한 하프 16분/8분 상행·하행 물결 아르페지오
                    int[] pattern = { r + 12, f + 12, r + 24, t + 24, f + 24, t + 24, r + 24, f + 12 };
                    if (partIdx == 0 && s % 2 != 0) return; // Part A는 8분음, B/C는 16분 질주!
                    int note = pattern[barStep % pattern.Length];
                    byte vel = (byte)(72 + (barStep % 4 == 0 ? 16 : 0) + partIdx * 4);
                    EmitNoteSafe(events, lastOff, 4, note, vel, stepTick, stepTicks * 2, maxTicks);
                    break;
                }
                default: // HarpWave
                {
                    if (s % 2 != 0 && partIdx < 2) return;
                    int[] pattern = { r + 12, t + 12, f + 12, r + 24, f + 12, t + 12, seventh + 12, f + 12 };
                    int note = pattern[barStep % pattern.Length];
                    byte vel = (byte)(68 + (barStep % 4 == 0 ? 14 : 0) + partIdx * 4);
                    EmitNoteSafe(events, lastOff, 4, note, vel, stepTick, stepTicks * 2, maxTicks);
                    break;
                }
            }
        }

        private static void RenderBrassChoirLayer(List<TimedEvent> events, Dictionary<(int ch, int pitch), int> lastOff,
            TrackMixConfig mix, int[] chord, int s, int barStep, int phraseStep, int partIdx, int stepTick, int stepTicks, int maxTicks)
        {
            if (chord.Length < 3) return;
            int r = chord[0], t = chord[1], f = chord[2];

            switch (mix.AccentStyle)
            {
                case AccentStyleKind.ChoirGothic:
                {
                    // 최종보스 흑호: Part B/C 마다 장엄한 합창 성가대 화음 지속 패드
                    if (partIdx == 0 && phraseStep % 32 != 0) return;
                    if (barStep == 0)
                    {
                        byte vel = (byte)(68 + partIdx * 10);
                        foreach (int n in new[] { r + 12, t + 12, f + 12 })
                            EmitNoteSafe(events, lastOff, 5, n, vel, stepTick, stepTicks * 14, maxTicks);
                    }
                    break;
                }
                case AccentStyleKind.BrassFanfare:
                {
                    // 궁궐/타이틀/승리: 대취타 브라스 팡파르 스탭
                    if (barStep == 0 || (partIdx >= 1 && (barStep == 6 || barStep == 12)))
                    {
                        int dur = (barStep == 0) ? stepTicks * 3 : stepTicks * 2;
                        byte vel = (byte)(barStep == 0 ? 94 : 80);
                        foreach (int n in new[] { r + 12, f + 12, t + 24 })
                            EmitNoteSafe(events, lastOff, 5, n, vel, stepTick, dur, maxTicks);
                    }
                    break;
                }
                case AccentStyleKind.BrassFunk:
                {
                    // 저잣거리/중간보스/야간: 펑키 신코페이션 혼 펀치 (&4 및 1, 3박)
                    if (partIdx == 0 && barStep != 0) return;
                    if (barStep == 0 || barStep == 6 || (partIdx == 2 && barStep == 14))
                    {
                        byte vel = (byte)(barStep == 0 ? 90 : 84);
                        foreach (int n in new[] { t + 12, f + 12, (chord.Length >= 4 ? chord[3] + 12 : r + 24) })
                            EmitNoteSafe(events, lastOff, 5, n, vel, stepTick, stepTicks * 2 - 6, maxTicks);
                    }
                    break;
                }
                case AccentStyleKind.HornSwell:
                {
                    // 남산 성곽길: 비장한 호른 옥타브 응답
                    if (barStep == 0 && phraseStep % 32 == 0)
                    {
                        EmitNoteSafe(events, lastOff, 5, r + 12, 86, stepTick, stepTicks * 8, maxTicks);
                        EmitNoteSafe(events, lastOff, 5, f + 12, 78, stepTick, stepTicks * 8, maxTicks);
                    }
                    break;
                }
                case AccentStyleKind.VibeSwing:
                {
                    if (barStep == 0 || barStep == 10)
                    {
                        foreach (int n in new[] { t + 12, f + 12, r + 24 })
                            EmitNoteSafe(events, lastOff, 5, n, 76, stepTick, stepTicks * 3, maxTicks);
                    }
                    break;
                }
            }
        }

        private static void RenderSparkleLayer(List<TimedEvent> events, Dictionary<(int ch, int pitch), int> lastOff,
            TrackMixConfig mix, int[] chord, int s, int barStep, int phraseStep, int partIdx, int stepTick, int stepTicks, int maxTicks)
        {
            if (chord.Length < 3) return;
            int r = chord[0], t = chord[1], f = chord[2];

            switch (mix.SparkleStyle)
            {
                case SparkleStyleKind.TimpaniBattle:
                {
                    // 팀파니 강박 쿵! + 4마디 끝 드럼 롤 크레센도
                    if (barStep == 0 || (partIdx >= 1 && barStep == 8))
                    {
                        int timpNote = Math.Clamp(r - 12, 36, 52);
                        EmitNoteSafe(events, lastOff, 6, timpNote, (byte)(barStep == 0 ? 102 : 86), stepTick, stepTicks * 2, maxTicks);
                    }
                    else if (phraseStep >= 60)
                    {
                        int timpNote = Math.Clamp(f - 12, 36, 52);
                        EmitNoteSafe(events, lastOff, 6, timpNote, (byte)(76 + (phraseStep - 60) * 10), stepTick, stepTicks - 4, maxTicks);
                    }
                    break;
                }
                case SparkleStyleKind.BellsRoyal:
                {
                    // 광화문/타이틀 장엄한 종루 튜블러 벨 (4마디/2마디 강박)
                    if (barStep == 0 && (phraseStep % 32 == 0 || partIdx == 2))
                    {
                        EmitNoteSafe(events, lastOff, 6, r + 24, 92, stepTick, stepTicks * 6, maxTicks);
                    }
                    break;
                }
                case SparkleStyleKind.VibeNight:
                {
                    // 심야 비브라폰 은은한 텐션 코드 반짝임
                    if (barStep == 4 || (partIdx >= 1 && barStep == 12))
                    {
                        EmitNoteSafe(events, lastOff, 6, f + 24, 78, stepTick, stepTicks * 4, maxTicks);
                        EmitNoteSafe(events, lastOff, 6, t + 24, 72, stepTick + 4, stepTicks * 4, maxTicks);
                    }
                    break;
                }
                case SparkleStyleKind.ClavFunk:
                {
                    // 저잣거리 클라비넷 펑크 16비트 백킹 리프
                    if (partIdx >= 1 && (barStep == 2 || barStep == 6 || barStep == 10 || barStep == 14))
                    {
                        EmitNoteSafe(events, lastOff, 6, t + 12, 78, stepTick, stepTicks - 6, maxTicks);
                        EmitNoteSafe(events, lastOff, 6, f + 12, 74, stepTick, stepTicks - 6, maxTicks);
                    }
                    break;
                }
                case SparkleStyleKind.GlockVictory:
                {
                    // 승리 글로켄슈필 환희 스파클
                    if (barStep % 4 == 0)
                    {
                        int gn = (barStep == 0) ? r + 24 : (barStep == 8 ? f + 24 : t + 24);
                        EmitNoteSafe(events, lastOff, 6, gn, 82, stepTick, stepTicks * 2, maxTicks);
                    }
                    break;
                }
            }
        }

        private static void RenderDrumStep(List<TimedEvent> events, string dStr, TrackMixConfig mix,
            int s, int barStep, int phraseStep, int partIdx, int stepTick, int maxTicks)
        {
            bool isAccent = (s % 8 == 0 || s % 8 == 4);
            byte kVel = (byte)(isAccent ? 116 : 88);
            byte sVel = (byte)(isAccent ? 108 : 78);
            byte hVel = (byte)(isAccent ? 82 : 60);

            void AddPerc(int note, byte vel, int len)
            {
                int offTick = Math.Min(maxTicks - 1, stepTick + len);
                if (stepTick >= offTick) return;
                events.Add(new TimedEvent(stepTick, new byte[] { 0x99, (byte)note, vel }, 1));
                events.Add(new TimedEvent(offTick, new byte[] { 0x89, (byte)note, 0 }, 0));
            }

            if (dStr.Contains('K')) AddPerc(35, kVel, 12);
            if (dStr.Contains('S')) AddPerc(38, sVel, 12);
            if (dStr.Contains('H'))
            {
                // Part B/C 코러스에서는 클로즈드 하이햇 대신 라이드 심벌(51)로 사운드를 활짝 열기!
                int hatNote = (mix.UseRideInChorus && partIdx == 1 && barStep % 2 == 0) ? 51 : 42;
                AddPerc(hatNote, hVel, 8);
            }
            if (dStr.Contains('O')) AddPerc(46, 88, 20);
            if (dStr.Contains('C')) AddPerc((partIdx == 2 && barStep > 0) ? 57 : 49, 116, 48); // Crash 1 / Crash 2 교차
            if (dStr.Contains('T')) AddPerc(41, 108, 16);

            // 추가 다층 퍼커션 디테일 (고스트 스네어, 탬버린/셰이커, 우드블록/클라베, 4마디 탐탐 필인)
            if (partIdx >= 1 && (barStep % 4 == 2))
            {
                // 탬버린(54) / 셰이커(70) 업비트 반짝임
                AddPerc((mix.SwingTicks > 0) ? 70 : 54, (byte)(56 + partIdx * 8), 8);
            }
            if ((barStep == 7 || barStep == 15) && !dStr.Contains('S') && !dStr.Contains('K'))
            {
                // 미세한 고스트 스네어 탭으로 인간적인 그루브감 부여
                AddPerc(38, 42, 8);
            }
            if (barStep == 10 && partIdx >= 1 && (mix.SwingTicks > 0 || mix.AccentStyle == AccentStyleKind.BrassFanfare))
            {
                // 한국 전통 박/목탁 느낌의 Claves(75) / High Woodblock(76) 포인트
                AddPerc(76, 72, 10);
            }
            if (phraseStep >= 60 && phraseStep <= 63)
            {
                // 매 4마디 끝(step 60~63) 하강 탐탐(50 -> 48 -> 45 -> 41) 드럼 필인!
                int[] tomCascade = { 50, 48, 45, 41 };
                AddPerc(tomCascade[phraseStep - 60], (byte)(92 + (phraseStep - 60) * 6), 14);
            }
        }
    }
}
