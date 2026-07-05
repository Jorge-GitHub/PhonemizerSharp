# PhonemizerSharp
A .NET library for converting text into phoneme sequences. Suitable for TTS engines, speech processing, and natural language applications.

In plain terms: you give it words or sentences, and it returns the sounds for that text using IPA phonemes. That makes it useful for text-to-speech tools, speech experiments, pronunciation helpers, alignment workflows, and any project where written text needs to become a sound-oriented representation.

The current implementation uses eSpeak NG under the hood. On Windows x64, the native eSpeak NG library and data files are bundled with the project, so the default runtime does not require a separate eSpeak NG install.

## What It Does

- Converts text into IPA phoneme output.
- Supports eSpeak NG language and voice codes such as `en-us` and `en-gb`.
- Provides a simple async API through `PhonemizerService`.
- Uses the bundled native eSpeak NG runtime by default on Windows x64.
- Also includes an optional process runtime if you want to call an external `espeak-ng` executable.

## Quick Start

Reference the project from your app, then call `PhonemizeAsync`:

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

The result includes the original text, phonemes, language, alphabet, provider, runtime, and provider name.

## Requirements

- .NET 10 SDK
- Windows x64 for the bundled native eSpeak NG runtime

For the default Windows x64 path, no extra eSpeak NG installation is needed. The project copies `libespeak-ng.dll` and `espeak-ng-data` into the build output.

For other platforms, install `libespeak-ng` and make sure it can be found by the operating system loader, or set `EspeakNgProviderSettings.NativeLibraryPath` and `DataPath`.

If you choose `PhonemizerRuntime.Process`, PhonemizerSharp launches an external `espeak-ng` executable. In that mode, `espeak-ng` must be available on `PATH`, or you must set `EspeakNgProviderSettings.ExecutablePath`.

## Choosing a Runtime

By default, `PhonemizerService` uses `PhonemizerRuntime.Auto`, which currently selects the native eSpeak NG runtime.

Use the default service for the bundled native runtime:

```csharp
using PhonemizerSharp.Application;

using PhonemizerService phonemizer = new();
```

Use the process runtime only when you specifically want to call an external `espeak-ng` executable:

```csharp
using PhonemizerSharp.Application;
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
```

## Configuration

`PhonemizerRequest`:

| Property | Default | Description |
| --- | --- | --- |
| `Text` | `""` | Text to phonemize. Blank text is rejected. |
| `Language` | `""` | eSpeak NG language or voice. Falls back to the provider default. |
| `Alphabet` | `Ipa` | Output alphabet. IPA is the only supported alphabet today. |

`EspeakNgProviderSettings`:

| Property | Default | Description |
| --- | --- | --- |
| `ExecutablePath` | `espeak-ng` | Executable used by the process runtime. |
| `NativeLibraryPath` | `""` | Optional explicit path to `libespeak-ng`. |
| `DataPath` | `""` | Optional eSpeak NG data path. |
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

## License

PhonemizerSharp is licensed under the Apache License 2.0. See [LICENSE](LICENSE).
