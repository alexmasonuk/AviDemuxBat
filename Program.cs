/********************************************************************************* 
* Copyright (C) 2022 Alex Mason - All Rights Reserved.                           *
*                                                                                *
* This code may not be used, redistributed or modified without the author's      *
* express permission. The code is provided "as-is" without any warranty or       * 
* liability. Use of the code is entirely at your own risk.                       *
*                                                                                * 
* If you wish to use, redistribute or modify this code, then please do contact   *
* the author who will be happy to discuss the matter. You can do that in writing *
* by email: alexweborders@googlemail.com.                                        *
*                                                                                *
*********************************************************************************/

/********************************************************************************* 
* Author: Alex Mason                                                             *
* Contact: alexweborders@googlemail.com                                          *
* First created: 11.01.2022                                                      *
* Last Updated: 14.09.2022                                                       *
* Description: AviDemuxBat. The script runs from the command line, taking in     *
*              the input and output directories, as well as the name of the      *
*              script that should run. The script should lay in the /scripts     * 
*              directory. A batch file is created in the /output directory.      * 
* USE:         AviDemuxBat.exe "INPUT_PATH" "OUTPUT_PATH" "SCRIPT"               *             
* UPDATE LOG:                                                                    *
* 14.09.2022:  Since Ed wants to use the script, trying to make it a bit more    *
*              useable outside of my own computer. Added arguments for input,    *
*              output and also the script to be run. Supports .mov, . avi, .mp4, *
*              and .mkv. More filetypes can be added if needed.  It is also      *
*              possible to launch the batch file from the cmd line (v0.1).       *
*                                                                                *
*********************************************************************************/

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AviDemuxBat
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("Running AviDemuxBat process by Alex Mason, v0.1.");

            // Set up static variable
            string pathToCreateBat = @"output\AviDemuxBatProcess.bat";
            string pathToApp = Directory.GetCurrentDirectory() + @"\";
            string pathToAviDemux = pathToApp +  @"avidemux\AvidemuxPortable.exe";

            // Process arguments
            if (args.Length != 3)
            {
                Console.WriteLine("Not enough input arguments. Correct useage is AviDemuxBat.exe <INPUT_PATH> <OUTPUT_PATH> <SCRIPT>.");
                return;
            }    
            
            // Input directory
            string pathToFiles = args[0];
            if (!Directory.Exists(pathToFiles))
            {
                Console.WriteLine("INPUT_PATH does not exist. Check and try again.");
                return;
            }

            // Output directory
            string pathToOutput = args[1];
            try
            {
                if(!Directory.Exists(pathToOutput))
                {
                    Directory.CreateDirectory(pathToOutput);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OUTPUT_PATH could not be created. Check and try again. Exception raised: " + ex.ToString());
                return;
            }

            // Script to use
            string pathToScript = pathToApp + "scripts\\" + args[2];
            if(!File.Exists(pathToScript))
            {
                Console.WriteLine("SCRIPT could not be foudn. Check and try again.");
                return;
            }

            // Get the video files that will be proccessed
            string[] filesToProcess = Directory.GetFiles(pathToFiles);
            List<string> outPut = new List<string>();
            foreach (string f in filesToProcess)
            {
                //Console.WriteLine("Current file has extension: " + Path.GetExtension(f));
                string curExt = Path.GetExtension(f).ToLower();
                if (curExt == ".mov" || curExt == ".avi" || curExt == ".mp4" || curExt == ".mkv")
                {
                    //Example:
                    //START / B / WAIT "" "D:\Dropbox\PortableApps\AvidemuxPortable\AvidemuxPortable.exe"
                    //--load "D:\tmp\P1000227.MP4"--run "D:\tmp\process_script.py"--save "D:\tmp\Compressed\P1000227_COMP.MP4"--quit
                    outPut.Add("START /B /WAIT \"\" \"" + pathToAviDemux + "\" --load \"" + f + "\" --run \"" +
                        pathToScript + "\" --save \"" + pathToOutput + "\\" + Path.GetFileName(f) + "\" --quit");
                    outPut.Add(""); // space just for readability
                }
            }

            Console.WriteLine("Found " + outPut.Count + " files to process.");

            // Delete existing batch file
            if (File.Exists(pathToCreateBat))
                File.Delete(pathToCreateBat);

            // Output batch file
            StreamWriter sw = new StreamWriter(pathToCreateBat);
            foreach (string o in outPut)
                sw.WriteLine(o);
            sw.Close();

            Console.WriteLine("Output complete, batch file written to " + pathToCreateBat + ".");

            bool done = false;
            while (!done)
            {
                Console.Write("Run batch file? [y/n]");
                
                string response = Console.ReadLine().ToString();
                if (response == "y")
                {
                    Console.WriteLine("Launching process " + pathToApp + pathToCreateBat + "...");
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo("cmd.exe", "/c \"" + (pathToApp + pathToCreateBat) + "\"");
                    p.Start();
                    Console.WriteLine("Process started, do not close this window.");
                    while(!p.HasExited)
                    {
                        Thread.Sleep(1000);
                    }
                    done = true;
                }
                else if (response == "n")
                {
                    done = true;
                }
                else
                {
                    Console.WriteLine("Response not recognised, try again...");
                }
            }
            Console.WriteLine("Exiting.");
            return;
        }
    }
}
