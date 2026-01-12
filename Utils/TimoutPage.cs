
namespace TheDuckIsWatching.Utils;

public abstract class TimeoutPage : ContentPage
{
    private IDispatcherTimer _inactivityTimer; //для закрытия страницы по истечении таймера
    private long _secondsLeft = CONST.InactivityLimitSeconds;
    private ToolbarItem timeout;
    private string _getSeconds { get => $"{_secondsLeft}"; }  //не факт что на кнопке должно быть просто число

    public TimeoutPage()
    {
        timeout = new ToolbarItem();
        timeout.Clicked += timeout_Clicked; //клик по таймеру сбрасывает таймер

        ToolbarItems.Add(timeout);

        _inactivityTimer = Dispatcher.CreateTimer();
        _inactivityTimer.Interval = TimeSpan.FromSeconds(1);
        _inactivityTimer.Tick += OnTimerTick;

        //ResetTimer(false); //не отработает при возврате на страницу, потому что она уже создана 
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        _secondsLeft--;
        if (_secondsLeft <= 0)
        {
            StopTimer();
            await Navigation.PopToRootAsync(); // возврат на начальную страницу
            await Task.CompletedTask;
        }
        UpdateTimeoutButton();
    }

    private void UpdateTimeoutButton()
    {
        timeout.Text = _inactivityTimer.IsRunning? _getSeconds: string.Empty;
    }

    // сброса таймера в т.ч. при возврате на страницу
    public void ResetTimer(bool stop=false)
    {
        if (stop) StopTimer();
        _secondsLeft = CONST.InactivityLimitSeconds;
        _inactivityTimer.Start();
        UpdateTimeoutButton();
    }

    // остановка таймера, в т.ч. при уходе со страницы
    public void StopTimer()
    {
        _inactivityTimer.Stop();
        UpdateTimeoutButton();
    }

    private async void timeout_Clicked(object? sender, EventArgs e)
    {
        ResetTimer();
        await Task.CompletedTask;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
        ResetTimer(false);  
        base.OnNavigatedTo(e); 
        await Task.CompletedTask;
    }

    protected override async void OnNavigatedFrom(NavigatedFromEventArgs e)
    {
        StopTimer();
        base.OnNavigatedFrom(e);
        await Task.CompletedTask;
    }

    //Переназначен клик на кнопки для того чтоб таймер останавливать
    public virtual async Task OnButtonClicked(Func<object?, EventArgs, Task<bool>> OnClicked, object? sender, EventArgs e)
    {
        StopTimer();
        var reset = await OnClicked(sender, e); //клик может открывать другую страницу, завершение не должно включать таймер
        if (reset) ResetTimer(false);
    }

    //Переназначено Navigation. для того чтоб таймер останавливать
    public async Task NavigateTo(Func<Page>? createPage = null)
    {
        if (createPage == null)
        {
            await Navigation.PopToRootAsync(); //при возврате назад страница удаляется и таймер останавливается
            return;
        }
        StopTimer();
        await Navigation.PushAsync(createPage()); //при переходе на другую страницу старая не удаляется и таймер работает с ивентом.
    }
}
