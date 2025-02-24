using System;
using System.IO;
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;

public class ExampleWriteFilesRequest : FileOps.FileOperationRequest
{
    public string myTestData = "This is some text test data which will be written to a file.";
    public string myOtherTestData = "This is some more text which is written to another save file.";
    public byte[] largeData = new byte[1024 * 1024 * 40];

    public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
    {
      //  OnScreenLog.Add("DoFileOperations start");

        ExampleWriteFilesResponse fileResponse = response as ExampleWriteFilesResponse;

        string outpath = mp.PathName.Data + "/MySaveFile.txt";

        File.WriteAllText(outpath, myTestData);

        FileInfo info = new FileInfo(outpath);
        fileResponse.totalFileSizeWritten = info.Length;

        string outpath2 = mp.PathName.Data + "/MyOtherSaveFile.txt";

        File.WriteAllText(outpath2, myOtherTestData);

        info = new FileInfo(outpath2);
        fileResponse.totalFileSizeWritten += info.Length;

        string outpath3 = mp.PathName.Data + "/Data.dat";

        int totalWritten = 0;

        // Calculate the size of chunks to write out so the progress bar can be updated while the large data is being written.
        // In this case lets make it write out 5% of the file each loop.
        int chunkSize = largeData.Length / 20; 

        // Example of updating the progress value.
        using (FileStream fs = new FileStream(outpath3, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 1024 * 1024 * 5)) // ; File.Create(outpath3, 1024 * 1024 * 5)) // File.OpenWrite(outpath3))
        {
            // Add some information to the file.
            while (totalWritten < largeData.Length)
            {
                int writeSize = Math.Min(largeData.Length - totalWritten, chunkSize);

                fs.Write(largeData, totalWritten, writeSize);

                totalWritten += writeSize;

                // Update progress value during saving
                response.UpdateProgress((float)totalWritten / (float)largeData.Length);
            }
        }

        info = new FileInfo(outpath3);
        fileResponse.lastWriteTime = info.LastWriteTime;
        fileResponse.totalFileSizeWritten += info.Length;
    }
}

public class ExampleWriteFilesResponse : FileOps.FileOperationResponse
{
    public DateTime lastWriteTime;
    public long totalFileSizeWritten;
}

public class ExampleEnumerateFilesRequest : FileOps.FileOperationRequest
{
    public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
    {
        ExampleEnumerateFilesResponse fileResponse = response as ExampleEnumerateFilesResponse;

        string outpath = mp.PathName.Data;

        fileResponse.files = Directory.GetFiles(outpath, "*.*", SearchOption.AllDirectories);
    }
}

public class ExampleEnumerateFilesResponse : FileOps.FileOperationResponse
{
    public string[] files;
}

public class ExampleReadFilesRequest : FileOps.FileOperationRequest
{
    public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
    {
        ExampleReadFilesResponse fileResponse = response as ExampleReadFilesResponse;

        string outpath = mp.PathName.Data + "/MySaveFile.txt";

        fileResponse.myTestData = File.ReadAllText(outpath);

        string outpath2 = mp.PathName.Data + "/MyOtherSaveFile.txt";

        fileResponse.myOtherTestData = File.ReadAllText(outpath2);

        string outpath3 = mp.PathName.Data + "/Data.dat";

        //fileResponse.largeData = File.ReadAllBytes(outpath3);

        FileInfo info = new FileInfo(outpath3);

        fileResponse.largeData = new byte[info.Length];

        int totalRead = 0;

        // Example of updating the progress value.
        using (FileStream fs = new FileStream(outpath3, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 1024 * 5)) // File.OpenRead(outpath3))
        {
            // Add some information to the file.
            while (totalRead < info.Length)
            {
                int readSize = Math.Min((int)info.Length - totalRead, 1024 * 1024 * 2); // read up to 2 mb in a single write

                fs.Read(fileResponse.largeData, totalRead, readSize);

                totalRead += readSize;

                // Update progress value during saving
                response.UpdateProgress((float)totalRead / (float)info.Length);
            }
        }
    }
}

public class ExampleReadFilesResponse : FileOps.FileOperationResponse
{
    public string myTestData;
    public string myOtherTestData;
    public byte[] largeData;
}
