using System;
using System.Collections.Generic;
using System.Text;

#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Sessions;
using Unity.PSN.PS5.Users;


namespace PSNSample
{
    public class SonySessionSignalling : IScreen
    {
        MenuLayout m_MenuSessionSignalling;

        public SonySessionSignalling()
        {
            Initialize();

            SessionSignalling.OnRequestNotification += OnRequestNotification;
            SessionSignalling.OnGroupNotification += OnGroupNotification;
            SessionSignalling.OnConnectionNotification += OnConnectionNotification;

            Sockets.OnRecvSocketNotification += OnRecvSocketNotification;
        }

        private void OnRequestNotification(SessionSignalling.RequestEvent reqEvent)
        {
            OnScreenLog.Add("Request Event :");
            OnScreenLog.Add("   UserId : " + reqEvent.UserId);
            OnScreenLog.Add("   CtxId : " + reqEvent.CtxId);
            OnScreenLog.Add("   ErrorCode : " + reqEvent.ErrorCode.ToString("X"));
            OnScreenLog.Add("   Id : " + reqEvent.Id);
            OnScreenLog.Add("   Reason : " + reqEvent.Reason);
        }

        private void OnGroupNotification(SessionSignalling.GroupEvent groupEvent)
        {
            OnScreenLog.Add("Group Event :");
            OnScreenLog.Add("   UserId : " + groupEvent.UserId);
            OnScreenLog.Add("   CtxId : " + groupEvent.CtxId);
            OnScreenLog.Add("   ErrorCode : " + groupEvent.ErrorCode.ToString("X"));
            OnScreenLog.Add("   Id : " + groupEvent.Id);
            OnScreenLog.Add("   Reason : " + groupEvent.Reason);

            Group group = FindGroup(groupEvent.Id);

            if (groupEvent.Id != SessionSignalling.InvalidId && group == null)
            {
                group = CreateGroup(groupEvent.CtxId, groupEvent.Id, null);
            }

            if (groupEvent.PeerActivatedData != null && groupEvent.Reason == SessionSignalling.GroupEvent.Reasons.PeerActivated && groupEvent.ErrorCode == 0)
            {
                OnScreenLog.Add("   Peer AccountId : " + groupEvent.PeerActivatedData.AccountId);
                OnScreenLog.Add("   Peer Platform : " + groupEvent.PeerActivatedData.Platform);

                if (group != null)
                {
                    group.PeerAddr = groupEvent.PeerActivatedData;
                }

                var requestOp = ActivateUser(groupEvent.CtxId, groupEvent.PeerActivatedData.AccountId, groupEvent.PeerActivatedData.Platform);

                OnScreenLog.Add("Responding to connection request other user " + groupEvent.PeerActivatedData.AccountId);

                requestOp.ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Group Id = " + antecedent.Request.GroupId);
                    }
                });

