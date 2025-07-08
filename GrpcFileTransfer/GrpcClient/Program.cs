using Grpc.Net.Client;
using GrpcFileTransferDownloadClient;
using System;

namespace GrpcDownloadClient
{
    class Program
    {
        static async Task Main (string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5277");
            var downloadClient = new FileService.FileServiceClient(channel);

            CancellationTokenSource cancellationTokenSource = new();
            int count = 0;
            decimal chunkSize = 0;

            string downloaPath = @"C:\Users\berke\Documents\gRPC_FileTransfer\GrpcFileTransfer\GrpcClient\Downloads";

            var fileInfo = new GrpcFileTransferDownloadClient.FileInfo
            {
                FileName = "chromedriver",
                FileExtension = ".exe"
            };

            FileStream fileStream = null;

            var download = downloadClient.FileDownload(fileInfo);
            
            while (await download.ResponseStream.MoveNext(cancellationTokenSource.Token))
            {
                if (count++ == 0)
                {
                    fileStream = new FileStream(@$"{downloaPath}/{download.ResponseStream.Current.Info.FileName}{download.ResponseStream.Current.Info.FileExtension}", FileMode.CreateNew);
                    fileStream.SetLength(download.ResponseStream.Current.FileSize);
                }

                var buffer = download.ResponseStream.Current.Buffer.ToByteArray();
                await fileStream.WriteAsync(buffer, 0, download.ResponseStream.Current.ReadedByte);
                Console.WriteLine($"{Math.Round((chunkSize += download.ResponseStream.Current.ReadedByte * 100) / download.ResponseStream.Current.FileSize)}%");
            }

            Console.WriteLine("Download Completed!");
            await fileStream.DisposeAsync();

        }
    }
}