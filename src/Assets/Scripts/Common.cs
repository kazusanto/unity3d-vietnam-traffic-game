using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public class Range<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }

        public Range() {
        }

        public Range(T min, T max) {
            this.Min = min;
            this.Max = max;
        }

        public void Init(T value) {
            this.Min = this.Max = value;
        }
    }

    public class Pair<T, U>
    {
        public T First { get; set; }
        public U Second { get; set; }

        public Pair() {
        }

        public Pair(T first, U second) {
            this.First = first;
            this.Second = second;
        }
    }

    static class CommonExtensions {
        public static void AddUnique<T>(this List<T> list, T value) {
            if (list.IndexOf(value) == -1) {
                list.Add(value);
            }
        }
    }
}
