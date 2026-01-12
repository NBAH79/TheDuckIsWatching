using ICSharpCode.SharpZipLib.Zip;
using SQLite;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheDuckIsWatching.Models;
public class Item
{
    [PrimaryKey]
    public Guid ID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsImportant { get; set; } = false; //звездочка "важное"
    public int Color { get; set; } = 0; //подсветка цветом
    
    // тут можно будет добавить
    // в будущем может понадобится флажок приоритета (причем тут? а смотря как приложение использовать)
    // в будущем может понадобится ручная сортировка (не понятно надо ли, и куда кнопки ставить или как драгать)

    // т.к. не влияет на выборку прогрессивной подгрузки, может быть реализовано другой таблицей:
    // удаление при подборе ПИН-кода более чем N раз
    // удаление по истечении даты
    // группы 

    public override string ToString()
    {
        return Title;
    }
}
