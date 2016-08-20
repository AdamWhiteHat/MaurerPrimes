using System;
using System.Numerics;
using System.Collections.Generic;

namespace AlgorithmLibrary
{
	public class TrialDivision
	{		
		public static BigInteger CheckForSmallComposites(int bits)
		{
			if (!(bits <= 20))
			{
				throw new ArgumentException("bits > 20");
			}

			Log.MethodEnter("TrialDivision.CheckForSmallComposites", bits);

			BigInteger result = 0;
			List<long> lucky = null;

			bool composite = true;
			while (composite)
			{
				long n = 1 << (bits - 1);

				for (int i = 0; i < bits - 1; i++)
				{
					n |= CryptoRandomSingleton.Next(2) << i;
				}

				long bound = (long)Math.Sqrt(n);

				lucky = Eratosthenes.Sieve(bound);
				composite = false;

				for (int i = 0; !composite && i < lucky.Count; i++)
				{
					composite = n % lucky[i] == 0;
				}

				if (!composite)
				{
					result = n;
				}
			}

			Log.MethodLeave();
			return result;
		}
	}
}
