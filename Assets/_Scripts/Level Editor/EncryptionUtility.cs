using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;

public class EncryptionUtility : MonoBehaviour
{
    string magical = "8791234897";

    public string EncryptDecrypt(string data)
    {
        string result = "";

        for (int i = 0; i < data.Length; i++)
        {
            result += (char) (data[i] ^ magical[i % magical.Length]);
        }

        return result;
    }
}
