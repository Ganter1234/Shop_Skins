[<kbd><br>en English README<br><br></kbd>](./README.md)

# [Shop] Skins
Модуль для покупки скинов на персонажа

# Конфиг
```json
{
    "SpawnDelay": 1.0,
    "EnablePreview": true,
    "Cameraman": {
        "name": "Камера мен",
        "price": "5000",
        "sellprice": "1000",
        "duration": "86400",
        "ModelT": "characters/models/cameraman_ported_lev/cameraman_ported_cs2.vmdl",
        "ModelCT": "characters/models/cameraman_ported_lev/cameraman_ported_cs2.vmdl"
    },
    "Deadpool": {
        "name": "Дедпул",
        "price": "5000",
        "sellprice": "1000",
        "duration": "86400",
        "ModelT": "characters/models/kolka/deadpool/deadpool.vmdl",
        "ModelCT": "characters/models/kolka/deadpool/deadpool.vmdl"
    }
}
```

# Установка
Установить [Metamod:Source](https://www.sourcemm.net/downloads.php?branch=dev), [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases) и [MultiAddonManager](https://github.com/Source2ZE/MultiAddonManager/releases)
Установить плагин [Shop Core](https://github.com/Ganter1234/Shop-Core/releases)
Скопировать содержимое архива в папку /game/csgo/addons/counterstrikesharp
Отредактировать конфиг и добавить необходимые модели в ваш аддон через MultiAddonManager
Перезапустить сервер или принудительно загрузить плагин с помощью команды css_plugins load