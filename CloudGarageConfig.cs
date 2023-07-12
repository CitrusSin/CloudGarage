using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudGarage
{
    public class CloudGarageConfig : IRocketPluginConfiguration, IDefaultable
    {
        public string SQLAddress;
        public uint SQLPort;
        public string SQLUsername;
        public string SQLPassword;

        public string DatabaseName;
        public string DatabaseTableName;

        public void LoadDefaults()
        {
            this.SQLAddress = "127.0.0.1";
            this.SQLPort = 3306;
            this.SQLUsername = "root";
            this.SQLPassword = "";

            this.DatabaseName = "unturned";
            this.DatabaseTableName = "cloudgarage";
        }
    }
}
