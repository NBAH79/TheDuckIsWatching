using TheDuckIsWatching.Models;
using TheDuckIsWatching.Services;
using Microsoft.Extensions.Configuration;

namespace TheDuckIsWatching;

public partial class MainPage : ContentPage
{
    private IGlobal _global;
    private IStorage _storage;
    private IConfiguration _configuration;

    public MainPage(IConfiguration configuration, IGlobal global, IStorage storage)
    {
        _global = global;
        _configuration = configuration;
        _storage = storage;
        //this.Appearing += async (o, e) => await OnPageAppearing(o, e);
        InitializeComponent();
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
        _storage.Lock();
        //если пин не был задан - задать
        if (!await _storage.HasPin())
        {
            await DisplayAlert("Добро пожаловать!", $"Для защиты вашей информации, пожалуйста, создайте ПИН-код.", "OK");
            await Navigation.PushAsync(new SetPinPage(_global, _storage));
        }
        base.OnNavigatedTo(e);
    }
    private void info_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AboutPage());
    }

    private void OnPinChanged(object sender, TextChangedEventArgs e) =>
        Continue.IsEnabled = !String.IsNullOrWhiteSpace(PinEntry.Text);

    private async void OnContinueClicked(object? sender, EventArgs e)
    {
        //если пин не был задан и не введен введен показать ошибку
        if (String.IsNullOrEmpty(PinEntry.Text) || !await _storage.CheckPin(_global.GetHash(PinEntry.Text)))
        {
            Body_.IsVisible = false;
            Spinner_.IsVisible = true;
            Spinner_.IsRunning = true;
            Clear.IsEnabled = false;
            Help.IsEnabled = false;
            await Task.Delay(CONST.PinEnterRetryTimeout); //чтоб нельзя было спамить пин и подобрать
            Help.IsEnabled = true;
            Clear.IsEnabled = true;
            Spinner_.IsRunning = false;
            Spinner_.IsVisible = false;
            Body_.IsVisible = true;
            await DisplayAlert("Ошибка!", $"Неверный ПИН-код", "OK");
            return;
        }

        await _storage.Unlock();

        //скрыть клавиатуру
        PinEntry.IsEnabled = false;
        PinEntry.IsEnabled = true;

        //стереть введенный ключ
        PinEntry.Text = null;

        //перейти на страницу списка
        await Navigation.PushAsync(new ListPage(_global, _storage));
    }


    private async void OnClearClicked(object? sender, EventArgs e)
    {
        //надо ввести слово "Очистить" для подтверждения
        var action = await DisplayPromptAsync($"Внимание!", "Сброс ПИН-кода приведёт к безвозвратному удалению всех карточек.\r\nДля подтверждения введите слово \"Очистить\".", "Очистить", "Отмена");
        if (string.Compare(action, "Очистить") != 0) return;

        //удалить записи и ключ расшифровки
        _storage.Lock();
        //await _global.DropAllItemsAsync(await _storage.GetKey()); //если по какой то причине бд не удалена, а ключ изменится, то не удалится
        _global.DropDatabaseAsync();

        _storage.ClearKey();
        _storage.ClearPin();

        //перейти на страницу ввода нового пин-кода
        await Navigation.PushAsync(new SetPinPage(_global, _storage));

    }
}
