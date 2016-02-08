using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public static class UnitConst
    {
        public const float size = 2.0f;
        public const float harf = size / 2.0f;
    }

    public struct Unit
    {
        public int x { get; set; }
        public int y { get; set; }

        public Unit(int ux, int uy) {
            x = ux; y = uy; 
        }

        public Vector2 toVector2() {
            return Unit.vector2(x, y);
        }

        public Vector3 toVector3() {
            return Unit.vector3(x, y);
        }

        public static Vector2 vector2(int ux, int uy) {
            return new Vector2(ux * UnitConst.size, uy * UnitConst.size);
        }

        public static Vector3 vector3(int ux, int uy) {
            return new Vector3(ux * UnitConst.size, 0.0f, uy * UnitConst.size);
        }

        public static Unit operator+ (Unit lhs, Unit rhs) {
            return new Unit(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public static Unit operator- (Unit lhs, Unit rhs) {
            return new Unit(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        public static Unit operator* (Unit lhs, int operand) {
            return new Unit(lhs.x * operand, lhs.y * operand);
        }

        public static Unit operator/ (Unit lhs, int operand) {
            return new Unit(lhs.x / operand, lhs.y / operand);
        }
    }

    public static class UnitExtensions
    {
        public static Vector2 unit(this Vector2 vec, int ux, int uy) {
            vec.x = ux * UnitConst.size;
            vec.y = uy * UnitConst.size;
            return vec;
        }

        public static Vector3 unit(this Vector3 vec, int ux, int uy) {
            vec.x = ux * UnitConst.size;
            vec.y = 0.0f;
            vec.z = uy * UnitConst.size;
            return vec;
        }

        public static Unit toUnit(this Vector2 vec) {
            return new Unit((int)((vec.x + UnitConst.harf) / UnitConst.size), (int)((vec.y + UnitConst.harf) / UnitConst.size));
        }

        public static Unit toUnit(this Vector3 vec) {
            return new Unit((int)((vec.x + UnitConst.harf) / UnitConst.size), (int)((vec.z + UnitConst.harf) / UnitConst.size));
        }
    }
}
