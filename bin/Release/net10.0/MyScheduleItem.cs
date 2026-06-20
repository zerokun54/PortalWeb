using System;

namespace PortalWeb
{
    public class MyScheduleItem
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public int Period { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;

        // ✨ シラバス内容を保存するポケット
        public string Syllabus { get; set; } = string.Empty;
    }
}