using TheDuckIsWatching.Services;

namespace TheDuckIsWatching.Utils;

public abstract class SafePage : TimeoutPage
{
    protected readonly IGlobal _global;
    protected readonly IStorage _storage;

    public SafePage(IGlobal global, IStorage storage) : base()
    {
        _global = global;
        _storage = storage;
    }

    protected override bool OnBackButtonPressed()
    {
        OnBack();
        return base.OnBackButtonPressed();
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
        try
        {
            if (await _storage.Locked())
            {
                //если каким-либо образом отобразилась не главная страница, при этом ПИН не введен, авернуться на главную
                await Navigation.PopToRootAsync();
                return;
            }
            else
            {
                await OnOpen();
                base.OnNavigatedTo(e);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка!", ex.Message, "OK");
        }

    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs e)
    {
        await OnClose();
        base.OnNavigatedFrom(e);
    }

    public override async Task OnButtonClicked(Func<object?, EventArgs, Task<bool>> OnClicked, object? sender, EventArgs e)
    {
        try
        {
            await base.OnButtonClicked(OnClicked, sender, e);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка!", ex.Message, "OK");
        }
    }

    public abstract Task OnOpen(); //вход на страницу
    public abstract Task OnClose(); //выход со страницы
    public abstract void OnBack(); //когда нажата кнопка назад
}
