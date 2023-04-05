using System;
using System.Collections.Generic;
using web_server.Models;

namespace web_app.Models
{
    public class DisplayModelShedule
    {
        public List<Schedule> Schedules { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<int, DateTime> Waited { get; set; }

    }
}
