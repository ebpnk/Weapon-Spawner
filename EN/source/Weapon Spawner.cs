using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Text.Json;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;



namespace WeaponSpawnerPlugin
{
    public class WeaponSpawner : BasePlugin
    {
        private const string PluginAuthor = "DoctorishHD";
        private const string PluginName = "Weapon Spawner";
        private const string PluginVersion = "1.0";
        private const string ConfigFileName = "config.json";
        private WeaponSpawnerConfig? config;
        private HashSet<CCSPlayerController> authenticatedSessions = new HashSet<CCSPlayerController>();
        private HashSet<string> setMaps = new HashSet<string>();

        public override string ModuleAuthor => PluginAuthor;
        public override string ModuleName => PluginName;
        public override string ModuleVersion => PluginVersion;

        public override void Load(bool hotReload)
        {
            base.Load(hotReload);
            LoadConfig();
            AddCommand("css_givew", "Give weapons.", CommandSpawnWeapon);
            AddCommand("css_givew_list", "list weapons.", CommandListWeapons);
        }

        private void LoadConfig()
        {
            string configFilePath = Path.Combine(ModuleDirectory, ConfigFileName);
            if (!File.Exists(configFilePath))
            {
                var defaultConfig = new WeaponSpawnerConfig
                {
                    AdminFlag = "@css/weaponspawner"
                };

                string jsonConfig = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFilePath, jsonConfig);
                Console.WriteLine("Configuration file created with default flag.");
            }

            config = JsonSerializer.Deserialize<WeaponSpawnerConfig>(File.ReadAllText(configFilePath));
        }

        private bool IsAuthenticated(CCSPlayerController? player)
        {
            // Убедитесь, что player не null перед проверкой на аутентификацию
            return player != null && authenticatedSessions.Contains(player);
        }
        

        private void CommandSpawnWeapon(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null || config == null)
            {
                return;
            }

            if (!IsAdminWithFlag(player, config.SpawnWeaponPermissionFlag))
            {
                player.PrintToChat("You do not have permission to use this command.");
                return;
            }

            string weaponName = commandInfo.ArgCount > 1 ? commandInfo.GetArg(1) : "";
            if (string.IsNullOrEmpty(weaponName))
            {
                player?.PrintToChat("Application: css_givew <weapon_name>");
                return;
            }

            string weaponCode = ConvertWeaponNameToCode(weaponName);
            if (string.IsNullOrEmpty(weaponCode))
            {
                player?.PrintToChat($"Weapon {weaponName} not found.");
                return;
            }

