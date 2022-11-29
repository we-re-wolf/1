using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;

namespace AutoUpdater
{
    internal class S3Sync
    {

        SecretConsumer secretConsumer = new SecretConsumer();

        public void UpdateS3Bucket()
        {
            IAmazonS3 client = new AmazonS3Client(secretConsumer.AWSAccessKey, secretConsumer.AWSSecretKey, RegionEndpoint.APSouth1);
            // create a TransferUtility instance passing it the IAmazonS3 created in the first step
            TransferUtility utility = new TransferUtility(client);

            //Download filesData
            Console.WriteLine("Downloading files Data from AWS S3 ...");
            bool isServerFilesDataAvailable = false;
            try
            {
                utility.Download(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles_server.filesData", secretConsumer.BucketName, secretConsumer.BucketSubFolder + "/" + "0");
                Console.WriteLine("Downloaded files Data");
                isServerFilesDataAvailable = true;
            }
            catch
            {

            }           

            //Compare Files
            if (File.Exists(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles_server.filesData") && isServerFilesDataAvailable)
            {

                Console.WriteLine("Comparing Files .....");

                string[] localFiles = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles.filesData");
                string[] serverFiles = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles_server.filesData");

                if (localFiles[0] == serverFiles[0])
                {
                    Console.WriteLine("Server version is same as local version. NO FILES UPDATED");
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles_server.filesData");
                    return;
                }

                List<int> filesToDelete = new List<int>();
                List<int> filesToAdd = new List<int>();

                //Find files to delete and add on server
                for (int i = 0; i < Int32.Parse(serverFiles[1]); i++)
                {
                    if(!localFiles.Contains(serverFiles[2 + i * 3]))
                    {
                        filesToDelete.Add(2+3*i);
                    }
                    else
                    {
                        int tmpIDX = Array.IndexOf(localFiles, serverFiles[2 + i * 3]);
                        if (localFiles[tmpIDX + 1 ] != serverFiles[2 + i * 3 + 1] || localFiles[tmpIDX + 2] != serverFiles[2 + i * 3 + 2])
                        {
                            filesToAdd.Add(tmpIDX);
                        }
                    }
                }
                for (int i = 0; i < Int32.Parse(localFiles[1]); i++)
                {
                    if (!serverFiles.Contains(localFiles[2 + i * 3]))
                    {
                        filesToAdd.Add(2+3*i);
                    }
                }

                //uploading files to server
                int count = 0;
                Console.WriteLine("Uploading " + filesToAdd.Count + " files");
                foreach (int item in filesToAdd)
                {

                    string bucketName = secretConsumer.BucketName + @"/" + secretConsumer.BucketSubFolder;

                    if (item == 2)
                    {
                        utility.Upload(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + localFiles[item], bucketName, "0");
                    }
                    else
                    {
                        utility.Upload(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + localFiles[item], bucketName, localFiles[item + 2] + StringToMD5(localFiles[item]));
                    }
                    
                    count++;
                    Console.WriteLine("File: " + count + " uploaded");
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(filesToAdd.Count + " Files Updated on AWS S3");
                Console.ForegroundColor = ConsoleColor.White;

                //deleting files on server
                foreach (int item in filesToDelete)
                {
                    string bucketName = secretConsumer.BucketName + @"/" + secretConsumer.BucketSubFolder;
                    if(item==2)
                    {
                        client.DeleteObjectAsync(bucketName, "0");
                    }
                    else
                    {
                        client.DeleteObjectAsync(bucketName, serverFiles[item + 2] + StringToMD5(serverFiles[item]));
                    }
                   
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(filesToDelete.Count + " Files Deleted on AWS S3");
                Console.ForegroundColor = ConsoleColor.White;

                //delete the files manifest file downloaded from s3
                File.Delete(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles_server.filesData");
            }
            else
            {
                //Uploading all files to S3 Bucket
                string bucketName = secretConsumer.BucketName + @"/" + secretConsumer.BucketSubFolder;
                client.DeleteBucketAsync(bucketName);
                string[] localFiles = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles.filesData");

                Console.WriteLine("Uploading " + localFiles[1] +" files");
                int count = 0;
                for (int i = 2; i < localFiles.Length; i=i+3 )
                {
                    if(i==2)
                    {
                        utility.Upload(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + localFiles[i], bucketName, "0");
                    }
                    else
                    {
                        utility.Upload(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + localFiles[i], bucketName, localFiles[i + 2] + StringToMD5(localFiles[i]));
                    }
                    
                    count++;
                    Console.WriteLine("File: " + count + " uploaded");
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("All Files Updated on AWS S3");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public void UpdateLocalFiles()
        {
            IAmazonS3 client = new AmazonS3Client(secretConsumer.AWSAccessKey, secretConsumer.AWSSecretKey, RegionEndpoint.APSouth1);
            // create a TransferUtility instance passing it the IAmazonS3 created in the first step
            TransferUtility utility = new TransferUtility(client);

            //Download filesData
            Console.WriteLine("Downloading files Data from AWS S3 ...");
            bool isServerFilesDataAvailable = false;
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + '\\' + secretConsumer.LocalFolderToDownloadTo + '\\');
            try
            {
                utility.Download(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles_server.filesData", secretConsumer.BucketName, secretConsumer.BucketSubFolder + "/" + "0");
                Console.WriteLine("Downloaded files Data");
                isServerFilesDataAvailable = true;
            }
            catch
            {

            }

            //Compare Files
            if (File.Exists(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles_server.filesData") && isServerFilesDataAvailable)
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles.filesData"))
                {
                    Console.WriteLine("Comparing Files .....");

                    string[] localFiles = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles.filesData");
                    string[] serverFiles = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles_server.filesData");

                    if (localFiles[0] == serverFiles[0])
                    {
                        Console.WriteLine("Server version is same as local version. NO FILES UPDATED");
                        //delete the files manifest file downloaded from s3
                        File.Delete(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles_server.filesData");
                        return;
                    }

                    List<int> filesToDelete = new List<int>();
                    List<int> filesToAdd = new List<int>();

                    //Find files to delete and add on server
                    for (int i = 0; i < Int32.Parse(localFiles[1]); i++)
                    {
                        if (!serverFiles.Contains(localFiles[2 + i * 3]))
                        {
                            filesToDelete.Add(2 + i * 3);
                        }
                        else
                        {
                            int tmpIDX = Array.IndexOf(serverFiles, localFiles[2 + i * 3]);
                            if (serverFiles[tmpIDX + 1] != localFiles[2 + i * 3 + 1] || serverFiles[tmpIDX + 2] != localFiles[2 + i * 3 + 2])
                            {
                                filesToAdd.Add(tmpIDX);
                            }
                        }
                    }
                    for (int i = 0; i < Int32.Parse(serverFiles[1]); i++)
                    {
                        if (!localFiles.Contains(serverFiles[2 + i * 3]))
                        {
                            filesToAdd.Add(2 + i * 3);
                        }
                    }




                    //deleting files locally
                    string fileFullPath = "";
                    foreach (int item in filesToDelete)
                    {
                        fileFullPath = Directory.GetCurrentDirectory() + '\\' + secretConsumer.LocalFolderToDownloadTo + '\\' + localFiles[item];
                        if(File.Exists(fileFullPath))
                        {
                            File.Delete(fileFullPath);
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(filesToDelete.Count + " Outdated Files Deleted locally");
                    Console.ForegroundColor = ConsoleColor.White;


                    //downloading files from server
                    int count = 0;
                    Console.WriteLine("Downloading " + filesToAdd.Count + " files");
                    foreach (int item in filesToAdd)
                    {
                        string bucketName = secretConsumer.BucketName + @"/" + secretConsumer.BucketSubFolder;
                        if(item==2)
                        {
                            utility.Download(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + serverFiles[item], bucketName, "0");
                        }
                        else
                        {
                            utility.Download(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + serverFiles[item], bucketName, serverFiles[item+2]+ StringToMD5(serverFiles[item]));
                        }
                       
                        count++;
                        Console.WriteLine("File: " + count + " downloaded");
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(filesToAdd.Count + " Files downloaded from server");
                    Console.ForegroundColor = ConsoleColor.White;


                    //delete the files manifest file downloaded from s3
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles_server.filesData");
                }
                else
                {
                    //Copy all files from server to local
                    string bucketName = secretConsumer.BucketName + @"/" + secretConsumer.BucketSubFolder;
                    string[] serverFiles = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles_server.filesData");

                    Console.WriteLine("Downloading " + serverFiles[1] + " files");
                    int count = 0;
                    for (int i = 2; i < serverFiles.Length; i = i + 3)
                    {
                        if(i==2)
                        {
                            utility.Download(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + serverFiles[i], bucketName, "0");
                        }
                        else
                        {
                            utility.Download(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + serverFiles[i], bucketName, serverFiles[i + 2] + StringToMD5(serverFiles[i]));
                        }
                        
                        count++;
                        Console.WriteLine("File: " + count + " dowloaded");
                    }
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("All Files downloaded from server");
                    Console.ForegroundColor = ConsoleColor.White;

                    //delete the files manifest file downloaded from s3
                    File.Delete(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToDownloadTo + "\\" + "CurrentFiles_server.filesData");
                }
            }
            else
            {
                return;
            }
        }
        public void CreateFilesManifest(string version)
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom, "*", SearchOption.AllDirectories);
            string[] fileSizes = new string[files.Length];
            string[] fileMD5s = new string[files.Length];


            // Create a file to write to.
            string FilesManifestData = version + Environment.NewLine;

            if(File.Exists(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles.filesData"))
                FilesManifestData += files.Length.ToString() + Environment.NewLine;
            else
                FilesManifestData += (files.Length + 1).ToString() + Environment.NewLine;

            //adding self
            FilesManifestData += "CurrentFiles.filesData" + Environment.NewLine;
            FilesManifestData += 0 + Environment.NewLine;
            FilesManifestData += version + Environment.NewLine;

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fi = new FileInfo(files[i]);
                if(fi.Extension != ".filesData")
                {
                    fileSizes[i] = fi.Length.ToString();
                    fileMD5s[i] = CalculateMD5(files[i]);
                    FilesManifestData += MakeRelative(files[i], Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\") + Environment.NewLine;
                    FilesManifestData += fileSizes[i] + Environment.NewLine;
                    FilesManifestData += fileMD5s[i] + Environment.NewLine;
                }                
            }

            //save files manifest file
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\" + secretConsumer.LocalFolderToUploadFrom + "\\" + "CurrentFiles.filesData", FilesManifestData);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Files Manifest Created :)");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public static string StringToMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                // StringBuilder sb = new System.Text.StringBuilder();
                // for (int i = 0; i < hashBytes.Length; i++)
                // {
                //     sb.Append(hashBytes[i].ToString("X2"));
                // }
                // return sb.ToString();
            }
        }

        public static string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return Uri.UnescapeDataString(referenceUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
