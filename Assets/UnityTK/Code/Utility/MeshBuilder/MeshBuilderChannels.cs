using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK
{
	[System.Flags]
	public enum MeshBuilderChannels
	{
		INVALID = 0,
		NORMALS = 1,
		TANGENTS = 2,
		COLORS = 4,
		UV = 8,
		UV2 = 16,
		UV3 = 32,
		UV4 = 64,
		UV5 = 128,
		UV6 = 256,
		UV7 = 512,
		UV8 = 1024,

		ALLUV = UV | UV2 | UV3 | UV4 | UV5 | UV6 | UV7 | UV8
	}
	
	public enum MeshBuilderChannel : int
	{
		INVALID = 0,
		NORMALS = 1,
		TANGENTS = 2,
		COLORS = 3,
		UV = 4,
		UV2 = 5,
		UV3 = 6,
		UV4 = 7,
		UV5 = 8,
		UV6 = 9,
		UV7 = 10,
		UV8 = 11
	}

	public struct ChannelsEnumerable : IEnumerable<MeshBuilderChannel>
	{
		private MeshBuilderChannels channels;

		public ChannelsEnumerable(MeshBuilderChannels channels)
		{
			this.channels = channels;
		}

		public struct Enumerator : IEnumerator<MeshBuilderChannel>
		{
			private MeshBuilderChannels channels;
			private MeshBuilderChannel current;

			public Enumerator(MeshBuilderChannels channels)
			{
				this.channels = channels;
				this.current = MeshBuilderChannel.INVALID;
			}

			public MeshBuilderChannel Current
			{
				get
				{
					return this.current;
				}
			}

			object IEnumerator.Current => Current;

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				int p = (int)this.current;
				while (p < 11)
				{
					p++;
					var mask = (MeshBuilderChannels)(1 << (p-1));
					if ((this.channels & mask) > 0)
					{
						this.current = (MeshBuilderChannel)p;
						return true;
					}
				}

				return false;
			}

			public void Reset()
			{
				this.current = MeshBuilderChannel.INVALID;
			}
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this.channels);
		}

		IEnumerator<MeshBuilderChannel> IEnumerable<MeshBuilderChannel>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static class MeshBuilderChannelUtility
	{

		public static bool TryMapToUVIndex(this MeshBuilderChannel channel, out int index)
		{
			index = (int)channel - (int)MeshBuilderChannel.UV;
			return index >= 0;
		}

		public static bool HasAnyUV(this MeshBuilderChannels channels)
		{
			foreach (var channel in channels.AsEnumerable())
				if ((int)channel >= (int)MeshBuilderChannel.UV)
					return true;
			return false;
		}

		public static ChannelsEnumerable AsEnumerable(this MeshBuilderChannels channels)
		{
			return new ChannelsEnumerable(channels);
		}
	}
}
