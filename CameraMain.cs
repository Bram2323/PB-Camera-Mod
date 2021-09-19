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
using System.ComponentModel;
using System.Linq;
using System.IO;

namespace CameraMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVerson)]
    [BepInProcess("Poly Bridge 2")]
    [BepInDependency(PolyTechMain.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class CameraMain : PolyTechMod
    {
        public const string pluginGuid = "polytech.cameramod";

        public const string pluginName = "Camera Mod";

        public const string pluginVerson = "1.3.1";

        public ConfigDefinition modEnableDef = new ConfigDefinition(pluginName, "Enable/Disable Mod");
        public ConfigDefinition PosAtStartDef = new ConfigDefinition(pluginName, "Change Position At Start");
        public ConfigDefinition RotAtStartDef = new ConfigDefinition(pluginName, "Change Rotation At Start");
        public ConfigDefinition SizeAtStartDef = new ConfigDefinition(pluginName, "Change Size At Start");
        public ConfigDefinition PosAtStopDef = new ConfigDefinition(pluginName, "Change Position At Stop");
        public ConfigDefinition RotAtStopDef = new ConfigDefinition(pluginName, "Change Rotation At Stop");
        public ConfigDefinition SizeAtStopDef = new ConfigDefinition(pluginName, "Change Size At Stop");
        public ConfigDefinition RotateEverywhereDef = new ConfigDefinition(pluginName, "Rotate Everywhere");
        public ConfigDefinition ToggleRotateDef = new ConfigDefinition(pluginName, "Toggle Rotate Everywhere");
        public ConfigDefinition CamPosBoundsDef = new ConfigDefinition(pluginName, "Position Boundaries");
        public ConfigDefinition CamRotBoundsDef = new ConfigDefinition(pluginName, "Rotation Boundaries");
        public ConfigDefinition RecenterDef = new ConfigDefinition(pluginName, "Recenter Button");
        public ConfigDefinition GridDef = new ConfigDefinition(pluginName, "Grid Everywhere");
        public ConfigDefinition PivotDef = new ConfigDefinition(pluginName, "Visualize Pivot");
        public ConfigDefinition CamPosDef = new ConfigDefinition(pluginName, "Camera Position");
        public ConfigDefinition CamRotDef = new ConfigDefinition(pluginName, "Camera Rotation");
        public ConfigDefinition CamSizeDef = new ConfigDefinition(pluginName, "Camera Size");
        public ConfigDefinition FollowPosDef = new ConfigDefinition(pluginName, "Follow Position");
        public ConfigDefinition FollowRotDef = new ConfigDefinition(pluginName, "Follow Rotation");
        public ConfigDefinition BackgroundDef = new ConfigDefinition(pluginName, "Background");
        public ConfigDefinition BackgroundColorDef = new ConfigDefinition(pluginName, "Background Color");
        public ConfigDefinition ThemeDef = new ConfigDefinition(pluginName, "Theme");
        public ConfigDefinition MainMenuWorldDef = new ConfigDefinition(pluginName, "Main Menu world");
        public ConfigDefinition CustomMainMenuDef = new ConfigDefinition(pluginName, "Custom Main Menu");
        public ConfigDefinition FirstPersonDef = new ConfigDefinition(pluginName, "First Person");
        public ConfigDefinition ThirdPersonDef = new ConfigDefinition(pluginName, "Third Person");
        public ConfigDefinition AutoOffsetDef = new ConfigDefinition(pluginName, "Auto Offset");
        public ConfigDefinition PosOffsetDef = new ConfigDefinition(pluginName, "Position Offset");
        public ConfigDefinition RotOffsetDef = new ConfigDefinition(pluginName, "Rotation Offset");
        public ConfigDefinition ChangeTargetDef = new ConfigDefinition(pluginName, "Change Target");
        public ConfigDefinition ChangePerspectiveDef = new ConfigDefinition(pluginName, "Change Projection");
        public ConfigDefinition FieldOfViewDef = new ConfigDefinition(pluginName, "Field of View");

        public ConfigEntry<bool> mEnabled;

        public ConfigEntry<bool> mPosAtStart;
        public ConfigEntry<bool> mRotAtStart;
        public ConfigEntry<bool> mSizeAtStart;

        public ConfigEntry<bool> mPosAtStop;
        public ConfigEntry<bool> mRotAtStop;
        public ConfigEntry<bool> mSizeAtStop;

        public Vector3 cashedPivot = Vector3.zero;
        public bool allowRotate = false;

        public ConfigEntry<bool> mRotateEverywhere;
        public ConfigEntry<KeyboardShortcut> mToggleRotate;
        public bool ToggleRotatePressed = false;

        public ConfigEntry<bool> mCamPosBounds;
        public ConfigEntry<bool> mCamRotBounds;

        public ConfigEntry<bool> mRecenter;
        public ConfigEntry<bool> mGrid;
        public ConfigEntry<bool> mPivot;
        public Vec2 cashedAnchor = Vec2.zero;

        public ConfigEntry<Vector3> mCamPos;
        public ConfigEntry<Vector3> mCamRot;
        public ConfigEntry<float> mCamSize;

        public ConfigEntry<bool> mFollowPos;
        public ConfigEntry<bool> mFollowRot;

        public ConfigEntry<bool> mBackground;
        public ConfigEntry<string> mBackgroundColor;

        public ConfigEntry<Themes> mTheme;
        public bool themeEnabled = false;

        public ConfigEntry<MenuWorlds> mMainMenuWorld;

        public ConfigEntry<bool> mCustomMainMenu;

        public ConfigEntry<bool> mFirstPerson;
        public ConfigEntry<KeyboardShortcut> mThirdPerson;
        bool thirdPerson = false;

        public ConfigEntry<bool> mAutoOffset;

        public ConfigEntry<Vector3> mPosOffset;
        public ConfigEntry<Vector3> mRotOffset;

        public ConfigEntry<KeyboardShortcut> mChangeTarget;

        public ConfigEntry<KeyboardShortcut> mChangePerspective;

        public ConfigEntry<float> mFieldOfView;


        public List<Vehicle> vehicles = new List<Vehicle>();

        public List<CustomShape> shapes = new List<CustomShape>();

        public int CamIndex = 0;
        public int LastCamIndex = 0;
        public Vehicle CamTarget;

        public GameObject FollowCamObjMain;
        public GameObject FollowCamObj;
        public GameObject ReplayFollowCamObj;
        public GameObject PerspectiveCamObj;
        public GameObject ReplayPerspectiveCamObj;
        public Camera FollowCam;
        public Camera ReplayFollowCam;
        public Camera PerspectiveCam;
        public Camera ReplayPerspectiveCam;

        public Camera ReplayCam;

        public bool InSim = false;

        public GameObject PivotObj;

        public static CameraMain instance;

        public string MainPath = "";

        public bool loaded = false;

        void Awake()
        {
            if (instance == null) instance = this;
            authors = new string[] { "Bram2323" };

            MainPath = Application.dataPath.Replace("Poly Bridge 2_Data", "BepInEx/plugins/CameraMod/");

            FollowCamObjMain = Instantiate(new GameObject("FollowCameraMain"));
            FollowCamObj = Instantiate(new GameObject("FollowCamera"), FollowCamObjMain.transform);
            FollowCam = FollowCamObj.AddComponent<Camera>();
            ReplayFollowCamObj = Instantiate(new GameObject("ReplayFollowCamera"), FollowCamObj.transform);
            ReplayFollowCam = ReplayFollowCamObj.AddComponent<Camera>();
            PerspectiveCamObj = Instantiate(new GameObject("PerspectiveCamera"), FollowCamObjMain.transform);
            PerspectiveCam = PerspectiveCamObj.AddComponent<Camera>();
            ReplayPerspectiveCamObj = Instantiate(new GameObject("ReplayPerspectiveCamera"), PerspectiveCamObj.transform);
            ReplayPerspectiveCam = ReplayPerspectiveCamObj.AddComponent<Camera>();
            DontDestroyOnLoad(FollowCamObjMain);

            SetActiveFollowCam(false);
            SetActivePerspectiveCam(false);


            PivotObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            PivotObj.transform.localScale = Vector3.one * 0.5f;
            SphereCollider collider = PivotObj.GetComponent<SphereCollider>();
            if (collider) collider.enabled = false;
            DontDestroyOnLoad(PivotObj);

            int order = 0;

            mEnabled = Config.Bind(modEnableDef, true, new ConfigDescription("Controls if the mod should be enabled or disabled", null, new ConfigurationManagerAttributes { Order = order }));
            mEnabled.SettingChanged += onEnableDisable;
            order--;

            mPosAtStart = Config.Bind(PosAtStartDef, true, new ConfigDescription("Change the position of the camera at the start of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mRotAtStart = Config.Bind(RotAtStartDef, true, new ConfigDescription("Change the rotation of the camera at the start of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mSizeAtStart = Config.Bind(SizeAtStartDef, true, new ConfigDescription("Change the size of the camera at the start of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mPosAtStop = Config.Bind(PosAtStopDef, true, new ConfigDescription("Change the position of the camera at the end of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mRotAtStop = Config.Bind(RotAtStopDef, true, new ConfigDescription("Change the rotation of the camera at the end of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mSizeAtStop = Config.Bind(SizeAtStopDef, true, new ConfigDescription("Change the size of the camera at the end of a simulation", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mRotateEverywhere = Config.Bind(RotateEverywhereDef, false, new ConfigDescription("Controls if you can rotate the camera in build mode", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mToggleRotate = Config.Bind(ToggleRotateDef, new KeyboardShortcut(KeyCode.None), new ConfigDescription("What button toggles the rotate everywhere setting", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mCamPosBounds = Config.Bind(CamPosBoundsDef, true, new ConfigDescription("Enable/Disable position boundaries for the camera", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mCamRotBounds = Config.Bind(CamRotBoundsDef, true, new ConfigDescription("Enable/Disable rotation boundaries for the camera", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mRecenter = Config.Bind(RecenterDef, true, new ConfigDescription("Enable/Disable the recenter button", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mGrid = Config.Bind(GridDef, false, new ConfigDescription("Controls if you can enable/disable the grid everywhere", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mPivot = Config.Bind(PivotDef, false, new ConfigDescription("Controls if there will be a sphere displayed at the pivot point of the camera", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mCamPos = Config.Bind(CamPosDef, new Vector3(-23232323, 232323, 23232323), new ConfigDescription("Set the position of the camera (Reset to get the position of the camera)", null, new ConfigurationManagerAttributes { Order = order }));
            mCamPos.SettingChanged += onPositionChanged;
            order--;

            mCamRot = Config.Bind(CamRotDef, new Vector3(-23232323, 232323, 23232323), new ConfigDescription("Set the rotation of the camera (Reset to get the rotation of the camera)", null, new ConfigurationManagerAttributes { Order = order }));
            mCamRot.SettingChanged += onRotationChanged;
            order--;

            mCamSize = Config.Bind(CamSizeDef, -23232323f, new ConfigDescription("Set the size of the camera (Reset to get the size of the camera)", null, new ConfigurationManagerAttributes { Order = order }));
            mCamSize.SettingChanged += onSizeChanged;
            order--;

            mFollowPos = Config.Bind(FollowPosDef, true, new ConfigDescription("Follow the position of the target", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mFollowRot = Config.Bind(FollowRotDef, false, new ConfigDescription("Follow the rotation of the target", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mBackground = Config.Bind(BackgroundDef, true, new ConfigDescription("Enable/Disable the gradient background", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mBackgroundColor = Config.Bind(BackgroundColorDef, "", new ConfigDescription("The background color when the gradient background is disabled", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mTheme = Config.Bind(ThemeDef, Themes.Default, new ConfigDescription("The theme everything gets displayed in", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mMainMenuWorld = Config.Bind(MainMenuWorldDef, MenuWorlds.Default, new ConfigDescription("The world that gets loaded as the background of the main menu", null, new ConfigurationManagerAttributes { Order = order }));
            mMainMenuWorld.SettingChanged += MainMenuReload;
            order--;

            mCustomMainMenu = Config.Bind(CustomMainMenuDef, false, new ConfigDescription("Controls if it should load custom levels as the background of the main menu", null, new ConfigurationManagerAttributes { Order = order }));
            mCustomMainMenu.SettingChanged += MainMenuReload;
            order--;

            mFirstPerson = Config.Bind(FirstPersonDef, false, new ConfigDescription("A first person view of the target", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mAutoOffset = Config.Bind(AutoOffsetDef, true, new ConfigDescription("Automaticaly add a offset based on the target vehicle", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mThirdPerson = Config.Bind(ThirdPersonDef, new KeyboardShortcut(KeyCode.None), new ConfigDescription("Switch between first person and third person view when auto offset is enabled", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mPosOffset = Config.Bind(PosOffsetDef, Vector3.zero, new ConfigDescription("A offset thats added to the position off the car in first person mode", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mRotOffset = Config.Bind(RotOffsetDef, Vector3.zero, new ConfigDescription("A offset thats added to the rotaion of the car in first person mode", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mChangeTarget = Config.Bind(ChangeTargetDef, new KeyboardShortcut(KeyCode.Tab), new ConfigDescription("What button changes the camera target", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mChangePerspective = Config.Bind(ChangePerspectiveDef, new KeyboardShortcut(KeyCode.None), new ConfigDescription("What button changes the camara projection type", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            mFieldOfView = Config.Bind(FieldOfViewDef, 60f, new ConfigDescription("The field of view when using perspective projection type", null, new ConfigurationManagerAttributes { Order = order }));
            order--;

            Config.SettingChanged += onSettingChanged;
            onSettingChanged(null, null);

            try
            {
                if (!Directory.Exists(MainPath))
                {
                    Directory.CreateDirectory(MainPath);
                    Debug.Log("CameraMod folder Created!");
                }

                if (!Directory.Exists(MainPath + "Custom Menu Layouts"))
                {
                    Directory.CreateDirectory(MainPath + "Custom Menu Layouts");
                }

                string[] themes = new string[] {
                    "PineMountains",
                    "GlowingGorge",
                    "TranquilOasis",
                    "SanguineGulch",
                    "SerenityValley",
                    "Steamtown" 
                };

                foreach (string name in themes)
                {
                    if (!Directory.Exists(MainPath + "Custom Menu Layouts/" + name))
                    {
                        Directory.CreateDirectory(MainPath + "Custom Menu Layouts/" + name);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Something went wrong while creating camera mod folders!\n" + e);
            }

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            isCheat = false;
            isEnabled = mEnabled.Value;

            repositoryUrl = "https://github.com/Bram2323/PB-Camera-Mod/";

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

                SetActiveFollowCam(false);
                SetActivePerspectiveCam(false);
                backGround.GetComponent<MeshRenderer>().enabled = true;

                if (GameStateManager.GetState() == GameState.MAIN_MENU)
                {
                    GameStateMainMenu.Exit(GameState.MAIN_MENU);
                    GameStateMainMenu.Enter(GameState.MAIN_MENU);
                }
            }
        }

        public void onSettingChanged(object sender, EventArgs e)
        {
            isEnabled = mEnabled.Value;

            if (isEnabled && GameStateManager.GetState() == GameState.SIM)
            {
                onStopSim();
                onStartSim();
            }

            FollowCam.fieldOfView = mFieldOfView.Value;
            ReplayFollowCam.fieldOfView = mFieldOfView.Value;
            PerspectiveCam.fieldOfView = mFieldOfView.Value;
            ReplayPerspectiveCam.fieldOfView = mFieldOfView.Value;

            PivotObj.SetActive(mPivot.Value);
        }

        public void MainMenuReload(object sender, EventArgs e)
        {
            if (isEnabled && GameStateManager.GetState() == GameState.MAIN_MENU)
            {
                GameStateMainMenu.Exit(GameState.MAIN_MENU);
                GameStateMainMenu.Enter(GameState.MAIN_MENU);
            }
        }

        public void onPositionChanged(object sender, EventArgs e)
        {
            if (!isEnabled || mCamPos.Value == new Vector3(-23232323, 232323, 23232323)) return;
            Cameras.MainCamera().transform.position = mCamPos.Value;
            PointsOfView.UpdatePivotBasedOnCamera();
        }

        public void onRotationChanged(object sender, EventArgs e)
        {
            if (!isEnabled || mCamRot.Value == new Vector3(-23232323, 232323, 23232323)) return;
            Cameras.MainCamera().transform.rotation = Quaternion.Euler(mCamRot.Value);
            PointsOfView.UpdatePivotBasedOnCamera();
        }

        public void onSizeChanged(object sender, EventArgs e)
        {
            if (!isEnabled || mCamSize.Value == -23232323) return;
            Cameras.SetOrthographicSize(mCamSize.Value);
            PointsOfView.UpdatePivotBasedOnCamera();
        }

        public override void enableMod()
        {
            isEnabled = true;
            mEnabled.Value = true;
            onEnableDisable(null, null);
        }

        public override void disableMod()
        {
            isEnabled = false;
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
        
        void Update()
        {
            if (!loaded) return;

            if (Cameras.m_Instance && Cameras.MainCamera())
            {
                if (mCamPos.Value == new Vector3(-23232323, 232323, 23232323)) mCamPos.Value = Cameras.MainCamera().transform.position;
                if (mCamRot.Value == new Vector3(-23232323, 232323, 23232323)) mCamRot.Value = Cameras.MainCamera().transform.rotation.eulerAngles;
                if (mCamSize.Value == -23232323) mCamSize.Value = Cameras.GetOrthographicSize();
            }

            if (!CheckForCheating()) return;

            UpdateThemeSettings();

            if (mGrid.Value && (GameStateManager.GetState() != GameState.BUILD) && GameInput.JustPressed(BindingType.GRID))
            {
                if (Profile.m_GridEnabled)
                {
                    GameUI.m_Instance.m_TopBar.OnGridSelected();
                }
                else
                {
                    GameUI.m_Instance.m_TopBar.OnGrid();
                }
            }

            PivotObj.transform.position = PointsOfView.m_Pivot;

            if (!mRecenter.Value && GameUI.m_Instance.m_Recenter.gameObject.activeInHierarchy) GameUI.m_Instance.m_Recenter.gameObject.SetActive(false);

            if (CameraControl.instance)
            {
                CameraControl control = CameraControl.instance;
                Camera cam = control.cam;

                if (!mCamPosBounds.Value && control.focusBounds.max.x < 1000) control.focusBounds = new Bounds2(Vec2.zero, Vec2.one * 10000);

                Color backgroundColor;
                if (ColorUtility.TryParseHtmlString(mBackgroundColor.Value, out backgroundColor))
                {
                    cam.backgroundColor = backgroundColor;
                    Cameras.ReplayCamera().backgroundColor = backgroundColor;
                }

                if (GameStateManager.GetState() == GameState.SIM && InSim)
                {
                    if (mThirdPerson.Value.IsDown()) thirdPerson = !thirdPerson;

                    vehicles = Vehicles.m_Vehicles;

                    if (mChangeTarget.Value.IsDown())
                    {
                        CamIndex++;
                        if (CamIndex > vehicles.Count) CamIndex = 0;

                        Vehicle target = null;
                        if (CamIndex != 0)
                        {
                            target = vehicles[CamIndex - 1];
                            SetActiveFollowCam(mFirstPerson.Value);
                        }
                        else
                        {
                            SetActiveFollowCam(false);
                        }
                        CamTarget = target;
                    }

                    if (CamIndex > vehicles.Count) CamIndex = 0;

                    if (CamTarget)
                    {
                        UpdateFollowCam(CamTarget);

                        if (mFollowRot.Value)
                        {
                            cam.transform.eulerAngles = new Vector3(0, 0, CamTarget.m_MeshRenderer.transform.eulerAngles.z);
                            cam.transform.position = new Vector3(CamTarget.m_MeshRenderer.transform.position.x, CamTarget.m_MeshRenderer.transform.position.y, -200);
                            PointsOfView.UpdatePivotBasedOnCamera();
                        }

                        if (mFollowPos.Value)
                        {
                            CameraControl.instance.RotMouse(Vec2.zero);
                            PointsOfView.UpdatePivotBasedOnCamera();
                        }
                    }
                }
            }
        }

        public void onStartSim()
        {
            CameraControl control = CameraControl.instance;
            Camera cam = control.cam;
            vehicles = Vehicles.m_Vehicles;
            shapes = CustomShapes.m_Shapes;

            CamIndex = LastCamIndex;
            if (CamIndex > vehicles.Count) CamIndex = 0;
            CamTarget = null;
            if (CamIndex != 0)
            {
                CamTarget = vehicles[CamIndex - 1];
                SetActiveFollowCam(mFirstPerson.Value);
            }

            InSim = true;
        }

        public void onStopSim()
        {
            InSim = false;

            LastCamIndex = CamIndex;
            CamIndex = 0;
            SetActiveFollowCam(false);
        }

        [HarmonyPatch(typeof(GameStateMainMenu), "GetWorldForMainMenu")]
        private static class patchGetMainMenuWorld
        {
            private static void Postfix(ref CampaignWorld __result)
            {
                if (!instance.CheckForCheating()) return;

                if (instance.mMainMenuWorld.Value == MenuWorlds.Default)
                {
                    if (instance.mCustomMainMenu.Value)
                    {
                        List<CampaignWorld> worlds = new List<CampaignWorld>();

                        foreach (CampaignWorld world in CampaignWorlds.m_Instance.m_Worlds)
                        {
                            string[] levels = instance.GetCustomLayoutsForWorld(world.m_ThemeStub.m_ThemeName);
                            if (levels.Length > 0) worlds.Add(world);
                        }

                        if (worlds.Count > 0) __result = worlds[UnityEngine.Random.Range(0, worlds.Count)];
                        else __result = CampaignWorlds.m_Instance.m_Worlds[UnityEngine.Random.Range(0, CampaignWorlds.m_Instance.m_Worlds.Length)];
                    }

                    return;
                }
                
                __result = CampaignWorlds.m_Instance.m_Worlds[(int)instance.mMainMenuWorld.Value - 1];
            }
        }

        [HarmonyPatch(typeof(GameStateMainMenu), "LoadLayout")]
        private static class patchLoadLayout
        {
            private static bool Prefix()
            {
                if (!instance.CheckForCheating()) return true;

                if (instance.mCustomMainMenu.Value)
                {
                    string[] paths = instance.GetCustomLayoutsForWorld(GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName);

                    if (paths.Length != 0)
                    {
                        string fullPath = paths[0];

                        string path = Path.GetDirectoryName(fullPath);
                        string name = Path.GetFileName(fullPath);

                        SandboxLayoutData customSandboxLayoutData = SandboxLayout.Load(path, name);
                        if (customSandboxLayoutData == null)
                        {
                            customSandboxLayoutData = SandboxLayout.LoadLegacy(path, name);
                        }
                        if (customSandboxLayoutData == null)
                        {
                            Debug.LogWarningFormat("Could not load: " + name);
                            instance.LoadMainMenuError();
                            return false;
                        }

                        instance.LoadMainMenuLayout(customSandboxLayoutData, GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName);
                        return false;
                    }
                    else
                    {
                        instance.LoadMainMenuError();
                        return false;
                    }
                }
                else if (GameStateMainMenu.m_World.m_MainMenuLevels.Length > 0) return true;

                instance.LoadMainMenuLevel();
                return false;
            }
        }

        [HarmonyPatch(typeof(GameStateMainMenu), "LoadNextLayout")]
        private static class patchLoadNextLayout
        {
            private static bool Prefix()
            {
                if (!instance.CheckForCheating() || !instance.mCustomMainMenu.Value) return true;

                string[] paths = instance.GetCustomLayoutsForWorld(GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName);

                GameStateMainMenu.m_LevelIndexInWorld++;
                if (GameStateMainMenu.m_LevelIndexInWorld >= paths.Length)
                {
                    GameStateMainMenu.m_LevelIndexInWorld = 0;
                }

                if (paths.Length != 0)
                {
                    string fullPath = paths[GameStateMainMenu.m_LevelIndexInWorld];

                    string path = Path.GetDirectoryName(fullPath);
                    string name = Path.GetFileName(fullPath);

                    SandboxLayoutData customSandboxLayoutData = SandboxLayout.Load(path, name);
                    if (customSandboxLayoutData == null)
                    {
                        customSandboxLayoutData = SandboxLayout.LoadLegacy(path, name);
                    }
                    if (customSandboxLayoutData == null)
                    {
                        Debug.LogWarningFormat("Could not load: " + name);
                        instance.LoadMainMenuError();
                        return false;
                    }

                    instance.LoadMainMenuLayout(customSandboxLayoutData, GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName);
                    return false;
                }

                byte[] level = new levels().error;

                int offset = 0;
                SandboxLayoutData sandboxLayoutData = new SandboxLayoutData(level, ref offset);

                string themeName = GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName;

                instance.LoadMainMenuLayout(sandboxLayoutData, themeName);
                return false;
            }
        }

        [HarmonyPatch(typeof(GameStateMainMenu), "LoadCustomCamera")]
        private static class patchMainMenuLoadCamera
        {
            private static bool Prefix()
            {
                if (!instance.CheckForCheating()) return true;

                if (instance.mCustomMainMenu.Value)
                {
                    PointsOfView.RotateTo(PointOfViewType.SIM_RIGHT, 0f);

                    string[] paths = instance.GetCustomLayoutsForWorld(GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName);

                    if (paths.Length != 0)
                    {
                        string fullPath = paths[GameStateMainMenu.m_LevelIndexInWorld];

                        string path = Path.GetDirectoryName(fullPath);
                        string name = Path.GetFileNameWithoutExtension(fullPath);

                        string cameraPath = path + "\\" + name + ".txt";

                        if (File.Exists(cameraPath))
                        {
                            string data = File.ReadAllText(cameraPath);
                            string[] numStrings = data.Split(new char[] { ' ', '/', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            if (numStrings.Length == 7)
                            {
                                float[] nums = new float[7];
                                bool invalid = true;
                                for (int i = 0; i < 7; i++)
                                {
                                    invalid = !float.TryParse(numStrings[i], out nums[i]) && invalid;
                                }

                                if (!invalid)
                                {
                                    Cameras.MainCamera().transform.position = new Vector3(nums[0], nums[1], nums[2]);
                                    Cameras.MainCamera().transform.rotation = Quaternion.Euler(nums[3], nums[4], nums[5]);
                                    Cameras.SetOrthographicSize(nums[6]);
                                    PointsOfView.UpdatePivotBasedOnCamera();
                                }
                                else Debug.LogWarning("Camera file found for '" + name + "' but does not contain valid data!");
                            }
                            else Debug.LogWarning("Camera file found for '" + name + "' but does not contain valid data!");
                        }
                    }
                }
                else if (GameStateMainMenu.m_World.m_MainMenuLevels.Length == 0)
                {
                    Cameras.MainCamera().transform.position = new Vector3(45, 59, -200);
                    Cameras.MainCamera().transform.rotation = new Quaternion(-0.1f, 0.1f, 0, -1);
                    Cameras.SetOrthographicSize(20);
                    PointsOfView.UpdatePivotBasedOnCamera();
                }
                else return true;

                return false;
            }

            private static void Postfix()
            {
                instance.loaded = true;
            }
        }

        [HarmonyPatch(typeof(GameStateManager), "LateUpdateManual")]
        private static class patchManagerUpdateManual
        {
            private static void Postfix()
            {
                if (!instance.CheckForCheating() || GameStateManager.GetState() != GameState.MAIN_MENU) return;

                CustomShapes.UpdateManual();
            }
        }

        public string[] GetCustomLayoutsForWorld(string themeName)
        {
            string folderName = "";

            switch (themeName)
            {
                case "PineMountains":
                    folderName = "PineMountains";
                    break;
                case "Volcano":
                    folderName = "GlowingGorge";
                    break;
                case "Savanna":
                    folderName = "TranquilOasis";
                    break;
                case "Western":
                    folderName = "SanguineGulch";
                    break;
                case "ZenGardens":
                    folderName = "SerenityValley";
                    break;
                case "Steampunk":
                    folderName = "Steamtown";
                    break;
                default:
                    Debug.Log("Unkown theme name: " + themeName);
                    return new string[0];
            }

            string dirPath = MainPath + "Custom Menu Layouts/" + folderName;

            string[] files = Directory.GetFiles(dirPath, "*.layout").OrderBy(f => f).ToArray();

            return files;
        }

        public void LoadMainMenuLayout(SandboxLayoutData sandboxLayoutData, string themeName = "")
        {
            if (sandboxLayoutData == null) return;
            if (string.IsNullOrWhiteSpace(themeName)) themeName = sandboxLayoutData.m_ThemeStubKey;

            if (string.IsNullOrEmpty(themeName))
            {
                return;
            }

            Sandbox.Clear();
            Sandbox.Load(themeName, sandboxLayoutData, true);
            PointsOfView.OnLayoutLoaded();
            Sandbox.m_CurrentLayoutName = "MainMenu_Custom";
            if (Profile.m_LastMainMenuThemeName != themeName)
            {
                Profile.m_LastMainMenuThemeName = themeName;
                Profile.Save();
            }
            GameStateMainMenu.m_CurrentLayoutHash = Sandbox.m_CurrentLayoutHash;
        }

        public void LoadMainMenuError()
        {
            byte[] error = new levels().error;

            int errorOffset = 0;
            SandboxLayoutData errorSandboxLayoutData = new SandboxLayoutData(error, ref errorOffset);

            string errorThemeName = GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName;

            LoadMainMenuLayout(errorSandboxLayoutData, errorThemeName);
        }

        public void LoadMainMenuLevel()
        {
            byte[] error = new levels().level;

            int errorOffset = 0;
            SandboxLayoutData errorSandboxLayoutData = new SandboxLayoutData(error, ref errorOffset);

            string errorThemeName = GameStateMainMenu.m_World.m_ThemeStub.m_ThemeName;

            LoadMainMenuLayout(errorSandboxLayoutData, errorThemeName);
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

            private static void Postfix()
            {
                if (!instance.CheckForCheating()) return;

                if (instance.mGrid.Value)
                {
                    GameUI.m_Instance.m_TopBar.m_GridButton.interactable = true;
                    GameUI.m_Instance.m_TopBar.m_GridSelectedButton.interactable = true;
                    GameUI.m_Grid.SetActive(Profile.m_GridEnabled);
                }
            }
        }

        [HarmonyPatch(typeof(CameraControl), "RecalcCamPosition")]
        private static class patchCameraControl
        {
            private static void Prefix(CameraControl __instance, ref Vec2 ___anchorPos, ref Vec2 ___offsetFromAnchor)
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

                if (instance.mChangePerspective.Value.IsDown()) instance.SetActivePerspectiveCam(!instance.PerspectiveCam.enabled);

                Camera cam = null;
                if (CameraControl.instance && CameraControl.instance.cam) cam = CameraControl.instance.cam;
                if (cam)
                {
                    instance.UpdatePerspectiveCam(CameraControl.instance.cam);
                    bool backgroundEnabled = instance.mBackground.Value && !instance.FollowCam.enabled && !instance.PerspectiveCam.enabled;
                    GameObject backGround = cam.gameObject.transform.GetChild(0).gameObject;
                    backGround.GetComponent<MeshRenderer>().enabled = backgroundEnabled;
                }

                if (instance.mToggleRotate.Value.IsDown())
                {
                    instance.mRotateEverywhere.Value = !instance.mRotateEverywhere.Value;
                    GameUI.m_Instance.m_TopBar.m_MessageTopCenter.ShowMessage("Rotate Everywhere " + (instance.mRotateEverywhere.Value ? "enabled" : "disabled"), 2);
                }

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

                    if (instance.mCamRotBounds.Value)
                    {
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
                    else __instance.transform.RotateAround(PointsOfView.m_Pivot, Cameras.MainCamera().transform.right, num2);
                }
                return false;
            }
        }



        [HarmonyPatch(typeof(CameraControl), "RotMouse")]
        private static class patchCameraRotate
        {
            private static bool Prefix(Vec2 deltaYawPitch, CameraControl __instance, ref Vec2 ___mouseYawPitch, ref Vec2 ___anchorPos, ref Vec2 ___offsetFromAnchor)
            {
                if (!instance.CheckForCheating() || instance.mCamRotBounds.Value) return true;

                ___mouseYawPitch += deltaYawPitch;

                if (___mouseYawPitch.x < 0f)
                {
                    ___mouseYawPitch.x = ___mouseYawPitch.x + 360f;
                }
                if (360f <= ___mouseYawPitch.x)
                {
                    ___mouseYawPitch.x = ___mouseYawPitch.x - 360f;
                }
                ___mouseYawPitch.x = Mathf.Clamp(___mouseYawPitch.x, 0f, 360f);

                if (___mouseYawPitch.y < 0f)
                {
                    ___mouseYawPitch.y = ___mouseYawPitch.y + 360f;
                }
                if (360f <= ___mouseYawPitch.y)
                {
                    ___mouseYawPitch.y = ___mouseYawPitch.y - 360f;
                }
                ___mouseYawPitch.y = Mathf.Clamp(___mouseYawPitch.y, 0f, 360f);

                __instance.cam.transform.eulerAngles = new Vector3(___mouseYawPitch.y, ___mouseYawPitch.x, 0f);

                if (GameStateManager.GetState() == GameState.SIM && (instance.mFollowPos.Value || instance.mFirstPerson.Value) && instance.CamTarget)
                {
                    ___anchorPos = instance.CamTarget.m_MeshRenderer.transform.position;
                    ___offsetFromAnchor = new Vec2(0, 0);
                }

                Vector3 pos = ___anchorPos;
                __instance.cam.transform.position = pos + __instance.cameraDistanceZ * __instance.cam.transform.forward + __instance.cam.transform.rotation * ___offsetFromAnchor;

                return false;
            }
        }

        [HarmonyPatch(typeof(CameraControl), "Update_Manual_AfterExternalTransformations")]
        private static class patchCameraUpdateManual
        {
            private static bool Prefix(CameraControl __instance, ref Vec2 ___mouseYawPitch, Vec2 ___anchorPos, Vec2 ___offsetFromAnchor)
            {
                if (!instance.CheckForCheating() || instance.mCamRotBounds.Value) return true;

                Vector3 pos = ___anchorPos;
                Vector3 b = pos + __instance.cameraDistanceZ * __instance.cam.transform.forward + __instance.cam.transform.rotation * ___offsetFromAnchor;
                Vector3 position = __instance.cam.transform.position;
                Vec2 displacement = Quaternion.Inverse(__instance.cam.transform.rotation) * (position - b);

                Vector3 vector = __instance.cam.transform.up;
                vector.y = 0f;
                vector.Normalize();
                float x = Mathf.Atan2(-vector.x, vector.z) * 57.29578f;
                Vector3 vector2 = Quaternion.Euler(0f, x, 0f) * __instance.cam.transform.forward;
                float y = Mathf.Atan2(vector2.y, vector2.z) * 57.29578f;
                if (___mouseYawPitch.y > 180)
                {
                    x -= 180;
                    y += 180;
                    y *= -1;
                }
                ___mouseYawPitch = -1f * new Vec2(x, y);

                __instance.Update_Manual_AfterExternalTranslation_InScreenSpace(displacement);

                return false;
            }
        }


        [HarmonyPatch(typeof(CameraControl), "Update")]
        private static class patchControlUpdate
        {
            private static void Postfix(ref Vec2 ___anchorPos)
            {
                instance.cashedAnchor = ___anchorPos;
            }
        }


        [HarmonyPatch(typeof(GameStateSim), "Enter")]
        private static class patchEnterSim
        {
            private static void Prefix()
            {
                instance.cashedPivot = PointsOfView.m_Pivot;
            }
        }

        [HarmonyPatch(typeof(Panel_PointOfView), "OnCenterView")]
        private static class patchCenterView
        {
            private static void Prefix()
            {
                instance.allowRotate = true;
            }
        }

        [HarmonyPatch(typeof(Panel_PointOfView), "OnLeftView")]
        private static class patchLeftView
        {
            private static void Prefix()
            {
                instance.allowRotate = true;
            }
        }

        [HarmonyPatch(typeof(Panel_PointOfView), "OnRightView")]
        private static class patchRightView
        {
            private static void Prefix()
            {
                instance.allowRotate = true;
            }
        }

        [HarmonyPatch(typeof(PointsOfView), "RotateTo")]
        private static class patchCameraInterpolate
        {
            private static bool Prefix(PointOfViewType type)
            {
                if (instance.allowRotate)
                {
                    instance.allowRotate = false;
                    return true;
                }

                if (!instance.CheckForCheating() || type == PointOfViewType.BUILD || GameStateManager.GetState() == GameState.MAIN_MENU) return true;

                bool changePos, changeRot, changeSize;

                if (GameStateManager.GetState() == GameState.SIM)
                {
                    changePos = instance.mPosAtStart.Value;
                    changeRot = instance.mRotAtStart.Value;
                    changeSize = instance.mSizeAtStart.Value;
                }
                else
                {
                    changePos = instance.mPosAtStop.Value;
                    changeRot = instance.mRotAtStop.Value;
                    changeSize = instance.mSizeAtStop.Value;
                }

                if (changePos && changeRot && changeSize) return true;

                PointOfView pointOfView = PointsOfView.GetPointOfView(type);
                if (pointOfView == null)
                {
                    return false;
                }

                float distance;
                if (changeRot) distance = Vector3.Distance(pointOfView.m_Pivot, pointOfView.m_Pos);
                else distance = Vector3.Distance(PointsOfView.m_Pivot, Cameras.MainCamera().transform.position);

                CameraInterpolate.Cancel();
                if (changePos) PointsOfView.m_Pivot = pointOfView.m_Pivot;
                else if (GameStateManager.GetState() == GameState.SIM) PointsOfView.m_Pivot = instance.cashedPivot;
                if (changeSize) Cameras.SetOrthographicSize(pointOfView.m_OrthographicsSize);
                if (changeRot) Cameras.MainCamera().transform.rotation = pointOfView.m_Rot;

                Cameras.MainCamera().transform.position = PointsOfView.m_Pivot;
                Cameras.MainCamera().transform.position -= Cameras.MainCamera().transform.forward * distance;

                Bridge.RefreshZoomDependentVisibility();
                CameraControl.RegisterTransformUpdate();

                Profile.m_PointOfViewType = PointOfViewType.SIM_CENTER;

                return false;
            }
        }

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



        [HarmonyPatch(typeof(Cameras), "EnableThemePostProcessing")]
        private static class patchEnableTheme
        {
            private static void Prefix()
            {
                instance.themeEnabled = true;
            }
        }

        [HarmonyPatch(typeof(Cameras), "DisableThemePostProcessing")]
        private static class patchDisableTheme
        {
            private static void Prefix()
            {
                instance.themeEnabled = false;
            }
        }

        public void UpdateThemeSettings()
        {
            if (mTheme.Value == Themes.SimMode)
            {
                Cameras.DisableDesaturationPostFX();
                if (themeEnabled == false) Theme.m_Instance.EnablePostProcessing();
                if (Theme.m_Instance.m_SunLight.gameObject.activeInHierarchy == false) Theme.m_Instance.EnableSimModeLighting();
                if (Cameras.m_Instance.m_GradientSky.gameObject.activeInHierarchy == false) Cameras.m_Instance.m_GradientSky.gameObject.SetActive(mBackground.Value);
                Cameras.MainCamera().backgroundColor = GameUI.m_Instance.m_BuildModeBackgroundColor;
                RenderSettings.ambientLight = Theme.m_Instance.m_ThemeStub.m_AmbientLightColor;
                CustomShapes.RemoveOverrideColor();
                CustomShapes.HideStaticPins();
                TerrainIslands.EnableLights();
            }
            else if (mTheme.Value == Themes.BuildMode)
            {
                Cameras.EnableDesaturationPostFX();
                if (themeEnabled == true) Cameras.DisableThemePostProcessing();
                if (Theme.m_Instance.m_BuildModeLight.gameObject.activeInHierarchy == false) Theme.m_Instance.EnableBuildModeLighting();
                if (Cameras.m_Instance.m_GradientSky.gameObject.activeInHierarchy == true) Cameras.m_Instance.m_GradientSky.gameObject.SetActive(false);
                Cameras.MainCamera().backgroundColor = GameUI.m_Instance.m_BuildModeBackgroundColor;
                RenderSettings.ambientLight = Utils.RGBToColor(130, 130, 130);
                CustomShapes.OverrideColor(Color.blue);
                CustomShapes.HideStaticPins();
                SandboxItems.EnableOutlines();
                TerrainIslands.DisableLights();
            }
            else if (mTheme.Value == Themes.SandboxMode)
            {
                Cameras.DisableDesaturationPostFX();
                if (themeEnabled == true) Cameras.DisableThemePostProcessing();
                if (Theme.m_Instance.m_BuildModeLight.gameObject.activeInHierarchy == false) Theme.m_Instance.EnableBuildModeLighting();
                if (Cameras.m_Instance.m_GradientSky.gameObject.activeInHierarchy == true) Cameras.m_Instance.m_GradientSky.gameObject.SetActive(false);
                Cameras.MainCamera().backgroundColor = GameUI.m_Instance.m_BlueprintBackgroundColor;
                RenderSettings.ambientLight = Utils.RGBToColor(130, 130, 130);
                CustomShapes.RemoveOverrideColor();
                CustomShapes.ShowStaticPins();
                SandboxItems.EnableOutlines();
                TerrainIslands.DisableLights();
            }
        }


        public void SetActiveFollowCam(bool active)
        {
            if (Cameras.m_Instance != null) ReplayFollowCam.targetTexture = Cameras.ReplayCamera().targetTexture;

            ReplayFollowCam.enabled = active;
            FollowCam.enabled = active;
        }
        
        public void UpdateFollowCam(Vehicle target)
        {
            if (!target.m_MeshRenderer || !target.Physics) return;

            Vector2 VehicleOffset = GetOffset(target);
            float OffsetX = mPosOffset.Value.x + VehicleOffset.x;
            float OffsetY = mPosOffset.Value.y + VehicleOffset.y;
            float OffsetZ = mPosOffset.Value.z;

            float RotOffset = 0;
            if (thirdPerson && mAutoOffset.Value) RotOffset = 25;

            Vector3 Rot = target.m_MeshRenderer.transform.eulerAngles;
            if (target.Physics.isFlipped) Rot += new Vector3(180, 0, 180);
            FollowCam.transform.eulerAngles = new Vector3(-Rot.z + mRotOffset.Value.x + RotOffset, 90f + mRotOffset.Value.y, Rot.x + mRotOffset.Value.z);

            if (target.Physics.isFlipped)
            {
                OffsetX *= -1;
                OffsetZ *= -1;
            }


            Vector3 Offset = target.m_MeshRenderer.transform.right * OffsetX;
            Offset += target.m_MeshRenderer.transform.up * OffsetY;
            Offset += target.m_MeshRenderer.transform.forward * OffsetZ;

            FollowCam.transform.position = target.m_MeshRenderer.transform.position + Offset;
        }

        public void SetActivePerspectiveCam(bool active)
        {
            if (Cameras.m_Instance != null) ReplayPerspectiveCam.targetTexture = Cameras.ReplayCamera().targetTexture;

            ReplayPerspectiveCam.enabled = active;
            PerspectiveCam.enabled = active;
        }

        public void UpdatePerspectiveCam(Camera cam)
        {
            PerspectiveCamObj.transform.rotation = cam.transform.rotation;

            float size = cam.orthographicSize * 3;

            PerspectiveCamObj.transform.position = cam.transform.position + cam.transform.forward * (200 - size);
        }
        
        public Vector2 GetOffset(Vehicle vehicle)
        {
            if (!mAutoOffset.Value || vehicle.m_DisplayNameLocKey == null) return Vector2.zero;

            if (!thirdPerson)
            {
                switch (vehicle.m_DisplayNameLocKey)
                {
                    case "VEHICLE_TRUCK_WITH_CONTAINER":
                    case "VEHICLE_TRUCK_WITH_LIQUID":
                    case "VEHICLE_TRUCK":
                    case "VEHICLE_TRUCK_WITH_FLATBED":
                        return new Vector2(0.75f, 1.25f);
                    case "VEHICLE_SCHOOL_BUS":
                        return new Vector2(0.75f, 1.3f);
                    case "VEHICLE_VESPA":
                        return new Vector2(-0.25f, 0.81f);
                    case "VEHICLE_CHOPPER":
                        return new Vector2(-0.47f, 1);
                    case "VEHICLE_DUNE_BUGGY":
                        return new Vector2(0.13f, 0.75f);
                    case "VEHICLE_COMPACT_CAR":
                        return new Vector2(-0.1f, 0.7f);
                    case "VEHICLE_SPORTS_CAR":
                        return new Vector2(0.1f, 0.6f);
                    case "VEHICLE_TAXI":
                        return new Vector2(0, 0.9f);
                    case "VEHICLE_MODELT":
                        return new Vector2(0, 1.1f);
                    case "VEHICLE_PICKUP_TRUCK":
                        return new Vector2(0.62f, 1.2f);
                    case "VEHICLE_LIMO":
                        return new Vector2(0.8f, 0.85f);
                    case "VEHICLE_VAN":
                        return new Vector2(0.9f, 1.1f);
                    case "VEHICLE_TOWTRUCK":
                        return new Vector2(0.45f, 1.16f);
                    case "VEHICLE_STEAMPUNK_HOTROD":
                        return new Vector2(-0.29f, 1.2f);
                    case "VEHICLE_MONSTER_TRUCK":
                        return new Vector2(0.04f, 2.07f);
                    case "VEHICLE_AMBULANCE":
                        return new Vector2(0.17f, 1.2f);
                    case "VEHICLE_FIRE_TRUCK":
                        return new Vector2(1, 1.3f);
                    case "VEHICLE_BULLDOZER":
                        return new Vector2(-0.2f, 1.8f);
                    case "VEHICLE_DUMP_TRUCK":
                        return new Vector2(1.6f, 1.9f);
                    case "VEHICLE_ARTICULATED_BUS":
                        return new Vector2(2.4f, 1.5f);
                }
            }
            else
            {
                switch (vehicle.m_DisplayNameLocKey)
                {
                    case "VEHICLE_TRUCK_WITH_CONTAINER":
                    case "VEHICLE_TRUCK_WITH_LIQUID":
                    case "VEHICLE_TRUCK_WITH_FLATBED":
                        return new Vector2(-8f, 5f);
                    case "VEHICLE_TRUCK":
                    case "VEHICLE_SCHOOL_BUS":
                        return new Vector2(-4f, 4f);
                    case "VEHICLE_VESPA":
                    case "VEHICLE_CHOPPER":
                        return new Vector2(-2f, 2f);
                    case "VEHICLE_DUNE_BUGGY":
                        return new Vector2(-2.5f, 2.5f);
                    case "VEHICLE_COMPACT_CAR":
                    case "VEHICLE_SPORTS_CAR":
                        return new Vector2(-2.5f, 2.2f);
                    case "VEHICLE_TAXI":
                        return new Vector2(-3f, 2.5f);
                    case "VEHICLE_MODELT":
                        return new Vector2(-3f, 2.7f);
                    case "VEHICLE_PICKUP_TRUCK":
                        return new Vector2(-3.2f, 3f);
                    case "VEHICLE_LIMO":
                        return new Vector2(-4.5f, 3.5f);
                    case "VEHICLE_VAN":
                        return new Vector2(-3.6f, 3.5f);
                    case "VEHICLE_TOWTRUCK":
                        return new Vector2(-4f, 3.7f);
                    case "VEHICLE_STEAMPUNK_HOTROD":
                        return new Vector2(-4.5f, 4f);
                    case "VEHICLE_MONSTER_TRUCK":
                        return new Vector2(-4.5f, 4.4f);
                    case "VEHICLE_AMBULANCE":
                        return new Vector2(-5f, 4.5f);
                    case "VEHICLE_FIRE_TRUCK":
                        return new Vector2(-6f, 5f);
                    case "VEHICLE_BULLDOZER":
                        return new Vector2(-5.3f, 4.8f);
                    case "VEHICLE_DUMP_TRUCK":
                        return new Vector2(-5.6f, 5.5f);
                    case "VEHICLE_ARTICULATED_BUS":
                        return new Vector2(-7f, 6f);
                }
            }

            return new Vector2(0, 0);
        }
    }

    public enum MenuWorlds
    {
        Default,
        [Description("Pine Mountains")]
        PineMountains,
        [Description("Glowing Gorge")]
        GlowingGorge,
        [Description("Tranquil Oasis")]
        TranquilOasis,
        [Description("Sanguine Gulch")]
        SanguineGulch,
        [Description("Serenity Valley")]
        SerenityValley,
        [Description("Steamtown")]
        Steamtown,
    }

    public enum Themes
    {
        Default,
        [Description("Simulation Mode")]
        SimMode,
        [Description("Build Mode")]
        BuildMode,
        [Description("Sandbox Mode")]
        SandboxMode
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

    public class levels
    {
        public byte[] level = new byte[] { 25, 0, 0, 0, 9, 0, 83, 116, 101, 97, 109, 112, 117, 110, 107, 45, 0, 0, 0, 0, 0, 0, 193, 0, 0, 160,
            64, 0, 0, 0, 0, 1, 0, 36, 0, 53, 55, 99, 48, 98, 98, 102, 56, 45, 50, 55, 97, 52, 45, 52, 55, 99, 57, 45, 97, 97,
            57, 56, 45, 56, 102, 97, 101, 100, 98, 53, 48, 97, 51, 56, 52, 0, 0, 224, 64, 0, 0, 160, 64, 0, 0, 0, 0, 1, 0, 36,
            0, 54, 57, 56, 52, 51, 50, 99, 49, 45, 50, 56, 54, 51, 45, 52, 50, 54, 48, 45, 56, 102, 51, 97, 45, 49, 57, 49, 49, 99,
            99, 50, 52, 99, 52, 57, 53, 0, 0, 160, 63, 0, 0, 242, 65, 0, 0, 0, 0, 1, 0, 36, 0, 102, 55, 101, 51, 53, 57, 49,
            56, 45, 53, 49, 100, 101, 45, 52, 55, 101, 53, 45, 97, 56, 99, 98, 45, 101, 51, 97, 55, 98, 97, 99, 102, 102, 102, 51, 54, 0,
            0, 16, 192, 0, 0, 242, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57, 49, 100, 57, 51, 98, 48, 97, 45, 53, 55, 53, 101, 45, 52,
            98, 97, 53, 45, 97, 49, 57, 56, 45, 49, 55, 49, 49, 50, 98, 48, 98, 48, 54, 51, 100, 0, 0, 0, 191, 0, 0, 242, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 54, 97, 57, 97, 101, 97, 101, 100, 45, 99, 97, 98, 53, 45, 52, 56, 51, 98, 45, 97, 97, 97, 50,
            45, 101, 48, 56, 99, 102, 49, 55, 100, 97, 102, 57, 56, 36, 241, 14, 65, 96, 100, 170, 64, 0, 0, 0, 0, 1, 0, 36, 0, 50,
            98, 52, 99, 98, 56, 102, 102, 45, 101, 54, 55, 100, 45, 52, 100, 97, 97, 45, 57, 49, 55, 100, 45, 56, 97, 57, 101, 100, 51, 53,
            53, 48, 48, 97, 98, 74, 214, 44, 65, 118, 171, 189, 64, 0, 0, 0, 0, 1, 0, 36, 0, 99, 99, 99, 102, 53, 48, 51, 97, 45,
            97, 102, 51, 97, 45, 52, 99, 53, 100, 45, 57, 97, 48, 54, 45, 52, 102, 102, 52, 99, 56, 52, 102, 48, 56, 102, 98, 148, 227, 72,
            65, 172, 216, 217, 64, 0, 0, 0, 0, 1, 0, 36, 0, 50, 102, 57, 55, 53, 57, 54, 50, 45, 57, 57, 99, 99, 45, 52, 48, 99,
            102, 45, 97, 54, 97, 48, 45, 51, 50, 97, 49, 48, 54, 56, 52, 98, 100, 97, 55, 222, 242, 97, 65, 144, 127, 255, 64, 0, 0, 0,
            0, 1, 0, 36, 0, 97, 52, 102, 54, 99, 53, 53, 51, 45, 101, 100, 99, 101, 45, 52, 100, 51, 54, 45, 97, 55, 102, 101, 45, 98,
            98, 48, 56, 56, 100, 56, 54, 97, 57, 55, 99, 190, 36, 119, 65, 102, 208, 22, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 101, 100,
            101, 56, 50, 99, 55, 45, 102, 100, 99, 48, 45, 52, 98, 101, 97, 45, 98, 57, 97, 102, 45, 49, 48, 100, 54, 102, 57, 51, 51, 102,
            50, 49, 56, 202, 224, 131, 65, 246, 97, 49, 65, 0, 0, 0, 0, 1, 0, 36, 0, 102, 50, 97, 49, 53, 48, 48, 50, 45, 98, 97,
            100, 97, 45, 52, 102, 51, 51, 45, 97, 50, 49, 99, 45, 97, 48, 50, 54, 57, 48, 54, 101, 99, 99, 48, 57, 158, 147, 137, 65, 22,
            190, 78, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 54, 102, 51, 56, 101, 99, 48, 45, 53, 101, 97, 50, 45, 52, 53, 57, 51, 45,
            97, 56, 100, 57, 45, 49, 52, 97, 101, 56, 50, 49, 98, 52, 102, 53, 98, 80, 188, 140, 65, 89, 87, 109, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 101, 98, 54, 102, 99, 97, 50, 102, 45, 98, 97, 49, 52, 45, 52, 98, 56, 51, 45, 56, 52, 101, 98, 45, 99, 57, 98,
            101, 49, 99, 97, 56, 97, 99, 48, 102, 5, 75, 141, 65, 78, 103, 134, 65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 99, 51, 56, 101,
            101, 54, 54, 45, 102, 48, 54, 97, 45, 52, 100, 97, 48, 45, 56, 55, 101, 97, 45, 55, 51, 54, 56, 50, 49, 52, 56, 99, 55, 98,
            48, 82, 111, 139, 65, 56, 3, 150, 65, 0, 0, 0, 0, 1, 0, 36, 0, 48, 102, 52, 100, 101, 51, 102, 55, 45, 54, 49, 50, 98,
            45, 52, 98, 48, 101, 45, 98, 54, 98, 97, 45, 50, 51, 54, 56, 56, 56, 49, 100, 56, 100, 50, 56, 51, 41, 135, 65, 71, 32, 165,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 52, 53, 53, 101, 97, 49, 55, 45, 50, 54, 57, 57, 45, 52, 56, 97, 52, 45, 57, 97,
            54, 98, 45, 100, 54, 102, 101, 54, 52, 102, 55, 51, 50, 48, 51, 206, 215, 128, 65, 51, 111, 179, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 48, 52, 50, 99, 101, 55, 52, 56, 45, 53, 56, 54, 102, 45, 52, 101, 97, 50, 45, 57, 49, 49, 57, 45, 48, 49, 55, 101, 55,
            54, 56, 54, 101, 49, 50, 99, 68, 246, 112, 65, 69, 208, 192, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 53, 55, 49, 56, 54, 48,
            51, 45, 49, 98, 102, 97, 45, 52, 54, 51, 54, 45, 97, 102, 101, 51, 45, 52, 51, 52, 52, 97, 98, 97, 55, 102, 56, 55, 49, 61,
            165, 92, 65, 235, 164, 204, 65, 0, 0, 0, 0, 1, 0, 36, 0, 98, 97, 102, 54, 48, 57, 55, 97, 45, 55, 99, 100, 99, 45, 52,
            102, 53, 50, 45, 98, 100, 49, 55, 45, 98, 53, 56, 52, 56, 49, 102, 51, 56, 101, 100, 50, 240, 251, 68, 65, 183, 28, 215, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 53, 51, 54, 101, 57, 55, 99, 50, 45, 56, 102, 50, 101, 45, 52, 52, 101, 54, 45, 98, 99, 102, 100,
            45, 48, 52, 53, 57, 51, 55, 55, 56, 56, 57, 102, 50, 94, 216, 42, 65, 173, 200, 223, 65, 0, 0, 0, 0, 1, 0, 36, 0, 52,
            101, 102, 49, 52, 97, 55, 50, 45, 100, 51, 56, 99, 45, 52, 49, 52, 55, 45, 97, 48, 53, 99, 45, 55, 50, 97, 102, 52, 49, 101,
            57, 97, 56, 99, 55, 97, 185, 14, 65, 161, 184, 230, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 51, 53, 99, 57, 50, 98, 99, 45,
            52, 49, 54, 50, 45, 52, 52, 51, 55, 45, 57, 54, 53, 57, 45, 50, 52, 53, 54, 49, 101, 100, 100, 98, 99, 53, 102, 166, 59, 226,
            64, 154, 236, 235, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 51, 53, 55, 49, 54, 101, 54, 45, 102, 54, 49, 98, 45, 52, 54, 49,
            55, 45, 97, 48, 55, 52, 45, 56, 50, 100, 53, 51, 99, 50, 98, 98, 55, 100, 98, 32, 9, 165, 64, 113, 116, 239, 65, 0, 0, 0,
            0, 1, 0, 36, 0, 102, 52, 52, 99, 56, 98, 99, 49, 45, 52, 49, 102, 50, 45, 52, 57, 48, 55, 45, 56, 97, 48, 100, 45, 51,
            54, 52, 49, 98, 102, 102, 52, 56, 48, 53, 50, 202, 177, 77, 64, 220, 111, 241, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 51, 57,
            55, 101, 99, 52, 56, 45, 55, 53, 100, 99, 45, 52, 57, 53, 99, 45, 97, 55, 50, 57, 45, 98, 97, 101, 57, 52, 49, 97, 50, 100,
            99, 56, 53, 222, 249, 30, 193, 67, 102, 170, 64, 0, 0, 0, 0, 1, 0, 36, 0, 54, 56, 102, 100, 99, 99, 48, 53, 45, 101, 49,
            53, 51, 45, 52, 99, 102, 98, 45, 57, 57, 48, 52, 45, 57, 49, 99, 56, 98, 100, 99, 53, 98, 100, 56, 52, 35, 249, 60, 193, 30,
            149, 189, 64, 0, 0, 0, 0, 1, 0, 36, 0, 56, 98, 98, 52, 98, 48, 100, 101, 45, 54, 97, 49, 55, 45, 52, 101, 53, 49, 45,
            98, 55, 50, 52, 45, 53, 99, 98, 48, 55, 99, 100, 99, 50, 56, 57, 99, 73, 243, 88, 193, 126, 252, 217, 64, 0, 0, 0, 0, 1,
            0, 36, 0, 51, 52, 53, 57, 97, 56, 53, 51, 45, 100, 56, 102, 48, 45, 52, 99, 100, 53, 45, 57, 56, 102, 54, 45, 98, 51, 102,
            99, 51, 52, 53, 57, 49, 52, 56, 49, 115, 0, 114, 193, 60, 105, 255, 64, 0, 0, 0, 0, 1, 0, 36, 0, 99, 49, 54, 50, 51,
            101, 48, 99, 45, 101, 54, 57, 48, 45, 52, 48, 97, 49, 45, 98, 101, 52, 98, 45, 102, 101, 102, 98, 50, 53, 52, 102, 101, 48, 55,
            101, 125, 147, 131, 193, 119, 206, 22, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 53, 49, 50, 49, 97, 53, 99, 45, 49, 100, 56, 55,
            45, 52, 101, 57, 50, 45, 56, 100, 49, 97, 45, 54, 48, 100, 100, 97, 55, 48, 54, 102, 100, 50, 51, 206, 213, 139, 193, 84, 113, 49,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 57, 50, 99, 53, 57, 100, 57, 45, 102, 48, 55, 51, 45, 52, 97, 53, 53, 45, 98, 100,
            55, 49, 45, 99, 55, 54, 52, 51, 51, 49, 48, 97, 101, 102, 48, 143, 144, 145, 193, 194, 194, 78, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 100, 56, 56, 50, 49, 52, 53, 49, 45, 102, 49, 57, 50, 45, 52, 51, 52, 49, 45, 57, 49, 51, 49, 45, 55, 50, 97, 53, 101,
            101, 50, 101, 102, 56, 57, 52, 39, 180, 148, 193, 68, 76, 109, 65, 0, 0, 0, 0, 1, 0, 36, 0, 50, 50, 53, 57, 52, 53, 97,
            53, 45, 57, 56, 99, 98, 45, 52, 98, 98, 97, 45, 98, 55, 100, 49, 45, 101, 56, 98, 100, 100, 52, 97, 51, 52, 97, 57, 57, 49,
            80, 149, 193, 235, 95, 134, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 97, 56, 51, 48, 54, 51, 99, 45, 101, 100, 101, 99, 45, 52,
            102, 57, 53, 45, 97, 98, 101, 49, 45, 51, 54, 54, 53, 49, 51, 98, 97, 55, 102, 99, 48, 2, 48, 143, 193, 60, 24, 165, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 102, 57, 100, 55, 52, 98, 55, 98, 45, 48, 97, 97, 55, 45, 52, 51, 101, 51, 45, 57, 102, 102, 50,
            45, 101, 56, 56, 98, 51, 101, 51, 51, 97, 49, 98, 50, 19, 124, 147, 193, 75, 2, 150, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53,
            53, 53, 100, 54, 53, 50, 102, 45, 101, 52, 56, 100, 45, 52, 52, 98, 57, 45, 56, 101, 48, 100, 45, 48, 98, 49, 97, 52, 56, 52,
            53, 54, 49, 55, 55, 208, 232, 136, 193, 32, 107, 179, 65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 49, 100, 49, 54, 101, 49, 54, 45,
            56, 48, 55, 51, 45, 52, 102, 55, 51, 45, 97, 51, 57, 102, 45, 99, 100, 99, 52, 98, 52, 48, 57, 55, 102, 101, 102, 175, 119, 128,
            193, 92, 196, 192, 65, 0, 0, 0, 0, 1, 0, 36, 0, 50, 48, 99, 52, 48, 48, 49, 98, 45, 54, 57, 50, 97, 45, 52, 51, 52,
            98, 45, 98, 48, 53, 50, 45, 56, 99, 51, 52, 51, 57, 97, 51, 100, 52, 57, 102, 232, 178, 108, 193, 26, 167, 204, 65, 0, 0, 0,
            0, 1, 0, 36, 0, 52, 54, 55, 100, 53, 101, 51, 48, 45, 56, 49, 97, 50, 45, 52, 50, 51, 99, 45, 97, 52, 52, 52, 45, 98,
            57, 54, 53, 49, 49, 99, 100, 52, 51, 53, 98, 59, 28, 85, 193, 90, 19, 215, 65, 0, 0, 0, 0, 1, 0, 36, 0, 101, 102, 52,
            50, 102, 101, 57, 51, 45, 57, 100, 55, 53, 45, 52, 52, 101, 50, 45, 57, 49, 56, 53, 45, 99, 98, 56, 52, 54, 49, 55, 50, 100,
            52, 48, 98, 152, 230, 58, 193, 230, 194, 223, 65, 0, 0, 0, 0, 1, 0, 36, 0, 52, 53, 49, 57, 102, 102, 99, 53, 45, 99, 52,
            98, 51, 45, 52, 100, 98, 98, 45, 56, 52, 56, 53, 45, 54, 57, 99, 98, 48, 49, 54, 97, 51, 102, 57, 100, 162, 189, 30, 193, 136,
            189, 230, 65, 0, 0, 0, 0, 1, 0, 36, 0, 52, 52, 97, 52, 98, 51, 100, 100, 45, 99, 101, 102, 100, 45, 52, 98, 50, 100, 45,
            56, 50, 101, 99, 45, 101, 52, 49, 51, 49, 100, 51, 48, 98, 53, 100, 101, 100, 61, 1, 193, 169, 243, 235, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 48, 101, 98, 48, 101, 55, 54, 100, 45, 49, 51, 102, 48, 45, 52, 54, 99, 53, 45, 56, 53, 53, 101, 45, 49, 98, 57,
            53, 49, 100, 102, 49, 48, 54, 49, 100, 36, 10, 197, 192, 175, 124, 239, 65, 0, 0, 0, 0, 1, 0, 36, 0, 55, 55, 57, 51, 100,
            56, 50, 54, 45, 100, 101, 54, 101, 45, 52, 100, 99, 97, 45, 56, 56, 51, 99, 45, 99, 98, 99, 56, 50, 55, 102, 53, 54, 51, 51,
            99, 65, 222, 134, 192, 206, 119, 241, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57, 48, 100, 50, 53, 53, 52, 51, 45, 53, 53, 53, 97,
            45, 52, 55, 102, 101, 45, 57, 56, 97, 56, 45, 56, 101, 50, 52, 102, 101, 98, 97, 52, 100, 97, 53, 0, 0, 0, 0, 9, 0, 0,
            0, 0, 0, 0, 0, 44, 0, 0, 0, 1, 0, 0, 0, 36, 0, 102, 55, 101, 51, 53, 57, 49, 56, 45, 53, 49, 100, 101, 45, 52,
            55, 101, 53, 45, 97, 56, 99, 98, 45, 101, 51, 97, 55, 98, 97, 99, 102, 102, 102, 51, 54, 36, 0, 53, 51, 57, 55, 101, 99, 52,
            56, 45, 55, 53, 100, 99, 45, 52, 57, 53, 99, 45, 97, 55, 50, 57, 45, 98, 97, 101, 57, 52, 49, 97, 50, 100, 99, 56, 53, 1,
            0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 53, 51, 57, 55, 101, 99, 52, 56, 45, 55, 53, 100, 99, 45, 52, 57, 53,
            99, 45, 97, 55, 50, 57, 45, 98, 97, 101, 57, 52, 49, 97, 50, 100, 99, 56, 53, 36, 0, 102, 52, 52, 99, 56, 98, 99, 49, 45,
            52, 49, 102, 50, 45, 52, 57, 48, 55, 45, 56, 97, 48, 100, 45, 51, 54, 52, 49, 98, 102, 102, 52, 56, 48, 53, 50, 1, 0, 0,
            0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 102, 52, 52, 99, 56, 98, 99, 49, 45, 52, 49, 102, 50, 45, 52, 57, 48, 55, 45,
            56, 97, 48, 100, 45, 51, 54, 52, 49, 98, 102, 102, 52, 56, 48, 53, 50, 36, 0, 53, 51, 53, 55, 49, 54, 101, 54, 45, 102, 54,
            49, 98, 45, 52, 54, 49, 55, 45, 97, 48, 55, 52, 45, 56, 50, 100, 53, 51, 99, 50, 98, 98, 55, 100, 98, 1, 0, 0, 0, 1,
            0, 0, 0, 1, 0, 0, 0, 36, 0, 53, 51, 53, 55, 49, 54, 101, 54, 45, 102, 54, 49, 98, 45, 52, 54, 49, 55, 45, 97, 48,
            55, 52, 45, 56, 50, 100, 53, 51, 99, 50, 98, 98, 55, 100, 98, 36, 0, 49, 51, 53, 99, 57, 50, 98, 99, 45, 52, 49, 54, 50,
            45, 52, 52, 51, 55, 45, 57, 54, 53, 57, 45, 50, 52, 53, 54, 49, 101, 100, 100, 98, 99, 53, 102, 1, 0, 0, 0, 1, 0, 0,
            0, 1, 0, 0, 0, 36, 0, 49, 51, 53, 99, 57, 50, 98, 99, 45, 52, 49, 54, 50, 45, 52, 52, 51, 55, 45, 57, 54, 53, 57,
            45, 50, 52, 53, 54, 49, 101, 100, 100, 98, 99, 53, 102, 36, 0, 52, 101, 102, 49, 52, 97, 55, 50, 45, 100, 51, 56, 99, 45, 52,
            49, 52, 55, 45, 97, 48, 53, 99, 45, 55, 50, 97, 102, 52, 49, 101, 57, 97, 56, 99, 55, 1, 0, 0, 0, 1, 0, 0, 0, 2,
            0, 0, 0, 36, 0, 52, 101, 102, 49, 52, 97, 55, 50, 45, 100, 51, 56, 99, 45, 52, 49, 52, 55, 45, 97, 48, 53, 99, 45, 55,
            50, 97, 102, 52, 49, 101, 57, 97, 56, 99, 55, 36, 0, 53, 51, 54, 101, 57, 55, 99, 50, 45, 56, 102, 50, 101, 45, 52, 52, 101,
            54, 45, 98, 99, 102, 100, 45, 48, 52, 53, 57, 51, 55, 55, 56, 56, 57, 102, 50, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0,
            0, 36, 0, 53, 51, 54, 101, 57, 55, 99, 50, 45, 56, 102, 50, 101, 45, 52, 52, 101, 54, 45, 98, 99, 102, 100, 45, 48, 52, 53,
            57, 51, 55, 55, 56, 56, 57, 102, 50, 36, 0, 98, 97, 102, 54, 48, 57, 55, 97, 45, 55, 99, 100, 99, 45, 52, 102, 53, 50, 45,
            98, 100, 49, 55, 45, 98, 53, 56, 52, 56, 49, 102, 51, 56, 101, 100, 50, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36,
            0, 98, 97, 102, 54, 48, 57, 55, 97, 45, 55, 99, 100, 99, 45, 52, 102, 53, 50, 45, 98, 100, 49, 55, 45, 98, 53, 56, 52, 56,
            49, 102, 51, 56, 101, 100, 50, 36, 0, 97, 53, 55, 49, 56, 54, 48, 51, 45, 49, 98, 102, 97, 45, 52, 54, 51, 54, 45, 97, 102,
            101, 51, 45, 52, 51, 52, 52, 97, 98, 97, 55, 102, 56, 55, 49, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 97,
            53, 55, 49, 56, 54, 48, 51, 45, 49, 98, 102, 97, 45, 52, 54, 51, 54, 45, 97, 102, 101, 51, 45, 52, 51, 52, 52, 97, 98, 97,
            55, 102, 56, 55, 49, 36, 0, 48, 52, 50, 99, 101, 55, 52, 56, 45, 53, 56, 54, 102, 45, 52, 101, 97, 50, 45, 57, 49, 49, 57,
            45, 48, 49, 55, 101, 55, 54, 56, 54, 101, 49, 50, 99, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 48, 52, 50,
            99, 101, 55, 52, 56, 45, 53, 56, 54, 102, 45, 52, 101, 97, 50, 45, 57, 49, 49, 57, 45, 48, 49, 55, 101, 55, 54, 56, 54, 101,
            49, 50, 99, 36, 0, 97, 52, 53, 53, 101, 97, 49, 55, 45, 50, 54, 57, 57, 45, 52, 56, 97, 52, 45, 57, 97, 54, 98, 45, 100,
            54, 102, 101, 54, 52, 102, 55, 51, 50, 48, 51, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 97, 52, 53, 53, 101,
            97, 49, 55, 45, 50, 54, 57, 57, 45, 52, 56, 97, 52, 45, 57, 97, 54, 98, 45, 100, 54, 102, 101, 54, 52, 102, 55, 51, 50, 48,
            51, 36, 0, 48, 102, 52, 100, 101, 51, 102, 55, 45, 54, 49, 50, 98, 45, 52, 98, 48, 101, 45, 98, 54, 98, 97, 45, 50, 51, 54,
            56, 56, 56, 49, 100, 56, 100, 50, 56, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 48, 102, 52, 100, 101, 51, 102,
            55, 45, 54, 49, 50, 98, 45, 52, 98, 48, 101, 45, 98, 54, 98, 97, 45, 50, 51, 54, 56, 56, 56, 49, 100, 56, 100, 50, 56, 36,
            0, 100, 99, 51, 56, 101, 101, 54, 54, 45, 102, 48, 54, 97, 45, 52, 100, 97, 48, 45, 56, 55, 101, 97, 45, 55, 51, 54, 56, 50,
            49, 52, 56, 99, 55, 98, 48, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 100, 99, 51, 56, 101, 101, 54, 54, 45,
            102, 48, 54, 97, 45, 52, 100, 97, 48, 45, 56, 55, 101, 97, 45, 55, 51, 54, 56, 50, 49, 52, 56, 99, 55, 98, 48, 36, 0, 101,
            98, 54, 102, 99, 97, 50, 102, 45, 98, 97, 49, 52, 45, 52, 98, 56, 51, 45, 56, 52, 101, 98, 45, 99, 57, 98, 101, 49, 99, 97,
            56, 97, 99, 48, 102, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 101, 98, 54, 102, 99, 97, 50, 102, 45, 98, 97,
            49, 52, 45, 52, 98, 56, 51, 45, 56, 52, 101, 98, 45, 99, 57, 98, 101, 49, 99, 97, 56, 97, 99, 48, 102, 36, 0, 97, 54, 102,
            51, 56, 101, 99, 48, 45, 53, 101, 97, 50, 45, 52, 53, 57, 51, 45, 97, 56, 100, 57, 45, 49, 52, 97, 101, 56, 50, 49, 98, 52,
            102, 53, 98, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 97, 54, 102, 51, 56, 101, 99, 48, 45, 53, 101, 97, 50,
            45, 52, 53, 57, 51, 45, 97, 56, 100, 57, 45, 49, 52, 97, 101, 56, 50, 49, 98, 52, 102, 53, 98, 36, 0, 102, 50, 97, 49, 53,
            48, 48, 50, 45, 98, 97, 100, 97, 45, 52, 102, 51, 51, 45, 97, 50, 49, 99, 45, 97, 48, 50, 54, 57, 48, 54, 101, 99, 99, 48,
            57, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 102, 50, 97, 49, 53, 48, 48, 50, 45, 98, 97, 100, 97, 45, 52,
            102, 51, 51, 45, 97, 50, 49, 99, 45, 97, 48, 50, 54, 57, 48, 54, 101, 99, 99, 48, 57, 36, 0, 49, 101, 100, 101, 56, 50, 99,
            55, 45, 102, 100, 99, 48, 45, 52, 98, 101, 97, 45, 98, 57, 97, 102, 45, 49, 48, 100, 54, 102, 57, 51, 51, 102, 50, 49, 56, 1,
            0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 49, 101, 100, 101, 56, 50, 99, 55, 45, 102, 100, 99, 48, 45, 52, 98, 101,
            97, 45, 98, 57, 97, 102, 45, 49, 48, 100, 54, 102, 57, 51, 51, 102, 50, 49, 56, 36, 0, 97, 52, 102, 54, 99, 53, 53, 51, 45,
            101, 100, 99, 101, 45, 52, 100, 51, 54, 45, 97, 55, 102, 101, 45, 98, 98, 48, 56, 56, 100, 56, 54, 97, 57, 55, 99, 1, 0, 0,
            0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 97, 52, 102, 54, 99, 53, 53, 51, 45, 101, 100, 99, 101, 45, 52, 100, 51, 54, 45,
            97, 55, 102, 101, 45, 98, 98, 48, 56, 56, 100, 56, 54, 97, 57, 55, 99, 36, 0, 50, 102, 57, 55, 53, 57, 54, 50, 45, 57, 57,
            99, 99, 45, 52, 48, 99, 102, 45, 97, 54, 97, 48, 45, 51, 50, 97, 49, 48, 54, 56, 52, 98, 100, 97, 55, 1, 0, 0, 0, 1,
            0, 0, 0, 2, 0, 0, 0, 36, 0, 50, 102, 57, 55, 53, 57, 54, 50, 45, 57, 57, 99, 99, 45, 52, 48, 99, 102, 45, 97, 54,
            97, 48, 45, 51, 50, 97, 49, 48, 54, 56, 52, 98, 100, 97, 55, 36, 0, 99, 99, 99, 102, 53, 48, 51, 97, 45, 97, 102, 51, 97,
            45, 52, 99, 53, 100, 45, 57, 97, 48, 54, 45, 52, 102, 102, 52, 99, 56, 52, 102, 48, 56, 102, 98, 1, 0, 0, 0, 1, 0, 0,
            0, 1, 0, 0, 0, 36, 0, 54, 57, 56, 52, 51, 50, 99, 49, 45, 50, 56, 54, 51, 45, 52, 50, 54, 48, 45, 56, 102, 51, 97,
            45, 49, 57, 49, 49, 99, 99, 50, 52, 99, 52, 57, 53, 36, 0, 50, 98, 52, 99, 98, 56, 102, 102, 45, 101, 54, 55, 100, 45, 52,
            100, 97, 97, 45, 57, 49, 55, 100, 45, 56, 97, 57, 101, 100, 51, 53, 53, 48, 48, 97, 98, 1, 0, 0, 0, 1, 0, 0, 0, 1,
            0, 0, 0, 36, 0, 50, 98, 52, 99, 98, 56, 102, 102, 45, 101, 54, 55, 100, 45, 52, 100, 97, 97, 45, 57, 49, 55, 100, 45, 56,
            97, 57, 101, 100, 51, 53, 53, 48, 48, 97, 98, 36, 0, 99, 99, 99, 102, 53, 48, 51, 97, 45, 97, 102, 51, 97, 45, 52, 99, 53,
            100, 45, 57, 97, 48, 54, 45, 52, 102, 102, 52, 99, 56, 52, 102, 48, 56, 102, 98, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0,
            0, 36, 0, 102, 55, 101, 51, 53, 57, 49, 56, 45, 53, 49, 100, 101, 45, 52, 55, 101, 53, 45, 97, 56, 99, 98, 45, 101, 51, 97,
            55, 98, 97, 99, 102, 102, 102, 51, 54, 36, 0, 54, 97, 57, 97, 101, 97, 101, 100, 45, 99, 97, 98, 53, 45, 52, 56, 51, 98, 45,
            97, 97, 97, 50, 45, 101, 48, 56, 99, 102, 49, 55, 100, 97, 102, 57, 56, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36,
            0, 54, 97, 57, 97, 101, 97, 101, 100, 45, 99, 97, 98, 53, 45, 52, 56, 51, 98, 45, 97, 97, 97, 50, 45, 101, 48, 56, 99, 102,
            49, 55, 100, 97, 102, 57, 56, 36, 0, 57, 49, 100, 57, 51, 98, 48, 97, 45, 53, 55, 53, 101, 45, 52, 98, 97, 53, 45, 97, 49,
            57, 56, 45, 49, 55, 49, 49, 50, 98, 48, 98, 48, 54, 51, 100, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 57,
            49, 100, 57, 51, 98, 48, 97, 45, 53, 55, 53, 101, 45, 52, 98, 97, 53, 45, 97, 49, 57, 56, 45, 49, 55, 49, 49, 50, 98, 48,
            98, 48, 54, 51, 100, 36, 0, 57, 48, 100, 50, 53, 53, 52, 51, 45, 53, 53, 53, 97, 45, 52, 55, 102, 101, 45, 57, 56, 97, 56,
            45, 56, 101, 50, 52, 102, 101, 98, 97, 52, 100, 97, 53, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 57, 48, 100,
            50, 53, 53, 52, 51, 45, 53, 53, 53, 97, 45, 52, 55, 102, 101, 45, 57, 56, 97, 56, 45, 56, 101, 50, 52, 102, 101, 98, 97, 52,
            100, 97, 53, 36, 0, 55, 55, 57, 51, 100, 56, 50, 54, 45, 100, 101, 54, 101, 45, 52, 100, 99, 97, 45, 56, 56, 51, 99, 45, 99,
            98, 99, 56, 50, 55, 102, 53, 54, 51, 51, 99, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 55, 55, 57, 51, 100,
            56, 50, 54, 45, 100, 101, 54, 101, 45, 52, 100, 99, 97, 45, 56, 56, 51, 99, 45, 99, 98, 99, 56, 50, 55, 102, 53, 54, 51, 51,
            99, 36, 0, 48, 101, 98, 48, 101, 55, 54, 100, 45, 49, 51, 102, 48, 45, 52, 54, 99, 53, 45, 56, 53, 53, 101, 45, 49, 98, 57,
            53, 49, 100, 102, 49, 48, 54, 49, 100, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 48, 101, 98, 48, 101, 55, 54,
            100, 45, 49, 51, 102, 48, 45, 52, 54, 99, 53, 45, 56, 53, 53, 101, 45, 49, 98, 57, 53, 49, 100, 102, 49, 48, 54, 49, 100, 36,
            0, 52, 52, 97, 52, 98, 51, 100, 100, 45, 99, 101, 102, 100, 45, 52, 98, 50, 100, 45, 56, 50, 101, 99, 45, 101, 52, 49, 51, 49,
            100, 51, 48, 98, 53, 100, 101, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 52, 52, 97, 52, 98, 51, 100, 100, 45,
            99, 101, 102, 100, 45, 52, 98, 50, 100, 45, 56, 50, 101, 99, 45, 101, 52, 49, 51, 49, 100, 51, 48, 98, 53, 100, 101, 36, 0, 52,
            53, 49, 57, 102, 102, 99, 53, 45, 99, 52, 98, 51, 45, 52, 100, 98, 98, 45, 56, 52, 56, 53, 45, 54, 57, 99, 98, 48, 49, 54,
            97, 51, 102, 57, 100, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 52, 53, 49, 57, 102, 102, 99, 53, 45, 99, 52,
            98, 51, 45, 52, 100, 98, 98, 45, 56, 52, 56, 53, 45, 54, 57, 99, 98, 48, 49, 54, 97, 51, 102, 57, 100, 36, 0, 101, 102, 52,
            50, 102, 101, 57, 51, 45, 57, 100, 55, 53, 45, 52, 52, 101, 50, 45, 57, 49, 56, 53, 45, 99, 98, 56, 52, 54, 49, 55, 50, 100,
            52, 48, 98, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 101, 102, 52, 50, 102, 101, 57, 51, 45, 57, 100, 55, 53,
            45, 52, 52, 101, 50, 45, 57, 49, 56, 53, 45, 99, 98, 56, 52, 54, 49, 55, 50, 100, 52, 48, 98, 36, 0, 52, 54, 55, 100, 53,
            101, 51, 48, 45, 56, 49, 97, 50, 45, 52, 50, 51, 99, 45, 97, 52, 52, 52, 45, 98, 57, 54, 53, 49, 49, 99, 100, 52, 51, 53,
            98, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 52, 54, 55, 100, 53, 101, 51, 48, 45, 56, 49, 97, 50, 45, 52,
            50, 51, 99, 45, 97, 52, 52, 52, 45, 98, 57, 54, 53, 49, 49, 99, 100, 52, 51, 53, 98, 36, 0, 50, 48, 99, 52, 48, 48, 49,
            98, 45, 54, 57, 50, 97, 45, 52, 51, 52, 98, 45, 98, 48, 53, 50, 45, 56, 99, 51, 52, 51, 57, 97, 51, 100, 52, 57, 102, 1,
            0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 50, 48, 99, 52, 48, 48, 49, 98, 45, 54, 57, 50, 97, 45, 52, 51, 52,
            98, 45, 98, 48, 53, 50, 45, 56, 99, 51, 52, 51, 57, 97, 51, 100, 52, 57, 102, 36, 0, 100, 49, 100, 49, 54, 101, 49, 54, 45,
            56, 48, 55, 51, 45, 52, 102, 55, 51, 45, 97, 51, 57, 102, 45, 99, 100, 99, 52, 98, 52, 48, 57, 55, 102, 101, 102, 1, 0, 0,
            0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 100, 49, 100, 49, 54, 101, 49, 54, 45, 56, 48, 55, 51, 45, 52, 102, 55, 51, 45,
            97, 51, 57, 102, 45, 99, 100, 99, 52, 98, 52, 48, 57, 55, 102, 101, 102, 36, 0, 102, 57, 100, 55, 52, 98, 55, 98, 45, 48, 97,
            97, 55, 45, 52, 51, 101, 51, 45, 57, 102, 102, 50, 45, 101, 56, 56, 98, 51, 101, 51, 51, 97, 49, 98, 50, 1, 0, 0, 0, 1,
            0, 0, 0, 1, 0, 0, 0, 36, 0, 102, 57, 100, 55, 52, 98, 55, 98, 45, 48, 97, 97, 55, 45, 52, 51, 101, 51, 45, 57, 102,
            102, 50, 45, 101, 56, 56, 98, 51, 101, 51, 51, 97, 49, 98, 50, 36, 0, 53, 53, 53, 100, 54, 53, 50, 102, 45, 101, 52, 56, 100,
            45, 52, 52, 98, 57, 45, 56, 101, 48, 100, 45, 48, 98, 49, 97, 52, 56, 52, 53, 54, 49, 55, 55, 1, 0, 0, 0, 1, 0, 0,
            0, 2, 0, 0, 0, 36, 0, 53, 53, 53, 100, 54, 53, 50, 102, 45, 101, 52, 56, 100, 45, 52, 52, 98, 57, 45, 56, 101, 48, 100,
            45, 48, 98, 49, 97, 52, 56, 52, 53, 54, 49, 55, 55, 36, 0, 49, 97, 56, 51, 48, 54, 51, 99, 45, 101, 100, 101, 99, 45, 52,
            102, 57, 53, 45, 97, 98, 101, 49, 45, 51, 54, 54, 53, 49, 51, 98, 97, 55, 102, 99, 48, 1, 0, 0, 0, 1, 0, 0, 0, 2,
            0, 0, 0, 36, 0, 49, 97, 56, 51, 48, 54, 51, 99, 45, 101, 100, 101, 99, 45, 52, 102, 57, 53, 45, 97, 98, 101, 49, 45, 51,
            54, 54, 53, 49, 51, 98, 97, 55, 102, 99, 48, 36, 0, 50, 50, 53, 57, 52, 53, 97, 53, 45, 57, 56, 99, 98, 45, 52, 98, 98,
            97, 45, 98, 55, 100, 49, 45, 101, 56, 98, 100, 100, 52, 97, 51, 52, 97, 57, 57, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0,
            0, 36, 0, 50, 50, 53, 57, 52, 53, 97, 53, 45, 57, 56, 99, 98, 45, 52, 98, 98, 97, 45, 98, 55, 100, 49, 45, 101, 56, 98,
            100, 100, 52, 97, 51, 52, 97, 57, 57, 36, 0, 100, 56, 56, 50, 49, 52, 53, 49, 45, 102, 49, 57, 50, 45, 52, 51, 52, 49, 45,
            57, 49, 51, 49, 45, 55, 50, 97, 53, 101, 101, 50, 101, 102, 56, 57, 52, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36,
            0, 100, 56, 56, 50, 49, 52, 53, 49, 45, 102, 49, 57, 50, 45, 52, 51, 52, 49, 45, 57, 49, 51, 49, 45, 55, 50, 97, 53, 101,
            101, 50, 101, 102, 56, 57, 52, 36, 0, 100, 57, 50, 99, 53, 57, 100, 57, 45, 102, 48, 55, 51, 45, 52, 97, 53, 53, 45, 98, 100,
            55, 49, 45, 99, 55, 54, 52, 51, 51, 49, 48, 97, 101, 102, 48, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 100,
            57, 50, 99, 53, 57, 100, 57, 45, 102, 48, 55, 51, 45, 52, 97, 53, 53, 45, 98, 100, 55, 49, 45, 99, 55, 54, 52, 51, 51, 49,
            48, 97, 101, 102, 48, 36, 0, 53, 53, 49, 50, 49, 97, 53, 99, 45, 49, 100, 56, 55, 45, 52, 101, 57, 50, 45, 56, 100, 49, 97,
            45, 54, 48, 100, 100, 97, 55, 48, 54, 102, 100, 50, 51, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 53, 53, 49,
            50, 49, 97, 53, 99, 45, 49, 100, 56, 55, 45, 52, 101, 57, 50, 45, 56, 100, 49, 97, 45, 54, 48, 100, 100, 97, 55, 48, 54, 102,
            100, 50, 51, 36, 0, 99, 49, 54, 50, 51, 101, 48, 99, 45, 101, 54, 57, 48, 45, 52, 48, 97, 49, 45, 98, 101, 52, 98, 45, 102,
            101, 102, 98, 50, 53, 52, 102, 101, 48, 55, 101, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 99, 49, 54, 50, 51,
            101, 48, 99, 45, 101, 54, 57, 48, 45, 52, 48, 97, 49, 45, 98, 101, 52, 98, 45, 102, 101, 102, 98, 50, 53, 52, 102, 101, 48, 55,
            101, 36, 0, 51, 52, 53, 57, 97, 56, 53, 51, 45, 100, 56, 102, 48, 45, 52, 99, 100, 53, 45, 57, 56, 102, 54, 45, 98, 51, 102,
            99, 51, 52, 53, 57, 49, 52, 56, 49, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 51, 52, 53, 57, 97, 56, 53,
            51, 45, 100, 56, 102, 48, 45, 52, 99, 100, 53, 45, 57, 56, 102, 54, 45, 98, 51, 102, 99, 51, 52, 53, 57, 49, 52, 56, 49, 36,
            0, 56, 98, 98, 52, 98, 48, 100, 101, 45, 54, 97, 49, 55, 45, 52, 101, 53, 49, 45, 98, 55, 50, 52, 45, 53, 99, 98, 48, 55,
            99, 100, 99, 50, 56, 57, 99, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 56, 98, 98, 52, 98, 48, 100, 101, 45,
            54, 97, 49, 55, 45, 52, 101, 53, 49, 45, 98, 55, 50, 52, 45, 53, 99, 98, 48, 55, 99, 100, 99, 50, 56, 57, 99, 36, 0, 54,
            56, 102, 100, 99, 99, 48, 53, 45, 101, 49, 53, 51, 45, 52, 99, 102, 98, 45, 57, 57, 48, 52, 45, 57, 49, 99, 56, 98, 100, 99,
            53, 98, 100, 56, 52, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 54, 56, 102, 100, 99, 99, 48, 53, 45, 101, 49,
            53, 51, 45, 52, 99, 102, 98, 45, 57, 57, 48, 52, 45, 57, 49, 99, 56, 98, 100, 99, 53, 98, 100, 56, 52, 36, 0, 53, 55, 99,
            48, 98, 98, 102, 56, 45, 50, 55, 97, 52, 45, 52, 55, 99, 57, 45, 97, 97, 57, 56, 45, 56, 102, 97, 101, 100, 98, 53, 48, 97,
            51, 56, 52, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 45, 0, 0, 0, 0, 0, 0,
            193, 0, 0, 160, 64, 0, 0, 0, 0, 1, 0, 36, 0, 53, 55, 99, 48, 98, 98, 102, 56, 45, 50, 55, 97, 52, 45, 52, 55, 99,
            57, 45, 97, 97, 57, 56, 45, 56, 102, 97, 101, 100, 98, 53, 48, 97, 51, 56, 52, 0, 0, 224, 64, 0, 0, 160, 64, 0, 0, 0,
            0, 1, 0, 36, 0, 54, 57, 56, 52, 51, 50, 99, 49, 45, 50, 56, 54, 51, 45, 52, 50, 54, 48, 45, 56, 102, 51, 97, 45, 49,
            57, 49, 49, 99, 99, 50, 52, 99, 52, 57, 53, 0, 0, 160, 63, 0, 0, 242, 65, 0, 0, 0, 0, 1, 0, 36, 0, 102, 55, 101,
            51, 53, 57, 49, 56, 45, 53, 49, 100, 101, 45, 52, 55, 101, 53, 45, 97, 56, 99, 98, 45, 101, 51, 97, 55, 98, 97, 99, 102, 102,
            102, 51, 54, 0, 0, 16, 192, 0, 0, 242, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57, 49, 100, 57, 51, 98, 48, 97, 45, 53, 55,
            53, 101, 45, 52, 98, 97, 53, 45, 97, 49, 57, 56, 45, 49, 55, 49, 49, 50, 98, 48, 98, 48, 54, 51, 100, 0, 0, 0, 191, 0,
            0, 242, 65, 0, 0, 0, 0, 1, 0, 36, 0, 54, 97, 57, 97, 101, 97, 101, 100, 45, 99, 97, 98, 53, 45, 52, 56, 51, 98, 45,
            97, 97, 97, 50, 45, 101, 48, 56, 99, 102, 49, 55, 100, 97, 102, 57, 56, 36, 241, 14, 65, 96, 100, 170, 64, 0, 0, 0, 0, 1,
            0, 36, 0, 50, 98, 52, 99, 98, 56, 102, 102, 45, 101, 54, 55, 100, 45, 52, 100, 97, 97, 45, 57, 49, 55, 100, 45, 56, 97, 57,
            101, 100, 51, 53, 53, 48, 48, 97, 98, 74, 214, 44, 65, 118, 171, 189, 64, 0, 0, 0, 0, 1, 0, 36, 0, 99, 99, 99, 102, 53,
            48, 51, 97, 45, 97, 102, 51, 97, 45, 52, 99, 53, 100, 45, 57, 97, 48, 54, 45, 52, 102, 102, 52, 99, 56, 52, 102, 48, 56, 102,
            98, 148, 227, 72, 65, 172, 216, 217, 64, 0, 0, 0, 0, 1, 0, 36, 0, 50, 102, 57, 55, 53, 57, 54, 50, 45, 57, 57, 99, 99,
            45, 52, 48, 99, 102, 45, 97, 54, 97, 48, 45, 51, 50, 97, 49, 48, 54, 56, 52, 98, 100, 97, 55, 222, 242, 97, 65, 144, 127, 255,
            64, 0, 0, 0, 0, 1, 0, 36, 0, 97, 52, 102, 54, 99, 53, 53, 51, 45, 101, 100, 99, 101, 45, 52, 100, 51, 54, 45, 97, 55,
            102, 101, 45, 98, 98, 48, 56, 56, 100, 56, 54, 97, 57, 55, 99, 190, 36, 119, 65, 102, 208, 22, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 49, 101, 100, 101, 56, 50, 99, 55, 45, 102, 100, 99, 48, 45, 52, 98, 101, 97, 45, 98, 57, 97, 102, 45, 49, 48, 100, 54, 102,
            57, 51, 51, 102, 50, 49, 56, 202, 224, 131, 65, 246, 97, 49, 65, 0, 0, 0, 0, 1, 0, 36, 0, 102, 50, 97, 49, 53, 48, 48,
            50, 45, 98, 97, 100, 97, 45, 52, 102, 51, 51, 45, 97, 50, 49, 99, 45, 97, 48, 50, 54, 57, 48, 54, 101, 99, 99, 48, 57, 158,
            147, 137, 65, 22, 190, 78, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 54, 102, 51, 56, 101, 99, 48, 45, 53, 101, 97, 50, 45, 52,
            53, 57, 51, 45, 97, 56, 100, 57, 45, 49, 52, 97, 101, 56, 50, 49, 98, 52, 102, 53, 98, 80, 188, 140, 65, 89, 87, 109, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 101, 98, 54, 102, 99, 97, 50, 102, 45, 98, 97, 49, 52, 45, 52, 98, 56, 51, 45, 56, 52, 101, 98,
            45, 99, 57, 98, 101, 49, 99, 97, 56, 97, 99, 48, 102, 5, 75, 141, 65, 78, 103, 134, 65, 0, 0, 0, 0, 1, 0, 36, 0, 100,
            99, 51, 56, 101, 101, 54, 54, 45, 102, 48, 54, 97, 45, 52, 100, 97, 48, 45, 56, 55, 101, 97, 45, 55, 51, 54, 56, 50, 49, 52,
            56, 99, 55, 98, 48, 82, 111, 139, 65, 56, 3, 150, 65, 0, 0, 0, 0, 1, 0, 36, 0, 48, 102, 52, 100, 101, 51, 102, 55, 45,
            54, 49, 50, 98, 45, 52, 98, 48, 101, 45, 98, 54, 98, 97, 45, 50, 51, 54, 56, 56, 56, 49, 100, 56, 100, 50, 56, 51, 41, 135,
            65, 71, 32, 165, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 52, 53, 53, 101, 97, 49, 55, 45, 50, 54, 57, 57, 45, 52, 56, 97,
            52, 45, 57, 97, 54, 98, 45, 100, 54, 102, 101, 54, 52, 102, 55, 51, 50, 48, 51, 206, 215, 128, 65, 51, 111, 179, 65, 0, 0, 0,
            0, 1, 0, 36, 0, 48, 52, 50, 99, 101, 55, 52, 56, 45, 53, 56, 54, 102, 45, 52, 101, 97, 50, 45, 57, 49, 49, 57, 45, 48,
            49, 55, 101, 55, 54, 56, 54, 101, 49, 50, 99, 68, 246, 112, 65, 69, 208, 192, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 53, 55,
            49, 56, 54, 48, 51, 45, 49, 98, 102, 97, 45, 52, 54, 51, 54, 45, 97, 102, 101, 51, 45, 52, 51, 52, 52, 97, 98, 97, 55, 102,
            56, 55, 49, 61, 165, 92, 65, 235, 164, 204, 65, 0, 0, 0, 0, 1, 0, 36, 0, 98, 97, 102, 54, 48, 57, 55, 97, 45, 55, 99,
            100, 99, 45, 52, 102, 53, 50, 45, 98, 100, 49, 55, 45, 98, 53, 56, 52, 56, 49, 102, 51, 56, 101, 100, 50, 240, 251, 68, 65, 183,
            28, 215, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 51, 54, 101, 57, 55, 99, 50, 45, 56, 102, 50, 101, 45, 52, 52, 101, 54, 45,
            98, 99, 102, 100, 45, 48, 52, 53, 57, 51, 55, 55, 56, 56, 57, 102, 50, 94, 216, 42, 65, 173, 200, 223, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 52, 101, 102, 49, 52, 97, 55, 50, 45, 100, 51, 56, 99, 45, 52, 49, 52, 55, 45, 97, 48, 53, 99, 45, 55, 50, 97,
            102, 52, 49, 101, 57, 97, 56, 99, 55, 97, 185, 14, 65, 161, 184, 230, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 51, 53, 99, 57,
            50, 98, 99, 45, 52, 49, 54, 50, 45, 52, 52, 51, 55, 45, 57, 54, 53, 57, 45, 50, 52, 53, 54, 49, 101, 100, 100, 98, 99, 53,
            102, 166, 59, 226, 64, 154, 236, 235, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 51, 53, 55, 49, 54, 101, 54, 45, 102, 54, 49, 98,
            45, 52, 54, 49, 55, 45, 97, 48, 55, 52, 45, 56, 50, 100, 53, 51, 99, 50, 98, 98, 55, 100, 98, 32, 9, 165, 64, 113, 116, 239,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 102, 52, 52, 99, 56, 98, 99, 49, 45, 52, 49, 102, 50, 45, 52, 57, 48, 55, 45, 56, 97,
            48, 100, 45, 51, 54, 52, 49, 98, 102, 102, 52, 56, 48, 53, 50, 202, 177, 77, 64, 220, 111, 241, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 53, 51, 57, 55, 101, 99, 52, 56, 45, 55, 53, 100, 99, 45, 52, 57, 53, 99, 45, 97, 55, 50, 57, 45, 98, 97, 101, 57, 52,
            49, 97, 50, 100, 99, 56, 53, 222, 249, 30, 193, 67, 102, 170, 64, 0, 0, 0, 0, 1, 0, 36, 0, 54, 56, 102, 100, 99, 99, 48,
            53, 45, 101, 49, 53, 51, 45, 52, 99, 102, 98, 45, 57, 57, 48, 52, 45, 57, 49, 99, 56, 98, 100, 99, 53, 98, 100, 56, 52, 35,
            249, 60, 193, 30, 149, 189, 64, 0, 0, 0, 0, 1, 0, 36, 0, 56, 98, 98, 52, 98, 48, 100, 101, 45, 54, 97, 49, 55, 45, 52,
            101, 53, 49, 45, 98, 55, 50, 52, 45, 53, 99, 98, 48, 55, 99, 100, 99, 50, 56, 57, 99, 73, 243, 88, 193, 126, 252, 217, 64, 0,
            0, 0, 0, 1, 0, 36, 0, 51, 52, 53, 57, 97, 56, 53, 51, 45, 100, 56, 102, 48, 45, 52, 99, 100, 53, 45, 57, 56, 102, 54,
            45, 98, 51, 102, 99, 51, 52, 53, 57, 49, 52, 56, 49, 115, 0, 114, 193, 60, 105, 255, 64, 0, 0, 0, 0, 1, 0, 36, 0, 99,
            49, 54, 50, 51, 101, 48, 99, 45, 101, 54, 57, 48, 45, 52, 48, 97, 49, 45, 98, 101, 52, 98, 45, 102, 101, 102, 98, 50, 53, 52,
            102, 101, 48, 55, 101, 125, 147, 131, 193, 119, 206, 22, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 53, 49, 50, 49, 97, 53, 99, 45,
            49, 100, 56, 55, 45, 52, 101, 57, 50, 45, 56, 100, 49, 97, 45, 54, 48, 100, 100, 97, 55, 48, 54, 102, 100, 50, 51, 206, 213, 139,
            193, 84, 113, 49, 65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 57, 50, 99, 53, 57, 100, 57, 45, 102, 48, 55, 51, 45, 52, 97, 53,
            53, 45, 98, 100, 55, 49, 45, 99, 55, 54, 52, 51, 51, 49, 48, 97, 101, 102, 48, 143, 144, 145, 193, 194, 194, 78, 65, 0, 0, 0,
            0, 1, 0, 36, 0, 100, 56, 56, 50, 49, 52, 53, 49, 45, 102, 49, 57, 50, 45, 52, 51, 52, 49, 45, 57, 49, 51, 49, 45, 55,
            50, 97, 53, 101, 101, 50, 101, 102, 56, 57, 52, 39, 180, 148, 193, 68, 76, 109, 65, 0, 0, 0, 0, 1, 0, 36, 0, 50, 50, 53,
            57, 52, 53, 97, 53, 45, 57, 56, 99, 98, 45, 52, 98, 98, 97, 45, 98, 55, 100, 49, 45, 101, 56, 98, 100, 100, 52, 97, 51, 52,
            97, 57, 57, 49, 80, 149, 193, 235, 95, 134, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 97, 56, 51, 48, 54, 51, 99, 45, 101, 100,
            101, 99, 45, 52, 102, 57, 53, 45, 97, 98, 101, 49, 45, 51, 54, 54, 53, 49, 51, 98, 97, 55, 102, 99, 48, 2, 48, 143, 193, 60,
            24, 165, 65, 0, 0, 0, 0, 1, 0, 36, 0, 102, 57, 100, 55, 52, 98, 55, 98, 45, 48, 97, 97, 55, 45, 52, 51, 101, 51, 45,
            57, 102, 102, 50, 45, 101, 56, 56, 98, 51, 101, 51, 51, 97, 49, 98, 50, 19, 124, 147, 193, 75, 2, 150, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 53, 53, 53, 100, 54, 53, 50, 102, 45, 101, 52, 56, 100, 45, 52, 52, 98, 57, 45, 56, 101, 48, 100, 45, 48, 98, 49,
            97, 52, 56, 52, 53, 54, 49, 55, 55, 208, 232, 136, 193, 32, 107, 179, 65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 49, 100, 49, 54,
            101, 49, 54, 45, 56, 48, 55, 51, 45, 52, 102, 55, 51, 45, 97, 51, 57, 102, 45, 99, 100, 99, 52, 98, 52, 48, 57, 55, 102, 101,
            102, 175, 119, 128, 193, 92, 196, 192, 65, 0, 0, 0, 0, 1, 0, 36, 0, 50, 48, 99, 52, 48, 48, 49, 98, 45, 54, 57, 50, 97,
            45, 52, 51, 52, 98, 45, 98, 48, 53, 50, 45, 56, 99, 51, 52, 51, 57, 97, 51, 100, 52, 57, 102, 232, 178, 108, 193, 26, 167, 204,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 52, 54, 55, 100, 53, 101, 51, 48, 45, 56, 49, 97, 50, 45, 52, 50, 51, 99, 45, 97, 52,
            52, 52, 45, 98, 57, 54, 53, 49, 49, 99, 100, 52, 51, 53, 98, 59, 28, 85, 193, 90, 19, 215, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 101, 102, 52, 50, 102, 101, 57, 51, 45, 57, 100, 55, 53, 45, 52, 52, 101, 50, 45, 57, 49, 56, 53, 45, 99, 98, 56, 52, 54,
            49, 55, 50, 100, 52, 48, 98, 152, 230, 58, 193, 230, 194, 223, 65, 0, 0, 0, 0, 1, 0, 36, 0, 52, 53, 49, 57, 102, 102, 99,
            53, 45, 99, 52, 98, 51, 45, 52, 100, 98, 98, 45, 56, 52, 56, 53, 45, 54, 57, 99, 98, 48, 49, 54, 97, 51, 102, 57, 100, 162,
            189, 30, 193, 136, 189, 230, 65, 0, 0, 0, 0, 1, 0, 36, 0, 52, 52, 97, 52, 98, 51, 100, 100, 45, 99, 101, 102, 100, 45, 52,
            98, 50, 100, 45, 56, 50, 101, 99, 45, 101, 52, 49, 51, 49, 100, 51, 48, 98, 53, 100, 101, 100, 61, 1, 193, 169, 243, 235, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 48, 101, 98, 48, 101, 55, 54, 100, 45, 49, 51, 102, 48, 45, 52, 54, 99, 53, 45, 56, 53, 53, 101,
            45, 49, 98, 57, 53, 49, 100, 102, 49, 48, 54, 49, 100, 36, 10, 197, 192, 175, 124, 239, 65, 0, 0, 0, 0, 1, 0, 36, 0, 55,
            55, 57, 51, 100, 56, 50, 54, 45, 100, 101, 54, 101, 45, 52, 100, 99, 97, 45, 56, 56, 51, 99, 45, 99, 98, 99, 56, 50, 55, 102,
            53, 54, 51, 51, 99, 65, 222, 134, 192, 206, 119, 241, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57, 48, 100, 50, 53, 53, 52, 51, 45,
            53, 53, 53, 97, 45, 52, 55, 102, 101, 45, 57, 56, 97, 56, 45, 56, 101, 50, 52, 102, 101, 98, 97, 52, 100, 97, 53, 0, 0, 0,
            0, 1, 0, 0, 0, 0, 0, 0, 0, 160, 192, 31, 133, 163, 64, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128,
            63, 9, 0, 83, 112, 111, 114, 116, 115, 67, 97, 114, 0, 0, 240, 65, 0, 0, 128, 65, 0, 0, 128, 64, 0, 0, 0, 0, 0, 0,
            200, 66, 0, 0, 0, 0, 0, 0, 200, 66, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 36, 0, 50, 51, 49,
            48, 54, 52, 98, 97, 45, 52, 54, 100, 98, 45, 52, 48, 102, 57, 45, 56, 102, 48, 102, 45, 101, 101, 56, 56, 53, 97, 48, 100, 54,
            101, 54, 55, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 196, 193, 51, 51, 179, 62, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0,
            128, 0, 0, 128, 63, 0, 0, 224, 63, 0, 0, 0, 0, 0, 11, 0, 86, 105, 99, 116, 111, 114, 121, 70, 108, 97, 103, 36, 0, 50,
            51, 49, 48, 54, 52, 98, 97, 45, 52, 54, 100, 98, 45, 52, 48, 102, 57, 45, 56, 102, 48, 102, 45, 101, 101, 56, 56, 53, 97, 48,
            100, 54, 101, 54, 55, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36, 0, 50, 51, 49, 48, 54, 52, 98, 97, 45,
            52, 54, 100, 98, 45, 52, 48, 102, 57, 45, 56, 102, 48, 102, 45, 101, 101, 56, 56, 53, 97, 48, 100, 54, 101, 54, 55, 0, 0, 0,
            0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 84, 101, 114, 114, 97, 105, 110, 95, 66, 111, 111,
            107, 69, 110, 100, 67, 0, 0, 0, 0, 0, 0, 64, 64, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 16, 0, 84, 101, 114, 114, 97, 105, 110, 95, 66, 111, 111, 107, 69, 110, 100, 65, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 159, 134, 1, 0, 100, 0, 0, 0, 100, 0, 0, 0, 100, 0, 0, 0, 0, 0, 0, 0, 100, 0, 0, 0, 100,
            0, 0, 0, 100, 0, 0, 0, 100, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 19, 0, 0, 0, 0, 0, 48, 192, 0, 0,
            48, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63,
            0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 81, 255, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0,
            0, 0, 19, 0, 0, 0, 0, 0, 192, 63, 0, 0, 32, 64, 0, 0, 0, 191, 0, 0, 32, 64, 0, 0, 224, 191, 0, 0, 32, 192,
            0, 0, 160, 191, 0, 0, 32, 192, 0, 0, 128, 190, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0,
            0, 63, 0, 0, 128, 190, 0, 0, 0, 63, 0, 0, 0, 191, 0, 0, 0, 191, 0, 0, 64, 63, 0, 0, 0, 191, 0, 0, 0, 63,
            0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 191, 0, 0, 128, 191, 0, 0, 32, 192, 0, 0, 128, 62, 0, 0, 32, 192, 0, 0,
            64, 63, 0, 0, 32, 192, 128, 131, 158, 63, 192, 227, 11, 191, 0, 0, 160, 63, 0, 0, 0, 0, 0, 0, 192, 63, 0, 0, 0, 63,
            0, 0, 224, 63, 0, 0, 192, 63, 2, 0, 0, 0, 0, 0, 64, 192, 0, 0, 84, 65, 68, 139, 172, 191, 0, 0, 192, 191, 0, 0,
            84, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 0, 191, 0, 0, 68, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128,
            0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
            80, 204, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 0, 191, 0, 0, 32, 64,
            0, 0, 224, 191, 0, 0, 32, 192, 0, 0, 160, 191, 0, 0, 32, 192, 0, 0, 128, 190, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0,
            192, 63, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 128, 190, 0, 0, 0, 63, 0, 0, 0, 191, 0, 0, 0, 191, 0, 0, 128, 62,
            0, 0, 32, 192, 0, 0, 64, 63, 0, 0, 32, 192, 0, 0, 0, 0, 0, 0, 0, 191, 0, 0, 128, 63, 0, 0, 0, 191, 0, 0,
            192, 63, 0, 0, 0, 63, 0, 0, 224, 63, 0, 0, 192, 63, 0, 0, 192, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 0, 191,
            0, 0, 84, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 84, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 64, 63, 0, 0,
            68, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 63,
            0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 96, 204, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0,
            0, 0, 15, 0, 0, 0, 0, 0, 128, 190, 0, 0, 32, 64, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 224, 191, 0, 0, 32, 192,
            0, 0, 160, 191, 0, 0, 32, 192, 0, 0, 128, 190, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0,
            0, 63, 0, 0, 128, 190, 0, 0, 0, 63, 0, 0, 0, 191, 0, 0, 0, 191, 0, 0, 64, 63, 0, 0, 0, 191, 0, 0, 128, 62,
            0, 0, 32, 192, 0, 0, 64, 63, 0, 0, 32, 192, 0, 0, 192, 63, 0, 0, 0, 63, 0, 0, 224, 63, 0, 0, 192, 63, 0, 0,
            192, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 64, 63, 0, 0, 84, 65, 68, 139, 172, 191, 0, 0, 160, 63, 0, 0, 84, 65,
            68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 32, 64, 0, 0, 68, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0,
            0, 128, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 106, 204,
            0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 160, 191, 0, 0, 32, 64, 0, 0,
            224, 191, 0, 0, 192, 63, 0, 0, 48, 192, 0, 0, 32, 192, 0, 0, 16, 192, 0, 0, 32, 192, 0, 0, 160, 191, 0, 0, 192, 63,
            0, 0, 128, 62, 0, 0, 192, 63, 0, 0, 64, 191, 0, 0, 32, 192, 0, 0, 128, 190, 0, 0, 32, 192, 0, 0, 64, 63, 0, 0,
            192, 63, 0, 0, 16, 64, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 32, 192, 0, 0, 224, 63, 0, 0, 32, 192, 0, 0, 48, 64,
            0, 0, 192, 63, 0, 0, 32, 64, 0, 0, 32, 64, 0, 0, 128, 63, 0, 0, 32, 64, 0, 236, 29, 63, 176, 107, 252, 63, 0, 0,
            0, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 0, 64, 0, 0, 84, 65, 68, 139, 172, 191, 0, 0, 96, 64, 0, 0, 84, 65,
            68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 32, 64, 0, 0, 28, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0,
            0, 128, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 187, 221,
            0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 192, 63, 0, 0, 32, 64, 0, 0,
            0, 191, 0, 0, 32, 64, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 0, 63,
            0, 0, 128, 191, 0, 0, 0, 63, 0, 0, 160, 191, 0, 0, 0, 191, 0, 0, 64, 63, 0, 0, 0, 191, 0, 0, 0, 63, 0, 0,
            192, 191, 0, 0, 192, 191, 0, 0, 192, 191, 0, 0, 224, 191, 0, 0, 32, 192, 0, 0, 128, 62, 0, 0, 32, 192, 0, 0, 64, 63,
            0, 0, 32, 192, 0, 132, 158, 63, 192, 227, 11, 191, 0, 0, 160, 63, 0, 0, 0, 0, 0, 0, 192, 63, 0, 0, 0, 63, 0, 0,
            224, 63, 0, 0, 192, 63, 2, 0, 0, 0, 0, 0, 0, 64, 0, 0, 12, 65, 68, 139, 172, 191, 0, 0, 48, 64, 0, 0, 12, 65,
            68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 184, 192, 0, 0, 176, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0,
            0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 238, 0,
            0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 128, 191, 0, 0, 32, 64, 0, 0,
            160, 191, 0, 0, 192, 63, 0, 0, 160, 191, 0, 0, 32, 192, 0, 0, 64, 191, 0, 0, 32, 192, 0, 0, 64, 191, 0, 0, 192, 63,
            0, 0, 64, 63, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0, 0, 63, 0, 0, 0, 191, 0, 0, 0, 63, 0, 0, 0, 191, 0, 0,
            0, 191, 0, 0, 64, 63, 0, 0, 0, 191, 0, 0, 64, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 32, 192, 0, 0, 160, 63,
            0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 160, 192, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0,
            208, 192, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 48, 192, 0, 0, 176, 65, 0, 0, 128, 63, 0, 0, 0, 128,
            0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0,
            0, 0, 0, 0, 221, 0, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 128, 191,
            0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 192, 63, 0, 0, 160, 191, 0, 0, 32, 192, 0, 0, 64, 191, 0, 0, 32, 192, 0, 0,
            64, 191, 0, 0, 192, 63, 0, 0, 0, 191, 0, 0, 192, 63, 0, 0, 128, 190, 0, 0, 0, 63, 0, 0, 128, 62, 0, 0, 0, 63,
            0, 0, 0, 63, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0,
            32, 192, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 32, 64, 0, 0, 128, 62, 0, 0, 32, 64, 0, 0, 0, 0,
            0, 0, 192, 63, 0, 0, 128, 190, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 0, 192, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0,
            96, 192, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 12, 193, 0, 0, 176, 65, 0, 0, 128, 63, 0, 0, 0, 128,
            0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0,
            0, 0, 0, 0, 255, 0, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0, 0, 0, 128, 191,
            0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 0, 64, 0, 0, 160, 191, 0, 0, 0, 192, 0, 0, 128, 191, 0, 0, 32, 192, 0, 0,
            128, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 63,
            0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 24, 193, 0, 0, 194, 65, 68, 139,
            172, 191, 0, 0, 0, 193, 0, 0, 194, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 128, 62, 0, 0, 176, 65, 0, 0, 128, 63,
            0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0,
            1, 0, 0, 0, 0, 0, 0, 0, 204, 0, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0,
            0, 0, 128, 191, 0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 0, 64, 0, 0, 160, 191, 0, 0, 0, 192, 0, 0, 128, 191, 0, 0,
            32, 192, 0, 0, 160, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 191, 0, 0, 64, 191,
            0, 0, 128, 190, 0, 0, 160, 63, 0, 0, 128, 190, 0, 0, 160, 63, 0, 0, 128, 62, 0, 0, 64, 191, 0, 0, 128, 62, 0, 0,
            64, 191, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 128, 63,
            0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 0, 191, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 80, 64, 0, 0,
            176, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63,
            0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 187, 0, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0,
            0, 0, 17, 0, 0, 0, 0, 0, 128, 191, 0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 192, 63, 0, 0, 160, 191, 0, 0, 32, 192,
            0, 0, 64, 191, 0, 0, 32, 192, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0,
            0, 63, 0, 0, 128, 190, 0, 0, 0, 63, 0, 0, 0, 191, 0, 0, 0, 0, 0, 0, 0, 191, 0, 0, 0, 191, 0, 0, 64, 63,
            0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 32, 192, 0, 0, 0, 0, 0, 0, 0, 191, 0, 0, 128, 63, 0, 0, 0, 191, 0, 0,
            160, 63, 0, 0, 0, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 128, 64,
            0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 32, 64, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 200, 64, 0, 0,
            176, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63,
            0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 170, 0, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0,
            0, 0, 14, 0, 0, 0, 0, 0, 128, 191, 0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 192, 63, 0, 0, 160, 191, 0, 0, 32, 192,
            0, 0, 64, 191, 0, 0, 32, 192, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0,
            0, 63, 0, 0, 0, 191, 0, 0, 0, 63, 0, 0, 0, 191, 0, 0, 0, 191, 0, 0, 64, 63, 0, 0, 0, 191, 0, 0, 64, 63,
            0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 32, 64, 2, 0,
            0, 0, 0, 0, 224, 64, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 176, 64, 0, 0, 192, 65, 68, 139, 172, 191, 0, 0, 0, 0,
            0, 0, 112, 64, 0, 0, 134, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0,
            128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 226, 0, 0, 0, 0, 32, 66, 0, 0, 0, 63,
            0, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 128, 191, 0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 192, 63, 0, 0,
            160, 191, 0, 0, 32, 192, 0, 0, 64, 191, 0, 0, 32, 192, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 0, 191, 0, 0, 192, 63,
            0, 0, 128, 190, 0, 0, 0, 63, 0, 0, 128, 62, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 192, 63, 0, 0, 64, 63, 0, 0,
            192, 63, 0, 0, 64, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63,
            0, 0, 32, 64, 0, 0, 128, 62, 0, 0, 32, 64, 0, 0, 0, 0, 0, 0, 192, 63, 0, 0, 128, 190, 0, 0, 32, 64, 2, 0,
            0, 0, 0, 0, 144, 64, 0, 0, 150, 65, 68, 139, 172, 191, 0, 0, 64, 64, 0, 0, 150, 65, 68, 139, 172, 191, 0, 0, 0, 0,
            0, 0, 216, 64, 0, 0, 134, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0,
            128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 226, 0, 0, 0, 0, 32, 66, 0, 0, 0, 63,
            0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, 128, 191, 0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 192, 63, 0, 0,
            160, 191, 0, 0, 192, 191, 0, 0, 128, 191, 0, 0, 32, 192, 0, 0, 128, 63, 0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 192, 191,
            0, 0, 160, 63, 0, 0, 160, 63, 0, 0, 64, 63, 0, 0, 160, 63, 0, 0, 64, 63, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0,
            192, 191, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0, 32, 64, 2, 0, 0, 0,
            0, 0, 192, 64, 0, 0, 152, 65, 68, 139, 172, 191, 0, 0, 240, 64, 0, 0, 152, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0,
            28, 65, 0, 0, 134, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63,
            0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 226, 0, 0, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0,
            0, 0, 0, 0, 0, 0, 11, 0, 0, 0, 0, 0, 160, 191, 0, 0, 32, 64, 0, 0, 160, 191, 0, 0, 32, 192, 0, 0, 128, 63,
            0, 0, 32, 192, 0, 0, 160, 63, 0, 0, 192, 191, 0, 0, 160, 63, 0, 0, 160, 63, 0, 0, 64, 63, 0, 0, 160, 63, 0, 0,
            64, 63, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63,
            0, 0, 128, 63, 0, 0, 32, 64, 2, 0, 0, 0, 0, 0, 16, 65, 0, 0, 152, 65, 68, 139, 172, 191, 0, 0, 40, 65, 0, 0,
            152, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 28, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128,
            0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
            187, 221, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 17, 0, 0, 0, 0, 0, 192, 63, 0, 0, 32, 64,
            0, 0, 0, 191, 0, 0, 32, 64, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0,
            0, 63, 0, 0, 128, 191, 0, 0, 0, 63, 0, 0, 160, 191, 0, 0, 0, 191, 0, 0, 64, 63, 0, 0, 0, 191, 0, 0, 0, 63,
            0, 0, 192, 191, 0, 0, 192, 191, 0, 0, 192, 191, 0, 0, 224, 191, 0, 0, 32, 192, 0, 0, 128, 62, 0, 0, 32, 192, 0, 0,
            64, 63, 0, 0, 32, 192, 0, 132, 158, 63, 192, 227, 11, 191, 0, 0, 160, 63, 0, 0, 0, 0, 0, 0, 192, 63, 0, 0, 0, 63,
            0, 0, 224, 63, 0, 0, 192, 63, 2, 0, 0, 0, 0, 0, 0, 191, 0, 0, 12, 65, 68, 139, 172, 191, 0, 0, 128, 62, 0, 0,
            12, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 160, 191, 0, 0, 28, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128,
            0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
            170, 221, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0, 192, 63, 0, 0, 32, 64,
            0, 0, 0, 191, 0, 0, 32, 64, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63, 0, 0, 128, 63, 0, 0,
            64, 63, 0, 0, 192, 191, 0, 0, 192, 191, 0, 0, 224, 191, 0, 0, 32, 192, 0, 0, 128, 62, 0, 0, 32, 192, 0, 0, 64, 63,
            0, 0, 32, 192, 0, 0, 128, 63, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 191, 0, 0, 128, 63, 0, 0, 0, 0, 0, 0,
            192, 63, 0, 0, 0, 63, 0, 0, 224, 63, 0, 0, 192, 63, 2, 0, 0, 0, 0, 0, 224, 191, 0, 0, 12, 65, 68, 139, 172, 191,
            0, 0, 128, 191, 0, 0, 12, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 160, 63, 0, 0, 28, 65, 0, 0, 128, 63, 0, 0,
            0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 1, 0,
            0, 0, 0, 0, 0, 0, 170, 221, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0,
            192, 63, 0, 0, 32, 64, 0, 0, 0, 191, 0, 0, 32, 64, 0, 0, 64, 191, 0, 0, 192, 63, 0, 0, 160, 63, 0, 0, 192, 63,
            0, 0, 128, 63, 0, 0, 64, 63, 0, 0, 192, 191, 0, 0, 192, 191, 0, 0, 224, 191, 0, 0, 32, 192, 0, 0, 128, 62, 0, 0,
            32, 192, 0, 0, 64, 63, 0, 0, 32, 192, 0, 0, 128, 63, 0, 0, 192, 191, 0, 0, 64, 191, 0, 0, 192, 191, 0, 0, 128, 63,
            0, 0, 0, 0, 0, 0, 192, 63, 0, 0, 0, 63, 0, 0, 224, 63, 0, 0, 192, 63, 2, 0, 0, 0, 0, 0, 64, 63, 0, 0,
            12, 65, 68, 139, 172, 191, 0, 0, 192, 63, 0, 0, 12, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 200, 192, 0, 0, 104, 65,
            0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0,
            128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 217, 183, 164, 0, 0, 32, 66, 0, 0, 0, 63, 0, 0, 0, 0, 0, 0, 0, 0,
            7, 0, 0, 0, 0, 0, 0, 191, 0, 0, 128, 63, 0, 0, 0, 191, 0, 0, 128, 191, 0, 0, 128, 62, 0, 0, 128, 191, 0, 0,
            0, 63, 0, 0, 0, 191, 0, 0, 128, 62, 0, 0, 0, 0, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 62, 0, 0, 128, 63,
            2, 0, 0, 0, 0, 0, 208, 192, 0, 0, 112, 65, 68, 139, 172, 191, 0, 0, 192, 192, 0, 0, 112, 65, 68, 139, 172, 191, 0, 0,
            0, 0, 0, 0, 160, 192, 0, 0, 104, 65, 0, 0, 128, 63, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63,
            0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 128, 63, 0, 0, 1, 0, 0, 0, 0, 0, 0, 217, 183, 164, 0, 0, 32, 66, 0, 0,
            0, 63, 0, 0, 0, 0, 0, 0, 0, 0, 11, 0, 0, 0, 0, 0, 0, 191, 0, 0, 128, 63, 0, 0, 0, 191, 0, 0, 0, 63,
            0, 0, 128, 190, 0, 0, 0, 0, 0, 0, 128, 190, 0, 0, 128, 191, 0, 0, 128, 62, 0, 0, 128, 191, 0, 0, 128, 62, 0, 0,
            0, 0, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 0, 63, 0, 0, 128, 63, 0, 0, 128, 62, 0, 0, 128, 63, 0, 0, 0, 0,
            0, 0, 0, 63, 0, 0, 128, 190, 0, 0, 128, 63, 2, 0, 0, 0, 0, 0, 168, 192, 0, 0, 112, 65, 68, 139, 172, 191, 0, 0,
            152, 192, 0, 0, 112, 65, 68, 139, 172, 191, 0, 0, 0, 0, 0, 0, 0, 0, 13, 0, 82, 67, 69, 32, 84, 114, 101, 98, 117, 99,
            104, 101, 116, 108, 0, 67, 97, 110, 32, 121, 111, 117, 32, 99, 97, 116, 97, 112, 117, 108, 116, 32, 116, 104, 101, 32, 99, 97, 114, 32,
            116, 111, 32, 116, 104, 101, 32, 111, 116, 104, 101, 114, 32, 115, 105, 100, 101, 32, 97, 110, 100, 32, 98, 97, 99, 107, 63, 10, 84, 114,
            121, 32, 116, 111, 32, 117, 115, 101, 32, 116, 104, 101, 32, 99, 117, 115, 116, 111, 109, 32, 115, 104, 97, 112, 101, 115, 32, 97, 115, 32,
            112, 111, 119, 101, 114, 10, 10, 10, 10, 10, 45, 66, 121, 58, 32, 66, 114, 97, 109, 50, 51, 50, 51, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0 };

        public byte[] error = new byte[] {25, 0, 0, 0, 9, 0, 83, 116, 101, 97, 109, 112, 117, 110, 107, 31, 0, 0, 0, 0, 0, 128, 192, 0, 0, 160,
            64, 0, 0, 0, 0, 1, 0, 36, 0, 48, 51, 51, 100, 50, 98, 52, 57, 45, 102, 55, 57, 102, 45, 52, 100, 52, 102, 45, 57, 52,
            97, 51, 45, 51, 53, 48, 48, 102, 55, 54, 53, 51, 52, 98, 56, 0, 0, 128, 64, 0, 0, 160, 64, 0, 0, 0, 0, 1, 0, 36,
            0, 102, 56, 98, 54, 53, 52, 48, 98, 45, 52, 101, 57, 54, 45, 52, 53, 53, 52, 45, 97, 49, 56, 97, 45, 98, 55, 54, 53, 100,
            52, 50, 99, 48, 52, 98, 97, 0, 0, 96, 192, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 56, 57, 50, 48, 53, 55, 55,
            55, 45, 48, 101, 97, 57, 45, 52, 56, 98, 53, 45, 98, 56, 48, 102, 45, 48, 97, 51, 100, 56, 101, 53, 56, 101, 102, 56, 101, 0,
            0, 96, 192, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 55, 56, 50, 102, 97, 49, 54, 101, 45, 102, 54, 55, 52, 45, 52,
            57, 55, 51, 45, 97, 51, 53, 49, 45, 99, 97, 49, 97, 49, 57, 99, 98, 97, 100, 97, 50, 0, 0, 32, 192, 0, 0, 48, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 56, 54, 52, 53, 49, 48, 102, 97, 45, 57, 52, 52, 51, 45, 52, 50, 54, 51, 45, 57, 54, 54, 57,
            45, 98, 54, 56, 101, 54, 53, 52, 97, 48, 99, 101, 99, 0, 0, 32, 192, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 98,
            51, 50, 102, 98, 52, 52, 98, 45, 55, 50, 51, 49, 45, 52, 98, 50, 99, 45, 57, 50, 51, 55, 45, 99, 49, 52, 50, 51, 99, 55,
            101, 101, 98, 102, 51, 0, 0, 96, 192, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 49, 50, 101, 50, 99, 99, 54, 45,
            98, 99, 48, 99, 45, 52, 48, 98, 102, 45, 56, 54, 56, 56, 45, 50, 98, 98, 51, 54, 101, 48, 99, 49, 55, 100, 54, 0, 0, 32,
            192, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 51, 97, 51, 97, 50, 98, 52, 45, 102, 52, 57, 51, 45, 52, 100, 99,
            102, 45, 57, 102, 102, 98, 45, 53, 99, 49, 53, 53, 102, 53, 57, 53, 54, 55, 48, 0, 0, 0, 192, 0, 0, 16, 65, 0, 0, 0,
            0, 1, 0, 36, 0, 56, 48, 101, 48, 56, 99, 56, 52, 45, 54, 48, 52, 99, 45, 52, 102, 98, 53, 45, 56, 52, 100, 51, 45, 101,
            50, 97, 53, 51, 99, 56, 100, 48, 52, 53, 99, 0, 0, 0, 192, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 102, 54,
            101, 55, 101, 50, 51, 45, 52, 50, 98, 97, 45, 52, 102, 51, 100, 45, 97, 101, 48, 49, 45, 51, 55, 57, 57, 48, 99, 102, 50, 56,
            48, 51, 50, 0, 0, 0, 192, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 55, 97, 102, 49, 98, 50, 97, 45, 102, 54,
            100, 57, 45, 52, 98, 99, 102, 45, 97, 57, 53, 55, 45, 53, 98, 98, 57, 51, 101, 57, 48, 57, 49, 49, 57, 0, 0, 128, 191, 0,
            0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 53, 53, 57, 57, 97, 101, 97, 45, 99, 53, 52, 48, 45, 52, 49, 51, 102, 45,
            97, 99, 101, 100, 45, 49, 54, 100, 98, 55, 102, 49, 102, 52, 100, 53, 100, 0, 0, 128, 191, 0, 0, 32, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 54, 50, 48, 50, 54, 101, 52, 56, 45, 53, 101, 50, 55, 45, 52, 99, 100, 100, 45, 56, 57, 54, 97, 45, 49, 53, 98,
            53, 52, 48, 100, 99, 51, 50, 56, 97, 0, 0, 128, 191, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 49, 55, 50, 98,
            51, 55, 98, 45, 100, 99, 100, 54, 45, 52, 55, 55, 48, 45, 56, 102, 100, 102, 45, 55, 53, 49, 51, 98, 101, 101, 55, 49, 100, 52,
            54, 0, 0, 0, 191, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 54, 53, 100, 97, 56, 54, 102, 49, 45, 53, 99, 49, 99,
            45, 52, 98, 97, 57, 45, 98, 99, 55, 99, 45, 57, 100, 48, 98, 48, 54, 101, 48, 57, 49, 53, 54, 0, 0, 0, 191, 0, 0, 16,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 52, 98, 98, 54, 102, 51, 56, 45, 52, 52, 56, 49, 45, 52, 51, 51, 99, 45, 98, 53,
            50, 48, 45, 97, 51, 54, 99, 99, 56, 51, 48, 52, 50, 99, 50, 0, 0, 0, 191, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 97, 56, 56, 55, 50, 97, 101, 102, 45, 56, 57, 56, 52, 45, 52, 56, 50, 57, 45, 98, 99, 52, 48, 45, 100, 100, 54, 51, 101,
            99, 49, 49, 102, 102, 53, 55, 0, 0, 0, 63, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 102, 102, 53, 51, 102, 57,
            101, 45, 53, 98, 54, 56, 45, 52, 54, 98, 100, 45, 56, 50, 53, 52, 45, 55, 97, 101, 97, 97, 57, 101, 57, 101, 55, 51, 100, 0,
            0, 0, 63, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 99, 52, 57, 100, 98, 100, 101, 54, 45, 51, 51, 97, 102, 45, 52,
            54, 53, 52, 45, 56, 97, 102, 101, 45, 53, 52, 100, 98, 52, 49, 55, 55, 56, 54, 101, 48, 0, 0, 0, 63, 0, 0, 16, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 50, 101, 51, 56, 55, 57, 101, 48, 45, 98, 54, 55, 54, 45, 52, 53, 57, 100, 45, 97, 54, 101, 98,
            45, 57, 52, 99, 49, 53, 52, 53, 51, 52, 55, 53, 101, 0, 0, 128, 63, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 54,
            54, 54, 57, 57, 54, 48, 54, 45, 49, 49, 56, 53, 45, 52, 54, 101, 97, 45, 56, 51, 54, 97, 45, 98, 55, 53, 49, 55, 50, 98,
            97, 99, 97, 51, 54, 0, 0, 128, 63, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57, 49, 98, 53, 52, 55, 49, 98, 45,
            98, 102, 51, 101, 45, 52, 49, 50, 51, 45, 56, 52, 48, 97, 45, 102, 101, 101, 101, 51, 50, 49, 56, 98, 100, 48, 49, 0, 0, 0,
            64, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 57, 54, 98, 57, 102, 98, 55, 45, 50, 49, 51, 51, 45, 52, 52, 101,
            56, 45, 98, 99, 101, 50, 45, 48, 56, 97, 101, 57, 56, 51, 50, 53, 49, 52, 98, 0, 0, 0, 64, 0, 0, 16, 65, 0, 0, 0,
            0, 1, 0, 36, 0, 100, 51, 55, 99, 102, 53, 99, 56, 45, 102, 101, 52, 55, 45, 52, 97, 52, 50, 45, 97, 102, 101, 52, 45, 48,
            51, 97, 51, 51, 101, 48, 100, 48, 97, 50, 97, 0, 0, 32, 64, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 102, 102, 101,
            57, 52, 52, 99, 100, 45, 50, 99, 55, 102, 45, 52, 101, 101, 99, 45, 97, 99, 100, 97, 45, 50, 97, 48, 99, 97, 50, 102, 101, 49,
            57, 57, 54, 0, 0, 32, 64, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 102, 49, 57, 57, 49, 52, 49, 45, 57, 99,
            54, 98, 45, 52, 102, 101, 50, 45, 56, 102, 54, 50, 45, 101, 53, 51, 51, 49, 102, 54, 99, 102, 97, 98, 57, 0, 0, 96, 64, 0,
            0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 98, 57, 54, 53, 98, 51, 99, 51, 45, 48, 97, 102, 51, 45, 52, 51, 55, 100, 45,
            56, 57, 49, 49, 45, 53, 52, 50, 99, 48, 57, 50, 101, 55, 97, 48, 49, 0, 0, 96, 64, 0, 0, 16, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 97, 101, 101, 54, 53, 53, 100, 57, 45, 97, 57, 50, 56, 45, 52, 100, 55, 56, 45, 57, 50, 102, 101, 45, 56, 53, 48,
            102, 99, 52, 55, 49, 97, 55, 102, 97, 0, 0, 32, 64, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57, 100, 53, 51, 57,
            98, 49, 52, 45, 49, 56, 98, 49, 45, 52, 49, 56, 50, 45, 56, 53, 102, 101, 45, 49, 57, 48, 48, 57, 102, 51, 98, 49, 55, 101,
            49, 0, 0, 96, 64, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 99, 54, 100, 48, 53, 101, 97, 45, 56, 54, 48, 54,
            45, 52, 52, 52, 48, 45, 56, 98, 54, 55, 45, 55, 54, 56, 57, 54, 99, 49, 98, 98, 50, 56, 56, 0, 0, 96, 192, 0, 0, 108,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 98, 98, 50, 55, 53, 56, 53, 57, 45, 101, 51, 56, 53, 45, 52, 56, 54, 102, 45, 98, 98,
            53, 50, 45, 50, 56, 53, 56, 98, 100, 99, 101, 57, 98, 99, 53, 0, 0, 0, 0, 9, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 160, 64, 0, 0, 0, 0, 0, 0, 36, 0, 55, 100, 100, 54, 55, 50, 52, 49, 45, 54, 51, 49, 48, 45, 52, 97, 57,
            56, 45, 97, 52, 57, 57, 45, 51, 55, 50, 52, 99, 100, 98, 49, 99, 54, 51, 50, 0, 0, 128, 63, 0, 0, 216, 64, 0, 0, 0,
            0, 0, 0, 36, 0, 50, 97, 102, 99, 98, 49, 102, 56, 45, 57, 57, 48, 49, 45, 52, 54, 99, 54, 45, 57, 51, 98, 100, 45, 99,
            55, 49, 101, 99, 52, 51, 57, 100, 53, 99, 54, 0, 0, 128, 191, 0, 0, 216, 64, 0, 0, 0, 0, 0, 0, 36, 0, 100, 100, 99,
            49, 49, 51, 101, 53, 45, 100, 56, 50, 49, 45, 52, 50, 52, 100, 45, 56, 51, 53, 48, 45, 98, 48, 99, 97, 56, 48, 55, 99, 48,
            99, 99, 51, 0, 0, 0, 64, 0, 0, 160, 64, 0, 0, 0, 0, 0, 0, 36, 0, 50, 55, 99, 54, 55, 49, 102, 49, 45, 49, 53,
            54, 100, 45, 52, 101, 57, 56, 45, 98, 48, 99, 51, 45, 102, 97, 49, 54, 49, 97, 50, 98, 55, 57, 52, 52, 0, 0, 0, 192, 0,
            0, 160, 64, 0, 0, 0, 0, 0, 0, 36, 0, 54, 54, 55, 56, 55, 49, 98, 49, 45, 52, 50, 53, 101, 45, 52, 99, 56, 57, 45,
            97, 57, 97, 55, 45, 49, 100, 54, 56, 97, 49, 98, 53, 102, 101, 52, 98, 0, 0, 64, 192, 0, 0, 216, 64, 0, 0, 0, 0, 0,
            0, 36, 0, 54, 99, 53, 101, 56, 101, 48, 48, 45, 99, 102, 57, 57, 45, 52, 48, 49, 53, 45, 98, 54, 98, 54, 45, 49, 98, 55,
            51, 102, 102, 48, 54, 54, 52, 101, 49, 0, 0, 64, 64, 0, 0, 216, 64, 0, 0, 0, 0, 0, 0, 36, 0, 101, 48, 101, 102, 51,
            57, 55, 99, 45, 50, 55, 55, 102, 45, 52, 53, 97, 52, 45, 98, 50, 51, 53, 45, 49, 97, 99, 55, 56, 56, 99, 100, 56, 53, 57,
            102, 42, 0, 0, 0, 3, 0, 0, 0, 36, 0, 50, 97, 102, 99, 98, 49, 102, 56, 45, 57, 57, 48, 49, 45, 52, 54, 99, 54, 45,
            57, 51, 98, 100, 45, 99, 55, 49, 101, 99, 52, 51, 57, 100, 53, 99, 54, 36, 0, 55, 100, 100, 54, 55, 50, 52, 49, 45, 54, 51,
            49, 48, 45, 52, 97, 57, 56, 45, 97, 52, 57, 57, 45, 51, 55, 50, 52, 99, 100, 98, 49, 99, 54, 51, 50, 0, 0, 0, 0, 1,
            0, 0, 0, 3, 0, 0, 0, 36, 0, 100, 100, 99, 49, 49, 51, 101, 53, 45, 100, 56, 50, 49, 45, 52, 50, 52, 100, 45, 56, 51,
            53, 48, 45, 98, 48, 99, 97, 56, 48, 55, 99, 48, 99, 99, 51, 36, 0, 55, 100, 100, 54, 55, 50, 52, 49, 45, 54, 51, 49, 48,
            45, 52, 97, 57, 56, 45, 97, 52, 57, 57, 45, 51, 55, 50, 52, 99, 100, 98, 49, 99, 54, 51, 50, 1, 0, 0, 0, 0, 0, 0,
            0, 1, 0, 0, 0, 36, 0, 102, 56, 98, 54, 53, 52, 48, 98, 45, 52, 101, 57, 54, 45, 52, 53, 53, 52, 45, 97, 49, 56, 97,
            45, 98, 55, 54, 53, 100, 52, 50, 99, 48, 52, 98, 97, 36, 0, 50, 55, 99, 54, 55, 49, 102, 49, 45, 49, 53, 54, 100, 45, 52,
            101, 57, 56, 45, 98, 48, 99, 51, 45, 102, 97, 49, 54, 49, 97, 50, 98, 55, 57, 52, 52, 1, 0, 0, 0, 1, 0, 0, 0, 1,
            0, 0, 0, 36, 0, 50, 55, 99, 54, 55, 49, 102, 49, 45, 49, 53, 54, 100, 45, 52, 101, 57, 56, 45, 98, 48, 99, 51, 45, 102,
            97, 49, 54, 49, 97, 50, 98, 55, 57, 52, 52, 36, 0, 55, 100, 100, 54, 55, 50, 52, 49, 45, 54, 51, 49, 48, 45, 52, 97, 57,
            56, 45, 97, 52, 57, 57, 45, 51, 55, 50, 52, 99, 100, 98, 49, 99, 54, 51, 50, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0,
            0, 36, 0, 55, 100, 100, 54, 55, 50, 52, 49, 45, 54, 51, 49, 48, 45, 52, 97, 57, 56, 45, 97, 52, 57, 57, 45, 51, 55, 50,
            52, 99, 100, 98, 49, 99, 54, 51, 50, 36, 0, 54, 54, 55, 56, 55, 49, 98, 49, 45, 52, 50, 53, 101, 45, 52, 99, 56, 57, 45,
            97, 57, 97, 55, 45, 49, 100, 54, 56, 97, 49, 98, 53, 102, 101, 52, 98, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 36,
            0, 54, 54, 55, 56, 55, 49, 98, 49, 45, 52, 50, 53, 101, 45, 52, 99, 56, 57, 45, 97, 57, 97, 55, 45, 49, 100, 54, 56, 97,
            49, 98, 53, 102, 101, 52, 98, 36, 0, 48, 51, 51, 100, 50, 98, 52, 57, 45, 102, 55, 57, 102, 45, 52, 100, 52, 102, 45, 57, 52,
            97, 51, 45, 51, 53, 48, 48, 102, 55, 54, 53, 51, 52, 98, 56, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 36, 0, 48,
            51, 51, 100, 50, 98, 52, 57, 45, 102, 55, 57, 102, 45, 52, 100, 52, 102, 45, 57, 52, 97, 51, 45, 51, 53, 48, 48, 102, 55, 54,
            53, 51, 52, 98, 56, 36, 0, 54, 99, 53, 101, 56, 101, 48, 48, 45, 99, 102, 57, 57, 45, 52, 48, 49, 53, 45, 98, 54, 98, 54,
            45, 49, 98, 55, 51, 102, 102, 48, 54, 54, 52, 101, 49, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 36, 0, 54, 99, 53,
            101, 56, 101, 48, 48, 45, 99, 102, 57, 57, 45, 52, 48, 49, 53, 45, 98, 54, 98, 54, 45, 49, 98, 55, 51, 102, 102, 48, 54, 54,
            52, 101, 49, 36, 0, 54, 54, 55, 56, 55, 49, 98, 49, 45, 52, 50, 53, 101, 45, 52, 99, 56, 57, 45, 97, 57, 97, 55, 45, 49,
            100, 54, 56, 97, 49, 98, 53, 102, 101, 52, 98, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 36, 0, 54, 99, 53, 101, 56,
            101, 48, 48, 45, 99, 102, 57, 57, 45, 52, 48, 49, 53, 45, 98, 54, 98, 54, 45, 49, 98, 55, 51, 102, 102, 48, 54, 54, 52, 101,
            49, 36, 0, 100, 100, 99, 49, 49, 51, 101, 53, 45, 100, 56, 50, 49, 45, 52, 50, 52, 100, 45, 56, 51, 53, 48, 45, 98, 48, 99,
            97, 56, 48, 55, 99, 48, 99, 99, 51, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 36, 0, 50, 97, 102, 99, 98, 49, 102,
            56, 45, 57, 57, 48, 49, 45, 52, 54, 99, 54, 45, 57, 51, 98, 100, 45, 99, 55, 49, 101, 99, 52, 51, 57, 100, 53, 99, 54, 36,
            0, 101, 48, 101, 102, 51, 57, 55, 99, 45, 50, 55, 55, 102, 45, 52, 53, 97, 52, 45, 98, 50, 51, 53, 45, 49, 97, 99, 55, 56,
            56, 99, 100, 56, 53, 57, 102, 1, 0, 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 36, 0, 101, 48, 101, 102, 51, 57, 55, 99, 45,
            50, 55, 55, 102, 45, 52, 53, 97, 52, 45, 98, 50, 51, 53, 45, 49, 97, 99, 55, 56, 56, 99, 100, 56, 53, 57, 102, 36, 0, 102,
            56, 98, 54, 53, 52, 48, 98, 45, 52, 101, 57, 54, 45, 52, 53, 53, 52, 45, 97, 49, 56, 97, 45, 98, 55, 54, 53, 100, 52, 50,
            99, 48, 52, 98, 97, 1, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 36, 0, 101, 48, 101, 102, 51, 57, 55, 99, 45, 50, 55,
            55, 102, 45, 52, 53, 97, 52, 45, 98, 50, 51, 53, 45, 49, 97, 99, 55, 56, 56, 99, 100, 56, 53, 57, 102, 36, 0, 50, 55, 99,
            54, 55, 49, 102, 49, 45, 49, 53, 54, 100, 45, 52, 101, 57, 56, 45, 98, 48, 99, 51, 45, 102, 97, 49, 54, 49, 97, 50, 98, 55,
            57, 52, 52, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 36, 0, 50, 55, 99, 54, 55, 49, 102, 49, 45, 49, 53, 54, 100,
            45, 52, 101, 57, 56, 45, 98, 48, 99, 51, 45, 102, 97, 49, 54, 49, 97, 50, 98, 55, 57, 52, 52, 36, 0, 50, 97, 102, 99, 98,
            49, 102, 56, 45, 57, 57, 48, 49, 45, 52, 54, 99, 54, 45, 57, 51, 98, 100, 45, 99, 55, 49, 101, 99, 52, 51, 57, 100, 53, 99,
            54, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 36, 0, 50, 97, 102, 99, 98, 49, 102, 56, 45, 57, 57, 48, 49, 45, 52,
            54, 99, 54, 45, 57, 51, 98, 100, 45, 99, 55, 49, 101, 99, 52, 51, 57, 100, 53, 99, 54, 36, 0, 100, 100, 99, 49, 49, 51, 101,
            53, 45, 100, 56, 50, 49, 45, 52, 50, 52, 100, 45, 56, 51, 53, 48, 45, 98, 48, 99, 97, 56, 48, 55, 99, 48, 99, 99, 51, 0,
            0, 0, 0, 1, 0, 0, 0, 3, 0, 0, 0, 36, 0, 100, 100, 99, 49, 49, 51, 101, 53, 45, 100, 56, 50, 49, 45, 52, 50, 52,
            100, 45, 56, 51, 53, 48, 45, 98, 48, 99, 97, 56, 48, 55, 99, 48, 99, 99, 51, 36, 0, 54, 54, 55, 56, 55, 49, 98, 49, 45,
            52, 50, 53, 101, 45, 52, 99, 56, 57, 45, 97, 57, 97, 55, 45, 49, 100, 54, 56, 97, 49, 98, 53, 102, 101, 52, 98, 0, 0, 0,
            0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 98, 51, 50, 102, 98, 52, 52, 98, 45, 55, 50, 51, 49, 45, 52, 98, 50, 99, 45,
            57, 50, 51, 55, 45, 99, 49, 52, 50, 51, 99, 55, 101, 101, 98, 102, 51, 36, 0, 56, 57, 50, 48, 53, 55, 55, 55, 45, 48, 101,
            97, 57, 45, 52, 56, 98, 53, 45, 98, 56, 48, 102, 45, 48, 97, 51, 100, 56, 101, 53, 56, 101, 102, 56, 101, 0, 0, 0, 0, 1,
            0, 0, 0, 2, 0, 0, 0, 36, 0, 56, 57, 50, 48, 53, 55, 55, 55, 45, 48, 101, 97, 57, 45, 52, 56, 98, 53, 45, 98, 56,
            48, 102, 45, 48, 97, 51, 100, 56, 101, 53, 56, 101, 102, 56, 101, 36, 0, 100, 49, 50, 101, 50, 99, 99, 54, 45, 98, 99, 48, 99,
            45, 52, 48, 98, 102, 45, 56, 54, 56, 56, 45, 50, 98, 98, 51, 54, 101, 48, 99, 49, 55, 100, 54, 0, 0, 0, 0, 0, 0, 0,
            0, 2, 0, 0, 0, 36, 0, 100, 49, 50, 101, 50, 99, 99, 54, 45, 98, 99, 48, 99, 45, 52, 48, 98, 102, 45, 56, 54, 56, 56,
            45, 50, 98, 98, 51, 54, 101, 48, 99, 49, 55, 100, 54, 36, 0, 55, 56, 50, 102, 97, 49, 54, 101, 45, 102, 54, 55, 52, 45, 52,
            57, 55, 51, 45, 97, 51, 53, 49, 45, 99, 97, 49, 97, 49, 57, 99, 98, 97, 100, 97, 50, 0, 0, 0, 0, 0, 0, 0, 0, 2,
            0, 0, 0, 36, 0, 55, 56, 50, 102, 97, 49, 54, 101, 45, 102, 54, 55, 52, 45, 52, 57, 55, 51, 45, 97, 51, 53, 49, 45, 99,
            97, 49, 97, 49, 57, 99, 98, 97, 100, 97, 50, 36, 0, 56, 54, 52, 53, 49, 48, 102, 97, 45, 57, 52, 52, 51, 45, 52, 50, 54,
            51, 45, 57, 54, 54, 57, 45, 98, 54, 56, 101, 54, 53, 52, 97, 48, 99, 101, 99, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0,
            0, 36, 0, 97, 51, 97, 51, 97, 50, 98, 52, 45, 102, 52, 57, 51, 45, 52, 100, 99, 102, 45, 57, 102, 102, 98, 45, 53, 99, 49,
            53, 53, 102, 53, 57, 53, 54, 55, 48, 36, 0, 100, 49, 50, 101, 50, 99, 99, 54, 45, 98, 99, 48, 99, 45, 52, 48, 98, 102, 45,
            56, 54, 56, 56, 45, 50, 98, 98, 51, 54, 101, 48, 99, 49, 55, 100, 54, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36,
            0, 53, 55, 97, 102, 49, 98, 50, 97, 45, 102, 54, 100, 57, 45, 52, 98, 99, 102, 45, 97, 57, 53, 55, 45, 53, 98, 98, 57, 51,
            101, 57, 48, 57, 49, 49, 57, 36, 0, 51, 53, 53, 57, 57, 97, 101, 97, 45, 99, 53, 52, 48, 45, 52, 49, 51, 102, 45, 97, 99,
            101, 100, 45, 49, 54, 100, 98, 55, 102, 49, 102, 52, 100, 53, 100, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 36, 0, 51,
            53, 53, 57, 57, 97, 101, 97, 45, 99, 53, 52, 48, 45, 52, 49, 51, 102, 45, 97, 99, 101, 100, 45, 49, 54, 100, 98, 55, 102, 49,
            102, 52, 100, 53, 100, 36, 0, 54, 50, 48, 50, 54, 101, 52, 56, 45, 53, 101, 50, 55, 45, 52, 99, 100, 100, 45, 56, 57, 54, 97,
            45, 49, 53, 98, 53, 52, 48, 100, 99, 51, 50, 56, 97, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 36, 0, 54, 50, 48,
            50, 54, 101, 52, 56, 45, 53, 101, 50, 55, 45, 52, 99, 100, 100, 45, 56, 57, 54, 97, 45, 49, 53, 98, 53, 52, 48, 100, 99, 51,
            50, 56, 97, 36, 0, 97, 102, 54, 101, 55, 101, 50, 51, 45, 52, 50, 98, 97, 45, 52, 102, 51, 100, 45, 97, 101, 48, 49, 45, 51,
            55, 57, 57, 48, 99, 102, 50, 56, 48, 51, 50, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 97, 102, 54, 101, 55,
            101, 50, 51, 45, 52, 50, 98, 97, 45, 52, 102, 51, 100, 45, 97, 101, 48, 49, 45, 51, 55, 57, 57, 48, 99, 102, 50, 56, 48, 51,
            50, 36, 0, 53, 55, 97, 102, 49, 98, 50, 97, 45, 102, 54, 100, 57, 45, 52, 98, 99, 102, 45, 97, 57, 53, 55, 45, 53, 98, 98,
            57, 51, 101, 57, 48, 57, 49, 49, 57, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 36, 0, 56, 48, 101, 48, 56, 99, 56,
            52, 45, 54, 48, 52, 99, 45, 52, 102, 98, 53, 45, 56, 52, 100, 51, 45, 101, 50, 97, 53, 51, 99, 56, 100, 48, 52, 53, 99, 36,
            0, 97, 102, 54, 101, 55, 101, 50, 51, 45, 52, 50, 98, 97, 45, 52, 102, 51, 100, 45, 97, 101, 48, 49, 45, 51, 55, 57, 57, 48,
            99, 102, 50, 56, 48, 51, 50, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 36, 0, 97, 102, 54, 101, 55, 101, 50, 51, 45,
            52, 50, 98, 97, 45, 52, 102, 51, 100, 45, 97, 101, 48, 49, 45, 51, 55, 57, 57, 48, 99, 102, 50, 56, 48, 51, 50, 36, 0, 49,
            49, 55, 50, 98, 51, 55, 98, 45, 100, 99, 100, 54, 45, 52, 55, 55, 48, 45, 56, 102, 100, 102, 45, 55, 53, 49, 51, 98, 101, 101,
            55, 49, 100, 52, 54, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 36, 0, 49, 52, 98, 98, 54, 102, 51, 56, 45, 52, 52,
            56, 49, 45, 52, 51, 51, 99, 45, 98, 53, 50, 48, 45, 97, 51, 54, 99, 99, 56, 51, 48, 52, 50, 99, 50, 36, 0, 97, 56, 56,
            55, 50, 97, 101, 102, 45, 56, 57, 56, 52, 45, 52, 56, 50, 57, 45, 98, 99, 52, 48, 45, 100, 100, 54, 51, 101, 99, 49, 49, 102,
            102, 53, 55, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 99, 52, 57, 100, 98, 100, 101, 54, 45, 51, 51, 97, 102,
            45, 52, 54, 53, 52, 45, 56, 97, 102, 101, 45, 53, 52, 100, 98, 52, 49, 55, 55, 56, 54, 101, 48, 36, 0, 97, 56, 56, 55, 50,
            97, 101, 102, 45, 56, 57, 56, 52, 45, 52, 56, 50, 57, 45, 98, 99, 52, 48, 45, 100, 100, 54, 51, 101, 99, 49, 49, 102, 102, 53,
            55, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 97, 56, 56, 55, 50, 97, 101, 102, 45, 56, 57, 56, 52, 45, 52,
            56, 50, 57, 45, 98, 99, 52, 48, 45, 100, 100, 54, 51, 101, 99, 49, 49, 102, 102, 53, 55, 36, 0, 54, 53, 100, 97, 56, 54, 102,
            49, 45, 53, 99, 49, 99, 45, 52, 98, 97, 57, 45, 98, 99, 55, 99, 45, 57, 100, 48, 98, 48, 54, 101, 48, 57, 49, 53, 54, 1,
            0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 97, 56, 56, 55, 50, 97, 101, 102, 45, 56, 57, 56, 52, 45, 52, 56, 50,
            57, 45, 98, 99, 52, 48, 45, 100, 100, 54, 51, 101, 99, 49, 49, 102, 102, 53, 55, 36, 0, 50, 101, 51, 56, 55, 57, 101, 48, 45,
            98, 54, 55, 54, 45, 52, 53, 57, 100, 45, 97, 54, 101, 98, 45, 57, 52, 99, 49, 53, 52, 53, 51, 52, 55, 53, 101, 1, 0, 0,
            0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 54, 53, 100, 97, 56, 54, 102, 49, 45, 53, 99, 49, 99, 45, 52, 98, 97, 57, 45,
            98, 99, 55, 99, 45, 57, 100, 48, 98, 48, 54, 101, 48, 57, 49, 53, 54, 36, 0, 51, 102, 102, 53, 51, 102, 57, 101, 45, 53, 98,
            54, 56, 45, 52, 54, 98, 100, 45, 56, 50, 53, 52, 45, 55, 97, 101, 97, 97, 57, 101, 57, 101, 55, 51, 100, 1, 0, 0, 0, 1,
            0, 0, 0, 2, 0, 0, 0, 36, 0, 51, 102, 102, 53, 51, 102, 57, 101, 45, 53, 98, 54, 56, 45, 52, 54, 98, 100, 45, 56, 50,
            53, 52, 45, 55, 97, 101, 97, 97, 57, 101, 57, 101, 55, 51, 100, 36, 0, 99, 52, 57, 100, 98, 100, 101, 54, 45, 51, 51, 97, 102,
            45, 52, 54, 53, 52, 45, 56, 97, 102, 101, 45, 53, 52, 100, 98, 52, 49, 55, 55, 56, 54, 101, 48, 1, 0, 0, 0, 1, 0, 0,
            0, 2, 0, 0, 0, 36, 0, 102, 102, 101, 57, 52, 52, 99, 100, 45, 50, 99, 55, 102, 45, 52, 101, 101, 99, 45, 97, 99, 100, 97,
            45, 50, 97, 48, 99, 97, 50, 102, 101, 49, 57, 57, 54, 36, 0, 51, 102, 49, 57, 57, 49, 52, 49, 45, 57, 99, 54, 98, 45, 52,
            102, 101, 50, 45, 56, 102, 54, 50, 45, 101, 53, 51, 51, 49, 102, 54, 99, 102, 97, 98, 57, 1, 0, 0, 0, 1, 0, 0, 0, 2,
            0, 0, 0, 36, 0, 97, 99, 54, 100, 48, 53, 101, 97, 45, 56, 54, 48, 54, 45, 52, 52, 52, 48, 45, 56, 98, 54, 55, 45, 55,
            54, 56, 57, 54, 99, 49, 98, 98, 50, 56, 56, 36, 0, 51, 102, 49, 57, 57, 49, 52, 49, 45, 57, 99, 54, 98, 45, 52, 102, 101,
            50, 45, 56, 102, 54, 50, 45, 101, 53, 51, 51, 49, 102, 54, 99, 102, 97, 98, 57, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0,
            0, 36, 0, 51, 102, 49, 57, 57, 49, 52, 49, 45, 57, 99, 54, 98, 45, 52, 102, 101, 50, 45, 56, 102, 54, 50, 45, 101, 53, 51,
            51, 49, 102, 54, 99, 102, 97, 98, 57, 36, 0, 57, 100, 53, 51, 57, 98, 49, 52, 45, 49, 56, 98, 49, 45, 52, 49, 56, 50, 45,
            56, 53, 102, 101, 45, 49, 57, 48, 48, 57, 102, 51, 98, 49, 55, 101, 49, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36,
            0, 51, 102, 49, 57, 57, 49, 52, 49, 45, 57, 99, 54, 98, 45, 52, 102, 101, 50, 45, 56, 102, 54, 50, 45, 101, 53, 51, 51, 49,
            102, 54, 99, 102, 97, 98, 57, 36, 0, 97, 101, 101, 54, 53, 53, 100, 57, 45, 97, 57, 50, 56, 45, 52, 100, 55, 56, 45, 57, 50,
            102, 101, 45, 56, 53, 48, 102, 99, 52, 55, 49, 97, 55, 102, 97, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 57,
            100, 53, 51, 57, 98, 49, 52, 45, 49, 56, 98, 49, 45, 52, 49, 56, 50, 45, 56, 53, 102, 101, 45, 49, 57, 48, 48, 57, 102, 51,
            98, 49, 55, 101, 49, 36, 0, 98, 57, 54, 53, 98, 51, 99, 51, 45, 48, 97, 102, 51, 45, 52, 51, 55, 100, 45, 56, 57, 49, 49,
            45, 53, 52, 50, 99, 48, 57, 50, 101, 55, 97, 48, 49, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 98, 57, 54,
            53, 98, 51, 99, 51, 45, 48, 97, 102, 51, 45, 52, 51, 55, 100, 45, 56, 57, 49, 49, 45, 53, 52, 50, 99, 48, 57, 50, 101, 55,
            97, 48, 49, 36, 0, 97, 99, 54, 100, 48, 53, 101, 97, 45, 56, 54, 48, 54, 45, 52, 52, 52, 48, 45, 56, 98, 54, 55, 45, 55,
            54, 56, 57, 54, 99, 49, 98, 98, 50, 56, 56, 1, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 54, 54, 54, 57, 57,
            54, 48, 54, 45, 49, 49, 56, 53, 45, 52, 54, 101, 97, 45, 56, 51, 54, 97, 45, 98, 55, 53, 49, 55, 50, 98, 97, 99, 97, 51,
            54, 36, 0, 100, 51, 55, 99, 102, 53, 99, 56, 45, 102, 101, 52, 55, 45, 52, 97, 52, 50, 45, 97, 102, 101, 52, 45, 48, 51, 97,
            51, 51, 101, 48, 100, 48, 97, 50, 97, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 36, 0, 100, 51, 55, 99, 102, 53, 99,
            56, 45, 102, 101, 52, 55, 45, 52, 97, 52, 50, 45, 97, 102, 101, 52, 45, 48, 51, 97, 51, 51, 101, 48, 100, 48, 97, 50, 97, 36,
            0, 51, 57, 54, 98, 57, 102, 98, 55, 45, 50, 49, 51, 51, 45, 52, 52, 101, 56, 45, 98, 99, 101, 50, 45, 48, 56, 97, 101, 57,
            56, 51, 50, 53, 49, 52, 98, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 36, 0, 51, 57, 54, 98, 57, 102, 98, 55, 45,
            50, 49, 51, 51, 45, 52, 52, 101, 56, 45, 98, 99, 101, 50, 45, 48, 56, 97, 101, 57, 56, 51, 50, 53, 49, 52, 98, 36, 0, 57,
            49, 98, 53, 52, 55, 49, 98, 45, 98, 102, 51, 101, 45, 52, 49, 50, 51, 45, 56, 52, 48, 97, 45, 102, 101, 101, 101, 51, 50, 49,
            56, 98, 100, 48, 49, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 36, 0, 57, 49, 98, 53, 52, 55, 49, 98, 45, 98, 102,
            51, 101, 45, 52, 49, 50, 51, 45, 56, 52, 48, 97, 45, 102, 101, 101, 101, 51, 50, 49, 56, 98, 100, 48, 49, 36, 0, 54, 54, 54,
            57, 57, 54, 48, 54, 45, 49, 49, 56, 53, 45, 52, 54, 101, 97, 45, 56, 51, 54, 97, 45, 98, 55, 53, 49, 55, 50, 98, 97, 99,
            97, 51, 54, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 31, 0, 0, 0, 0, 0, 128,
            192, 0, 0, 160, 64, 0, 0, 0, 0, 1, 0, 36, 0, 48, 51, 51, 100, 50, 98, 52, 57, 45, 102, 55, 57, 102, 45, 52, 100, 52,
            102, 45, 57, 52, 97, 51, 45, 51, 53, 48, 48, 102, 55, 54, 53, 51, 52, 98, 56, 0, 0, 128, 64, 0, 0, 160, 64, 0, 0, 0,
            0, 1, 0, 36, 0, 102, 56, 98, 54, 53, 52, 48, 98, 45, 52, 101, 57, 54, 45, 52, 53, 53, 52, 45, 97, 49, 56, 97, 45, 98,
            55, 54, 53, 100, 52, 50, 99, 48, 52, 98, 97, 0, 0, 96, 192, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 56, 57, 50,
            48, 53, 55, 55, 55, 45, 48, 101, 97, 57, 45, 52, 56, 98, 53, 45, 98, 56, 48, 102, 45, 48, 97, 51, 100, 56, 101, 53, 56, 101,
            102, 56, 101, 0, 0, 96, 192, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 55, 56, 50, 102, 97, 49, 54, 101, 45, 102, 54,
            55, 52, 45, 52, 57, 55, 51, 45, 97, 51, 53, 49, 45, 99, 97, 49, 97, 49, 57, 99, 98, 97, 100, 97, 50, 0, 0, 32, 192, 0,
            0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 56, 54, 52, 53, 49, 48, 102, 97, 45, 57, 52, 52, 51, 45, 52, 50, 54, 51, 45,
            57, 54, 54, 57, 45, 98, 54, 56, 101, 54, 53, 52, 97, 48, 99, 101, 99, 0, 0, 32, 192, 0, 0, 16, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 98, 51, 50, 102, 98, 52, 52, 98, 45, 55, 50, 51, 49, 45, 52, 98, 50, 99, 45, 57, 50, 51, 55, 45, 99, 49, 52,
            50, 51, 99, 55, 101, 101, 98, 102, 51, 0, 0, 96, 192, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 49, 50, 101, 50,
            99, 99, 54, 45, 98, 99, 48, 99, 45, 52, 48, 98, 102, 45, 56, 54, 56, 56, 45, 50, 98, 98, 51, 54, 101, 48, 99, 49, 55, 100,
            54, 0, 0, 32, 192, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 51, 97, 51, 97, 50, 98, 52, 45, 102, 52, 57, 51,
            45, 52, 100, 99, 102, 45, 57, 102, 102, 98, 45, 53, 99, 49, 53, 53, 102, 53, 57, 53, 54, 55, 48, 0, 0, 0, 192, 0, 0, 16,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 56, 48, 101, 48, 56, 99, 56, 52, 45, 54, 48, 52, 99, 45, 52, 102, 98, 53, 45, 56, 52,
            100, 51, 45, 101, 50, 97, 53, 51, 99, 56, 100, 48, 52, 53, 99, 0, 0, 0, 192, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 97, 102, 54, 101, 55, 101, 50, 51, 45, 52, 50, 98, 97, 45, 52, 102, 51, 100, 45, 97, 101, 48, 49, 45, 51, 55, 57, 57, 48,
            99, 102, 50, 56, 48, 51, 50, 0, 0, 0, 192, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 53, 55, 97, 102, 49, 98, 50,
            97, 45, 102, 54, 100, 57, 45, 52, 98, 99, 102, 45, 97, 57, 53, 55, 45, 53, 98, 98, 57, 51, 101, 57, 48, 57, 49, 49, 57, 0,
            0, 128, 191, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 53, 53, 57, 57, 97, 101, 97, 45, 99, 53, 52, 48, 45, 52,
            49, 51, 102, 45, 97, 99, 101, 100, 45, 49, 54, 100, 98, 55, 102, 49, 102, 52, 100, 53, 100, 0, 0, 128, 191, 0, 0, 32, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 54, 50, 48, 50, 54, 101, 52, 56, 45, 53, 101, 50, 55, 45, 52, 99, 100, 100, 45, 56, 57, 54, 97,
            45, 49, 53, 98, 53, 52, 48, 100, 99, 51, 50, 56, 97, 0, 0, 128, 191, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49,
            49, 55, 50, 98, 51, 55, 98, 45, 100, 99, 100, 54, 45, 52, 55, 55, 48, 45, 56, 102, 100, 102, 45, 55, 53, 49, 51, 98, 101, 101,
            55, 49, 100, 52, 54, 0, 0, 0, 191, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 54, 53, 100, 97, 56, 54, 102, 49, 45,
            53, 99, 49, 99, 45, 52, 98, 97, 57, 45, 98, 99, 55, 99, 45, 57, 100, 48, 98, 48, 54, 101, 48, 57, 49, 53, 54, 0, 0, 0,
            191, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 49, 52, 98, 98, 54, 102, 51, 56, 45, 52, 52, 56, 49, 45, 52, 51, 51,
            99, 45, 98, 53, 50, 48, 45, 97, 51, 54, 99, 99, 56, 51, 48, 52, 50, 99, 50, 0, 0, 0, 191, 0, 0, 32, 65, 0, 0, 0,
            0, 1, 0, 36, 0, 97, 56, 56, 55, 50, 97, 101, 102, 45, 56, 57, 56, 52, 45, 52, 56, 50, 57, 45, 98, 99, 52, 48, 45, 100,
            100, 54, 51, 101, 99, 49, 49, 102, 102, 53, 55, 0, 0, 0, 63, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 102, 102,
            53, 51, 102, 57, 101, 45, 53, 98, 54, 56, 45, 52, 54, 98, 100, 45, 56, 50, 53, 52, 45, 55, 97, 101, 97, 97, 57, 101, 57, 101,
            55, 51, 100, 0, 0, 0, 63, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 99, 52, 57, 100, 98, 100, 101, 54, 45, 51, 51,
            97, 102, 45, 52, 54, 53, 52, 45, 56, 97, 102, 101, 45, 53, 52, 100, 98, 52, 49, 55, 55, 56, 54, 101, 48, 0, 0, 0, 63, 0,
            0, 16, 65, 0, 0, 0, 0, 1, 0, 36, 0, 50, 101, 51, 56, 55, 57, 101, 48, 45, 98, 54, 55, 54, 45, 52, 53, 57, 100, 45,
            97, 54, 101, 98, 45, 57, 52, 99, 49, 53, 52, 53, 51, 52, 55, 53, 101, 0, 0, 128, 63, 0, 0, 16, 65, 0, 0, 0, 0, 1,
            0, 36, 0, 54, 54, 54, 57, 57, 54, 48, 54, 45, 49, 49, 56, 53, 45, 52, 54, 101, 97, 45, 56, 51, 54, 97, 45, 98, 55, 53,
            49, 55, 50, 98, 97, 99, 97, 51, 54, 0, 0, 128, 63, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57, 49, 98, 53, 52,
            55, 49, 98, 45, 98, 102, 51, 101, 45, 52, 49, 50, 51, 45, 56, 52, 48, 97, 45, 102, 101, 101, 101, 51, 50, 49, 56, 98, 100, 48,
            49, 0, 0, 0, 64, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 57, 54, 98, 57, 102, 98, 55, 45, 50, 49, 51, 51,
            45, 52, 52, 101, 56, 45, 98, 99, 101, 50, 45, 48, 56, 97, 101, 57, 56, 51, 50, 53, 49, 52, 98, 0, 0, 0, 64, 0, 0, 16,
            65, 0, 0, 0, 0, 1, 0, 36, 0, 100, 51, 55, 99, 102, 53, 99, 56, 45, 102, 101, 52, 55, 45, 52, 97, 52, 50, 45, 97, 102,
            101, 52, 45, 48, 51, 97, 51, 51, 101, 48, 100, 48, 97, 50, 97, 0, 0, 32, 64, 0, 0, 16, 65, 0, 0, 0, 0, 1, 0, 36,
            0, 102, 102, 101, 57, 52, 52, 99, 100, 45, 50, 99, 55, 102, 45, 52, 101, 101, 99, 45, 97, 99, 100, 97, 45, 50, 97, 48, 99, 97,
            50, 102, 101, 49, 57, 57, 54, 0, 0, 32, 64, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 51, 102, 49, 57, 57, 49, 52,
            49, 45, 57, 99, 54, 98, 45, 52, 102, 101, 50, 45, 56, 102, 54, 50, 45, 101, 53, 51, 51, 49, 102, 54, 99, 102, 97, 98, 57, 0,
            0, 96, 64, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 98, 57, 54, 53, 98, 51, 99, 51, 45, 48, 97, 102, 51, 45, 52,
            51, 55, 100, 45, 56, 57, 49, 49, 45, 53, 52, 50, 99, 48, 57, 50, 101, 55, 97, 48, 49, 0, 0, 96, 64, 0, 0, 16, 65, 0,
            0, 0, 0, 1, 0, 36, 0, 97, 101, 101, 54, 53, 53, 100, 57, 45, 97, 57, 50, 56, 45, 52, 100, 55, 56, 45, 57, 50, 102, 101,
            45, 56, 53, 48, 102, 99, 52, 55, 49, 97, 55, 102, 97, 0, 0, 32, 64, 0, 0, 48, 65, 0, 0, 0, 0, 1, 0, 36, 0, 57,
            100, 53, 51, 57, 98, 49, 52, 45, 49, 56, 98, 49, 45, 52, 49, 56, 50, 45, 56, 53, 102, 101, 45, 49, 57, 48, 48, 57, 102, 51,
            98, 49, 55, 101, 49, 0, 0, 96, 64, 0, 0, 32, 65, 0, 0, 0, 0, 1, 0, 36, 0, 97, 99, 54, 100, 48, 53, 101, 97, 45,
            56, 54, 48, 54, 45, 52, 52, 52, 48, 45, 56, 98, 54, 55, 45, 55, 54, 56, 57, 54, 99, 49, 98, 98, 50, 56, 56, 0, 0, 96,
            192, 0, 0, 108, 65, 0, 0, 0, 0, 1, 0, 36, 0, 98, 98, 50, 55, 53, 56, 53, 57, 45, 101, 51, 56, 53, 45, 52, 56, 54,
            102, 45, 98, 98, 53, 50, 45, 50, 56, 53, 56, 98, 100, 99, 101, 57, 98, 99, 53, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0,
            0, 208, 192, 31, 133, 163, 64, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 3, 0, 86, 97, 110, 0, 0,
            0, 0, 0, 0, 192, 65, 0, 0, 192, 63, 0, 0, 0, 0, 0, 0, 160, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 63,
            0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 36, 0, 98, 102, 57, 50, 101, 49, 50, 55, 45, 57, 101, 56, 54, 45, 52, 101, 52,
            57, 45, 98, 102, 53, 49, 45, 49, 102, 49, 48, 99, 98, 49, 102, 101, 102, 52, 55, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 208,
            64, 51, 51, 163, 64, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 0, 128, 0, 0, 128, 63, 0, 0, 224, 63, 0, 0, 0, 0, 0,
            11, 0, 86, 105, 99, 116, 111, 114, 121, 70, 108, 97, 103, 36, 0, 98, 102, 57, 50, 101, 49, 50, 55, 45, 57, 101, 56, 54, 45, 52,
            101, 52, 57, 45, 98, 102, 53, 49, 45, 49, 102, 49, 48, 99, 98, 49, 102, 101, 102, 52, 55, 1, 0, 0, 0, 0, 0, 1, 0, 0,
            0, 1, 0, 0, 0, 36, 0, 98, 102, 57, 50, 101, 49, 50, 55, 45, 57, 101, 56, 54, 45, 52, 101, 52, 57, 45, 98, 102, 53, 49,
            45, 49, 102, 49, 48, 99, 98, 49, 102, 101, 102, 52, 55, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 128, 192, 0, 0, 0, 0, 0,
            0, 0, 0, 16, 0, 84, 101, 114, 114, 97, 105, 110, 95, 66, 111, 111, 107, 69, 110, 100, 67, 0, 0, 0, 0, 0, 0, 64, 64, 0,
            0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 128, 64, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 84, 101, 114, 114, 97, 105, 110,
            95, 66, 111, 111, 107, 69, 110, 100, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 181, 0, 0, 192, 63, 0,
            0, 0, 0, 254, 255, 255, 64, 0, 0, 64, 64, 0, 159, 134, 1, 0, 100, 0, 0, 0, 100, 0, 0, 0, 100, 0, 0, 0, 0, 0,
            0, 0, 100, 0, 0, 0, 100, 0, 0, 0, 100, 0, 0, 0, 100, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 13, 0, 82, 67, 69, 32, 84, 114, 101, 98, 117, 99, 104, 101, 116, 108, 0, 67, 97, 110, 32, 121, 111, 117, 32,
            99, 97, 116, 97, 112, 117, 108, 116, 32, 116, 104, 101, 32, 99, 97, 114, 32, 116, 111, 32, 116, 104, 101, 32, 111, 116, 104, 101, 114, 32,
            115, 105, 100, 101, 32, 97, 110, 100, 32, 98, 97, 99, 107, 63, 10, 84, 114, 121, 32, 116, 111, 32, 117, 115, 101, 32, 116, 104, 101, 32,
            99, 117, 115, 116, 111, 109, 32, 115, 104, 97, 112, 101, 115, 32, 97, 115, 32, 112, 111, 119, 101, 114, 10, 10, 10, 10, 10, 45, 66, 121,
            58, 32, 66, 114, 97, 109, 50, 51, 50, 51, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    }
}
