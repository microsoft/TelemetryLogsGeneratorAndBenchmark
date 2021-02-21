using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BenchmarkLogGenerator
{
    public class Scheduler
    {
        private PriorityQueue<Step> m_queue;

        public DateTime Now { get; private set; }

        public Scheduler(DateTime now)
        {
            m_queue = new PriorityQueue<Step>(1024);
            Now = now;
        }

        public void Run()
        {
            while (m_queue.Count != 0)
            {
                var step = m_queue.Dequeue();
                if (step.DueTime > Now)
                {
                    Now = step.DueTime;
                }
                step.Execute(this);
            }
        }

        public Event ScheduleNewFlow(IEnumerable<Step> steps)
        {
            var completionEvent = NewEvent();
            var flow = new Flow(steps.GetEnumerator(), completionEvent);
            var step = new FlowDelayStep(DateTimeUtil.Zero, flow);
            Enqueue(step);
            return completionEvent;
        }

        public Step DelayFlow(TimeSpan duration)
        {
            return new FlowDelayStep(DateTimeUtil.Add(Now, duration));
        }

        public Event NewEvent()
        {
            return new Event(this);
        }

        private void Enqueue(Step step)
        {
            if (step == null)
            {
                return;
            }

            m_queue.Enqueue(step);
        }

        internal sealed class Flow
        {
            private IEnumerator<Step> m_steps;
            private Event m_completionEvent;

            internal Flow(IEnumerator<Step> steps, Event completionEvent)
            {
                m_steps = steps;
                m_completionEvent = completionEvent;
            }

            public Step NextStep()
            {
                if (m_steps.MoveNext())
                {
                    var step = m_steps.Current;
                    step.Flow = this;
                    return step;
                }
                else
                {
                    m_completionEvent.Signal();
                    return null;
                }
            }
        }

        public abstract class Step : IComparable<Step>
        {
            public DateTime DueTime { get; private set; }
            internal Flow Flow { get; set; }

            public Step(DateTime dueTime)
            {
                DueTime = dueTime;
            }

            public abstract void Execute(Scheduler scheduler);

            public int CompareTo(Step other)
            {
                return DueTime.CompareTo(other.DueTime);
            }

            protected void EnqueueNextStep(Scheduler scheduler)
            {
                if (Flow == null)
                {
                    return;
                }

                var step = Flow.NextStep();
                if (step == null)
                {
                    return;
                }

                // TODO: fast path (avoid enqueue)

                scheduler.Enqueue(step);
            }
        }

        internal sealed class FlowDelayStep : Step
        {
            public FlowDelayStep(DateTime dueTime) : base(dueTime)
            {
            }

            public FlowDelayStep(DateTime dueTime, Flow flow) : base(dueTime)
            {
                Flow = flow;
            }

            public override void Execute(Scheduler scheduler)
            {
                EnqueueNextStep(scheduler);
            }
        }

        internal sealed class BlockOnEventStep : Step
        {
            private Event m_event;

            public BlockOnEventStep(Event evt) : base(DateTimeUtil.Zero)
            {
                m_event = evt;
            }

            public override void Execute(Scheduler scheduler)
            {
                if (m_event.IsSet)
                {
                    EnqueueNextStep(scheduler);
                }
                else
                {
                    m_event.AddFlow(Flow);
                }
            }
        }

        public class Event
        {
            public bool IsSet { get; private set; }

            private Scheduler m_scheduler;
            private List<Flow> m_blockedFlows;
            private List<Event> m_chainedEvents;

            public Event(Scheduler scheduler)
            {
                m_scheduler = scheduler;
                m_blockedFlows = null;
                m_chainedEvents = null;
            }

            public Step Wait()
            {
                return new BlockOnEventStep(this);
            }

            public static Event WhenAny(params Event[] events)
            {
                var scheduler = events[0].m_scheduler;
                var whenAny = new Event(scheduler);
                foreach (var evt in events)
                {
                    evt.AddChained(whenAny);
                }
                return whenAny;
            }

            public static Event WhenAll(params Event[] events)
            {
                var scheduler = events[0].m_scheduler;
                var whenAll = new WhenAllEvent(scheduler, events.Length);
                foreach (var evt in events)
                {
                    evt.AddChained(whenAll);
                }
                return whenAll;
            }

            internal void AddFlow(Flow flow)
            {
                if (IsSet)
                {
                    m_scheduler.Enqueue(flow.NextStep());
                }
                else
                {
                    AddBlockedFlow(flow);
                }
            }

            public void AddChained(Event evt)
            {
                if (IsSet)
                {
                    evt.Signal();
                }
                else
                {
                    AddChainedEvent(evt);
                }
            }

            public virtual void Signal()
            {
                if (!IsSet)
                {
                    TransitionToSet();
                }
            }

            protected void TransitionToSet()
            {
                IsSet = true;

                if (m_chainedEvents != null)
                {
                    foreach (var evt in m_chainedEvents)
                    {
                        evt.Signal();
                    }
                    m_chainedEvents = null;
                }

                if (m_blockedFlows != null)
                {
                    foreach (var flow in m_blockedFlows)
                    {
                        m_scheduler.Enqueue(flow.NextStep());
                    }
                    m_blockedFlows = null;
                }
            }

            private void AddBlockedFlow(Flow flow)
            {
                if (m_blockedFlows == null)
                {
                    m_blockedFlows = new List<Flow>();
                }
                m_blockedFlows.Add(flow);
            }

            private void AddChainedEvent(Event evt)
            {
                if (m_chainedEvents == null)
                {
                    m_chainedEvents = new List<Event>();
                }
                m_chainedEvents.Add(evt);
            }
        }

        public sealed class WhenAllEvent : Event
        {
            private int m_count;

            public WhenAllEvent(Scheduler scheduler, int count) : base(scheduler)
            {
                m_count = count;
            }

            public override void Signal()
            {
                if (!IsSet)
                {
                    Debug.Assert(m_count != 0);
                    m_count -= 1;
                    if (m_count == 0)
                    {
                        TransitionToSet();
                    }
                }
            }
        }
    }

    public static class DateTimeUtil
    {
        public static DateTime Zero
        {
            get
            {
                return new DateTime(0, DateTimeKind.Utc);
            }
        }

        public static DateTime Add(DateTime dt, TimeSpan ts)
        {
            return DateTime.SpecifyKind(dt.Add(ts), DateTimeKind.Utc);
        }
    }
}
