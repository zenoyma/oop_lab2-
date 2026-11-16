using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace Scientists.Core.Storage;

// Файли на Google Drive (взято з ЛР 1*, тільки тепер зберігаються будь-які файли, а не таблиця).
// Потрібен client_secret.json (OAuth-клієнт типу "Desktop app" з Google Cloud Console) у папці appFolder.
// Право DriveFile: програма бачить лише ті файли, які сама створила
public class GoogleDriveStorage : IFileStorage, IListableStorage
{
    private readonly string appFolder;
    private DriveService? service;

    public GoogleDriveStorage(string appFolder)
    {
        this.appFolder = appFolder;
    }

    public async Task SaveAsync(string name, byte[] content, string mimeType)
    {
        DriveService drive = await AuthorizeAsync();
        using var stream = new MemoryStream(content);

        string? id = await FindFileId(drive, name);
        if (id == null)
        {
            var create = drive.Files.Create(new DriveFile { Name = name, MimeType = mimeType }, stream, mimeType);
            await UploadOrThrow(create);                                                // files.create
        }
        else
        {
            var update = drive.Files.Update(new DriveFile(), id, stream, mimeType);    // files.update
            await UploadOrThrow(update);
        }
    }

    public async Task<byte[]> LoadAsync(string name)
    {
        DriveService drive = await AuthorizeAsync();
        string id = await FindFileId(drive, name)
                    ?? throw new FileNotFoundException($"На Google Drive немає файлу «{name}»");

        using var stream = new MemoryStream();
        await drive.Files.Get(id).DownloadAsync(stream);                                 // files.get
        return stream.ToArray();
    }

    // Назви файлів програми з потрібним розширенням (.xml, .xsl), нові зверху
    public async Task<List<string>> ListAsync(string extension)
    {
        DriveService drive = await AuthorizeAsync();
        var request = drive.Files.List();
        request.Q = "trashed = false";
        request.Fields = "files(name)";
        request.OrderBy = "modifiedTime desc";
        var result = await request.ExecuteAsync();                                      // files.list
        return result.Files
            .Select(f => f.Name)
            .Where(n => n.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToList();
    }

    // OAuth: перший раз відкривається браузер для входу, далі токен береться з appFolder\token
    private async Task<DriveService> AuthorizeAsync()
    {
        if (service != null)
        {
            return service;
        }
        string secretPath = Path.Combine(appFolder, "client_secret.json");
        if (!File.Exists(secretPath))
        {
            throw new FileNotFoundException(
                $"Не знайдено {secretPath}. Створіть OAuth-клієнт (Desktop app) у Google Cloud Console, " +
                "увімкніть Google Drive API і покладіть завантажений JSON сюди під назвою client_secret.json.");
        }

        using var secretStream = File.OpenRead(secretPath);
        UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            GoogleClientSecrets.FromStream(secretStream).Secrets,
            new[] { DriveService.Scope.DriveFile },
            "user",
            CancellationToken.None,
            new FileDataStore(Path.Combine(appFolder, "token"), fullPath: true));

        service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Scientists19",
        });
        return service;
    }

    // Шукає файл за назвою; null - файлу немає
    private static async Task<string?> FindFileId(DriveService drive, string name)
    {
        var request = drive.Files.List();
        request.Q = $"name = '{name.Replace("\\", "\\\\").Replace("'", "\\'")}' and trashed = false";
        request.Fields = "files(id)";
        var result = await request.ExecuteAsync();
        return result.Files.FirstOrDefault()?.Id;
    }

    private static async Task UploadOrThrow(Google.Apis.Upload.ResumableUpload upload)
    {
        var progress = await upload.UploadAsync();
        if (progress.Exception != null)
        {
            throw progress.Exception;
        }
    }
}
