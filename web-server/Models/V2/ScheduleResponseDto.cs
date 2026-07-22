using System.Collections.Generic;
using System;
using web_server.Models.DBModels;

namespace web_server.Models.V2;

public class ScheduleResponseDto
{
    public int UserId { get; set; }
    public string Role { get; set; }
    public string DisplayName { get; set; }
    public string PhotoUrl { get; set; }
    public DateTime Date { get;set; }
    public List<Course> Courses { get; set; }
    public List<Schedule> Schedules { get; set; }
}

public class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
}

public class ScheduleDto
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public string UserName { get; set; }

    public int CourseId { get; set; }
    public CourseDto Course { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool Looped { get; set; }
    public string Status { get; set; }
}
