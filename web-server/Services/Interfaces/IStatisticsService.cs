using System;
using System.Collections.Generic;
using web_app.Models;

namespace web_server.Services.Interfaces
{
    public interface IStatisticsService
    {
        public Dictionary<DateTime, List<StudentPayment>> FormingStatData(string args);
    }
}
