using TheDuckIsWatching.Models;
using TheDuckIsWatching.Services;
using TheDuckIsWatching.Utils;

namespace TheDuckIsWatching;

public partial class ListPage : SafePage
{
    private List<Item> items = new List<Item>();
    public ListPage(IGlobal global, IStorage storage) : base(global, storage)
	{
        InitializeComponent();
	}

    public override async Task OnOpen() //когда страница становится видна
    {
        await Task.CompletedTask;
    }

    protected override async void OnAppearing()
    {
        //бублик и чтоб не было визуального лага при входе
        base.OnAppearing();
        loadingIndicator.IsRunning = true;
        loadingIndicator.IsVisible = true;
        listView.IsVisible = false;
        //items.Clear();
        //listView.InvalidateMeasure();
        await LoadItems();
        loadingIndicator.IsRunning = false;
        loadingIndicator.IsVisible = false;
        listView.IsVisible = true;
    }

    //protected override async void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    items.Clear();
    //    listView.InvalidateMeasure();
    //    loadingIndicator.IsRunning = true;
    //    loadingIndicator.IsVisible = true;
    //    listView.IsVisible = false;
    //}

    private async Task OnChoose(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItem = e.CurrentSelection.FirstOrDefault() as Item;
        if (selectedItem == null) return;
        //if (e.Item == null) return;
        //var i = e.Item as Item;
        //((ListView)sender!).SelectedItem = null;// убираем выделение? чтобы элемент можно было нажать снова
        ((CollectionView)sender!).SelectedItem = null;
        //await NavigateTo(() => new CardPage(selectedItem!.ID, _global, _storage));
        await OpenCard(selectedItem!.ID);
    }
    private async void OnStarClicked(object sender, EventArgs e)
    {
        var button = (ImageButton)sender!;
        if (button.CommandParameter is Item item)
        {
            item.IsImportant = !item.IsImportant;
            var key = await _storage.GetKey();
            await _global.ImportantCardAsync(await _storage.GetKey(), item.ID, item.IsImportant);
            await button.FadeTo(0, 100);
            if (item.IsImportant)
            {
                button.Source = "star_gold.svg";
            }
            else
            {
                button.Source = "star_gray.svg";
            }
            await button.FadeTo(1, 200);
        }
        ResetTimer();
    }

    public override async Task OnClose() //уход со страницы
    {
        items.Clear();
        await Task.CompletedTask;

    }
    public override void OnBack() { }  //когда нажата кнопочка назад


    private async void OnAddButtonClicked(object sender, EventArgs e) =>
        await OnButtonClicked(addButton_Clicked, sender, e);

    private async void OnHelpClicked(object sender, EventArgs e) =>
        await OnButtonClicked(help_Clicked, sender, e);

    private async void OnExportClicked(object sender, EventArgs e) =>
        await OnButtonClicked(export_Clicked, sender, e);

    private async void OnImportClicked(object sender, EventArgs e) =>
        await OnButtonClicked(import_Clicked, sender, e);

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        await OnChoose(sender, e);

    private async void OnLoadMoreItems(object sender, EventArgs e)
    {
        await LoadItems();
    }

    public async Task LoadItems()
    {
        var key = await _storage.GetKey();
        await _global.CreateTableAsync(key); //Если уже создана таблица в БД, то ничего не будет
        //Подгружаем большое количество элементов в первый раз
        var nextItems = await _global.GetItemsAsync(key, items.Count, items.Count>0 ? CONST.ProgressiveListItems : CONST.ProgressiveListItemsStart);
        items.AddRange(nextItems);
        listView.ItemsSource = items;
        listView.InvalidateMeasure();
        export.IsEnabled = items.Count > 0; //если нечего экспортировать
    }

    private async Task<bool> help_Clicked(object? sender, EventArgs e)
    {
        await Shell.Current.DisplayAlert("Справка", CONST.HelpTopics["list"], "ОК");
        return true;
    }

    private async Task OpenCard(Guid id)
    {
        
        items.Clear();
        //чтоб не было визуального лага при возвращении
        loadingIndicator.IsRunning = true;
        loadingIndicator.IsVisible = true;
        listView.IsVisible = false;

        //для обновления списка при возвращении
        listView.ItemsSource = new List<Item>();
        listView.InvalidateMeasure();
        await NavigateTo(() => new CardPage(id, _global, _storage));
    }

