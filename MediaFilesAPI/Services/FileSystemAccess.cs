using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFilesAPI.Services
{
    internal class FileSystemAccess : IAsyncDisposable
    {
        private readonly Task<IJSObjectReference> task;

        protected FileSystemAccess(IJSRuntime jsr, string fileSystemUrl)
        {
            task = jsr.InvokeAsync<IJSObjectReference>("import", fileSystemUrl).AsTask();
        }

        // Invoke exports
        protected async ValueTask InvokeVoidAsync(string identifier, params object[]? args)
        {
            await (await task).InvokeVoidAsync(identifier, args);
        }
        protected async ValueTask<T> InvokeAsync<T>(string identifier, params object[]? args)
        {
            return await (await task).InvokeAsync<T>(identifier, args);
        }

        // On disposal release FileSystemAccess
        public async ValueTask DisposeAsync()
        {
            await (await task).DisposeAsync();
        }
    }
}
