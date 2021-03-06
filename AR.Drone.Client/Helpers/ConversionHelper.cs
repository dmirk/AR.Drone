﻿using System;

namespace AR.Drone.Client.Helpers
{
    public class ConversionHelper
    {
        public static int ToInt(float value)
        {
            int result = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
            return result;
        }

        public static float ToSingle(uint value)
        {
            float result = BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
            return result;
        }
    }
}