    private async Task<bool> addButton_Clicked(object? sender, EventArgs e)
    {
        if (await _storage.Locked()) return true;

        //var action = await DisplayPromptAsync($"Добавление карточки", "Название", "Добавить", "Отмена");
        //if (String.IsNullOrWhiteSpace(action)) return true; //Отмена или пусто
        //var card = new Item
        //{
        //    ID = Guid.NewGuid(),
        //    Title = action,
        //    Text = string.Empty,
        //};
        //try
        //{
        //    await _global.AddCardAsync(await _storage.GetKey(), card);
            await OpenCard(Guid.Empty);
        //    return false; //не включать таймер
        //}
        //catch (Exception ex)
        //{
        //    await DisplayAlert("Ошибка!", ex.Message, "OK");
        //}
        return true;
    }

    private async Task<bool> export_Clicked(object? sender, EventArgs e)
    {
        await Export();//возвращает статус операции
        return true;
    }

    private async Task<bool> Export()
    {

        var password = await DisplayPromptAsync($"Введите пароль для архива", "Рекомендуется использовать пароль длиной не менее 20 символов.\r\nОбратите внимание: удалённый файл не означает надёжно стёртый!", "Сохранить", "Отмена");
        if (password == null)
        {
            return false; //Отмена
        }

        if (password.Length == 0) //Пустой пароль не примет зип, дело вовсе не в том что утки любят пустые пароли. 
        {
            await DisplayAlert("Ошибка!", $"Вы не ввели пароль!", "OK");
            return false;
        }

        //Так как без дополнительных разрешений нельзя записывать на диск, и тем более шариться по папкам,
        //решено подготовить архив к отправке туда, куда пользователь укажет.
        //Запишется архив в файл во внутреннем хранилище приложения, ОС получит на него ссылку. Это можно без разрешений.
        //Временный путь в кеше, он настроен в file_paths.xml и в манифесте
        var filename = Path.Combine(FileSystem.CacheDirectory, $"TheDuckIsWatching_{DateTime.UtcNow.ToString(CONST.DateTimeFormat)}.zip");
        var confirm = await DisplayAlert("Подготовка к отправке", await _global.CreateBackup(await _storage.GetKey(), filename, password, (x) => { return ""; }), "Отправить", "Отмена");
        if (!confirm)
        {
            return false;
        }
        ;
#if DEBUG
        // Дебаг проверка: файл реально создался?
        if (!File.Exists(filename))
        {
            await Shell.Current.DisplayAlert("Ошибка", "Файла нет по пути " + filename, "ОК");
            return false;
        }

        // Дебаг проверка: размер файла больше 0?
        var info = new FileInfo(filename);
        if (info.Length == 0)
        {
            await Shell.Current.DisplayAlert("Ошибка", "Файл пустой", "ОК");
            return false;
        }
#endif

        // известно, что евреи не используют Андройд, потому что их отпугивает системное меню "Поделиться"
        // на некоторых платформах этот вызов должен быть обязательно из главного потока
        await MainThread.InvokeOnMainThreadAsync(async () => await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Отправить",
            File = new ShareFile(filename, "application/zip")
        }));

        //удалить из кэша, но это надо делать с диалогом чтоб поставить выполнение на паузу
        //потому что RequestAsync завершается сразу и будет ошибка что расшаренного файла не существует.
        //пользователь вернется после расшаривания и нажмет Подтвердить и тогда только файл удалится
        await Shell.Current.DisplayAlert("Безопасная очистка", "Файл отправлен. Локальная копия будет удалена из кэша для обеспечения безопасности.", "Подтвердить");
        File.Delete(filename);
        return true;
    }

    private async Task<bool> import_Clicked(object? sender, EventArgs e)
    {
        await Import(); //возвращает статус операции
        return true;
    }
    private async Task<bool> Import()
    {
        // на некоторых платформах этот вызов должен быть обязательно из главного потока
        var file = await MainThread.InvokeOnMainThreadAsync(async () => await FilePicker.Default.PickAsync(CONST.PickOptions));
        if (file == null)
        {
            await DisplayAlert("Отмена", $"Файл не был выбран.", "OK");
            return false;
        }

        var password = await DisplayPromptAsync($"Введите пароль архива", "Записи добавятся к существующим", "Импортировать", "Отмена");
        if (password == null) return false; //Отмена
        await DisplayAlert("Результат операции", await _global.RestoreBackup(await _storage.GetKey(), file, password, (x) => { return ""; }), "OK");
        await LoadItems();
        return true;
    }
}