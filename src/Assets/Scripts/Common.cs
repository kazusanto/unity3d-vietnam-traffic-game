using System;
using System.Collections;
using System.Collections.Generic;

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

    static class Extensions {
        public static void AddUnique<T>(this List<T> list, T value) {
            if (list.IndexOf(value) == -1) {
                list.Add(value);
            }
        }
    }
}
