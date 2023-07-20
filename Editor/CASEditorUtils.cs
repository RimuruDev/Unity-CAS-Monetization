﻿//
//  Clever Ads Solutions Unity Plugin
//
//  Copyright © 2023 CleverAdsSolutions. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Networking;

namespace CAS.UEditor
{
    public static class CASEditorUtils
    {
        #region Constants
        public const string packageName = "com.cleversolutions.ads.unity";
        public const string androidAdmobSampleAppID = "ca-app-pub-3940256099942544~3347511713";
        public const string iosAdmobSampleAppID = "ca-app-pub-3940256099942544~1458002511";

        public const string rootCASFolderPath = "Assets/CleverAdsSolutions";
        public const string editorFolderPath = rootCASFolderPath + "/Editor";
        public const string androidLibFolderPath = "Assets/Plugins/Android/CASPlugin.androidlib";
        public const string androidResSettingsPath = androidLibFolderPath + "/res/raw/cas_settings";
        public const string androidLibManifestPath = androidLibFolderPath + "/AndroidManifest.xml";
        public const string androidLibPropertiesPath = androidLibFolderPath + "/project.properties";
        public const string androidLibNetworkConfigPath = androidLibFolderPath + "/res/xml/meta_network_security_config.xml";

        public const string iosResSettingsPath = "Library/ios_cas_settings";

        public const string promoDependency = "CrossPromotion";

        public const string androidPluginsPath = "Assets/Plugins/Android/";
        public const string mainGradlePath = androidPluginsPath + "mainTemplate.gradle";
        public const string launcherGradlePath = androidPluginsPath + "launcherTemplate.gradle";
        public const string projectGradlePath = androidPluginsPath + "baseProjectTemplate.gradle";
        public const string propertiesGradlePath = androidPluginsPath + "gradleTemplate.properties";
        public const string settingsGradlePath = androidPluginsPath + "settingsTemplate.gradle";
        public const string packageManifestPath = "Packages/manifest.json";

        public const int targetAndroidVersion = 21;
        public const int targetIOSVersion = 12;

        public const string gitRootURL = "https://github.com/cleveradssolutions/";
        public const string websiteURL = "https://cleveradssolutions.com";

        public static System.Version minEDM4UVersion
        {
#if UNITY_2022_2_OR_NEWER
            get { return new System.Version(1, 2, 176); }
#else
            get { return new System.Version(1, 2, 174); }
#endif
        }
        #endregion

        #region Internal Constants
        internal const string gitUnityRepo = "CAS-Unity";
        internal const string gitUnityRepoURL = gitRootURL + gitUnityRepo;
        internal const string supportURL = gitUnityRepoURL + "#support";
        internal const string gitAppAdsTxtRepoUrl = gitRootURL + "App-ads.txt";
        internal const string attributionReportEndPoint = "https://";

        internal const string logTag = "[CAS.AI] ";
        internal const string logAutoFeature = "\nYou can disable this automatic feature by `Assets > CleverAdsSolutions > Settings > Other settings` menu.\n";
        internal const string editorRuntimeActiveAdPrefs = "typesadsavailable";
        internal const string editorLatestVersionPrefs = "cas_last_ver_";
        internal const string editorLatestVersionTimestampPrefs = "cas_last_ver_time_";
        internal const string iosSKAdNetworksTemplateFile = "CASSKAdNetworks.txt";

        internal const string legacyUnityAdsPackageName = "com.unity.ads";
        internal const string editorIconNamePrefix = "cas_editoricon_";
        internal const string latestEMD4uURL = "https://github.com/googlesamples/unity-jar-resolver/raw/master/external-dependency-manager-latest.unitypackage";
        #endregion

        #region Menu items
        [MenuItem("Assets/CleverAdsSolutions/Android Settings...", priority = 1010)]
        public static void OpenAndroidSettingsWindow()
        {
            OpenSettingsWindow(BuildTarget.Android);
        }

        [MenuItem("Assets/CleverAdsSolutions/iOS Settings...", priority = 1011)]
        public static void OpenIOSSettingsWindow()
        {
            OpenSettingsWindow(BuildTarget.iOS);
        }

