using Scientists.Core;
using Scientists.Core.Storage;

namespace Scientists.UI;

// Головне вікно: показує дані і передає дії користувача контролеру, саме нічого не шукає і не зберігає
public partial class MainPage : ContentPage
{
    private const string AnyValue = "(будь-яке)";

    // тут лежать збережені результати, журнал, client_secret.json і токен Google
    private static readonly string FilesFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Scientists19");

    private readonly ScientistsController controller = new(
        new LocalFileStorage(), null, Path.Combine(FilesFolder, "log.txt"));
    private readonly Dictionary<string, Picker> filters = new();   // атрибут -> список його значень
    private int strategyIndex;                                     // обраний спосіб аналізу (SAX, DOM, LINQ)

    public MainPage()
    {
        InitializeComponent();

        List<string> names = controller.GetStrategyNames();
        for (int i = 0; i < names.Count; i++)
        {
            int index = i;   // своя копія, інакше всі лямбди побачать останнє i
            var radio = new RadioButton { Content = names[i], GroupName = "strategy", IsChecked = i == 0 };
            radio.CheckedChanged += (_, e) => { if (e.Value) strategyIndex = index; };
            strategiesPanel.Add(radio);
        }
        foreach (var (attribute, caption) in controller.GetSearchAttributes())
        {
            var picker = new Picker { Title = caption, WidthRequest = 260, HorizontalOptions = LayoutOptions.Start };
            filtersPanel.Add(picker);
            filters[attribute] = picker;
        }
        RefreshFilters();
    }

    public async void OnOpenXmlClicked(object? sender, EventArgs e)
    {
        string? path = await PickLocalFile(".xml");
        if (path != null) await OpenSource(Place.File, path, isXml: true);
    }

    public async void OnOpenXslClicked(object? sender, EventArgs e)
    {
        string? path = await PickLocalFile(".xsl");
        if (path != null) await OpenSource(Place.File, path, isXml: false);
    }

    public async void OnOpenXmlDriveClicked(object? sender, EventArgs e)
    {
        string? name = await PickDriveFile(".xml");
        if (name != null) await OpenSource(Place.Drive, name, isXml: true);
    }

    public async void OnOpenXslDriveClicked(object? sender, EventArgs e)
    {
        string? name = await PickDriveFile(".xsl");
        if (name != null) await OpenSource(Place.Drive, name, isXml: false);
    }

    public async void OnUploadSourcesClicked(object? sender, EventArgs e) => await Run("Помилка Google Drive", async () =>
    {
        ShowMessage("Збереження на Google Drive…");
        List<string> names = await controller.UploadSourcesToDriveAsync();
        ShowMessage($"На Google Drive збережено: {string.Join(", ", names)}");
    });

    public async void OnSearchClicked(object? sender, EventArgs e) => await Run("Помилка пошуку", () =>
    {
        var selected = filters.ToDictionary(f => f.Key, f => f.Value.SelectedIndex > 0 ? (string?)f.Value.SelectedItem : null);
        var found = controller.Search(strategyIndex, selected);
        resultsView.ItemsSource = found;
        ShowMessage($"{controller.GetStrategyNames()[strategyIndex]}: знайдено {found.Count}");
        return Task.CompletedTask;
    });

    // Очищає умови пошуку і результати
    public void OnClearClicked(object? sender, EventArgs e)
    {
        foreach (Picker picker in filters.Values)
        {
            picker.SelectedIndex = picker.Items.Count > 0 ? 0 : -1;
        }
        controller.Clear();
        resultsView.ItemsSource = controller.Results;   // порожній список, щоб показало «Немає результатів»
        ShowMessage("");
    }

    public async void OnSaveHtmlClicked(object? sender, EventArgs e) => await SaveResults(Place.File, asHtml: true);
    public async void OnSaveXmlClicked(object? sender, EventArgs e) => await SaveResults(Place.File, asHtml: false);
    public async void OnSaveHtmlDriveClicked(object? sender, EventArgs e) => await SaveResults(Place.Drive, asHtml: true);
    public async void OnSaveXmlDriveClicked(object? sender, EventArgs e) => await SaveResults(Place.Drive, asHtml: false);

