using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Prescription.Blazor.Models;

namespace Prescription.Blazor.Services;

/// <summary>Professional chosen on the selection screen (instead of login).</summary>
public interface ILoggedProfessionalStore
{
    Professional? Current { get; }

    /// <summary>False until <see cref="LoadAsync"/> runs (browser storage is only reachable after the first render).</summary>
    bool IsLoaded { get; }

    event Action? Changed;

    Task LoadAsync();
    Task SetAsync(Professional professional);
    Task ClearAsync();
}

/// <summary>Kept in the browser sessionStorage (encrypted): valid while the tab stays open.</summary>
public sealed class LoggedProfessionalStore(ProtectedSessionStorage storage) : ILoggedProfessionalStore
{
    private const string Key = "prescription.loggedProfessional";

    public Professional? Current { get; private set; }
    public bool IsLoaded { get; private set; }

    public event Action? Changed;

    public async Task LoadAsync()
    {
        if (IsLoaded)
        {
            return;
        }
        try
        {
            var result = await storage.GetAsync<Professional>(Key);
            Current = result.Success ? result.Value : null;
        }
        catch (CryptographicException)
        {
            // Stored by a previous server instance (other keys): ask for the professional again.
            await storage.DeleteAsync(Key);
            Current = null;
        }
        IsLoaded = true;
        Changed?.Invoke();
    }

    public async Task SetAsync(Professional professional)
    {
        await storage.SetAsync(Key, professional);
        Current = professional;
        IsLoaded = true;
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        await storage.DeleteAsync(Key);
        Current = null;
        IsLoaded = true;
        Changed?.Invoke();
    }
}