        [MenuItem("Assets/CleverAdsSolutions/Documentation...", priority = 1031)]
        public static void OpenDocumentationMenu()
        {
            OpenDocumentation(gitUnityRepo);
        }

        [MenuItem("Assets/CleverAdsSolutions/Report Issue...", priority = 1032)]
        public static void ReportIssueMenu()
        {
            Application.OpenURL(gitRootURL + gitUnityRepo + "/issues");
        }

#if UNITY_ANDROID || UNITY_IOS || CASDeveloper
        [MenuItem("Assets/CleverAdsSolutions/Configure project", priority = 1051)]
        public static void ConfigureProjectForTargetPlatform()
        {
            try
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                if (target == BuildTarget.Android)
                    TryResolveAndroidDependencies(); // Resolve before call configure
                CASPreprocessBuild.ConfigureProject(target, CASEditorSettings.Load());
            }
            finally
            {
                if (!IsBatchMode())
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Configure project",
                        "CAS Plugin has successfully applied all required configurations to your project.",
                        "Ok");
                }
            }
        }
#endif

        #endregion

        #region Public API
        public static string GetName(this AdNetwork network)
        {
            // Fix Wrong String implementation of deprecated Enums
            switch (network)
            {
                case Dependency.adBase:
                    return Dependency.adBaseName;
                case AdNetwork.DigitalTurbine:
                    return "DigitalTurbine";
                case AdNetwork.AudienceNetwork:
                    return "AudienceNetwork";
                default:
                    return network.ToString();
            }
        }

        public static string GetNativeSettingsPath(BuildTarget platform, string managerId)
        {
            if (string.IsNullOrEmpty(managerId))
                return "";

            string root = platform == BuildTarget.Android ? androidResSettingsPath : iosResSettingsPath;
            string suffixChar = char.ToLower(managerId[managerId.Length - 1]).ToString();
            return root + managerId.Length.ToString() + suffixChar + ".json";
        }

        public static CASInitSettings GetSettingsAsset(BuildTarget platform, bool create = true)
        {
            return (CASInitSettings)GetSettingsAsset(
                "CASSettings" + platform.ToString(), "Assets/Resources", typeof(CASInitSettings), create, "Resources");
        }

        public static UnityEngine.Object GetSettingsAsset(string name, string location, Type type, bool create, string requireDir)
        {
            var found = AssetDatabase.FindAssets(name, new string[] { "Assets" });
            for (int i = 0; i < found.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(found[i]);
                if (assetPath.StartsWith("Assets/") && assetPath.EndsWith(".asset"))
                {
                    if (string.IsNullOrEmpty(requireDir) || Path.GetDirectoryName(assetPath).EndsWith(requireDir))
                    {
                        try
                        {
                            var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                            if (asset)
                                return asset;
                            AssetDatabase.DeleteAsset(assetPath);
                        }
                        catch { }
                    }
                }
            }
            if (!create)
                return null;
            var result = ScriptableObject.CreateInstance(type);
            if (!AssetDatabase.IsValidFolder(location))
            {
                Directory.CreateDirectory(location);
                AssetDatabase.ImportAsset(location);
            }
            AssetDatabase.CreateAsset(result, location + "/" + name + ".asset");
            return result;
        }

        public static bool IsDependencyExists(string name, BuildTarget platform)
        {
            return AssetDatabase.FindAssets(GetDependencyName(name, platform)).Length > 0;
        }

        public static string GetDependencyName(string name, BuildTarget platform)
        {
            var platformPrefix = name == Dependency.adBaseName ? "" : platform.ToString();
            return "CAS" + platformPrefix + name + "Dependencies";
        }

        private static bool IsPathInPackage(string path)
        {
            return !path.StartsWith("Assets/");
        }

        private static bool IsPathInHomeAssets(string path)
        {
            return path.StartsWith(rootCASFolderPath);
        }

        public static string GetDependencyPath(string name, BuildTarget platform)
        {
            var depFileName = GetDependencyName(name, platform);
            var foundDepFiles = AssetDatabase.FindAssets(depFileName);
            string result = null;
            for (int i = 0; i < foundDepFiles.Length; i++)
            {
                var pathToFile = AssetDatabase.GUIDToAssetPath(foundDepFiles[i]);
                if (pathToFile.EndsWith(depFileName + ".xml"))
                {
                    if (result == null)
                    {
                        result = pathToFile;
                    }
                    else
                    {
                        string removeFile;
                        if (!IsPathInPackage(pathToFile) && (IsPathInPackage(result) || IsPathInHomeAssets(result)))
                        {
                            removeFile = pathToFile;
                        }
                        else
                        {
                            removeFile = result;
                            result = pathToFile;
                        }
                        Debug.LogWarning(logTag + "Removed duplicate dependency at path: " + removeFile);
                        AssetDatabase.DeleteAsset(removeFile);
                    }
                }
            }
            return result;
        }

        public static string GetDependencyPathOrDefault(string name, BuildTarget platform)
        {
            var foundPath = GetDependencyPath(name, platform);
            if (foundPath != null)
                return foundPath;

            return editorFolderPath + "/" + GetDependencyName(name, platform) + ".xml";
        }

        public static KeyValuePair[] DefaultUserTrackingUsageDescription()
        {
            return new KeyValuePair[]{
                new KeyValuePair( "en", "Get ads that are more interesting and support keeping this game free by allowing tracking" ),
                new KeyValuePair( "de", "Erhalten Sie interessantere Anzeigen und unterstützen Sie, dieses Spiel kostenlos zu halten, indem Sie Tracking zulassen" ),
                new KeyValuePair( "es", "Obtenga anuncios más interesantes y ayude a mantener este juego gratuito al permitir el seguimiento" ),
                new KeyValuePair( "fr", "Obtenez des publicités plus intéressantes et aidez à garder ce jeu gratuit en autorisant le suivi" ),
                new KeyValuePair( "uk", "Отримуйте більш цікаву рекламу та допомагайте цьому додатку залишатися безкоштовним, поділившись інформацією про пристрій"),
                new KeyValuePair( "ru", "Получайте более интересную рекламу и помогайте этой игре быть бесплатной, поделившись информацией об устройстве" ),
            };
        }

        public static bool isUseAdvertiserIdLimited()
        {
            var audience = GetSettingsAsset(BuildTarget.Android, false).defaultAudienceTagged;
            return CASEditorSettings.Load().isUseAdvertiserIdLimited(audience);
        }

        public static string GetNewVersionOrNull(string repo, string currVersion, bool force, Action<string> asyncResult = null)
        {
            try
            {
                string remoteVersion = null;
                if (!force)
                {
                    var editorSettings = CASEditorSettings.Load();
                    if (!editorSettings.autoCheckForUpdatesEnabled)
                        return null;

                    if (!HasTimePassed(editorLatestVersionTimestampPrefs + repo, 1, false))
                        remoteVersion = EditorPrefs.GetString(editorLatestVersionPrefs + repo);
                }

                if (string.IsNullOrEmpty(remoteVersion))
                    return RequestLatestVersion(repo, currVersion, asyncResult);

                return GetNewVersionOrNull(currVersion, remoteVersion);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return null;
        }

        private static string GetNewVersionOrNull(string local, string remote)
        {
            if (remote != local && ParseVersionToCompare(local) < ParseVersionToCompare(remote))
                return remote;
            return null;
        }

        private static System.Version ParseVersionToCompare(string versionName)
        {
            try
            {
                int separator = versionName.IndexOf('-');
                // Append Revision version for pre release
                // And 9 Revision for release
                if (separator > 0)
                    versionName = versionName.Substring(0, versionName.Length - separator + 1) +
                        "." + versionName[versionName.Length - 1];
                else
                    versionName += ".9";
                return new System.Version(versionName);
            }
#if CASDeveloper
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            catch {}
#endif
            return new System.Version(0, 1);
        }

        public static bool IsPackageExist(string package, string manifest = null)
        {
            if (manifest == null && File.Exists(packageManifestPath))
                manifest = File.ReadAllText(packageManifestPath);
            return manifest != null && manifest.Contains("\"" + package + "\"");
        }

        // Deprecated. Replaced with OnHeaderGUI()
        public static void LinksToolbarGUI(string gitRepoName)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Support", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                Application.OpenURL(gitRootURL + gitRepoName + "#support");
            if (GUILayout.Button("Wiki", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                OpenDocumentation(gitRepoName);
            if (GUILayout.Button("CAS.ai", EditorStyles.toolbarButton))
                Application.OpenURL(websiteURL);
            EditorGUILayout.EndHorizontal();
        }

        public static void OnHeaderGUI(string gitRepoName, bool allowedPackageUpdate, string currVersion, ref string newCASVersion)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            HelpStyles.HelpButton(gitRootURL + gitRepoName + "/wiki");

            if (GUILayout.Button(gitRepoName + " " + currVersion, EditorStyles.toolbarButton))
                Application.OpenURL(gitRootURL + gitRepoName + "/releases");
            if (GUILayout.Button("Check for Updates", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
                newCASVersion = GetNewVersionOrNull(gitRepoName, currVersion, true);
                string message = string.IsNullOrEmpty(newCASVersion) ? "You are using the latest version."
                    : "There is a new version " + newCASVersion + " of the " + gitRepoName + " available for update.";
                EditorUtility.DisplayDialog("Check for Updates", message, "OK");
            }

            if (GUILayout.Button("Support", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                Application.OpenURL(gitRootURL + gitRepoName + "#support");
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(newCASVersion))
            {
                if (HelpStyles.WarningWithButton("There is a new version " + newCASVersion + " of the " + gitRepoName + " available for update.", "Update"))
                {
#if UNITY_2018_4_OR_NEWER
                    if (allowedPackageUpdate)
                        UpdatePackageManagerRepo(gitRepoName, newCASVersion);
                    else
#endif
                        InstallUnityPackagePlugin(GetUnityPackagePluginFromReleases(newCASVersion, gitRepoName))
                            .WithProgress("Download plugin " + gitRepoName);
                }
            }
        }

        #endregion

        #region EDM4U Reflection
        internal static void CheckAssemblyForType<T>(string assembly)
        {
            var targetType = typeof(T);
            var targetAssembly = targetType.Assembly.GetName().Name;
            if (targetAssembly != assembly)
                Debug.LogError(logTag + targetType.FullName + " in assembly: " + targetAssembly + " Expecting: " + assembly);
        }

        internal static Type GetAndroidDependenciesResolverType()
        {
            const string assemblyName = "Google.JarResolver";
#if CASDeveloper
            CheckAssemblyForType<GooglePlayServices.PlayServicesResolver>(assemblyName);
#endif
            return Type.GetType("GooglePlayServices.PlayServicesResolver, " + assemblyName, false);
        }

        public static bool IsAndroidDependenciesResolverExist()
        {
            return GetAndroidDependenciesResolverType() != null;
        }

        public static System.Version GetEDM4UVersion(BuildTarget platform)
        {
#if CASDeveloper
            CheckAssemblyForType<Google.AndroidResolverVersionNumber>("Google.JarResolver");
            CheckAssemblyForType<Google.IOSResolverVersionNumber>("Google.IOSResolver");
#endif
            try
            {
                Type resolverType = null;
                if (platform == BuildTarget.Android)
                    resolverType = Type.GetType("Google.AndroidResolverVersionNumber, Google.JarResolver", false);
                else if (platform == BuildTarget.iOS)
                    resolverType = Type.GetType("Google.IOSResolverVersionNumber, Google.IOSResolver", false);
                if (resolverType == null)
                    return null;
                return resolverType.GetProperty("Value").GetValue(null, null) as System.Version;
            }
            catch
            {
                return null;
            }
        }

        public static bool TryResolveAndroidDependencies(bool force = true)
        {
#if UNITY_ANDROID
            CASPreprocessGradle.UpdateGradleTemplateIfNeed();
#endif
            bool success = false;
            var resolverType = GetAndroidDependenciesResolverType();
            if (resolverType == null)
                return success;

            bool needResolve = force;
            if (!needResolve)
                needResolve = (bool)resolverType
                    .GetProperty("AutomaticResolutionEnabled", BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null, null);
            if (!needResolve)
                return success;
            try
            {
                EditorUtility.DisplayProgressBar("Hold on.", "Resolve Android dependencies", 0.6f);
                success = (bool)resolverType
                    .GetMethod("ResolveSync", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(bool) }, null)
                    .Invoke(null, new object[] { true });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            EditorUtility.ClearProgressBar();
            return success;
        }

        public static T GetAndroidResolverSetting<T>(string property)
        {
            const string assemblyName = "Google.JarResolver";
            const string settingsTypeName = "GooglePlayServices.SettingsDialog, " + assemblyName;
#if CASDeveloper
            CheckAssemblyForType<GooglePlayServices.SettingsDialog>(assemblyName);
#endif
            try
            {
                var settingsType = Type.GetType(settingsTypeName, false);
                if (settingsType != null)
                {
                    return (T)settingsType.GetProperty(property, BindingFlags.NonPublic | BindingFlags.Static)
                            .GetValue(null, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(logTag + settingsTypeName + " error: " + e.Message);
            }
            return default(T);
        }

        public static T GetIOSResolverSetting<T>(string property)
        {
            const string assemblyName = "Google.IOSResolver";
            const string settingsTypeName = "Google.IOSResolver, " + assemblyName;
#if CASDeveloper
            CheckAssemblyForType<Google.IOSResolver>(assemblyName);
#endif
            try
            {
                var settingsType = Type.GetType(settingsTypeName, false);
                if (settingsType != null)
                    return (T)settingsType.GetProperty(property, BindingFlags.Public | BindingFlags.Static)
                            .GetValue(null, null);
            }
            catch (Exception e)
            {
                Debug.LogWarning(logTag + settingsTypeName + " error: " + e.Message);
            }
            return default(T);
        }
        #endregion

        #region Internal API

        internal static EditorWebRequest InstallUnityPackagePlugin(string url)
        {
            string cacheFile = Path.GetFullPath("Library/" + Path.GetFileName(url));
            var request = new EditorWebRequest(url)
                .ToFile(cacheFile);
            request.StartAsync((response) =>
                {
                    response.Dispose();
                    AssetDatabase.ImportPackage(cacheFile, true);
                    File.Delete(cacheFile);
                });
            return request;
        }

        internal static string GetUnityPackagePluginFromReleases(string version, string repo)
        {
            return gitRootURL + repo + "/releases/download/" + version + "/CleverAdsSolutions.unitypackage";
        }

        internal static void OpenDocumentation(string gitRepoName)
        {
            Application.OpenURL(gitRootURL + gitRepoName + "/wiki");
        }

        internal static void OpenSettingsWindow(BuildTarget target)
        {
            var asset = GetSettingsAsset(target);
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        internal static string GetAdmobAppIdFromJson(string json)
        {
            return JsonUtility.FromJson<AdmobAppIdData>(json).admob_app_id;
        }

        internal static string GetTemplatePath(string templateFile)
        {
            string templateFolder = "/Templates/" + templateFile;
            string path = "Packages/" + packageName + templateFolder;
            if (!File.Exists(path))
            {
                path = rootCASFolderPath + templateFolder;
                if (!File.Exists(path))
                {
                    Debug.LogError(logTag + "Template " + templateFile + " file not found. Try reimport CAS package.");
                    return null;
                }
            }
            return path;
        }

        internal static void WriteToAsset(string path, params string[] data)
        {
            WriteToAsset(path, true, data);
        }

        internal static void WriteToAsset(string path, bool overwrite, params string[] data)
        {
            if (data.Length == 0)
                return;
            try
            {
                var fullPath = Path.GetFullPath(path);
                var needImport = !AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (!overwrite && !needImport) return;

                if (needImport)
                {
                    var directoryPath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);
                }

                if (data.Length == 1)
                    File.WriteAllText(fullPath, data[0]);
                else
                    File.WriteAllLines(fullPath, data);
                File.SetLastWriteTime(fullPath, System.DateTime.Now);
                if (needImport)
                    AssetDatabase.ImportAsset(path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal static void DialogOrCancelBuild(string message, BuildTarget target = BuildTarget.NoTarget, string btn = "Continue")
        {
            if (!IsBatchMode() && !EditorUtility.DisplayDialog("CAS Configure project", message, btn, "Cancel build"))
                StopBuildWithMessage("Cancel build: " + message, target);
        }

        internal static void StopBuildWithMessage(string message, BuildTarget target)
        {
            EditorUtility.ClearProgressBar();
            if (target != BuildTarget.NoTarget
                && !IsBatchMode()
                && EditorUtility.DisplayDialog("CAS Configure project", message, "Open Settings", "Close"))
            {
                OpenSettingsWindow(target);
            }

#if UNITY_2018_1_OR_NEWER
            throw new BuildFailedException(logTag + message);
#elif UNITY_2017_1_OR_NEWER
            throw new BuildPlayerWindow.BuildMethodException( logTag + message );
#else
            throw new OperationCanceledException(logTag + message);
#endif
        }

        internal static string SelectSettingsFileAndGetAppId(string managerID, BuildTarget platform)
        {
            string filePath = "";
            try
            {
                filePath = EditorUtility.OpenFilePanelWithFilters(
                   "Select CAS Settings file for build", "", new[] { "Settings file", "json" });
                if (!string.IsNullOrEmpty(filePath))
                {
                    var content = File.ReadAllText(filePath);
                    WriteToAsset(GetNativeSettingsPath(platform, managerID), content);
                    return GetAdmobAppIdFromJson(content);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            StopBuildWithMessage("Selected wrong settings file: " + filePath, BuildTarget.NoTarget);
            return null;
        }

        internal static void GetCrossPromoAlias(BuildTarget platform, string managerId, HashSet<string> result)
        {
            if (!IsDependencyExists(promoDependency, platform))
                return;

            string pattern = "alias\\\": \\\""; //: "iOSBundle\\\": \\\"";
            string cachePath = Path.GetFullPath(GetNativeSettingsPath(platform, managerId));

            if (File.Exists(cachePath))
            {
                try
                {
                    string content = File.ReadAllText(cachePath);
                    int beginIndex = content.IndexOf(pattern);
                    while (beginIndex > 0)
                    {
                        beginIndex += pattern.Length;
                        var endIndex = content.IndexOf('\\', beginIndex);
                        result.Add(content.Substring(beginIndex, endIndex - beginIndex));
                        beginIndex = content.IndexOf(pattern, endIndex);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return;
        }

        private static string RequestLatestVersion(string repo, string currVersion, Action<string> asyncResult)
        {
            string url = "https://api.github.com/repos/cleveradssolutions/" + repo + "/releases/latest";
            string remoteVersion = null;
            EditorWebRequest.OnComplete handler = (response) =>
            {
                try
                {
                    var content = response.ReadContent();
                    if (content != null)
                        remoteVersion = JsonUtility.FromJson<GitVersionInfo>(content).tag_name;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                if (string.IsNullOrEmpty(remoteVersion))
                {
                    Debug.LogWarning(logTag + "Check " + repo + " updates failed. Code " +
                        response.responseCode + ". " + response.error);
                    SaveLatestRepoVersion(repo, currVersion);
                }
                else
                {
                    SaveLatestRepoVersion(repo, remoteVersion);
                    remoteVersion = GetNewVersionOrNull(currVersion, remoteVersion);
                }
                response.Dispose();

                if (asyncResult != null)
                    asyncResult(remoteVersion);
            };
            var request = new EditorWebRequest(url);
            if (asyncResult == null)
            {
                request.WithProgress("Check latest CAS version")
                    .StartSync();
                handler(request);
            }
            else
            {
                request.StartAsync(handler);
            }
            return remoteVersion;
        }

        internal static void UpdatePackageManagerRepo(string gitRepoName, string version)
        {
            var request = UnityEditor.PackageManager.Client.Add(gitRootURL + gitRepoName + ".git#" + version);
            new EditorOperation(() =>
            {
                if (request.IsCompleted)
                {
                    if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                        Debug.Log("Package Manager: Updated " + request.Result.displayName);
                    else if (request.Status >= UnityEditor.PackageManager.StatusCode.Failure)
                        Debug.LogError(request.Error.message);
                    return false;
                }
                return true;
            });
        }

        internal static void RemovePackage(string packageName)
        {
            var request = UnityEditor.PackageManager.Client.Remove(packageName);
            new EditorOperation(() =>
            {
                if (request.IsCompleted)
                {
                    if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                        Debug.Log("Package Manager: Removed " + packageName);
                    else if (request.Status >= UnityEditor.PackageManager.StatusCode.Failure)
                        Debug.LogError(request.Error.message);
                    return false;
                }
                return true;
            });
        }

        private static void SaveLatestRepoVersion(string repo, string version)
        {
            EditorPrefs.SetString(editorLatestVersionPrefs + repo, version);
            EditorPrefs.SetString(editorLatestVersionTimestampPrefs + repo, DateTime.Now.ToBinary().ToString());
        }

        internal static bool HasTimePassed(string prefKey, int days, bool projectOnly)
        {
            string pref;
            if (projectOnly)
                pref = PlayerPrefs.GetString(prefKey, string.Empty);
            else
                pref = EditorPrefs.GetString(prefKey, string.Empty);

            if (string.IsNullOrEmpty(pref))
            {
                return true;
            }
            else
            {
                DateTime checkTime;
                try
                {
                    long binartDate = long.Parse(pref);
                    checkTime = DateTime.FromBinary(binartDate);
                }
                catch
                {
                    return true;
                }
                checkTime = checkTime.Add(TimeSpan.FromDays(days));
                return DateTime.Compare(DateTime.Now, checkTime) > 0; // Now time is later than checkTime
            }
        }

        internal static bool IsBatchMode()
        {
#if UNITY_2018_3_OR_NEWER
            //!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("UNITY_THISISABUILDMACHINE"));
            return Application.isBatchMode;
#else
            return false;
#endif
        }
        #endregion
    }

    [Serializable]
    internal class AdmobAppIdData
    {
        public string admob_app_id = null;
    }

    [Serializable]
    internal class GitVersionInfo
    {
        public string tag_name = null;
    }

    [Serializable]
    public class KeyValuePair
    {
        public string key;
        public string value;

        public KeyValuePair(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public static class HelpStyles
    {
        public static GUIStyle labelStyle;
        public static GUIStyle wordWrapTextAred;
        public static GUIStyle largeTitleStyle;
        private static GUIStyle boxScopeStyle;

        public static GUIContent helpIconContent;
        public static GUIContent errorIconContent;
        private static GUIContent tempContent;

        public static Texture[] formatIcons;

        static HelpStyles()
        {
            labelStyle = new GUIStyle("AssetLabel");
            labelStyle.fontSize = 9;
            labelStyle.fixedHeight = 10;
            labelStyle.padding = new RectOffset(4, 3, 0, 0);
            boxScopeStyle = new GUIStyle(EditorStyles.helpBox);
            boxScopeStyle.padding = new RectOffset(6, 6, 6, 6);
            wordWrapTextAred = new GUIStyle(EditorStyles.textArea);
            wordWrapTextAred.wordWrap = true;
            errorIconContent = EditorGUIUtility.IconContent("d_console.erroricon.sml");
            errorIconContent = new GUIContent(string.Empty, errorIconContent.image,
                "Remove dependency.\nThe dependency cannot be used in current configuration.");
            helpIconContent = EditorGUIUtility.IconContent("_Help");
            helpIconContent = new GUIContent(string.Empty, helpIconContent.image,
                "Open home page of the mediation network.");
            tempContent = new GUIContent();
            largeTitleStyle = new GUIStyle(EditorStyles.boldLabel);
            largeTitleStyle.fontSize = 18;

            formatIcons = new Texture[5 * 2];
            var editorIcons = AssetDatabase.FindAssets(CASEditorUtils.editorIconNamePrefix);
            for (int i = 0; i < editorIcons.Length; i++)
            {
                var iconPath = AssetDatabase.GUIDToAssetPath(editorIcons[i]);
                var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                var suffix = iconPath.Substring(iconPath.LastIndexOf('_') + 1);
                var index = -1;
                switch (suffix)
                {
                    case "banner.png":
                        index = 0;
                        break;
                    case "inter.png":
                        index = 1;
                        break;
                    case "reward.png":
                        index = 2;
                        break;
                    case "native.png":
                        index = 3;
                        break;
                    case "mrec.png":
                        index = 4;
                        break;
                }
                if (index > -1)
                    formatIcons[index + (iconPath.Contains("_dark_") ? 5 : 0)] = asset;
            }
        }

        public static Texture GetFormatIcon(AdFlags format, bool active)
        {
            int iconIndex;
            switch (format)
            {
                case AdFlags.Banner: iconIndex = 0; break;
                case AdFlags.Interstitial: iconIndex = 1; break;
                case AdFlags.Rewarded: iconIndex = 2; break;
                case AdFlags.Native: iconIndex = 3; break;
                default: return null;
            }
            return formatIcons[active ? iconIndex : iconIndex + 5];
        }

        public static void BeginBoxScope()
        {
            EditorGUILayout.BeginVertical(boxScopeStyle);
        }

        public static void EndBoxScope()
        {
            EditorGUILayout.EndVertical();
        }

        public static GUIContent GetContent(string text, Texture image = null, string tooltip = "")
        {
            tempContent.text = text;
            tempContent.image = image;
            tempContent.tooltip = tooltip;
            return tempContent;
        }

        public static void Devider()
        {
            var dividerRect = EditorGUILayout.GetControlRect(GUILayout.Height(1));
            if (Event.current.type == EventType.Repaint) //draw the divider
                GUI.skin.box.Draw(dividerRect, GUIContent.none, 0);
        }

        public static void HelpButton(string url)
        {
            if (GUILayout.Button(helpIconContent, EditorStyles.label, GUILayout.ExpandWidth(false)))
                Application.OpenURL(url);

        }

        public static bool WarningWithButton(string message, string btnText, MessageType type = MessageType.Warning)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(message, type);
            // Expand height correct work with Unity 2020 or newer
#if UNITY_2020_1_OR_NEWER
            var height = GUILayout.ExpandHeight(true);
#else
            var height = GUILayout.Height(type == MessageType.None ? 28 : 38);
#endif
            var action = GUILayout.Button(btnText, GUILayout.ExpandWidth(false), height);
            EditorGUILayout.EndHorizontal();
            return action;
        }
    }

    internal class EditorOperation
    {
        private readonly Func<bool> operation;

        internal EditorOperation(Func<bool> operation)
        {
            this.operation = operation;
            EditorApplication.update += Process;
        }

        private void Process()
        {
            if (!operation())
                EditorApplication.update -= Process;
        }
    }

    internal class EditorWebRequest : UnityWebRequest
    {
        internal delegate void OnComplete(EditorWebRequest request);

        private OnComplete complete;
        private string title;

        internal EditorWebRequest(string url) : base(url, "GET", null, null)
        {
            timeout = 10;
        }

        public EditorWebRequest ToFile(string path)
        {
            timeout = 20;
            var handler = new DownloadHandlerFile(path);
            handler.removeFileOnAbort = true;
            downloadHandler = handler;
            return this;
        }

        public EditorWebRequest WithProgress(string title)
        {
            this.title = title;
            return this;
        }

        public void StartAsync(OnComplete complete)
        {
            this.complete = complete;
            Prepare();
            EditorApplication.update += Process;
        }

        public EditorWebRequest StartSync()
        {
            if (title == null)
                title = "CAS Wait for response";
            Prepare();
            while (!CheckDone()) { }
            return this;
        }

        public string ReadContent()
        {
            return isDone ? downloadHandler.text.Trim() : null;
        }

        private void Prepare()
        {
            if (downloadHandler == null)
                downloadHandler = new DownloadHandlerBuffer();
            SendWebRequest();
        }

        private void Process()
        {
            if (CheckDone())
                EditorApplication.update -= Process;
        }

        private bool CheckDone()
        {
            if (!isDone)
            {
                if (title == null)
                    return false;
                var totalBytes = GetResponseHeader("Content-Length");
                var message = string.IsNullOrEmpty(totalBytes) ? "Connecting..." : (downloadedBytes + "/" + totalBytes);
                if (!EditorUtility.DisplayCancelableProgressBar(title, message, downloadProgress))
                    return false;
            }
            EditorUtility.ClearProgressBar();

            if (complete != null)
                complete(this);
            return true;
        }
    }
}
