using Foundation;
using UIKit;

namespace TableTop.Maui;

[Register("SceneDelegate")]
public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
{
    [Export("window")]
    public UIWindow? Window { get; set; }

    [Export("scene:willConnectToSession:options:")]
    public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
    {
        // This method is called when a new scene session is being created.
        // Use this method to select a configuration to create the new scene with.
    }

    [Export("sceneDidDisconnect:")]
    public void SceneDidDisconnect(UIScene scene)
    {
        // Called as the scene is being released by the system.
    }

    [Export("sceneDidBecomeActive:")]
    public void SceneDidBecomeActive(UIScene scene)
    {
        // Called when the scene has moved from an inactive state to an active state.
    }

    [Export("sceneWillResignActive:")]
    public void SceneWillResignActive(UIScene scene)
    {
        // Called when the scene will move from an active state to an inactive state.
    }

    [Export("sceneWillEnterForeground:")]
    public void SceneWillEnterForeground(UIScene scene)
    {
        // Called as the scene transitions from the background to the foreground.
    }

    [Export("sceneDidEnterBackground:")]
    public void SceneDidEnterBackground(UIScene scene)
    {
        // Called as the scene transitions from the foreground to the background.
    }
}
