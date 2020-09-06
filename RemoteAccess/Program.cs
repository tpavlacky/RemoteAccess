using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RemoteAccess
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Program started...");
			StartRemoteProcess("calc.exe");
			var processes = GetAllRunningProcesses();

			ReportProcessState(processes);

			KillRemoteProcess("calc.exe");

			processes = GetAllRunningProcesses();
			ReportProcessState(processes);

			Console.WriteLine("Loading folder content ...");

			var folderContent = GetFolderContent();

			foreach(var fileInfo in folderContent)
			{
				Console.WriteLine(fileInfo);
			}

			Console.WriteLine();
			Console.WriteLine("Press key to exit");
			Console.ReadKey();
		}

		private static void ReportProcessState(IEnumerable<string> processes)
		{
			if (processes.Any(process => process.Contains("calc.exe")))
			{
				Console.WriteLine("Program running on remote system.");
			}
			else
			{
				Console.WriteLine("Program not running on remote system.");
			}
		}

		private static void StartRemoteProcess(string processName)
		{
			Console.WriteLine("Initializing remote process...");
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/C PsExec64.exe \\\\192.168.0.103 -u Tomas2 -p Tomas -i -d " + processName,
					RedirectStandardOutput = true,
					UseShellExecute = false
				}
			};

			Console.WriteLine("Starting remote process: " + processName);
			process.Start();
			Console.WriteLine("Remote process started");
			Console.WriteLine("Waiting for exit...");
			
			process.WaitForExit();
			Console.WriteLine("Exited");
		}

		private static IEnumerable<string> GetAllRunningProcesses()
		{
			Console.WriteLine("Getting remote processes...");
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/C tasklist /s 192.168.0.103 /u Tomas2 /p Tomas",
					RedirectStandardOutput = true,
					UseShellExecute = false
				}
			};

			process.Start();
			//needs to be before wait for exit to avoid deadlock
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			Console.WriteLine("Remote processes obtained");

			return output.Split(new string[] { "\\n" }, StringSplitOptions.None);
		}

		private static void KillRemoteProcess(string processName)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/C taskkill /s 192.168.0.103 /u Tomas2 /p Tomas /im " + processName,
					RedirectStandardOutput = true,
					UseShellExecute = false
				}
			};

			Console.WriteLine("Killing remote process: " + processName);
			process.Start();
			Console.WriteLine("Waiting for kill to be perfomed...");

			process.WaitForExit();
			Console.WriteLine("Killed!");
		}

		private static IEnumerable<string> GetFolderContent()
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/C PsExec64.exe \\\\192.168.0.103 -u Tomas2 -p Tomas cmd /c dir C:\\Users\\Tomas\\Desktop",
					RedirectStandardOutput = true,
					UseShellExecute = false
				}
			};

			Console.WriteLine("Getting content of the folder ...");
			process.Start();
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			Console.WriteLine("Content of the folder obtained");

			return output.Split(new string[] { "\\n" }, StringSplitOptions.None);
		}
	}
}
