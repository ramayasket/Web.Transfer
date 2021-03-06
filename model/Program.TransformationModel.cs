using System;
using System.IO;
using System.Linq;
using System.Text;
using Web.Transfer.Base32;
using Web.Transfer.Crypto;
using Kw.Common;

namespace model
{
    partial class Program
    {
        /// <summary>
        /// Illustrates data transformation steps.
        /// </summary>
        /// <remarks>
        /// PREPARE DATA TO TRANSFER
        /// 1. Pack input bytes by Zip.
        /// 2. Encrypt packed bytes with password.
        /// 3. Convert encrypted bytes to confidential string.
        /// TRANSFER CONFIDENTIAL STRING OVER INTERNET.
        /// TRANSFORM TRANSFERRED STRING TO ORIGINAL DATA.
        /// 4. Convert confidential string to encrypted bytes.
        /// 5. Decrypt converted bytes with password.
        /// 6. Unpack decrypted bytes to original data.
        /// </remarks>
        internal static void TransformationModel()
        {
            //TestData.Cleanup();

            const string PASSWORD = "Деремнустах!";

            var my_input =
                File.ReadAllBytes("C:\\1.input");
            //new byte[] { 0x08, 0x00, 0x52 };
            //new byte[] { 0x08, 0x00 };
            //new byte[] { 0x08 };
            //new byte[0];

            var input_size = my_input.Length;
            Console.WriteLine($"Input is {input_size} bytes");

            ////
            //// Step 1: Pack input bytes by Zip.
            ////
            //var my_packed = my_input.Pack();
            //File.WriteAllBytes("C:\\1.packed", my_packed);

            ////
            //// Step 2: Encrypt packed bytes with password.
            ////
            var my_secret = RijndaelEncryptorDecryptor.Encrypt(my_input, PASSWORD);
            File.WriteAllBytes("C:\\1.secret", my_secret);

            string my_confidential = null;

            ////
            //// Step 3: Convert encrypted bytes to confidential string to be transferred.
            ////
            my_confidential = Base32Core.ToBase32String(my_secret);
            File.WriteAllText("C:\\1.confidential", my_confidential);

            ////
            //// TRANSFER CONFIDENTIAL STRING OVER INTERNET.
            //// TRANSFORM TRANSFERRED STRING TO ORIGINAL DATA.
            ////
            var my_transferred = my_confidential; // emulate transfer

            // ReSharper disable once AssignNullToNotNullAttribute
            var transfer_size = Encoding.UTF8.GetBytes(my_transferred).Length;

            Console.WriteLine($"Transfer is {transfer_size} bytes");
            Console.WriteLine($"Sample transfer ratio is {transfer_size / (1.0 * input_size)}");

            ////
            //// Step 4: Convert confidential string to encrypted bytes.
            ////
            var my_secret2 = Base32Core.FromBase32String(my_transferred);
            File.WriteAllBytes("C:\\1.secret2", my_secret2);

            Console.WriteLine($"Confidential conversion OK: {my_secret.SequenceEqual(my_secret2)}");

            ////
            //// Step 5: Decrypt converted bytes with password.
            ////
            var my_output = RijndaelEncryptorDecryptor.Decrypt(my_secret2, PASSWORD);

            ////
            //// Step 6: Unpack decrypted bytes to original data.
            ////
            File.WriteAllBytes("C:\\1.output", my_output);

            Console.WriteLine($"Encrypting/decrypting OK: {my_input.SequenceEqual(my_output)}");

        }
    }
}
