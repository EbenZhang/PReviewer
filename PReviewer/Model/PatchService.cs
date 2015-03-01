using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtendedCL;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class PatchService: IPatchService
    {
        public async Task RevertViaPatch(string content, string patch, string patchTo)
        {
            var name = Path.GetFileName(patchTo);
            using (var stream = File.OpenWrite(patchTo))
            {
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteAsync(content);
                }
            }

            var dir = Path.GetDirectoryName(patchTo);
            var tmpPatchFile = Path.Combine(dir, Path.GetRandomFileName() + ".patch");
            using (var stream = File.OpenWrite(tmpPatchFile))
            {
                var headOfPatch = string.Format("--- a/{0}\r\n+++ b/{1}\r\n", name, name);
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteLineAsync(headOfPatch);
                    await sw.WriteAsync(patch);
                    if (!patch.EndsWith("\n"))
                    {
                        await sw.WriteAsync("\n");
                    }
                }
            }

            var patchExe = Path.Combine(PathHelper.ProcessDir, "PatchBin", "patch.exe");

            var p = new Process();
            p.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute  =false,
                RedirectStandardOutput =  true,
                RedirectStandardError = true,
                FileName = patchExe,
                WorkingDirectory = dir,
                Arguments = "-p1 -R --binary -i " + Path.GetFileName(tmpPatchFile)
            };
            p.OutputDataReceived += p_OutputDataReceived;
            p.ErrorDataReceived += POnErrorDataReceived;
            p.Start();
            p.WaitForExit();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            try
            {
                File.Delete(tmpPatchFile);
            }
            catch
            {
                // ignored
            }
        }

        private void POnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var d = e.Data;
        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var d = e.Data;
        }
    }
}
