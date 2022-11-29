using AutoUpdater;
using System.Diagnostics;

class Program
{


    static void Main(string[] args)
    {
        // check for flags to allow advanced control
        bool isAdmin = false;
        if(args.Length > 0)
        {
            if (args[0] == "-admin")
                isAdmin = true;
        }

        //Creating S3Sync instance for use with upload and download.
        S3Sync syncer = new S3Sync();

        SecretConsumer secretConsumer = new();

        if (isAdmin)
        {
            string input;
            Start:
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-------Available Commands-------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1 : UpdateLocalFiles\n2 : UpdateServerFiles\n3 : GenerateManifestFile ");
            Console.Write("Enter Command Number and hit enter : ");
            input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    syncer.UpdateLocalFiles();
                    goto Start;
                    break;
                case "2":
                    syncer.UpdateS3Bucket();
                    goto Start;
                    break;
                case "3":
                    Console.Write("Enter Current Version : ");
                    input = Console.ReadLine();
                    if(input == "" || input == null)
                    {
                        goto Start;
                    }
                    Console.WriteLine("Building Manifest file ...");
                    syncer.CreateFilesManifest(input);
                    goto Start;
                    break;
                default:
                    goto Start;
                    break;
            }
        }
        else //if not admin
        {
            // auto update and launch functionality
            syncer.UpdateLocalFiles();
            
            string fileToOpen = Directory.GetCurrentDirectory() + '\\' + secretConsumer.LocalFolderToDownloadTo + '\\' +  secretConsumer.PathToAppExecutableToLaunchAfterUpdate;

            Process.Start(fileToOpen);

            Console.ReadLine();
        }
    }
}