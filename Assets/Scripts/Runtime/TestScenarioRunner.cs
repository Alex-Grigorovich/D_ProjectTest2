using System.Collections;
using UnityEngine;
using Mirror;
using NetworkMessaging.Messages;
using NetworkMessaging.Services;
using VContainer;
using VContainer.Unity;
using NetworkMessaging.DI;

namespace NetworkMessaging.Runtime
{
    public class TestScenarioRunner : MonoBehaviour
    {
        [SerializeField] private bool isServer = true;
        private NetworkMessageService _service;
        private IObjectResolver _resolver;

        private void Awake()
        {
            _resolver = LifetimeScope.Find<NetworkMessageInstaller>().Container;
            _service = _resolver.Resolve<NetworkMessageService>();
        }

        private IEnumerator Start()
        {
            if (isServer)
            {
                _service.Initialize();

                if (!NetworkServer.active)
                {
                    NetworkManager.singleton.StartServer();
                }

                yield return new WaitForSeconds(1f);

                float timeout = 30f;
                float startTime = Time.time;
                while (NetworkServer.connections.Count == 0 && (Time.time - startTime) < timeout)
                {
                    yield return null;
                }
                
                if (NetworkServer.connections.Count == 0)
                {
                    Debug.LogWarning("[Server] Timeout waiting for client");
                    yield break;
                }
            }
            else
            {
                _service.Initialize();

                yield return new WaitForSeconds(1f);

                if (NetworkManager.singleton == null)
                {
                    Debug.LogError("[Client] NetworkManager not found!");
                    yield break;
                }

                NetworkManager.singleton.StartClient();
                
                float timeout = 10f;
                float startTime = Time.time;
                while (!NetworkClient.isConnected && (Time.time - startTime) < timeout)
                {
                    yield return null;
                }
                
                if (NetworkClient.isConnected)
                {
                    _service.Subscribe<HelloMessage>();
                    yield return new WaitForSeconds(2f);
                }
                else
                {
                    Debug.LogError("[Client] Failed to connect to server");
                }
            }

            if (isServer && NetworkServer.connections.Count > 0)
            {
                yield return new WaitForSeconds(1f);
                
                var hello = new HelloMessage { Text = "Hello Client!" };
                _service.SendToSubscribers(hello);
            }
        }
    }
}