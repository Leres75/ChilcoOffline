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
        /// Checks if any process in the GetGroup is running.
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
        }

        public void TimeRollover()
        {
            int diff = DateTime.Now.DayOfYear - group.DateLastRun.DayOfYear;
            while(diff < 0)
            {
                diff += 365;
            }
            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    group.LeftoverTime += group.ruleset.DailyPlaytime;
                }
                group.DateLastRun = DateTime.Now;
            }
            if (group.LeftoverTime.Ticks > group.ruleset.MaxPlaytime.Ticks && group.ruleset.MaxPlaytime.Ticks != 0)
            {
                group.LeftoverTime = group.ruleset.MaxPlaytime;
            }
        }

        private void UpdateLeftoverTime()
        {
            if(group.ruleset.DoTimeRollover) TimeRollover();

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