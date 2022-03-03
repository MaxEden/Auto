using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;

namespace Auto.Android
{
    public class AndroidBuild
    {
        protected void AdbInstallOnDevice(Logger logger, FileInfo apkPath)
        {
            CLI.Run(logger, "adb", "-d", "install", "-r", apkPath.FullName);
        }

        public void UploadAabToInternalTest(string credentialsPath, string aabPath, string packageName)
        {
            GoogleCredential credential = GoogleCredential.FromFile(credentialsPath).CreateScoped(new[]
            {
                AndroidPublisherService.Scope.Androidpublisher
            });

            var service = new AndroidPublisherService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DeployBot",
            });

            var edit = service.Edits.Insert(new AppEdit()
                {
                    ExpiryTimeSeconds = (DateTimeOffset.Now + TimeSpan.FromHours(1)).ToUnixTimeSeconds().ToString()
                },
                packageName).Execute();

            // var listing = service.Edits.Listings.Update(new Listing()
            //     {
            //         Language = "en-US",
            //         Title = GameName,
            //         FullDescription = GameName +" Test",
            //         ShortDescription = GameName +" Test",
            //     },
            //     packageName,
            //     edit.Id,
            //     "en-US").Execute();

            var upload = service.Edits.Bundles.Upload(packageName,
                edit.Id,
                new FileStream(
                    aabPath,
                    FileMode.Open),
                "application/octet-stream");

            Bundle uploadedBundle = null;
            upload.ResponseReceived += bundle => uploadedBundle = bundle;
            upload.ProgressChanged += p => Console.Write("=");
            Console.WriteLine("Upload start:");

            var task = upload.UploadAsync();
            while(!(task.IsCompleted || task.IsCanceled))
            {
                Thread.Sleep(100);
            }

            int retry = 1;
            while(task.IsCanceled && retry < 3)
            {
                Console.WriteLine("Retry... #" + retry);
                task = upload.UploadAsync();
                while(!task.IsCompleted)
                {
                    Thread.Sleep(100);
                }
            }

            //var result = uploadB.Upload();
            var result = task.Result;

            if(result.Exception != null || result.Status != UploadStatus.Completed || uploadedBundle == null)
            {
                Console.WriteLine("File upload failed.");
                return;
            }
            else
            {
                Console.WriteLine("File uploaded successfully!");
            }

            var timeStamp = DateTime.Now.ToLocalTime().ToShortDateString() + " " +
                            DateTime.Now.ToLocalTime().ToShortTimeString();

            var release = new TrackRelease()
            {
                Name = $"Auto upload of {uploadedBundle.VersionCode}",
                ReleaseNotes = new List<LocalizedText>()
                {
                    new LocalizedText()
                    {
                        Language = "en-US",
                        Text = $"Uploaded {uploadedBundle.VersionCode} on: {timeStamp}"
                    }
                },
                VersionCodes = new List<long?>()
                {
                    //uploadedApk.VersionCode
                    uploadedBundle.VersionCode
                },
                //Status = "draft",
                Status = "completed",
                //UserFraction = 0,
//                CountryTargeting = new CountryTargeting()
//                {
//                    Countries = new List<string>(),
//                    IncludeRestOfWorld = true
//                }
            };

            var trackName = "internal";

            service.Edits.Tracks.Update(
                    new Track()
                    {
                        Releases = new List<TrackRelease>() { release }
                    },
                    packageName,
                    edit.Id,
                    trackName)
                .Execute();

            service.Edits.Commit(packageName, edit.Id).Execute();

            Console.WriteLine(
                $"Successfully uploaded {packageName} ver.{uploadedBundle.VersionCode} on track {trackName} at {timeStamp}");
        }
    }
}