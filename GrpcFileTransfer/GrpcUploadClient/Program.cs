using Google.Protobuf;
using Grpc.Net.Client;
using GrpcFileTransferUploadClient;
using System.Threading.Tasks;

namespace GrpcUploadClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5277");
            var uploadClient = new FileService.FileServiceClient(channel);

            string file = @"C:\Users\berke\Documents\Test\chromedriver.exe";

            decimal chunkSize = 0;

            FileStream fileStream = new(file, FileMode.Open);
            var bytesContent = new BytesContent
            {
                FileSize = fileStream.Length,
                ReadedByte = 0,
                Info = new GrpcFileTransferUploadClient.FileInfo { FileName = Path.GetFileNameWithoutExtension(fileStream.Name), FileExtension = Path.GetExtension(fileStream.Name) }
            };

            var upload = uploadClient.FileUpload();
            byte[] buffer = new byte[2048];

            while((bytesContent.ReadedByte = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                bytesContent.Buffer = ByteString.CopyFrom(buffer);
                await upload.RequestStream.WriteAsync(bytesContent);
            }
            await upload.RequestStream.CompleteAsync();
            var response = await upload.ResponseAsync;
        }
    }
}