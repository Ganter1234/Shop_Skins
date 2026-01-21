using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json.Linq;
using ShopAPI;

namespace Shop_Skins;

public class Shop_SkinsPlugin : BasePlugin
{
    public override string ModuleName => "[SHOP] Skins";
    public override string ModuleAuthor => "Ganter1234";
    public override string ModuleVersion => "1.6";
    private IShopApi? _api;
	private readonly string CategoryName = "Skins";
    public static JObject? JsonSkins { get; set; }
    public string[,] playerModel = new string[65, 4];
	public CounterStrikeSharp.API.Modules.Timers.Timer[] previewTimer = new CounterStrikeSharp.API.Modules.Timers.Timer[65];
	public static Dictionary<CCSPlayerController, CDynamicProp> thirdPersonPool = new Dictionary<CCSPlayerController, CDynamicProp>();
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = IShopApi.Capability.Get();
        if (_api == null) throw new DllNotFoundException("Shop Core not found...");

		string Fpath = Path.Combine(ModuleDirectory,"../../configs/plugins/Shop/skins.json");
		if (!File.Exists(Fpath))
			throw new FileNotFoundException("Shop skins config not found... (configs/plugins/Shop/skins.json)");

		JsonSkins = JObject.Parse(File.ReadAllText(Fpath));

		RegisterListener<Listeners.OnTick>(UpdatePreviewCamera);
		RegisterEventHandler<EventRoundStart>(ClearPreviewCameraInfo);

		RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
        {
			manifest.AddResource("characters\\models\\ctm_sas\\ctm_sas.vmdl");
			manifest.AddResource("characters\\models\\tm_phoenix\\tm_phoenix.vmdl");
			foreach (var key in JsonSkins!.Properties())
			{
				if(JsonSkins.TryGetValue(key.Name, out var obj) && obj is JObject JsonItem)
				{
					if(JsonItem["ModelT"]!.ToString().Length > 0)
					{
						//Console.WriteLine($"[SHOP Skins] Precaching {(string)JsonItem["ModelT"]!}");
						if(JsonItem["ModelT"]!.ToString().Contains(".vmdl", StringComparison.Ordinal))
							manifest.AddResource((string)JsonItem["ModelT"]!);
						else
							Console.WriteLine("The 'ModelT' parameter must contain the path to the model with the .vmdl extension");
					}
					if(JsonItem["ModelCT"]!.ToString().Length > 0)
					{
						//Console.WriteLine($"[SHOP Skins] Precaching {(string)JsonItem["ModelCT"]!}");
						if(JsonItem["ModelCT"]!.ToString().Contains(".vmdl", StringComparison.Ordinal))
							manifest.AddResource((string)JsonItem["ModelCT"]!);
						else
							Console.WriteLine("The 'ModelCT' parameter must contain the path to the model with the .vmdl extension");
					}
				}
			}
        });

		_api!.CreateCategory(CategoryName, Localizer["DisplayName"]);

		foreach (var key in JsonSkins!.Properties())
		{
			if(JsonSkins.TryGetValue(key.Name, out var obj) && obj is JObject JsonItem)
			{
				Task.Run(async () =>
				{
					int ItemID = await _api.AddItem(key.Name, (string)JsonItem["name"]!, CategoryName, (int)JsonItem["price"]!, (int)JsonItem["sellprice"]!, (int)JsonItem["duration"]!);
					_api.SetItemCallbacks(ItemID, OnClientBuyItem, OnClientSellItem, OnClientToggleItem, null, JsonSkins.Value<bool>("EnablePreview") ? OnClientPreview : null);
				}).Wait();
			}
		}

		RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
			if(@event == null) return HookResult.Continue;

            var player = @event.Userid;
			if (player == null || !player.IsValid) return HookResult.Continue;

			if(previewTimer[player.Slot] != null)
			{
				previewTimer[player.Slot].Kill();
				previewTimer[player.Slot] = null!;
			}

			AddTimer(JsonSkins["SpawnDelay"] == null ? 0.1f : (float)JsonSkins["SpawnDelay"]!, () =>
			{
				if (!player.IsValid
					|| player.PlayerPawn == null
					|| !player.PlayerPawn.IsValid
					|| player.PlayerPawn.Value == null
					|| !player.PlayerPawn.Value.IsValid
					|| player.TeamNum < 2
					|| !IsPlayerAlive(player))
					return;

				var playerPawn = player.PlayerPawn.Value;

				if(!string.IsNullOrEmpty(playerModel[player.Slot, player.TeamNum]))
				{
					SetPlayerModel(playerPawn, playerModel[player.Slot, player.TeamNum]);
				}
			}, CounterStrikeSharp.API.Modules.Timers.TimerFlags.STOP_ON_MAPCHANGE);

            return HookResult.Continue;
        });

		RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
			if(@event.Userid == null) return HookResult.Continue;
			playerModel[@event.Userid.Slot, 2] = "";
			playerModel[@event.Userid.Slot, 3] = "";
			return HookResult.Continue;
		});
    }

    public override void Unload(bool hotReload)
    {
		if(_api == null) return;
        _api!.UnregisterCategory(CategoryName, true);
    }

	public HookResult OnClientBuyItem(CCSPlayerController player, int ItemID, string CategoryName, string UniqueName, int BuyPrice, int SellPrice, int Duration, int Count)
	{
		var slot = player.Slot;
		if(JsonSkins!.TryGetValue(UniqueName, out var obj) && obj is JObject JsonItem)
		{
			var playerPawn = player.PlayerPawn?.Value;

			if(JsonItem["ModelT"]!.ToString().Length > 0)
			{
				playerModel[slot, 2] = (string)JsonItem["ModelT"]!;
				if (player.IsValid && playerPawn != null && playerPawn.IsValid && player.TeamNum == 2 && IsPlayerAlive(player))
					SetPlayerModel(playerPawn, playerModel[slot, 2]);
			}
			if(JsonItem["ModelCT"]!.ToString().Length > 0)
			{
				playerModel[slot, 3] = (string)JsonItem["ModelCT"]!;
				if (player.IsValid && playerPawn != null && playerPawn.IsValid && player.TeamNum == 3 && IsPlayerAlive(player))
					SetPlayerModel(playerPawn, playerModel[slot, 3]);
			}
		}

		return HookResult.Continue;
	}
	public HookResult OnClientToggleItem(CCSPlayerController player, int ItemID, string UniqueName, int State)
	{
		var slot = player.Slot;
		if(State == 1)
		{
			if(JsonSkins!.TryGetValue(UniqueName, out var obj) && obj is JObject JsonItem)
			{
				var playerPawn = player.PlayerPawn?.Value;

				if(JsonItem["ModelT"]!.ToString().Length > 0)
				{
					playerModel[slot, 2] = (string)JsonItem["ModelT"]!;
					if (player.IsValid && playerPawn != null && playerPawn.IsValid && player.TeamNum == 2 && IsPlayerAlive(player))
						SetPlayerModel(playerPawn, playerModel[slot, 2]);
				}
				if(JsonItem["ModelCT"]!.ToString().Length > 0)
				{
					playerModel[slot, 3] = (string)JsonItem["ModelCT"]!;
					if (player.IsValid && playerPawn != null && playerPawn.IsValid && player.TeamNum == 3 && IsPlayerAlive(player))
						SetPlayerModel(playerPawn, playerModel[slot, 3]);
				}
			}
		}
		else
		{
			playerModel[slot, 2] = "";
			playerModel[slot, 3] = "";

			if (!player.IsValid
				|| player.PlayerPawn == null
				|| !player.PlayerPawn.IsValid
				|| player.PlayerPawn.Value == null
				|| !player.PlayerPawn.Value.IsValid
				|| player.TeamNum < 2
				|| !IsPlayerAlive(player))
				return HookResult.Continue;

			var playerPawn = player.PlayerPawn.Value;

			SetDefaultPlayerModel(playerPawn, player.TeamNum);
		}

		return HookResult.Continue;
	}
	public HookResult OnClientSellItem(CCSPlayerController player, int ItemID, string UniqueName, int SellPrice)
	{
		OnClientToggleItem(player, ItemID, UniqueName, 0);
		return HookResult.Continue;
	}
	public void SetDefaultPlayerModel(CCSPlayerPawn pawn, int team)
    {
		string model = team == 2 ? "characters\\models\\tm_phoenix\\tm_phoenix.vmdl" : "characters\\models\\ctm_sas\\ctm_sas.vmdl";
        SetPlayerModel(pawn, model);
    }
	public void SetPlayerModel(CCSPlayerPawn pawn, string model)
    {
        Server.NextFrame(() =>
        {
            pawn.SetModel(model);
        });
    }
	public static bool IsPlayerAlive(CCSPlayerController player)
	{
		var pawn = player.PlayerPawn.Value;
		if (pawn == null || !pawn.IsValid)
			return false;

		return pawn.LifeState == (byte)LifeState_t.LIFE_ALIVE;
	}

	#region PreviewMode
	/* Preview (Thirdperson code: https://github.com/UgurhanK/ThirdPerson-WIP/blob/main/ThirdPerson/ThirdPerson.cs) */
	public void OnClientPreview(CCSPlayerController player, int ItemID, string UniqueName, string CategoryName)
	{
		var slot = player.Slot;
		if (player.TeamNum < 2 || !IsPlayerAlive(player))
		{
			player.PrintToChat(StringExtensions.ReplaceColorTags(Localizer.ForPlayer(player, "Preview_Alive")));
			return;
		}

		if(previewTimer[slot] != null)
		{
			player.PrintToChat(StringExtensions.ReplaceColorTags(Localizer.ForPlayer(player, "Preview_Wait")));
			return;
		}

		var playerPawn = player.PlayerPawn?.Value;
		string oldModel = playerPawn!.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
		if(JsonSkins!.TryGetValue(UniqueName, out var obj) && obj is JObject JsonItem)
		{
			if(JsonItem["ModelT"]!.ToString().Length > 0)
			{
				if (playerPawn != null && playerPawn.IsValid && player.TeamNum == 2)
				{
					SetPlayerModel(playerPawn, (string)JsonItem["ModelT"]!);
				}
			}
			if(JsonItem["ModelCT"]!.ToString().Length > 0)
			{
				if (playerPawn != null && playerPawn.IsValid && player.TeamNum == 3)
				{
					SetPlayerModel(playerPawn, (string)JsonItem["ModelCT"]!);
				}
			}
		}

		DefaultThirdPerson(player);
		previewTimer[slot] = AddTimer(5.0f, () =>
        {
			previewTimer[slot] = null!;
			if (!player.IsValid
				|| player.PlayerPawn == null
				|| !player.PlayerPawn.IsValid
				|| player.PlayerPawn.Value == null
				|| !player.PlayerPawn.Value.IsValid
				|| player.TeamNum < 2
				|| !IsPlayerAlive(player))
				return;

			DefaultThirdPerson(player);
			player.PrintToChat(StringExtensions.ReplaceColorTags(Localizer.ForPlayer(player, "Preview_End")));
			if (playerPawn != null && playerPawn.IsValid)
				SetPlayerModel(playerPawn, oldModel);
        });
	}

	public void DefaultThirdPerson(CCSPlayerController caller)
	{
		if (!thirdPersonPool.ContainsKey(caller))
		{
			CDynamicProp? _cameraProp = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

			if (_cameraProp == null) return;

			_cameraProp.DispatchSpawn();
			_cameraProp.Render = Color.FromArgb(0, 255, 255, 255);
            Utilities.SetStateChanged(_cameraProp, "CBaseModelEntity", "m_clrRender");
			_cameraProp.Teleport(CalculatePositionInFront(caller, 120, 90), caller.PlayerPawn.Value!.EyeAngles, null);

			caller.PlayerPawn!.Value!.CameraServices!.ViewEntity.Raw = _cameraProp.EntityHandle.Raw;
			Utilities.SetStateChanged(caller.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices");
			thirdPersonPool.Add(caller, _cameraProp);

			AddTimer(0.5f, () =>
			{
				_cameraProp.Teleport(CalculatePositionInFront(caller, 120, 80), caller.PlayerPawn.Value.V_angle, null);
			});

		}
		else
		{
			caller!.PlayerPawn!.Value!.CameraServices!.ViewEntity.Raw = uint.MaxValue;
			AddTimer(0.3f, () => Utilities.SetStateChanged(caller.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices"));
			if (thirdPersonPool[caller] != null && thirdPersonPool[caller].IsValid) thirdPersonPool[caller].Remove();
			thirdPersonPool.Remove(caller);
		}
	}

	public Vector CalculatePositionInFront(CCSPlayerController player, float offSetXY, float offSetZ = 0)
    {
        var pawn = player.PlayerPawn.Value;
        // Extract yaw angle from player's rotation QAngle
        float yawAngle = pawn!.EyeAngles!.Y;

        // Convert yaw angle from degrees to radians
        float yawAngleRadians = (float)(yawAngle * Math.PI / 180.0);

        // Calculate offsets in x and y directions
        float offsetX = offSetXY * (float)Math.Cos(yawAngleRadians);
        float offsetY = offSetXY * (float)Math.Sin(yawAngleRadians);

        // Calculate position in front of the player
        var positionInFront = new Vector
        {
            X = pawn!.AbsOrigin!.X + offsetX,
            Y = pawn!.AbsOrigin!.Y + offsetY,
            Z = pawn!.AbsOrigin!.Z + offSetZ
        };

        return positionInFront;
    }

	public void UpdatePreviewCamera()
	{
		foreach (var player in thirdPersonPool.Keys)
		{
			thirdPersonPool[player].Teleport(CalculatePositionInFront(player, -110, 90), player.PlayerPawn.Value!.EyeAngles, null);
		}
	}

	public HookResult ClearPreviewCameraInfo(EventRoundStart @event, GameEventInfo info)
	{
		thirdPersonPool.Clear();
		return HookResult.Continue;
	}
	#endregion
}
