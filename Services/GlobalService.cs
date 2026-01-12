using ICSharpCode.SharpZipLib.Zip;
using SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheDuckIsWatching.Models;

namespace TheDuckIsWatching.Services;

internal class GlobalService : IGlobal
{
    public GlobalService() { }

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;

    public string GetDBPath
    {
        get => Path.Combine(FileSystem.AppDataDirectory, CONST.db);
    }

    public SQLite.SQLiteOpenFlags GetDBFlags
    {
        get => Flags;
    }
    protected SQLiteAsyncConnection GetContext(string? key)
    {
        var connectionString = new SQLiteConnectionString(GetDBPath, true, key);
        return new SQLiteAsyncConnection(connectionString);

    }

    public async Task CreateTableAsync(string? key)
    {
        var context = GetContext(key);
        var created = await context.CreateTableAsync<Item>();
    }

    public async Task InsertManyAsync(string? key, List<Item> items)
    {
        var context = GetContext(key);
        await context.InsertAllAsync(items);
    }

    public async Task<List<Item>> GetAllItemsAsync(string? key)
    {
        var context = GetContext(key);
        return await context.Table<Item>().ToListAsync();
    }

    public async Task<List<Item>> GetItemsAsync(string? key,int offset,int quantity)
    {
        var context = GetContext(key);
        return await context.Table<Item>().Skip(offset).Take(quantity).ToListAsync();
    }

    public async Task DropAllItemsAsync(string? key)
    {
        var context = GetContext(key);
        if (context != null) await context.DropTableAsync<Item>(); //на случай если нет таблицы в БД
    }

    public void DropDatabaseAsync()
    {
        var path = GetDBPath;
        if (File.Exists(path)) File.Delete(path); // просто стереть файл базы, потому что старая была со старым ключем
    }

    public async Task AddCardAsync(string? key, Item card)
    {
        var context = GetContext(key);
        await context.InsertAsync(card);
    }

    public async Task ImportantCardAsync(string? key, Guid id, bool imp) //ставит и снимает звездочку "важно"
    {
        var context = GetContext(key);
        string sql = "UPDATE Item SET IsImportant = ? WHERE ID = ?";
        await context.ExecuteAsync(sql, imp ? 1 : 0, id);
    }

    public async Task<Item> GetCardAsync(string? key, Guid _id)
    {
        var context = GetContext(key);
        return await context.Table<Item>().Where(x => x.ID == _id).FirstOrDefaultAsync() ?? throw new Exception($"Не найдена карточка {_id}");
    }

    public async Task UpdateCardAsync(string? key, Item card)
    {
        var context = GetContext(key);
        string query = "UPDATE Item SET Text = ?, Title = ?, Color = ? WHERE ID = ?";
        int result = await context.ExecuteAsync(query, card.Text, card.Title, card.Color, card.ID);
    }

    public async Task DeleteCardAsync(string? key, Guid _id)
    {
        var context = GetContext(key);
        var i = await context.Table<Item>().Where(x => x.ID == _id).FirstOrDefaultAsync() ?? throw new Exception($"Не найдена карточка {_id}");
        await context.DeleteAsync(i);
    }
    public string GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)));
    }

    public async Task<string> CreateBackup(string? key, string filename, string password, Func<float, string> progress)
    {
        var context = GetContext(key);
        var items = await context.Table<Item>().ToListAsync();
        var counter = 0;

        //чтоб не было путаницы с часовыми поясами
        var date = DateTime.UtcNow;
        try
        {
            using (FileStream fsOut = File.Create(filename))
            using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
            {
                zipStream.Password = password;
                zipStream.SetLevel(5); // среднее сжатие

                foreach (var item in items)
                {
                    // формируем в памяти байтовый массив, не пишем временные файлы
                    // так как нет ограничения на вводимые символы, cериализуем данные в JSON
                    // символы не из допустимого диапазона станут нечитаемые в виде \xxxx
                    // но это не проблема
                    string jsonString = JsonSerializer.Serialize(item);
                    byte[] jsonData = Encoding.UTF8.GetBytes(jsonString);

                    // запись для JSON-файла внутри архива с расширением txt для удобного просмотра
                    var entry = new ZipEntry($"{item.ID.ToString(CONST.GuidFormat)}.txt")
                    {
                        DateTime = date,
                        Size = jsonData.Length,
                        AESKeySize = string.IsNullOrEmpty(password) ? 0 : 256 // шифрование AES-256 как у RAR (не читается Windows ниже 11)
                    };

                    zipStream.PutNextEntry(entry);
                    zipStream.Write(jsonData, 0, jsonData.Length);
                    counter++;
                    progress(items.Count / counter); //связь с элементом отображения прогресса
                }
                zipStream.CloseEntry();

                zipStream.Finish();
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        return $"Собрано {counter} из {items.Count} записей";
    }

    public async Task<string> RestoreBackup(string? key, FileResult file, string password, Func<float, string> progress)
    {
        var context = GetContext(key);
        var counter = 0;

        try
        {
            using (Stream fsIn = await file.OpenReadAsync())
            {
                //await context.RunInTransactionAsync(transaction => //реализовано библиотекой SQLite-net-pcl
                //{
                //    try
                //    {
                        using (ZipFile zipFile = new ZipFile(fsIn))
                        {
                            zipFile.Password = password;

                            // перебираем все записи в архиве
                            foreach (ZipEntry entry in zipFile)
                            {
                                // gроверяем, что это файл, а не папка, и что он имеет расширение .txt и имя Guid
                                if (entry.IsFile && Guid.TryParse(Path.GetFileNameWithoutExtension(entry.Name), out Guid id) && entry.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                                {
                                    using (Stream zipStream = zipFile.GetInputStream(entry))
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        zipStream.CopyTo(ms); //синхронный метод внутри транзакции

                                        // десериализуем отдельный объект и добавляем в БД с новым гуидом
                                        var jsonBytes = ms.ToArray();
                                        var item = JsonSerializer.Deserialize<Item>(Encoding.UTF8.GetString(jsonBytes));
                                        if (item != null)
                                        {
                                            item.ID = Guid.NewGuid();
                                            //transaction.Insert(item); //синхронный метод
                                            await context.InsertAsync(item);
                                            counter++;
                                        }
                                        progress(counter); //не понятно сколько всего, показываем количество обработанных
                                    }
                                }
                            }

                        }
                //    }
                //    catch (Exception ex)
                //    {
                //        transaction.Rollback();
                //        return ex.Message;
                //    }
                //});
            }
        }
        catch (Exception ex)
        {
            return ex.Message; //в том числе и неверный пароль
        }
        return $"Добавлено {counter} записей";
    }


}
