using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using log4net;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UploadService
{
    public class UploadHandler
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(UploadHandler));
        private IConfiguration _uploadConfig;
        private DriveService _driveService;
        public UploadHandler(IConfiguration uploadConfig) 
        {
            _uploadConfig = uploadConfig;
            _logger.Info("Setting up Upload Handler, Getting Credential using FileStream");
            var appDataDir = Environment.GetEnvironmentVariable("APPDATA");
            UserCredential credential;
            // Load client secrets.
            using (var stream =
                   new FileStream($"{appDataDir}\\gcloud\\cred.json", FileMode.Open, FileAccess.Read))
            {
                /* The file token.json stores the user's access and refresh tokens, and is created
                 automatically when the authorization flow completes for the first time. */
                string credPath = "token.json";
                string[] Scopes = { DriveService.Scope.Drive };
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
           
            _logger.Info("Getting Drive Service");
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "FileUploadService"
            });
            _logger.Info("Upload handler is ready");
        }
        public void Upload(string filePath)
        {
            _logger.Info("Uploading file: " + filePath);
            try
            {
                FilesResource.CreateMediaUpload request;
                using (var stream = new FileStream(filePath,
                              FileMode.Open))
                {                    
                    Google.Apis.Drive.v3.Data.File fileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = Path.GetFileName(filePath),
                        Parents = new[] { "1XCPz5vQVos37HWnQy-KvBjOBpE4OSMO5" }
                    };

                    
                    // Create a new file, with metadata and stream.
                    request = _driveService.Files.Create(
                        fileMetadata, stream, GetMIMEFileType(filePath));
                    request.Fields = "id,name,parents";
                    var progress = request.Upload();
                    if (progress.Status == UploadStatus.Failed)
                    {
                        _logger.Info("Upload failed: " + progress.Exception);
                    }
                    else if (progress.Status == UploadStatus.Completed)
                    {
                        _logger.Info("File Upload Completed");
                    }
                    else _logger.Info($"Progress Status: {progress.Status}");

                }

                var file = request.ResponseBody;
                _logger.Info($"Upload Successful, File ID: {file?.Id}, {file?.Name}");
            }
            catch(Exception e)
            {
                _logger.Info($"File Upload Failed with error {e}");
            }
        }

        private string GetMIMEFileType(string filePath)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = "application/octet-stream"; // fallback
            }

            return contentType;
        }
    }

  
}
