using CgiResourceUpload.Interfaces;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace CgiResourceUpload.Helpers
{
    public class RegistryHelper
    {
        private const string RegKey = "SOFTWARE\\SharpCloud\\API\\SC.CgiResourceUpload";

        private readonly ILogger _logger;

        public RegistryHelper(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<string> RegRead(string keyName, string defVal)
        {
            // Open a subKey as read-only
            var key = Registry.CurrentUser.OpenSubKey(RegKey);

            try
            {
                // Get registry key value, or null if it does not exist
                if (key != null)
                {
                    var ret = (string)key.GetValue(keyName);
                    if (ret == null)
                    {
                        return defVal;
                    }
                    return ret;
                }
            }
            catch (Exception e)
            {
                await _logger.LogError(e);
            }

            return defVal;
        }

        public async Task<bool> RegWrite(string keyName, object newValue)
        {
            try
            {
                // Use CreateSubKey (create or open it if already exits):
                // OpenSubKey opens a subKey as read-only
                var key = Registry.CurrentUser.CreateSubKey(RegKey);
                
                key.SetValue(keyName, newValue);
                return true;
            }
            catch (Exception e)
            {
                await _logger.LogError(e);
                return false;
            }
        }
    }
}
