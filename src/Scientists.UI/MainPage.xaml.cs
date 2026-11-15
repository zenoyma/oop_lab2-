using Scientists.Core;

namespace Scientists.UI;

// Макет головного вікна: кнопки й меню вже є, але логіки ще немає (з'явиться в наступних етапах)
public partial class MainPage : ContentPage
{
    private static readonly string[] StrategyNames = { "SAX API", "DOM API", "LINQ to XML" };

    public MainPage()
    {
        InitializeComponent();

        for (int i = 0; i < StrategyNames.Length; i++)
        {
            strategiesPanel.Add(new RadioButton { Content = StrategyNames[i], GroupName = "strategy", IsChecked = i == 0 });
        }
        foreach (var (_, caption) in Scientist.SearchAttributes)
        {
            filtersPanel.Add(new Picker { Title = caption, WidthRequest = 260, HorizontalOptions = LayoutOptions.Start });
        }
        filesLabel.Text = "XML: не відкрито     XSL: не відкрито";
    }

    public void OnOpenXmlClicked(object? sender, EventArgs e) => NotYet();
    public void OnOpenXslClicked(object? sender, EventArgs e) => NotYet();
    public void OnOpenXmlDriveClicked(object? sender, EventArgs e) => NotYet();
    public void OnOpenXslDriveClicked(object? sender, EventArgs e) => NotYet();
    public void OnUploadSourcesClicked(object? sender, EventArgs e) => NotYet();
    public void OnSearchClicked(object? sender, EventArgs e) => NotYet();
    public void OnClearClicked(object? sender, EventArgs e) => NotYet();
    public void OnSaveHtmlClicked(object? sender, EventArgs e) => NotYet();
    public void OnSaveXmlClicked(object? sender, EventArgs e) => NotYet();
    public void OnSaveHtmlDriveClicked(object? sender, EventArgs e) => NotYet();
    public void OnSaveXmlDriveClicked(object? sender, EventArgs e) => NotYet();

    public async void OnHelpClicked(object? sender, EventArgs e) => await DisplayAlert("Довідка",
        "Лабораторна робота 2*, тема 19 «Кадри науковців (Зарплата)».\n" +
        "Автор: Бак Олександр, К-25.\n\n" +
        "Програма шукає науковців у XML-файлі трьома способами (SAX, DOM, LINQ to XML)\n" +
        "і зберігає результат у XML або HTML (через XSL).", "OK");

    // Викликається з MauiProgram перед закриттям вікна. true - можна закривати
    public Task<bool> OnClosing()
        => DisplayAlert("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");

    private void NotYet() => statusLabel.Text = "Ще не реалізовано";
}
