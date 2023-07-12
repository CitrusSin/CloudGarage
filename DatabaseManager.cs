using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Rocket.Unturned.Serialisation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudGarage
{
    public class DatabaseManager
    {
        private CloudGarageConfig config;

        public DatabaseManager(CloudGarageConfig config)
        {
            this.config = config;
            InitializeDatabaseTable();
        }

        /*
         * Specialized string.Format, preventing SQL Injection
         */
        private static string SqlSafeFormat(string sqlCmd, params object[] args)
        {
            string[] values = new string[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                string val = args[i].ToString();
                values[i] = val
                    .Replace("\"", "")
                    .Replace("'", "")
                    .Replace("`", "");
            }
            return string.Format(sqlCmd, values);
        }

        private MySqlConnection CreateConnection()
        {
            MySqlConnection conn = null;
            try
            {
                MySqlConnectionStringBuilder scsb = new MySqlConnectionStringBuilder
                {
                    Server = config.SQLAddress,
                    Port = config.SQLPort,
                    UserID = config.SQLUsername,
                    Password = config.SQLPassword,
                    Database = config.DatabaseName,
                    CharacterSet = "utf8"
                };
                if (scsb.Port == 0)
                {
                    scsb.Port = 3306;
                }
                conn = new MySqlConnection(scsb.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
            return conn;
        }

        private void InitializeDatabaseTable()
        {
            MySqlConnection con = CreateConnection();
            MySqlCommand cmd = con.CreateCommand();

            con.Open();

            using (con)
            {
                cmd.CommandText = SqlSafeFormat("show databases like '{0}';", config.DatabaseName);
                object databaseResult = cmd.ExecuteScalar();
                if (databaseResult == null)
                {
                    cmd.CommandText = SqlSafeFormat("create database `{0}`;", config.DatabaseName);
                    int affected = cmd.ExecuteNonQuery();
                    if (affected <= 0)
                    {
                        throw new ApplicationException("Failed to create database");
                    }
                }

                cmd.CommandText = SqlSafeFormat("use `{0}`;", config.DatabaseName);
                cmd.ExecuteNonQuery();
                cmd.CommandText = SqlSafeFormat(
                    "create table if not exists `{0}` (" +
                    "steamId bigint unsigned not null," +
                    "vehicleName varchar(64) not null," +
                    "vehicleId smallint unsigned not null," +
                    "vehicleHealth smallint unsigned default 100 not null," +
                    "vehicleFuel smallint unsigned default 100 not null," +
                    "vehicleBatteryCharge smallint unsigned default 100 not null," +
                    "isLocked boolean default 0 not null," +
                    "groupLockId bigint unsigned default 0 not null" +
                    ");",
                    config.DatabaseTableName
                );
                cmd.ExecuteNonQuery();
            }
        }

        public bool StoreVehicle(CSteamID steamID, string vehicleName, VehicleInfo vehicleInfo)
        {
            try
            {
                MySqlConnection con = CreateConnection();
                MySqlCommand cmdQuery = con.CreateCommand();
                cmdQuery.CommandText = SqlSafeFormat(
                    "select * from `{0}` where steamId={1} and vehicleName=\"{2}\";",
                    config.DatabaseTableName,
                    steamID.ToString(),
                    vehicleName
                );
                MySqlCommand cmdInsert = con.CreateCommand();
                cmdInsert.CommandText = SqlSafeFormat(
                    "insert into `{0}` " +
                    "(steamId, vehicleName, vehicleId, vehicleHealth, vehicleFuel, vehicleBatteryCharge, isLocked, groupLockId) " +
                    "values " +
                    "(\"{1}\", \"{2}\", {3}, {4}, {5}, {6}, {7}, {8});",
                    config.DatabaseTableName,
                    steamID.ToString(),
                    vehicleName,
                    vehicleInfo.Id,
                    vehicleInfo.Health,
                    vehicleInfo.Fuel,
                    vehicleInfo.BatteryCharge,
                    vehicleInfo.IsLocked ? 1 : 0,
                    vehicleInfo.OwnerGroup.ToString()
                );
                con.Open();

                using (con)
                {
                    object obj = cmdQuery.ExecuteScalar();
                    if (obj != null)
                    {
                        return false;
                    }
                    int affected = cmdInsert.ExecuteNonQuery();
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }

        public bool DrawVehicle(CSteamID steamID, string vehicleName, out VehicleInfo vehicleInfo)
        {
            return SearchVehicle(steamID, vehicleName, out vehicleInfo, true);
        }

        public VehicleInfo? FindVehicle(CSteamID steamID, string vehicleName)
        {
            if (SearchVehicle(steamID, vehicleName, out VehicleInfo vehicle, false))
            {
                return vehicle;
            }
            return null;
        }

        private bool SearchVehicle(CSteamID steamID, string vehicleName, out VehicleInfo vehicle, bool withdraw)
        {
            vehicle = new VehicleInfo();
            try
            {
                MySqlConnection con = CreateConnection();
                MySqlCommand cmdQuery = con.CreateCommand();
                cmdQuery.CommandText = SqlSafeFormat(
                    "select * from `{0}` where steamId={1} and vehicleName=\"{2}\";",
                    config.DatabaseTableName,
                    steamID.ToString(),
                    vehicleName
                );
                MySqlCommand cmdRemove = con.CreateCommand();
                cmdRemove.CommandText = SqlSafeFormat(
                    "delete from `{0}` where steamId={1} and vehicleName=\"{2}\";",
                    config.DatabaseTableName,
                    steamID.ToString(),
                    vehicleName
                );

                con.Open();
                using (con)
                {
                    MySqlDataReader rdr = cmdQuery.ExecuteReader();
                    bool successful = rdr.Read();
                    if (!successful)
                    {
                        rdr.Close();
                        return false;
                    }
                    vehicle.Id = rdr.GetUInt16("vehicleId");
                    vehicle.Health = rdr.GetUInt16("vehicleHealth");
                    vehicle.Fuel = rdr.GetUInt16("vehicleFuel");
                    vehicle.BatteryCharge = rdr.GetUInt16("vehicleBatteryCharge");
                    vehicle.IsLocked = rdr.GetBoolean("isLocked");
                    vehicle.OwnerGroup = new CSteamID(rdr.GetUInt64("groupLockId"));
                    vehicle.OwnerPlayer = steamID;
                    rdr.Close();
                    if (withdraw)
                    {
                        int affected = cmdRemove.ExecuteNonQuery();
                        return affected > 0;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                vehicle.Id = 0;
                return false;
            }
        }

        public Dictionary<string, VehicleInfo> ListVehicle(CSteamID steamID)
        {
            try
            {
                Dictionary<string, VehicleInfo> vehicles = new Dictionary<string, VehicleInfo>();

                MySqlConnection con = CreateConnection();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = SqlSafeFormat(
                    "select * from `{0}` where steamId={1};",
                    config.DatabaseTableName,
                    steamID.ToString()
                );

                con.Open();
                using (con)
                {
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        VehicleInfo vehicle = new VehicleInfo
                        {
                            Id = rdr.GetUInt16("vehicleId"),
                            Health = rdr.GetUInt16("vehicleHealth"),
                            Fuel = rdr.GetUInt16("vehicleFuel"),
                            BatteryCharge = rdr.GetUInt16("vehicleBatteryCharge"),
                            IsLocked = rdr.GetBoolean("isLocked"),
                            OwnerGroup = new CSteamID(rdr.GetUInt64("groupLockId")),
                            OwnerPlayer = steamID
                        };
                        vehicles.Add(rdr.GetString("vehicleName"), vehicle);
                    }
                    rdr.Close();
                    return vehicles;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }
    }

    public struct VehicleInfo
    {
        public ushort Id;
        public ushort Health;
        public ushort Fuel;
        public ushort BatteryCharge;
        public bool IsLocked;
        public CSteamID OwnerPlayer;
        public CSteamID OwnerGroup;
    }
}
