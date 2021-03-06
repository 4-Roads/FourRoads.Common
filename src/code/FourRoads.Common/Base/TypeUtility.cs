// //------------------------------------------------------------------------------
// // <copyright company="4 Roads Ltd">
// //     Copyright (c) 4 Roads Ltd.  All rights reserved.
// // </copyright>
// //------------------------------------------------------------------------------

#region

using System;

#endregion

namespace FourRoads.Common
{
    public class TypeUtility
    {
        public static bool MayBeOfType<T>(Type objectType)
        {
            if (objectType == typeof(T))
                return true;
            if (objectType.BaseType == typeof(T))
                return true;
            if (objectType.GetNestedType(typeof(T).Name) != null)
                return true;
            if (objectType.GetInterface(typeof(T).Name) != null)
                return true;
            return false;
        }

        public static object CreateObject(string type, object[] args)
        {
            try
            {
                //Get the type
                var objectType = Type.GetType(type);
                if (objectType != null)
                {
                    return Activator.CreateInstance(objectType, args);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load object of type '" + type + "' : " + ex.Message, ex);
            }
            throw new Exception("Could not find object of type '" + type + "'");
        }

        public static object CreateObject(string type)
        {
            try
            {
                //Get the type
                var objectType = Type.GetType(type);
                if (objectType != null)
                    return Activator.CreateInstance(objectType);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load object of type '" + type + "' : " + ex.Message, ex);
            }
            throw new Exception("Could not find object of type '" + type + "'");
        }

        public static T CreateObject<T>() where T : class
        {
            try
            {
                //Get the type
                var objectType = typeof(T);
                if (objectType != null)
                    return Activator.CreateInstance(objectType) as T;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load object of type '" + typeof(T).Name + "' : " + ex.Message, ex);
            }
            throw new Exception("Could not find object of type '" + typeof(T).Name + "'");
        }

        public static T CreateObject<T>(params object[] args) where T : class
        {
            try
            {
                //Get the type
                var objectType = typeof(T);
                if (objectType != null)
                {
                    return Activator.CreateInstance(objectType, args) as T;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load object of type '" + typeof(T).Name + "' : " + ex.Message, ex);
            }
            throw new Exception("Could not find object of type '" + typeof(T).Name + "'");
        }
    }
}