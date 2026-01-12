
namespace TheDuckIsWatching.Services;

internal class StorageService : IStorage
{
    private async Task SaveDataSecurely(string key, string value) => await SecureStorage.SetAsync(key, value);
    private async Task<string?> ReadDataSecurely(string key) => await SecureStorage.GetAsync(key);
    private void ClearDataSecurely(string key) => SecureStorage.Remove(key);
    //
    public async Task SetPin(string value) => await SaveDataSecurely(CONST.Pin, value);
    public async Task<bool> HasPin() => await ReadDataSecurely(CONST.Pin) != null;
    public void ClearPin() => ClearDataSecurely(CONST.Pin);
    public async Task<bool> CheckPin(string value) => await ReadDataSecurely(CONST.Pin) == value;

    public async Task Unlock() => await SaveDataSecurely(CONST.Lock, DateTime.UtcNow.ToString("ddMMyyyy:HHmmss"));
    public void Lock() => ClearDataSecurely(CONST.Lock);
    public async Task<bool> Locked() => await ReadDataSecurely(CONST.Lock) == null;

    public void ClearKey() => ClearDataSecurely(CONST.Key);
    public async Task SetKey() => await SaveDataSecurely(CONST.Key, Guid.NewGuid().ToString(CONST.KeyFormat));
    public async Task<string> GetKey() => await ReadDataSecurely(CONST.Key) ?? String.Empty;
}

