﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;
using OpenTK.Input;

namespace OpenTKAudio.Engine
{
    // this is from here https://forums.tigsource.com/index.php?topic=32474.0
    public class WaveData
    {
        public float durationInSec = 0.0f;
        
        int channels, bits_per_sample, sample_rate;

        byte[] sound_data;

        public byte[] SoundData
        {
            get { return sound_data; }
        }

        public int Channels
        {
            get { return channels; }
        }

        public int BitsPerSample
        {
            get { return bits_per_sample; }
        }

        public int SampleRate
        {
            get { return sample_rate; }
        }

        public WaveData(string filename)
        {
            sound_data = LoadWave(File.Open(filename, FileMode.Open), out channels, out bits_per_sample, out sample_rate);
        }

        private byte[] LoadWave(Stream stream, out int channels, out int bits, out int rate)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (BinaryReader reader = new BinaryReader(stream))
            {
                // RIFF header
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                int riff_chunck_size = reader.ReadInt32();

                string format = new string(reader.ReadChars(4));
                if (format != "WAVE")
                    throw new NotSupportedException("Specified stream is not a wave file.");

                // WAVE header
                string format_signature = new string(reader.ReadChars(4));
                if (format_signature != "fmt ")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int format_chunk_size = reader.ReadInt32();
                int audio_format = reader.ReadInt16();
                int num_channels = reader.ReadInt16();
                int sample_rate = reader.ReadInt32();
                int byte_rate = reader.ReadInt32();
                int block_align = reader.ReadInt16();
                int bits_per_sample = reader.ReadInt16();

                string data_signature = new string(reader.ReadChars(4));
                if (data_signature != "data")
                    throw new NotSupportedException("Specified wave file is not supported.");

                int data_chunk_size = reader.ReadInt32();

                channels = num_channels;
                bits = bits_per_sample;
                rate = sample_rate;

                float lengthInSamples = (float)data_chunk_size * 8f / (float)(channels * bits);
                durationInSec = lengthInSamples / (float)sample_rate;
                
                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }


        public ALFormat SoundFormat
        {
            get { return getSoundFormat(channels, bits_per_sample); }
        }

        private ALFormat getSoundFormat(int channels, int bits)
        {
            switch (channels)
            {
                case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }

        public void dispose()
        {
            sound_data = null;
        }

    }

}
