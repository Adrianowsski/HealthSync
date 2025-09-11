
using System;
using System.Collections.Generic;

namespace HealthSync.Intranet.Models.Notification
{

    public class NotificationDetailsViewModel
    {
        public string   Message { get; set; } = string.Empty;
        public DateTime Created { get; set; }

        public List<Row> Patients { get; set; } = new();

        public class Row
        {
            public string   Name   { get; set; } = string.Empty;
            public string   Email  { get; set; } = string.Empty;
            public bool     IsRead { get; set; }
            public DateTime? ReadAt { get; set; }
        }
    }
}