﻿using Microsoft.JSInterop;
namespace AudioBrowser;

/// <summary>
/// Helper for loading any JavaScript (ES6) module and calling its exports
/// </summary>
public abstract class JSModuleHelper : IAsyncDisposable
{
    private readonly Task<IJSObjectReference> moduleTask;

    // On construction, we start loading the JS module
    protected JSModuleHelper(IJSRuntime js, string moduleUrl)
    {
        moduleTask = js.InvokeAsync<IJSObjectReference>("import", moduleUrl).AsTask();
    }

    // Methods for invoking exports from the module
    protected async ValueTask InvokeVoidAsync(string identifier, params object[]? args)
    {
        await (await moduleTask).InvokeVoidAsync(identifier, args);
    }

    protected async ValueTask<T> InvokeAsync<T>(string identifier, params object[]? args)
    {
        return await (await moduleTask).InvokeAsync<T>(identifier, args);
    }

    // On disposal, we release the JS module
    public async ValueTask DisposeAsync()
    {
        await (await moduleTask).DisposeAsync();
    }
}
