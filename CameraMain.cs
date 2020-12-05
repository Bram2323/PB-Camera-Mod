using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using HarmonyLib;
using Poly.Math;
using BepInEx;
using BepInEx.Configuration;
using PolyTechFramework;

namespace CameraMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVerson)]
    [BepInProcess("Poly Bridge 2")]
    [BepInDependency(PolyTechMain.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class CameraMain : PolyTechMod
    {

        public const string pluginGuid = "polytech.cameramod";

        public const string pluginName = "Camera Mod";

        public const string pluginVerson = "1.1.1";

        public ConfigDefinition modEnableDef = new ConfigDefinition(pluginName, "Enable/Disable Mod");
        public ConfigDefinition PosAtStartDef = new ConfigDefinition(pluginName, "Change Position At Start");
        public ConfigDefinition PosAtStopDef = new ConfigDefinition(pluginName, "Change Position At Stop");
        public ConfigDefinition RotateEverywhereDef = new ConfigDefinition(pluginName, "Rotate Everywhere");
        public ConfigDefinition ToggleRotateDef = new ConfigDefinition(pluginName, "Toggle Rotate Everywhere");
        public ConfigDefinition FollowPosDef = new ConfigDefinition(pluginName, "Follow Position");
        public ConfigDefinition FollowRotDef = new ConfigDefinition(pluginName, "Follow Rotation");
        public ConfigDefinition BackgroundDef = new ConfigDefinition(pluginName, "Background");
        public ConfigDefinition FirstPersonDef = new ConfigDefinition(pluginName, "First Person");
        public ConfigDefinition AutoOffsetDef = new ConfigDefinition(pluginName, "Auto Offset");
        public ConfigDefinition OffsetDef = new ConfigDefinition(pluginName, "Offset");
        public ConfigDefinition ChangeTargetDef = new ConfigDefinition(pluginName, "Change Target");

        public ConfigEntry<bool> mEnabled;

        public ConfigEntry<bool> mPosAtStart;
        public ConfigEntry<bool> mPosAtStop;

        public ConfigEntry<bool> mRotateEverywhere;
        public ConfigEntry<KeyboardShortcut> mToggleRotate;
        public bool ToggleRotatePressed = false;

        public ConfigEntry<bool> mFollowPos;
        public ConfigEntry<bool> mFollowRot;

        public ConfigEntry<bool> mBackground;

        public ConfigEntry<bool> mFirstPerson;

        public ConfigEntry<bool> mAutoOffset;

        public ConfigEntry<Vector2> mOffset;

        public ConfigEntry<KeyboardShortcut> mChangeTarget;


        public bool backgroundEnabled;

        public bool ReCalcCamPos;


        public List<Vehicle> vehicles = new List<Vehicle>();

        public List<CustomShape> shapes = new List<CustomShape>();

        public int CamIndex = 0;
        public int LastCamIndex = 0;
        public Vehicle CamTarget;
        public bool IsPressed = false;

        public GameObject FollowCamObj;
        public GameObject ReplayFollowCamObj;
        public GameObject FollowCamObjMain;
        public Camera FollowCam;
        public Camera ReplayFollowCam;

        public Camera ReplayCam;

        public bool InSim = false;

        public static CameraMain instance;

        void Awake()
        {
            if (instance == null) instance = this;

            FollowCamObjMain = Instantiate(new GameObject("FollowCameraMain"));
            FollowCamObj = Instantiate(new GameObject("FollowCamera"), FollowCamObjMain.transform);
            FollowCam = FollowCamObj.AddComponent<Camera>();
            ReplayFollowCamObj = Instantiate(new GameObject("ReplayFollowCamera"), FollowCamObj.transform);
            ReplayFollowCam = ReplayFollowCamObj.AddComponent<Camera>();

            setActivateFollowCam(false);

            DontDestroyOnLoad(FollowCamObjMain);

            int order = 0;

            Config.Bind(modEnableDef, true, new ConfigDescription("Controls if the mod should be enabled or disabled", null, new ConfigurationManagerAttributes { Order = order }));
            mEnabled = (ConfigEntry<bool>)Config[modEnableDef];
            mEnabled.SettingChanged += onEnableDisable;
            order--;

            Config.Bind(PosAtStartDef, true, new ConfigDescription("Change the position of the camera at the start of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            mPosAtStart = (ConfigEntry<bool>)Config[PosAtStartDef];
            order--;

            Config.Bind(PosAtStopDef, true, new ConfigDescription("Change the position of the camera at the end of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            mPosAtStop = (ConfigEntry<bool>)Config[PosAtStopDef];
            order--;

            Config.Bind(RotateEverywhereDef, false, new ConfigDescription("Controls if you can rotate the camera in build mode", null, new ConfigurationManagerAttributes { Order = order }));
            mRotateEverywhere = (ConfigEntry<bool>)Config[RotateEverywhereDef];
            order--;

            mToggleRotate = Config.Bind(ToggleRotateDef, new KeyboardShortcut(KeyCode.None), new ConfigDescription("What button toggles the rotate everywhere setting", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            Config.Bind(FollowPosDef, true, new ConfigDescription("Follow the position of the target", null, new ConfigurationManagerAttributes { Order = order }));
            mFollowPos = (ConfigEntry<bool>)Config[FollowPosDef];
            order--;

            Config.Bind(FollowRotDef, false, new ConfigDescription("Follow the rotation of the target", null, new ConfigurationManagerAttributes { Order = order }));
            mFollowRot = (ConfigEntry<bool>)Config[FollowRotDef];
            order--;

            Config.Bind(BackgroundDef, true, new ConfigDescription("Enable/Disable the gradient background", null, new ConfigurationManagerAttributes { Order = order }));
            mBackground = (ConfigEntry<bool>)Config[BackgroundDef];
            order--;

            Config.Bind(FirstPersonDef, false, new ConfigDescription("A first person view of the target", null, new ConfigurationManagerAttributes { Order = order }));
            mFirstPerson = (ConfigEntry<bool>)Config[FirstPersonDef];
            order--;

            Config.Bind(AutoOffsetDef, true, new ConfigDescription("Automaticaly add a offset based on the target vehicle", null, new ConfigurationManagerAttributes { Order = order }));
            mAutoOffset = (ConfigEntry<bool>)Config[AutoOffsetDef];
            order--;

            Config.Bind(OffsetDef, new Vector2(0, 0), new ConfigDescription("A offset thats added to the 0 0 point off the car", null, new ConfigurationManagerAttributes { Order = order }));
            mOffset = (ConfigEntry<Vector2>)Config[OffsetDef];
            order--;

            mChangeTarget = Config.Bind(ChangeTargetDef, new KeyboardShortcut(KeyCode.Tab), new ConfigDescription("What button changes the camera target", null, new ConfigurationManagerAttributes { Order = order }));
            order--;


            Config.SettingChanged += onSettingChanged;
            onSettingChanged(null, null);

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            isCheat = false;
            isEnabled = mEnabled.Value;

            PolyTechMain.registerMod(this);
            
        }

        public void onEnableDisable(object sender, EventArgs e)
        {
            isEnabled = mEnabled.Value;
            if (isEnabled)
            {
                if (GameStateManager.GetState() == GameState.SIM)
                {
                    onStopSim();
                    onStartSim();
                }
            }
            else
            {
                CameraControl control = CameraControl.instance;
                if (control == null) return;
                Camera cam = control.cam;
                if (cam == null) return;
                GameObject backGround = cam.gameObject.transform.GetChild(0).gameObject;

                setActivateFollowCam(false);
                backGround.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        public void onSettingChanged(object sender, EventArgs e)
        {
            isEnabled = mEnabled.Value;
            backgroundEnabled = mBackground.Value && !mFirstPerson.Value;
            ReCalcCamPos = mFollowPos.Value || mFollowRot.Value || mFirstPerson.Value;

            if (this.isEnabled && GameStateManager.GetState() == GameState.SIM)
            {
                onStopSim();
                onStartSim();
            }

            CameraControl control = CameraControl.instance;
            if (control == null) return;
            Camera cam = control.cam;
            if (cam == null) return;
            GameObject backGround = cam.gameObject.transform.GetChild(0).gameObject;

            if (GameStateManager.GetState() != GameState.SIM) backGround.GetComponent<MeshRenderer>().enabled = true;
        }

        public override void enableMod()
        {
            this.isEnabled = true;
            mEnabled.Value = true;
            onEnableDisable(null, null);
        }

        public override void disableMod()
        {
            this.isEnabled = false;
            mEnabled.Value = false;
            onEnableDisable(null, null);
        }

        public override string getSettings()
        {
            return "";
        }

        public override void setSettings(string st)
        {
            return;
        }

        private bool CheckForCheating()
        {
            return mEnabled.Value && PolyTechMain.modEnabled.Value;
        }

        [HarmonyPatch(typeof(Main), "Update")]
        private static class patchUpdate
        {
            private static void Postfix()
            {
                if (!instance.CheckForCheating() || GameStateManager.GetState() != GameState.SIM || !instance.InSim) return;

                CameraControl control = CameraControl.instance;
                Camera cam = control.cam;
                GameObject backGround = cam.gameObject.transform.GetChild(0).gameObject;

                instance.vehicles = Vehicles.m_Vehicles;

                if (!instance.IsPressed && instance.mChangeTarget.Value.IsDown())
                {
                    instance.CamIndex++;
                    if (instance.CamIndex > instance.vehicles.Count) instance.CamIndex = 0;
                    
                    Vehicle target = null;
                    if (instance.CamIndex != 0)
                    {
                        target = instance.vehicles[instance.CamIndex - 1];
                        instance.setActivateFollowCam(instance.mFirstPerson.Value);
                        backGround.GetComponent<MeshRenderer>().enabled = instance.backgroundEnabled;
                    }
                    else
                    {
                        instance.setActivateFollowCam(false);
                        backGround.GetComponent<MeshRenderer>().enabled = instance.mBackground.Value;
                    }
                    instance.CamTarget = target;

                    if (target != null)
                    {
                        instance.ChangeTarget(target);
                    }
                }
                instance.IsPressed = instance.mChangeTarget.Value.IsDown();

                if (instance.CamIndex > instance.vehicles.Count) instance.CamIndex = 0;

                if (instance.ReCalcCamPos) control.RotMouse(Vec2.zero);

                if (instance.CamTarget != null)
                {
                    if (instance.mFollowRot.Value)
                    {
                        cam.transform.eulerAngles = new Vector3(0, 0, instance.CamTarget.m_MeshRenderer.transform.eulerAngles.z);
                        cam.transform.position = new Vector3(instance.CamTarget.m_MeshRenderer.transform.position.x, instance.CamTarget.m_MeshRenderer.transform.position.y, -20);
                    }
                }
            }
        }

        public void onStartSim()
        {
            CameraControl control = CameraControl.instance;
            Camera cam = control.cam;
            GameObject backGround = cam.gameObject.transform.GetChild(0).gameObject;
            vehicles = Vehicles.m_Vehicles;
            shapes = CustomShapes.m_Shapes;

            CamIndex = LastCamIndex;
            if (CamIndex > vehicles.Count) CamIndex = 0;
            Vehicle target = null;
            if (CamIndex != 0) target = vehicles[CamIndex - 1];
            if (target != null) ChangeTarget(target);

            backGround.GetComponent<MeshRenderer>().enabled = backgroundEnabled;
            if (mBackground.Value && mFirstPerson.Value && CamIndex == 0) backGround.GetComponent<MeshRenderer>().enabled = true;
            if (CamIndex != 0) setActivateFollowCam(mFirstPerson.Value);

            InSim = true;
        }

        public void onStopSim()
        {
            InSim = false;

            CameraControl control = CameraControl.instance;
            Camera cam = control.cam;
            GameObject backGround = cam.gameObject.transform.GetChild(0).gameObject;

            LastCamIndex = CamIndex;
            CamIndex = 0;
            backGround.GetComponent<MeshRenderer>().enabled = mBackground.Value;
            setActivateFollowCam(false);
        }

        [HarmonyPatch(typeof(GameStateSim), "StartSimulation")]
        private static class patchStartSim
        {
            private static void Postfix()
            {
                if (!instance.CheckForCheating() || GameStateManager.GetState() != GameState.SIM) return;
                instance.onStartSim();
            }
        }

        [HarmonyPatch(typeof(GameStateManager), "EnterState")]
        private static class patchStopSim
        {
            private static void Prefix(GameState newState, GameState prevState)
            {
                if (!instance.CheckForCheating()) return;
                if (prevState == GameState.SIM && newState != GameState.SIM) instance.onStopSim();
            }
        }

        [HarmonyPatch(typeof(CameraControl), "RecalcCamPosition")]
        private static class patchCameraControl
        {
            private static void Prefix(ref Vec2 ___anchorPos, ref Vec2 ___offsetFromAnchor)
            {
                if (!instance.CheckForCheating() || GameStateManager.GetState() != GameState.SIM || !(instance.mFollowPos.Value || instance.mFirstPerson.Value) || instance.CamTarget == null) return;
                ___anchorPos = instance.CamTarget.m_MeshRenderer.transform.position;
                ___offsetFromAnchor = new Vec2(0, 0);
            }
        }
        
        [HarmonyPatch(typeof(CameraRotate), "Update")]
        private static class patchCameraUpdate
        {
            private static bool Prefix(CameraRotate __instance)
            {
                if (!instance.CheckForCheating()) return true;

                if (instance.mToggleRotate.Value.IsDown() && !instance.ToggleRotatePressed)
                {
                    instance.mRotateEverywhere.Value = !instance.mRotateEverywhere.Value;
                    GameUI.m_Instance.m_TopBar.m_MessageTopCenter.ShowMessage("Rotate Everywhere " + (instance.mRotateEverywhere.Value ? "enabled" : "disabled"), 2);
                }
                instance.ToggleRotatePressed = instance.mToggleRotate.Value.IsDown();

                if (!instance.mRotateEverywhere.Value) return true;

                if (GameInput.IsDown(BindingType.ROTATE_SIM_CAMERA) && !GameInput.IsDown(BindingType.DRAW_BUILD_PAN) && !GameInput.IsDown(BindingType.PAN_WITH_MOUSE))
                {
                    float num = Input.GetAxis("Mouse X") * __instance.m_SensitivityX * Profile.m_CameraRotateSpeedNormalized;
                    float num2 = Input.GetAxis("Mouse Y") * -__instance.m_SensitivityY * Profile.m_CameraRotateSpeedNormalized;
                    if (CameraControl.instance && CameraControl.instance.isSimActive)
                    {
                        CameraControl.instance.RotMouse(new Vec2(num, num2));
                        return false;
                    }
                    __instance.transform.RotateAround(PointsOfView.m_Pivot, Vector3.up, num);
                    float angle = Mathf.Min(90f, num2);
                    __instance.transform.RotateAround(PointsOfView.m_Pivot, Cameras.MainCamera().transform.right, angle);
                    if (__instance.transform.forward.y > 0f)
                    {
                        float angle2 = Vector3.Angle(Cameras.MainCamera().transform.forward, new Vector3(Cameras.MainCamera().transform.forward.x, 0f, Cameras.MainCamera().transform.forward.z));
                        Cameras.MainCamera().transform.RotateAround(PointsOfView.m_Pivot, Cameras.MainCamera().transform.right, angle2);
                    }
                    if (Vector3.Dot(Cameras.MainCamera().transform.up, Vector3.up) < 0f)
                    {
                        float num3 = Vector3.Angle(Vector3.down, Cameras.MainCamera().transform.forward);
                        Cameras.MainCamera().transform.RotateAround(PointsOfView.m_Pivot, Cameras.MainCamera().transform.right, -num3);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(PointsOfView), "RotateTo")]
        private static class patchCameraInterpolate
        {
            private static bool Prefix(PointOfViewType type)
            {
                if (!instance.CheckForCheating() || type == PointOfViewType.BUILD) return true;

                if (GameStateManager.GetState() == GameState.SIM && !instance.mPosAtStart.Value) return false;
                else if (GameStateManager.GetState() != GameState.SIM && !instance.mPosAtStop.Value) return false;

                return true;
            }
        }

        /*
        [HarmonyPatch(typeof(CinemaCamera), "RestoreStart")]
        private static class patchRestoreStart
        {
            private static bool Prefix()
            {
                if (!CheckForCheating()) return true;

                if (GameStateManager.GetState() == GameState.SIM && !mPosAtStart.Value) return false;
                else if (GameStateManager.GetState() != GameState.SIM && !mPosAtStop.Value) return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(CinemaCamera), "RestoreEnd")]
        private static class patchRestoreEnd
        {
            private static bool Prefix()
            {
                if (!CheckForCheating()) return true;

                if (GameStateManager.GetState() == GameState.SIM && !mPosAtStart.Value) return false;
                else if (GameStateManager.GetState() != GameState.SIM && !mPosAtStop.Value) return false;

                return true;
            }
        }
        */

        [HarmonyPatch(typeof(AsyncCapture), "OnPostRender")]
        private static class patchCapture
        {
            private static bool Prefix(AsyncCapture __instance, Queue<AsyncGPUReadbackRequest> ___m_Requests, float ___m_LastSnapshotTime)
            {
                if (!instance.CheckForCheating()) return true;

                if (!__instance.m_IsRecording)
                {
                    return false;
                }
                if (Mathf.Approximately(Time.timeScale, 0f))
                {
                    return false;
                }
                __instance.m_ElapsedTime += Time.unscaledDeltaTime;
                float num = 1f / (float)__instance.framerate;
                if (__instance.m_ElapsedTime > num)
                {
                    if (___m_Requests.Count < 8)
                    {
                        ___m_Requests.Enqueue(AsyncGPUReadback.Request(Cameras.ReplayCamera().targetTexture, 0, TextureFormat.RGB24, null));
                    }
                    else
                    {
                        Debug.LogWarningFormat("Too many requests.", Array.Empty<object>());
                    }
                    ___m_LastSnapshotTime = Time.realtimeSinceStartup;
                    __instance.m_ElapsedTime -= num;
                }

                return false;
            }
        }


        public void setActivateFollowCam(bool Active)
        {
            if (Active)
            {
                ReplayFollowCam.enabled = Active;
                FollowCam.enabled = Active;
                if (Cameras.m_Instance != null)
                {
                    ReplayCam = Cameras.ReplayCamera();
                    ReplayFollowCam.targetTexture = ReplayCam.targetTexture;
                    //Cameras.m_Instance.m_Replay = ReplayFollowCam;
                }
            }
            else
            {
                FollowCam.transform.SetParent(FollowCamObjMain.transform, false);
                if (Cameras.m_Instance != null)
                {
                    //Cameras.m_Instance.m_Replay = ReplayCam;
                    //ReplayFollowCam.targetTexture = null;
                }
                ReplayFollowCam.enabled = Active;
                FollowCam.enabled = Active;
            }
        }



        public void ChangeTarget(Vehicle target)
        {
            FollowCam.transform.SetParent(target.m_MeshRenderer.transform, false);

            Vector3 Rot = target.m_MeshRenderer.transform.eulerAngles;

            Vector2 VehicleOffset = GetOffset(target);
            float S = mOffset.Value.x + VehicleOffset.x;
            float T = mOffset.Value.y + VehicleOffset.y;

            if (target.Physics.isFlipped) Rot += new Vector3(180, 0, 180);
            FollowCam.transform.eulerAngles = new Vector3(-Rot.z, 90f, Rot.x);

            float A = Rot.z * (Mathf.PI / 180);
            if (target.Physics.isFlipped) A *= -1;
            Vector2 Offset = new Vector2();
            Offset.x = Mathf.Cos(A) * S - Mathf.Sin(A) * T;
            Offset.y = Mathf.Cos(A) * T + Mathf.Sin(A) * S;

            if (target.Physics.isFlipped) Offset.y *= -1;

            FollowCam.transform.position = target.m_MeshRenderer.transform.position + new Vector3(Offset.x, Offset.y, 0);
        }



        /*
        public void SetSkyboxColor(Color bottom, Color top)
        {
            var side = new Texture2D(2, 1, TextureFormat.ARGB32, false);
            side.SetPixel(0, 0, bottom);
            side.SetPixel(1, 0, top);
            side.Apply();

            var up = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            up.SetPixel(0, 0, top);
            up.Apply();

            var down = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            down.SetPixel(0, 0, bottom);
            down.Apply();

            SkyboxManifest manifest = new SkyboxManifest(side, side, side, side, up, down);
            Material material = CreateSkyboxMaterial(manifest);
            SetSkybox(material);
        }

        public static Material CreateSkyboxMaterial(SkyboxManifest manifest)
        {
            Material result = new Material(Shader.Find("Skybox/Cubemap"));
            result.SetTexture("_FrontTex", manifest.textures[0]);
            result.SetTexture("_BackTex", manifest.textures[1]);
            result.SetTexture("_LeftTex", manifest.textures[2]);
            result.SetTexture("_RightTex", manifest.textures[3]);
            result.SetTexture("_UpTex", manifest.textures[4]);
            result.SetTexture("_DownTex", manifest.textures[5]);
            return result;
        }

        void SetSkybox(Material material)
        {
            GameObject camera = FollowCam.gameObject;
            Skybox skybox = camera.GetComponent<Skybox>();
            if (skybox == null)
                skybox = camera.AddComponent<Skybox>();
            skybox.material = material;
        }

        public struct SkyboxManifest
        {
            public Texture2D[] textures;

            public SkyboxManifest(Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D up, Texture2D down)
            {
                textures = new Texture2D[6]
                {
                     front,
                     back,
                     left,
                     right,
                     up,
                     down
                };
            }
        }
        */



        public Vector2 GetOffset(Vehicle vehicle)
        {
            if (vehicle.m_DisplayNameLocKey == "VEHICLE_TRUCK_WITH_CONTAINER" || vehicle.m_DisplayNameLocKey == "VEHICLE_TRUCK_WITH_LIQUID" || vehicle.m_DisplayNameLocKey == "VEHICLE_TRUCK" || vehicle.m_DisplayNameLocKey == "VEHICLE_TRUCK_WITH_FLATBED") return new Vector2(0.75f, 1.25f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_SCHOOL_BUS") return new Vector2(0.75f, 1.3f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_VESPA") return new Vector2(-0.25f, 0.81f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_CHOPPER") return new Vector2(-0.47f, 1);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_DUNE_BUGGY") return new Vector2(0.13f, 0.75f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_COMPACT_CAR") return new Vector2(-0.1f, 0.7f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_SPORTS_CAR") return new Vector2(0.1f, 0.6f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_TAXI") return new Vector2(0, 0.9f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_MODELT") return new Vector2(0, 1.1f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_PICKUP_TRUCK") return new Vector2(0.62f, 1.2f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_LIMO") return new Vector2(0.8f, 0.85f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_VAN") return new Vector2(0.9f, 1.1f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_TOWTRUCK") return new Vector2(0.45f, 1.16f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_STEAMPUNK_HOTROD") return new Vector2(-0.29f, 1.2f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_MONSTER_TRUCK") return new Vector2(0.04f, 2.07f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_AMBULANCE") return new Vector2(0.17f, 1.2f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_FIRE_TRUCK") return new Vector2(1, 1.3f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_BULLDOZER") return new Vector2(-0.2f, 1.8f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_DUMP_TRUCK") return new Vector2(1.6f, 1.9f);
            else if (vehicle.m_DisplayNameLocKey == "VEHICLE_ARTICULATED_BUS") return new Vector2(2.4f, 1.5f);

            return new Vector2(0, 0);
        }
    }


    /// <summary>
    /// Class that specifies how a setting should be displayed inside the ConfigurationManager settings window.
    /// 
    /// Usage:
    /// This class template has to be copied inside the plugin's project and referenced by its code directly.
    /// make a new instance, assign any fields that you want to override, and pass it as a tag for your setting.
    /// 
    /// If a field is null (default), it will be ignored and won't change how the setting is displayed.
    /// If a field is non-null (you assigned a value to it), it will override default behavior.
    /// </summary>
    /// 
    /// <example> 
    /// Here's an example of overriding order of settings and marking one of the settings as advanced:
    /// <code>
    /// // Override IsAdvanced and Order
    /// Config.AddSetting("X", "1", 1, new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
    /// // Override only Order, IsAdvanced stays as the default value assigned by ConfigManager
    /// Config.AddSetting("X", "2", 2, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
    /// Config.AddSetting("X", "3", 3, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
    /// </code>
    /// </example>
    /// 
    /// <remarks> 
    /// You can read more and see examples in the readme at https://github.com/BepInEx/BepInEx.ConfigurationManager
    /// You can optionally remove fields that you won't use from this class, it's the same as leaving them null.
    /// </remarks>
#pragma warning disable 0169, 0414, 0649
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>
        /// Should the setting be shown as a percentage (only use with value range settings).
        /// </summary>
        public bool? ShowRangeAsPercent;

        /// <summary>
        /// Custom setting editor (OnGUI code that replaces the default editor provided by ConfigurationManager).
        /// See below for a deeper explanation. Using a custom drawer will cause many of the other fields to do nothing.
        /// </summary>
        public System.Action<BepInEx.Configuration.ConfigEntryBase> CustomDrawer;

        /// <summary>
        /// Show this setting in the settings screen at all? If false, don't show.
        /// </summary>
        public bool? Browsable;

        /// <summary>
        /// Category the setting is under. Null to be directly under the plugin.
        /// </summary>
        public string Category;

        /// <summary>
        /// If set, a "Default" button will be shown next to the setting to allow resetting to default.
        /// </summary>
        public object DefaultValue;

        /// <summary>
        /// Force the "Reset" button to not be displayed, even if a valid DefaultValue is available. 
        /// </summary>
        public bool? HideDefaultButton;

        /// <summary>
        /// Force the setting name to not be displayed. Should only be used with a <see cref="CustomDrawer"/> to get more space.
        /// Can be used together with <see cref="HideDefaultButton"/> to gain even more space.
        /// </summary>
        public bool? HideSettingName;

        /// <summary>
        /// Optional description shown when hovering over the setting.
        /// Not recommended, provide the description when creating the setting instead.
        /// </summary>
        public string Description;

        /// <summary>
        /// Name of the setting.
        /// </summary>
        public string DispName;

        /// <summary>
        /// Order of the setting on the settings list relative to other settings in a category.
        /// 0 by default, higher number is higher on the list.
        /// </summary>
        public int? Order;

        /// <summary>
        /// Only show the value, don't allow editing it.
        /// </summary>
        public bool? ReadOnly;

        /// <summary>
        /// If true, don't show the setting by default. User has to turn on showing advanced settings or search for it.
        /// </summary>
        public bool? IsAdvanced;

        /// <summary>
        /// Custom converter from setting type to string for the built-in editor textboxes.
        /// </summary>
        public System.Func<object, string> ObjToStr;

        /// <summary>
        /// Custom converter from string to setting type for the built-in editor textboxes.
        /// </summary>
        public System.Func<string, object> StrToObj;
    }
}
