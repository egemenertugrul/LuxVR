using System.Collections;
using UnityEngine;
using Valve.VR;

public partial class OVR_Handler : System.IDisposable
{
    public delegate void VREventHandler(VREvent_t e);

    public VREventHandler onVREvent;
    private void DefaultEventHandler(VREvent_t e){}

    public OVR_Handler() 
    {
        onVREvent += DefaultEventHandler;
    }

    public void UpdateAll()
    {
        while(PollNextEvent(ref pEvent))
            DigestEvent(pEvent);
        
        poseHandler.UpdatePoses();
        overlayHandler.UpdateOverlays();
    }

    private EVRInitError error = EVRInitError.None;
    private VREvent_t pEvent = new VREvent_t();

    public bool StartupOpenVR()
    {
        _VRSystem = OpenVR.Init(ref error, _applicationType);

        bool result = !ErrorCheck(error);
        
        if(result)
        {
            GetOpenVRExistingInterfaces();
            onOpenVRChange.Invoke(true);
            //AddApplicationManifest();
        }
        else
            ShutDownOpenVR();

        return result;
    }

    //public void AddApplicationManifest()
    //{
    //    _Applications?.AddApplicationManifest(Application.dataPath, false);
    //}

    public bool? GetApplicationAutorun()
    {
        return _Applications?.GetApplicationAutoLaunch(GetApplicationKey());
    }

    public void ToggleAutorunWithSteamVR(bool isAutorun)
    {
        _Applications?.SetApplicationAutoLaunch(GetApplicationKey(), isAutorun);
    }

    private string GetApplicationKey()
    {
        return OpenVRManifest.LoadOpenVRManifestSettings().appKey; // "steam.overlay.732230"
    }

    public void GetOpenVRExistingInterfaces()
    {
        _Compositor = OpenVR.Compositor;
        _Chaperone = OpenVR.Chaperone;
        _ChaperoneSetup = OpenVR.ChaperoneSetup;
        _Overlay = OpenVR.Overlay;
        _Settings = OpenVR.Settings;
        _Applications = OpenVR.Applications;
        _RenderModels = OpenVR.RenderModels;
    }

    public bool ShutDownOpenVR()
    {
        overlayHandler.VRShutdown();

        _VRSystem = null;

        _Compositor = null;
        _Chaperone = null;
        _ChaperoneSetup = null;
        _Overlay = null;
        _Settings = null;
        _Applications = null;
        _RenderModels = null;

        OpenVR.Shutdown();

        return true;
    }

    private bool ErrorCheck(EVRInitError error)
    {
        bool err = (error != EVRInitError.None);

        if(err)
            Debug.Log("VR Error: " + OpenVR.GetStringForHmdError(error));

        return err;
    }

    ~OVR_Handler()
    {
        Dispose();
    }

    public void Dispose()
    {
        ShutDownOpenVR();
        _instance = null;
    }

    public void SafeDispose()
    {
        if(_instance != null)
            return;
        _instance = null;
    }
}