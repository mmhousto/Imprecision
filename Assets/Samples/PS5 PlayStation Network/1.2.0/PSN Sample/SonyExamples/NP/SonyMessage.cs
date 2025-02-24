using System;
using System.Collections.Generic;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Dialogs;

#if UNITY_PS5
using Unity.PSN.PS5.Commerce;
using UnityEngine;
#endif



namespace PSNSample
{

    public class SonyMessages : IScreen
    {
        MenuLayout m_Menumessages;

        public SonyMessages()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_Menumessages;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        MessageDialogSystem.OpenMsgDialogRequest runningProgressBar = null;
        Int32 progressCounter = 0;

        public void Update()
        {
            if(runningProgressBar != null)
            {
                if (runningProgressBar.Status == DialogSystem.DialogStatus.Running)
                {
                    progressCounter++;

                    if (progressCounter >= 5)
                    {
                        progressCounter = 0;

                        runningProgressBar.IncProgressBar(1);

                        if (runningProgressBar.ProgressValue == 40)
                        {
                            runningProgressBar.SetProgressBarMessage("After 40");
                        }

                        if (runningProgressBar.ProgressValue == 60)
                        {
                            runningProgressBar.SetProgressBarMessage("After 60");
                        }

                        if (runningProgressBar.ProgressValue >= 100)
                        {
                            runningProgressBar.CloseDialog();
                        }

                        OnScreenLog.Add("Progress " + runningProgressBar.ProgressValue);
                    }
                }
            }
        }

        public void Process(MenuStack stack)
        {
            MenuMessages(stack);
        }

        public void Initialize()
        {
            m_Menumessages = new MenuLayout(this, 450, 20);
        }

        public void MenuMessages(MenuStack menuStack)
        {
            m_Menumessages.Update();

            bool userEnabled = false;

            if (GamePad.activeGamePad != null) //&& GamePad.activeGamePad.loggedInUser.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
            {
                userEnabled = true;
            }

            if (m_Menumessages.AddItem("Open User Message", "Open the user message dialog", userEnabled))
            {
                MessageDialogSystem.UserMsgParams msgParams = new MessageDialogSystem.UserMsgParams()
                {
                    BtnType = MessageDialogSystem.UserMsgParams.ButtonTypes.OkCancel,
                    Msg = "This is a test user message with OK and Cancel buttons"
                };

                MessageDialogSystem.OpenMsgDialogRequest request = new MessageDialogSystem.OpenMsgDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Mode = MessageDialogSystem.OpenMsgDialogRequest.MsgModes.UserMsg,
                    UserMsg = msgParams
                };

                var requestOp = new AsyncRequest<MessageDialogSystem.OpenMsgDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);
                        OnScreenLog.Add("Button Pressed : " + antecedent.Request.SelectedButton);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_Menumessages.AddItem("Open User Message (Custom)", "Open the user message dialog with custom buttons", userEnabled))
            {
                MessageDialogSystem.UserMsgParams msgParams = new MessageDialogSystem.UserMsgParams()
                {
                    BtnType = MessageDialogSystem.UserMsgParams.ButtonTypes.CustomButtons,
                    Msg = "This is a test user message with OK and Cancel buttons",
                    CustomBtn1 = "Test",
                    CustomBtn2 = "Bail"
                };

                MessageDialogSystem.OpenMsgDialogRequest request = new MessageDialogSystem.OpenMsgDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Mode = MessageDialogSystem.OpenMsgDialogRequest.MsgModes.UserMsg,
                    UserMsg = msgParams
                };

                var requestOp = new AsyncRequest<MessageDialogSystem.OpenMsgDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);
                        OnScreenLog.Add("Button Pressed : " + antecedent.Request.SelectedButton);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_Menumessages.AddItem("Open System Message", "Open the system message dialog with custom buttons", userEnabled))
            {
                MessageDialogSystem.SystemMsgParams msgParams = new MessageDialogSystem.SystemMsgParams()
                {
                    MsgType = MessageDialogSystem.SystemMsgParams.SystemMessageTypes.PSNCommunicationRestriction
                };

                MessageDialogSystem.OpenMsgDialogRequest request = new MessageDialogSystem.OpenMsgDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Mode = MessageDialogSystem.OpenMsgDialogRequest.MsgModes.SystemMsg,
                    SystemMsg = msgParams
                };

                var requestOp = new AsyncRequest<MessageDialogSystem.OpenMsgDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);
                        OnScreenLog.Add("Button Pressed : " + antecedent.Request.SelectedButton);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_Menumessages.AddItem("Open Progress Bar", "Open the progress bar dialog", userEnabled))
            {
                MessageDialogSystem.ProgressBarParams msgParams = new MessageDialogSystem.ProgressBarParams()
                {
                    BarType = MessageDialogSystem.ProgressBarParams.BarTypes.PercentageCancel,
                    Msg = "Start"
                };

                MessageDialogSystem.OpenMsgDialogRequest request = new MessageDialogSystem.OpenMsgDialogRequest()
                {
                    UserId = GamePad.activeGamePad.loggedInUser.userId,
                    Mode = MessageDialogSystem.OpenMsgDialogRequest.MsgModes.ProgressBar,
                    ProgressBar = msgParams
                };

                var requestOp = new AsyncRequest<MessageDialogSystem.OpenMsgDialogRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Dialog Status : " + antecedent.Request.Status);

                        OnScreenLog.Add("Dialog Closed...");
                    }
                });

                runningProgressBar = request;

                OnScreenLog.Add("Opening Dialog ...");

                DialogSystem.Schedule(requestOp);
            }

            if (m_Menumessages.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }
    }

}
#endif