                SessionsManager.Schedule(requestOp);
            }
        }

        private void OnConnectionNotification(SessionSignalling.ConnectionEvent connEvent)
        {
            OnScreenLog.Add("Connection Event :");
            OnScreenLog.Add("   UserId : " + connEvent.UserId);
            OnScreenLog.Add("   CtxId : " + connEvent.CtxId);
            OnScreenLog.Add("   Group Id : " + connEvent.GroupId);
            OnScreenLog.Add("   ErrorCode : " + connEvent.ErrorCode.ToString("X"));
            OnScreenLog.Add("   Id : " + connEvent.Id);
            OnScreenLog.Add("   Reason : " + connEvent.Reason);

            if (connEvent.Reason == SessionSignalling.ConnectionEvent.Reasons.Established)
            {
                Connection conn = CreateConnection(connEvent.CtxId, connEvent.Id, connEvent.UserId);
            }
            else if (connEvent.Reason == SessionSignalling.ConnectionEvent.Reasons.Dead)
            {
                Connection c = Connections.Find(x => x.ConnId == connEvent.Id && x.CtxId == connEvent.CtxId);
                if (c != null)
                {
                    Connections.Remove(c);
                }
            }
        }

        public MenuLayout GetMenu()
        {
            return m_MenuSessionSignalling;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuSessionSignalling(stack);
        }

        public void Initialize()
        {
            m_MenuSessionSignalling = new MenuLayout(this, 450, 20);
        }

        private void OnRecvSocketNotification(byte[] ReceiveBuffer, uint dataLen, uint FromAddr, ushort FromPort, ushort FromVirtualPort)
        {
            OnScreenLog.Add("OnRecvSocketNotification : ", UnityEngine.Color.green);

            string message = Encoding.UTF8.GetString(ReceiveBuffer, 0, (int)dataLen);

            OnScreenLog.Add("   Data : " + message, UnityEngine.Color.green);
            OnScreenLog.Add("   FromAddr : " + FromAddr.ToString("X"), UnityEngine.Color.green);
            OnScreenLog.Add("   FromPort : " + FromPort, UnityEngine.Color.green);
            OnScreenLog.Add("   FromVirtualPort : " + FromVirtualPort, UnityEngine.Color.green);
        }

        class SocketState
        {
            public readonly UInt16 VirtualSendPort = 4126;
            public readonly UInt16 VirtualRecvPort = 4649;

            public enum States
            {
                Invalid,
                Pending,
                Ready,
                Error,
            }

            public States State = States.Invalid;

            public SessionSignalling.GetLocalNetInfoRequest LocalNetRequest;
            public SessionSignalling.GetNatRouterInfoRequest NatRouterInfoRequest;
            public Sockets.SetupUdpP2PSocketRequest SendSocketRequest;
            public Sockets.SetupUdpP2PSocketRequest ReceiveSocketRequest;

            public Int32 SendSocketId
            {
                get
                {
                    if (SendSocketRequest != null && SendSocketRequest.Result.apiResult == APIResultTypes.Success)
                    {
                        return SendSocketRequest.SocketId;
                    }

                    return -1;
                }
            }

            public Int32 RecvSocketId
            {
                get
                {
                    if (ReceiveSocketRequest != null && ReceiveSocketRequest.Result.apiResult == APIResultTypes.Success)
                    {
                        return ReceiveSocketRequest.SocketId;
                    }

                    return -1;
                }
            }

            public UInt32 LocalNetAddr
            {
                get
                {
                    if (LocalNetRequest != null && LocalNetRequest.Result.apiResult == APIResultTypes.Success && LocalNetRequest.LocalNetInfo != null)
                    {
                        return LocalNetRequest.LocalNetInfo.LocalAddr;
                    }

                    return 0;
                }
            }

            public byte[] ReceiveBuffer = new byte[8192];

            public void InitialiseSockets(UInt32 ctxId)
            {
                if (State != States.Invalid)
                {
                    return;
                }

                State = States.Pending;

                LocalNetRequest = new SessionSignalling.GetLocalNetInfoRequest()
                {
                    CtxId = ctxId
                };

                var requestOp = new AsyncRequest<SessionSignalling.GetLocalNetInfoRequest>(LocalNetRequest).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        if (antecedent.Request.LocalNetInfo != null)
                        {
                            OnScreenLog.Add("Retrieved Local IP addree : " + antecedent.Request.LocalNetInfo.LocalAddrStr);
                            CreateSockets(antecedent.Request.LocalNetInfo.LocalAddr);
                        }
                        else
                        {
                            State = States.Error;
                        }
                    }
                    else
                    {
                        State = States.Error;
                    }
                });

                OnScreenLog.Add("Getting Local IP address");

                SessionsManager.Schedule(requestOp);
            }

            void CreateSockets(UInt32 localNetAddr)
            {
                //Send Socket
                SendSocketRequest = new Sockets.SetupUdpP2PSocketRequest()
                {
                    SocketName = "SampleUdpSend",
                    NetAddress = localNetAddr,
                    VirtualPort = VirtualSendPort
                };

                var requestOp = new AsyncRequest<Sockets.SetupUdpP2PSocketRequest>(SendSocketRequest).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Send Socket Created : " + antecedent.Request.SocketId);
                    }
                    else
                    {
                        State = States.Error;
                    }
                });

                OnScreenLog.Add("Creating send socket");

                SessionsManager.Schedule(requestOp);

                // Receive Socket
                ReceiveSocketRequest = new Sockets.SetupUdpP2PSocketRequest()
                {
                    SocketName = "SampleUdpRecv",
                    NetAddress = 0,
                    VirtualPort = VirtualRecvPort
                };

                requestOp = new AsyncRequest<Sockets.SetupUdpP2PSocketRequest>(ReceiveSocketRequest).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Receive Socket Created : " + antecedent.Request.SocketId);
                        Sockets.SetNetReceiveData(antecedent.Request.SocketId, ReceiveBuffer);

                        // Last async op to be called. Check for valid state
                        if(LocalNetRequest != null && LocalNetRequest.Result.apiResult == APIResultTypes.Success &&
                           SendSocketRequest != null && SendSocketRequest.Result.apiResult == APIResultTypes.Success &&
                           ReceiveSocketRequest != null && ReceiveSocketRequest.Result.apiResult == APIResultTypes.Success)
                        {
                            State = States.Ready;
                            OnScreenLog.Add("Sockets Ready");
                        }
                        else
                        {
                            State = States.Error;
                        }
                    }
                    else
                    {
                        State = States.Error;
                    }
                });

                OnScreenLog.Add("Creating Receive socket");

                SessionsManager.Schedule(requestOp);
            }
        }

        SocketState sockets = new SocketState();

        class OnLineFriend
        {
            public UInt64 AccountId;
            public UserSystem.UserProfile UserProfile;

            public string DisplayName
            {
                get
                {
                    if(UserProfile == null)
                    {
                        if (AccountId > 0)
                        {
                            return AccountId.ToString("X");
                        }

                        return "Unknown";
                    }

                    if(UserProfile.PersonalDetails != null)
                    {
                        if( string.IsNullOrEmpty(UserProfile.PersonalDetails.DisplayName) == false)
                        {
                            return UserProfile.PersonalDetails.DisplayName;
                        }

                        return UserProfile.PersonalDetails.FirstName + " " + UserProfile.PersonalDetails.LastName;
                    }

                    return UserProfile.OnlineId;
                }
            }
        }

        class SignallingUser
        {
            public UInt32 CtxId;
            public Int32 UserId;
            public UInt32 RequestId;
            public List<OnLineFriend> OnlineFriends = null;
        }

        List<SignallingUser> SignallingUsers = new List<SignallingUser>();

        SignallingUser FindUser(Int32 userId)
        {
            for (int i = 0; i < SignallingUsers.Count; i++)
            {
                if (SignallingUsers[i].UserId == userId)
                {
                    return SignallingUsers[i];
                }
            }

            return null;
        }

        Int32 FindUserId(UInt32 ctxId)
        {
            for (int i = 0; i < SignallingUsers.Count; i++)
            {
                if (SignallingUsers[i].CtxId == ctxId)
                {
                    return SignallingUsers[i].UserId;
                }
            }

            return 0;
        }

        class Group
        {
            public UInt32 CtxId = SessionSignalling.InvalidId;
            public UInt32 GroupId = SessionSignalling.InvalidId;
            public Int32 LocalUserId;
            public SessionSignalling.PeerAddress PeerAddr;
        }

        List<Group> Groups = new List<Group>();

        Group CreateGroup(UInt32 ctxId, UInt32 groupId, SessionSignalling.PeerAddress peerAddr)
        {
            Int32 userId = FindUserId(ctxId);

            Group newGroup = FindGroup(groupId);

            if(newGroup == null)
            {
                newGroup = new Group();
                newGroup.CtxId = ctxId;
                newGroup.GroupId = groupId;
                newGroup.LocalUserId = userId;
                newGroup.PeerAddr = peerAddr;

                Groups.Add(newGroup);
            }

            return newGroup;
        }

        Group FindGroup(UInt32 groupId)
        {
            for (int i = 0; i < Groups.Count; i++)
            {
                if (Groups[i].GroupId == groupId)
                {
                    return Groups[i];
                }
            }

            return null;
        }

        class Connection
        {
            public UInt32 CtxId = SessionSignalling.InvalidId;
            public UInt32 ConnId = SessionSignalling.InvalidId;
            public Int32 LocalUserId;
            public SessionSignalling.ConnectionStatus Status;
        }

        List<Connection> Connections = new List<Connection>();

        Connection CreateConnection(UInt32 ctxId, UInt32 connId, Int32 userId)
        {
            Connection newConnection = FindConnection(connId);

            if (newConnection == null)
            {
                newConnection = new Connection();
                newConnection.CtxId = ctxId;
                newConnection.ConnId = connId;
                newConnection.LocalUserId = userId;

                Connections.Add(newConnection);
            }

            return newConnection;
        }

        Connection FindConnection(UInt32 connId)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].ConnId == connId)
                {
                    return Connections[i];
                }
            }

            return null;
        }

        void CreateConextRequest(Int32 userId)
        {
            SessionSignalling.CreateUserContextRequest request = new SessionSignalling.CreateUserContextRequest()
            {
                UserId = userId
            };

            var requestOp = new AsyncRequest<SessionSignalling.CreateUserContextRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Ctx Id = " + antecedent.Request.CtxId);

                    SignallingUser newUser = new SignallingUser();
                    newUser.CtxId = antecedent.Request.CtxId;
                    newUser.UserId = antecedent.Request.UserId;

                    SignallingUsers.Add(newUser);
                }
            });

            OnScreenLog.Add("Preparing User " + userId);

            SessionsManager.Schedule(requestOp);
        }

        void PrepareRequest(Int32 userId)
        {
            SignallingUser foundUser = FindUser(userId);

            if(foundUser == null)
            {
                OnScreenLog.AddError("Can't find Ctx Id for User " + userId);
            }

            SessionSignalling.UserToUserRequest request = new SessionSignalling.UserToUserRequest()
            {
                CtxId = foundUser.CtxId
            };

            var requestOp = new AsyncRequest<SessionSignalling.UserToUserRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Request Id = " + antecedent.Request.RequestId);

                    foundUser.RequestId = antecedent.Request.RequestId;
                }
            });

            OnScreenLog.Add("Preparing User " + userId);

            SessionsManager.Schedule(requestOp);
        }

        enum MenuTypes
        {
            MainSignalling,
            Friends,
            Sessions,
            Groups,
            SendTo,
            ConnectionInfo,
        }

        MenuTypes currentMenu = MenuTypes.MainSignalling;

        public void MenuSessionSignalling(MenuStack menuStack)
        {
            m_MenuSessionSignalling.Update();

            SignallingUser currentUser = FindUser(GamePad.activeGamePad.loggedInUser.userId);

            if (currentUser != null)
            {
                // Check to make sure the socket system has been initialised once a user has been assigned.
                if (sockets.State == SocketState.States.Invalid)
                {
                    sockets.InitialiseSockets(currentUser.CtxId);
                }
            }

            if (currentMenu == MenuTypes.MainSignalling)
            {
                DoMainButtons(menuStack);
            }
            else if (currentMenu == MenuTypes.Friends)
            {
                DoFriendsButtons();
            }
            else if (currentMenu == MenuTypes.Sessions)
            {
                DoSessionsButtons();
            }
            else if (currentMenu == MenuTypes.Groups)
            {
                DoGroupsButtons();
            }
            else if (currentMenu == MenuTypes.SendTo)
            {
                DoSendToButtons();
            }
            else if (currentMenu == MenuTypes.ConnectionInfo)
            {
                DoConnectionInfoButtons();
            }
        }

        public void DoMainButtons(MenuStack menuStack)
        {
            SignallingUser currentUser = FindUser(GamePad.activeGamePad.loggedInUser.userId);

            if (m_MenuSessionSignalling.AddItem("Create User Context", "Create a Context Id for the current user", currentUser == null))
            {
                CreateConextRequest(GamePad.activeGamePad.loggedInUser.userId);
            }

            if (m_MenuSessionSignalling.AddItem("Get NAT Router Info", "Obtain NAT router information", true ))
            {
                SessionSignalling.GetNatRouterInfoRequest request = new SessionSignalling.GetNatRouterInfoRequest();

                var requestOp = new AsyncRequest<SessionSignalling.GetNatRouterInfoRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        if (antecedent.Request.NatRouterInfo != null)
                        {
                            Unity.PSN.PS5.Sessions.SessionSignalling.NatRouterInfo natRouterInfo = antecedent.Request.NatRouterInfo;
                            OnScreenLog.Add("NAT Router Info :");
                            OnScreenLog.Add("   StunStatus = " + natRouterInfo.StunStatus);
                            if (natRouterInfo.StunStatus == Unity.PSN.PS5.Sessions.SessionSignalling.NatInfoStunStatus.Ok)
                            {
                                OnScreenLog.Add("   MappedAddr = " + natRouterInfo.MappedAddr.ToString("X"));
                                OnScreenLog.Add("   MappedAddrStr = " + natRouterInfo.MappedAddrStr);
                                OnScreenLog.Add("   NatType = " + natRouterInfo.NatType);
                            }
                        }
                    }
                });

                OnScreenLog.Add("Getting Local Net Info");

                SessionsManager.Schedule(requestOp);
            }

            bool isPrepared = currentUser != null && currentUser.RequestId != SessionSignalling.InvalidId;

            if (m_MenuSessionSignalling.AddItem("Prepare User", "Prepare the user to recieve a connection", isPrepared == false && currentUser != null))
            {
                PrepareRequest(currentUser.UserId);
            }

            if (m_MenuSessionSignalling.AddItem("Get Local Net Info", "Get the local net info", isPrepared))
            {
                SessionSignalling.GetLocalNetInfoRequest request = new SessionSignalling.GetLocalNetInfoRequest()
                {
                    CtxId = currentUser.CtxId
                };

                var requestOp = new AsyncRequest<SessionSignalling.GetLocalNetInfoRequest>(request).ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        if (antecedent.Request.LocalNetInfo != null)
                        {
                            OutputNetInfo(antecedent.Request.LocalNetInfo);
                            sockets.LocalNetRequest = antecedent.Request;
                        }
                    }
                });

                OnScreenLog.Add("Getting Local Net Info");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessionSignalling.AddItem("Connect to Friends", "List all online friends", isPrepared))
            {
                m_MenuSessionSignalling.SetSelectedItem(0);
                currentMenu = MenuTypes.Friends;
            }

            if (m_MenuSessionSignalling.AddItem("Connect to Session", "Connect to a session", isPrepared))
            {
                m_MenuSessionSignalling.SetSelectedItem(0);
                currentMenu = MenuTypes.Sessions;
            }

            if (m_MenuSessionSignalling.AddItem("Deactivate Groups", "Deactivate the current groups", isPrepared))
            {
                m_MenuSessionSignalling.SetSelectedItem(0);
                currentMenu = MenuTypes.Groups;
            }

            if (m_MenuSessionSignalling.AddItem("Send Data", "Send to any established connections", isPrepared))
            {
                m_MenuSessionSignalling.SetSelectedItem(0);
                currentMenu = MenuTypes.SendTo;
            }

            if (m_MenuSessionSignalling.AddItem("Connection Info", "List current connection info", isPrepared))
            {
                m_MenuSessionSignalling.SetSelectedItem(0);
                currentMenu = MenuTypes.ConnectionInfo;
            }

            if (m_MenuSessionSignalling.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        public void DoFriendsButtons()
        {
            SignallingUser currentUser = FindUser(GamePad.activeGamePad.loggedInUser.userId);

            bool isPrepared = currentUser != null && currentUser.RequestId != SessionSignalling.InvalidId;

            if (m_MenuSessionSignalling.AddItem("Get Online Friends", "Get a list of online friends.", currentUser != null))
            {
                AsyncOp requestOp = GetFriends(currentUser);

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Get Online Friends");

                SessionsManager.Schedule(requestOp);
            }

            if(currentUser != null && currentUser.OnlineFriends != null && currentUser.OnlineFriends.Count > 0)
            {
                for (int i = 0; i < currentUser.OnlineFriends.Count; i++)
                {
                    var onlineFriend = currentUser.OnlineFriends[i];

                    string buttonName = "Connect to " + onlineFriend.DisplayName;

                    if (m_MenuSessionSignalling.AddItem(buttonName, "Connect to the online friend"))
                    {
                        var requestOp = ActivateUser(currentUser.CtxId, onlineFriend.AccountId, NpPlatformType.PS5);

                        requestOp.ContinueWith((antecedent) =>
                        {
                            if (SonyNpMain.CheckAysncRequestOK(antecedent))
                            {
                                OnScreenLog.Add("Group Id = " + antecedent.Request.GroupId);

                                Group newGroup = CreateGroup(antecedent.Request.CtxId, antecedent.Request.GroupId, antecedent.Request.PeerAddr);
                            }
                        });

                        OnScreenLog.Add("Starting connection to other user " + onlineFriend.AccountId);

                        SessionsManager.Schedule(requestOp);
                    }
                }
            }

            if (m_MenuSessionSignalling.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.MainSignalling;
            }
        }

        public void DoSessionsButtons()
        {
            SignallingUser currentUser = FindUser(GamePad.activeGamePad.loggedInUser.userId);
            PlayerSession ps = null;

            if(currentUser != null)
            {
                ps = SessionsManager.FindPlayerSessionFromUserId(currentUser.UserId);
            }

            bool isPrepared = currentUser != null && currentUser.RequestId != SessionSignalling.InvalidId && ps != null;

            if (m_MenuSessionSignalling.AddItem("Activate Player Session", "Activate the current players session", isPrepared))
            {
                var requestOp = ActivateSession(currentUser.CtxId, ps);

                requestOp.ContinueWith((antecedent) =>
                {
                    if (SonyNpMain.CheckAysncRequestOK(antecedent))
                    {
                        OnScreenLog.Add("Group Id = " + antecedent.Request.GroupId);

                        Group newGroup = CreateGroup(antecedent.Request.CtxId, antecedent.Request.GroupId, null);
                    }
                });

                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Activating session...");

                SessionsManager.Schedule(requestOp);
            }

            if (m_MenuSessionSignalling.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.MainSignalling;
            }
        }

        public void DoGroupsButtons()
        {
            if (Groups != null && Groups.Count > 0)
            {
                for (int i = 0; i < Groups.Count; i++)
                {
                    string buttonName = "Deactivate Group Id " + Groups[i].GroupId;
                    string helpMsg = "Deactivate the Group";

                    if (Groups[i].PeerAddr != null)
                    {
                        helpMsg = "Deactivate the Group for peer " + Groups[i].PeerAddr.AccountId;
                    }

                    if (m_MenuSessionSignalling.AddItem(buttonName, helpMsg))
                    {
                        SessionSignalling.DeactivateRequest request = new SessionSignalling.DeactivateRequest()
                        {
                            CtxId = Groups[i].CtxId,
                            GroupId = Groups[i].GroupId
                        };

                        var requestOp = new AsyncRequest<SessionSignalling.DeactivateRequest>(request).ContinueWith((antecedent) =>
                        {
                            if (SonyNpMain.CheckAysncRequestOK(antecedent))
                            {
                                OnScreenLog.Add("DeactivateRequest Complete : GroupId " + antecedent.Request.GroupId);
                            }
                        });

                        OnScreenLog.Add("Deactivating group id " + Groups[i].GroupId);

                        SessionsManager.Schedule(requestOp);
                    }
                }
            }

            if (m_MenuSessionSignalling.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.MainSignalling;
            }
        }

        public void DoSendToButtons()
        {
            if (Connections != null && Connections.Count > 0)
            {
                for (int i = 0; i < Connections.Count; i++)
                {
                    Connection connection = Connections[i];

                    if (connection.Status == null)
                    {
                        string buttonName = "Get Connection Status " + connection.ConnId;

                        if (m_MenuSessionSignalling.AddItem(buttonName, "Get the status of the last connection"))
                        {
                            SessionSignalling.GetConnectionStatusRequest request = new SessionSignalling.GetConnectionStatusRequest()
                            {
                                CtxId = connection.CtxId,
                                ConnectionId = connection.ConnId
                            };

                            var requestOp = new AsyncRequest<SessionSignalling.GetConnectionStatusRequest>(request).ContinueWith((antecedent) =>
                            {
                                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                                {
                                    OutputConnectionStatus(antecedent.Request.Status);
                                    connection.Status = antecedent.Request.Status;
                                }
                            });

                            OnScreenLog.Add("Getting connection status for connection id " + connection.ConnId);

                            SessionsManager.Schedule(requestOp);
                        }
                    }
                    else
                    {
                        bool enabled = sockets.State == SocketState.States.Ready;

                        string buttonName = "Send Packet " + connection.ConnId;

                        if (m_MenuSessionSignalling.AddItem(buttonName, "Send a packet", enabled))
                        {
                            string IPAddressStr = "Unknown";

                            if (sockets.LocalNetRequest != null && sockets.LocalNetRequest.LocalNetInfo != null)
                            {
                                IPAddressStr = sockets.LocalNetRequest.LocalNetInfo.MappedAddrStr;
                            }

                            string someMessage = "This is a test message from " + IPAddressStr;

                            byte[] bytes = Encoding.UTF8.GetBytes(someMessage);

                            Sockets.SendRequest request = new Sockets.SendRequest()
                            {
                                SocketId = sockets.SendSocketId,
                                Data = bytes,
                                SendToAddress = connection.Status.PeerAddr,
                                SendToPort = connection.Status.Port,
                                ReceiveVirtualPort = sockets.VirtualRecvPort,
                                Encrypt = true,
                            };

                            var requestOp = new AsyncRequest<Sockets.SendRequest>(request).ContinueWith((antecedent) =>
                            {
                                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                                {
                                    OnScreenLog.Add("Send done");
                                }
                                else
                                {
                                    OnScreenLog.AddError("Send failed");
                                }
                            });

                            OnScreenLog.Add("Sending message :  " + someMessage, UnityEngine.Color.cyan);
                            OnScreenLog.Add("To :  " + connection.Status.PeerAddrStr + " : " + connection.Status.Port, UnityEngine.Color.cyan);

                            SessionsManager.Schedule(requestOp);
                        }
                    }
                }
            }

            if (m_MenuSessionSignalling.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.MainSignalling;
            }
        }

        public void DoConnectionInfoButtons()
        {
            if (Connections != null && Connections.Count > 0)
            {
                for (int i = 0; i < Connections.Count; i++)
                {
                    Connection connection = Connections[i];

                    string buttonName = "Get Connection Info " + connection.ConnId;

                    if (m_MenuSessionSignalling.AddItem(buttonName, "Get the various connection info types"))
                    {
                        foreach (SessionSignalling.ConnectionInfoCodes infoCode in Enum.GetValues(typeof(SessionSignalling.ConnectionInfoCodes)))
                        {
                            SessionSignalling.GetConnectionInfoRequest request = new SessionSignalling.GetConnectionInfoRequest()
                            {
                                CtxId = connection.CtxId,
                                ConnectionId = connection.ConnId,
                                InfoCode = infoCode
                            };

                            var requestOp = new AsyncRequest<SessionSignalling.GetConnectionInfoRequest>(request).ContinueWith((antecedent) =>
                            {
                                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                                {
                                    OutputConnectionInfo(antecedent.Request.Info);
                                }
                            });

                            OnScreenLog.Add("Getting connection info for connection id " + connection.ConnId + " using code " + infoCode);

                            SessionsManager.Schedule(requestOp);
                        }
                    }
                }
            }

            if (m_MenuSessionSignalling.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.MainSignalling;
            }
        }

        AsyncOp GetFriends(SignallingUser preparedUser)
        {
            UInt32 limit = 95;

            preparedUser.OnlineFriends = null;
         //   preparedUser.FriendAccountIds = null;

            UserSystem.GetFriendsRequest friendRequest = new UserSystem.GetFriendsRequest()
            {
                UserId = preparedUser.UserId,
                Offset = 0,
                Limit = limit,
                Filter = UserSystem.GetFriendsRequest.Filters.Online,
                SortOrder = UserSystem.GetFriendsRequest.Order.OnlineId,
            };

            UserSystem.GetProfilesRequest profileRequest = new UserSystem.GetProfilesRequest()
            {
                UserId = preparedUser.UserId,
                AccountIds = friendRequest.RetrievedAccountIds,
                RetrievedProfiles = new List<UserSystem.UserProfile>()
            };

            var requestOp = new AsyncRequest<UserSystem.GetFriendsRequest>(friendRequest).ContinueWith(new AsyncRequest<UserSystem.GetProfilesRequest>(profileRequest)).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Retrieved Friends Profiles...");

                    List<UserSystem.UserProfile> profiles = antecedent.Request.RetrievedProfiles;

                    if (profiles == null || profiles.Count == 0 || antecedent.Request.AccountIds == null || antecedent.Request.AccountIds.Count == 0)
                    {
                        OnScreenLog.AddWarning("No friends found online");
                    }
                    else
                    {
                        preparedUser.OnlineFriends = new List<OnLineFriend>();

                        for (int i = 0; i < profiles.Count; i++)
                        {
                            OnLineFriend friend = new OnLineFriend();

                            UInt64 accountId = antecedent.Request.AccountIds[i];

                            friend.AccountId = accountId;
                            friend.UserProfile = profiles[i];

                            preparedUser.OnlineFriends.Add(friend);

                            OnScreenLog.Add("  " + friend.AccountId + " : " + friend.DisplayName);
                        }
                    }
                }
            });

            return requestOp;
        }

        AsyncRequest<SessionSignalling.ActivateUserRequest> ActivateUser(UInt32 ctxId, UInt64 accountId, NpPlatformType platform)
        {
            SessionSignalling.PeerAddress peerAddr = new SessionSignalling.PeerAddress();
            peerAddr.AccountId = accountId;
            peerAddr.Platform = platform;

            SessionSignalling.ActivateUserRequest request = new SessionSignalling.ActivateUserRequest()
            {
                CtxId = ctxId,
                PeerAddr = peerAddr
            };

            var requestOp = new AsyncRequest<SessionSignalling.ActivateUserRequest>(request);

            return requestOp;
        }

        AsyncRequest<SessionSignalling.ActivateSessionRequest> ActivateSession(UInt32 ctxId, Session session)
        {
            SessionSignalling.ActivateSessionRequest request = new SessionSignalling.ActivateSessionRequest()
            {
                CtxId = ctxId,
                SessionID = session.SessionId,
                SessionType = (session is PlayerSession) ? SessionSignalling.SignalingSessionTypes.PlayerSession : SessionSignalling.SignalingSessionTypes.GameSession,
                TopologyType = SessionSignalling.SignalingTopologyTypes.Mesh,
                HostType = SessionSignalling.SignalingHostTypes.None,
            };

            var requestOp = new AsyncRequest<SessionSignalling.ActivateSessionRequest>(request);

            return requestOp;
        }

        void OutputNetInfo(SessionSignalling.NetInfo netInfo)
        {
            if (netInfo == null)
            {
                OnScreenLog.AddError("   OutputNetInfo failed : netInfo is null");
                return;
            }

            OnScreenLog.Add("Net Info :");
            OnScreenLog.Add("   LocalAddr = " + netInfo.LocalAddr.ToString("X"));
            OnScreenLog.Add("   LocalAddrStr = " + netInfo.LocalAddrStr);
            OnScreenLog.Add("   MappedAddr = " + netInfo.MappedAddr.ToString("X"));
            OnScreenLog.Add("   MappedAddrStr = " + netInfo.MappedAddrStr);
            OnScreenLog.Add("   NatStatus = " + netInfo.NatStatus);
        }

        void OutputConnectionStatus(SessionSignalling.ConnectionStatus connStatus)
        {
            if (connStatus == null)
            {
                OnScreenLog.AddError("   OutputConnectionStatus failed : connStatus is null");
                return;
            }

            OnScreenLog.Add("Connection Status :");
            OnScreenLog.Add("   Status = " + connStatus.Status);
            OnScreenLog.Add("   PeerAddr = " + connStatus.PeerAddr.ToString("X"));
            OnScreenLog.Add("   PeerAddrStr = " + connStatus.PeerAddrStr);
            OnScreenLog.Add("   Port = " + connStatus.Port);
        }

        void OutputConnectionInfo(SessionSignalling.ConnectionInfo connInfo)
        {
            if (connInfo == null)
            {
                OnScreenLog.AddError("   OutputConnectionInfo failed : connInfo is null");
                return;
            }

            OnScreenLog.Add("Connection Info :");
            OnScreenLog.Add("   InfoCode = " + connInfo.InfoCode);

            if(connInfo.InfoCode == SessionSignalling.ConnectionInfoCodes.RoundTripTime)
            {
                OnScreenLog.Add("   RoundTripTime (μs) = " + connInfo.RoundTripTime);
            }
            else if (connInfo.InfoCode == SessionSignalling.ConnectionInfoCodes.NetAddress)
            {
                OnScreenLog.Add("   Peer NetAddress = " + connInfo.NetAddress.ToString("X"));
                OnScreenLog.Add("   Peer NetAddressStr = " + connInfo.NetAddressStr);
                OnScreenLog.Add("   Peer Port = " + connInfo.Port);
            }
            else if (connInfo.InfoCode == SessionSignalling.ConnectionInfoCodes.MappedAddress)
            {
                OnScreenLog.Add("   Mapped NetAddress = " + connInfo.NetAddress.ToString("X"));
                OnScreenLog.Add("   Mapped NetAddressStr = " + connInfo.NetAddressStr);
                OnScreenLog.Add("   Mapped Port = " + connInfo.Port);
            }
            else if (connInfo.InfoCode == SessionSignalling.ConnectionInfoCodes.PacketLoss)
            {
                OnScreenLog.Add("   PacketLoss (%) = " + connInfo.PacketLoss);
            }
            else if (connInfo.InfoCode == SessionSignalling.ConnectionInfoCodes.PeerAddress)
            {
                OnScreenLog.Add("   Peer AccountId = " + connInfo.PeerAddress.AccountId);
                OnScreenLog.Add("   Peer Platform = " + connInfo.PeerAddress.Platform);
            }
        }
    }
}
#endif
