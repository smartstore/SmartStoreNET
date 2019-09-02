using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmartStore.Core.Security
{
    public static class PermissionHelper
    {
        /// <summary>
        /// Gets a list of all permission system names for a given type.
        /// See <see cref="Permissions"/> as an example.
        /// </summary>
        /// <returns>Permission system names.</returns>
        public static IList<string> GetPermissions(Type type)
        {
            Guard.NotNull(type, nameof(type));

            var result = new List<string>();
            GetPermissionsPerType(type);

            return result;

            void GetPermissionsPerType(Type permissionType)
            {
                var permissions = permissionType
                    .GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                    .Select(x => (string)x.GetRawConstantValue());

                result.AddRange(permissions);

                var nestedTypes = permissionType.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
                nestedTypes.Each(x => GetPermissionsPerType(x));
            }
        }
    }
}
