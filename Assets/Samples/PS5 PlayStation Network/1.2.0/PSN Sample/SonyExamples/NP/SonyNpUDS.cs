using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
#if UNITY_PS5
using Unity.PSN.PS5.UDS;
#endif
#endif

namespace PSNSample
{
#if UNITY_PS5
    public class FrameCounterEvent
    {
        UniversalDataSystem.UDSEvent udsEvent;
        UniversalDataSystem.EventProperty usernameProp;
        UniversalDataSystem.EventProperty framenumProp;
        UniversalDataSystem.PostEventRequest request;

        AsyncAction<AsyncRequest<UniversalDataSystem.PostEventRequest>> requestOp;

        public bool InUse { get; internal set; }

        public FrameCounterEvent()
        {
            InUse = false;

            udsEvent = new UniversalDataSystem.UDSEvent();
            udsEvent.Create("FrameCounter");

            usernameProp = udsEvent.Properties.Set("username", UniversalDataSystem.PropertyType.String);
            framenumProp = udsEvent.Properties.Set("framenum", UniversalDataSystem.PropertyType.UInt32);

            request = new UniversalDataSystem.PostEventRequest();
            request.UserId = 0;
            request.EventData = udsEvent;

            requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
            {
                try
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Frame Event sent");
                    }
                    else
                    {
                        OnScreenLog.AddError("Event send error");
                        InUse = false;
                    }
                }
                catch (Exception e)
                {
                    InUse = false;
                    throw e;  // rethrow the exception
                }

