using System;
using System.Collections.Generic;
using web_server.Models.DBModels;

namespace web_server.Models
{
    public class DisplayModelShedule
    {
        public List<Schedule> Schedules { get; set; }
        public DateTime Date { get; set; }

    }
}
