using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalonFryzjerski.Models.ViewModels
{
    public class EditAvailabilityWeekVm
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";

        public List<DayRow> Days { get; set; } = new();

        public class DayRow
        {
            public int DayOfWeek { get; set; } // 1..7 (Mon..Sun)
            public string DayName { get; set; } = "";
            public bool IsWorking { get; set; }

            [Required]
            public TimeSpan StartTime { get; set; }

            [Required]
            public TimeSpan EndTime { get; set; }
        }
    }
}