using System;
using System.Text.RegularExpressions;

namespace RaspberryIRBlaster.Common
{
    public static class Validators
    {
        /// <summary>
        /// Both the remote and button names just so happen to share the same rules.
        /// </summary>
        private static readonly Regex _remoteAndButtonNameValidator = new Regex(@"^[a-z0-9_\-]{1,30}$", RegexOptions.IgnoreCase);

        public static bool ValidateRemoteName(string name) => _remoteAndButtonNameValidator.IsMatch(name);

        public static bool ValidateButtonName(string name) => _remoteAndButtonNameValidator.IsMatch(name);
    }
}
