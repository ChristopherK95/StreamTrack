﻿using System;

namespace WpfApp1
{
    static class Notifications
    {
        public static System.Collections.ObjectModel.ObservableCollection<Notification> NotificationList = new();


        public static void AddNotifs(string StreamerName, string Status)
        {
            NotificationList.Add(new Notification() { StreamerName = StreamerName, Status = Status, TimeStamp = DateTime.Now, Id = DateTime.Now + ":" + NotificationList.Count });
        }
    }
}
