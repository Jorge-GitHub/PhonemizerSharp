using System.Runtime.InteropServices;
using System.Text;

namespace PhonemizerSharp.Application.Providers.EspeakNgNativeService;

internal sealed class Utf8NativeString : IDisposable
{
    public Utf8NativeString(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            this.Pointer = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, this.Pointer, bytes.Length);
            Marshal.WriteByte(this.Pointer, bytes.Length, 0);
            this.HasValue = true;
        }
    }

    public IntPtr Pointer { get; private set; }
    public bool HasValue { get; }

    public void Dispose()
    {
        if (this.Pointer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(this.Pointer);
            this.Pointer = IntPtr.Zero;
        }
    }
}
