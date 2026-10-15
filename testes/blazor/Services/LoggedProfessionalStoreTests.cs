using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Prescription.Blazor.Services;

namespace Prescription.Blazor.Tests.Services;

/// <summary>Real ProtectedSessionStorage; bUnit answers the sessionStorage JavaScript calls.</summary>
public class LoggedProfessionalStoreTests : BunitContext
{
    private const string Key = "prescription.loggedProfessional";

    private readonly LoggedProfessionalStore _store;

    public LoggedProfessionalStoreTests() =>
        _store = new LoggedProfessionalStore(new ProtectedSessionStorage(JSInterop.JSRuntime,
                                                                         new EphemeralDataProtectionProvider()));

    [Fact]
    public async Task Load_NothingStored_NoProfessional()
    {
        JSInterop.Setup<string?>("sessionStorage.getItem", Key).SetResult(null);

        await _store.LoadAsync();

        Assert.True(_store.IsLoaded);
        Assert.Null(_store.Current);
    }

    [Fact]
    public async Task Set_WritesSessionStorageAndNotifies()
    {
        var setItem = JSInterop.SetupVoid("sessionStorage.setItem", _ => true);
        setItem.SetVoidResult();
        var notified = false;
        _store.Changed += () => notified = true;

        await _store.SetAsync(TestData.Ana);

        Assert.Equal(TestData.Ana, _store.Current);
        Assert.True(notified);
        var call = Assert.Single(setItem.Invocations);
        Assert.Equal(Key, call.Arguments[0]);
        Assert.DoesNotContain("Ana Souza", (string)call.Arguments[1]!); // stored encrypted
    }

    [Fact]
    public async Task Clear_RemovesFromSessionStorage()
    {
        JSInterop.SetupVoid("sessionStorage.setItem", _ => true).SetVoidResult();
        var removeItem = JSInterop.SetupVoid("sessionStorage.removeItem", Key);
        removeItem.SetVoidResult();
        await _store.SetAsync(TestData.Ana);

        await _store.ClearAsync();

        Assert.Null(_store.Current);
        Assert.Single(removeItem.Invocations);
    }

    [Fact]
    public async Task Load_UnreadableValue_IsRemovedAndNoProfessional()
    {
        JSInterop.Setup<string?>("sessionStorage.getItem", Key).SetResult("AAAAAAAAAAAA");
        var removeItem = JSInterop.SetupVoid("sessionStorage.removeItem", Key);
        removeItem.SetVoidResult();

        await _store.LoadAsync();

        Assert.True(_store.IsLoaded);
        Assert.Null(_store.Current);
        Assert.Single(removeItem.Invocations);
    }
}
