﻿using SharpDX;

namespace Rml.SharpDx
{
    /// <summary>
    /// 
    /// </summary>
    public static class QuaternionHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aFrom"></param>
        /// <param name="aTo"></param>
        /// <returns></returns>
        public static Quaternion FromToRotation(Vector3 aFrom, Vector3 aTo)
        {
            var axis = Vector3.Cross(aFrom, aTo);
            var angle = Vector3Helper.Angle(aFrom, aTo);
            return Quaternion.RotationAxis(Vector3.Normalize(axis), angle);
        }
    }
}