using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace RandomizerFixPatcher
{
    static class Program
    {

        public static bool ReplaceBytes(byte[] fileBytes, byte[] replaceWith, int position)
        {
            for (var i = 0; i < replaceWith.Length; i++)
            {
                fileBytes[position + i] = replaceWith[i];
            }
            return true;
        }

        public static bool DoesByteSequenceMatch(byte[] fileBytes, byte[] comparisonBytes, int position)
        {
            for (var i = 0; i < comparisonBytes.Length; i++)
            {
                if (position + i >= fileBytes.Length)
                {
                    Console.WriteLine("Oop");
                    return false;
                }
                if (fileBytes[position + i] != comparisonBytes[i])
                {
                    Console.WriteLine("WHAT"+Environment.NewLine+fileBytes[position + i].ToString()+" != "+ comparisonBytes[i].ToString());
                    return false;
                }
            }
            return true;
        }

        //thanks https://social.msdn.microsoft.com/Forums/vstudio/en-US/15514c1a-b6a1-44f5-a06c-9b029c4164d7/searching-a-byte-array-for-a-pattern-of-bytes?forum=csharpgeneral
        public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        public static void PatchExe(string exeFile)
        {
            var bakpath = Path.Combine(Path.GetDirectoryName(exeFile), Path.GetFileNameWithoutExtension(exeFile) + ".bak");
            if (!File.Exists(bakpath))
            {
                try
                {
                    File.Copy(exeFile, bakpath);
                }
                catch (Exception exception)
                {
                    var diag = MessageBox.Show("Can't create a backup. Proceed anyway?", "Info", MessageBoxButtons.YesNo);
                    if (diag == DialogResult.No)
                        return;
                }
            }
            var fileBytes = File.ReadAllBytes(exeFile);
            var referenceFunc = new byte[] { 0xCC, 0xCC };
            var ind = IndexOf(fileBytes, referenceFunc);
            if (!DoesByteSequenceMatch(fileBytes,referenceFunc, 0x174503A))
            {
                MessageBox.Show("Game is already patched or an incompatible version. Make sure this is a No-CD Mansion and Garden exe.");
                return;
            }
            //MessageBox.Show("Found at: " + ind.ToString());
            //Hook into the game's loading procedures to initialize some random seeds (Sims2EP9RPC.exe+399A82).
            var jumpHookInitializer = new byte[] { 0xE9, 0x81, 0xB6, 0x3A, 0x01 };

            //Hook into the random seed sequencer (Sims2EP9RPC.GZDllGetGZCOMDirector+8C8D).
            var jumpHook = new byte[] { 0xE9, 0x84, 0xD0, 0x72, 0x01, 0x90 };

            //New randomization code (Sims2EP9RPC.exe+174503A).
            var newBytes = new byte[] { 0x83, 0x3D, 0xE0, 0x50, 0xB4, 0x01, 0x00, 0x74, 0x27, 0x0F, 0x1F, 0x40, 0x00, 0xEB, 0x03, 0x0F, 0x1F, 0x00, 0x51, 0xE8, 0xFE, 0x75, 0x25, 0x74, 0x59, 0x05, 0x00, 0x50, 0x02, 0x00, 0x51, 0x8B, 0xC8, 0xB8, 0x00, 0x50, 0x01, 0x00, 0xF7, 0xE1, 0x59, 0x89, 0x01, 0xE9, 0x4D, 0x2F, 0x8D, 0xFE, 0x51, 0x50, 0xA1, 0xF4, 0x50, 0xB4, 0x01, 0x50, 0xFF, 0x15, 0x58, 0x31, 0x1D, 0x01, 0x8B, 0xF0, 0x05, 0x50, 0xC6, 0x05, 0x00, 0xA3, 0xEC, 0x50, 0xB4, 0x01, 0x8B, 0xC6, 0x05, 0x80, 0xC6, 0x05, 0x00, 0xA3, 0xF0, 0x50, 0xB4, 0x01, 0xC7, 0x05, 0xE0, 0x50, 0xB4, 0x01, 0x01, 0x00, 0x00, 0x00, 0x68, 0xE4, 0x50, 0xB4, 0x01, 0xFF, 0x15, 0x34, 0x33, 0x1D, 0x01, 0xFF, 0x15, 0xAC, 0x33, 0x1D, 0x01, 0x8B, 0xC8, 0xA1, 0xE4, 0x50, 0xB4, 0x01, 0x01, 0xC8, 0x03, 0x05, 0xE6, 0x50, 0xB4, 0x01, 0xB9, 0x02, 0x00, 0x00, 0x00, 0xF7, 0xE1, 0xA3, 0xF8, 0xBF, 0x49, 0x01, 0xA3, 0xE4, 0x50, 0xB4, 0x01, 0x50, 0xE8, 0xAF, 0x75, 0x25, 0x74, 0x58, 0x58, 0x59, 0xE9, 0x73, 0xFF, 0xFF, 0xFF, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0x01, 0x00, 0x00, 0x00, 0xC0, 0x04, 0x12, 0x87, 0x9A, 0x53, 0xD7, 0x01, 0x50, 0xC6, 0x05, 0x00, 0x80, 0xC6, 0x05, 0x00, 0x6D, 0x00, 0x73, 0x00, 0x76, 0x00, 0x63, 0x00, 0x72, 0x00, 0x74, 0x00, 0x2E, 0x00, 0x64, 0x00, 0x6C, 0x00, 0x6C, 0x00, 0xE8, 0xCB, 0x48, 0xC5, 0xFE, 0xA1, 0xA0, 0x90, 0x48, 0x01, 0x83, 0xC0, 0x7C, 0x8B, 0x08, 0x83, 0xC1, 0x08, 0xA1, 0xE4, 0x50, 0xB4, 0x01, 0x89, 0x01, 0xA1, 0xA8, 0x90, 0x48, 0x01, 0x05, 0xDC, 0x00, 0x00, 0x00, 0x8B, 0x0D, 0xE4, 0x50, 0xB4, 0x01, 0x89, 0x08, 0xA1, 0xE4, 0x50, 0xB4, 0x01, 0xA3, 0xF8, 0xBF, 0x89, 0x01, 0xE9, 0x45, 0x49, 0xC5, 0xFE, 0x89, 0x01, 0x0F, 0xAC, 0xD0, 0x10, 0xE9, 0x6A, 0x2E, 0x8D, 0xFE };

            ReplaceBytes(fileBytes, jumpHookInitializer, 0x399A82);
            ReplaceBytes(fileBytes, jumpHook, 0x00417FB1 - 0x00400000);
            ReplaceBytes(fileBytes, newBytes, 0x174503A);
            try
            {
                File.WriteAllBytes(exeFile, fileBytes);
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't patch executable. Try launching me as administrator?");
                return;
            }
            MessageBox.Show("Game Patched Successfully");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
