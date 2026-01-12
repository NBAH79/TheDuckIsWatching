
namespace TheDuckIsWatching.Services;

public interface IStorage
{
    public Task<bool> HasPin();
    public Task<bool> CheckPin(string value);
    public void ClearPin();
    public Task Unlock();
    public void Lock();
    public void ClearKey();
    public Task SetPin(string value);
    public Task SetKey();
    public Task<bool> Locked();
    public Task<string> GetKey();
}
