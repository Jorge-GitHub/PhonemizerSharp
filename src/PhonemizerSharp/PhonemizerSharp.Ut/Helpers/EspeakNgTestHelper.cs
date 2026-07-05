using System.ComponentModel;
using System.Diagnostics;

namespace PhonemizerSharp.Ut.Helpers;

internal sealed class EspeakNgTestHelper
{
    public void RequireEspeakNg()
    {
        try
        {
            using Process process = Process.Start(new ProcessStartInfo
            {
                FileName = "espeak-ng",
                ArgumentList = { "--version" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            })!;

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Assert.Inconclusive(
                    "espeak-ng returned a non-zero exit code.");
            }
        }
        catch (Win32Exception)
        {
            Assert.Inconclusive(
                "espeak-ng was not found on PATH. Install it to run live phonemizer tests.");
        }
    }
}
