using TheDuckIsWatching.Services;
using Microsoft.Extensions.Configuration;

namespace TheDuckIsWatching;

public partial class SetPinPage : ContentPage
{
    private IGlobal _global;
    private IStorage _storage;
    public SetPinPage(IGlobal global, IStorage storage)
	{
        _global = global;
        _storage = storage;
		InitializeComponent();
	}

    private void help_Clicked(object sender, EventArgs e)
    {
        Shell.Current.DisplayAlert("Справка", CONST.HelpTopics["setpin"], "ОК");
    }

    private async void OnContinueClicked(object? sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(PinEntry.Text))
        {
            await DisplayAlert("Вы не задали ПИН-код", $"Необходимо ввести хотя бы 1 цифру. Для сохранности данных рекомендуется ПИН-код 4-6 цифр", "OK");
        }
        else
        {   
            //запомнить новый пин и сгенерировать ключ для шифрования БД
            await DisplayAlert("Новый ПИН установлен.", $"Для подтверждения войдите в систему с новым ПИН-кодом", "OK");
            await _storage.SetKey();
            await _storage.SetPin(_global.GetHash(PinEntry.Text));
            await Navigation.PopAsync();
        }

    }
}