using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WeirdWallpaperGenerator.Services
{
    public class SecureRandomService
    {
        private readonly RNGCryptoServiceProvider _cryptoService;

        public SecureRandomService()
        {
            _cryptoService = new RNGCryptoServiceProvider();
        }

        public int Next(int? minValue = null, int? maxExclusiveValue = null)
        {
            if (!minValue.HasValue)
                minValue = int.MinValue;
            if (!maxExclusiveValue.HasValue)
                maxExclusiveValue = int.MaxValue;

            if (minValue.Value >= maxExclusiveValue.Value)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue"); // TODO ?

            long diff = (long)maxExclusiveValue.Value - minValue.Value;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);

            return (int)(minValue + (ui % diff));
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            byte[] buffer = new byte[bytesNumber];
            _cryptoService.GetBytes(buffer);
            return buffer;
        }
    }
}
