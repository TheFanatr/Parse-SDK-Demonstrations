using Parse;
using Parse.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.UIElements.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Assets.Code
{
    [RequireComponent(typeof(PanelRenderer)), RequireComponent(typeof(UIElementsEventSystem))]
    public class UIController : MonoBehaviour
    {
        [field: SerializeField]
        List<VisualTreeAsset> Interfaces { get; set; }

        [field: SerializeField]
        int ActiveUIIndex { get; set; }

        PanelRenderer Renderer { get; set; }

        UIElementsEventSystem UIElementEvents { get; set; }

        void OnEnable()
        {
            Renderer = gameObject.GetComponent<PanelRenderer>();

            UIElementEvents = gameObject.GetComponent<UIElementsEventSystem>();

            Renderer.enabled = true;
            UIElementEvents.enabled = true;
        }

        void Start() => Seek(ActiveUIIndex);

        TextInformationStore Store { get; set; }

        TextInformationStore FetchedStore { get; set; }

        void BindElements()
        {
            VisualElement visualTree = Renderer.visualTree;

            switch (ActiveUIIndex)
            {
                case 0:
                    TextField softwareID = visualTree.Q<TextField>("SoftwareID"), hostURI = visualTree.Q<TextField>("HostURI"), access = visualTree.Q<TextField>("Access"), handle = visualTree.Q<TextField>("Handle"), token = visualTree.Q<TextField>("Password");
                    Toggle freshAccount = visualTree.Q<Toggle>("Fresh");

                    if (ParseClient.Instance != default)
                    {
                        softwareID.value = ParseClient.Instance.ServerConnectionData.ApplicationID;
                        hostURI.value = ParseClient.Instance.ServerConnectionData.ServerURI;
                        access.value = ParseClient.Instance.ServerConnectionData.Key;
                    }

                    visualTree.Q<Button>("Save").clickable.clicked += () =>
                    {
                        new ParseClient(softwareID.value, hostURI.value, access.value, new LateInitializedMutableServiceHub { }, new MetadataMutator { EnvironmentData = new EnvironmentData { OSVersion = SystemInfo.operatingSystem, Platform = $"Unity {Application.unityVersion} on {SystemInfo.operatingSystemFamily}", TimeZone = TimeZoneInfo.Local.StandardName }, HostManifestData = new HostManifestData { Name = Application.productName, Identifier = Application.productName, ShortVersion = Application.version, Version = Application.version } }, new AbsoluteCacheLocationMutator { CustomAbsoluteCacheFilePath = $"{Application.persistentDataPath.Replace('/', Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}Parse.cache" }).Publicize();
                        ParseClient.Instance.AddValidClass<TextInformationStore>();
                    };

                    visualTree.Q<Button>("Continue").clickable.clicked += async () =>
                    {
                        if (freshAccount.value)
                        {
                            await ParseClient.Instance.SignUpAsync(handle.value, token.value);
                        }
                        else
                        {
                            await ParseClient.Instance.LogInAsync(handle.value, token.value);
                        }

                        Store = GenerateTextInformationStore(ParseClient.Instance.GetCurrentUser());
                        Proceed();
                    };
                    return;
                case 1:
                    ShowContextualData(showTextStoreData: false);
                    return;
                case 2:
                    ShowContextualData();
                    return;
                case 3:
                    ShowContextualData(attachMutationAndCheckToContinueButton: false);
                    visualTree.Q<Button>("Continue").clickable.clicked += async () =>
                    {
                        await Store.DeleteAsync();

                        if ((FetchedStore = await ParseClient.Instance.GetQuery<TextInformationStore>().WhereEqualTo("objectId", FetchedStore.ObjectId).FirstOrDefaultAsync()) != null)
                        {
                            throw new Exception("The object was not deleted.");
                        }

                        Proceed();
                    };
                    return;
                case 4:
                    visualTree.Q<Button>("Continue").clickable.clicked += () =>
                    {
                        Store = GenerateTextInformationStore(ParseClient.Instance.GetCurrentUser());
                        Seek(1);
                    };
                    visualTree.Q<Button>("Deauthenticate").clickable.clicked += async () =>
                    {
                        await ParseClient.Instance.LogOutAsync();
                        Seek(0);
                    };
                    return;
            }

            void ShowContextualData(bool showTextStoreData = true, bool attachMutationAndCheckToContinueButton = true)
            {
                visualTree.Q<Label>("SessionID").text = ParseClient.Instance.GetCurrentUser().SessionToken;

                if (showTextStoreData)
                {
                    visualTree.Q<Label>("LocalTextStoreData").text = Store.Data;
                    visualTree.Q<Label>("FetchedTextStoreData").text = FetchedStore.Data;
                }

                if (attachMutationAndCheckToContinueButton)
                {
                    TextField textData = visualTree.Q<TextField>("TextData");

                    visualTree.Q<Button>("Continue").clickable.clicked += () => MutateStoreAndCheckWithServer(textData.value);
                }
            }

            TextInformationStore GenerateTextInformationStore(ParseUser user) => new TextInformationStore { ACL = new ParseACL(ParseClient.Instance.GetCurrentUser()) };

            async void MutateStoreAndCheckWithServer(string value)
            {
                Store.Data = value;

                await Store.SaveAsync();
                FetchedStore = await ParseClient.Instance.GetQuery<TextInformationStore>().WhereEqualTo("objectId", Store.ObjectId).FirstAsync();

                Proceed();
            }
        }

        void Seek(int index)
        {
            Renderer.uxml = Interfaces[ActiveUIIndex = index];

            Renderer.RecreateUIFromUxml();
            BindElements();
        }

        void Proceed() => Seek(++ActiveUIIndex);
    }    
}
