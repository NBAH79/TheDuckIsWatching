using TheDuckIsWatching.Models;
using TheDuckIsWatching.Services;
using TheDuckIsWatching.Utils;

namespace TheDuckIsWatching;

public partial class CardPage : SafePage
{
    private Guid _id;
    private Color _color { get; set; } = Colors.Black;
    private int _colornum = 0;

public CardPage(Guid id, IGlobal global, IStorage storage) : base(global, storage)
    {
        InitializeComponent();
        _id = id;
        _color = Colors.Black;
        _colornum = 0;
    }
    public override async Task OnOpen() //когда страница становится видна
    {
        if (_id != Guid.Empty) //позгрузка карточки
        {
            var card = await _global.GetCardAsync(await _storage.GetKey(), _id);
            titleEntry.Text = card.Title;
            textEditor.Text = card.Text;
            _colornum = card.Color;
            _color = (_colornum < CONST.Colors.Count && _colornum!=0) ? CONST.Colors[_colornum] : Colors.Black;
            colorButton.Color = _color;
            var _bcolor = _colornum == 0 ? Colors.Transparent : CONST.Colors[_colornum];
            SetDynamicGradient(titleEntry, _bcolor);
        }
        else //создание новой карточки
        {
            titleEntry.Text = string.Empty;
            textEditor.Text = string.Empty;
            _color = Colors.Black;
            _colornum = 0;
        }

    } 

    private async void OnApplyButtonClicked(object sender, EventArgs e)
    {
        await OnButtonClicked(applyButton_Clicked, sender, e);
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        textEditor.IsVisible = false;
        await OnButtonClicked(help_Clicked, sender, e);
        textEditor.IsVisible = true;
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        textEditor.IsVisible = false;
        await OnButtonClicked(deleteButton_Clicked, sender, e);
        textEditor.IsVisible = true;
    }

    public override async Task OnClose() { await Task.CompletedTask; } //уход со страницы
    public override void OnBack() { }  //когда нажата кнопочка назад 

    private void OnColorClicked(object sender, EventArgs e)
    {
        _colornum = _colornum == CONST.Colors.Count - 1 ? 0 : _colornum+1; //корректировка диапазона
        _color = _colornum==0 ? Colors.Black : CONST.Colors[_colornum]; //цвет иконки
        var _bcolor = _colornum == 0 ? Colors.Transparent : CONST.Colors[_colornum]; //цвет подсветки названия карточки
        //вручную обновляем элементы экрана
        colorButton.Color = _color;
        SetDynamicGradient(titleEntry, _bcolor);
        ResetTimer();
    }

    private void SetDynamicGradient(VisualElement element, Color color)
    {
        element.Background = new IndexToGradientConverter().MakeBrush(color);
    }
    private async Task<bool> help_Clicked(object? sender, EventArgs e)
    {
        await Shell.Current.DisplayAlert("Справка", CONST.HelpTopics["card"], "ОК");
        return true;
    }
    private void OnEditorTextChanged(object? sender, TextChangedEventArgs e)
    {
        string oldText = e.OldTextValue;
        string newText = e.NewTextValue;
        if (oldText != newText) ResetTimer();
    }

    private void OnTitleTextChanged(object? sender, TextChangedEventArgs e)
    {
        string oldText = e.OldTextValue;
        string newText = e.NewTextValue;
        if (oldText != newText) ResetTimer();
    }

    private async Task<bool> applyButton_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var card = new Item
            {
                ID = _id == Guid.Empty ? Guid.NewGuid() : _id,
                Title = string.IsNullOrEmpty(titleEntry.Text) ? "Без названия" : titleEntry.Text,
                Text = textEditor.Text,
                Color = _colornum,
            };
            if (_id == Guid.Empty)
            {
                await _global.AddCardAsync(await _storage.GetKey(), card);
            }
            else
            {
                await _global.UpdateCardAsync(await _storage.GetKey(), card);
            }
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка!", ex.Message, "OK");
        }
        return true;
    }

    private async Task<bool> deleteButton_Clicked(object? sender, EventArgs e)
    {
        if (textEditor.Text.Length > 0)
        {
            var action = await DisplayPromptAsync($"Карточка содержит данные!", "Для подтверждения введите слово 'Удалить'", "Удалить", "Отмена");
            if (string.Compare(action, "Удалить") != 0) return false;
        }
        try
        {
            await _global.DeleteCardAsync(await _storage.GetKey(), _id);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка!", ex.Message, "OK");
        }
        return true;
    }
}