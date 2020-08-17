using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattlefieldRespawnClicker
{
    class Program
    {
        static readonly int Interval = 3;
        static readonly int HoldTime = 2;
        static Win32.VirtualKeys DefaultKey = Win32.VirtualKeys.Numpad0;

        static void Main(string[] args)
        {
            Console.WriteLine("### BattlefieldRespawnClicker ver. {0} ###", Assembly.GetEntryAssembly().GetName().Version.ToString());

            Console.WriteLine("Parsing key...");
            Win32.VirtualKeys key = DefaultKey;
            if (args.Length > 0)
            {
                if (Enum.TryParse(args[0], out key))
                    Console.WriteLine("Custom key defined. Using: \"{0}\"", key.ToString());
                else
                    Console.WriteLine("Failed to parse cunsom key! Using default: \"{0}\"", DefaultKey.ToString());
            }
            else
                Console.WriteLine("No custom key defined. Using default: \"{0}\"", DefaultKey.ToString());


            Console.WriteLine("Getting process");
            Process[] processes = Process.GetProcesses();
            Process process = processes.First(p => p.ProcessName.StartsWith("bf") && p.MainWindowTitle.StartsWith("Battlefield"));
            if (process == null)
            {
                Console.WriteLine("No process found!");
                return;
            }
            process.EnableRaisingEvents = true;
            process.Exited += ProcessExitedCallback;

            Console.WriteLine("Getting handle");
            IntPtr handle = process.MainWindowHandle;
            Console.WriteLine("Process handle: {0}", handle.ToString());

            Console.WriteLine("Clicking loop...");
            bool isClicking = false;
            bool isInvert = false;
            Point mouse = Point.Empty;
            int offset = 3;
            while (true)
            {
                if (!WindowsManager.IsWindowInFocus(handle))
                    continue;

                if (WindowsManager.IsKeyPressed(key))
                {
                    isClicking = !isClicking;
                    if (isClicking)
                    {
                        Console.WriteLine("Clicking enabled");
                        mouse = WindowsManager.GetMousePosition();
                    }
                    else
                        Console.WriteLine("Clicking disabled");
                }

                if (isClicking)
                {
                    isInvert = !isInvert;
                    Point pos = new Point(mouse.X + (isInvert ? offset : -offset), mouse.Y);
                    WindowsManager.MoveMouse(pos);
                    WindowsManager.MouseClick(handle, HoldTime);
                    Thread.Sleep(Interval);
                }
            }
        }

        private static void ProcessExitedCallback(object sender, EventArgs e)
        {
            Console.WriteLine("Target process exited");
            Environment.Exit(0);
        }
    }
}