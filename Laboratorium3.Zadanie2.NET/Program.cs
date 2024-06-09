using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace koszty_czasow.zuzanna_lukiewska
{
    public class Program
    {
        public static void Main()
        {
            var algorytmy = new List<SymmetricAlgorithm>
            {
                new AesCryptoServiceProvider { KeySize = 128 },
                new AesCryptoServiceProvider { KeySize = 256 },
                new AesManaged { KeySize = 128 },
                new AesManaged { KeySize = 256 },
                new RijndaelManaged { KeySize = 128 },
                new RijndaelManaged { KeySize = 256 },
                new DESCryptoServiceProvider(),
                new TripleDESCryptoServiceProvider()
            };

            var nazwyAlgorytmów = new List<string>
            {
                "AES (CSP) 128 bit",
                "AES (CSP) 256 bit",
                "AES Managed 128 bit",
                "AES Managed 256 bit",
                "Rindeal Managed 128 bit",
                "Rindeal Managed 256 bit",
                "DES 56 bit",
                "3DES 168 bit"
            };

            var rozmiarBlokuBajtów = 1024 * 1024;
            var dane = new byte[rozmiarBlokuBajtów];
            new Random().NextBytes(dane);

            Console.WriteLine("Algorytm\t\tCzas na blok (s)\tBajty/sek (RAM)\tBajty/sek (HDD)");

            for (var i = 0; i < algorytmy.Count; i++)
            {
                var algorytm = algorytmy[i];
                using var szyfrator = algorytm.CreateEncryptor();

                var stoperBlokowy = Stopwatch.StartNew();
                szyfrator.TransformFinalBlock(dane, 0, rozmiarBlokuBajtów);
                stoperBlokowy.Stop();
                var czasNaBlok = stoperBlokowy.Elapsed.TotalSeconds;

                var stoperPamięci = Stopwatch.StartNew();
                for (var j = 0; j < 100; j++)
                {
                    szyfrator.TransformFinalBlock(dane, 0, rozmiarBlokuBajtów);
                }
                stoperPamięci.Stop();
                var bajtyNaSekundęRam = 100 * rozmiarBlokuBajtów / stoperPamięci.Elapsed.TotalSeconds;

                const string ścieżkaPliku = "temp.dat";
                File.WriteAllBytes(ścieżkaPliku, dane);
                var stoperDyskowy = Stopwatch.StartNew();
                for (var j = 0; j < 10; j++)
                {
                    using var strumieńPlikowy = File.OpenRead(ścieżkaPliku);
                    using var strumieńSzyfrowania = new CryptoStream(strumieńPlikowy, szyfrator, CryptoStreamMode.Read);
                    using var strumieńPamięci = new MemoryStream();
                    strumieńSzyfrowania.CopyTo(strumieńPamięci);
                }
                stoperDyskowy.Stop();
                var bajtyNaSekundęHdd = 10 * rozmiarBlokuBajtów / stoperDyskowy.Elapsed.TotalSeconds;
                File.Delete(ścieżkaPliku);

                Console.WriteLine($"{nazwyAlgorytmów[i]}\t{czasNaBlok}\t\t{bajtyNaSekundęRam}\t\t{bajtyNaSekundęHdd}");
            }
        }
    }
}
