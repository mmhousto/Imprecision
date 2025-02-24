using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;

using Unity.PSN.PS5.TCS;



namespace PSNSample
{
    public class SonyTitleCloudStorage : IScreen
    {
        MenuLayout m_MenuTUS;

        public SonyTitleCloudStorage()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuTUS;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuLeaderboards(stack);
        }

        public void Initialize()
        {
            m_MenuTUS = new MenuLayout(this, 450, 20);
        }

        public byte[] MakeData(int size, int startNumber)
        {
            byte[] someData = new byte[size];

            for (int i = 0; i < someData.Length; i++)
            {
                someData[i] = (byte)(startNumber + i);
            }

            return someData;
        }

        string OutputBinaryData(byte[] data, int maxLength)
        {
            string output = "";

            int max = Math.Min(data.Length, maxLength);

            for (int i = 0; i < max; i++)
            {
                output += data[i] + " ";
            }

            return output;
        }

        string dsObjectIdMeSlot0 = null;
        string dsObjectIdMeSlot1 = null;
        string dsObjectIdAnyoneSlot0 = null;

        public void MenuLeaderboards(MenuStack menuStack)
        {
            m_MenuTUS.Update();

            bool enabled = true;

            if (m_MenuTUS.AddItem("Add and Get Variable", "Add and get the value of a variable", enabled))
            {
                TitleCloudStorage.AddAndGetVariableRequest request = new TitleCloudStorage.AddAndGetVariableRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    AccountId = GamePad.activeGamePad.loggedInUser.accountId,
                    SlotId = 1,
                    Value = 10,
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.AddAndGetVariableRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputVariable(antecedent.Request.Variable);
                    }
                });

                OnScreenLog.Add("Adding " + request.Value + " to slot " + request.SlotId);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Set with Condition", "Set a variable only when it passes a condition. If the value in Slot 1 is less than 100 sets the value.", enabled))
            {
                TitleCloudStorage.SetVariableWithConditionRequest request = new TitleCloudStorage.SetVariableWithConditionRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotId = 1,
                    Value = 100,
                    Condition = TitleCloudStorage.SetVariableWithConditionRequest.Conditions.Greater
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.SetVariableWithConditionRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputVariable(antecedent.Request.Variable);
                    }
                });

                OnScreenLog.Add("Setting " + request.Value + " to slot " + request.SlotId + " is the value is " + request.Condition);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Get multiple vars by slot", "Get multi variables by slot.", enabled))
            {
                TitleCloudStorage.GetMultiVariablesBySlotRequest request = new TitleCloudStorage.GetMultiVariablesBySlotRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    Anyone = true,
                    SlotId = 1,
                    SortCondition = TitleCloudStorage.SortVariableConditions.SlotId,
                    SortMode = TitleCloudStorage.SortModes.Ascending
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiVariablesBySlotRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputVariables(antecedent.Request.Variables);
                    }
                });

                OnScreenLog.Add("Getting slot " + request.SlotId);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Set multiple vars by user", "Set multi variables by user. Updates Slots 1, 2 and 3.", enabled))
            {
                List<TitleCloudStorage.VariableToWrite> variables = new List<TitleCloudStorage.VariableToWrite>();

                variables.Add(new TitleCloudStorage.VariableToWrite() { SlotId = 1, Value = 10});
                variables.Add(new TitleCloudStorage.VariableToWrite() { SlotId = 2, Value = 20 });
                variables.Add(new TitleCloudStorage.VariableToWrite() { SlotId = 3, Value = 30 });

                TitleCloudStorage.SetMultiVariablesByUserRequest request = new TitleCloudStorage.SetMultiVariablesByUserRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    Variables = variables
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.SetMultiVariablesByUserRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Multiple variables set");
                    }
                });

                OnScreenLog.Add("Setting multiple variables");

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Get multiple vars by user", "Get multi variables by user. Get slots 1, 2 and 3.", enabled))
            {
                List<int> slotids = new List<int>();

                slotids.Add(1);
                slotids.Add(2);
                slotids.Add(3);

                TitleCloudStorage.GetMultiVariablesByUserRequest request = new TitleCloudStorage.GetMultiVariablesByUserRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotIds = slotids,
                    SortCondition = TitleCloudStorage.SortVariableConditions.Date,
                    SortMode = TitleCloudStorage.SortModes.Descending
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiVariablesByUserRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputVariables(antecedent.Request.Variables);
                    }
                });

                OnScreenLog.Add("Getting multiple variables");

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Delete multiple vars by user", "Delete multiple variables by user. Deletes slots 1, 2 and 3.", enabled))
            {
                List<int> slotids = new List<int>();

                slotids.Add(1);
                slotids.Add(2);
                slotids.Add(3);

                TitleCloudStorage.DeleteMultiVariablesByUserRequest request = new TitleCloudStorage.DeleteMultiVariablesByUserRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotIds = slotids
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.DeleteMultiVariablesByUserRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Multiple variables deleted");
                    }
                });

                OnScreenLog.Add("Deleting multiple variables");

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Upload Data (0)", "Updload data to user slot 0.", enabled))
            {

                byte[] data = MakeData(1024*1024*4, OnScreenLog.FrameCount % 256); // 4mb data
                byte[] info = MakeData(256, (OnScreenLog.FrameCount+1) % 256);

                TitleCloudStorage.UploadDataRequest request = new TitleCloudStorage.UploadDataRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotId = 0,
                    Data = data,
                    Info = info
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.UploadDataRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Data sent");
                    }
                });

                OnScreenLog.Add("Uploading data : Me slot 0");

                string dataStr = OutputBinaryData(data, 20);
                string infoStr = OutputBinaryData(info, 20);

                OnScreenLog.Add("     Data : " + dataStr);
                OnScreenLog.Add("     Info : " + infoStr);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Upload Data (1)", "Updload data to user slot 1.", enabled))
            {

                byte[] data = MakeData(1024 * 1024 * 1, OnScreenLog.FrameCount % 256); // 1mb data
                byte[] info = MakeData(256, (OnScreenLog.FrameCount + 1) % 256);

                TitleCloudStorage.UploadDataRequest request = new TitleCloudStorage.UploadDataRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotId = 1,
                    Data = data,
                    Info = info
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.UploadDataRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Data sent");
                    }
                });

                OnScreenLog.Add("Uploading data : Me slot 1");

                string dataStr = OutputBinaryData(data, 20);
                string infoStr = OutputBinaryData(info, 20);

                OnScreenLog.Add("     Data : " + dataStr);
                OnScreenLog.Add("     Info : " + infoStr);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Upload Data (anyone)", "Updload data to 'anyone' slot 0.", enabled))
            {
                byte[] data = MakeData(1024 * 1024 * 4, OnScreenLog.FrameCount % 256); // 4mb data
                byte[] info = MakeData(256, (OnScreenLog.FrameCount + 1) % 256);

                TitleCloudStorage.UploadDataRequest request = new TitleCloudStorage.UploadDataRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Anyone = true,
                    SlotId = 0,
                    Data = data,
                    Info = info
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.UploadDataRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Data sent");
                    }
                });

                OnScreenLog.Add("Uploading data : Me slot 0");

                string dataStr = OutputBinaryData(data, 20);
                string infoStr = OutputBinaryData(info, 20);

                OnScreenLog.Add("     Data : " + dataStr);
                OnScreenLog.Add("     Info : " + infoStr);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Get Data Status (0)", "Get data status by slot 0.", enabled))
            {
                TitleCloudStorage.GetMultiDataStatusesBySlotRequest request = new TitleCloudStorage.GetMultiDataStatusesBySlotRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotId = 0,
                    SortCondition = TitleCloudStorage.SortDataConditions.DataSize,
                    SortMode = TitleCloudStorage.SortModes.Ascending
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiDataStatusesBySlotRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputDataStatuses(antecedent.Request.Statuses);

                        dsObjectIdMeSlot0 = GetFirstObjectID(antecedent.Request.Statuses, 0);
                    }
                });

                OnScreenLog.Add("Geting data status..");

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Get Data Status (0,1)", "Get data status of slot 0 and 1.", enabled))
            {
                List<int> slotids = new List<int>();

                slotids.Add(0);
                slotids.Add(1);

                TitleCloudStorage.GetMultiDataStatusesByUserRequest request = new TitleCloudStorage.GetMultiDataStatusesByUserRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotIds = slotids,
                    SortCondition = TitleCloudStorage.SortDataConditions.DataSize,
                    SortMode = TitleCloudStorage.SortModes.Ascending
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiDataStatusesByUserRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputDataStatuses(antecedent.Request.Statuses);

                        dsObjectIdMeSlot0 = GetFirstObjectID(antecedent.Request.Statuses, 0);
                        dsObjectIdMeSlot1 = GetFirstObjectID(antecedent.Request.Statuses, 1);
                    }
                });

                OnScreenLog.Add("Geting data status..");

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Get Data Status (Anyone)", "Get data status of 'anyone' slot 0 ", enabled))
            {
                TitleCloudStorage.GetMultiDataStatusesBySlotRequest request = new TitleCloudStorage.GetMultiDataStatusesBySlotRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Anyone = true,
                    SlotId = 0,
                    SortCondition = TitleCloudStorage.SortDataConditions.DataSize,
                    SortMode = TitleCloudStorage.SortModes.Ascending
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiDataStatusesBySlotRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputDataStatuses(antecedent.Request.Statuses);

                        dsObjectIdAnyoneSlot0 = GetFirstObjectID(antecedent.Request.Statuses, 0);
                    }
                });

                OnScreenLog.Add("Geting data status..");

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Download Data Me (0)", "Download data from 'me' slot 0.", enabled && dsObjectIdMeSlot0 != null && dsObjectIdMeSlot0.Length > 0))
            {
                TitleCloudStorage.DownloadDataRequest request = new TitleCloudStorage.DownloadDataRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotId = 0,
                    ObjectId = dsObjectIdMeSlot0
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.DownloadDataRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputDownloadedData(antecedent.Request);
                    }
                });

                OnScreenLog.Add("Downloading data using ObjectId : " + dsObjectIdMeSlot0);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Download Data Me (1)", "Download data from me slot 1 with undersized buffer.", enabled && dsObjectIdMeSlot1 != null && dsObjectIdMeSlot1.Length > 0))
            {
                TitleCloudStorage.DownloadDataRequest request = new TitleCloudStorage.DownloadDataRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotId = 1,
                    ObjectId = dsObjectIdMeSlot1,
                    DownloadData = new byte[1000],
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.DownloadDataRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputDownloadedData(antecedent.Request);
                    }
                });

                OnScreenLog.Add("Downloading data using ObjectId : " + dsObjectIdMeSlot1);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Download Data Anyone (0)", "Download data from anyone slot 0.", enabled && dsObjectIdAnyoneSlot0 != null && dsObjectIdAnyoneSlot0.Length > 0))
            {
                TitleCloudStorage.DownloadDataRequest request = new TitleCloudStorage.DownloadDataRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Anyone = true,
                    SlotId = 0,
                    ObjectId = dsObjectIdAnyoneSlot0,
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.DownloadDataRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OutputDownloadedData(antecedent.Request);
                    }
                });

                OnScreenLog.Add("Downloading data using ObjectId : " + dsObjectIdAnyoneSlot0);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Delete Data Me (0)", "Delete Data in me slot 0.", enabled && dsObjectIdMeSlot0 != null && dsObjectIdMeSlot0.Length > 0))
            {
                TitleCloudStorage.DeleteMultiDataBySlotRequest request = new TitleCloudStorage.DeleteMultiDataBySlotRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotId = 0,
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.DeleteMultiDataBySlotRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Deleted slot 0");
                        dsObjectIdMeSlot0 = null;
                    }
                });

                OnScreenLog.Add("Deleting data with ObjectId : " + dsObjectIdMeSlot0);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Delete Data Me (0,1)", "Delete Data in slot 0 and 1.", enabled && dsObjectIdMeSlot0 != null && dsObjectIdMeSlot0.Length > 0 &&
                                                                                                          dsObjectIdMeSlot1 != null && dsObjectIdMeSlot1.Length > 0))
            {
                List<int> slotids = new List<int>();

                slotids.Add(0);
                slotids.Add(1);

                TitleCloudStorage.DeleteMultiDataByUserRequest request = new TitleCloudStorage.DeleteMultiDataByUserRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Me = true,
                    SlotIds = slotids
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.DeleteMultiDataByUserRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Deleted slot 0 and 1");
                        dsObjectIdMeSlot0 = null;
                        dsObjectIdMeSlot1 = null;
                    }
                });

                OnScreenLog.Add("Deleting data with  ObjectId : " + dsObjectIdMeSlot0 + " and " + dsObjectIdMeSlot1);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddItem("Delete Data Anyone (0)", "Delete Data in anyone slot 0.", enabled && dsObjectIdAnyoneSlot0 != null && dsObjectIdAnyoneSlot0.Length > 0))
            {
                TitleCloudStorage.DeleteMultiDataBySlotRequest request = new TitleCloudStorage.DeleteMultiDataBySlotRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Anyone = true,
                    SlotId = 0,
                };

                var requestOp = new AsyncRequest<TitleCloudStorage.DeleteMultiDataBySlotRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Deleted anyone slot 0");
                        dsObjectIdAnyoneSlot0 = null;
                    }
                });

                OnScreenLog.Add("Deleting data with ObjectId : " + dsObjectIdMeSlot0);

                TitleCloudStorage.Schedule(requestOp);
            }

            if (m_MenuTUS.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        string GetFirstObjectID(TitleCloudStorage.DataStatusCollection statuses, int slotId)
        {
            if (statuses == null) return null;

            int count = statuses.Statuses != null ? statuses.Statuses.Count : 0;

            if (count == 0) return null;

            for (int i = 0; i < count; i++)
            {
                if ( (statuses.Statuses[i].ValidFields & TitleCloudStorage.DataStatus.Filters.ObjectId) != 0 &&
                     (statuses.Statuses[i].ValidFields & TitleCloudStorage.DataStatus.Filters.SlotId) != 0 &&
                     statuses.Statuses[i].SlotId == slotId)
                {
                    return statuses.Statuses[i].ObjectId;
                }
            }

            return null;
        }

        private void OutputDownloadedData(TitleCloudStorage.DownloadDataRequest request)
        {
            OnScreenLog.Add("Data downloaded :");

            OnScreenLog.Add("     DownloadDataSize  : " + request.DownloadDataSize);
            OnScreenLog.Add("     TotalBytesRead  : " + request.TotalBytesRead);

            OnScreenLog.Add("     DownloadData  : " + OutputBinaryData(request.DownloadData, 20));
        }

        private void  OutputDataStatuses(TitleCloudStorage.DataStatusCollection statuses)
        {
            OnScreenLog.Add("Statuses : ");

            if (statuses == null)
            {
                OnScreenLog.Add("    No data status");
            }
            else
            {
                OnScreenLog.Add("      Limit : " + statuses.Limit);
                OnScreenLog.Add("      Offset : " + statuses.Offset);
                OnScreenLog.Add("      TotalDataStatusCount : " + statuses.TotalDataStatusCount);

                int count = statuses.Statuses != null ? statuses.Statuses.Count : 0;

                OnScreenLog.Add("      Num statuses : " + count);

                for (int i = 0; i < count; i++)
                {
                    OutputDataStatuses(statuses.Statuses[i]);
                }
            }
        }

        private void OutputDataStatuses(TitleCloudStorage.DataStatus status)
        {
            OnScreenLog.Add("Status : ");

            if (status == null)
            {
                OnScreenLog.Add("    No status");
            }
            else
            {
                OnScreenLog.Add("      ValidFields : " + status.ValidFields);
                OnScreenLog.Add("      DataSize : " + status.DataSize);

                string infoStr = "None";
                if(status.Info != null && status.Info.Length > 0)
                {
                    infoStr = OutputBinaryData(status.Info, 20);
                }

                OnScreenLog.Add("      Info : " + infoStr);
                OnScreenLog.Add("      SlotId : " + status.SlotId);
                OnScreenLog.Add("      ObjectId : " + status.ObjectId);
                OnScreenLog.Add("      LastUpdatedDateTime : " + status.LastUpdatedDateTime);

                if (status.Owner != null)
                {
                    OnScreenLog.Add("      Owner : ");
                    OnScreenLog.Add("         OnlineId : " + status.Owner.OnlineId);
                    OnScreenLog.Add("         AccountId : " + status.Owner.AccountId);
                }

                if (status.LastUpdatedUser != null)
                {
                    OnScreenLog.Add("      LastUpdatedUser : ");
                    OnScreenLog.Add("         OnlineId : " + status.LastUpdatedUser.OnlineId);
                    OnScreenLog.Add("         AccountId : " + status.LastUpdatedUser.AccountId);
                }
            }
        }

        private void OutputVariables(TitleCloudStorage.VariableCollection variables)
        {
            OnScreenLog.Add("Variables : ");

            if (variables == null)
            {
                OnScreenLog.Add("    No variables");
            }
            else
            {
                OnScreenLog.Add("      Limit : " + variables.Limit);
                OnScreenLog.Add("      Offset : " + variables.Offset);
                OnScreenLog.Add("      TotalVariableCount : " + variables.TotalVariableCount);

                int count = variables.Variables != null ? variables.Variables.Count : 0;

                OnScreenLog.Add("      Num vars : " + count);

                for (int i = 0; i < count; i++)
                {
                    OutputVariable(variables.Variables[i]);
                }
            }
        }

        private void OutputVariable(TitleCloudStorage.Variable variable)
        {
            OnScreenLog.Add("Variable : ");

            if (variable == null)
            {
                OnScreenLog.Add("    No variable");
            }
            else
            {
                OnScreenLog.Add("      ValidProperties : " + variable.ValidProperties);
                OnScreenLog.Add("      Value : " + variable.Value);
                OnScreenLog.Add("      PreviousValue : " + variable.PreviousValue);
                OnScreenLog.Add("      SlotId : " + variable.SlotId);
                OnScreenLog.Add("      LastUpdatedDateTime : " + variable.LastUpdatedDateTime);

                if (variable.Owner != null)
                {
                    OnScreenLog.Add("      Owner : ");
                    OnScreenLog.Add("         OnlineId : " + variable.Owner.OnlineId);
                    OnScreenLog.Add("         AccountId : " + variable.Owner.AccountId);
                }

                if (variable.LastUpdatedUser != null)
                {
                    OnScreenLog.Add("      LastUpdatedUser : ");
                    OnScreenLog.Add("         OnlineId : " + variable.LastUpdatedUser.OnlineId);
                    OnScreenLog.Add("         AccountId : " + variable.LastUpdatedUser.AccountId);
                }
            }
        }
    }

}
#endif
