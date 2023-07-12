using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace CloudGarage
{
    public class CloudGarageCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "cloudgarage";

        public string Help => "Garage in the sky";

        public string Syntax => "/cloudgarage <store/draw/list> [vehicle name]";

        public List<string> Aliases => new List<string>{ "cg" };

        public List<string> Permissions => new List<string> { "cloudgarage" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer) caller;

            if (command.Length < 1)
            {
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("command_invalid"), Color.red);
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("help_message"), Color.yellow);
                return;
            }
            string subCommand = command[0];
            if (subCommand.ToLower() == "store" || subCommand.ToLower() == "s")
            {
                if (command.Length < 2 || string.IsNullOrEmpty(command[1]))
                {
                    UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("command_invalid"), Color.red);
                    return;
                }
                StoreVehicle(player, command[1]);
            }
            else if (subCommand.ToLower() == "draw" || subCommand.ToLower() == "d")
            {
                if (command.Length < 2 || string.IsNullOrEmpty(command[1]))
                {
                    UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("command_invalid"), Color.red);
                    return;
                }
                DrawVehicle(player, command[1]);
            }
            else if (subCommand.ToLower() == "list" || subCommand.ToLower() == "l")
            {
                ListVehicle(player);
            }
            else
            {
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("command_invalid"), Color.red);
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("help_message"), Color.yellow);
                return;
            }
        }

        private void ListVehicle(UnturnedPlayer player)
        {
            Dictionary<string, VehicleInfo> vehicles = CloudGaragePlugin.Instance.Database.ListVehicle(player.CSteamID);
            foreach (string name in vehicles.Keys)
            {
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("vehicle_entry", name, vehicles[name].Id), Color.yellow);
            }
            if (vehicles.Count == 0)
            {
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("garage_empty"), Color.yellow);
            }
        }

        private void StoreVehicle(UnturnedPlayer player, string vehicleName)
        {
            Ray ray = new Ray(player.Player.look.aim.position, player.Player.look.aim.forward);
            bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo, 10f, RayMasks.VEHICLE);
            if (isHit)
            {
                InteractableVehicle vehicle = hitInfo.transform.GetComponent<InteractableVehicle>();
                // Check ownship
                if (vehicle.isLocked && vehicle.lockedOwner != player.CSteamID)
                {
                    UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("vehicle_owner_invalid"), Color.red);
                    return;
                }
                VehicleInfo vehicleInfo = new VehicleInfo
                {
                    Id = vehicle.id,
                    Health = vehicle.health,
                    Fuel = vehicle.fuel,
                    BatteryCharge = vehicle.batteryCharge,
                    IsLocked = vehicle.isLocked,
                    OwnerPlayer = player.CSteamID,
                    OwnerGroup = vehicle.isLocked ? vehicle.lockedOwner : CSteamID.Nil
                };
                bool successful = CloudGaragePlugin.Instance.Database.StoreVehicle(player.CSteamID, vehicleName, vehicleInfo);
                if (successful)
                {
                    VehicleManager.askVehicleDestroy(vehicle);
                    UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("vehicle_store_success", vehicleName, vehicleInfo.Id), Color.yellow);
                }
                else
                {
                    UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("vehicle_store_fail"), Color.yellow);
                }
            }
            else
            {
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("not_looking_at_vehicle"), Color.red);
            }
        }

        private void DrawVehicle(UnturnedPlayer player, string vehicleName)
        {
            Ray ray = new Ray(player.Player.look.aim.position, player.Player.look.aim.forward);
            bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo, 10f, RayMasks.GROUND);
            Vector3 spawnPoint = player.Position + 3f * Vector3.up + 8f * player.Player.look.aim.forward.GetHorizontal();
            if (isHit)
            {
                spawnPoint = hitInfo.point + 4f * Vector3.up;
            }
            bool success = CloudGaragePlugin.Instance.Database.DrawVehicle(player.CSteamID, vehicleName, out VehicleInfo vehicleInfo);
            if (!success)
            {
                UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("vehicle_draw_fail"), Color.red);
                return;
            }
            var vehicle = VehicleManager.spawnVehicleV2(vehicleInfo.Id, spawnPoint, Quaternion.Euler(0, 0, 0));
            VehicleManager.sendVehicleBatteryCharge(vehicle, vehicleInfo.BatteryCharge);
            VehicleManager.sendVehicleFuel(vehicle, vehicleInfo.Fuel);
            VehicleManager.sendVehicleHealth(vehicle, vehicleInfo.Health);
            VehicleManager.ServerSetVehicleLock(vehicle, vehicleInfo.OwnerPlayer, vehicleInfo.OwnerGroup, vehicleInfo.IsLocked);
            UnturnedChat.Say(player, CloudGaragePlugin.StaticTranslate("vehicle_draw_success", vehicleName, vehicleInfo.Id), Color.yellow);
        }
    }
}
