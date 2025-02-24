#if UNITY_PS5 || UNITY_PS4
using System;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Matchmaking;
using Unity.PSN.PS5.Sessions;
using UnityEngine;
#endif

#if UNITY_PS4
using PlatformInput = UnityEngine.PS4.PS4Input;
#elif UNITY_PS5
using PlatformInput = UnityEngine.PS5.PS5Input;
#endif

namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public partial class SonySessions : IScreen
    {
        // ***************************************************************************
        // Notifications
        // ***************************************************************************

        private void OnTicketNotification(Ticket.Notification notification)
        {
            OnScreenLog.Add("Ticket Notification : ", Color.cyan);

            OnScreenLog.Add("     NotificationType : " + notification.NotificationType, Color.cyan);

            if (notification.TicketId != null)
            {
                OnScreenLog.Add("     TicketId : " + notification.TicketId, Color.cyan);
            }
            else
            {
                OnScreenLog.Add("     TicketId : Null", Color.cyan);
            }

            if (notification.NotificationType == MatchMakingNotifications.NotificationTypes.TicketCanceled)
            {
                OnScreenLog.Add("     CancelReason : " + notification.CancelReason, Color.cyan);
            }
        }

        private void OnOfferNotification(Offer.Notification notification)
        {
            OnScreenLog.Add("Offer Notification : ", Color.cyan);

            OnScreenLog.Add("     NotificationType : " + notification.NotificationType, Color.cyan);

            if (notification.OfferId != null)
            {
                OnScreenLog.Add("     OfferId : " + notification.OfferId, Color.cyan);
            }
            else
            {
                OnScreenLog.Add("     OfferId : Null", Color.cyan);
            }
        }

        public void SetupMatchMakingCallbacks()
        {
            Ticket.OnTicketNotification += OnTicketNotification;
            Offer.OnOfferNotification += OnOfferNotification;
        }

        public List<TicketPlayer> GetPlayers(Session session)
        {
            if (session == null || session.Players == null || session.Players.Count == 0)
            {
                return null;
            }

            List<TicketPlayer> players = new List<TicketPlayer>();

            for (int i = 0; i < session.Players.Count; i++)
            {
                var sessionPlayer = session.Players[i];

                TicketPlayer player = new TicketPlayer();

                player.AccountId = sessionPlayer.AccountId;

                if (sessionPlayer.Platform == SessionPlatforms.PS5) player.Platform = Unity.PSN.PS5.NpPlatformType.PS5;
                else if (sessionPlayer.Platform == SessionPlatforms.PS4) player.Platform = Unity.PSN.PS5.NpPlatformType.PS4;

                player.Attributes = new List<Unity.PSN.PS5.Matchmaking.Attribute>();

                Unity.PSN.PS5.Matchmaking.Attribute attr = new Unity.PSN.PS5.Matchmaking.Attribute();
                attr.Name = "skill";
                attr.Datatype = Unity.PSN.PS5.Matchmaking.Attribute.AttrTypes.Number;
                attr.Value = "100";
                player.Attributes.Add(attr);

                player.TeamName = "Blue";

                players.Add(player);
            }

            return players;
        }

        public Ticket currentTicket = null;
        public Offer lastOffer = null;

        // ***************************************************************************
        // Menus
        // ***************************************************************************

        void DoMatchMakingButtons()
        {
            // Test the current user and calculate if they have a session and what state is it in.
            if (GamePad.activeGamePad != null && GamePad.activeGamePad.loggedInUser.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
            {
                int currentUserId = GamePad.activeGamePad.loggedInUser.userId;
                bool isUserRegistered = SessionsManager.IsUserRegistered(currentUserId);

                if (isUserRegistered == true)
                {
                    var foundSession = SessionsManager.FindPlayerSessionFromUserId(currentUserId);

                    bool enabled = foundSession != null;

                    if (m_MenuSessions.AddItem("Submit Ticket", "Submit a matchmaking ticket. Requires an active player session.", enabled))
                    {
                        List<TicketPlayer> players = GetPlayers(foundSession);

                        List<Unity.PSN.PS5.Matchmaking.Attribute> ticketAttributes = new List<Unity.PSN.PS5.Matchmaking.Attribute>();

                        Unity.PSN.PS5.Matchmaking.Attribute attr = new Unity.PSN.PS5.Matchmaking.Attribute()
                        {
                            Name = "court",
                            Datatype = Unity.PSN.PS5.Matchmaking.Attribute.AttrTypes.String,
                            Value = "Ice"
                        };

                        ticketAttributes.Add(attr);

                        AsyncOp requestOp = CreateSubmitTicketRequest(currentUserId, "Doubles_Match_Ignoring_NatType", players, ticketAttributes);


                        OnScreenLog.AddNewLine();
                        OnScreenLog.Add("Submitting ticket...");

                        SessionsManager.Schedule(requestOp);
                    }

                    // var gameSession = SessionsManager.FindGameSessionFromUserId(currentUserId);
                    string gameSessionId = null;
                    bool withGameSessionEnabled = false;
                    if (lastOffer != null && string.IsNullOrEmpty(lastOffer.Location.GameSessionId) == false)
                    {
                        gameSessionId = lastOffer.Location.GameSessionId;
                        withGameSessionEnabled = true;
                    }

                    if (m_MenuSessions.AddItem("Submit Ticket (w. Game Session)", "Submit a matchmaking ticket with a Game Session id. Requires an active player session.", withGameSessionEnabled))
                    {
                        List<TicketPlayer> players = GetPlayers(foundSession);

                        List<Unity.PSN.PS5.Matchmaking.Attribute> ticketAttributes = new List<Unity.PSN.PS5.Matchmaking.Attribute>();

                        Unity.PSN.PS5.Matchmaking.Attribute attr = new Unity.PSN.PS5.Matchmaking.Attribute()
                        {
                            Name = "court",
                            Datatype = Unity.PSN.PS5.Matchmaking.Attribute.AttrTypes.String,
                            Value = "Ice",
                        };

                        ticketAttributes.Add(attr);

                        AsyncOp requestOp = CreateSubmitTicketRequest(currentUserId, "Doubles_Match_Ignoring_NatType", players, ticketAttributes, gameSessionId);

                        OnScreenLog.AddNewLine();
                        OnScreenLog.Add("Submitting ticket... with game session id " + gameSessionId);

                        SessionsManager.Schedule(requestOp);
                    }

                    if (m_MenuSessions.AddItem("Get Ticket", "Get a matchmaking ticket. ", currentTicket != null))
                    {
                        MatchMakingRequests.GetTicketRequest request = new MatchMakingRequests.GetTicketRequest()
                        {
                            UserId = GamePad.activeGamePad.loggedInUser.userId,
                            TicketId = currentTicket.TicketId,
                            View = "v1.0"
                        };

                        var requestOp = new AsyncRequest<MatchMakingRequests.GetTicketRequest>(request).ContinueWith((antecedent) =>
                        {
                            if (SonyNpMain.CheckAysncRequestOK(antecedent))
                            {
                                OutputTicket(antecedent.Request.Ticket);

                                currentTicket = antecedent.Request.Ticket;
                            }
                        });

                        SessionsManager.Schedule(requestOp);
                    }

                    if (m_MenuSessions.AddItem("Cancel Ticket", "Cancel the ticket", currentTicket != null))
                    {
                        MatchMakingRequests.CancelTicketRequest request = new MatchMakingRequests.CancelTicketRequest()
                        {
                            UserId = GamePad.activeGamePad.loggedInUser.userId,
                            TicketId = currentTicket.TicketId,
                        };

                        var requestOp = new AsyncRequest<MatchMakingRequests.CancelTicketRequest>(request).ContinueWith((antecedent) =>
                        {
                            if (SonyNpMain.CheckAysncRequestOK(antecedent))
                            {
                                OnScreenLog.Add("Ticket cancelled");
                            }
                        });

                        SessionsManager.Schedule(requestOp);
                    }

                    if (m_MenuSessions.AddItem("Get Offer", "Get an offer. The OfferId of a ticket must be generated by PSN.", currentTicket != null && currentTicket.OfferId != null))
                    {
                        MatchMakingRequests.GetOfferRequest request = new MatchMakingRequests.GetOfferRequest()
                        {
                            UserId = GamePad.activeGamePad.loggedInUser.userId,
                            OfferId = currentTicket.OfferId,
                        };

                        var requestOp = new AsyncRequest<MatchMakingRequests.GetOfferRequest>(request).ContinueWith((antecedent) =>
                        {
                            if (SonyNpMain.CheckAysncRequestOK(antecedent))
                            {
                                OutputOffer(antecedent.Request.Offer);

                                lastOffer = antecedent.Request.Offer;
                            }
                        });

                        SessionsManager.Schedule(requestOp);
                    }

                    if (m_MenuSessions.AddItem("List User Tickets", "Get a list of User tickets.", true))
                    {
                        MatchMakingRequests.ListUserTicketsRequest request = new MatchMakingRequests.ListUserTicketsRequest()
                        {
                            UserId = GamePad.activeGamePad.loggedInUser.userId,
                            PlatformFilter = MatchMakingRequests.ListUserTicketsRequest.PlatformFilters.PS4 | MatchMakingRequests.ListUserTicketsRequest.PlatformFilters.PS5,
                            RulesetFilterName = "Doubles_Match_Ignoring_NatType"
                        };

                        var requestOp = new AsyncRequest<MatchMakingRequests.ListUserTicketsRequest>(request).ContinueWith((antecedent) =>
                        {
                            if (SonyNpMain.CheckAysncRequestOK(antecedent))
                            {
                                OutputUserTickets(antecedent.Request.UserTickets);
                            }
                        });

                        SessionsManager.Schedule(requestOp);
                    }
                }
            }

            if (m_MenuSessions.AddBackIndex("Back"))
            {
                currentMenu = MenuTypes.SessionSelection;
            }
        }
        
        // ***************************************************************************
        // Make Requests
        // ***************************************************************************

        AsyncOp CreateSubmitTicketRequest(Int32 currentUserId, string rulesetName, List<TicketPlayer> players, List<Unity.PSN.PS5.Matchmaking.Attribute> ticketAttributes, string gameSessionId = null)
        {
            MatchMakingRequests.SubmitTicketRequest request = new MatchMakingRequests.SubmitTicketRequest()
            {
                UserId = currentUserId,
                RulesetName = rulesetName,
                Players = players,
                TicketAttributes = ticketAttributes,
                GameSessionId = gameSessionId,
            };

            var requestOp = new AsyncRequest<MatchMakingRequests.SubmitTicketRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OutputTicket(antecedent.Request.Ticket);

                    currentTicket = antecedent.Request.Ticket;
                }
            });

            return requestOp;
        }


        // ***************************************************************************
        // Output
        // ***************************************************************************

        void OutputUserTickets(List<UserTicket> userTickets)
        {
            if (userTickets == null)
            {
                OnScreenLog.AddError("   OutputUserTickets failed : userTickets is null");
                return;
            }

            OnScreenLog.Add("\nUserTickets : " + userTickets.Count);

            for (int i = 0; i < userTickets.Count; i++)
            {
                UserTicket userTicket = userTickets[i];

                OnScreenLog.Add("   TicketId : " + userTicket.TicketId);
                OnScreenLog.Add("      RulesetName : " + userTicket.RulesetName);
                OnScreenLog.Add("      Platform : " + userTicket.Platform);
                OnScreenLog.Add("      Status : " + userTicket.Status);
            }
        }

        void OutputTicket(Ticket ticket)
        {
            if (ticket == null)
            {
                OnScreenLog.AddError("   OutputTicket failed : ticket is null");
                return;
            }

            OnScreenLog.Add("\nMatchmaking Ticket :");

            OnScreenLog.Add("   TicketId : " + ticket.TicketId);
            OnScreenLog.Add("   RulesetName : " + ticket.RulesetName);

            int numTicketAttributes = ticket.TicketAttributes != null ? ticket.TicketAttributes.Count : 0;

            OnScreenLog.Add("   TicketAttributes : " + numTicketAttributes);

            for (int i = 0; i < numTicketAttributes; i++)
            {
                var attr = ticket.TicketAttributes[i];
                OnScreenLog.Add("      Name : " + attr.Name + " : Type : " + attr.Datatype + " : Value : " + attr.Value);
            }

            int numPlayers = ticket.Players != null ? ticket.Players.Count : 0;

            OnScreenLog.Add("   Players : " + numPlayers);

            for (int i = 0; i < numPlayers; i++)
            {
                var player = ticket.Players[i];
                OnScreenLog.Add("      AccountId : " + player.AccountId);
                OnScreenLog.Add("         OnlineId : " + player.OnlineId);
                OnScreenLog.Add("         Platform : " + player.Platform);
                OnScreenLog.Add("         TeamName : " + player.TeamName);
                OnScreenLog.Add("         UseNateType : " + player.UseNateType);
                OnScreenLog.Add("         NatType : " + player.NatType);

                int numAttributes = player.Attributes != null ? player.Attributes.Count : 0;

                OnScreenLog.Add("         Attributes : " + numAttributes);

                for (int j = 0; j < numAttributes; j++)
                {
                    var attr = player.Attributes[j];
                    OnScreenLog.Add("            Name : " + attr.Name + " : Type : " + attr.Datatype + " : Value : " + attr.Value);
                }
            }

            OnScreenLog.Add("   Status : " + ticket.Status);

            OnScreenLog.Add("   OfferId : " + ticket.OfferId);

            if (ticket.Submitter != null)
            {
                OnScreenLog.Add("   Submitter : ");
                OnScreenLog.Add("      AccountId : " + ticket.Submitter.AccountId);
                OnScreenLog.Add("      Platform : " + ticket.Submitter.Platform);
            }
            else
            {
                OnScreenLog.Add("   Submitter : None");
            }

            if (ticket.Location != null)
            {
                OnScreenLog.Add("   Location : ");
                OnScreenLog.Add("      GameSessionId : " + ticket.Location.GameSessionId);
            }
            else
            {
                OnScreenLog.Add("   Location : None");
            }

            OnScreenLog.Add("   CreatedDateTime : " + ticket.CreatedDateTime);
            OnScreenLog.Add("   UpdatedDateTime : " + ticket.UpdatedDateTime);
        }

        void OutputOffer(Offer offer)
        {
            if (offer == null)
            {
                OnScreenLog.AddError("   OutputOffer failed : offer is null");
                return;
            }

            OnScreenLog.Add("\nMatchmaking Offer :");

            OnScreenLog.Add("   OfferId : " + offer.OfferId);
            OnScreenLog.Add("   RulesetName : " + offer.RulesetName);

            int numPlayers = offer.Players != null ? offer.Players.Count : 0;

            OnScreenLog.Add("   Players : " + numPlayers);

            for (int i = 0; i < numPlayers; i++)
            {
                var player = offer.Players[i];
                OnScreenLog.Add("      AccountId : " + player.AccountId);
                OnScreenLog.Add("         OnlineId : " + player.OnlineId);
                OnScreenLog.Add("         Platform : " + player.Platform);
                OnScreenLog.Add("         TeamName : " + player.TeamName);
                OnScreenLog.Add("         TicketId : " + player.TickedId);
            }

            OnScreenLog.Add("   Status : " + offer.Status);

            if (offer.Location != null)
            {
                OnScreenLog.Add("   Location : ");
                OnScreenLog.Add("      GameSessionId : " + offer.Location.GameSessionId);
            }
            else
            {
                OnScreenLog.Add("   Location : None");
            }

            OnScreenLog.Add("   CreatedDateTime : " + offer.CreatedDateTime);
            OnScreenLog.Add("   UpdatedDateTime : " + offer.UpdatedDateTime);
        }
    }
#endif
}
