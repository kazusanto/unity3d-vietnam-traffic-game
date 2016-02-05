using System;

namespace Common
{
    public class Range<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }

        public Range(T min, T max) {
            Min = min;
            Max = max;
        }

        public void Init(T value) {
            Min = Max = value;
        }
    }
}
