using System;
using System.Text.RegularExpressions;

namespace RaspberryIRBlaster.Common
{
    public static partial class Validators
    {
        /// <summary>
        /// Both the remote and button names just so happen to share the same rules.
        /// </summary>
        [GeneratedRegex(@"^[a-z0-9_\-]{1,30}$", RegexOptions.IgnoreCase)]
        private static partial Regex RemoteAndButtonNameValidator();

        public static bool ValidateRemoteName(string name) => RemoteAndButtonNameValidator().IsMatch(name);

        public static bool ValidateButtonName(string name) => RemoteAndButtonNameValidator().IsMatch(name);
    }
}
