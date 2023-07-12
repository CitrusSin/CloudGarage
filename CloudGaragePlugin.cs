using MySql.Data.MySqlClient;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace CloudGarage
{
    public class CloudGaragePlugin : RocketPlugin<CloudGarageConfig>
    {
        private static CloudGaragePlugin instance;
        public static CloudGaragePlugin Instance => instance;

        private DatabaseManager databaseManager;
        public DatabaseManager Database => databaseManager;

        public override TranslationList DefaultTranslations => new TranslationList
        {
            { "command_invalid", "命令格式错误！" },
            { "vehicle_owner_invalid", "你不是这辆车的主人！" },
            { "vehicle_store_success", "车辆 {0} (ID: {1}) 已成功停放到您的云车库中！" },
            { "vehicle_store_fail", "车辆存入失败！" },
            { "vehicle_draw_fail", "车辆取出失败！你可能没有这辆车。" },
            { "vehicle_draw_success", "车辆 {0} (ID: {1}) 已成功取出！" },
            { "vehicle_entry", "车辆 {0} (ID: {1})" },
            { "garage_empty", "你没有车！" },
            { "not_looking_at_vehicle", "请把视线移动到你想要存放的载具上！" },
            { "help_message", "/cg s <名称>    将车存入云车库\n/cg d <名称>    取出云车库的车\n/cg l           查看云车库中的车" }
        };

        private CloudGarageConfig Config;

        public CloudGaragePlugin()
        {
            instance = this;
        }

        protected override void Load()
        {
            base.Configuration.Load();
            this.Config = base.Configuration.Instance;
            databaseManager = new DatabaseManager(this.Config);
        }

        protected override void Unload()
        {
            base.Configuration.Save();
            base.Unload();
        }

        public static string StaticTranslate(string key, params object[] args)
        {
            return Instance.Translate(key, args);
        }
    }
}
