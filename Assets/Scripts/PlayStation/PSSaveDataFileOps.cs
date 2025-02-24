


#if UNITY_PS5

using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;

namespace Com.MorganHouston.Imprecision
{
    public class PSSaveDataFileOps
    {

        public PSSaveDataFileOps()
        {
            Initialize();
        }

        public void Initialize()
        {

        }

        public void MenuFileOps()
        {

            Mounting.MountPoint mp = PSSaveData.GetMountPoint();

            bool isEnabled = (mp != null);

            string moutPointToolTip = "";

            if (mp != null)
            {
                moutPointToolTip = " Use this on mount point \"" + mp.PathName.Data + "\".";
            }
        }


        public void WriteFiles(Mounting.MountPoint mp)
        {
            try
            {
                PSWriteFilesRequest request = new PSWriteFilesRequest();

                request.UserId = PSUser.GetActiveUserId;
                request.MountPointName = mp.PathName;

                /*for (int i = 0; i < 20; i++)
                {
                    request.largeData[i] = (byte)(OnScreenLog.FrameCount + i);
                }*/

                PSWriteFilesResponse response = new PSWriteFilesResponse();

                int requestId = FileOps.CustomFileOp(request, response);

                //OnScreenLog.Add("WriteFiles Async : Request Id = " + requestId);

                /*string dataStr = "";
                for (int i = 0; i < 20; i++)
                {
                    if (i > 0) dataStr += ", ";
                    dataStr += request.largeData[i];
                }
                OnScreenLog.Add("Write Data = " + dataStr);*/
            }
            catch (SaveDataException e)
            {
                //OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
            }
        }

        public void EnumerateFiles(Mounting.MountPoint mp)
        {
            try
            {
                PSEnumerateFilesRequest request = new PSEnumerateFilesRequest();

                request.UserId = PSUser.GetActiveUserId;
                request.MountPointName = mp.PathName;

                PSEnumerateFilesResponse response = new PSEnumerateFilesResponse();

                int requestId = FileOps.CustomFileOp(request, response);

                //OnScreenLog.Add("WriteFiles Async : Request Id = " + requestId);
            }
            catch (SaveDataException e)
            {
                //OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
            }
        }

        public void ReadFiles(Mounting.MountPoint mp)
        {
            try
            {
                PSReadFilesRequest request = new PSReadFilesRequest();

                request.UserId = PSUser.GetActiveUserId;
                request.MountPointName = mp.PathName;

                PSReadFilesResponse response = new PSReadFilesResponse();

                int requestId = FileOps.CustomFileOp(request, response);

                //OnScreenLog.Add("ReadFiles Async : Request Id = " + requestId);
            }
            catch (SaveDataException e)
            {
                //OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
            }
        }

        public void OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
        {
            switch (callbackEvent.ApiCalled)
            {
                case FunctionTypes.FileOps:
                    {
                        if (callbackEvent.Request is PSWriteFilesRequest)
                        {
                            PSWriteReponseOutput(callbackEvent.Response as PSWriteFilesResponse);
                        }
                        else if (callbackEvent.Request is PSEnumerateFilesRequest)
                        {
                            PSEnumerateReponseOutput(callbackEvent.Response as PSEnumerateFilesResponse);
                        }
                        else if (callbackEvent.Request is PSReadFilesRequest)
                        {
                            PSReadReponseOutput(callbackEvent.Response as PSReadFilesResponse);
                        }
                    }
                    break;
            }
        }

        public void PSWriteReponseOutput(PSWriteFilesResponse response)
        {
            if (response != null)
            {
                //OnScreenLog.Add("Last Write Time : " + response.lastWriteTime);
                //OnScreenLog.Add("Total File Size Written : " + response.totalFileSizeWritten);
            }
        }

        public void PSEnumerateReponseOutput(PSEnumerateFilesResponse response)
        {
            if (response != null)
            {
                if (response.files == null || response.files.Length == 0)
                {
                    //OnScreenLog.Add("No files were found");
                }
                else
                {
                    for (int i = 0; i < response.files.Length; i++)
                    {
                        //OnScreenLog.Add("   " + response.files[i]);
                    }
                }
            }
        }

        public void PSReadReponseOutput(PSReadFilesResponse response)
        {
            if (response != null)
            {
                //OnScreenLog.Add("My Test Data : " + response.myTestData);
                //OnScreenLog.Add("My Other Test Data : " + response.myOtherTestData);

                string dataStr = "";
                for (int i = 0; i < 20; i++)
                {
                    if (i > 0) dataStr += ", ";
                    dataStr += response.largeData[i];
                }
                //OnScreenLog.Add("Read Data = " + dataStr);
            }
        }

    }
}
#endif
