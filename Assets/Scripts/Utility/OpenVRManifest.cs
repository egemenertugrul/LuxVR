// A script to generate and register an OpenVR manifest in a Unity-friendly way
// OpenVR manifest lets you show proper app name in SteamVR status screen instead of process name, set a custom image for the loading screen in compositor or even allow to launch your game from Steam interface
// Feel free to do anything you want with this script, but please keep this copyright notice intact.
// https://gist.github.com/krzys-h/98464aa2f4a1ad814358f8f078111366
// Author: krzys_h, 2018-02-11

// Usage:
// Put this script anywhere in your assets folder
// Click Edit > Project Settings > OpenVR Manifest
// Fill in the settings
// On build, everything will happen automatically!
// If you want, you can move the file Assets/Resources/OpenVRManifestSettings.asset wherever you wish, but it must be stored in a folder called "Resources"

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
#if !UNITY_EDITOR
using Valve.VR;
#endif

public class OpenVRManifest : ScriptableObject
{
    private const string MANIFEST_SETTINGS_ASSET_NAME = "OpenVRManifestSettings";
    private const int APP_IMAGE_RECOMMENDED_WIDTH = 460;
    private const int APP_IMAGE_RECOMMENDED_HEIGHT = 215;

    [Header("Is Enabled")]
    public bool isEnabled;

    [Header("App key")]
    [Tooltip("App key name to use, should be unique system-wide for your application, e.g. mycompany.myapp")]
    public string appKey = "";

    [Header("App parameters")]
    [Tooltip("The app image, used for example in loading screen while in compositor. Recommended size is 460x215")]
    public Texture2D appImage = null;

    [Tooltip("Alternatively, you can provide the app image as a web URL")]
    public string appImageUrl = "";

    [Tooltip("Launch arguments to use when launching this app via Steam/SteamVR interface")]
    public string launchArguments = "";

    [Header("Manifest settings")]
    [Tooltip("Whether to register the manifest as temporary. Temporary manifests are loaded only until next SteamVR restart. Recommended during development as otherwise it permanently registers path to the .vrmanifest file somewhere in SteamVR. Note: Apps from temporary manifests can't be launched from Steam")]
    public bool temporaryManifest = true;
    // Note: if you want to remove permanently registered manifest, look in <steamdir>/config/appconfig.json

    public static OpenVRManifest LoadOpenVRManifestSettings()
    {
        return Resources.Load<OpenVRManifest>(MANIFEST_SETTINGS_ASSET_NAME);
    }

#if UNITY_EDITOR
    // A menu option to open VR manifest settings
    [MenuItem("Edit/OpenVR Manifest")]
    public static void OpenSettings()
    {
        OpenVRManifest manifestSettingsAsset = LoadOpenVRManifestSettings();
        if (manifestSettingsAsset == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            manifestSettingsAsset = ScriptableObject.CreateInstance<OpenVRManifest>();
            AssetDatabase.CreateAsset(manifestSettingsAsset, "Assets/Resources/" + MANIFEST_SETTINGS_ASSET_NAME + ".asset");
            AssetDatabase.SaveAssets();
        }

        Selection.activeObject = manifestSettingsAsset;
    }
    //[MenuItem("Edit/OpenVR Manifest", true)]
    //public static bool OpenSettingsValidate()
    //{
    //    return Array.IndexOf(XRSettings.supportedDevices, "OpenVR") >= 0;
    //}
#endif

#if UNITY_EDITOR
    // A class representing the structure of OpenVR manifest JSON
    [Serializable]
    public sealed class OpenVRManifestData
    {
        [Serializable]
        public sealed class OpenVRManifestApplicationData
        {
            [Serializable]
            public sealed class OpenVRManifestApplicationStringsData
            {
                public string name;
            }

            public string app_key;
            public string launch_type = "binary";
            public string binary_path_windows;
            public string binary_path_osx; // TODO: Add support for binary_path_osx and binary_path_linux
            public string binary_path_linux;
            public string arguments = "";
            public string arguments_osx;
            public string arguments_linux;
            public string image_path = ""; // can be a file name or a URL
            //public Dictionary<string, OpenVRManifestApplicationStringsData> strings; // keys are language names, e.g. en_us // TODO: JSONUtility doesn't support Dictionary :/
            [Serializable]
            public sealed class OpenVRManifestApplicationStringsArray
            {
                public OpenVRManifestApplicationStringsData en_us;
            }
            public OpenVRManifestApplicationStringsArray strings;
            //public List<string> mime_types; // For some special apps? e.g. "mime_types": [ "vr/media_player" ]
        }

        public string source = "builtin";
        public List<OpenVRManifestApplicationData> applications;
    }

