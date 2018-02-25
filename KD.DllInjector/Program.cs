using System;

namespace KD.DllInjector
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Settings: [Path to DLL: \"{ Settings.PathToDll }\", Process name: \"{ Settings.ProcessName }\"]");
            Console.WriteLine("Injecting...");
            DllInjectionResult result = DllInjector.DoWork();

            switch (result)
            {
                case DllInjectionResult.DllNotFound:
                    Console.WriteLine("Couldn't find a DLL at specified path.");
                    break;
                case DllInjectionResult.InjectionFailed:
                    Console.WriteLine("Injection failed.");
                    break;
                case DllInjectionResult.ProcessNotFound:
                    Console.WriteLine("Couldn't find a Process with specified name");
                    break;
                case DllInjectionResult.Success:
                    Console.WriteLine("Injection succeeded.");
                    break;
            }
        }
    }
}