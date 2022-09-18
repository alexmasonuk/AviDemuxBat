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
* USE:         AviDemuxBat.exe "INPUT_PATH" "OUTPUT_PATH"                        *  
* USE (debug): AviDemuxBat.exe "INPUT_PATH" "OUTPUT_PATH" "DEBUGON"              *
* UPDATE LOG:                                                                    *
* 14.09.2022:  Since Ed wants to use the script, trying to make it a bit more    *
*              useable outside of my own computer. Added arguments for input,    *
*              output and also the script to be run. Supports .mov, . avi, .mp4, *
*              and .mkv. More filetypes can be added if needed.  It is also      *
*              possible to launch the batch file from the cmd line (v0.1).       *
* 17.09.2022:  Fixed bug reported by Ed where the application could not be       *
*              executed from outside the working directory of the application    *
*              itself. Paths updated to absolute using the call                  *
*              System.Reflection.Assembly.GetExecutingAssembly().Location.       *
* 18.09.2022:  Added a simple menu so it is no longer necessary  (or possible)   *
*              to provide the script name as an argument, it is selectable       *
*              at runtime. Also added a debug mode.                              *
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
            // Write title.
            Console.WriteLine("<<<<< AviDemuxBat, Alex Mason, v0.3 (18.09.2022) >>>>>");

            // Set up static variables. Note that System.Reflection... call will give the full path
            // to the application, and the executable name must be removed. This allows the application
            // to work properly regardless of where it is called from.
            bool debugOn = false;
            string fullPathToApp = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string pathToApp = System.IO.Path.GetDirectoryName(fullPathToApp) + @"\";
            string pathToScriptsDir = pathToApp + @"scripts\";
            string pathToAviDemux = pathToApp + @"avidemux\AvidemuxPortable.exe";
            string pathToCreateBat = pathToApp + @"output\AviDemuxBatProcess.bat";

            // Process arguments.
            // Check there are sufficient arguments. As of 0.3, there are two mandatory arguments
            // and it is also possible to optionally provide the "DEBUGON" argument to see more output
            // Text during the runtime.
            if (args.Length == 3)
            {
                if (args[2] == "DEBUGON")
                {
                    debugOn = true;
                    Console.WriteLine("DEBUG outputs are turned on.");
                }
            }
            
            
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Incorrect number of arguments. Correct useage is AviDemuxBat.exe <INPUT_PATH> <OUTPUT_PATH> [DEBUGON].");
                if (debugOn)
                {
                    int k = 0;
                    foreach (string s in args)
                    {
                        Console.WriteLine("DEBUG: arg" + k + ": " + s);
                        k++;
                    }
                        
                }
                Console.WriteLine("Exiting.");
                return;
            }
                 

            // Input directory argument, should contain the location where the files to be processed
            // reside. Absolute path is required. Here I just check that the path really exists, I 
            // don't care if there are no files, or no valid files within it.
            string pathToFiles = args[0];
            if (!Directory.Exists(pathToFiles))
            {
                Console.WriteLine("INPUT_PATH does not exist. Check and try again.");
                if (debugOn) Console.WriteLine("DEBUG: INPUT_PATH=" + args[0]);
                Console.WriteLine("Exiting.");
                return;
            }

            // Output directory argument, if it does not exist then it is created.
            string pathToOutput = args[1];
            try
            {
                if (!Directory.Exists(pathToOutput))
                {
                    Directory.CreateDirectory(pathToOutput);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("OUTPUT_PATH could not be created. Check and try again. Exception raised: " + ex.ToString());
                if (debugOn) Console.WriteLine("DEBUG: outPUT_PATH=" + args[1]);
                Console.WriteLine("Exiting.");
                return;
            }

            // Script to use can be selected, a menu is provided. 
            // First we must have the path to the scripts, and get the files in there. We are only
            // interested in those with a *.py extension as that is how AviDemux saves its project
            // files.
            string[] files = Directory.GetFiles(pathToScriptsDir);
            List<string> scriptsList = new List<string>();
            foreach (string s in files)
            {
                if (s.ToLower().Contains(".py"))
                {
                    scriptsList.Add(s);
                    if (debugOn) Console.WriteLine("DEBUG: Found script file " + s);
                }
            }

            // Check we have some *.py files to process
            if (scriptsList.Count == 0)
            {
                Console.WriteLine("No AviDemux project files (*.py) found in  " + pathToScriptsDir + ". Check directory contains the script you want to process.");
                if (debugOn) Console.WriteLine("DEBUG: SCRIPTS_DIR=" + pathToScriptsDir);
                Console.WriteLine("Exiting.");
                return;
            }

            // Enable selection of the script to use
            Console.WriteLine("Select the script to use from the following options:");
            int listCount = 1;
            foreach (string s in scriptsList)
            {
                if (s.ToLower().Contains(".py"))
                {
                    Console.WriteLine(listCount + ". " + Path.GetFileName(s));
                    listCount++;
                }
            }

            // User will be asked over and over if they do not input the correct or 
            // valid value. If they input a string or non-integer value it will be
            // caught in the try/catch statement.
            string pathToScript = "";
            bool done = false;
            while (!done)
            {
                Console.Write("Input selection [1-" + scriptsList.Count + "]: ");
                string response = Console.ReadLine().ToString();
                int itemNo;
                try
                {
                    itemNo = Convert.ToInt32(response);
                }
                catch
                {
                    itemNo = -1;
                    if (debugOn) Console.WriteLine("DEBUG: Input exception caught and handled (itemNo=-1)");
                }

                if (itemNo >= 1 && itemNo <= scriptsList.Count)
                {
                    Console.WriteLine("Using script " + Path.GetFileName(scriptsList[itemNo-1]) + ".");
                    pathToScript = scriptsList[itemNo-1];
                    done = true;
                    break;
                }
                else
                {
                    Console.WriteLine("Response not recognised, try again...");
                }
            }
           
            // Check that the script really exists. It should by this point, but the code is left
            // from previous implementations and so I just leave it here as a final check.
            if (!File.Exists(pathToScript))
            {
                Console.WriteLine("SCRIPT could not be found. Check and try again.");
                if (debugOn) Console.WriteLine("DEBUG: pathToScript=" + pathToScript);
                Console.WriteLine("Exiting");
                return;
            }

            // Now we can start generating the content for the batch file. The batch file will contain statements
            // like that below:
            //START / B / WAIT "" "D:\Dropbox\PortableApps\AvidemuxPortable\AvidemuxPortable.exe"
            //--load "D:\tmp\P1000227.MP4"--run "D:\tmp\process_script.py"--save "D:\tmp\Compressed\P1000227_COMP.MP4"--quit
            Console.WriteLine("Started generating batch file...");
            
            // Get the video files that will be proccessed
            string[] filesToProcess = Directory.GetFiles(pathToFiles);
            List<string> outPut = new List<string>();
            foreach (string f in filesToProcess)
            {
                if (debugOn) Console.WriteLine("DEBUG: Current file has extension: " + Path.GetExtension(f));
                string curExt = Path.GetExtension(f).ToLower();
                if (curExt == ".mov" || curExt == ".avi" || curExt == ".mp4" || curExt == ".mkv")
                {
                    outPut.Add("START /B /WAIT \"\" \"" + pathToAviDemux + "\" --load \"" + f + "\" --run \"" +
                        pathToScript + "\" --save \"" + pathToOutput + "\\" + Path.GetFileName(f) + "\" --quit");
                    outPut.Add("");
                }
            }
            Console.WriteLine("Found " + outPut.Count + " files to process.");

            // Delete existing batch file, since we do not want to append.
            if (File.Exists(pathToCreateBat))
            {
                File.Delete(pathToCreateBat);
                if (debugOn) Console.WriteLine("DEBUG: An old batch file was deleted.");
            }

            // Output batch file
            StreamWriter sw = new StreamWriter(pathToCreateBat);
            foreach (string o in outPut)
            {
                sw.WriteLine(o);
            }
            sw.Close();
            Console.WriteLine("Output complete, batch file written to " + pathToCreateBat + ".");

            // Now we give the option to launch the batch file directly. There are some limitations, like
            // for example both cmd/shell windows should be left open during processing. This program
            // must sit and wait until the batch file has completed entirely.
            done = false;
            while (!done)
            {
                Console.Write("Run batch file? [y/n]: ");
                
                string response = Console.ReadLine().ToString();
                if (response == "y")
                {
                    Console.WriteLine("Launching process " + pathToCreateBat + "...");
                    Process p = new Process();
                    p.StartInfo = new ProcessStartInfo("cmd.exe", "/c \"" + pathToCreateBat + "\"");
                    p.Start();
                    Console.WriteLine("Process started. DO NOT CLOSE THIS WINDOW, OR THE NEW ONE LAUNCHED!!");
                    while(!p.HasExited)
                    {
                        Thread.Sleep(1000);
                    }
                    done = true;
                }
                else if (response == "n")
                {
                    Console.WriteLine("Batch file can be launched manually using the file located in " + pathToCreateBat);
                    done = true;
                }
                else
                {
                    Console.WriteLine("Response not recognised, try again...");
                    if (debugOn) Console.WriteLine("DEBUG: response=" + response);
                }
            }
            Console.WriteLine("Exiting.");
            return;
        }
    }
}
