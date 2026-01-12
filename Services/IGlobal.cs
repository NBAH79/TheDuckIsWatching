using TheDuckIsWatching.Models;

namespace TheDuckIsWatching.Services;

public interface IGlobal
{
    public string GetDBPath { get; }
    public SQLite.SQLiteOpenFlags GetDBFlags { get; }

    public Task CreateTableAsync(string? key);
    public Task InsertManyAsync(string? key, List<Item> items);
    
    public Task<List<Item>> GetAllItemsAsync(string? key);
    public Task<List<Item>> GetItemsAsync(string? key,int offset,int quantity);
    
    public Task DropAllItemsAsync(string? key);
    public void DropDatabaseAsync();
    
    public Task AddCardAsync(string? key, Item card);
    public Task ImportantCardAsync(string? key, Guid id, bool imp);
    public Task<Item> GetCardAsync(string? key, Guid _id);
    public Task UpdateCardAsync(string? key, Item card);
    public Task DeleteCardAsync(string? key, Guid _id);
    
    string GetHash(string inputString);

    public Task<string> CreateBackup(string? key, string filename, string password, Func<float, string> progress);
    public Task<string> RestoreBackup(string? key, FileResult file, string password, Func<float, string> progress);


}
