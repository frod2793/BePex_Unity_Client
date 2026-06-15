using System;
using BePex.EventSystem.Interfaces;

namespace BePex.EventSystem.Tests.PlayMode
{
    public class MockTimeProvider : ITimeProvider
    {
        public DateTime CurrentTime { get; set; } = DateTime.Now;

        public DateTime GetCurrentTime()
        {
            return CurrentTime;
        }
    }
}
