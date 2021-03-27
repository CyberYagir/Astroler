using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public TMP_Text serverip;
    public TMP_Text localip;
    public TMP_Text v6ip;
    public GameObject disconnect;
    private void Start()
    {
    }
    private void Update()
    {
        disconnect.SetActive(FindObjectOfType<Ship>() == null);
        serverip.text = "Server IP: " + NetworkManager.singleton.networkAddress;
        localip.text = "Local IP: " + IPManager.GetIP(ADDRESSFAM.IPv4);
        v6ip.text = "IPv6: " + IPManager.GetIP(ADDRESSFAM.IPv6);
        //Click to copy IPv4 to connect";
    }
    public void Disconnect()
    {
        var manager = FindObjectOfType<NetworkManager>();
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            FindObjectOfType<LoaderResources>().SaveWorld();
            manager.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            manager.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            manager.StopServer();
        }
    }

    public void CopyIPGlobal()
    {
        GUIUtility.systemCopyBuffer = NetworkManager.singleton.networkAddress;
    }
    public void CopyIPv4Local()
    {
        GUIUtility.systemCopyBuffer = IPManager.GetIP(ADDRESSFAM.IPv4);
    }
    public void CopyIPv6Local()
    {
        GUIUtility.systemCopyBuffer = IPManager.GetIP(ADDRESSFAM.IPv6);
    }
}


public class IPManager
{
    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }
}

public enum ADDRESSFAM
{
    IPv4, IPv6
}
