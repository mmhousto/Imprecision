

using System;
using System.Collections;
using Unity.SaveData.PS5;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Dialog;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;

namespace SaveData
{
    /// <summary>
    /// Dialog state machine for running the save/load/delete savedata process
    /// </summary>
    public class AutoSaveProcess
    {
        public delegate void ErrorHandler(uint errorCode);

        //  delegate
        /// <summary>
        /// Save process states
        /// </summary>
        private enum SaveState
        {
            Begin,
            SaveFiles,
            WriteIcon,
            WriteParams,
            Unmount,
            HandleError,

            LoadFiles,

            Exit
        }

        /// <summary>
        /// Start the auto-save process as a Unity Coroutine.
        /// </summary>
        public static IEnumerator StartAutoSaveProcess(int userId, Dialogs.NewItem newItem, DirName autoSaveDirName, UInt64 newSaveDataBlocks, SaveDataParams saveDataParams,
                                                          FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse, bool backup, ErrorHandler errHandler)
        {
            SaveState currentState = SaveState.Begin;

            Mounting.MountResponse mountResponse = new Mounting.MountResponse();
            Mounting.MountPoint mp = null;

            int errorCode = 0;

            while (currentState != SaveState.Exit)
            {
                switch (currentState)
                {
                    case SaveState.Begin:
                        {
                            Mounting.MountModeFlags flags = Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;

                            DirName dirName = autoSaveDirName;

                            errorCode = MountSaveData(userId, newSaveDataBlocks, mountResponse, dirName, flags);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                // Wait for save data to be mounted.
                                while (mountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                if (mountResponse.IsErrorCode == true)
                                {
                                    errorCode = mountResponse.ReturnCodeValue;

                                    // Must handle no space and broken save games
                                    //    ReturnCodes.DATA_ERROR_NO_SPACE_FS
                                    //    ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                    currentState = SaveState.HandleError;
                                }
                                else
                                {
                                    // Save data is now mounted, so files can be saved.
                                    mp = mountResponse.MountPoint;
                                    currentState = SaveState.SaveFiles;
                                }
                            }
                        }
                        break;
                    case SaveState.SaveFiles:
                        {
                            // Do actual saving
                            fileRequest.MountPointName = mp.PathName;
                            fileRequest.Async = true;
                            fileRequest.UserId = userId;

                            errorCode = FileOps.CustomFileOp(fileRequest, fileResponse);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (fileResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                // Write the icon and any detail parmas set here.
                                EmptyResponse iconResponse = new EmptyResponse();

                                errorCode = WriteIcon(userId, iconResponse, mp, newItem);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.HandleError;
                                }
                                else
                                {
                                    EmptyResponse paramsResponse = new EmptyResponse();

                                    errorCode = WriteParams(userId, paramsResponse, mp, saveDataParams);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.HandleError;
                                    }
                                    else
                                    {
                                        // Wait for save icon to be mounted.
                                        while (iconResponse.Locked == true || paramsResponse.Locked == true)
                                        {
                                            yield return null;
                                        }

                                        currentState = SaveState.WriteIcon;
                                    }
                                }
                            }
                        }
                        break;
                    case SaveState.WriteIcon:
                        {
                            // Write the icon and any detail parmas set here.
                            EmptyResponse iconResponse = new EmptyResponse();

                            errorCode = WriteIcon(userId, iconResponse, mp, newItem);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (iconResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.WriteParams;
                            }
                        }
                        break;
                    case SaveState.WriteParams:
                        {
                            EmptyResponse paramsResponse = new EmptyResponse();

                            errorCode = WriteParams(userId, paramsResponse, mp, saveDataParams);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                // Wait for save icon to be mounted.
                                while (paramsResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Unmount;
                            }
                        }
                        break;
                    case SaveState.Unmount:
                        {
                            EmptyResponse unmountResponse = new EmptyResponse();

                            errorCode = UnmountSaveData(userId, unmountResponse, mp);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (unmountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Exit;
                            }
                        }
                        break;
                    case SaveState.HandleError:
                        {
                            if (mp != null)
                            {
                                EmptyResponse unmountResponse = new EmptyResponse();

                                UnmountSaveData(userId, unmountResponse, mp);
                            }

                            if(errHandler != null)
                            {
                                errHandler((uint)errorCode);
                            }
                        }
                        currentState = SaveState.Exit;
                        break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Start the auto-save process as a Unity Coroutine.
        /// </summary>
        public static IEnumerator StartAutoSaveLoadProcess(int userId, DirName dirName, FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse, ErrorHandler errHandler)
        {
            SaveState currentState = SaveState.Begin;

            Mounting.MountResponse mountResponse = new Mounting.MountResponse();
            Mounting.MountPoint mp = null;

            int errorCode = 0;

            while (currentState != SaveState.Exit)
            {
                switch (currentState)
                {
                    case SaveState.Begin:
                        {
                            Mounting.MountModeFlags flags = Mounting.MountModeFlags.ReadOnly;

                            errorCode = MountSaveData(userId, 0, mountResponse, dirName, flags);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                // Wait for save data to be mounted.
                                while (mountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                if (mountResponse.IsErrorCode == true)
                                {
                                    errorCode = mountResponse.ReturnCodeValue;
                                    // Must handle broken save games
                                    //    ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                    currentState = SaveState.HandleError;
                                }
                                else
                                {
                                    // Save data is now mounted, so files can be saved.
                                    mp = mountResponse.MountPoint;
                                    currentState = SaveState.LoadFiles;
                                }
                            }
                        }
                       break;
                    case SaveState.LoadFiles:
                        {
                            // Do actual loading
                            fileRequest.MountPointName = mp.PathName;
                            fileRequest.Async = true;
                            fileRequest.UserId = userId;

                            errorCode = FileOps.CustomFileOp(fileRequest, fileResponse);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (fileResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Unmount;
                            }
                        }
                        break;
                    case SaveState.Unmount:
                        {
                            EmptyResponse unmountResponse = new EmptyResponse();

                            errorCode = UnmountSaveData(userId, unmountResponse, mp);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (unmountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Exit;
                            }
                        }
                        break;
                    case SaveState.HandleError:
                        {
                            if (mp != null)
                            {
                                EmptyResponse unmountResponse = new EmptyResponse();

                                UnmountSaveData(userId, unmountResponse, mp);
                            }

                            if (errHandler != null)
                            {
                                errHandler((uint)errorCode);
                            }
                        }
                        currentState = SaveState.Exit;
                        break;
                }

                yield return null;
            }
        }

        internal static int MountSaveData(int userId, UInt64 blocks, Mounting.MountResponse mountResponse, DirName dirName, Mounting.MountModeFlags flags)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.MountRequest request = new Mounting.MountRequest();

                request.UserId = userId;
                request.IgnoreCallback = true;
                request.DirName = dirName;

                request.MountMode = flags;

                if (blocks < Mounting.MountRequest.BLOCKS_MIN)
                {
                    blocks = Mounting.MountRequest.BLOCKS_MIN;
                }

                request.Blocks = blocks;

//              request.SystemBlocks = 0;     // setting to zero specifies savedata that does no support rollback. https://game.develop.playstation.net/resources/documents/sdk/latest/SaveData-Reference/0011.html

                Mounting.Mount(request, mountResponse);
                errorCode = 0;
            }
            catch
            {
                if (mountResponse.ReturnCodeValue < 0)
                {
                    errorCode = mountResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

        internal static int UnmountSaveData(int userId, EmptyResponse unmountResponse, Mounting.MountPoint mp)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.UnmountRequest request = new Mounting.UnmountRequest();

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.IgnoreCallback = true;

                Mounting.Unmount(request, unmountResponse);

                errorCode = 0;
            }
            catch
            {
                if (unmountResponse.ReturnCodeValue < 0)
                {
                    errorCode = unmountResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

        internal static int WriteIcon(int userId, EmptyResponse iconResponse, Mounting.MountPoint mp, Dialogs.NewItem newItem)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.SaveIconRequest request = new Mounting.SaveIconRequest();

                if (mp == null) return errorCode;

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.RawPNG = newItem.RawPNG;
                request.IconPath = newItem.IconPath;
                request.IgnoreCallback = true;

                Mounting.SaveIcon(request, iconResponse);

                errorCode = 0;
            }
            catch
            {
                if (iconResponse.ReturnCodeValue < 0)
                {
                    errorCode = iconResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

        internal static int WriteParams(int userId, EmptyResponse paramsResponse, Mounting.MountPoint mp, SaveDataParams saveDataParams)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Mounting.SetMountParamsRequest request = new Mounting.SetMountParamsRequest();

                if (mp == null) return errorCode;

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.IgnoreCallback = true;

                request.Params = saveDataParams;

                Mounting.SetMountParams(request, paramsResponse);

                errorCode = 0;
            }
            catch
            {
                if (paramsResponse.ReturnCodeValue < 0)
                {
                    errorCode = paramsResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

    }
}
