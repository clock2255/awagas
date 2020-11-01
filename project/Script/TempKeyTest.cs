using UnityEngine;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Atavism
{
    [System.Obsolete("Class Obsotete Use CoordProjectileEffect instead CoordObjectEffect", true)]
    public class TempKeyTest : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void GetKeys(string password)
        {
            EncryptString(password);
        }

        public string EncryptString(string inputString)
        {
            // TODO: Add Proper Exception Handlers
            byte[] bytes = Encoding.UTF32.GetBytes(inputString);
            // The hash function in use by the .NET RSACryptoServiceProvider here 
            // is SHA1
            // int maxLength = ( keySize ) - 2 - 
            //              ( 2 * SHA1.Create().ComputeHash( rawBytes ).Length );
            int maxLength = AtavismEncryption.rsa.KeySize - 42;
            int dataLength = bytes.Length;
            int iterations = dataLength / maxLength;
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i <= iterations; i++)
            {
                byte[] tempBytes = new byte[
                                            (dataLength - maxLength * i > maxLength) ? maxLength :
                                            dataLength - maxLength * i];
                Buffer.BlockCopy(bytes, maxLength * i, tempBytes, 0,
                                 tempBytes.Length);
                byte[] encryptedBytes = AtavismEncryption.rsa.Encrypt(tempBytes, true);
                // Be aware the RSACryptoServiceProvider reverses the order of 
                // encrypted bytes. It does this after encryption and before 
                // decryption. If you do not require compatibility with Microsoft 
                // Cryptographic API (CAPI) and/or other vendors. Comment out the 
                // next line and the corresponding one in the DecryptString function.
                Array.Reverse(encryptedBytes);
                // Why convert to base 64?
                // Because it is the largest power-of-two base printable using only 
                // ASCII characters
                stringBuilder.Append(Convert.ToBase64String(encryptedBytes));
            }
            return stringBuilder.ToString();
        }
    }
}