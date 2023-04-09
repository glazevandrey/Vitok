using System;
using System.Collections.Generic;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface IStatisticsService
    {
        public Dictionary<DateTime, List<StudentPayment>> FormingStatData(string args);
    }
}
