using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

namespace Alpaca.Utilities
{
    static public class Functions
    {
        static public string ByteArrayToHexString(byte[] array)
        {
            return ByteArrayToHexString(array, 0, array.Length);
        }
        static public string ByteArrayToHexString(byte[] array, int startIndex, int count)
        {
            if (array.Length == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            for (int i = startIndex; i < startIndex + count; ++i)
            {
                sb.Append(array[i].ToString("x2"));
            }
            return sb.ToString();
        }
        static public int GetLineNumber([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = -1)
        {
            return lineNumber;
        }
        static public string GetFunctionName([System.Runtime.CompilerServices.CallerMemberName] string functionName = "")
        {
            return functionName;
        }
        static public long LocalTicksToUnixTime(long ticks)
        {
            var dt = new DateTime(ticks);
            var utc = TimeZone.CurrentTimeZone.ToUniversalTime(dt);
            var delta = utc.Subtract(new DateTime(1970, 1, 1));
            return delta.Ticks / 10000 / 1000;
        }
        static public long UnixTimeToLocalTicks(long unixTime)
        {
            var utc = new DateTime(1970, 1, 1);
            utc = utc.AddTicks(unixTime * 10000 * 1000);
            var dt = TimeZone.CurrentTimeZone.ToLocalTime(utc);
            return dt.Ticks;
        }
        static public long DoubleIntToLong(int x, int y)
        {
            return ((long)((uint)x) << 32) | (uint)y;
        }
        static public int[] LongToDoubleInt(long x)
        {
            return new int[2] { (int)((uint)(((ulong)x & 0xffffffff00000000) >> 32)), (int)((uint)(x & 0x00000000ffffffff)) };
        }

        static public string GenerateRandomString(int digits)
        {
            if (digits < 1 || digits > 9)
            {
                throw new ArgumentOutOfRangeException("not support so many digits");
            }

            var characters = new char[] 
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
                'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
                'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D',
                'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N',
                'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
                'Y', 'Z',
            };

            var sb = new StringBuilder();
            for (int i = 0; i < digits; ++i)
            {
                sb.Append(characters[ThreadSafeRandom.Rand.Next(characters.Length)]);
            }

            return sb.ToString();
        }
        static public List<object> SScanf(string text, string format, bool valueTrim = false, string leftMark = "{", string rightMark = "}")
        {
            var formatSearchIndex = 0;
            var formatBeginNone = false;
            var formatEndNone = false;

            var splittedFormats = new List<string>();
            var types = new List<string>();
            var values = new List<string>();
            while (true)
            {
                var left = format.IndexOf(leftMark, formatSearchIndex);
                var right = format.IndexOf(rightMark, formatSearchIndex);
                if (left < 0 || right < 0)
                {
                    if (!formatEndNone)
                    {
                        splittedFormats.Add(format.Substring(formatSearchIndex, format.Length - formatSearchIndex));
                    }
                    break;
                }

                if (!formatBeginNone && left == 0)
                {
                    formatBeginNone = true;
                }
                if (!formatEndNone && right + 1 == format.Length)
                {
                    formatEndNone = true;
                }

                var type = format.Substring(left + leftMark.Length, right - left - 1);
                types.Add(type);

                if (left != formatSearchIndex)
                {
                    splittedFormats.Add(format.Substring(formatSearchIndex, left - formatSearchIndex));
                }

                formatSearchIndex = right + rightMark.Length;
            }

            var textSearchIndex = formatBeginNone ? 0 : text.IndexOf(splittedFormats[0]) + splittedFormats[0].Length;
            var splittedFormatsIndex = formatBeginNone ? 0 : 1;
            while (true)
            {
                if (splittedFormatsIndex == splittedFormats.Count)
                {
                    if (formatEndNone)
                    {
                        values.Add(text.Substring(textSearchIndex, text.Length - textSearchIndex));
                    }
                    break;
                }
                else
                {
                    var splittedFormat = splittedFormats[splittedFormatsIndex];
                    var index = text.IndexOf(splittedFormat, textSearchIndex);
                    var param = text.Substring(textSearchIndex, index - textSearchIndex);
                    values.Add(param);

                    textSearchIndex = index + splittedFormat.Length;
                    ++splittedFormatsIndex;
                }
            }

            var objects = new List<object>(values.Count);
            for (int i = 0; i < values.Count; ++i)
            {
                switch (types[i])
                {
                    case "d":
                        objects.Add(int.Parse(valueTrim ? values[i].Trim() : values[i]));
                        break;
                    case "f":
                        objects.Add(float.Parse(valueTrim ? values[i].Trim() : values[i]));
                        break;
                    case "s":
                        objects.Add(valueTrim ? values[i].Trim() : values[i]);
                        break;
                    case "ld":
                        objects.Add(long.Parse(valueTrim ? values[i].Trim() : values[i]));
                        break;
                    case "lf":
                        objects.Add(double.Parse(valueTrim ? values[i].Trim() : values[i]));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return objects;
        }
        static public byte[] GetMD5Hash(byte[] input)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(input);
            }
        }
        static public string GetMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.Unicode.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var d in data)
                {
                    sb.Append(d.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }

    static public class InstanceId
    {
        static private long _id = 0;
        static public long NewId
        {
            get { return Interlocked.Increment(ref _id); }
        }
    }

    static public class AutoLock
    {
        public class AutoReaderLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock = null;
            public AutoReaderLock(ReaderWriterLockSlim rwlock)
            {
                this._lock = rwlock;
            }
            public void Dispose()
            {
                _lock.ExitReadLock();
            }
        }
        public class AutoWriterLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock = null;
            public AutoWriterLock(ReaderWriterLockSlim rwlock)
            {
                this._lock = rwlock;
            }
            public void Dispose()
            {
                _lock.ExitWriteLock();
            }
        }
        public class AutoAsyncLock : IDisposable
        {
            private readonly SemaphoreSlim _semaphore = null;
            public AutoAsyncLock(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }
            public void Dispose()
            {
                _semaphore.Release();
            }
        }
        static public AutoReaderLock EnterReadLock(ReaderWriterLockSlim rwlock)
        {
            var l = new AutoReaderLock(rwlock);
            rwlock.EnterReadLock();
            return l;
        }
        static public AutoWriterLock EnterWriteLock(ReaderWriterLockSlim rwlock)
        {
            var l = new AutoWriterLock(rwlock);
            rwlock.EnterWriteLock();
            return l;
        }
        static public async Task<AutoAsyncLock> LockAsync(SemaphoreSlim semaphore)
        {
            var l = new AutoAsyncLock(semaphore);
            await semaphore.WaitAsync().ConfigureAwait(false);
            return l;
        }
    }

    static public class ThreadSafeRandom
    {
        [ThreadStatic]
        static private Random _rand;
        static private readonly object _lock = new object();
        static private readonly RNGCryptoServiceProvider _seedProvider = new RNGCryptoServiceProvider();
        static public Random Rand
        {
            get
            {
                if (_rand != null)
                {
                    return _rand;
                }
                lock (_lock)
                {
                    if (_rand != null)
                    {
                        return _rand;
                    }

                    var seed = new byte[4];
                    _seedProvider.GetBytes(seed);
                    _rand = new Random(BitConverter.ToInt32(seed, 0));
                    return _rand;
                }
            }
        }
    }
}
