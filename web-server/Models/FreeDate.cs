using System;
using System.Collections.Generic;

namespace web_server.Models
{
    public class UserDate : TransferModel
    {
        public List<DateTime> dateTimes = new List<DateTime>();
    }
    //public class FreeDateTEST : TransferModel
    //{
    //    public Dictionary<string, List<string>> Days { get; set; } = new Dictionary<string, List<string>>();
    //    public FreeDate SetHoursToDay(string day, List<string> hours)
    //    {
    //        if (!Days.ContainsKey(day))
    //        {
    //            Days.Add(day, hours);
    //        }
    //        else
    //        {
    //            Days[day] = hours;
    //        }

    //        return this ;
    //    }
    //    public FreeDate AddHoursToDay(string day, string hours)
    //    {
    //        if (!Days.ContainsKey(day))
    //        {
    //            return this;
    //        }
    //        else
    //        {
    //            Days[day].Add(hours);
    //        }

    //        return this;
    //    }
    //    public FreeDate RemoveHoursToDay(string day, string hours)
    //    {
    //        if (!Days.ContainsKey(day))
    //        {
    //            return this;
    //        }
    //        else
    //        {
    //            Days[day].Remove(hours);
    //        }

    //        return this;
    //    }

    //}
}