    // Code responsible for generating the manifest + copying the logo image in a postprocess hook
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {

        //if (Array.IndexOf(XRSettings.supportedDevices, "OpenVR") < 0)
        //    return;

        OpenVRManifest manifestSettingsAsset = LoadOpenVRManifestSettings();
        if (manifestSettingsAsset == null)
        {
            Debug.Log("OpenVR manifest not configured! Click Edit > Project Settings > OpenVR Manifest");
            return;
        }

        if (!manifestSettingsAsset.isEnabled)
        {
            return;
        }

        if (manifestSettingsAsset.appKey == "")
        {
            Debug.LogWarning("App key cannot be empty. OpenVR manifest will not be generated. See Edit > Project Settings > OpenVR Manifest");
            return;
        }

        if (manifestSettingsAsset.appImage != null && manifestSettingsAsset.appImageUrl != "")
        {
            Debug.LogWarning("Cannot use app image and app image URL at the same time. See Edit > Project Settings > OpenVR Manifest");
            throw new UnityException("Cannot use app image and app image URL at the same time");
        }

        string outputDirName = Path.GetDirectoryName(pathToBuiltProject);
        string programBinaryName = Path.GetFileName(pathToBuiltProject);
        string programBinaryNameBase = Path.GetFileNameWithoutExtension(pathToBuiltProject);

        // Put the app image inside output directory
        string appImage = "";
        if (manifestSettingsAsset.appImageUrl != "")
        {
            appImage = manifestSettingsAsset.appImageUrl;
        }
        else if (manifestSettingsAsset.appImage)
        {
            if (manifestSettingsAsset.appImage.width != APP_IMAGE_RECOMMENDED_WIDTH || manifestSettingsAsset.appImage.width != APP_IMAGE_RECOMMENDED_HEIGHT)
            {
                Debug.LogWarning("The OpenVR app image is not using the recommended size of " + APP_IMAGE_RECOMMENDED_WIDTH + "x" + APP_IMAGE_RECOMMENDED_HEIGHT + ", but we'll use it anyway");
            }

            // Make sure we can access the texture with EncodeToPNG
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(manifestSettingsAsset.appImage)) as TextureImporter;
            if (importer)
            {
                bool dirty = false;
                if (!importer.isReadable)
                {
                    Debug.Log("Texture " + manifestSettingsAsset.appImage + " did not have script read access enabled, enabling now...");
                    importer.isReadable = true;
                    dirty = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    Debug.Log("Texture " + manifestSettingsAsset.appImage + " had compression enabled, disabling now...");
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    dirty = true;
                }
                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    Debug.Log("Texture " + manifestSettingsAsset.appImage + " had NPOT scaling enabled, disabling now...");
                    importer.npotScale = TextureImporterNPOTScale.None;
                    dirty = true;
                }
                if (importer.mipmapEnabled)
                {
                    Debug.Log("Texture " + manifestSettingsAsset.appImage + " had mipmaps enabled, disabling now...");
                    importer.mipmapEnabled = false;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                }
            }
            else
            {
                Debug.LogWarning("Unable to access TextureImporter for " + manifestSettingsAsset.appImage.name + " ?");
            }

            // And save the texture in build directory
            string appImageFilename = programBinaryNameBase + "_vrlogo.png";
            string appImagePath = Path.Combine(outputDirName, appImageFilename);

            byte[] appImageData = manifestSettingsAsset.appImage.EncodeToPNG();
            File.WriteAllBytes(appImagePath, appImageData);

            appImage = appImageFilename;
        }

        // Generate the manifest file
        OpenVRManifestData jsonData = new OpenVRManifestData()
        {
            applications = new List<OpenVRManifestData.OpenVRManifestApplicationData>()
            {
                new OpenVRManifestData.OpenVRManifestApplicationData()
                {
                    app_key = manifestSettingsAsset.appKey,
                    binary_path_windows = programBinaryName,
                    arguments = manifestSettingsAsset.launchArguments,
                    image_path = appImage,
                    strings = new OpenVRManifestData.OpenVRManifestApplicationData.OpenVRManifestApplicationStringsArray()
                    {
                        en_us = new OpenVRManifestData.OpenVRManifestApplicationData.OpenVRManifestApplicationStringsData()
                        {
                            name = Application.productName
                        }
                    }
                }
            }
        };

        string json = JsonUtility.ToJson(jsonData, true);

        string vrmanifestPath = Path.Combine(outputDirName, programBinaryNameBase + ".vrmanifest");
        File.WriteAllText(vrmanifestPath, json);
    }
#endif


    // Code to register the manifest on runtime
    // Note: Don't put the #if !UNITY_EDITOR on the outside of this block, it won't work because of a Unity bug (see https://stackoverflow.com/questions/44655667/runtimeinitializeonload-not-running-with-conditional-compilation)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnRuntimeLoad()
    {
#if !UNITY_EDITOR
        OpenVRManifest manifestSettingsAsset = LoadOpenVRManifestSettings();
        if (manifestSettingsAsset == null)
        {
            Debug.Log("OpenVR manifest not configured!");
            return;
        }

        if (!manifestSettingsAsset.isEnabled)
        {
            return;
        }
        
        // At this point, OpenVR may be already initialized (if SteamVR is enabled by default in the first scene) or not (if None is at top
        // of supported SDKs list, that is the case e.g. when using VRTK, or your first scene just has no VR enabled at all).
        // If it's not, we'll have to handle initialization outselves
        
        if (XRSettings.loadedDeviceName != "OpenVR")
        {
            EVRInitError error = EVRInitError.None;
            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Utility);
            if (error != EVRInitError.None)
            {
                Debug.LogError("Failed to initialize OpenVR to register manifest, error = " + error.ToString());
                return;
            }
        }

        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName; // TODO: This is UGLY! Why doesn't Unity have this
        string exeDir = Path.GetDirectoryName(exePath);
        string exeNameBase = Path.GetFileNameWithoutExtension(exePath);
        string vrmanifestPath = Path.Combine(exeDir, exeNameBase + ".vrmanifest");
        Debug.Log("Registering the OpenVR manifest from "+vrmanifestPath);
        
        OpenVR.Applications.AddApplicationManifest(vrmanifestPath, manifestSettingsAsset.temporaryManifest);

        // To make sure that the new settings get applied immediately
        // (without this it works only after app restart)
        // PS. OpenVR documentation sucks, it says that if you pass 0 instead of process id it will use the calling process automatically but it actually doesn't
        OpenVR.Applications.IdentifyApplication((uint) System.Diagnostics.Process.GetCurrentProcess().Id, manifestSettingsAsset.appKey);

        // And don't forget to clean up afterwards!
        if (XRSettings.loadedDeviceName != "OpenVR")
        {
            OpenVR.Shutdown();
        }
#endif
    }
}