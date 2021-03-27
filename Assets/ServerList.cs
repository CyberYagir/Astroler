using Mirror;
using Mirror.Discovery;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerList : MonoBehaviour
{

    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
    public NetworkDiscovery networkDiscovery;

    public Transform holder, item;
    // Start is called before the first frame update
    void Start()
    {
        networkDiscovery = FindObjectOfType<NetworkDiscovery>();
    }

    public void Refresh()
    {
        discoveredServers.Clear();
        networkDiscovery.StartDiscovery(); 
        
    }

    public void OnDiscoveredServer(ServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;
        foreach (Transform item in holder)
        {
            Destroy(item.gameObject);
        }
        foreach (ServerResponse inf in discoveredServers.Values)
        {
            var g = Instantiate(item, holder);
            g.GetComponentInChildren<TMP_Text>().text = inf.EndPoint.Address.ToString();
            g.gameObject.SetActive(true);
        }
           
    }
}