                InUse = false;
            });
        }

        public void UpdateValues(string username, UInt32 framenum)
        {
            usernameProp.Update(username);
            framenumProp.Update(framenum);
        }

        public void Post(int userId)
        {
            if (InUse == true)
            {
                return;
            }

            requestOp.Reset(); // Reset the async op

            request.UserId = userId;

            UniversalDataSystem.Schedule(requestOp);
        }
    }

    public class SonyNpUDS : IScreen
    {
        MenuLayout m_MenuUDS;

        public SonyNpUDS()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuUDS;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuUDS(stack);
        }

        public void Initialize()
        {
            m_MenuUDS = new MenuLayout(this, 450, 20);
        }

        FrameCounterEvent reusableCounterEvent = new FrameCounterEvent();

        public void MenuUDS(MenuStack menuStack)
        {
            m_MenuUDS.Update();

            bool enabled = UniversalDataSystem.IsInitialized;

            if (m_MenuUDS.AddItem("Start UDS", "Start the Universal Data System", !enabled))
            {
                UniversalDataSystem.StartSystemRequest request = new UniversalDataSystem.StartSystemRequest();

                request.PoolSize = 256 * 1024;

                var requestOp = new AsyncRequest<UniversalDataSystem.StartSystemRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("System Started");
                    }
                });

                UniversalDataSystem.Schedule(requestOp);
            }

            if (m_MenuUDS.AddItem("Stop UDS", "Stop the Universal Data System", enabled))
            {
                UniversalDataSystem.StopSystemRequest request = new UniversalDataSystem.StopSystemRequest();

                var requestOp = new AsyncRequest<UniversalDataSystem.StopSystemRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("System Stopped");
                    }
                });

                UniversalDataSystem.Schedule(requestOp);
            }

            if (m_MenuUDS.AddItem("Post Event (Unlock Trophy)", "Post a UDS Event. This manually unlocks a trophy using an event built in C#.", enabled))
            {
                UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

                myEvent.Create("_UnlockTrophy");

                UniversalDataSystem.EventProperty prop = new UniversalDataSystem.EventProperty("_trophy_id", (Int32)1);

                myEvent.Properties.Set(prop);

                UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();

                request.UserId = GamePad.activeGamePad.loggedInUser.userId;
                request.EventData = myEvent;

                var getTrophyOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Trophy unlocked");
                    }
                });

                UniversalDataSystem.Schedule(getTrophyOp);
            }

            if (m_MenuUDS.AddItem("Post Complex Event", "Post a custom UDS Event what contains properties, arrays, arrays of arrays and arrays of properties. ", enabled))
            {
                UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

                myEvent.Create("Test");

                myEvent.Properties.Set("name", "a test name");
                myEvent.Properties.Set("someid", (Int64)10);
                myEvent.Properties.Set("score", 1.234f);
                myEvent.Properties.Set("double", (double)1.23456789012345);
                myEvent.Properties.Set("bool", true);

                byte[] someData = new byte[100];

                for (int i = 0; i < someData.Length; i++)
                {
                    someData[i] = (byte)i;
                }

                myEvent.Properties.Set("binary", someData);

                UniversalDataSystem.EventProperties subProperties = new UniversalDataSystem.EventProperties();
                myEvent.Properties.Set("nested", subProperties);

                subProperties.Set("moredata", 5.678f);
                subProperties.Set("anotherstring", "subprops");

                UniversalDataSystem.EventPropertyArray nestedArray = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Int32);

                subProperties.Set("nestedArray", nestedArray);
                nestedArray.CopyValues(new Int32[] { 10, 11, 12, 13, 14 });

                UniversalDataSystem.EventPropertyArray arrayProps = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.String);
                myEvent.Properties.Set("ArrayOfStuff", arrayProps);

                string[] testArray = new string[] { "One", "Two", "Three", "Four", "Five" };
                arrayProps.CopyValues(testArray);

                // Create an array of arrays
                UniversalDataSystem.EventPropertyArray arrayOfArraysProps = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Array);
                myEvent.Properties.Set("ArrayOfArrays", arrayOfArraysProps);

                // Create two array of different types to add
                UniversalDataSystem.EventPropertyArray array1 = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Int32);
                UniversalDataSystem.EventPropertyArray array2 = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Float);

                arrayOfArraysProps.Set(array1);
                arrayOfArraysProps.Set(array2);

                Int32[] testArray1 = new Int32[] { 1, 2, 3, 4, 5 };
                array1.CopyValues(testArray1);

                float[] testArray2 = new float[] { 1.12f, 2.23f, 3.45f, 4.56f, 5.67f };
                array2.CopyValues(testArray2);

                UniversalDataSystem.EventPropertyArray array3 = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Binary);

                arrayOfArraysProps.Set(array3);

                byte[] moreBinaryDataA = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
                byte[] moreBinaryDataB = new byte[] { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119 };
                byte[] moreBinaryDataC = new byte[] { 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219 };

                List<byte[]> testBinaryArray3 = new List<byte[]>();
                testBinaryArray3.Add(moreBinaryDataA);
                testBinaryArray3.Add(moreBinaryDataB);
                testBinaryArray3.Add(moreBinaryDataC);

                array3.CopyValues(testBinaryArray3.ToArray());

                // Create an array of properties
                UniversalDataSystem.EventPropertyArray arrayOfProperties = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Properties);
                myEvent.Properties.Set("ArrayOfProperties", arrayOfProperties);

                UniversalDataSystem.EventProperties subProperties1 = new UniversalDataSystem.EventProperties();
                UniversalDataSystem.EventProperties subProperties2 = new UniversalDataSystem.EventProperties();

                arrayOfProperties.Set(subProperties1);
                arrayOfProperties.Set(subProperties2);

                subProperties1.Set("moredata", 100.1234f);
                subProperties1.Set("anotherstring", "inside array 1");

                subProperties2.Set("moredata", 200.1234f);
                subProperties2.Set("anotherstring", "inside array 2");

                PostEvent(myEvent);
            }

            if (m_MenuUDS.AddItem("Post Binary Event", "Post a custom UDS Event what contains binary data", enabled))
            {
                UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

                myEvent.Create("Test");

                myEvent.Properties.Set("name", "a test name");
                myEvent.Properties.Set("someid", (Int64)10);
                myEvent.Properties.Set("score", 1.234f);
                myEvent.Properties.Set("double", (double)1.23456789012345);
                myEvent.Properties.Set("bool", true);

                byte[] someData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
                myEvent.Properties.Set("binary", someData);

                UniversalDataSystem.EventProperties subProperties = new UniversalDataSystem.EventProperties();
                myEvent.Properties.Set("nested", subProperties);

                subProperties.Set("morebinary", someData);

                PostEvent(myEvent);
            }

            if (m_MenuUDS.AddItem("Post Resuable Event", "Post a resuable UDS Event. The event is pre-allocated and updated before being sent.", enabled && reusableCounterEvent.InUse == false))
            {
                reusableCounterEvent.UpdateValues(GamePad.activeGamePad.loggedInUser.userName, (uint)OnScreenLog.FrameCount);

                reusableCounterEvent.Post(GamePad.activeGamePad.loggedInUser.userId);
            }

            if (m_MenuUDS.AddItem("Get Memory Stats", "Get the memory stats for UDS", enabled))
            {
                UniversalDataSystem.GetMemoryStatsRequest request = new UniversalDataSystem.GetMemoryStatsRequest();

                var requestOp = new AsyncRequest<UniversalDataSystem.GetMemoryStatsRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Get Memory Stats");

                        OnScreenLog.Add("  PoolSize         : " + antecedent.Request.PoolSize);
                        OnScreenLog.Add("  MaxInuseSize     : " + antecedent.Request.MaxInuseSize);
                        OnScreenLog.Add("  CurrentInuseSize : " + antecedent.Request.CurrentInuseSize);
                    }

                });

                UniversalDataSystem.Schedule(requestOp);
            }

            if (m_MenuUDS.AddItem("Stress Test UDS", "Stress test the UDS by flooding it will an excessive number of events. This test will deliberately throw an exception.", enabled))
            {
                for (int i = 0; i < 1000; i++)
                {
                    UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

                    myEvent.Create("UpdateCustomProgress");

                    myEvent.Properties.Set("MyProgress ", (Int32)10);

                    UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();

                    request.UserId = GamePad.activeGamePad.loggedInUser.userId;
                    request.EventData = myEvent;

                    var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
                    {
                        if (SonyNpMain.CheckAysncRequestOK(antecedent) == false)
                        {
                            OnScreenLog.AddError("Event send error: " + antecedent.Request.Result.ErrorMessage());
                        }
                    });

                    try
                    {
                        UniversalDataSystem.Schedule(requestOp);
                    }
                    catch(ExceededMaximumOperations e)
                    {
                        OnScreenLog.AddWarning("Schedule Exception: " + e.Message);
                        OnScreenLog.AddWarning("Schedule up to " + i + " operations" );
                        OnScreenLog.AddWarning("This exception is expected as this tests what happens if the too many operations are scheduled");

                        break;
                    }
                }
            }

            if (m_MenuUDS.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        private void PostEvent(UniversalDataSystem.UDSEvent udsEvent)
        {
            UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();

            request.UserId = GamePad.activeGamePad.loggedInUser.userId;
            request.CalculateEstimatedSize = true;
            request.EventData = udsEvent;

            var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Event sent - Estimated size = " + antecedent.Request.EstimatedSize);
                }
                else
                {
                    OnScreenLog.AddError("Event send error");
                }
            });

            UniversalDataSystem.Schedule(requestOp);

            UniversalDataSystem.EventDebugStringRequest stringRequest = new UniversalDataSystem.EventDebugStringRequest();

            stringRequest.UserId = GamePad.activeGamePad.loggedInUser.userId;
            stringRequest.EventData = udsEvent;

            var secondRequestOp = new AsyncRequest<UniversalDataSystem.EventDebugStringRequest>(stringRequest).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add(antecedent.Request.Output, true);
                }
                else
                {
                    OnScreenLog.AddError("Event string error");
                }
            });

            UniversalDataSystem.Schedule(secondRequestOp);
        }

        private void OutputEvent(UniversalDataSystem.UDSEvent udsEvent)
        {
            OnScreenLog.Add("UDSEvent");

            OnScreenLog.Add("   Name : " + udsEvent.Name);

            OnScreenLog.AddNewLine();
        }
    }
#endif
}
