using Prescription.Blazor.Models;
using Prescription.Blazor.Services;

namespace Prescription.Blazor.Tests.Fakes;

/// <summary>In-memory store; <see cref="Stored"/> is what LoadAsync reads (the browser sessionStorage).</summary>
public sealed class FakeLoggedProfessionalStore(Professional? stored = null) : ILoggedProfessionalStore
{
    public Professional? Stored { get; private set; } = stored;
    public Professional? Current { get; private set; }
    public bool IsLoaded { get; private set; }

    public event Action? Changed;

    /// <summary>Already loaded (as after the first render): Current = Stored.</summary>
    public FakeLoggedProfessionalStore Loaded()
    {
        Current = Stored;
        IsLoaded = true;
        return this;
    }

    public Task LoadAsync()
    {
        Current = Stored;
        IsLoaded = true;
        Changed?.Invoke();
        return Task.CompletedTask;
    }

    public Task SetAsync(Professional professional)
    {
        Stored = Current = professional;
        IsLoaded = true;
        Changed?.Invoke();
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        Stored = Current = null;
        IsLoaded = true;
        Changed?.Invoke();
        return Task.CompletedTask;
    }
}
