using System;
using System.IO;
using dnlib.DotNet;
using LoGiC.NET.Protections;
using SharpConfigParser;
using LoGiC.NET.Utils;

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
            //args[9] = "C:\\CWTProjects\\bpg-cbt-power-express\\PL_Express\\bin\\x86\\Debug\\PowerExpress.exe";
            //args[1] = "C:\\CWTProjects\\bpg-cbt-power-express\\PL_Express\\Obfuscator\\Processed\\";

            string path = args[0];
            string pathOutput = args[1] + Path.GetFileName(path);

            Console.WriteLine("Obfuscate Start:\nFILE : " + path);
            Console.WriteLine("OUTPUT : " + pathOutput);

            Console.WriteLine("Preparing obfuscation...");
            if (!File.Exists("config.txt"))
            {
                Console.WriteLine("Config file not found, continuing without it.");
                goto obfuscation;
            }
            Parser p = new Parser() { ConfigFile = "config.txt" };
            ForceWinForms = bool.Parse(p.Read("ForceWinFormsCompatibility").ReadResponse().ReplaceSpaces());
            DontRename = bool.Parse(p.Read("DontRename").ReadResponse().ReplaceSpaces());

            Randomizer.Initialize();

            obfuscation:
            ForceWinForms = true;
            Module = ModuleDefMD.Load(path);
            FileExtension = Path.GetExtension(path);

            Console.WriteLine("Renaming...");
            Renamer.Execute();

            Console.WriteLine("Adding junk methods...");
            JunkMethods.Execute();

            Console.WriteLine("Adding proxy calls...");
            //ProxyAdder.Execute();

            Console.WriteLine("Encrypting strings...");
            StringEncryption.Execute();

            Console.WriteLine("Injecting Anti-Tamper...");
            AntiTamper.Execute();

            Console.WriteLine("Executing Anti-De4dot...");
            AntiDe4dot.Execute();

            Console.WriteLine("Executing Control Flow...");
            ControlFlow.Execute();

            Console.WriteLine("Encoding ints...");
            IntEncoding.Execute();

            Console.WriteLine("Watermarking...");
            Watermark.AddAttribute();

            Console.WriteLine("Saving file...");
            Console.WriteLine("Writing to: " + pathOutput);

            FilePath = pathOutput;
            Module.Write(Stream, new dnlib.DotNet.Writer.ModuleWriterOptions(Module) { Logger = DummyLogger.NoThrowInstance });

            StripDOSHeader.Execute();

            // Save stream to file
            File.WriteAllBytes(pathOutput, Stream.ToArray());

            //if (AntiTamper.Tampered)
            //    AntiTamper.Inject(FilePath);

            Console.WriteLine("Done!");
        }
    }
}
