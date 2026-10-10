using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HanlangGame.Engine
{
    public static class SoundSynth
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi)]
        private static extern int mciSendString(string lpstrCommand, StringBuilder? lpstrReturnString, int uReturnLength, IntPtr hwndCallback);

        private static bool _enabled = true;
        private static bool _bgmEnabled = true;
        private static string _targetBgmTrack = "";
        private static readonly object _bgmLock = new();
        private static readonly System.Collections.Generic.Dictionary<string, byte[]> _bgmMidCache = new();
        private static readonly System.Threading.AutoResetEvent _bgmWakeEvent = new(false);
        private static System.Threading.Thread? _bgmThread;
        private static bool _bgmWorkerStarted = false;

        public static void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled) StopBgm();
        }

        public static void SetBgmEnabled(bool enabled)
        {
            _bgmEnabled = enabled;
            if (!enabled) StopBgm();
        }

        public static void StopBgm()
        {
            lock (_bgmLock)
            {
                _targetBgmTrack = "";
            }
            _bgmWakeEvent.Set();
        }

        public static void PlayBgm(string trackName)
        {
            if (!_bgmEnabled || !_enabled) return;

            EnsureBgmWorkerStarted();

            lock (_bgmLock)
            {
                if (_targetBgmTrack == trackName) return;
                _targetBgmTrack = trackName;
            }
            _bgmWakeEvent.Set();
        }

        private static void EnsureBgmWorkerStarted()
        {
            lock (_bgmLock)
            {
                if (_bgmWorkerStarted) return;
                _bgmWorkerStarted = true;
                _bgmThread = new System.Threading.Thread(BgmWorkerLoop)
                {
                    IsBackground = true,
                    Name = "HanlangBgmAudioThread"
                };
                _bgmThread.Start();
            }
        }

        private static void BgmWorkerLoop()
        {
            string currentPlayingTrack = "";
            var sbMode = new StringBuilder(64);

            while (true)
            {
                string targetTrack;
                bool isBgmActive;

                lock (_bgmLock)
                {
                    targetTrack = _targetBgmTrack;
                    isBgmActive = _bgmEnabled && _enabled;
                }

                if (!isBgmActive || string.IsNullOrEmpty(targetTrack))
                {
                    if (!string.IsNullOrEmpty(currentPlayingTrack))
                    {
                        try
                        {
                            mciSendString("stop hanlang_bgm", null, 0, IntPtr.Zero);
                            mciSendString("close hanlang_bgm", null, 0, IntPtr.Zero);
                        }
                        catch { }
                        currentPlayingTrack = "";
                    }
                    _bgmWakeEvent.WaitOne(500);
                    continue;
                }

                // 트랙 전환 시 기존 트랙 정지 및 단일 스레드 컨텍스트에서 새 트랙 오픈
                if (targetTrack != currentPlayingTrack)
                {
                    if (!string.IsNullOrEmpty(currentPlayingTrack))
                    {
                        try
                        {
                            mciSendString("stop hanlang_bgm", null, 0, IntPtr.Zero);
                            mciSendString("close hanlang_bgm", null, 0, IntPtr.Zero);
                        }
                        catch { }
                        currentPlayingTrack = "";
                    }

                    // 잔류 장치 방지
                    try
                    {
                        mciSendString("close hanlang_bgm", null, 0, IntPtr.Zero);
                    }
                    catch { }

                    if (!_bgmMidCache.TryGetValue(targetTrack, out byte[]? midBytes))
                    {
                        midBytes = MidiGenerator.GenerateBgmMid(targetTrack);
                        lock (_bgmLock)
                        {
                            _bgmMidCache[targetTrack] = midBytes;
                        }
                    }

                    string tempPath = Path.Combine(Path.GetTempPath(), $"hanlang_bgm_v6_{targetTrack}.mid");
                    if (!File.Exists(tempPath) || new FileInfo(tempPath).Length != midBytes.Length)
                    {
                        try
                        {
                            File.WriteAllBytes(tempPath, midBytes);
                        }
                        catch { }
                    }

                    try
                    {
                        mciSendString($"open \"{tempPath}\" type sequencer alias hanlang_bgm", null, 0, IntPtr.Zero);
                        mciSendString("play hanlang_bgm from 0", null, 0, IntPtr.Zero);
                        currentPlayingTrack = targetTrack;
                    }
                    catch { }
                }

                // 새 트랙 요청 대기 (이벤트 발생 시 즉시 반응, 200ms 동안 없으면 타임아웃되어 루프 체크)
                bool signaled = _bgmWakeEvent.WaitOne(200);
                if (signaled) continue;

                // 트랙 재생 완료 감지 및 심리스 무한 루프 재연주
                if (!string.IsNullOrEmpty(currentPlayingTrack))
                {
                    try
                    {
                        sbMode.Clear();
                        mciSendString("status hanlang_bgm mode", sbMode, 64, IntPtr.Zero);
                        string mode = sbMode.ToString().Trim().ToLowerInvariant();
                        if (mode == "stopped")
                        {
                            mciSendString("play hanlang_bgm from 0", null, 0, IntPtr.Zero);
                        }
                    }
                    catch { }
                }
            }
        }

        private static byte[] CreateWav(short[] samples, int sampleRate = 22050)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            int byteRate = sampleRate * 2; // 16-bit mono
            int dataChunkSize = samples.Length * 2;

            // RIFF header
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + dataChunkSize);
            writer.Write("WAVE".ToCharArray());

            // fmt chunk
            writer.Write("fmt ".ToCharArray());
            writer.Write(16); // Subchunk1Size
            writer.Write((short)1); // PCM
            writer.Write((short)1); // Mono
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)2); // BlockAlign
            writer.Write((short)16); // BitsPerSample

            // data chunk
            writer.Write("data".ToCharArray());
            writer.Write(dataChunkSize);
            foreach (var s in samples)
            {
                writer.Write(s);
            }

            return ms.ToArray();
        }

        private static void PlayAsync(byte[] wavBytes)
        {
            if (!_enabled || wavBytes == null) return;
            Task.Run(() =>
            {
                try
                {
                    using var ms = new MemoryStream(wavBytes);
                    using var player = new SoundPlayer(ms);
                    player.Play();
                }
                catch
                {
                    // Ignore audio playback exceptions gracefully
                }
            });
        }

        public static void PlayPunch()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.10); // 100ms
            short[] samples = new short[count];
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = 180.0 * (1.0 - t * 0.7); // 180Hz -> 54Hz
                phase += freq / sampleRate;
                double val = Math.Sin(phase * Math.PI * 2) > 0 ? 0.8 : -0.8;
                double env = 1.0 - t; // decay
                samples[i] = (short)(val * env * 12000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        public static void PlayKick()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.15); // 150ms
            short[] samples = new short[count];
            double phase = 0;
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = 240.0 * (1.0 - t * 0.8);
                phase += freq / sampleRate;
                double val = (Math.Sin(phase * Math.PI * 2) > 0 ? 0.7 : -0.7) + (rand.NextDouble() * 0.4 - 0.2);
                double env = Math.Pow(1.0 - t, 1.5);
                samples[i] = (short)(Math.Clamp(val, -1.0, 1.0) * env * 14000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        public static void PlayJump()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.12); // 120ms
            short[] samples = new short[count];
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = 160.0 + t * 350.0; // 160Hz -> 510Hz
                phase += freq / sampleRate;
                double val = Math.Sin(phase * Math.PI * 2) > 0 ? 0.6 : -0.6;
                double env = 1.0 - t * 0.3;
                samples[i] = (short)(val * env * 9000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        public static void PlayHit()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.14);
            short[] samples = new short[count];
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double noise = rand.NextDouble() * 2.0 - 1.0;
                double env = Math.Pow(1.0 - t, 2.0);
                samples[i] = (short)(noise * env * 15000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        public static void PlayCoin()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.18);
            short[] samples = new short[count];
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = (t < 0.5) ? 987.77 : 1318.51; // B5 then E6
                phase += freq / sampleRate;
                double val = Math.Sin(phase * Math.PI * 2) > 0 ? 0.5 : -0.5;
                double env = 1.0 - (t % 0.5) * 1.5;
                samples[i] = (short)(val * Math.Max(0, env) * 10000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        public static void PlayLevelUp()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.40); // 400ms
            short[] samples = new short[count];
            double phase = 0;
            double[] notes = { 523.25, 659.25, 783.99, 1046.50 }; // C5, E5, G5, C6

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                int noteIdx = Math.Min(3, (int)(t * 4));
                double freq = notes[noteIdx];
                phase += freq / sampleRate;
                double val = Math.Sin(phase * Math.PI * 2) > 0 ? 0.6 : -0.6;
                double subT = (t * 4) - noteIdx;
                double env = 1.0 - subT * 0.5;
                samples[i] = (short)(val * env * 11000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        public static void PlaySpecial()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.28);
            short[] samples = new short[count];
            double phase = 0;
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = 300.0 + Math.Sin(t * Math.PI * 6) * 150.0;
                phase += freq / sampleRate;
                double tone = Math.Sin(phase * Math.PI * 2) > 0 ? 0.5 : -0.5;
                double noise = (rand.NextDouble() * 2.0 - 1.0) * 0.4;
                double env = 1.0 - t * 0.6;
                samples[i] = (short)((tone + noise) * env * 12000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 순간 쳐내기 (Just Parry) - 맑고 날카로운 금속음
        public static void PlayParry()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.25);
            short[] samples = new short[count];
            double p1 = 0, p2 = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                p1 += 1760.0 / sampleRate; // A6
                p2 += 2637.0 / sampleRate; // E7
                double tone1 = Math.Sin(p1 * Math.PI * 2);
                double tone2 = Math.Sin(p2 * Math.PI * 2);
                double env = Math.Pow(1.0 - t, 2.5);
                samples[i] = (short)((tone1 * 0.6 + tone2 * 0.4) * env * 16000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 일반 방어 (Guard) - 묵직한 둔탁한 막기 소리
        public static void PlayGuard()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.12);
            short[] samples = new short[count];
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = 120.0 * (1.0 - t * 0.5);
                phase += freq / sampleRate;
                double val = Math.Sin(phase * Math.PI * 2) > 0 ? 0.7 : -0.7;
                double env = 1.0 - t;
                samples[i] = (short)(val * env * 11000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 무기/바람 가르는 소리 (Whoosh)
        public static void PlayWhoosh()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.15);
            short[] samples = new short[count];
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double noise = rand.NextDouble() * 2.0 - 1.0;
                double env = Math.Sin(t * Math.PI); // 부드러운 호형 엔벨로프
                samples[i] = (short)(noise * env * 11000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 항아리/술통 깨지는 파열음 (Shatter)
        public static void PlayShatter()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.22);
            short[] samples = new short[count];
            var rand = new Random();
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                phase += (800.0 - t * 500.0) / sampleRate;
                double tone = Math.Sin(phase * Math.PI * 2) * 0.3;
                double noise = (rand.NextDouble() * 2.0 - 1.0) * 0.7;
                double env = Math.Pow(1.0 - t, 2.0);
                samples[i] = (short)((tone + noise) * env * 16000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 제2 비기: 용비어천격 솟구침 사운드 (Dragon)
        public static void PlayDragon()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.35);
            short[] samples = new short[count];
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = 220.0 + t * t * 750.0; // 220Hz -> 970Hz 급상승
                phase += freq / sampleRate;
                double val = Math.Sin(phase * Math.PI * 2) > 0 ? 0.65 : -0.65;
                double env = 1.0 - t * 0.4;
                samples[i] = (short)(val * env * 14000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 제3 비기: 월인천강포 방출 빔 사운드 (Wave)
        public static void PlayWave()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.38);
            short[] samples = new short[count];
            double p1 = 0, p2 = 0;
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                p1 += (520.0 + Math.Sin(t * 40.0) * 80.0) / sampleRate;
                p2 += (260.0 + Math.Sin(t * 20.0) * 40.0) / sampleRate;
                double tone = (Math.Sin(p1 * Math.PI * 2) + Math.Sin(p2 * Math.PI * 2)) * 0.4;
                double noise = (rand.NextDouble() * 2.0 - 1.0) * 0.25;
                double env = (t < 0.1) ? (t * 10.0) : Math.Pow(1.0 - (t - 0.1) / 0.9, 1.2);
                samples[i] = (short)((tone + noise) * env * 15000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 천둥 번개 굉음 (Thunder)
        public static void PlayThunder()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.45);
            short[] samples = new short[count];
            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double noise = rand.NextDouble() * 2.0 - 1.0;
                double env = (t < 0.05) ? (t * 20.0) : Math.Pow(1.0 - (t - 0.05) / 0.95, 2.5);
                samples[i] = (short)(noise * env * 16500);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        // 회전 낙법 사운드 (Ukemi)
        public static void PlayUkemi()
        {
            int sampleRate = 22050;
            int count = (int)(sampleRate * 0.12);
            short[] samples = new short[count];
            double phase = 0;

            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double freq = 320.0 + (1.0 - t) * 220.0;
                phase += freq / sampleRate;
                double val = Math.Sin(phase * Math.PI * 2) > 0 ? 0.6 : -0.6;
                double env = 1.0 - t;
                samples[i] = (short)(val * env * 10000);
            }

            PlayAsync(CreateWav(samples, sampleRate));
        }

        private static double NoteFreq(string note)
        {
            if (string.IsNullOrEmpty(note) || note == "-" || note == "=" || note == ".") return 0.0;

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
            if (semitone < 0) return 0.0;

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
                int midi = (octave + 1) * 12 + semitone;
                return 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);
            }
            return 0.0;
        }

        public static byte[] GenerateBgmWav(string trackName)
        {
            int sampleRate = 22050;
            float bpm;
            int totalSteps;
            double duty;
            string[] melody;
            string[] bass;
            string[] drums;

            switch (trackName.ToLowerInvariant())
            {
                case "boss":
                    // 「마도 결전」 (168 BPM, D 계면조 / Phrygian 모드, 128 스텝 16비트 배틀 비트)
                    bpm = 168f;
                    totalSteps = 128;
                    duty = 0.125;
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

                case "title":
                    // 「대취타 유생」 (122 BPM, C 우조 대취타 궁중 행진곡, 64 스텝)
                    bpm = 122f;
                    totalSteps = 64;
                    duty = 0.50;
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
                    duty = 0.25;
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
                    duty = 0.50;
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

                default: // "stage" - 「한양 저잣거리 활극」 (148 BPM, E 계면조 / G 평조, 자진모리/휘모리 장단, 128 스텝)
                    bpm = 148f;
                    totalSteps = 128;
                    duty = 0.25;
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

            double stepDuration = 60.0 / bpm / 4.0; // 16th note step duration
            int totalSamples = (int)(sampleRate * stepDuration * totalSteps);
            short[] samples = new short[totalSamples];

            // Chiptune generator states
            double mPhase = 0.0;
            double bPhase = 0.0;
            uint lfsr = 0x7FFF;

            int prevStep = -1;
            double kickTime = 999.0, kickPhase = 0.0;
            double snareTime = 999.0, snarePhase = 0.0;
            double hatTime = 999.0;
            double openHatTime = 999.0;
            double jingTime = 999.0, jingPhase = 0.0;
            double tapTime = 999.0, tapPhase = 0.0;

            for (int i = 0; i < totalSamples; i++)
            {
                double timeSec = (double)i / sampleRate;
                int step = (int)(timeSec / stepDuration) % totalSteps;
                double stepLocalTime = timeSec - ((int)(timeSec / stepDuration) * stepDuration);
                double stepProgress = Math.Clamp(stepLocalTime / stepDuration, 0.0, 1.0);

                // Step transition trigger for authentic percussion
                if (step != prevStep)
                {
                    prevStep = step;
                    string dStr = drums[step % drums.Length];
                    if (dStr.Contains('K')) { kickTime = 0.0; kickPhase = 0.0; }
                    if (dStr.Contains('S')) { snareTime = 0.0; snarePhase = 0.0; }
                    if (dStr.Contains('H')) { hatTime = 0.0; }
                    if (dStr.Contains('O')) { openHatTime = 0.0; }
                    if (dStr.Contains('C')) { jingTime = 0.0; jingPhase = 0.0; }
                    if (dStr.Contains('T')) { tapTime = 0.0; tapPhase = 0.0; }
                }

                // 15-bit Galois LFSR Hardware Noise Generator
                uint bit = (lfsr & 1u) ^ ((lfsr >> 1) & 1u);
                lfsr = (lfsr >> 1) | (bit << 14);
                double noiseSample = ((lfsr & 1u) == 1u) ? 1.0 : -1.0;

                // Channel 1: Pulse Wave (Melody) with Joseon Sigimsae (농현 / 요성 vibrato & articulation)
                string mNoteStr = melody[step % melody.Length];
                double mFreq = NoteFreq(mNoteStr);
                double mVal = 0.0;
                if (mFreq > 0.0)
                {
                    // Sigimsae vibrato onset past 25% of step
                    if (stepProgress > 0.25)
                    {
                        double vib = Math.Sin((stepProgress - 0.25) * Math.PI * 2.0 * (bpm / 24.0));
                        mFreq *= (1.0 + vib * 0.032);
                    }
                    mPhase += mFreq / sampleRate;

                    double pulse = ((mPhase % 1.0) < duty) ? 0.75 : -0.75;
                    double mEnv = (stepProgress < 0.05) ? (stepProgress / 0.05) : (1.0 - 0.22 * stepProgress);
                    if (stepProgress > 0.85)
                    {
                        int nextStep = (step + 1) % totalSteps;
                        if (melody[nextStep] != "=")
                        {
                            mEnv *= Math.Max(0.0, 1.0 - (stepProgress - 0.85) / 0.15);
                        }
                    }
                    mVal = pulse * mEnv;
                }

                // Channel 2: NES 16-Step Quantized Triangle Wave (Bass)
                string bNoteStr = bass[step % bass.Length];
                double bFreq = NoteFreq(bNoteStr);
                double bVal = 0.0;
                if (bFreq > 0.0)
                {
                    bPhase += bFreq / sampleRate;
                    double tri = Math.Abs((bPhase % 1.0) * 4.0 - 2.0) - 1.0;
                    int quantLevel = (int)Math.Round((tri + 1.0) * 7.5);
                    quantLevel = Math.Clamp(quantLevel, 0, 15);
                    double quantTri = (quantLevel / 7.5) - 1.0;

                    double bEnv = Math.Max(0.0, 1.0 - stepProgress * 0.45);
                    if (stepProgress > 0.88)
                    {
                        int nextStep = (step + 1) % totalSteps;
                        if (bass[nextStep] != "=")
                        {
                            bEnv *= Math.Max(0.0, 1.0 - (stepProgress - 0.88) / 0.12);
                        }
                    }
                    bVal = quantTri * bEnv;
                }

                // Channel 3: Joseon Percussion & Noise Drums (Buk, Janggu Chae, Kkwaenggwari, Jing)
                double dVal = 0.0;
                if (kickTime < 0.22)
                {
                    double kFreq = 42.0 + 115.0 * Math.Exp(-kickTime / 0.026);
                    kickPhase += kFreq / sampleRate;
                    double kAmp = Math.Exp(-kickTime / 0.045);
                    dVal += Math.Sin(kickPhase * Math.PI * 2.0) * kAmp * 0.95;
                    kickTime += 1.0 / sampleRate;
                }
                if (snareTime < 0.18)
                {
                    double sAmp = Math.Exp(-snareTime / 0.035);
                    double sNoise = noiseSample * sAmp * 0.65;
                    double sToneFreq = 120.0 + 100.0 * Math.Exp(-snareTime / 0.018);
                    snarePhase += sToneFreq / sampleRate;
                    double sTone = Math.Sin(snarePhase * Math.PI * 2.0) * sAmp * 0.40;
                    dVal += (sNoise + sTone) * 0.72;
                    snareTime += 1.0 / sampleRate;
                }
                if (hatTime < 0.08)
                {
                    double hAmp = Math.Exp(-hatTime / 0.018);
                    dVal += noiseSample * hAmp * 0.32;
                    hatTime += 1.0 / sampleRate;
                }
                if (openHatTime < 0.30)
                {
                    double oAmp = Math.Exp(-openHatTime / 0.085);
                    dVal += noiseSample * oAmp * 0.45;
                    openHatTime += 1.0 / sampleRate;
                }
                if (jingTime < 0.85)
                {
                    double jAmp = Math.Exp(-jingTime / 0.24);
                    double jFreq = 65.0 + 55.0 * Math.Exp(-jingTime / 0.05);
                    jingPhase += jFreq / sampleRate;
                    double jTone = Math.Sin(jingPhase * Math.PI * 2.0) * 0.38;
                    dVal += (noiseSample * 0.60 + jTone) * jAmp * 0.65;
                    jingTime += 1.0 / sampleRate;
                }
                if (tapTime < 0.18)
                {
                    double tAmp = Math.Exp(-tapTime / 0.050);
                    double tFreq = 60.0 + 65.0 * Math.Exp(-tapTime / 0.018);
                    tapPhase += tFreq / sampleRate;
                    dVal += Math.Sin(tapPhase * Math.PI * 2.0) * tAmp * 0.55;
                    tapTime += 1.0 / sampleRate;
                }

                // 3-Channel Master Mixing & Tanh Soft Saturation Limiter
                double mix = (mVal * 0.44) + (bVal * 0.40) + (dVal * 0.38);
                double limited = Math.Tanh(mix / 1.15);
                samples[i] = (short)(limited * 28000.0);
            }

            return CreateWav(samples, sampleRate);
        }
    }
}
