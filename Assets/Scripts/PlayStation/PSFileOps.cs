#if !UNITY_PS5 && !UNITY_PS4
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class PSNManager : MonoBehaviour
    { }
}
#else
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;


namespace Com.MorganHouston.Imprecision
{
    public class PSWriteFilesRequest : FileOps.FileOperationRequest
    {
        public string userID;
        public string userName;
        public int points = 0;
        public int coins = 0;
        public int currentBlaster = 0;
        public int currentSkin = 0;
        public int highestWave = 0;
        public int highestWaveParty = 0;
        public string playerName = "PlayerName";
        public string RewardOvertime { get; private set; }
        public int[] ownedBlasters = { 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        public int[] ownedSkins = { 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        public int cubesEliminated = 0;
        public int totalPointsEarned = 0;
        public int totalProjectilesFired = 0;

        public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
        {
            //  OnScreenLog.Add("DoFileOperations start");

            PSWriteFilesResponse fileResponse = response as PSWriteFilesResponse;

            /*string outpath = mp.PathName.Data + "/MySaveFile.txt";

            File.WriteAllText(outpath, myTestData);*/

            BinaryFormatter formatter = new BinaryFormatter();
            string path = mp.PathName.Data + "/playerData.hax";
            FileStream stream = new FileStream(path, FileMode.Create);

            Com.MorganHouston.Imprecision.SavePlayerData data = new Com.MorganHouston.Imprecision.SavePlayerData(Player.Instance);

            formatter.Serialize(stream, data);
            stream.Close();

            FileInfo info = new FileInfo(path);
            fileResponse.lastWriteTime = info.LastWriteTime;
            fileResponse.totalFileSizeWritten = info.Length;
            /*string outpath2 = mp.PathName.Data + "/MyOtherSaveFile.txt";

            File.WriteAllText(outpath2, myOtherTestData);

            info = new FileInfo(outpath2);
            fileResponse.totalFileSizeWritten += info.Length;

            string outpath3 = mp.PathName.Data + "/Data.dat";

            int totalWritten = 0;

            // Calculate the size of chunks to write out so the progress bar can be updated while the large data is being written.
            // In this case lets make it write out 5% of the file each loop.
            /*int chunkSize = largeData.Length / 20;

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

            info = new FileInfo(outpath);
            fileResponse.lastWriteTime = info.LastWriteTime;
            fileResponse.totalFileSizeWritten += info.Length;*/
        }
    }

    public class PSWriteFilesResponse : FileOps.FileOperationResponse
    {
        public DateTime lastWriteTime;
        public long totalFileSizeWritten;
    }

    public class PSEnumerateFilesRequest : FileOps.FileOperationRequest
    {
        public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
        {
            PSEnumerateFilesResponse fileResponse = response as PSEnumerateFilesResponse;

            string outpath = mp.PathName.Data;

            fileResponse.files = Directory.GetFiles(outpath, "*.*", SearchOption.AllDirectories);
        }
    }

    public class PSEnumerateFilesResponse : FileOps.FileOperationResponse
    {
        public string[] files;
    }

    public class PSReadFilesRequest : FileOps.FileOperationRequest
    {
        public override void DoFileOperations(Mounting.MountPoint mp, FileOps.FileOperationResponse response)
        {
            PSReadFilesResponse fileResponse = response as PSReadFilesResponse;

            string path = mp.PathName.Data + "/playerData.hax";
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                Com.MorganHouston.Imprecision.SavePlayerData data = formatter.Deserialize(stream) as Com.MorganHouston.Imprecision.SavePlayerData;
                CloudSaveLogin.Instance.LoadPlayerData(data);
                stream.Close();
            }
            else
            {
                //Debug.Log("Save file not found in " + path);
                Com.MorganHouston.Imprecision.SavePlayerData data = new Com.MorganHouston.Imprecision.SavePlayerData();
                CloudSaveLogin.Instance.LoadPlayerData(data);
            }

            /*
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
            }*/
        }
    }

    public class PSReadFilesResponse : FileOps.FileOperationResponse
    {
        public string myTestData;
        public string myOtherTestData;
        public byte[] largeData;
    }
}
#endif