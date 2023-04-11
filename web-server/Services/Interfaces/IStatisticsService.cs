using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using web_server.Models.DBModels;

namespace web_server.Services.Interfaces
{
    public interface IStatisticsService
    {
        public Task<Dictionary<DateTime, List<StudentPayment>>> FormingStatData(string args);
    }
}
