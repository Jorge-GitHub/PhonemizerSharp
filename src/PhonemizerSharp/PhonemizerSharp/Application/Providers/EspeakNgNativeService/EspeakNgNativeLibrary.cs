using System.Runtime.InteropServices;
using System.Text;

namespace PhonemizerSharp.Application.Providers.EspeakNgNativeService;

internal sealed class EspeakNgNativeLibrary : IDisposable
{
    private const int AudioOutputSynchronous = 2;
    private const int InitializeDontExit = 0x8000;
    private const int CharacterModeUtf8 = 1;
    private const int PhonemeModeIpa = 2;

    private readonly IntPtr handle;
    private readonly EspeakInitializeDelegate initialize;
    private readonly EspeakSetVoiceByPropertiesDelegate setVoiceByProperties;
    private readonly EspeakSetVoiceByNameDelegate setVoiceByName;
    private readonly EspeakTextToPhonemesDelegate textToPhonemes;
    private readonly EspeakTerminateDelegate terminate;
    private bool initialized;
    private bool disposed;

    public EspeakNgNativeLibrary(string libraryPath)
    {
        this.handle = this.LoadLibrary(libraryPath);
        this.initialize = this.GetDelegate<EspeakInitializeDelegate>(
            this.handle, "espeak_Initialize");
        this.setVoiceByProperties = this.GetDelegate<EspeakSetVoiceByPropertiesDelegate>(
            this.handle, "espeak_SetVoiceByProperties");
        this.setVoiceByName = this.GetDelegate<EspeakSetVoiceByNameDelegate>(
            this.handle, "espeak_SetVoiceByName");
        this.textToPhonemes = this.GetDelegate<EspeakTextToPhonemesDelegate>(
            this.handle, "espeak_TextToPhonemes");
        this.terminate = this.GetDelegate<EspeakTerminateDelegate>(
            this.handle, "espeak_Terminate");
    }

    public void Initialize(string dataPath)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        if (!this.initialized)
        {
            using Utf8NativeString nativeDataPath =
                new(!string.IsNullOrWhiteSpace(dataPath) ? dataPath.Trim() : "");
            IntPtr pathPointer = nativeDataPath.HasValue
                ? nativeDataPath.Pointer
                : IntPtr.Zero;

            int sampleRate = this.initialize(
                AudioOutputSynchronous,
                buflength: 0,
                pathPointer,
                InitializeDontExit);

            if (sampleRate <= 0)
            {
                throw new InvalidOperationException(
                    "Native eSpeak NG could not be initialized. "
                    + "Verify that the eSpeak NG data files are installed.");
            }

            this.initialized = true;
        }
    }

    public void SetVoice(string voiceName)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        using Utf8NativeString nativeVoice = new(voiceName);
        EspeakVoice voice = new()
        {
            Languages = nativeVoice.Pointer
        };

        int result = this.setVoiceByProperties(ref voice);
        if (result != 0)
        {
            result = this.setVoiceByName(nativeVoice.Pointer);
        }

        if (result != 0)
        {
            throw new InvalidOperationException(
                $"Native eSpeak NG could not select language or voice '{voiceName}'. "
                + $"Error code: {result}.");
        }
    }

    public string TextToPhonemes(string text, int ipaMode)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);

        using Utf8NativeString nativeText = new(text);
        IntPtr textPointer = nativeText.Pointer;
        StringBuilder builder = new();

        while (textPointer != IntPtr.Zero)
        {
            IntPtr phonemePointer = this.textToPhonemes(
                ref textPointer,
                CharacterModeUtf8,
                this.ResolvePhonemeMode(ipaMode));

            if (phonemePointer == IntPtr.Zero)
            {
                break;
            }

            string? value = Marshal.PtrToStringUTF8(phonemePointer);
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(value.Trim());
            }
        }

        return builder.ToString().Trim();
    }

    public void Dispose()
    {
        if (!this.disposed)
        {
            if (this.initialized)
            {
                _ = this.terminate();
                this.initialized = false;
            }

            NativeLibrary.Free(this.handle);
            this.disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private IntPtr LoadLibrary(string libraryPath)
    {
        if (!string.IsNullOrWhiteSpace(libraryPath))
        {
            return NativeLibrary.Load(libraryPath.Trim());
        }

        List<string> candidates = this.GetLibraryCandidates();
        Exception? lastException = null;

        foreach (string candidate in candidates)
        {
            try
            {
                return NativeLibrary.Load(candidate);
            }
            catch (Exception exception)
                when (exception is DllNotFoundException
                    or BadImageFormatException)
            {
                lastException = exception;
            }
        }

        throw new DllNotFoundException(
            "Native eSpeak NG library was not found. Tried: "
            + string.Join(", ", candidates),
            lastException);
    }

    private List<string> GetLibraryCandidates()
    {
        if (OperatingSystem.IsWindows())
        {
            return
            [
                "libespeak-ng.dll",
                "espeak-ng.dll",
                "espeak.dll"
            ];
        }

        if (OperatingSystem.IsMacOS())
        {
            return
            [
                "libespeak-ng.dylib"
            ];
        }

        return
        [
            "libespeak-ng.so.1",
            "libespeak-ng.so",
            "espeak-ng"
        ];
    }

    private TDelegate GetDelegate<TDelegate>(
        IntPtr libraryHandle,
        string exportName)
        where TDelegate : Delegate
    {
        IntPtr export = NativeLibrary.GetExport(libraryHandle, exportName);
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(export);
    }

    private int ResolvePhonemeMode(int ipaMode)
    {
        if (ipaMode == 3)
        {
            return PhonemeModeIpa | ('_' << 8);
        }

        return PhonemeModeIpa;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EspeakInitializeDelegate(
        int output,
        int buflength,
        IntPtr path,
        int options);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EspeakSetVoiceByNameDelegate(IntPtr name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EspeakSetVoiceByPropertiesDelegate(
        ref EspeakVoice voiceSpecification);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr EspeakTextToPhonemesDelegate(
        ref IntPtr textPointer,
        int textMode,
        int phonemeMode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int EspeakTerminateDelegate();

    [StructLayout(LayoutKind.Sequential)]
    private struct EspeakVoice
    {
        public IntPtr Name;
        public IntPtr Languages;
        public IntPtr Identifier;
        public byte Gender;
        public byte Age;
        public byte Variant;
        public byte Reserved;
        public int Score;
        public IntPtr Spare;
    }
}
