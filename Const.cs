
namespace TheDuckIsWatching;

public static class CONST
{
    public const string db = "TheDuckIsWatchingSQLite.db3";
    public static string Pin = "Pin";
    public static string Lock = "Lock";
    public static string Key = "Key";
    public static string KeyFormat = "N";
    public static int PinEnterRetryTimeout = 5000;
    public const string DateTimeFormat = "yyyy_MM_dd_HH_mm_ss";
    public const string GuidFormat = "D";

    //таймер защищенных страниц
    public const long InactivityLimitSeconds = 60;
    
    //прогрессивная подгрузка
    public const int ProgressiveListItemsStart = 20;
    public const int ProgressiveListItems = 3;

    //цветная подсветка 
    public static readonly List<Color> Colors = new List<Color>
    {
        Microsoft.Maui.Graphics.Colors.Transparent,
        Color.FromArgb("#F0F0F0"),
        Color.FromArgb("#FFE7EB"),
        Color.FromArgb("#fae8c5"),
        Color.FromArgb("#F6F5E3"),
        Color.FromArgb("#EEF6E3"),
        Color.FromArgb("#E3F6EE"),
        Color.FromArgb("#E3EDF6"),
        Color.FromArgb("#E9E3F6"),
        Color.FromArgb("#F6E3F0")
    };

    //экспорт в ZIP
    private static readonly FilePickerFileType zipFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
             {
                { DevicePlatform.iOS, new[] { "com.pkware.zip-archive" } },  // UTType для iOS
                { DevicePlatform.Android, new[] { "application/zip" } },     // MIME-тип для Android
                { DevicePlatform.WinUI, new[] { ".zip" } },                  // Расширение для Windows
                { DevicePlatform.MacCatalyst, new[] { "com.pkware.zip-archive" } }
             });

    public static readonly PickOptions PickOptions = new PickOptions
    {
        PickerTitle = "Выберите архив",
        FileTypes = zipFileType // ограничить выбор только zip-файлами
    };

    //Кнопки справки на страницах
    public static Dictionary<string, string> HelpTopics = new Dictionary<string, string>
    {
        {"setpin","Установка ПИН-кода для приложения. Вы можете задать ПИН-код для доступа к приложению. Рекомендуемая длина — от 4 до 6 символов."},
        {"list","Список карточек. Здесь можно добавить новую карточку, а также импортировать или экспортировать наборы карточек."},
        {"card","Работа с карточкой. Вы можете редактировать данные и название карточки, сохранять или удалять карточку, или отменить внесённые изменения."}
    };

    
}
