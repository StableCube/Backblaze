# About

C# client for the Backblaze B2 api.

## Install 

Install the [Nuget package](https://www.nuget.org/packages/StableCube.Backblaze.DotNetClient/) then register service in DI.

#### Startup.cs
```C#
services.AddBackblazeApiClient();
```

## Usage

The IBackblazeUploader class is a higher level client that saves you from dealing with splitting large files and other annoyances. 
If you have a need for finer grained control you may want to look into the IB2ClientV2 class.

```C#
using StableCube.Backblaze.DotNetClient;

public class ClientExample
{
    private IBackblazeUploader _backblazeClient;

    public ClientExample(IBackblazeUploader backblazeClient)
    {
        _backblazeClient = backblazeClient;
    }

    public async Task DoExampleStuffAsync(
        CancellationToken cancellationToken = default(CancellationToken)
    )
    {
        // Authorize with Backblaze
        var authResponse = await _backblazeClient.B2Client.AuthorizeAsync(<KEY_ID>, <APPLICATION_KEY>, cancellationToken);

        // Create object with file source path and destination info on bucket
        var uploadFile = new UploadFile(<BUCKET_ID>, <SOURCE_FILE_PATH>, <FILENAME_IN_BUCKET>);

        // Start upload and track progress with a callback
        var resultResponse = await _backblazeClient.UploadDynamicAsync(
            auth: authResponse.Data,
            file: uploadFile,
            cancellationToken: cancellationToken,
            progressData: new Progress<TransferProgress>((TransferProgress progress) => {
                if(progress.BytesTransferred == progress.TotalBytes)
                    return;

                // Just an example. You should probably rate control this to not spam the console
                Console.WriteLine($"Bytes Transferred: {progress.BytesTransferred}");
            })
        );
    }
}
```