using System;
using System.IO;
using dnlib.DotNet;
using LoGiC.NET.Protections;
using SharpConfigParser;
using LoGiC.NET.Utils;
using System.Net.Configuration;

namespace LoGiC.NET
{
    class Program
    {
        public static ModuleDefMD Module { get; set; }

        public static string FileExtension { get; set; }

        public static bool DontRename { get; set; }

        public static bool ForceWinForms { get; set; }

        public static string FilePath { get; set; }

        public static MemoryStream Stream = new MemoryStream();

        static void Main(string[] args)
        {
            string path = args[0];
            string pathOutput = args[1] + Path.GetFileName(path);

            System.Collections.Generic.List<string> customObfuscation = new System.Collections.Generic.List<string>();
            //customObfuscation.Add("DAL_DataAccess.dll");

            customObfuscation.ForEach(s => s = s.ToUpper());

            bool isCustomObfuscation = customObfuscation.Contains(Path.GetFileName(path).ToUpper());
            Console.WriteLine($"Custom Obfuscation: -- " + (isCustomObfuscation ? "Y" : "N"));

            Console.WriteLine($"Obfuscate Start:\nFILE : " + path);
            Console.WriteLine($"OUTPUT : " + pathOutput);

            Console.WriteLine($"Preparing obfuscation...");
            if (!File.Exists("config.txt"))
            {
                Console.WriteLine($"Config file not found, continuing without it.");
                goto obfuscation;
            }
            Parser p = new Parser() { ConfigFile = "config.txt" };
            ForceWinForms = bool.Parse(p.Read("ForceWinFormsCompatibility").ReadResponse().ReplaceSpaces());
            DontRename = bool.Parse(p.Read("DontRename").ReadResponse().ReplaceSpaces());

            obfuscation:
            Randomizer.Initialize();

            ForceWinForms = true;
            Module = ModuleDefMD.Load(path);
            FileExtension = Path.GetExtension(path);

            FilePath = path;

            Console.WriteLine($"Renaming...");
            Renamer.Execute();
            Console.WriteLine($"   Done");

            Console.WriteLine($"Adding junk methods...");
            JunkMethods.Execute();
            Console.WriteLine($"   Done");

            //Console.WriteLine("Adding proxy calls...");
            //ProxyAdder.Execute(); // SIRA

            Console.WriteLine($"Encrypting strings...");
            StringEncryption.Execute();
            Console.WriteLine($"   Done");

            Console.WriteLine($"Injecting Anti-Tamper...");
            AntiTamper.Execute();
            Console.WriteLine($"   Done");

            Console.WriteLine($"Executing Anti-De4dot...");
            AntiDe4dot.Execute();
            Console.WriteLine($"   Done");

            Console.WriteLine($"Executing Control Flow...");
            ControlFlow.Execute();
            Console.WriteLine($"   Done");

            Console.WriteLine($"Encoding ints...");
            if (!isCustomObfuscation)
            {
                IntEncoding.Execute(); // partially sira
                Console.WriteLine($"   Done");
            }
            else
            {
                Console.Write($" Skipped");
            }

            Console.WriteLine($"Watermarking...");
            Watermark.AddAttribute();
            Console.WriteLine($"   Done");

            Console.WriteLine($"Saving file...");
            Console.WriteLine($"Writing to: " + pathOutput);

            FilePath = pathOutput;
            Module.Write(Stream, new dnlib.DotNet.Writer.ModuleWriterOptions(Module) { Logger = DummyLogger.NoThrowInstance });
            Console.WriteLine($"   Done");

            Console.WriteLine($"Strip DOS Header...");
            StripDOSHeader.Execute();
            Console.WriteLine($"   Done");

            // Save stream to file
            Console.WriteLine($"Saving file stream...");
            File.WriteAllBytes(pathOutput, Stream.ToArray());
            Console.WriteLine($"   Done");

            Console.WriteLine($"Anti-Tamper Injection...");
            if (AntiTamper.Tampered)
                AntiTamper.Inject(FilePath);
            Console.WriteLine($"   Done");

            Console.WriteLine($"ALL DONE!");
        }
    }
}
