using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace MinicraftPlusSharp.Gfx
{
    public abstract class Ellipsis
    {

        private readonly DotUpdater updateMethod;

        protected Ellipsis(DotUpdater updateMethod, int intervalCount)
        {
            this.updateMethod = updateMethod;
            updateMethod.SetIntervalCount(intervalCount);
        }

        public string UpdateAndGet()
        {
            updateMethod.Update();
            return Get();
        }

        public abstract string Get();

        public virtual void NextInterval(int interval) { }

        protected int GetInterval()
        {
            return updateMethod.GetInterval();
        }

        protected int GetIntervalCount()
        {
            return updateMethod.GetIntervalCount();
        }
    }



    public class SequentialEllipsis : Ellipsis
    {

        public SequentialEllipsis()
            : this(new CallUpdater(Updater.normSpeed * 2 / 3))
        {
        }

        public SequentialEllipsis(DotUpdater updater)
            : base(updater, 3)
        {
        }

        public override string Get()
        {
            StringBuilder dots = new StringBuilder();

            int ePos = GetInterval();

            for (int i = 0; i < GetIntervalCount(); i++)
            {
                if (ePos == i)
                {
                    dots.Append(".");
                }
                else
                {
                    dots.Append(" ");
                }
            }

            return dots.ToString();
        }
    }

    public class SmoothEllipsis : Ellipsis
    {
        private static readonly string dotString = "   ";

        private char[] Dots => dotString.ToCharArray();

        public SmoothEllipsis()
            : this(new TimeUpdater())
        {
        }
        public SmoothEllipsis(DotUpdater updater)
            : base(updater, dotString.Length * 2)
        {
            updater.SetEllipsis(this);
        }

        public override string Get()
        {
            return new string(Dots);
        }

        public override void NextInterval(int interval)
        {
            int epos = interval % Dots.Length;
            char set = interval < GetIntervalCount() / 2 ? '.' : ' ';
            Dots[epos] = set;
        }
    }


    public abstract class DotUpdater
    {
        private readonly int countPerCycle;
        private int intervalCount;
        private int curInterval;
        private int countPerInterval;
        private int counter;

        private Ellipsis ellipsis = null;

        private bool started = false;

        protected DotUpdater(int countPerCycle)
        {
            this.countPerCycle = countPerCycle;
        }

        public void SetEllipsis(Ellipsis ellipsis)
        {
            this.ellipsis = ellipsis;
        }

        // called by Ellipsis classes, passing their value.
        public void SetIntervalCount(int numIntervals)
        {
            intervalCount = numIntervals;
            countPerInterval = Math.Max(1, (int)Math.Round(countPerCycle / (float)intervalCount));
        }

        public int GetInterval()
        {
            return curInterval;
        }

        public int GetIntervalCount()
        {
            return intervalCount;
        }

        private void IncInterval(int amt)
        {
            if (ellipsis != null)
            {
                for (int i = curInterval + 1; i <= curInterval + amt; i++)
                {
                    ellipsis.NextInterval(i % intervalCount);
                }
            }

            curInterval += amt;
            curInterval %= intervalCount;
        }

        protected void IncCounter(int amt)
        {
            counter += amt;

            int intervals = counter / countPerInterval;

            if (intervals > 0)
            {
                IncInterval(intervals);
                counter -= intervals * countPerInterval;
            }
        }

        public virtual void Start()
        {
            started = true;
        }

        public virtual void Update()
        {
            if (!started)
            {
                Start();
            }
        }
    }


    public class TickUpdater : DotUpdater
    {
        private int lastTick;

        public TickUpdater()
            : this(Updater.normSpeed)
        {
        }

        public TickUpdater(int ticksPerCycle)
            : base(ticksPerCycle)
        {
        }

        public override void Start()
        {
            base.Start();

            lastTick = Updater.tickCount;
        }

        public override void Update()
        {
            base.Update();

            int newTick = Updater.tickCount;
            int ticksPassed = newTick - lastTick;

            lastTick = newTick;

            IncCounter(ticksPassed);
        }
    }

    public class TimeUpdater : DotUpdater
    {

        private long lastTime;

        public TimeUpdater()
            : this(750)
        {
        }

        public TimeUpdater(int millisPerCycle)
            : base(millisPerCycle)
        {
        }

        public override void Start()
        {
            base.Start();

            lastTime = JavaSystem.NanoTime();
        }

        public override void Update()
        {
            base.Update();

            long now = JavaSystem.NanoTime();
            int diffMillis = (int)((now - lastTime) / 1E6);

            lastTime = now;

            IncCounter(diffMillis);
        }
    }

    public class CallUpdater : DotUpdater
    {
        public CallUpdater(int callsPerCycle)
            : base(callsPerCycle)
        {
        }

        public override void Update()
        {
            base.Update();
            IncCounter(1);
        }
    }
}
