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
using System.Diagnostics;
using AudioBrowser.Services;

namespace AudioBrowser.Pages
{
    public partial class AudioPlayer : IDisposable
    {
        [Parameter, EditorRequired]
        public AudioFilesAccess.JSFile File { get; set; } = default!;
        IJSObjectReference? playingSource;
        CancellationTokenSource disposalCts = new();

        //protected override async Task OnInitializedAsync()
        //{
            
  
        //}
        public void Dispose()
        {
            disposalCts.Cancel();
            GC.SuppressFinalize(this);
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
    }
}