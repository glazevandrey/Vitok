namespace web_application.Helpers
{
    public class Functions
    {
        //public static UserDate ConvertViewTimeToFreeDate(string raw_time, User tutor)
        //{

        //    var res = new FreeDate();

        //    var split = raw_time.Split(',');

        //    foreach (var day in split)
        //    {
        //        var _day = "";
        //        if (day[2] == '0')
        //        {
        //            _day = "Понедельник";
        //        }
        //        if (day[2] == '1')
        //        {
        //            _day = "Вторник";
        //        }
        //        if (day[2] == '2')
        //        {
        //            _day = "Среда";
        //        }
        //        if (day[2] == '3')
        //        {
        //            _day = "Четверг";
        //        }
        //        if (day[2] == '4')
        //        {
        //            _day = "Пятница";
        //        }
        //        if (day[2] == '5')
        //        {
        //            _day = "Суббота";
        //        }
        //        if (day[2] == '6')
        //        {
        //            _day = "Воскресенье";
        //        }

        //        res.SetHoursToDay(_day, new System.Collections.Generic.List<string>());
        //        var availible_times = tutor.FreeDates.Days[_day];
        //        var int_time = 0;
        //        var int_time2 = 0;

        //        string time = "";

        //        //                     a047
        //        if (day.Length == 4)
        //        {
        //            int_time = Convert.ToInt32(day[3].ToString());
        //            time = availible_times[int_time];
        //            res.AddHoursToDay(_day, time);
        //        }
        //        else if(day.Length > 4)
        //        {
        //            //                     a0412

        //            int_time = Convert.ToInt32(day[3].ToString());
        //            int_time2 = Convert.ToInt32(day[4].ToString());
        //            time = availible_times[int_time+int_time2];
        //            res.AddHoursToDay(_day, time);
        //        }
        //        else
        //        {
        //            return res;
        //        }
        //    }

        //    return res;
        //}
    }
}
