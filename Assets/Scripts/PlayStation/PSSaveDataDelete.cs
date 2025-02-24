
#if UNITY_PS5

using System.IO;
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Delete;

namespace Com.MorganHouston.Imprecision
{
    public class PSSaveDataDelete
    {
        public PSSaveDataDelete()
        {
            Initialize();
        }

        public void Initialize()
        {

        }

        public void MenuUserProfiles()
        {


            bool isEnabled = SaveDataDirNames.HasCurrentDirName();

            string dirName = SaveDataDirNames.GetCurrentDirName();

            string dirNameToolTip = "";

            if (isEnabled == true)
            {
                dirNameToolTip = " Use this on directory name \"" + dirName + "\".";
            }
        }

        public void Delete()
        {
            try
            {
                Deleting.DeleteRequest request = new Deleting.DeleteRequest();

                DirName dirName = new DirName();
                dirName.Data = SaveDataDirNames.GetCurrentDirName();

                request.UserId = PSUser.GetActiveUserId;
                request.DirName = dirName;

                EmptyResponse response = new EmptyResponse();

                int requestId = Deleting.Delete(request, response);

                //OnScreenLog.Add("Delete Async : Request Id = " + requestId);
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
                case FunctionTypes.Delete:
                    {
                        EmptyResponse response = callbackEvent.Response as EmptyResponse;

                        if (response != null && response.ReturnCode == ReturnCodes.SUCCESS)
                        {
                            Deleting.DeleteRequest request = callbackEvent.Request as Deleting.DeleteRequest;

                            if (request != null)
                            {
                                SaveDataDirNames.RemoveDirName(request.DirName.Data);
                            }
                        }
                    }
                    break;
            }
        }

    }
}
#endif
