//
// System.__ComObject
//
// Authors:
//   Sebastien Pouliot <sebastien@ximian.com>
//   Korn�l P�l <http://www.kornelpal.hu/>
//   Jonathan Chambers <joncham@gmail.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
// Copyright (C) 2005 Korn�l P�l
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System
{
	// This is a private class that is used as a generic wrapper class
	// for COM objects that have no specific wrapper class.
	//
	// It has no public methods, it's functionality is exposed trough
	// System.Runtime.InteropServices.Marshal class and can be casted to
	// any interface that is implemented by the wrapped COM object.
	//
	// This class is referenced in .NET Framework SDK Documentation so
	// many times that obj.GetType().FullName == "System.__ComObject" and
	// Type.GetType("System.__ComObject") may be used.

	internal class __ComObject : MarshalByRefObject
	{
		#region Sync with object-internals.h
		IntPtr hash_table;
		#endregion

		// this is used internally and for the the methods
		// Marshal.GetComObjectData and Marshal.SetComObjectData
		Hashtable hashtable;

		[ThreadStatic]
		static bool coinitialized;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern void Finalizer ();

		~__ComObject ()
		{
			Finalizer ();
		}

		public __ComObject ()
		{
			// call CoInitialize once per thread
			if (!coinitialized) {
				CoInitialize (IntPtr.Zero);
				coinitialized = true;
			}

			hashtable = new Hashtable ();

			IntPtr ppv;
			int hr = CoCreateInstance (GetType ().GUID, IntPtr.Zero, 0x1 | 0x4 | 0x10, IID_IUnknown, out ppv);
			Marshal.ThrowExceptionForHR (hr);

			SetIUnknown (ppv);
		}

        public __ComObject (IntPtr pItf)
        {
            hashtable = new Hashtable ();

            IntPtr ppv;
            Guid iid = IID_IUnknown;
			int hr = Marshal.QueryInterface (pItf, ref iid, out ppv);
			Marshal.ThrowExceptionForHR (hr);

			SetIUnknown (ppv);
        }

		public Hashtable Hashtable
		{
			get
			{
				return hashtable;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern __ComObject CreateRCW (Type t);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern void SetIUnknown (IntPtr t);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern IntPtr GetIUnknown ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern IntPtr FindInterface (Type t);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern void CacheInterface (Type t, IntPtr pItf);

		internal IntPtr GetInterface(Type t)
		{
			IntPtr pItf = FindInterface (t);
			if (pItf != IntPtr.Zero) {
				return pItf;
			}

			Guid iid = t.GUID;
			IntPtr ppv;
			int hr = Marshal.QueryInterface (GetIUnknown(), ref iid, out ppv);
			Marshal.ThrowExceptionForHR (hr);
			CacheInterface (t, ppv);
			return ppv;
		}

		internal IntPtr IUnknown
		{
			get
			{
				return GetIUnknown();
			}
		}

		internal static Guid IID_IUnknown
		{
			get
			{
				return new Guid("00000000-0000-0000-C000-000000000046");
			}
		}

		internal static Guid IID_IDispatch
		{
			get
			{
				return new Guid ("00020400-0000-0000-C000-000000000046");
			}
		}

		[DllImport ("ole32.dll", CallingConvention = CallingConvention.StdCall)]
		static extern int CoInitialize (IntPtr pvReserved);

		[DllImport ("ole32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, PreserveSig = true)]
		static extern int CoCreateInstance (
		   [In, MarshalAs (UnmanagedType.LPStruct)] Guid rclsid,
		   IntPtr pUnkOuter,
		   uint dwClsContext,
		  [In, MarshalAs (UnmanagedType.LPStruct)] Guid riid,
			out IntPtr pUnk);
	}
}
