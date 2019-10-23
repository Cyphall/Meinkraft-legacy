using System;
using System.Runtime.InteropServices;

namespace Meinkraft
{
	public unsafe class NativeArray<T> : IDisposable where T : unmanaged
	{
		private T* _array;
		public uint size { get; }
		
		public NativeArray(uint size, T defaultValue)
		{
			this.size = size;
			_array = (T*) Marshal.AllocHGlobal(new IntPtr(sizeof(T) * size)).ToPointer();
			for (int i = 0; i < size; i++)
			{
				_array[i] = defaultValue;
			}
		}

		public T this[int i]
		{
			get
			{
				if (i >= size) throw new IndexOutOfRangeException();
				return _array[i];
			}
			set
			{
				if (i >= size) throw new IndexOutOfRangeException();
				_array[i] = value;
			}
		}
		
		public void Dispose()
		{
			Marshal.FreeHGlobal(new IntPtr(_array));
		}
	}
}