#if UNITY_PS5 || UNITY_PS4
using System.Collections.Generic;
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Checks;
using Unity.PSN.PS5.WebApi;
#endif


namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4
    public class SonyWebApiEvents : IScreen
    {
        MenuLayout m_MenuSessions;

        public SonyWebApiEvents()
        {
            Initialize();
        }

        public MenuLayout GetMenu()
        {
            return m_MenuSessions;
        }

        public void OnEnter()
        {
        }

        public void OnExit()
        {
        }

        public void Process(MenuStack stack)
        {
            MenuWebEvents(stack);
        }

        public void Initialize()
        {
            m_MenuSessions = new MenuLayout(this, 450, 20);
        }

        List<WebApiFilters> randomFilters = new List<WebApiFilters>();
        List<WebApiPushEvent> randomPushEvents = new List<WebApiPushEvent>();

        public void MenuWebEvents(MenuStack menuStack)
        {
            m_MenuSessions.Update();

            bool enabled = true;

            if (m_MenuSessions.AddItem("Create Test Filters", "Create a series of filters", enabled))
            {
                for(int i = 0; i < 5; i++)
                {
                    WebApiFilters filter = MakeRandomFilter();
                    RegisterFilter(filter);
                }
            }

            if (m_MenuSessions.AddItem("Create Test Events", "Get a series of different events", enabled && randomFilters.Count > 0))
            {
                bool makeOrdered = false;
                for (int i = 0; i < 25; i++)
                {
                    WebApiFilters filter = randomFilters[i % (randomFilters.Count-1)]; // Leave 1 filter as unsused so it can be unregistered as a test

                    WebApiPushEvent pushEvent = MakeRandomPushEvent(filter, makeOrdered);
                    RegisterPushEvent(pushEvent);

                    makeOrdered = !makeOrdered;
                }
            }

            if (m_MenuSessions.AddItem("Delete Test Filters", "Delete all test filters if they aren't referenced by events", enabled && randomFilters.Count > 0))
            {
                for (int i = 0; i < randomFilters.Count; i++)
                {
                    UnregisterFilter(randomFilters[i]);
                }
            }

            if (m_MenuSessions.AddItem("Delete Test Event", "Delete all test events", enabled && randomPushEvents.Count > 0))
            {
                for (int i = 0; i < randomPushEvents.Count; i++)
                {
                    UnregisterPushEvent(randomPushEvents[i]);
                }
            }

            if (m_MenuSessions.AddItem("View Events and Filters", "View all active events and filters", enabled))
            {
                OutputActiveEventsAndFilters();
            }

            if (m_MenuSessions.AddBackIndex("Back"))
            {
                menuStack.PopMenu();
            }
        }

        public WebApiFilters MakeRandomFilter()
        {
            WebApiFilters newFilter = new WebApiFilters();

            newFilter.AddFilterParams(new string[] { "np:service:friendlist:friend", "np:service:presence2:onlineStatus", "np:service:blocklist" });

            return newFilter;
        }

        public void RegisterFilter(WebApiFilters filters)
        {
            WebApiNotifications.RegisterFilterRequest request = new WebApiNotifications.RegisterFilterRequest()
            {
                Filters = filters
            };

            var requestOp = new AsyncRequest<WebApiNotifications.RegisterFilterRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Filter is registered");

                    OutputFullFilter(antecedent.Request.Filters);

                    randomFilters.Add(antecedent.Request.Filters);
                }
                else
                {
                    OnScreenLog.AddError("Registered Filter Request error");
                }
            });

            WebApiNotifications.Schedule(requestOp);
        }

        public void UnregisterFilter(WebApiFilters filters)
        {
            if(filters.RefCount > 0)
            {
                OnScreenLog.AddWarning("Filter " + filters.PushFilterId + " can't be unregistered as it is being referenced by 1 or more PushEvents - " + filters.RefCount);
                return;
            }

            WebApiNotifications.UnregisterFilterRequest request = new WebApiNotifications.UnregisterFilterRequest()
            {
                Filters = filters
            };

            var requestOp = new AsyncRequest<WebApiNotifications.UnregisterFilterRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Filter is unregistered");

                    OutputFullFilter(antecedent.Request.Filters);

                    randomFilters.Remove(antecedent.Request.Filters);
                }
                else
                {
                    OnScreenLog.AddError("Unregistered Filter Request error");
                }
            });

            WebApiNotifications.Schedule(requestOp);
        }


        public void NotificationEventHandler(WebApiNotifications.CallbackParams eventData)
        {

        }

        public WebApiPushEvent MakeRandomPushEvent(WebApiFilters filter, bool orderGuaranteed)
        {
            WebApiPushEvent pushEvent = new WebApiPushEvent();

            pushEvent.Filters = filter;
            pushEvent.UserId = GamePad.activeGamePad.loggedInUser.userId;
            pushEvent.OrderGuaranteed = orderGuaranteed;

            return pushEvent;
        }

        public void RegisterPushEvent(WebApiPushEvent pushEvent)
        {
            WebApiNotifications.RegisterPushEventRequest request = new WebApiNotifications.RegisterPushEventRequest()
            {
                PushEvent = pushEvent,
                Callback = NotificationEventHandler
            };

            var requestOp = new AsyncRequest<WebApiNotifications.RegisterPushEventRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("Event is registered");

                    OutputPushEventFull(antecedent.Request.PushEvent);

                    randomPushEvents.Add(antecedent.Request.PushEvent);
                }
                else
                {
                    OnScreenLog.AddError("Create PushEvent Request error");
                }
            });

            WebApiNotifications.Schedule(requestOp);
        }

        public void UnregisterPushEvent(WebApiPushEvent pushEvent)
        {
            WebApiNotifications.UnregisterPushEventRequest request = new WebApiNotifications.UnregisterPushEventRequest()
            {
                PushEvent = pushEvent
            };

            var requestOp = new AsyncRequest<WebApiNotifications.UnregisterPushEventRequest>(request).ContinueWith((antecedent) =>
            {
                if (SonyNpMain.CheckAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("PushEvent is unregistered");

                    OutputPushEventFull(antecedent.Request.PushEvent);

                    randomPushEvents.Remove(antecedent.Request.PushEvent);
                }
                else
                {
                    OnScreenLog.AddError("Unregistered PushEvent Request error");
                }
            });

            WebApiNotifications.Schedule(requestOp);
        }

        static public void OutputFullFilter(WebApiFilters filters)
        {
            OnScreenLog.Add("     ServiceName : " + filters.ServiceName + " ServiceLabel : " + filters.ServiceLabel);
            OnScreenLog.Add("     ServiceLabel : " + filters.ServiceLabel);
            OnScreenLog.Add("     PushFilterId : " + filters.PushFilterId);
            OnScreenLog.Add("     RefCount : " + filters.RefCount);

            for (int i = 0; i < filters.Filters.Count; i++)
            {
                OnScreenLog.Add("        : " + filters.Filters[i].DataType);
            }
        }

        static public void OutputFilterLine(WebApiFilters filters, bool drawColumnTitle)
        {
            string output;
            if (drawColumnTitle)
            {
                output = string.Format("{0, -15} | {1, -15} | {2, -15} | {3, -15} | {4, -15}", "PushFilterId", "ServiceName", "ServiceLabel", "RefCount", "First Filter");
                OnScreenLog.Add(output);
            }

            string firstFilter = "";
            if (filters.Filters.Count > 0) firstFilter = filters.Filters[0].DataType;

            output = string.Format("{0, -15} | {1, -15} | {2, -15} | {3, -15} | {4, -15}", filters.PushFilterId, filters.ServiceName, filters.ServiceLabel, filters.RefCount, firstFilter);
            OnScreenLog.Add(output);
        }

        static public void OutputPushEventFull(WebApiPushEvent pushEvent)
        {
            OnScreenLog.Add("  PushEvent:");
            OnScreenLog.Add("     UserId : " + pushEvent.UserId);
            OnScreenLog.Add("     PushCallbackId : " + pushEvent.PushCallbackId);
            OnScreenLog.Add("     OrderGuaranteed : " + pushEvent.OrderGuaranteed);
            OnScreenLog.Add("     Filters:");
            OutputFullFilter(pushEvent.Filters);
        }

        static public void OutputPushEventLine(WebApiPushEvent pushEvent, bool drawColumnTitle)
        {
            string output;
            if (drawColumnTitle)
            {
                output = string.Format("{0, -15} {1, -15} {2, -15} {3, -15} {4, -15}", "UserId", "PushCallbackId", "Ordered", "PushFilterId", "First Filter");
                OnScreenLog.Add(output);
            }

            string firstFilter = "";
            WebApiFilters filters = pushEvent.Filters;
            if (filters.Filters.Count > 0) firstFilter = filters.Filters[0].DataType;

            output = string.Format("{0, -15} {1, -15} {2, -15} {3, -15} {4, -15}", pushEvent.UserId, pushEvent.PushCallbackId, pushEvent.OrderGuaranteed, filters.PushFilterId, firstFilter);
            OnScreenLog.Add(output);
        }

        static public void OutputActiveEventsAndFilters()
        {
            List<WebApiFilters> filters = new List<WebApiFilters>();
            List<WebApiPushEvent> pushEvents = new List<WebApiPushEvent>();

            WebApiNotifications.GetActiveFilters(filters);
            WebApiNotifications.GetActivePushEvents(pushEvents);

            OnScreenLog.Add("Active Filters:");

            bool firstLine = true;
            for (int i = 0; i < filters.Count; i++)
            {
                OutputFilterLine(filters[i], firstLine);
                firstLine = false;
            }

            OnScreenLog.Add("Active Push Events:");

            firstLine = true;
            for (int i = 0; i < pushEvents.Count; i++)
            {
                OutputPushEventLine(pushEvents[i], firstLine);
                firstLine = false;
            }
        }
    }
#endif
}
