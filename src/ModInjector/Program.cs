using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ModInjector
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.Title = "FNAF Rewritten: '87 Auto-Pilot Patcher";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine("   ✦ FNAF Rewritten: '87 Auto-Pilot Installer ✦   ");
            Console.WriteLine("=================================================");
            Console.ResetColor();
            Console.WriteLine();

            string managedDir = null;

            // 1. Check command line argument
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                managedDir = ResolveManagedDir(args[0]);
            }

            // 2. Auto-detect common locations
            if (string.IsNullOrEmpty(managedDir))
            {
                string[] candidates = new string[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Managed"),
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FNAFRewritten87_Data", "Managed"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "FNAFRewritten87_Data", "Managed"),
                    @"C:\Games\FNAF-Rewritten-87\FNAFRewritten87_Data\Managed",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FNAF-Rewritten-87", "FNAFRewritten87_Data", "Managed"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "FNAF-Rewritten-87", "FNAFRewritten87_Data", "Managed")
                };

                foreach (var candidate in candidates)
                {
                    string resolved = ResolveManagedDir(candidate);
                    if (!string.IsNullOrEmpty(resolved))
                    {
                        managedDir = resolved;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[✓] Auto-detected game Managed directory: {managedDir}");
                        Console.ResetColor();
                        break;
                    }
                }
            }

            // 3. Prompt user if still not found
            while (string.IsNullOrEmpty(managedDir))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Enter path to FNAF Rewritten: '87 game folder or Managed folder: ");
                Console.ResetColor();
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Operation cancelled.");
                    return 1;
                }
                managedDir = ResolveManagedDir(input.Trim().Trim('\"', '\''));
                if (string.IsNullOrEmpty(managedDir))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not locate Assembly-CSharp.dll at that path. Please try again.");
                    Console.ResetColor();
                }
            }

            string targetDll = Path.Combine(managedDir, "Assembly-CSharp.dll");
            string backupDll = Path.Combine(managedDir, "Assembly-CSharp.dll.bak");
            string modDllInManaged = Path.Combine(managedDir, "FNAF87Mod.dll");

            // Look for FNAF87Mod.dll next to injector or in managed
            string localModDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FNAF87Mod.dll");
            if (File.Exists(localModDll))
            {
                Console.WriteLine($"[+] Copying FNAF87Mod.dll to {managedDir}...");
                File.Copy(localModDll, modDllInManaged, true);
            }
            else if (!File.Exists(modDllInManaged))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Error: FNAF87Mod.dll not found in {AppDomain.CurrentDomain.BaseDirectory} or {managedDir}");
                Console.ResetColor();
                return 1;
            }

            // Backup Assembly-CSharp.dll if no backup exists
            if (!File.Exists(backupDll))
            {
                Console.WriteLine($"[+] Creating backup: {backupDll}");
                File.Copy(targetDll, backupDll, false);
            }

            Console.WriteLine("[*] Reading Assembly-CSharp.dll and FNAF87Mod.dll...");
            var readerParams = new ReaderParameters { ReadWrite = true };

            using (var asm = AssemblyDefinition.ReadAssembly(targetDll, readerParams))
            using (var modAsm = AssemblyDefinition.ReadAssembly(modDllInManaged))
            {
                var officeLightType = asm.MainModule.Types.FirstOrDefault(t => t.Name == "OfficeLightScript");
                if (officeLightType == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: Could not find OfficeLightScript in Assembly-CSharp.dll");
                    Console.ResetColor();
                    return 1;
                }

                var startMethod = officeLightType.Methods.FirstOrDefault(m => m.Name == "Start");
                if (startMethod == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: Could not find Start() method in OfficeLightScript");
                    Console.ResetColor();
                    return 1;
                }

                var autoPilotType = modAsm.MainModule.Types.FirstOrDefault(t => t.Name == "AutoPilot");
                if (autoPilotType == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: Could not find AutoPilot in FNAF87Mod.dll");
                    Console.ResetColor();
                    return 1;
                }

                var attachMethod = autoPilotType.Methods.FirstOrDefault(m => m.Name == "Attach");
                if (attachMethod == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[!] Error: Could not find Attach method in AutoPilot");
                    Console.ResetColor();
                    return 1;
                }

                bool alreadyInjected = startMethod.Body.Instructions.Any(i =>
                    i.OpCode == OpCodes.Call && i.Operand != null && i.Operand.ToString().Contains("AutoPilot::Attach"));

                if (alreadyInjected)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[✓] Auto-Pilot hook is already installed in Assembly-CSharp.dll!");
                    Console.ResetColor();
                }
                else
                {
                    var attachRef = asm.MainModule.ImportReference(attachMethod);
                    var il = startMethod.Body.GetILProcessor();
                    var firstInst = startMethod.Body.Instructions[0];

                    var ldarg0 = il.Create(OpCodes.Ldarg_0);
                    var callAttach = il.Create(OpCodes.Call, attachRef);

                    il.InsertBefore(firstInst, ldarg0);
                    il.InsertAfter(ldarg0, callAttach);

                    Console.WriteLine("[+] Injected AutoPilot::Attach(this) into OfficeLightScript.Start()");
                    asm.Write();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[✓] Successfully patched Assembly-CSharp.dll!");
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=================================================");
            Console.WriteLine("   ✦ Installation Complete! ✦   ");
            Console.WriteLine("=================================================");
            Console.ResetColor();
            Console.WriteLine("Launch FNAFRewritten87.exe and start any Night:");
            Console.WriteLine("  • [B] = Toggle Auto-Pilot (Active / Standby)");
            Console.WriteLine("  • [H] = Toggle Tactical HUD Overlay");
            Console.WriteLine();
            return 0;
        }

        static string ResolveManagedDir(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            try
            {
                if (File.Exists(path))
                {
                    if (Path.GetFileName(path).Equals("Assembly-CSharp.dll", StringComparison.OrdinalIgnoreCase))
                        return Path.GetDirectoryName(path);
                    if (Path.GetFileName(path).Equals("FNAFRewritten87.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        string dataManaged = Path.Combine(Path.GetDirectoryName(path), "FNAFRewritten87_Data", "Managed");
                        if (File.Exists(Path.Combine(dataManaged, "Assembly-CSharp.dll"))) return dataManaged;
                    }
                }

                if (Directory.Exists(path))
                {
                    if (File.Exists(Path.Combine(path, "Assembly-CSharp.dll")))
                        return Path.GetFullPath(path);

                    string dataManaged = Path.Combine(path, "FNAFRewritten87_Data", "Managed");
                    if (File.Exists(Path.Combine(dataManaged, "Assembly-CSharp.dll")))
                        return Path.GetFullPath(dataManaged);

                    string managed = Path.Combine(path, "Managed");
                    if (File.Exists(Path.Combine(managed, "Assembly-CSharp.dll")))
                        return Path.GetFullPath(managed);
                }
            }
            catch { }

            return null;
        }
    }
}
