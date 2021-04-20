using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KLDiscovery
{
    class Program
    {
        static void Main(string[] args)
        {
            string directoryPath = AskUserForDirectory("Enter Directory Path: ");
            string outputFilePath = AskUserForOutputFilePath("Enter Output File Path (Ex. C:/temp/output.csv): ");
            bool includeSubdirectories = AskUserForFlag("Include subdirectories within input directory? (Y/N): ");

            List<Tuple<string, string>> files = GetFilesFromDirectory(directoryPath, includeSubdirectories);

            if(files.Count > 0)
            {
                CreateCSV(files, outputFilePath);
            } 
            else
            {
                Console.WriteLine("No PDF or JPG's");
            }
            


        }

        /// <summary>
        /// Prompts user to enter a valid directory and if invalid then will keep asking until a valid directory is inputted.
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns>valid directory path</returns>
        public static string AskUserForDirectory(string prompt)
        {
            Console.WriteLine(prompt);
            string userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput) || !Directory.Exists(userInput))
            {
                AskUserForDirectory("Please enter a valid directory path: ");
            }

            return userInput;
        }

        /// <summary>
        /// Prompts user to enter a valid file path with file name & extension and if invalid then will keep asking until a valid path is inputted.
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public static string AskUserForOutputFilePath(string prompt)
        {
            Console.WriteLine(prompt);
            string userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput))
            {
                AskUserForOutputFilePath("Enter Output File Path (Ex. C:/temp/output.csv): ");
            }
            else if (File.Exists(userInput))
            {
                AskUserForOutputFilePath("File already exists please enter another file name with path: ");
            }

            return userInput;
        }

        /// <summary>
        /// Asks user for input based on the question param. Then returns the user input or asks the user the same question if input was invalid.
        /// </summary>
        /// <param name="question"></param>
        /// <returns>user input</returns>
        private static bool AskUserForFlag(string prompt)
        {
            Console.WriteLine(prompt);
            string userInput = Console.ReadLine();
            bool includeSubDirs = false;
            if(string.IsNullOrWhiteSpace(userInput))
            {
                AskUserForFlag(prompt);
            }
            
            if (userInput.ToUpper() == "Y" || userInput.ToUpper() == "YES")
            {
                includeSubDirs = true;

            }
            else if (userInput.ToUpper() == "N" || userInput.ToUpper() == "NO")
            {
                includeSubDirs = false;
            }
            else
            {
                AskUserForFlag(prompt);
            }

            return includeSubDirs;

        }

        /// <summary>
        /// Loop through given directory files (depending on flag include sub directories)
        /// Also passing each file to check whether it is a pdf or jpg before adding to list
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="includeSubDirs"></param>
        /// <returns>List of all files in the directory that are jpg or pdf</returns>
        public static List<Tuple<string, string>> GetFilesFromDirectory(string directoryPath, bool includeSubDirs)
        {
            List<Tuple<string, string>> files = new List<Tuple<string, string>>();
            string isJPG_PDF = "";

            if(includeSubDirs)
            {
                foreach (string path in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    isJPG_PDF = CheckFileSignature(path);
                    if (!String.IsNullOrWhiteSpace(isJPG_PDF))
                    {

                        files.Add(new Tuple<string, string> (path, isJPG_PDF));
                    }
                }

            }
            else
            {
                foreach(string path in Directory.EnumerateFiles(directoryPath, "*", SearchOption.TopDirectoryOnly))
                {
                    isJPG_PDF = CheckFileSignature(path);
                    if (!String.IsNullOrWhiteSpace(isJPG_PDF))
                    {
                        files.Add(new Tuple<string, string>(path, isJPG_PDF));
                    }
                    
                }

            }

            return files;
        }

        /// <summary>
        /// Check to see if the file signature is of pdf or jpg
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>pdf jpg or null</returns>
        public static string CheckFileSignature(string filename)
        {
            int bytesUsed = 4;
            byte[] buffer;
            string result = "";
            string isJPG_PDF = "";

            using (FileStream filesStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(filesStream))
                    buffer = binaryReader.ReadBytes(bytesUsed);
            }
            string fileSignature = BitConverter.ToString(buffer);
            result = fileSignature.Replace("-", String.Empty).ToLower();

            if (result == "ffd8ffe0")
            {
                isJPG_PDF = "jpg";
            }
            else if (result == "25504446")
            {
                isJPG_PDF = "pdf";
            }
            else
            {
                isJPG_PDF = null;
            }

            return isJPG_PDF;
        }

        public static void CreateCSV(List<Tuple<string, string>> files, string outputPath)
        {
            string fullPathToFile = "";
            string fileType = "";
            string MD5Hash = "";
            StringBuilder csv = new StringBuilder();

            foreach(Tuple<string, string> x in files)
            {
                fullPathToFile = x.Item1; // Item1 is file path
                fileType = x.Item2; // Item2 is file type pdf or jpg
                MD5Hash = GetMD5Hash(x.Item1);

                csv.AppendLine(string.Format("{0}, {1}, {2}", fullPathToFile, fileType, MD5Hash));

            }

            File.WriteAllText(outputPath, csv.ToString());
            Console.WriteLine("CSV Processed");

        }

        /// <summary>
        /// Convert file to md5 hash
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>string of hash</returns>
        public static string GetMD5Hash(string filePath)
        {
            byte[] myHash;
            StringBuilder sb = new StringBuilder();
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        myHash = md5.ComputeHash(stream);
                        for (int i = 0; i < myHash.Length; i++)
                            sb.Append(myHash[i].ToString("x2"));
                    }
                }
            } 
            catch (Exception ex)
            {
                Console.WriteLine("Dev Error: " + ex.ToString());
            }
            

            return sb.ToString();

        }

    }
}
