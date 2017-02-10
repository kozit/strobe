﻿using StrobeVM.DIF;
using StrobeVM;
using StrobeVM.Firmware;
using System.IO;
using System;

namespace strvmc
{
	/// <summary>
	/// Strobe VMC.
	/// </summary>
	public class StrobeVMC
	{
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="param">The command-line arguments.</param>
		public static void Main(string[] param)
		{
            bool debug = false;
			// The kernel has 1MB memory, change this if you want to.
			Kernel kernel = new Kernel(1024 * 1024);
			int i = 0;

			// Check for the console input
			foreach (string s in param)
			{
				// Switch the string
				switch (s.ToLower())
				{
                    // Show out the parsed executable
                    case "--debug":
                        debug = true;
                        break;
					// Save the loaded bytes to a DIF file
				case "--save":
					Executeable[] Execs = kernel.Save ();
					int n=0;
					foreach (Executeable e in Execs) {
						File.WriteAllBytes("bin" + n + ".dif",new DIFFormat().GetBytes(e));
					}
					break;
					// Kernel 512MB memory
				case "--512m":
					kernel = new Kernel(1024 * 1024 * 512);
					break;
					// Kernel 32MB memory
				case "--32m":
					kernel = new Kernel(1024 * 1024 * 32);
					break;
					// Kernel 1MB memory
				case "--1m":
					kernel = new Kernel(1024 * 1024);
					break;
					// Kernel 1GB memory
				case "--1g":
					kernel = new Kernel(1024 * 1024 * 1024);
					break;
					// Bios setup
				case "--bios":
					kernel.Start(new DIFFormat().Load(BIOS.Setup()));
					break;
					// Load File
					default:
						try
						{
							// Load the executeable using the DIF Format
							Executeable x = new DIFFormat().Load(File.ReadAllBytes(s));
							// Start the application in the kernel
							kernel.Start(x);
						}
						catch(Exception e)
						{
                            if (s.Length > 2)
                            // Show output
                            System.Console.WriteLine("{1} @ {0}", s, e.Message);
							// Error while loading, increase the counter
							i++;
						}
					break;
				}
			}

            if (debug)
            {
                foreach (Executeable e in kernel.Save())
                {
                    var x = e.CPU();

                    foreach (var y in x)
                    {
                        System.Console.WriteLine("{0}: {1}", y.Op, System.Convert.ToBase64String(y.Param));
                    }
                }
            }

			// Step the kernel while it has running processes...
			while (kernel.running.Count > 0)
			{
				try
				{
					kernel.Step();
				}
				catch (System.Exception e)
				{
					System.Console.WriteLine("VM ERROR: {0}",e.Message);
					System.Environment.Exit(1);
				}
			}
		}
	}
}
