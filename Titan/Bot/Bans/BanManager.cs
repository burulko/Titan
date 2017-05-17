﻿using System;
using System.IO;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;
using Titan.UI;

namespace Titan.Bot.Bans
{
    public class BanManager
    {

        private Logger _log = LogCreator.Create();

        private FileInfo _apiKeyFile;

        public string APIKey;

        public BanManager()
        {
            _apiKeyFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, "steamapi.key"));
        }

        public void ParseApiKeyFile()
        {
            if(!_apiKeyFile.Exists)
            {
                Titan.Instance.UIManager.ShowForm(UIType.APIKeyInput);

                File.WriteAllText(_apiKeyFile.ToString(), APIKey);
            }

            using(var reader = File.OpenText(_apiKeyFile.ToString()))
            {
                APIKey = reader.ReadLine();
            }

            _log.Debug("Using Steam API key: {Key}", APIKey);
        }

        public BanInfo GetBanInfoFor(SteamID steamId)
        {
            using(dynamic steamUser = WebAPI.GetInterface("ISteamUser", APIKey))
            {
                KeyValue pair = steamUser.GetPlayerBans(steamids: steamId.ConvertToUInt64());

                foreach(var get in pair["players"].Children)
                {
                    if(get["SteamId"].AsUnsignedLong() == steamId.ConvertToUInt64())
                    {
                        return new BanInfo
                        {
                            SteamId = get["SteamId"].AsUnsignedLong(),
                            CommunityBanned = get["CommunityBanned"].AsBoolean(),
                            VacBanned = get["VACBanned"].AsBoolean(),
                            VacBanCount = get["NumberOfVACBans"].AsInteger(),
                            DaysSinceLastBan = get["DaysSinceLastBan"].AsInteger(),
                            GameBanCount = get["NumberOfGameBans"].AsInteger(),
                            EconomyBan = get["EconomyBan"].AsString()
                        };
                    }
                }
            }

            _log.Warning("Did not receive ban informations for {SteamID}. Skipping...");
            return null;
        }

    }
}