# PhonemizerSharp
A .NET library for converting text into phoneme sequences. Suitable for TTS engines, speech processing, and natural language applications.

## Repository Layout

```text
src/
  PhonemizerSharp/
    PhonemizerSharp.slnx
    PhonemizerSharp/
      PhonemizerSharp.csproj
    PhonemizerSharp.Ut/
      PhonemizerSharp.Ut.csproj
```

## Requirements

- .NET 10 SDK

The default runtime uses the bundled Windows x64 eSpeak NG native assets (`libespeak-ng.dll` and `espeak-ng-data`), so a separate eSpeak NG install is not required for the default Windows x64 path.

If you explicitly choose the process runtime, PhonemizerSharp launches an `espeak-ng` executable. In that mode, `espeak-ng` must be available on `PATH`, or you must set `EspeakNgProviderSettings.ExecutablePath`.

On non-Windows x64 platforms, install `libespeak-ng` and make sure it can be found by the operating system loader, or set `EspeakNgProviderSettings.NativeLibraryPath` and `DataPath`.

## Quick Start

```csharp
using PhonemizerSharp.Application;
using PhonemizerSharp.Domain.Requests;

using PhonemizerService phonemizer = new();

var result = await phonemizer.PhonemizeAsync(new PhonemizerRequest
{
    Text = "Hello world",
    Language = "en-us"
});

Console.WriteLine(result.Phonemes);
```

## Choosing a Runtime

`PhonemizerService` uses `PhonemizerSettings`. By default, `Runtime.Auto` currently selects the native eSpeak NG runtime.

Use the process runtime only when you prefer to call an external `espeak-ng` executable:

```csharp
using PhonemizerSharp.Application;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Settings;
using PhonemizerSharp.Domain.Settings.Enums;

using PhonemizerService phonemizer = new(new PhonemizerSettings
{
    Runtime = PhonemizerRuntime.Process,
    EspeakNg =
    {
        ExecutablePath = "espeak-ng",
        DefaultLanguage = "en-us",
        TimeoutInSeconds = 30
    }
});

var result = await phonemizer.PhonemizeAsync(new PhonemizerRequest
{
    Text = "PhonemizerSharp",
    Language = "en-gb"
});
```

Use the native runtime when you want to load `libespeak-ng` directly:

```csharp
using PhonemizerSharp.Application;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Settings;
using PhonemizerSharp.Domain.Settings.Enums;

using PhonemizerService phonemizer = new(new PhonemizerSettings
{
    Runtime = PhonemizerRuntime.NativeLibrary,
    EspeakNg =
    {
        NativeLibraryPath = "",
        DataPath = "",
        DefaultLanguage = "en-us"
    }
});

var result = await phonemizer.PhonemizeAsync(new PhonemizerRequest
{
    Text = "Speech tools",
    Language = "en-us"
});
```

## Configuration

`PhonemizerRequest`:

| Property | Default | Description |
| --- | --- | --- |
| `Text` | `""` | Text to phonemize. Blank text is rejected. |
| `Language` | `""` | eSpeak NG language or voice, such as `en-us` or `en-gb`. Falls back to the provider default. |
| `Alphabet` | `Ipa` | Output alphabet. IPA is the only supported alphabet today. |

`EspeakNgProviderSettings`:

| Property | Default | Description |
| --- | --- | --- |
| `ExecutablePath` | `espeak-ng` | Executable used by the process runtime. |
| `NativeLibraryPath` | `""` | Optional explicit path to `libespeak-ng`. |
| `DataPath` | `""` | Optional eSpeak NG data path. If you pass the `espeak-ng-data` folder, the library uses its parent as the data home. |
| `DefaultLanguage` | `en-us` | Language used when a request does not specify one. |
| `IpaMode` | `3` | eSpeak NG IPA mode. |
| `TimeoutInSeconds` | `30` | Process runtime timeout. Values less than or equal to zero disable the timeout. |
| `Quiet` | `true` | Adds `-q` when using the process runtime. |

## Build and Test

From the repository root:

```powershell
dotnet build .\src\PhonemizerSharp\PhonemizerSharp.slnx
dotnet test .\src\PhonemizerSharp\PhonemizerSharp.slnx
```

Some tests exercise live eSpeak NG behavior. The native live tests can use the bundled Windows x64 assets. The process live tests require an `espeak-ng` executable.

To skip live eSpeak NG tests:

```powershell
dotnet test .\src\PhonemizerSharp\PhonemizerSharp.slnx --filter "TestCategory!=LiveEspeakNg&TestCategory!=LiveEspeakNgNative"
```

## License

PhonemizerSharp is licensed under the Apache License 2.0. See [LICENSE](LICENSE).
