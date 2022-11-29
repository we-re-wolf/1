using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AutoUpdater
{
    //class to access properties in appData.json file
    internal class SecretConsumer
    {
        ConfigurationBuilder _builder = null;
        public string AWSAccessKey
        {
            get
            {
                string returnString = string.Empty;
                try
                {
                    IConfigurationRoot configuration = _builder.Build();
                    returnString = StringProcessor.DecryptString(configuration.GetConnectionString("AWSAccessKey"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return returnString;
            }
        }
        public string AWSSecretKey
        {
            get
            {
                string returnString = string.Empty;
                try
                {
                    IConfigurationRoot configuration = _builder.Build();
                    returnString = StringProcessor.DecryptString(configuration.GetConnectionString("AWSSecretKey"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return returnString;
            }
        }
        public string BucketName
        {
            get
            {
                string returnString = string.Empty;
                try
                {
                    IConfigurationRoot configuration = _builder.Build();
                    returnString = configuration.GetConnectionString("BucketName");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return returnString;
            }
        }
        public string BucketSubFolder
        {
            get
            {
                string returnString = string.Empty;
                try
                {
                    IConfigurationRoot configuration = _builder.Build();
                    returnString = configuration.GetConnectionString("BucketSubFolder");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return returnString;
            }
        }
        public string LocalFolderToUploadFrom
        {
            get
            {
                string returnString = string.Empty;
                try
                {
                    IConfigurationRoot configuration = _builder.Build();
                    returnString = configuration.GetConnectionString("LocalFolderToUploadFrom");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return returnString;
            }
        }
        public string LocalFolderToDownloadTo
        {
            get
            {
                string returnString = string.Empty;
                try
                {
                    IConfigurationRoot configuration = _builder.Build();
                    returnString = configuration.GetConnectionString("LocalFolderToDownloadTo");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return returnString;
            }
        }
        public string PathToAppExecutableToLaunchAfterUpdate
        {
            get
            {
                string returnString = string.Empty;
                try
                {
                    IConfigurationRoot configuration = _builder.Build();
                    returnString = configuration.GetConnectionString("PathToAppExecutableToLaunchAfterUpdate");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return returnString;
            }
        }
        public SecretConsumer()
        {
            this._builder = (ConfigurationBuilder)new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appData.json", optional: false, reloadOnChange: true);
        }
    }
}
