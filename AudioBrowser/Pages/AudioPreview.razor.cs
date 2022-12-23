using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using System.Runtime.InteropServices;
using System.Text;
using SkypeVoiceChanger.Effects;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AudioBrowser.Services;

namespace AudioBrowser.Pages
{
    public partial class AudioPreview
    {
        [Parameter, EditorRequired]
        public AudioFilesAccess.JSFile File { get; set; } = default !;
        WaveformSvg? svg;
        IJSObjectReference? playingSource;
        bool effectInProcess;
        string? message;
        CancellationTokenSource disposalCts = new();
        protected override async Task OnInitializedAsync()
        {
            await InitializeWaveformAsync();
        }

        public void Dispose()
        {
            disposalCts.Cancel();
        }

        async Task InitializeWaveformAsync()
        {
            var data = await MediaFiles.DecodeAudioFileAsync(File);
            var sw = new Stopwatch();
            await Task.Run(() =>
            {
                sw.Start();
                svg = GenerateWaveformSvg(data);
                sw.Stop();
            }, disposalCts.Token);
            message = $"{sw.ElapsedMilliseconds:F1}ms";
        }

        async Task TogglePlayingAudioAsync()
        {
            if (playingSource is null)
            {
                // Toggle to play
                playingSource = await MediaFiles.PlayAudioFileAsync(File);
            }
            else
            {
                // Toggle to stop
                await playingSource.InvokeVoidAsync("stop");
                await playingSource.DisposeAsync();
                playingSource = null;
            }
        }

        async Task ChangeVoiceAsync()
        {
            effectInProcess = true;
            var audioData = await MediaFiles.DecodeAudioFileAsync(File);
            var sw = new Stopwatch();
            await Task.Run(() =>
            {
                sw.Start();
                //ChangeVoiceCore(audioData);
                sw.Stop();
            });
            message = $"Effect: {sw.ElapsedMilliseconds:F1}ms";
            effectInProcess = false;
            // Autoplay
            playingSource = await MediaFiles.PlayAudioDataAsync(audioData);
        }

        void ChangeVoiceCore(byte[] samplesData)
        {
            var audioDataFloat = MemoryMarshal.Cast<byte, float>(samplesData);
            var effect = new SuperPitch();
            effect.SliderChanged();
            effect.Init();
            for (var i = 0; i < audioDataFloat.Length; i++)
            {
                effect.OnSample(ref audioDataFloat[i]);
            }

            svg = GenerateWaveformSvg(samplesData);
        }

        record WaveformSvg(string TopPath, string BottomPath);
        WaveformSvg GenerateWaveformSvg(byte[] samplesData)
        {
            // Generates repeated strings like "M 0 38 v 8"
            var audioDataFloat = MemoryMarshal.Cast<byte, float>(samplesData);
            var width = 400;
            var height = 64;
            var topPath = new StringBuilder(15 * width);
            var botPath = new StringBuilder(15 * width);
            var numSamples = audioDataFloat.Length;
            var samplesPerColumn = numSamples / width;
            var minusHalfHeight = -height / 2;
            var midpointHeightStr = ((int)(height * 0.6)).ToString();
            for (var x = 0; x < width; x++)
            {
                var chunk = audioDataFloat.Slice(x * samplesPerColumn, samplesPerColumn);
                var min = chunk[0];
                var max = chunk[0];
                var sampleResolution = 8;
                for (var i = sampleResolution; i < chunk.Length; i += sampleResolution)
                {
                    ref var sample = ref chunk[i];
                    if (sample < min)
                        min = sample;
                    if (sample > max)
                        max = sample;
                }

                topPath.Append('M').Append(x).Append(' ').Append(midpointHeightStr).Append('v').Append((int)(minusHalfHeight * max));
                botPath.Append('M').Append(x).Append(' ').Append(midpointHeightStr).Append('v').Append((int)(minusHalfHeight * min / 2));
            }

            return new WaveformSvg(topPath.ToString(), botPath.ToString());
        }
    }
}