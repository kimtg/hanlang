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
                "Dm", "Bb", "Gm", "A7", "Dm", "F", "Gm", "A7",
                // Part B: 9~16 measures
                "Bb", "C", "Dm", "F", "Gm", "Bb", "A7", "A7",
                // Part C: 17~24 measures
                "Dm", "Gm", "C", "F", "Bb", "Gm", "A7", "Dm"
            },
            "finalboss" or "boss" => new string[]
            {
                // Part A: 1~8 measures
                "Cm", "Ab", "Fm", "G7", "Cm", "Eb", "Fm", "G7",
                // Part B: 9~16 measures
                "Ab", "Bb", "Cm", "Eb", "Fm", "Ab", "G7", "G7",
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
                "Am", "F", "G", "C", "Dm", "Am", "F", "E7"
            },
            "stage_night" => new string[]
            {
                // Part A: 1~8 measures
                "Dm", "Gm", "A7", "Dm", "Bb", "C", "Dm", "A7",
                // Part B: 9~16 measures
                "F", "C", "Dm", "Bb", "Gm", "A7", "Bb", "C",
                // Part C: 17~24 measures
                "Dm", "Bb", "C", "F", "Gm", "A7", "Bb", "Dm"
            },
            "stage_palace" => new string[]
            {
                // Part A: 1~8 measures
                "G", "D", "Em", "Bm", "C", "G", "Am", "D7",
                // Part B: 9~16 measures
                "Em", "C", "G", "D", "Em", "Bm", "C", "D",
                // Part C: 17~24 measures
                "G", "Em", "C", "D", "Bm", "Em", "Am7", "D7"
            },
            "title" => new string[]
            {
                // Part A: 1~4 measures
                "C", "G", "Am", "F",
                // Part B: 5~8 measures
                "Dm", "G", "Em", "Am",
                // Part C: 9~12 measures
                "F", "G", "C", "C"
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
                "Em", "G", "Am", "Bm", "C", "D", "Em", "B7",
                // Part B: 9~16 measures
                "G", "D", "Em", "C", "Am7", "Bm7", "C", "D7",
                // Part C: 17~24 measures
                "Em", "C", "D", "G", "Am", "Bm", "C", "B7"
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
                    // 「숙명의 대결」 (168 BPM, D minor / D Dorian & Harmonic Minor, 384 스텝 24마디) - 중간보스 마달 & 철웅 테마
                    bpm = 168f;
                    totalSteps = 384;
                    ch0Prog = 107; // Koto (맹렬한 배틀 가야금 아르페지오)
                    ch1Prog = 77;  // Shakuhachi (날카로운 살수 피리 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (배틀 스트링)
                    ch3Prog = 32;  // Acoustic Bass (전투 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 전통 5음 국악 인트로 모티프: D, F, G, A, C)
                        "D4", "F4", "A4", "=", "D5", "=", "=", "Eb5", "D5", "=", "Bb4", "=", "A4", "=", "=", "=",
                        "F4", "G4", "A4", "=", "Bb4", "=", "A4", "=", "G4", "=", "F4", "=", "Eb4", "=", "=", "=",
                        "D4", "=", "=", "=", "F4", "G4", "A4", "=", "Bb4", "=", "=", "C5", "D5", "=", "=", "=",
                        "Eb5", "=", "D5", "=", "C5", "=", "Bb4", "=", "A4", "=", "=", "=", "=", "=", ".", ".",
                        "D5", "F5", "A5", "=", "D6", "=", "=", "C6", "Bb5", "=", "A5", "=", "G5", "=", "=", "=",
                        "F5", "G5", "A5", "=", "Bb5", "=", "C6", "=", "Bb5", "=", "A5", "=", "G5", "=", "=", "=",
                        "F5", "=", "G5", "A5", "F5", "=", "Eb5", "=", "D5", "=", "C5", "D5", "Eb5", "=", "C5", "=",
                        "D4", "=", "F4", "A4", "D5", "=", "=", "C#5", "D5", "=", "=", "=", "D5", "=", "=", ".",

                        // Part B (Measures 9~16 - 7음 음계 & 도리안/화성단음계 전개: E, B, C# 이끄는음 등장)
                        "Bb4", "=", "C5", "D5", "E5", "=", "F5", "=", "G5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "C5", "=", "D5", "E5", "F5", "=", "G5", "=", "A5", "=", "Bb5", "A5", "G5", "=", "F5", "=",
                        "D5", "E5", "F5", "G5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G5", "=", "F5", "=",
                        "F5", "=", "G5", "A5", "B5", "=", "C6", "=", "D6", "=", "C6", "B5", "A5", "=", "G5", "=",
                        "G5", "=", "A5", "Bb5", "C6", "=", "D6", "=", "Eb6", "=", "D6", "C6", "Bb5", "=", "A5", "=",
                        "Bb5", "=", "C6", "D6", "E6", "=", "F6", "=", "G6", "=", "F6", "E6", "D6", "=", "C6", "=",
                        "A5", "=", "Bb5", "C6", "C#6", "=", "E6", "=", "G6", "=", "F6", "E6", "D6", "=", "C#6", "=",
                        "A4", "=", "C#5", "E5", "A5", "=", "=", "=", "D5", "=", "=", "=", "D5", "=", "=", ".",

                        // Part C (Measures 17~24 - 화려한 7음 질풍 아르페지오 & 절정부)
                        "D5", "=", "E5", "F5", "G5", "=", "A5", "B5", "C6", "=", "D6", "C6", "Bb5", "=", "A5", "=",
                        "G5", "=", "A5", "Bb5", "C6", "=", "D6", "=", "Eb6", "=", "D6", "C6", "Bb5", "=", "A5", "=",
                        "C5", "D5", "E5", "F5", "G5", "=", "A5", "=", "Bb5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "F5", "=", "G5", "A5", "B5", "=", "C6", "=", "D6", "=", "E6", "F6", "D6", "=", "=", "=",
                        "Bb5", "=", "C6", "D6", "E6", "=", "F6", "=", "G6", "=", "F6", "E6", "D6", "=", "C6", "=",
                        "G5", "=", "A5", "Bb5", "C6", "=", "D6", "=", "Eb6", "=", "D6", "C6", "Bb5", "=", "A5", "=",
                        "A5", "=", "Bb5", "C6", "C#6", "=", "E6", "=", "D6", "=", "C#6", "D6", "E6", "=", "C#6", "=",
                        "D4", "=", "F4", "A4", "D5", "=", "=", "C#5", "D5", "=", "=", "=", "D5", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "D2", "D2", "D2", "D2", "Eb2", "Eb2", "D2", "D2", "F2", "F2", "G2", "G2", "A2", "G2", "F2", "Eb2",
                        "D2", "D2", "D2", "D2", "F2", "F2", "D2", "D2", "G2", "G2", "A2", "A2", "Bb2", "A2", "G2", "F2",
                        "D2", "D2", "D2", "D2", "Eb2", "Eb2", "D2", "D2", "C2", "C2", "C2", "C2", "D2", "D2", "Eb2", "D2",
                        "A1", "A1", "Bb1", "Bb1", "C2", "C2", "D2", "D2", "Eb2", "Eb2", "F2", "F2", "G2", "G2", "A2", "A2",
                        "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "Eb2", "Eb2", "Eb2", "Eb2", "D2", "D2", "D2", "D2",
                        "Bb1", "Bb1", "Bb1", "Bb1", "C2", "C2", "C2", "C2", "D2", "D2", "D2", "D2", "Eb2", "Eb2", "Eb2", "Eb2",
                        "F2", "F2", "F2", "F2", "Eb2", "Eb2", "Eb2", "Eb2", "D2", "D2", "D2", "D2", "C2", "C2", "C2", "C2",
                        "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "D2", "-", "D2", "-",

                        // Part B (9~16)
                        "Bb1", "-", "Bb1", "Bb1", "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "Bb2", "-", "A2", "F2",
                        "C2", "-", "C2", "C2", "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "C3", "-", "Bb2", "G2",
                        "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "D3", "-", "C3", "A2",
                        "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "F3", "-", "E3", "C3",
                        "G2", "-", "G2", "G2", "Bb2", "-", "Bb2", "Bb2", "D3", "-", "D3", "D3", "G3", "-", "F3", "D3",
                        "Bb2", "-", "Bb2", "Bb2", "D3", "-", "D3", "D3", "F3", "-", "F3", "F3", "Bb3", "-", "A3", "F3",
                        "A2", "A2", "A2", "A2", "C#3", "C#3", "C#3", "C#3", "E3", "E3", "E3", "E3", "A3", "A3", "G3", "E3",
                        "A2", "A2", "A2", "A2", "A2", "A2", "A2", "A2", "A2", "A2", "A2", "A2", "A2", "-", "A2", "-",

                        // Part C (17~24)
                        "D2", "D2", "F2", "F2", "G2", "G2", "A2", "A2", "D3", "D3", "C3", "C3", "Bb2", "Bb2", "A2", "A2",
                        "G2", "G2", "Bb2", "Bb2", "C3", "C3", "D3", "D3", "G3", "G3", "F3", "F3", "Eb3", "Eb3", "D3", "D3",
                        "C2", "C2", "E2", "E2", "G2", "G2", "Bb2", "Bb2", "C3", "C3", "D3", "D3", "E3", "E3", "C3", "C3",
                        "F2", "F2", "A2", "A2", "C3", "C3", "D3", "D3", "F3", "F3", "E3", "E3", "D3", "D3", "C3", "C3",
                        "Bb1", "Bb1", "D2", "D2", "F2", "F2", "A2", "A2", "Bb2", "Bb2", "C3", "C3", "D3", "D3", "Bb2", "Bb2",
                        "G2", "G2", "Bb2", "Bb2", "D3", "D3", "G3", "G3", "F3", "F3", "D3", "D3", "C3", "C3", "Bb2", "Bb2",
                        "A2", "A2", "C#3", "C#3", "E3", "E3", "G3", "G3", "A3", "A3", "G3", "G3", "E3", "E3", "C#3", "C#3",
                        "D2", "-", "D2", "-", "F2", "-", "A2", "-", "D3", "-", "A2", "-", "D2", "-", "D2", "-"
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
                        "C", "K", "H", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
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
                    // 「마도학사 흑호의 강림」 (180 BPM, C Phrygian & C Harmonic Minor, 384 스텝 24마디) - 9장 최종결전 테마
                    bpm = 180f;
                    totalSteps = 384;
                    ch0Prog = 107; // Koto (질풍노도의 배틀 가야금)
                    ch1Prog = 19;  // Church Organ (마도학사 흑호의 웅장한 파이프오르간)
                    ch2Prog = 48;  // String Ensemble 1 (어둠의 장엄한 풀 오케스트라)
                    ch3Prog = 32;  // Acoustic Bass (위압적인 배틀 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 고딕 C Phrygian 5음 테마 모티프: C, Db, Eb, G, Ab)
                        "C4", "=", "=", "=", "Eb4", "=", "G4", "=", "C5", "=", "=", "=", "=", "=", "Db5", "C5",
                        "Ab4", "=", "G4", "=", "Eb4", "=", "F4", "=", "G4", "=", "=", "=", "Ab4", "=", "G4", "=",
                        "C5", "=", "=", "Db5", "C5", "=", "Bb4", "=", "Ab4", "=", "=", "=", "G4", "=", "=", "=",
                        "F4", "=", "Eb4", "=", "Db4", "=", "=", "=", "C4", "=", "=", "=", "=", "=", "=", "=",
                        "C5", "Eb5", "G5", "=", "C6", "=", "=", "Bb5", "Ab5", "=", "G5", "=", "F5", "=", "=", "=",
                        "Eb5", "F5", "G5", "=", "Ab5", "=", "Bb5", "=", "Ab5", "=", "G5", "=", "F5", "=", "=", "=",
                        "Eb5", "=", "F5", "G5", "Eb5", "=", "Db5", "=", "C5", "=", "Bb4", "C5", "Db5", "=", "Bb4", "=",
                        "C4", "=", "Eb4", "G4", "C5", "=", "=", "B4", "C5", "=", "=", "=", "C5", "=", "=", ".",

                        // Part B (Measures 9~16 - C 화성단음계 & 7음 풀 오케스트라: B 이끄는음, D, F, Ab 다채로운 전개)
                        "Ab4", "=", "Bb4", "C5", "D5", "=", "Eb5", "=", "F5", "=", "G5", "F5", "Eb5", "=", "D5", "=",
                        "Bb4", "=", "C5", "D5", "Eb5", "=", "F5", "=", "G5", "=", "Ab5", "G5", "Eb5", "=", "D5", "=",
                        "C5", "D5", "Eb5", "F5", "G5", "=", "Ab5", "=", "Bb5", "=", "C6", "Bb5", "Ab5", "=", "G5", "=",
                        "Eb5", "=", "F5", "G5", "Ab5", "=", "Bb5", "=", "C6", "=", "D6", "C6", "Bb5", "=", "Ab5", "=",
                        "F5", "=", "G5", "Ab5", "Bb5", "=", "C6", "=", "D6", "=", "Eb6", "D6", "C6", "=", "Bb5", "=",
                        "Ab5", "=", "Bb5", "C6", "D6", "=", "Eb6", "=", "F6", "=", "Eb6", "D6", "C6", "=", "B5", "=",
                        "G5", "=", "Ab5", "B5", "D6", "=", "F6", "=", "Eb6", "=", "D6", "C6", "B5", "=", "Ab5", "=",
                        "G4", "=", "B4", "D5", "G5", "=", "=", "=", "C5", "=", "=", "=", "C5", "=", "=", ".",

                        // Part C (Measures 17~24 - 최종 결전의 7음 광란 클라이맥스)
                        "C5", "=", "D5", "Eb5", "F5", "=", "G5", "Ab5", "Bb5", "=", "C6", "Bb5", "Ab5", "=", "G5", "=",
                        "F5", "=", "G5", "Ab5", "Bb5", "=", "C6", "=", "D6", "=", "Eb6", "D6", "C6", "=", "Bb5", "=",
                        "Bb4", "C5", "D5", "Eb5", "F5", "=", "G5", "=", "Ab5", "=", "Bb5", "Ab5", "G5", "=", "F5", "=",
                        "Eb5", "=", "F5", "G5", "Ab5", "=", "Bb5", "=", "C6", "=", "D6", "Eb6", "C6", "=", "=", "=",
                        "Ab5", "=", "Bb5", "C6", "D6", "=", "Eb6", "=", "F6", "=", "G6", "F6", "Eb6", "=", "D6", "=",
                        "F5", "=", "G5", "Ab5", "B5", "=", "C6", "=", "D6", "=", "Eb6", "D6", "C6", "=", "B5", "=",
                        "G5", "=", "Ab5", "B5", "D6", "=", "F6", "=", "Eb6", "=", "D6", "C6", "B5", "=", "Db5", "=",
                        "C4", "=", "Eb4", "G4", "C5", "=", "=", "B4", "C5", "=", "=", "=", "C5", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "C2", "C2", "C2", "C2", "Db2", "Db2", "C2", "C2", "Eb2", "Eb2", "F2", "F2", "G2", "F2", "Eb2", "Db2",
                        "C2", "C2", "C2", "C2", "Eb2", "Eb2", "C2", "C2", "F2", "F2", "G2", "G2", "Ab2", "G2", "F2", "Eb2",
                        "C2", "C2", "C2", "C2", "Db2", "Db2", "C2", "C2", "Bb1", "Bb1", "Bb1", "Bb1", "C2", "C2", "Db2", "C2",
                        "G1", "G1", "Ab1", "Ab1", "Bb1", "Bb1", "C2", "C2", "Db2", "Db2", "Eb2", "Eb2", "F2", "F2", "G2", "G2",
                        "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "Db2", "Db2", "Db2", "Db2", "C2", "C2", "C2", "C2",
                        "Ab1", "Ab1", "Ab1", "Ab1", "Bb1", "Bb1", "Bb1", "Bb1", "C2", "C2", "C2", "C2", "Db2", "Db2", "Db2", "Db2",
                        "Eb2", "Eb2", "Eb2", "Eb2", "Db2", "Db2", "Db2", "Db2", "C2", "C2", "C2", "C2", "Bb1", "Bb1", "Bb1", "Bb1",
                        "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "C2", "-", "C2", "-",

                        // Part B (9~16)
                        "Ab1", "-", "Ab1", "Ab1", "C2", "-", "C2", "C2", "Eb2", "-", "Eb2", "Eb2", "Ab2", "-", "G2", "Eb2",
                        "Bb1", "-", "Bb1", "Bb1", "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "Bb2", "-", "Ab2", "F2",
                        "C2", "-", "C2", "C2", "Eb2", "-", "Eb2", "Eb2", "G2", "-", "G2", "G2", "C3", "-", "Bb2", "G2",
                        "Eb2", "-", "Eb2", "Eb2", "G2", "-", "G2", "G2", "Bb2", "-", "Bb2", "Bb2", "Eb3", "-", "D3", "Bb2",
                        "F2", "-", "F2", "F2", "Ab2", "-", "Ab2", "Ab2", "C3", "-", "C3", "C3", "F3", "-", "Eb3", "C3",
                        "Ab2", "-", "Ab2", "Ab2", "C3", "-", "C3", "C3", "Eb3", "-", "Eb3", "Eb3", "Ab3", "-", "G3", "Eb3",
                        "G2", "G2", "G2", "G2", "B2", "B2", "B2", "B2", "D3", "D3", "D3", "D3", "G3", "G3", "F3", "D3",
                        "G2", "G2", "G2", "G2", "G2", "G2", "G2", "G2", "G2", "G2", "G2", "G2", "G2", "-", "G2", "-",

                        // Part C (17~24)
                        "C2", "C2", "Eb2", "Eb2", "F2", "F2", "G2", "G2", "C3", "C3", "Bb2", "Bb2", "Ab2", "Ab2", "G2", "G2",
                        "F2", "F2", "Ab2", "Ab2", "Bb2", "Bb2", "C3", "C3", "F3", "F3", "Eb3", "Eb3", "Db3", "Db3", "C3", "C3",
                        "Bb1", "Bb1", "D2", "D2", "F2", "F2", "Ab2", "Ab2", "Bb2", "Bb2", "C3", "C3", "D3", "D3", "Bb2", "Bb2",
                        "Eb2", "Eb2", "G2", "G2", "Bb2", "Bb2", "C3", "C3", "Eb3", "Eb3", "D3", "D3", "C3", "C3", "Bb2", "Bb2",
                        "Ab1", "Ab1", "C2", "C2", "Eb2", "Eb2", "G2", "G2", "Ab2", "Ab2", "Bb2", "Bb2", "C3", "C3", "Ab2", "Ab2",
                        "F2", "F2", "Ab2", "Ab2", "C3", "C3", "F3", "F3", "Eb3", "Eb3", "C3", "C3", "Bb2", "Bb2", "Ab2", "Ab2",
                        "G2", "G2", "B2", "B2", "D3", "D3", "F3", "F3", "G3", "G3", "F3", "F3", "D3", "D3", "B2", "B2",
                        "C2", "-", "C2", "-", "Eb2", "-", "G2", "-", "C3", "-", "G2", "-", "C2", "-", "C2", "-"
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
                        "C", "K", "T", "H", "S", ".", "H", "H", "K", ".", "H", "H", "S", ".", "H", "O",
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
                    // 「남산 성곽의 황혼」 (128 BPM, A minor / C major, 384 스텝 24마디) - 3장 청계천 & 4장 남산 성곽길
                    bpm = 128f;
                    totalSteps = 384;
                    ch0Prog = 40;  // Violin (해금 질감의 애잔하고 비장한 선율)
                    ch1Prog = 77;  // Shakuhachi (성곽의 황혼을 가르는 피리 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (따스하고 비장한 현악 패드)
                    ch3Prog = 32;  // Acoustic Bass
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 고풍스러운 A 계면조 5음 테마: A, C, D, E, G)
                        "A4", "=", "=", "=", "C5", "=", "D5", "=", "E5", "=", "=", "=", "=", "=", "G5", "=",
                        "E5", "=", "D5", "=", "C5", "=", "=", "=", "A4", "=", "=", "=", "D5", "=", "=", "=",
                        "E5", "=", "G5", "=", "A5", "=", "=", "C6", "A5", "=", "=", "=", "G5", "=", "E5", "=",
                        "D5", "=", "C5", "=", "A4", "=", "=", "=", "G4", "=", "E4", "=", "A4", "=", "=", "=",
                        "A5", "=", "=", "=", "G5", "=", "E5", "=", "D5", "=", "C5", "D5", "E5", "=", "G5", "=",
                        "A5", "=", "=", "C6", "D6", "=", "C6", "=", "A5", "=", "=", "=", "G5", "=", "E5", "=",
                        "D5", "=", "E5", "=", "G5", "=", "E5", "=", "D5", "=", "C5", "A4", "G4", "=", "A4", "=",
                        "C5", "=", "D5", "=", "E5", "=", "=", "=", "G#4", "=", "B4", "=", "A4", "=", "=", ".",

                        // Part B (Measures 9~16 - 서정적인 7음 다이아토닉 선율: B, F, G# 이끄는음 등장)
                        "C5", "=", "D5", "E5", "F5", "=", "G5", "=", "A5", "=", "B5", "A5", "G5", "=", "E5", "=",
                        "G5", "=", "A5", "G5", "F5", "=", "E5", "=", "D5", "=", "C5", "D5", "E5", "=", "=", "=",
                        "A5", "=", "B5", "C6", "D6", "=", "E6", "=", "D6", "=", "C6", "B5", "A5", "=", "G5", "=",
                        "E5", "=", "F5", "G5", "A5", "=", "G5", "=", "F5", "=", "E5", "D5", "C5", "=", "=", "=",
                        "F5", "=", "G5", "A5", "B5", "=", "C6", "=", "D6", "=", "C6", "B5", "A5", "=", "G5", "=",
                        "D5", "=", "E5", "F5", "G5", "=", "A5", "=", "B5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "E5", "=", "F#5", "G#5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G#5", "=", "B4", "=",
                        "E4", "=", "G#4", "B4", "E5", "=", "=", "=", "A4", "=", "=", "=", "A4", "=", "=", ".",

                        // Part C (Measures 17~24 - 장엄한 7음 오케스트라 클라이맥스)
                        "A5", "=", "B5", "C6", "D6", "=", "E6", "=", "F6", "=", "E6", "D6", "C6", "=", "B5", "=",
                        "F5", "=", "G5", "A5", "B5", "=", "C6", "=", "D6", "=", "C6", "B5", "A5", "=", "G5", "=",
                        "G5", "=", "A5", "B5", "C6", "=", "D6", "=", "E6", "=", "D6", "C6", "B5", "=", "A5", "=",
                        "C6", "=", "D6", "E6", "F6", "=", "E6", "=", "D6", "=", "C6", "B5", "A5", "=", "G5", "=",
                        "D5", "=", "E5", "F5", "G5", "=", "A5", "=", "B5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "C5", "=", "D5", "E5", "F5", "=", "G5", "=", "A5", "=", "G5", "F5", "E5", "=", "D5", "=",
                        "F5", "=", "E5", "D5", "C5", "=", "B4", "=", "A4", "=", "G#4", "A4", "B4", "=", "G#4", "=",
                        "A4", "=", "C5", "E5", "A5", "=", "=", "=", "A4", "=", "=", "=", "A4", "=", "=", "."
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
                    // 「달빛 아래 육조거리」 (144 BPM, D minor / D Dorian & 7-Tone Jazz Fusion, 384 스텝 24마디) - 5장 주막거리 & 6장 육조거리
                    bpm = 144f;
                    totalSteps = 384;
                    ch0Prog = 107; // Koto (거문고/가야금의 탄력 있는 심야 리프)
                    ch1Prog = 73;  // Flute (대금 날카로운 대선율)
                    ch2Prog = 45;  // Pizzicato Strings (긴장감 넘치는 피치카토 반주)
                    ch3Prog = 32;  // Acoustic Bass (긴박한 워킹 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 고요한 D 계면조 5음 야간 리프)
                        "D4", "=", "=", "F4", "G4", "=", "A4", "=", "D5", "=", "=", "C5", "A4", "=", "=", "=",
                        "F4", "=", "G4", "=", "A4", "=", "=", "C5", "A4", "=", "G4", "=", "D4", "=", "=", "=",
                        "D4", "F4", "G4", "=", "A4", "=", "=", "=", "C5", "=", "=", "D5", "C5", "=", "=", "=",
                        "A4", "=", "G4", "=", "F4", "=", "D4", "=", "C4", "=", "D4", "=", "=", "=", "=", ".",
                        "D5", "=", "=", "F5", "G5", "=", "A5", "=", "C6", "=", "=", "A5", "G5", "=", "F5", "=",
                        "G5", "=", "A5", "=", "C6", "=", "=", "D6", "C6", "=", "A5", "=", "D5", "=", "=", "=",
                        "A5", "=", "G5", "F5", "D5", "=", "C5", "=", "D5", "=", "F5", "G5", "A5", "=", "=", "=",
                        "F5", "=", "D5", "=", "C5", "=", "C#5", "=", "D4", "=", "=", "=", "D4", "=", "=", ".",

                        // Part B (Measures 9~16 - 스타일리시한 7음 재즈 퓨전: E, Bb, C# 이끄는음 전개)
                        "F4", "=", "G4", "A4", "Bb4", "=", "C5", "=", "D5", "=", "E5", "D5", "C5", "=", "Bb4", "=",
                        "C5", "=", "D5", "E5", "F5", "=", "G5", "=", "A5", "=", "G5", "F5", "E5", "=", "D5", "=",
                        "D5", "E5", "F5", "G5", "A5", "=", "Bb5", "=", "C6", "=", "Bb5", "A5", "G5", "=", "F5", "=",
                        "Bb4", "=", "C5", "D5", "E5", "=", "F5", "=", "G5", "=", "F5", "E5", "D5", "=", "C5", "=",
                        "G4", "=", "A4", "Bb4", "C5", "=", "D5", "=", "E5", "=", "D5", "C5", "Bb4", "=", "A4", "=",
                        "A4", "=", "Bb4", "C#5", "D5", "=", "E5", "=", "G5", "=", "F5", "E5", "D5", "=", "C#5", "=",
                        "Bb4", "=", "C5", "D5", "E5", "=", "F5", "=", "G5", "=", "A5", "Bb5", "C6", "=", "D6", "=",
                        "C#5", "=", "E5", "G5", "A5", "=", "=", "=", "D5", "=", "=", "=", "D5", "=", "=", ".",

                        // Part C (Measures 17~24 - 혈투와 동살풀이 7음 질풍 피날레)
                        "D5", "=", "E5", "F5", "G5", "=", "A5", "=", "Bb5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "Bb5", "=", "C6", "D6", "C6", "=", "Bb5", "=", "A5", "=", "G5", "F5", "E5", "=", "D5", "=",
                        "C6", "=", "D6", "E6", "D6", "=", "C6", "=", "Bb5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "F5", "=", "G5", "A5", "Bb5", "=", "C6", "=", "D6", "=", "E6", "F6", "D6", "=", "=", "=",
                        "G5", "=", "A5", "Bb5", "C6", "=", "D6", "=", "Eb6", "=", "D6", "C6", "Bb5", "=", "A5", "=",
                        "A5", "=", "Bb5", "C#6", "D6", "=", "E6", "=", "G6", "=", "F6", "E6", "D6", "=", "C#6", "=",
                        "Bb5", "=", "A5", "G5", "F5", "=", "E5", "=", "D5", "=", "C#5", "D5", "E5", "=", "C#5", "=",
                        "D4", "=", "F4", "A4", "D5", "=", "=", "C#5", "D5", "=", "=", "=", "D5", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "D2", "-", "D2", "F2", "G2", "-", "A2", "-", "D2", "-", "F2", "-", "G2", "-", "A2", "-",
                        "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "G2", "-", "G2", "G2", "A2", "G2", "F2", "D2",
                        "D2", "-", "F2", "-", "G2", "-", "A2", "-", "Bb2", "-", "A2", "-", "G2", "-", "F2", "-",
                        "G2", "-", "A2", "-", "C3", "-", "D3", "-", "G2", "-", "F2", "-", "D2", "-", "D2", "-",
                        "D2", "D2", "F2", "F2", "G2", "G2", "A2", "A2", "D2", "D2", "F2", "F2", "G2", "G2", "A2", "A2",
                        "G2", "G2", "A2", "A2", "C3", "C3", "D3", "D3", "G2", "G2", "F2", "F2", "D2", "D2", "D2", "D2",
                        "D2", "-", "F2", "-", "G2", "-", "A2", "-", "Bb2", "-", "A2", "-", "G2", "-", "F2", "-",
                        "D2", "-", "D2", "F2", "G2", "-", "A2", "-", "D2", "-", "A2", "-", "D2", "-", "D2", "-",

                        // Part B (9~16)
                        "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "F3", "-", "E3", "C3",
                        "C2", "-", "C2", "C2", "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "C3", "-", "Bb2", "G2",
                        "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "D3", "-", "C3", "A2",
                        "Bb1", "-", "Bb1", "Bb1", "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "Bb2", "-", "A2", "F2",
                        "G2", "-", "G2", "G2", "Bb2", "-", "Bb2", "Bb2", "D3", "-", "D3", "D3", "G3", "-", "F3", "D3",
                        "A2", "-", "A2", "A2", "C#3", "-", "C#3", "C#3", "E3", "-", "E3", "E3", "A3", "-", "G3", "E3",
                        "Bb2", "Bb2", "D3", "D3", "F3", "F3", "A3", "A3", "Bb3", "Bb3", "A3", "A3", "F3", "F3", "D3", "D3",
                        "C2", "-", "E2", "-", "G2", "-", "C3", "-", "C2", "-", "G2", "-", "C2", "-", "C2", "-",

                        // Part C (17~24)
                        "D2", "D2", "F2", "F2", "G2", "G2", "A2", "A2", "D3", "D3", "C3", "C3", "A2", "A2", "F2", "F2",
                        "Bb1", "Bb1", "D2", "D2", "F2", "F2", "Bb2", "Bb2", "A2", "A2", "G2", "G2", "F2", "F2", "D2", "D2",
                        "C2", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "Bb2", "Bb2", "A2", "A2", "G2", "G2", "E2", "E2",
                        "F2", "F2", "A2", "A2", "C3", "C3", "F3", "F3", "E3", "E3", "D3", "D3", "C3", "C3", "A2", "A2",
                        "G2", "G2", "Bb2", "Bb2", "D3", "D3", "G3", "G3", "F3", "F3", "D3", "D3", "C3", "C3", "Bb2", "Bb2",
                        "A2", "A2", "C#3", "C#3", "E3", "E3", "A3", "A3", "G3", "G3", "E3", "E3", "C#3", "C#3", "A2", "A2",
                        "Bb1", "Bb1", "D2", "D2", "F2", "F2", "A2", "A2", "Bb2", "Bb2", "C3", "C3", "D3", "D3", "F3", "F3",
                        "D2", "-", "F2", "-", "G2", "-", "A2", "-", "D2", "-", "A2", "-", "D2", "-", "D2", "-"
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
                    // 「궁궐의 폭풍전야」 (160 BPM, G major / G 우조 & 7-Tone Royal Court Suite, 384 스텝 24마디) - 7장 광화문 & 8장 근정전 & 9장 집현전
                    bpm = 160f;
                    totalSteps = 384;
                    ch0Prog = 59;  // Muted Trumpet (태평소 당당한 궁중 취타 팡파르)
                    ch1Prog = 73;  // Flute (대금 화음 고음 선율)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 궁중 오케스트라)
                    ch3Prog = 32;  // Acoustic Bass (위엄 있는 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 위풍당당 G 우조 5음 대취타: G, B, D, E, A)
                        "G4", "=", "=", "=", "D5", "=", "=", "=", "E5", "=", "G5", "=", "E5", "=", "=", "=",
                        "D5", "=", "E5", "=", "D5", "=", "B4", "=", "A4", "=", "=", "B4", "D5", "=", "=", "=",
                        "G5", "=", "=", "=", "=", "=", "A5", "=", "G5", "=", "E5", "=", "D5", "=", "=", "=",
                        "B4", "=", "A4", "=", "G4", "=", "A4", "=", "B4", "=", "=", "D5", "G4", "=", "=", "=",
                        "G5", "=", "=", "=", "D6", "=", "=", "=", "B5", "=", "A5", "=", "G5", "=", "A5", "=",
                        "B5", "=", "=", "D6", "E6", "=", "D6", "=", "B5", "=", "=", "=", "G5", "=", "E5", "=",
                        "D5", "=", "E5", "=", "G5", "=", "A5", "=", "B5", "=", "A5", "G5", "E5", "=", "D5", "=",
                        "B4", "=", "A4", "=", "G4", "=", "B4", "=", "D5", "=", "=", "=", "G5", "=", "=", ".",

                        // Part B (Measures 9~16 - 화려한 7음 궁중 관현악: C, F#, D# 이끄는음 등장)
                        "E5", "=", "F#5", "G5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "C5", "=", "D5", "E5", "F#5", "=", "G5", "=", "A5", "=", "G5", "F#5", "E5", "=", "D5", "=",
                        "G5", "=", "A5", "B5", "C6", "=", "D6", "=", "E6", "=", "D6", "C6", "B5", "=", "A5", "=",
                        "D5", "=", "E5", "F#5", "G5", "=", "A5", "=", "B5", "=", "A5", "G5", "F#5", "=", "E5", "=",
                        "E5", "=", "F#5", "G5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "B4", "=", "C5", "D#5", "F#5", "=", "A5", "=", "G5", "=", "F#5", "E5", "D#5", "=", "B4", "=",
                        "C5", "=", "D5", "E5", "F#5", "=", "G5", "=", "A5", "=", "B5", "C6", "D6", "=", "C6", "=",
                        "D5", "=", "F#5", "A5", "D6", "=", "=", "=", "G5", "=", "=", "=", "G5", "=", "=", ".",

                        // Part C (Measures 17~24 - 역모 진압과 대취타 7음 총출동 피날레)
                        "G5", "=", "A5", "B5", "C6", "=", "D6", "=", "E6", "=", "F#6", "E6", "D6", "=", "C6", "=",
                        "E5", "=", "F#5", "G5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "C6", "=", "D6", "E6", "D6", "=", "C6", "=", "B5", "=", "A5", "G5", "F#5", "=", "E5", "=",
                        "D5", "=", "E5", "F#5", "G5", "=", "A5", "=", "B5", "=", "C6", "D6", "B5", "=", "=", "=",
                        "B5", "=", "C6", "D6", "E6", "=", "F#6", "=", "G6", "=", "F#6", "E6", "D6", "=", "C6", "=",
                        "E5", "=", "F#5", "G5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "A4", "=", "B4", "C5", "D5", "=", "E5", "=", "F#5", "=", "G5", "A5", "B5", "=", "A5", "=",
                        "G4", "=", "B4", "D5", "G5", "=", "=", "=", "G4", "=", "=", "=", "G5", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "E3", "-", "D3", "B2",
                        "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "-", "B2", "B2", "D3", "B2", "A2", "G2",
                        "D2", "-", "D2", "D2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "-", "A2", "G2",
                        "G2", "-", "A2", "A2", "B2", "-", "D3", "D3", "B2", "-", "A2", "G2", "D2", "-", "D2", "-",
                        "G2", "G2", "B2", "B2", "D3", "D3", "E3", "E3", "G3", "G3", "E3", "E3", "D3", "D3", "B2", "B2",
                        "G2", "G2", "A2", "A2", "B2", "B2", "D3", "D3", "B2", "B2", "A2", "A2", "G2", "G2", "D2", "D2",
                        "E2", "E2", "G2", "G2", "A2", "A2", "B2", "B2", "D3", "D3", "B2", "B2", "A2", "A2", "G2", "G2",
                        "G2", "-", "B2", "-", "D3", "-", "G3", "-", "G2", "-", "D3", "-", "G2", "-", "G2", "-",

                        // Part B (9~16)
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "E3", "-", "D3", "B2",
                        "C2", "-", "C2", "C2", "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "C3", "-", "B2", "G2",
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "G3", "-", "F#3", "D3",
                        "D2", "-", "D2", "D2", "F#2", "-", "F#2", "F#2", "A2", "-", "A2", "A2", "D3", "-", "C3", "A2",
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "E3", "-", "D3", "B2",
                        "B1", "-", "B1", "B1", "D#2", "-", "D#2", "D#2", "F#2", "-", "F#2", "F#2", "B2", "-", "A2", "F#2",
                        "C2", "C2", "E2", "E2", "G2", "G2", "B2", "B2", "C3", "C3", "D3", "D3", "E3", "E3", "C3", "C3",
                        "D2", "-", "F#2", "-", "A2", "-", "D3", "-", "D2", "-", "A2", "-", "D2", "-", "D2", "-",

                        // Part C (17~24)
                        "G2", "G2", "B2", "B2", "D3", "D3", "G3", "G3", "F#3", "F#3", "E3", "E3", "D3", "D3", "B2", "B2",
                        "E2", "E2", "G2", "G2", "B2", "B2", "E3", "E3", "D3", "D3", "C3", "C3", "B2", "B2", "G2", "G2",
                        "C2", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "B2", "B2", "A2", "A2", "G2", "G2", "E2", "E2",
                        "D2", "D2", "F#2", "F#2", "A2", "A2", "D3", "D3", "C3", "C3", "B2", "B2", "A2", "A2", "F#2", "F#2",
                        "B1", "B1", "D2", "D2", "F#2", "F#2", "B2", "B2", "A2", "A2", "G2", "G2", "F#2", "F#2", "D2", "D2",
                        "E2", "E2", "G2", "G2", "B2", "B2", "E3", "E3", "D3", "D3", "C3", "C3", "B2", "B2", "G2", "G2",
                        "A1", "A1", "C2", "C2", "E2", "E2", "G2", "G2", "A2", "A2", "B2", "B2", "C3", "C3", "A2", "A2",
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
                    // 「대취타 유생」 (122 BPM, C major / C 우조 & 7-Tone Hymn, 192 스텝 12마디)
                    bpm = 122f;
                    totalSteps = 192;
                    ch0Prog = 56;  // Trumpet (태평소 당당한 팡파르 선율)
                    ch1Prog = 73;  // Flute (대금 고음 화음)
                    ch2Prog = 48;  // String Ensemble 1 (장엄한 궁중 대취타 관현악)
                    ch3Prog = 32;  // Acoustic Bass (위풍당당 행진 베이스)
                    melody = new string[]
                    {
                        // Part A (1~4 measures - 전통 5음 궁중 행진 모티프)
                        "C4", "=", "=", "=", "G4", "=", "=", "=", "A4", "=", "=", "C5", "A4", "=", "=", "=",
                        "G4", "=", "F4", "=", "D4", "=", "C4", "=", "D4", "=", "=", "F4", "G4", "=", "=", "=",
                        "A4", "=", "=", "=", "C5", "=", "D5", "=", "C5", "=", "=", "=", "A4", "=", "G4", "=",
                        "F4", "=", "G4", "=", "A4", "=", "B4", "=", "C5", "=", "D5", "=", "C5", "=", "=", ".",

                        // Part B (5~8 measures - 풍성한 7음 다이아토닉 선율: F, B, E)
                        "D5", "=", "E5", "F5", "G5", "=", "A5", "=", "B5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "G4", "=", "A4", "B4", "C5", "=", "D5", "=", "E5", "=", "D5", "C5", "B4", "=", "A4", "=",
                        "E5", "=", "F5", "G5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G5", "=", "F5", "=",
                        "A4", "=", "B4", "C5", "D5", "=", "E5", "=", "F5", "=", "E5", "D5", "C5", "=", "=", ".",

                        // Part C (9~12 measures - 천명을 향한 출사표 7음 대단원)
                        "F5", "=", "G5", "A5", "B5", "=", "C6", "=", "D6", "=", "C6", "B5", "A5", "=", "G5", "=",
                        "G5", "=", "A5", "B5", "C6", "=", "D6", "=", "E6", "=", "D6", "C6", "B5", "=", "A5", "=",
                        "C5", "=", "E5", "G5", "C6", "=", "D6", "=", "E6", "=", "D6", "C6", "B5", "=", "A5", "=",
                        "D5", "=", "E5", "F5", "G5", "=", "B5", "=", "C6", "=", "=", "=", "C5", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~4)
                        "C2", "-", "C2", "C2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "G2", "-", "F2", "G2",
                        "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "G2", "-", "G2", "G2", "D2", "-", "D2", "G2",
                        "C2", "-", "C2", "C2", "F2", "-", "F2", "F2", "G2", "-", "G2", "G2", "A2", "-", "G2", "F2",
                        "F2", "-", "G2", "G2", "A2", "-", "C3", "C3", "G2", "-", "F2", "D2", "C2", "-", "C2", "-",

                        // Part B (5~8)
                        "D2", "-", "D2", "D2", "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "D3", "-", "C3", "A2",
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "G3", "-", "F3", "D3",
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "E3", "-", "D3", "B2",
                        "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "E3", "-", "E3", "E3", "A2", "-", "A2", "-",

                        // Part C (9~12)
                        "F2", "-", "F2", "F2", "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "F3", "-", "E3", "C3",
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "G3", "-", "F3", "D3",
                        "C2", "-", "C2", "C2", "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "C3", "-", "B2", "G2",
                        "C2", "-", "G2", "-", "C3", "-", "G2", "-", "C2", "-", "G2", "-", "C2", "-", "C2", "-"
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
                    // 「도성 주막 풍류」 (130 BPM, A minor / C major Swing & 7-Tone Tavern, 192 스텝 12마디)
                    bpm = 130f;
                    totalSteps = 192;
                    ch0Prog = 107; // Koto (가야금 타령 가락)
                    ch1Prog = 74;  // Recorder (단소 구성진 목관)
                    ch2Prog = 45;  // Pizzicato Strings (통통 튀는 피치카토 반주)
                    ch3Prog = 32;  // Acoustic Bass (유쾌한 스윙 베이스)
                    melody = new string[]
                    {
                        // Part A (1~4 measures - 익살스러운 민요 5음 테마)
                        "A4", "=", "C5", "=", "D5", "=", "=", "E5", "D5", "=", "C5", "=", "A4", "=", "=", "=",
                        "G4", "=", "A4", "=", "C5", "=", "D5", "=", "C5", "=", "=", "A4", "G4", "=", "=", "=",
                        "E5", "=", "=", "=", "G5", "=", "E5", "=", "D5", "=", "C5", "=", "D5", "=", "E5", "=",
                        "G5", "=", "E5", "=", "D5", "=", "C5", "=", "B4", "=", "G#4", "=", "A4", "=", ".", ".",

                        // Part B (5~8 measures - 활기찬 7음 스윙: F, B, G# 이끄는음)
                        "F5", "=", "G5", "A5", "B5", "=", "C6", "=", "B5", "=", "A5", "G5", "F5", "=", "E5", "=",
                        "D5", "=", "E5", "F5", "G5", "=", "A5", "=", "G5", "=", "F5", "E5", "D5", "=", "C5", "=",
                        "E5", "=", "F#5", "G#5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G#5", "=", "E5", "=",
                        "A5", "=", "G5", "F5", "E5", "=", "D5", "=", "C5", "=", "B4", "C5", "A4", "=", "=", ".",

                        // Part C (9~12 measures - 풍류 만개 7음 피날레)
                        "D5", "=", "E5", "F5", "G5", "=", "A5", "=", "B5", "=", "C6", "D6", "C6", "=", "A5", "=",
                        "G5", "=", "A5", "B5", "C6", "=", "D6", "=", "E6", "=", "D6", "C6", "B5", "=", "G5", "=",
                        "C6", "=", "B5", "A5", "G5", "=", "F5", "=", "E5", "=", "D5", "C5", "D5", "=", "E5", "=",
                        "E5", "=", "D5", "C5", "B4", "=", "A4", "=", "G#4", "=", "B4", "=", "A4", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~4)
                        "A2", "-", "C3", "-", "D3", "-", "E3", "-", "A2", "-", "C3", "-", "D3", "-", "E3", "-",
                        "A2", "-", "A2", "C3", "D3", "-", "E3", "G3", "E3", "-", "D3", "C3", "A2", "-", "A2", "-",
                        "E2", "-", "G2", "-", "A2", "-", "C3", "-", "D3", "-", "E3", "-", "C3", "-", "A2", "-",
                        "D3", "-", "C3", "-", "A2", "-", "G2", "-", "A2", "-", "C3", "-", "A2", "-", "A2", "-",

                        // Part B (5~8)
                        "F2", "-", "A2", "-", "C3", "-", "F3", "-", "F2", "-", "A2", "-", "C3", "-", "F3", "-",
                        "D2", "-", "F2", "-", "A2", "-", "D3", "-", "D2", "-", "F2", "-", "A2", "-", "D3", "-",
                        "E2", "-", "G#2", "-", "B2", "-", "E3", "-", "E2", "-", "G#2", "-", "B2", "-", "E3", "-",
                        "A2", "-", "C3", "-", "E3", "-", "A3", "-", "A2", "-", "E2", "-", "A2", "-", "A2", "-",

                        // Part C (9~12)
                        "D2", "-", "F2", "-", "A2", "-", "D3", "-", "D2", "-", "F2", "-", "A2", "-", "D3", "-",
                        "G2", "-", "B2", "-", "D3", "-", "G3", "-", "G2", "-", "B2", "-", "D3", "-", "G3", "-",
                        "C2", "-", "E2", "-", "G2", "-", "C3", "-", "C2", "-", "E2", "-", "G2", "-", "C3", "-",
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
                    // 「훈민정음 만세」 (140 BPM, D major / D 평조 & 7-Tone Radiant Hymn, 192 스텝 12마디)
                    bpm = 140f;
                    totalSteps = 192;
                    ch0Prog = 107; // Koto (가야금 환희 주선율)
                    ch1Prog = 73;  // Flute (대금 화려한 멜로디)
                    ch2Prog = 48;  // String Ensemble 1 (웅장한 승리 오케스트레이션)
                    ch3Prog = 32;  // Acoustic Bass (축제 베이스)
                    melody = new string[]
                    {
                        // Part A (1~4 measures - 환희의 5음 궁중 팡파르)
                        "D4", "=", "=", "=", "G4", "=", "A4", "=", "B4", "=", "=", "=", "=", "=", "D5", "B4",
                        "A4", "=", "B4", "=", "D5", "=", "E5", "=", "D5", "=", "=", "=", "B4", "=", "=", "=",
                        "D5", "=", "=", "=", "E5", "=", "D5", "=", "B4", "=", "A4", "=", "B4", "=", "D5", "=",
                        "E5", "=", "F#5", "=", "G5", "=", "A5", "=", "B5", "=", "C#6", "=", "D6", "=", ".", ".",

                        // Part B (5~8 measures - 빛나는 7음 감동의 찬가: G, C#, F#)
                        "E5", "=", "F#5", "G5", "A5", "=", "B5", "=", "C#6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "A5", "=", "B5", "C#6", "D6", "=", "E6", "=", "F#6", "=", "E6", "D6", "C#6", "=", "B5", "=",
                        "F#5", "=", "G#5", "A5", "B5", "=", "C#6", "=", "D6", "=", "C#6", "B5", "A5", "=", "G#5", "=",
                        "B5", "=", "C#6", "D6", "E6", "=", "F#6", "=", "G6", "=", "F#6", "E6", "D6", "=", "=", ".",

                        // Part C (9~12 measures - 성세의 영원한 7음 대합주)
                        "G5", "=", "A5", "B5", "C6", "=", "D6", "=", "E6", "=", "D6", "C6", "B5", "=", "A5", "=",
                        "A5", "=", "B5", "C#6", "D6", "=", "E6", "=", "F#6", "=", "E6", "D6", "C#6", "=", "B5", "=",
                        "D5", "=", "F#5", "A5", "D6", "=", "E6", "=", "F#6", "=", "E6", "D6", "C#6", "=", "B5", "=",
                        "G5", "=", "A5", "B5", "C#6", "=", "E6", "=", "D6", "=", "=", "=", "D6", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~4)
                        "D2", "-", "D2", "D2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "G2", "-", "A2", "G2",
                        "A2", "-", "A2", "A2", "D3", "-", "D3", "D3", "G2", "-", "G2", "G2", "E2", "-", "G2", "A2",
                        "D2", "-", "D2", "D2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "-", "A2", "G2",
                        "G2", "-", "A2", "A2", "B2", "-", "D3", "D3", "B2", "-", "A2", "G2", "D2", "-", "D2", "-",

                        // Part B (5~8)
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "E3", "-", "D3", "B2",
                        "A2", "-", "A2", "A2", "C#3", "-", "C#3", "C#3", "E3", "-", "E3", "E3", "A3", "-", "G3", "E3",
                        "F#2", "-", "F#2", "F#2", "A2", "-", "A2", "A2", "C#3", "-", "C#3", "C#3", "F#3", "-", "E3", "C#3",
                        "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "F#3", "-", "F#3", "F#3", "B2", "-", "B2", "-",

                        // Part C (9~12)
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "G3", "-", "F#3", "D3",
                        "A2", "-", "A2", "A2", "C#3", "-", "C#3", "C#3", "E3", "-", "E3", "E3", "A3", "-", "G3", "E3",
                        "D2", "-", "D2", "D2", "F#2", "-", "F#2", "F#2", "A2", "-", "A2", "A2", "D3", "-", "C#3", "A2",
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

                default: // "stage_market" / "stage" - 「한양 저잣거리 활극」 (148 BPM, E minor / G major & 7-Tone Fusion, 384 스텝 24마디)
                    bpm = 148f;
                    totalSteps = 384;
                    ch0Prog = 107; // Koto (가야금 청명한 활극 리프)
                    ch1Prog = 73;  // Flute (대금 화음 대선율)
                    ch2Prog = 48;  // String Ensemble 1 (풍성한 궁중 현악 패드)
                    ch3Prog = 32;  // Acoustic Bass (묵직한 리듬 베이스)
                    melody = new string[]
                    {
                        // Part A (Measures 1~8 - 친숙하고 청명한 E 계면조 5음 활극 인트로: E, G, A, B, D)
                        "E4", "=", "G4", "A4", "B4", "=", "=", "A4", "G4", "=", "E4", "G4", "A4", "=", "B4", "=",
                        "D5", "=", "=", "B4", "A4", "=", "G4", "A4", "B4", "=", "=", "=", "E4", "=", "=", "=",
                        "B4", "D5", "E5", "=", "G5", "=", "=", "E5", "D5", "=", "B4", "=", "A4", "=", "=", "B4",
                        "D5", "=", "B4", "=", "A4", "G4", "E4", "=", "D4", "=", "E4", "=", "=", ".", "E4", "=",
                        "E5", "=", "=", "D5", "B4", "=", "A4", "B4", "D5", "=", "=", "B4", "A4", "=", "G4", "=",
                        "E4", "G4", "A4", "=", "B4", "=", "D5", "=", "E5", "=", "D5", "=", "B4", "=", "=", "=",
                        "G5", "=", "E5", "D5", "B4", "=", "A4", "=", "G4", "=", "E4", "G4", "A4", "=", "B4", "=",
                        "D5", "=", "B4", "=", "A4", "=", "G4", "=", "F#4", "=", "D#4", "=", "E4", "=", "=", ".",

                        // Part B (Measures 9~16 - 풍부한 7음 음계 & 경쾌한 장터 전개: F#, C, D# 이끄는음 등장)
                        "G4", "=", "A4", "B4", "C5", "=", "D5", "=", "E5", "=", "F#5", "E5", "D5", "=", "B4", "=",
                        "D5", "=", "E5", "F#5", "G5", "=", "A5", "=", "B5", "=", "A5", "G5", "F#5", "=", "D5", "=",
                        "E5", "=", "F#5", "G5", "A5", "=", "B5", "=", "C6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "C5", "=", "D5", "E5", "G5", "=", "A5", "=", "G5", "=", "E5", "D5", "C5", "=", "B4", "=",
                        "A4", "=", "B4", "C5", "D5", "=", "E5", "=", "F#5", "=", "G5", "A5", "G5", "=", "E5", "=",
                        "B4", "=", "C5", "D#5", "E5", "=", "F#5", "=", "G5", "=", "A5", "B5", "A5", "=", "F#5", "=",
                        "C6", "=", "B5", "A5", "G5", "=", "F#5", "=", "E5", "=", "D5", "C5", "B4", "=", "A4", "=",
                        "B4", "=", "D#5", "F#5", "A5", "=", "=", "=", "E5", "=", "=", "=", "E5", "=", "=", ".",

                        // Part C (Measures 17~24 - 절정부 7음 질풍 피날레 & 자진모리 합주)
                        "E5", "=", "F#5", "G5", "A5", "=", "B5", "=", "C6", "=", "D6", "C6", "B5", "=", "A5", "=",
                        "C6", "=", "D6", "E6", "D6", "=", "C6", "=", "B5", "=", "A5", "G5", "F#5", "=", "E5", "=",
                        "D6", "=", "E6", "F#6", "E6", "=", "D6", "=", "C6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "G5", "=", "A5", "B5", "C6", "=", "D6", "=", "E6", "=", "F#6", "G6", "E6", "=", "=", "=",
                        "A5", "=", "B5", "C6", "D6", "=", "E6", "=", "F#6", "=", "E6", "D6", "C6", "=", "B5", "=",
                        "B5", "=", "C6", "D6", "E6", "=", "D6", "=", "C6", "=", "B5", "A5", "G5", "=", "F#5", "=",
                        "C6", "=", "B5", "A5", "G5", "=", "F#5", "=", "E5", "=", "D#5", "E5", "F#5", "=", "D#5", "=",
                        "E4", "=", "G4", "B4", "E5", "=", "=", "=", "E4", "=", "=", "=", "E4", "=", "=", "."
                    };
                    bass = new string[]
                    {
                        // Part A (1~8)
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "A2", "-", "A2", "A2", "B2", "A2", "G2", "E2",
                        "A2", "-", "A2", "A2", "B2", "-", "B2", "B2", "D3", "-", "B2", "A2", "E2", "E2", "G2", "E2",
                        "E2", "E2", "G2", "E2", "A2", "A2", "B2", "A2", "G2", "E2", "D2", "E2", "G2", "A2", "B2", "G2",
                        "E2", "-", "G2", "A2", "B2", "-", "D3", "B2", "A2", "G2", "E2", "D2", "E2", "E2", "B2", "E2",
                        "E2", "E2", "B2", "E2", "G2", "G2", "D3", "G2", "A2", "A2", "E3", "A2", "B2", "B2", "A2", "G2",
                        "D2", "D2", "A2", "D2", "G2", "G2", "D3", "G2", "E2", "E2", "B2", "E2", "A2", "B2", "D3", "B2",
                        "E2", "-", "E2", "G2", "A2", "-", "A2", "B2", "D3", "-", "B2", "A2", "G2", "E2", "D2", "E2",
                        "G2", "A2", "B2", "D3", "B2", "A2", "G2", "E2", "D2", "E2", "G2", "E2", "E2", "-", "E2", "-",

                        // Part B (9~16)
                        "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "G3", "-", "F#3", "D3",
                        "D2", "-", "D2", "D2", "F#2", "-", "F#2", "F#2", "A2", "-", "A2", "A2", "D3", "-", "C3", "A2",
                        "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "B2", "-", "B2", "B2", "E3", "-", "D3", "B2",
                        "C2", "-", "C2", "C2", "E2", "-", "E2", "E2", "G2", "-", "G2", "G2", "C3", "-", "B2", "G2",
                        "A2", "-", "A2", "A2", "C3", "-", "C3", "C3", "E3", "-", "E3", "E3", "A3", "-", "G3", "E3",
                        "B2", "-", "B2", "B2", "D3", "-", "D3", "D3", "F#3", "-", "F#3", "F#3", "B3", "-", "A3", "F#3",
                        "C2", "C2", "E2", "E2", "G2", "G2", "B2", "B2", "C3", "C3", "D3", "D3", "E3", "E3", "C3", "C3",
                        "D2", "-", "F#2", "-", "A2", "-", "D3", "-", "D2", "-", "A2", "-", "D2", "-", "D2", "-",

                        // Part C (17~24)
                        "E2", "E2", "G2", "G2", "A2", "A2", "B2", "B2", "E3", "E3", "D3", "D3", "C3", "C3", "B2", "B2",
                        "C2", "C2", "E2", "E2", "G2", "G2", "C3", "C3", "B2", "B2", "A2", "A2", "G2", "G2", "E2", "E2",
                        "D2", "D2", "F#2", "F#2", "A2", "A2", "D3", "D3", "C3", "C3", "B2", "B2", "A2", "A2", "F#2", "F#2",
                        "G2", "G2", "B2", "B2", "D3", "D3", "G3", "G3", "F#3", "F#3", "E3", "E3", "D3", "D3", "B2", "B2",
                        "A2", "A2", "C3", "C3", "E3", "E3", "A3", "A3", "G3", "G3", "F#3", "F#3", "E3", "E3", "C3", "C3",
                        "B1", "B1", "D#2", "D#2", "F#2", "F#2", "B2", "B2", "A2", "A2", "G2", "G2", "F#2", "F#2", "D#2", "D#2",
                        "C2", "C2", "E2", "E2", "G2", "G2", "B2", "B2", "C3", "C3", "B2", "B2", "A2", "A2", "F#2", "F#2",
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

                // Determine current chord notes for harmonic reference
                int[] currentChordNotes = Array.Empty<int>();
                if (chords != null && chords.Length > 0)
                {
                    int stepsPerChord = Math.Max(1, totalSteps / chords.Length);
                    int chordIndex = (s / stepsPerChord) % chords.Length;
                    currentChordNotes = ParseChord(chords[chordIndex]);
                }

                // --- Channel 0: Melody (Koto / 가야금 / 해금 / 태평소) ---
                string mStr = melody[s % melody.Length];
                int mNote = NoteToMidi(mStr);
                if (mNote >= 0)
                {
                    // Check ties or rests for duration
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
                    // Dynamic velocity: longer accented notes hit stronger, short passing notes are lighter
                    byte mVel = (byte)(durTicks >= stepTicks * 3 ? 112 : (durTicks >= stepTicks * 2 ? 106 : (s % 4 == 0 ? 102 : 92)));
                    events.Add(new TimedEvent(stepTick, new byte[] { 0x90, (byte)mNote, mVel }, 1));
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
                    // Part A: 한국 고유의 유니즌/옥타브 대선율
                    // Part B & Part C: 코드 구성음 기반 3도/5도 대위 화음 및 카운터 멜로디
                    if (durTicks >= stepTicks * 2 || (s % 4 == 0))
                    {
                        int fNote = (mNote <= 72) ? (mNote + 12) : mNote;
                        if (s >= (totalSteps / 3) && currentChordNotes.Length >= 2)
                        {
                            int targetHarm = currentChordNotes[1] + 12;
                            while (targetHarm < 65) targetHarm += 12;
                            while (targetHarm > 84) targetHarm -= 12;
                            if (Math.Abs(targetHarm - mNote) >= 2)
                            {
                                fNote = targetHarm;
                            }
                        }

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
                }

                // --- Channel 2: Strings Ensemble / Pizzicato Chord Progression ---
                // Render rich multi-voice chord progression (Triads, 7ths) from chords sequence
                if (chords != null && chords.Length > 0 && currentChordNotes.Length > 0)
                {
                    int stepsPerChord = Math.Max(1, totalSteps / chords.Length);

                    if (ch2Prog == 45) // Pizzicato Strings: comping every quarter note (4 steps)
                    {
                        if (s % 4 == 0)
                        {
                            int pDur = stepTicks * 2;
                            byte pVel = (byte)(s % 8 == 0 ? 80 : 70);
                            foreach (int cn in currentChordNotes)
                            {
                                events.Add(new TimedEvent(stepTick, new byte[] { 0x92, (byte)cn, pVel }, 1));
                                events.Add(new TimedEvent(stepTick + pDur, new byte[] { 0x82, (byte)cn, 0 }, 0));
                            }
                        }
                    }
                    else // Sustained String Ensemble Pad: legato hold across the chord duration
                    {
                        if (s % stepsPerChord == 0)
                        {
                            int padDur = stepsPerChord * stepTicks - 6;
                            foreach (int cn in currentChordNotes)
                            {
                                events.Add(new TimedEvent(stepTick, new byte[] { 0x92, (byte)cn, 72 }, 1));
                                events.Add(new TimedEvent(stepTick + padDur, new byte[] { 0x82, (byte)cn, 0 }, 0));
                            }
                        }
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
