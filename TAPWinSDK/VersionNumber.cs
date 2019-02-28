using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPWin
{
    class VersionNumber
    {
        private VersionNumber()
        {

        }

        internal static int FromString(string str)
        {
            var major = 0;
            var minor = 0;
            var build = 0;

            string[] components = str.Split('.');
            if (components.Length == 3) {
                if (int.TryParse(components[0],out major) && int.TryParse(components[1],out minor) && int.TryParse(components[2], out build))
                {
                    return major * 10000 + minor * 100 + build;
                }
            }
            return 0;
        }
    }
}
