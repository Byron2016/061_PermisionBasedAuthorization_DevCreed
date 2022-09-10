using System.Collections.Generic;

namespace PBaseWebADotNet5.Web.Constants
{
    public static class Permissions
    {
        public static List<string> GeneratePermissionList(string module)
        {
            return new List<string>
            {
                $"Permissions.{module}.View",
                $"Permissions.{module}.Create",
                $"Permissions.{module}.Edit",
                $"Permissions.{module}.Delete"
            };
        }
    }
}
