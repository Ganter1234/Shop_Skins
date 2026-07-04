[<kbd><br>🇷🇺 Russian README<br><br></kbd>](./README_RU.md)

# [Shop] Skins
Module for Shop Core which adds the ability to buy skins for characters

# Config
```json
{
    "SpawnDelay": 1.0,
    "EnablePreview": true,
    "Cameraman": {
        "name": "Cameraman",
        "price": "5000",
        "sellprice": "1000",
        "duration": "86400",
        "ModelT": "characters/models/cameraman_ported_lev/cameraman_ported_cs2.vmdl",
        "ModelCT": "characters/models/cameraman_ported_lev/cameraman_ported_cs2.vmdl"
    },
    "Deadpool": {
        "name": "Deadpool",
        "price": "5000",
        "sellprice": "1000",
        "duration": "86400",
        "ModelT": "characters/models/kolka/deadpool/deadpool.vmdl",
        "ModelCT": "characters/models/kolka/deadpool/deadpool.vmdl"
    }
}
```

# Installation
Install [Metamod:Source](https://www.sourcemm.net/downloads.php?branch=dev), [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases), and [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager/releases)  
Install the [Shop Core](https://github.com/Ganter1234/Shop-Core/releases) plugin  
Copy the contents of the archive to the /game/csgo/addons/counterstrikesharp folder  
Edit the config file and add the necessary models to your add-on via MultiAddonManager  
Restart the server or force-load the plugin using the command css_plugins load  