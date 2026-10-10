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
        private static string _currentBgmTrack = "";

        public static void SetEnabled(bool enabled) => _enabled = enabled;
        public static void SetBgmEnabled(bool enabled)
        {
            _bgmEnabled = enabled;
            if (!enabled) StopBgm();
        }

        public static void StopBgm()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentBgmTrack))
                {
                    mciSendString("stop hanlang_bgm", null, 0, IntPtr.Zero);
                    mciSendString("close hanlang_bgm", null, 0, IntPtr.Zero);
                    _currentBgmTrack = "";
                }
            }
            catch { }
        }

        public static void PlayBgm(string trackName)
        {
            if (!_bgmEnabled || !_enabled) return;
            if (_currentBgmTrack == trackName) return;

            Task.Run(() =>
            {
                try
                {
                    StopBgm();

                    string tempPath = Path.Combine(Path.GetTempPath(), $"hanlang_bgm_{trackName}.wav");
                    if (!File.Exists(tempPath))
                    {
                        byte[] wavBytes = GenerateBgmWav(trackName);
                        File.WriteAllBytes(tempPath, wavBytes);
                    }

                    mciSendString($"open \"{tempPath}\" type waveaudio alias hanlang_bgm", null, 0, IntPtr.Zero);
                    mciSendString("play hanlang_bgm repeat", null, 0, IntPtr.Zero);
                    _currentBgmTrack = trackName;
                }
                catch
                {
                    // Fallback gracefully if audio device is unavailable
                }
            });
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

        private static byte[] GenerateBgmWav(string trackName)
        {
            int sampleRate = 22050;
            float bpm = trackName switch
            {
                "boss" => 165f,
                "title" => 125f,
                _ => 142f // stage
            };

            float stepDuration = 60f / bpm / 4f; // 16th note
            int totalSteps = 64; // 4 measures
            int totalSamples = (int)(sampleRate * stepDuration * totalSteps);
            short[] samples = new short[totalSamples];

            // Scales: Pentatonic / Minor martial arts modes
            double[] melodyNotes;
            double[] bassNotes;

            if (trackName == "boss")
            {
                // D minor / Phrygian rapid battle arpeggios
                melodyNotes = new double[]
                {
                    293.66, 349.23, 440.00, 587.33, 440.00, 349.23, 293.66, 261.63,
                    293.66, 349.23, 440.00, 523.25, 587.33, 523.25, 440.00, 349.23,
                    220.00, 293.66, 349.23, 440.00, 415.30, 349.23, 293.66, 220.00,
                    246.94, 293.66, 349.23, 440.00, 587.33, 659.25, 587.33, 440.00,
                    293.66, 349.23, 440.00, 587.33, 659.25, 698.46, 659.25, 587.33,
                    523.25, 440.00, 349.23, 293.66, 261.63, 293.66, 349.23, 440.00,
                    220.00, 261.63, 329.63, 440.00, 392.00, 329.63, 261.63, 220.00,
                    246.94, 293.66, 370.00, 440.00, 587.33, 440.00, 293.66, 220.00
                };
                bassNotes = new double[]
                {
                    146.83, 146.83, 146.83, 146.83, 146.83, 146.83, 174.61, 196.00,
                    146.83, 146.83, 146.83, 146.83, 220.00, 196.00, 174.61, 146.83,
                    110.00, 110.00, 110.00, 110.00, 110.00, 110.00, 130.81, 146.83,
                    123.47, 123.47, 123.47, 123.47, 146.83, 164.81, 146.83, 123.47,
                    146.83, 146.83, 146.83, 146.83, 174.61, 174.61, 196.00, 196.00,
                    130.81, 130.81, 130.81, 130.81, 146.83, 146.83, 174.61, 174.61,
                    110.00, 110.00, 110.00, 110.00, 130.81, 130.81, 146.83, 146.83,
                    123.47, 123.47, 123.47, 123.47, 146.83, 146.83, 110.00, 110.00
                };
            }
            else if (trackName == "title")
            {
                // Heroic pentatonic Joseon march
                melodyNotes = new double[]
                {
                    261.63, 261.63, 329.63, 392.00, 440.00, 440.00, 523.25, 440.00,
                    392.00, 392.00, 329.63, 261.63, 293.66, 293.66, 329.63, 293.66,
                    261.63, 261.63, 329.63, 392.00, 440.00, 523.25, 587.33, 523.25,
                    440.00, 440.00, 392.00, 329.63, 261.63, 261.63, 261.63, 261.63,
                    329.63, 392.00, 440.00, 523.25, 587.33, 523.25, 440.00, 392.00,
                    329.63, 392.00, 440.00, 392.00, 329.63, 293.66, 261.63, 220.00,
                    261.63, 293.66, 329.63, 392.00, 440.00, 523.25, 659.25, 523.25,
                    440.00, 392.00, 329.63, 293.66, 261.63, 261.63, 261.63, 261.63
                };
                bassNotes = new double[]
                {
                    130.81, 130.81, 196.00, 196.00, 220.00, 220.00, 196.00, 196.00,
                    196.00, 196.00, 164.81, 164.81, 146.83, 146.83, 196.00, 146.83,
                    130.81, 130.81, 196.00, 196.00, 220.00, 220.00, 261.63, 220.00,
                    196.00, 196.00, 164.81, 164.81, 130.81, 130.81, 196.00, 130.81,
                    164.81, 164.81, 220.00, 220.00, 261.63, 261.63, 220.00, 196.00,
                    164.81, 164.81, 220.00, 196.00, 164.81, 146.83, 130.81, 110.00,
                    130.81, 146.83, 164.81, 196.00, 220.00, 261.63, 329.63, 261.63,
                    220.00, 196.00, 164.81, 146.83, 130.81, 130.81, 196.00, 130.81
                };
            }
            else // "stage" - Upbeat Joseon martial arts theme
            {
                melodyNotes = new double[]
                {
                    329.63, 329.63, 392.00, 440.00, 493.88, 440.00, 392.00, 329.63,
                    440.00, 493.88, 587.33, 659.25, 587.33, 493.88, 440.00, 392.00,
                    659.25, 659.25, 587.33, 493.88, 440.00, 493.88, 587.33, 493.88,
                    440.00, 392.00, 329.63, 392.00, 440.00, 392.00, 329.63, 293.66,
                    329.63, 392.00, 440.00, 493.88, 587.33, 493.88, 440.00, 392.00,
                    329.63, 392.00, 440.00, 493.88, 587.33, 659.25, 587.33, 493.88,
                    440.00, 493.88, 587.33, 493.88, 440.00, 392.00, 329.63, 293.66,
                    329.63, 329.63, 392.00, 440.00, 493.88, 440.00, 392.00, 329.63
                };
                bassNotes = new double[]
                {
                    164.81, 164.81, 164.81, 164.81, 220.00, 220.00, 196.00, 164.81,
                    220.00, 220.00, 246.94, 246.94, 293.66, 246.94, 220.00, 196.00,
                    164.81, 164.81, 164.81, 164.81, 220.00, 220.00, 293.66, 246.94,
                    220.00, 196.00, 164.81, 196.00, 220.00, 196.00, 164.81, 146.83,
                    164.81, 164.81, 196.00, 220.00, 293.66, 246.94, 220.00, 196.00,
                    164.81, 164.81, 220.00, 246.94, 293.66, 329.63, 293.66, 246.94,
                    220.00, 246.94, 293.66, 246.94, 220.00, 196.00, 164.81, 146.83,
                    164.81, 164.81, 196.00, 220.00, 246.94, 220.00, 196.00, 164.81
                };
            }

            double mPhase = 0;
            double bPhase = 0;
            var rand = new Random(42);

            for (int i = 0; i < totalSamples; i++)
            {
                float timeSec = (float)i / sampleRate;
                int step = (int)(timeSec / stepDuration) % totalSteps;
                float stepT = (timeSec % stepDuration) / stepDuration;

                // 1. Ch 1: Melody (Square wave with 25% or 50% duty cycle)
                double mFreq = melodyNotes[step % melodyNotes.Length];
                mPhase += mFreq / sampleRate;
                double mVal = ((mPhase % 1.0) < 0.4) ? 0.35 : -0.35;
                double mEnv = Math.Max(0.0, 1.0 - stepT * 0.4);

                // 2. Ch 2: Bass (Triangle wave)
                double bFreq = bassNotes[step % bassNotes.Length];
                bPhase += bFreq / sampleRate;
                double bVal = (Math.Abs((bPhase % 1.0) * 4.0 - 2.0) - 1.0) * 0.35;
                double bEnv = Math.Max(0.0, 1.0 - stepT * 0.3);

                // 3. Ch 3: Percussion (Noise snare / hi-hat)
                double nVal = 0;
                if (step % 4 == 2) // Snare on 2 and 4
                {
                    double snareEnv = Math.Max(0.0, 1.0 - stepT * 3.0);
                    nVal = (rand.NextDouble() * 2.0 - 1.0) * 0.30 * snareEnv;
                }
                else if (step % 2 == 0) // Closed hi-hat
                {
                    double hatEnv = Math.Max(0.0, 1.0 - stepT * 6.0);
                    nVal = (rand.NextDouble() * 2.0 - 1.0) * 0.12 * hatEnv;
                }

                double mixed = (mVal * mEnv + bVal * bEnv + nVal) * 11500;
                samples[i] = (short)Math.Clamp(mixed, -32768, 32767);
            }

            return CreateWav(samples, sampleRate);
        }
    }
}
