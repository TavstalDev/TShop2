using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavstal.TShop.Compability;
using UnityEngine;

namespace Tavstal.TShop.Helpers
{
    public static class MathHelper
    {
        private static System.Random _random;
        private static object syncObj = new object();

        public static Vector3 GetCenterOfVectors(Vector3[] vectors)
        {
            Vector3 sum = Vector3.zero;
            if (vectors == null || vectors.Length == 0)
            {
                return sum;
            }

            foreach (Vector3 vec in vectors)
            {
                sum += vec;
            }
            return sum / vectors.Length;
        }

        public static Vector3 GetCenterOfVectors(List<Vector3> vectors)
        {
            Vector3 sum = Vector3.zero;
            if (vectors == null || vectors.Count == 0)
            {
                return sum;
            }

            foreach (Vector3 vec in vectors)
            {
                sum += vec;
            }
            return sum / vectors.Count;
        }

        public static int Next(int min, int max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new System.Random(); // Or exception...
                return _random.Next(min, max);
            }
        }

        public static int Next(int max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new System.Random(); // Or exception...
                return _random.Next(0, max);
            }
        }

        public static int Clamp(int value, int minValue, int maxValue)
        {
            return maxValue < value ? maxValue : (value < minValue ? minValue : value);
        }

        public static double Next(double min, double max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new System.Random(); // Or exception...
                return (_random.NextDouble() * Math.Abs(max - min)) + min;
            }
        }

        public static double Next(double max)
        {
            lock (syncObj)
            {
                if (_random == null)
                    _random = new System.Random(); // Or exception...
                return (_random.NextDouble() * Math.Abs(max));
            }
        }

        public static double Clamp(double value, double minValue, double maxValue)
        {
            return maxValue < value ? maxValue : (value < minValue ? minValue : value);
        }
    }
}