    public async void OnHelpClicked(object? sender, EventArgs e) => await DisplayAlert("Довідка",
        "Лабораторна робота 2*, тема 19 «Кадри науковців (Зарплата)».\n" +
        "Автор: Бак Олександр, К-25.\n\n" +
        "1. Відкрийте XML-файл з науковцями (з диска або з Google Drive).\n" +
        "2. Оберіть спосіб аналізу (SAX, DOM або LINQ to XML) і значення атрибутів.\n" +
        "   «(будь-яке)» - атрибут не враховується.\n" +
        "3. «Знайти» - показати записи, «Очистити» - скинути умови і результати.\n" +
        "4. Для HTML відкрийте XSL-файл і натисніть «Зберегти HTML».\n" +
        "   Відфільтровані дані можна зберегти і як XML.\n\n" +
        $"Файли зберігаються в {FilesFolder}, там же журнал подій log.txt.", "OK");

    // Викликається з MauiProgram перед закриттям вікна. true - можна закривати
    public Task<bool> OnClosing()
        => DisplayAlert("Вихід", "Чи дійсно ви хочете завершити роботу з програмою?", "Так", "Ні");

    private async Task OpenSource(Place place, string name, bool isXml) => await Run("Не вдалося відкрити файл", async () =>
    {
        ShowMessage(place == Place.Drive ? "Завантаження з Google Drive…" : "");
        if (isXml)
        {
            await controller.OpenXmlAsync(place, name);
            RefreshFilters();
            resultsView.ItemsSource = null;
        }
        else
        {
            await controller.OpenXslAsync(place, name);
        }
        RefreshFiles();
        ShowMessage($"Відкрито: {name}");
    });

    private async Task SaveResults(Place place, bool asHtml) => await Run("Помилка збереження", async () =>
    {
        string? name = await DisplayPromptAsync("Файл", "Назва файлу:", "OK", "Скасувати", initialValue: "result");
        if (string.IsNullOrWhiteSpace(name)) return;
        name = Path.GetFileName(name);   // тільки назва, без шляху - щоб файл не потрапив в іншу папку

        if (place == Place.Drive)
        {
            ShowMessage("Збереження на Google Drive…");
            string saved = await controller.SaveResultsAsync(place, name, asHtml);
            ShowMessage($"Збережено на Google Drive: {saved}");
            return;
        }
        Directory.CreateDirectory(FilesFolder);
        string path = await controller.SaveResultsAsync(place, Path.Combine(FilesFolder, name), asHtml);
        ShowMessage($"Збережено: {path}");
        if (asHtml && await DisplayAlert("HTML", "Відкрити збережений файл у браузері?", "Так", "Ні"))
        {
            await Launcher.OpenAsync(new OpenFileRequest("HTML", new ReadOnlyFile(path)));
        }
    });

    private async Task<string?> PickLocalFile(string extension)
    {
        var types = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.WinUI] = new[] { extension },
        });
        var file = await FilePicker.PickAsync(new PickOptions { PickerTitle = $"Оберіть {extension}-файл", FileTypes = types });
        return file?.FullPath;
    }

    // Список файлів з Drive; null - користувач скасував або файлів немає
    private async Task<string?> PickDriveFile(string extension)
    {
        string? chosen = null;
        await Run("Помилка Google Drive", async () =>
        {
            ShowMessage("Отримання списку файлів з Google Drive…");
            List<string> names = await controller.GetDriveFileNamesAsync(extension);
            ShowMessage("");
            if (names.Count == 0)
            {
                await DisplayAlert("Google Drive", $"На Google Drive ще немає файлів {extension}", "OK");
                return;
            }
            string? name = await DisplayActionSheet("Оберіть файл", "Скасувати", null, names.ToArray());
            if (name != null && name != "Скасувати") chosen = name;
        });
        return chosen;
    }

    // Заповнює списки значеннями атрибутів з відкритого XML
    private void RefreshFilters()
    {
        foreach (var (attribute, picker) in filters)
        {
            picker.Items.Clear();
            List<string> values = controller.GetAttributeValues(attribute);
            if (values.Count == 0) continue;
            picker.Items.Add(AnyValue);
            foreach (string v in values) picker.Items.Add(v);
            picker.SelectedIndex = 0;
        }
        RefreshFiles();
    }

    private void RefreshFiles()
        => filesLabel.Text = $"XML: {controller.XmlName ?? "не відкрито"}     XSL: {controller.XslName ?? "не відкрито"}";

    private void ShowMessage(string text) => statusLabel.Text = text;

    // Усі помилки (поганий XML, немає мережі, не обрано файл) показуємо у вікні, а не падаємо
    private async Task Run(string title, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowMessage("");
            await DisplayAlert(title, ex.Message, "OK");
        }
    }
}