            player.GiveNamedItem(weaponCode);
            Console.WriteLine($"The weapon {weaponName} has been created!");
        }

        private void CommandListWeapons(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null || config == null)
            {
                return;
            }

            if (!IsAdminWithFlag(player, config.ListWeaponsPermissionFlag))
            {
                player.PrintToChat("You do not have permission to use this command.");
                return;
            }

            string weaponList = GetWeaponList();
            player?.PrintToChat(weaponList);
        }

        private string ReplaceColorPlaceholders(string message)
        {
            if (message.Contains('{'))
            {
                string modifiedMessage = message;
                foreach (FieldInfo field in typeof(ChatColors).GetFields())
                {
                    string pattern = $"{{{field.Name}}}";
                    if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        modifiedMessage = modifiedMessage.Replace(pattern, field.GetValue(null).ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                }
                return modifiedMessage;
            }

            return message;
        }

        private string GetWeaponList()
        {
            var weapons = new List<string>
            {
                "",
                "{Red}ak47",
                "{Red}aug",
                "{Red}scar-20",
                "{Red}g3sg1",
                "{Red}m4a1",
                "{Red}m4a1-s",
                "{Red}galil",
                "{Red}sg553",
                "{Red}awp",
                "{Red}famas",
                "{Red}ssg08",
                "{Red}p90",
                "{Red}mp9",
                "{Red}mp7",
                "{Red}ump-45",
                "{Red}bizon",
                "{Red}mac-10",
                "{Red}mp5-sd",
                "{Blue}usp-s",
                "{Blue}p2000",
                "{Blue}tec-9",
                "{Blue}deagle",
                "{Blue}five-seveN",
                "{Blue}dual-berettas",
                "{Blue}p250",
                "{Blue}cz75a",
                "{Blue}glock-18",
                "{Blue}r8",
                "{Red}xm1014",
                "{Red}nova",
                "{Red}sawed-off",
                "{Red}mag-7",
                "{Red}negev",
                "{Red}m249",
                "{Red}knifegg",
                "{Red}taser",
                "{Yellow}smoke",
                "{Yellow}bumpmine",
                "{Yellow}decoy",
                "{Yellow}molotov",
                "{Yellow}hegrenade",
                "{Yellow}incgrenade",
                "{Yellow}flash",
            };


            return string.Join(", ", weapons.Select(ReplaceColorPlaceholders));
        }


        private string ConvertWeaponNameToCode(string weaponName)
        {
            switch (weaponName.ToLower())
            {
                // Винтовки
                case "ak47": return "weapon_ak47";
                case "aug": return "weapon_aug";
                case "scar-20": return "weapon_scar20";
                case "g3sg1": return "weapon_g3sg1";
                case "m4a1": return "weapon_m4a1";
                case "m4a1-s": return "weapon_m4a1_silencer";
                case "galil": return "weapon_galilar";
                case "sg553": return "weapon_sg556";
                case "awp": return "weapon_awp";
                case "famas": return "weapon_famas";
                case "ssg08": return "weapon_ssg08";
                // ПП
                case "p90": return "weapon_p90";
                case "mp9": return "weapon_mp9";
                case "mp7": return "weapon_mp7";
                case "ump-45": return "weapon_ump45";
                case "bizon": return "weapon_bizon";
                case "mac-10": return "weapon_mac10";
                case "mp5-sd": return "weapon_mp5sd";
                // Пистолеты
                case "usp-s": return "weapon_usp_silencer";
                case "p2000": return "weapon_hkp2000";
                case "tec-9": return "weapon_tec9";
                case "deagle": return "weapon_deagle";
                case "five-seveN": return "weapon_fiveseven";
                case "dual-berettas": return "weapon_elite";
                case "p250": return "weapon_p250";
                case "cz75a": return "weapon_cz75a";
                case "glock-18": return "weapon_glock";
                case "r8": return "weapon_revolver";
                // Тяжелое
                case "xm1014": return "weapon_xm1014";
                case "nova": return "weapon_nova";
                case "sawed-off": return "weapon_sawedoff";
                case "mag-7": return "weapon_mag7";
                case "negev": return "weapon_negev";
                case "m249": return "weapon_m249";
                // Ножи
                case "knifegg": return "weapon_knife_css";
                case "taser": return "weapon_taser";

                // ... Добавьте другие ножи здесь ...
                // Гранаты
                case "decoy": return "weapon_decoy";
                case "molotov": return "weapon_molotov";
                case "hegrenade": return "weapon_hegrenade";
                case "incgrenade": return "weapon_incgrenade";
                case "flash": return "weapon_flashbang";
                case "smoke": return "weapon_smokegrenade";
                // ... Добавьте другие гранаты здесь ...
                // Экипировка
                case "bumpmine": return "weapon_bumpmine";
                // ... Добавьте другую экипировку здесь ...

                default: return ""; // Если оружие не найдено
            }
        }

        private bool IsAdminWithFlag(CCSPlayerController? player, string? flag)
        {
            if (player == null || flag == null) return false;

            return AdminManager.PlayerHasPermissions(player, flag);
        }
    }
    public class WeaponSpawnerConfig
    {
        public string? AdminFlag { get; set; } = "@css/weaponspawner";
        public string? SpawnWeaponPermissionFlag { get; set; } = "@css/givew";
        public string? ListWeaponsPermissionFlag { get; set; } = "@css/listw";
    }
}
