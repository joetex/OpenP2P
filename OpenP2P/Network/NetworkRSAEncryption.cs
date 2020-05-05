using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OpenP2P
{
    public class NetworkRSAEncryption
    {

        public const int KEYSIZE = 736;

        public void Test()
        {
       
            // Create an instance of the RSA algorithm class  
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEYSIZE);

            // Get the public keyy   
            string publicKey = rsa.ToXmlString(false); // false to get the public key   
            string privateKey = rsa.ToXmlString(true); // true to get the private key   

            Console.WriteLine("Public Key: " + publicKey);
            Console.WriteLine("Private Key: " + privateKey);

            string base64Decoded;
            byte[] data = System.Convert.FromBase64String(publicKey.Replace("<RSAKeyValue><Modulus>", "").Replace("</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>", ""));
            base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(data);
            Console.WriteLine("Public key decoded: " + base64Decoded);
            // Call the encryptText method   
            string text = "0123456789012345678901234567890123456789";

            byte[] encryptedBytes = Encrypt(publicKey, text);
            Console.WriteLine("Decrypted Byte Size: " + text.Length);
            Console.WriteLine("Encrypted Byte Size: " +  encryptedBytes.Length);

            UnicodeEncoding byteConverter = new UnicodeEncoding();

            byte[] decryptedBytes = Decrypt(privateKey, encryptedBytes);
            string decryptedStr = byteConverter.GetString(decryptedBytes); 
            Console.WriteLine("Decrypted message: {0}", decryptedStr);
        }

        // Create a method to encrypt a text and save it to a specific file using a RSA algorithm public key   
        public byte[] Encrypt(string publicKey, string text)
        {
            // Convert the text to an array of bytes   
            UnicodeEncoding byteConverter = new UnicodeEncoding();
            byte[] dataToEncrypt = byteConverter.GetBytes(text);

            // Create a byte array to store the encrypted data in it   
            byte[] encryptedData;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEYSIZE))
            {
                // Set the rsa pulic key   
                rsa.FromXmlString(publicKey);

                // Encrypt the data and store it in the encyptedData Array   
                encryptedData = rsa.Encrypt(dataToEncrypt, false);
            }
            // Save the encypted data array into a file   
            //File.WriteAllBytes(fileName, encryptedData);

            Console.WriteLine("Data has been encrypted");
            return encryptedData;
        }

        // Method to decrypt the data withing a specific file using a RSA algorithm private key   
        public byte[] Decrypt(string privateKey, byte[] dataToDecrypt)
        { 
            // Create an array to store the decrypted data in it   
            byte[] decryptedData;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                // Set the private key of the algorithm   
                rsa.FromXmlString(privateKey);
                decryptedData = rsa.Decrypt(dataToDecrypt, false);
            }
            
            return decryptedData;
        }
    }
}