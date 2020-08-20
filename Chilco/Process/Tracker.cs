using System;
using System.Diagnostics;

namespace Chilco
{
    internal class Tracker
    {
        private Stopwatch RunningTime;
        public Group group;

        public Tracker(Group group)
        {
            this.group = group;
            RunningTime = new Stopwatch();
        }

        /// <summary>
        /// Checks if any process in the Group is running.
        /// </summary>
        /// <returns>true if one or more processes in the Group are running</returns>
        private bool IsRunning()
        {
            bool running = false;
            foreach (string s in group.ruleset.Processes)
            {
                if (Process.GetProcessesByName(s).Length > 0)
                {
                    running = true;
                    break;
                }
            }
            return running;
        }

        public void CheckProcesses()
        {
            UpdateLeftoverTime();
            if (group.LeftoverTime.Ticks < 1)
            {
                Butcher.KillProcesses(group);
            }
            Console.WriteLine("Gruppe " + group.ruleset.Title + ":  " + group.LeftoverTime);
        }

        public void TimeRollover()
        {
            int diff = DateTime.Now.DayOfYear - group.DateLastRun.DayOfYear;
            if (DateTime.Now.Ticks < group.DateLastRun.Ticks) diff = 0;
            while(diff < 0)
            {
                diff += 365;
            }
            
            if (diff > 0)
            {
                if (group.ruleset.DoTimeRollover)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        group.LeftoverTime += group.ruleset.DailyPlaytime;
                    }
                }
                else
                {
                    group.LeftoverTime = group.ruleset.DailyPlaytime;
                }
                
                if (group.LeftoverTime.Ticks > group.ruleset.MaxPlaytime.Ticks && group.ruleset.MaxPlaytime.Ticks != 0)
                {
                    group.LeftoverTime = group.ruleset.MaxPlaytime;
                }
            }
            group.DateLastRun = DateTime.Now;
        }

        private void UpdateLeftoverTime()
        {
            TimeRollover();
            if (RunningTime.IsRunning)
            {
                if (group.LeftoverTime > RunningTime.Elapsed)
                    group.LeftoverTime -= RunningTime.Elapsed;
                else group.LeftoverTime = new TimeSpan(0);

                RunningTime.Reset();
            }

            if (IsRunning() && group.LeftoverTime.Ticks > 0)
            {
                RunningTime.Start();
                group.DateLastRun = DateTime.Now;
            }
        }
    }
}