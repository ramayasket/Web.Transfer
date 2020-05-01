using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kw.Common;

namespace model
{
	class Program
	{
		static void Main(string[] args)
		{
			const string PASSWORD = "Деремнустах!";

			var my_bytes =
				File.ReadAllBytes("C:\\1.jpg");
				//new byte[] { 0x08, 0x00, 0x52 };
				//new byte[] { 0x08, 0x00 };
				//new byte[] { 0x08 };
				//new byte[0];

			var my_confidential = my_bytes.ToConfidentialString();

			try
			{
				var try64 = Convert.FromBase64String(my_confidential);
				throw new InvalidOperationException("Confidential string is Base64-compatible");
			}
			catch (FormatException x)
			{
				Console.WriteLine("Confidential string is Base64-protected");
				File.WriteAllText("C:\\1.confidential", my_confidential);
			}

			var my_secret = RijndaelCrypting.Encrypt(my_confidential, PASSWORD);
			File.WriteAllBytes("C:\\1.secret", my_secret);

			var my_secret_text = my_secret.ToConfidentialString();
			File.WriteAllText("C:\\1.secret_text", my_secret_text);

			var my_unsecret = my_secret_text.FromConfidentialString();
			File.WriteAllBytes("C:\\1.unsecret", my_unsecret);

			var my_unconfidential = RijndaelCrypting.Decrypt(my_unsecret, PASSWORD);

			if(my_confidential == my_unconfidential)
				Console.WriteLine("Encrypt/Decrypt sequence is correct");
			else
				throw new InvalidOperationException("Encrypt/Decrypt sequence is incorrect");

			var my_restored = my_unconfidential.FromConfidentialString();
			File.WriteAllBytes("C:\\1.restored", my_restored);
		}
	}
}
