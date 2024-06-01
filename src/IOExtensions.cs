namespace RoboZZle.Offline;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PCLStorage;

static class IoExtensions {
    public static async Task<string[]> ReadLinesAsync(this IFile file) {
        string? text = await file.ReadAllTextAsync().ConfigureAwait(false);
        return text.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
    }

    public static async Task WriteJson(this IFile file, object @object) {
        var serializer = new JsonSerializer();
        using var dataStream = await file.OpenAsync(FileAccess.ReadAndWrite).ConfigureAwait(false);
        using var dataWriter = new StreamWriter(dataStream);
        using var jsonWriter = new JsonTextWriter(dataWriter);
        serializer.Serialize(jsonWriter, @object);
        await dataWriter.FlushAsync().ConfigureAwait(false);
    }

    public static async Task<T> ReadJson<T>(this IFile file) {
        var serializer = new JsonSerializer();
        using var dataStream = await file.OpenAsync(FileAccess.Read).ConfigureAwait(false);
        using var dataReader = new StreamReader(dataStream);
        using var jsonReader = new JsonTextReader(dataReader);
        return serializer.Deserialize<T>(jsonReader)!;
    }

    public static async Task<IFile?> GetFileOrNull(this IFolder folder, string name) {
        try {
            return await folder.GetFileAsync(name).ConfigureAwait(false);
        } catch (System.IO.FileNotFoundException) {
            return null;
        }
    }

    public static async Task AppendAllTextAsync(this IFile file, string text) {
        using var stream = await file.OpenAsync(FileAccess.ReadAndWrite).ConfigureAwait(false);
        using var writer =
            new StreamWriter(
                stream,
                stream.Length == 0
                    ? Encoding.UTF8
                    : new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        stream.Seek(0, SeekOrigin.End);
        await writer.WriteAsync(text).ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }
}