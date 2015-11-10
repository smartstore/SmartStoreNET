using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Globalization;
using System.IO;

namespace SmartStore.Utilities
{
    public static class TypeHelper
    {
        public delegate T Creator<T>();

        public static Type GetElementType(Type type)
        {
            if (!type.IsPredefinedSimpleType())
            {
                if (type.HasElementType)
                {
                    return GetElementType(type.GetElementType());
                }
                if (type.IsPredefinedGenericType())
                {
                    return GetElementType(type.GetGenericArguments()[0]);
                }
                Type type2 = type.FindIEnumerable();
                if (type2 != null)
                {
                    Type type3 = type2.GetGenericArguments()[0];
                    return GetElementType(type3);
                }
            }

            return type;
        }

    }
}