using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Web.Configuration;
using System.IO;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;
using Org.BouncyCastle.Utilities.IO;

namespace OB.BL.Operations.Helper.FTP
{
    public class PgpEncrypt
    {
        /// <summary>
        /// Read Public Key
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
       private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);
            foreach (PgpPublicKeyRing kRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey k in kRing.GetPublicKeys())
                {
                    if (k.IsEncryptionKey)
                        return k;
                }
            }

            throw new ArgumentException("Encryption key is Invalid.");
        }

        /// <summary>
        /// Find Private Key (Used when decrypting messages)
        /// </summary>
        /// <param name="pgpSec">private key</param>
        /// <param name="keyId"></param>
        /// <param name="pass">private key password string</param>
        /// <returns></returns>
        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
                return null;

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        /// <summary>
        /// Decrypt a stream
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="keyIn"></param>
        /// <param name="passCode"></param>
        /// <returns></returns>
        public static Stream Decrypt(Stream inputData, string keyInString, string passCode)
        {
            Stream inputStream = inputData;
            Stream keyIn = IoHelper.GetStream(keyInString);
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            MemoryStream decoded = new MemoryStream();

            try
            {
                PgpObjectFactory pgpF = new PgpObjectFactory(inputStream);
                PgpEncryptedDataList enc;
                PgpObject o = pgpF.NextPgpObject();

                // the first object might be a PGP marker packet.
                if (o is PgpEncryptedDataList)
                    enc = (PgpEncryptedDataList)o;
                else
                    enc = (PgpEncryptedDataList)pgpF.NextPgpObject();

                // find the secret key
                PgpPrivateKey sKey = null;
                PgpPublicKeyEncryptedData pbe = null;
                PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(
                PgpUtilities.GetDecoderStream(keyIn));
                foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
                {
                    sKey = FindSecretKey(pgpSec, pked.KeyId, passCode.ToCharArray());
                    if (sKey != null)
                    {
                        pbe = pked;
                        break;
                    }
                }
                if (sKey == null)
                    throw new ArgumentException("Private key or password is invalid");

                Stream clear = pbe.GetDataStream(sKey);
                PgpObjectFactory plainFact = new PgpObjectFactory(clear);
                PgpObject message = plainFact.NextPgpObject();

                if (message is PgpCompressedData)
                {
                    PgpCompressedData cData = (PgpCompressedData)message;
                    PgpObjectFactory pgpFact = new PgpObjectFactory(cData.GetDataStream());
                    message = pgpFact.NextPgpObject();
                }
                if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message;
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, decoded);
                }
                else if (message is PgpOnePassSignatureList)
                    throw new PgpException("encrypted message contains a signed message - not literal data.");
                else
                    throw new PgpException("message is not a simple encrypted file - type unknown.");

                return decoded;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Encrypt the data
        /// </summary>
        /// <param name="inputData">string to encrypt</param>
        /// <param name="passPhraseString">Public key to be use on encryption</param>
        /// <param name="withIntegrityCheck"></param>
        /// <param name="armor"></param>
        /// <returns>encrypted stream</returns>
        public static Stream Encrypt(string inputData, string passPhraseString, bool withIntegrityCheck, bool armor)
        {
            if (string.IsNullOrEmpty(passPhraseString))
                throw new ArgumentNullException("passPhraseString", "Parameter can't be NULL");

            var passPhrase = ReadPublicKey(IoHelper.GetStream(passPhraseString));
            byte[] processedData = Compress(inputData, PgpLiteralData.Console, CompressionAlgorithmTag.Zip);

            MemoryStream bOut = new MemoryStream();
            Stream output = bOut;

            if (armor)
                output = new ArmoredOutputStream(output);

            PgpEncryptedDataGenerator encGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
            encGen.AddMethod(passPhrase);

            Stream encOut = encGen.Open(output, processedData.Length);
            encOut.Write(processedData, 0, processedData.Length);
            encOut.Close();

            if (armor)
                output.Close();

            bOut.Seek(0, SeekOrigin.Begin);
            return bOut;
        }

        private static byte[] Compress(string clearDataString, string fileName, CompressionAlgorithmTag algorithm)
        {
            MemoryStream bOut = new MemoryStream();
            var clearData = IoHelper.GetBytes(clearDataString);

            PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(algorithm);
            Stream cos = comData.Open(bOut);
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();

            Stream pOut = lData.Open(
            cos,                    // the compressed output stream
            PgpLiteralData.Binary,
            fileName,               // "filename" to store
            clearData.Length,       // length of clear data
            DateTime.UtcNow         // current time
            );

            pOut.Write(clearData, 0, clearData.Length);
            pOut.Close();

            comData.Close();

            return bOut.ToArray();
        }
    }
}
