using Microsoft.JSInterop;
namespace AudioBrowser.Services;

public class AudioFilesAccess : JSModuleHelper
{
    public AudioFilesAccess(IJSRuntime js) : base(js, "/js/audioFileAccess.js")
    {
    }

    public async ValueTask<JSDirectory> ShowDirectoryPicker()
    {
        return await InvokeAsync<JSDirectory>("showDirectoryPicker");
    }

    public async ValueTask<JSDirectory> ReopenLastDirectory()
    {
        return await InvokeAsync<JSDirectory>("reopenLastDirectory");
    }

    public async ValueTask<JSFile> GetFileAsync(string path)
    {
        throw new NotImplementedException("MediaFilesAccess.GetFileAsync is not yet implemented.");
        //return await InvokeAsync<JSFile>("getFile", path); // needs js function
    }

    public async ValueTask<JSFile[]> GetFilesAsync(JSDirectory directory)
    {
        return await InvokeAsync<JSFile[]>("getFiles", directory.Instance);
    }

    public async ValueTask<byte[]> DecodeAudioFileAsync(JSFile file)
    {
        return await InvokeAsync<byte[]>("decodeAudioFile", file.Name);
    }

    public async ValueTask<IJSObjectReference> PlayAudioFileAsync(JSFile file)
    {
        return await InvokeAsync<IJSObjectReference>("playAudioFile", file.Name);
    }

    public async ValueTask<IJSObjectReference> PlayAudioDataAsync(byte[] data)
    {
        return await InvokeAsync<IJSObjectReference>("playAudioData", data);
    }

    public record JSDirectory(string Name, IJSObjectReference Instance) : IAsyncDisposable
    {
        // When .NET is done with this JSDirectory, also release the underlying JS object
        public ValueTask DisposeAsync() => Instance.DisposeAsync();
    }
    public record JSFile(string Name, long Size, DateTime LastModified, string Artist);
}
