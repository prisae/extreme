using System;
using System.Numerics;
using Extreme.Cartesian.Green;
using Extreme.Cartesian.Green.Tensor;
using Extreme.Cartesian.Model;
using Extreme.Core;
using System.Runtime.InteropServices;
using System.Security;
using System.Collections.Generic;
using Extreme.Cartesian.Forward;
using Extreme.Core;
using Extreme.Core.Model;
using Extreme.Cartesian.Fft;
using Extreme.Parallel;
using Extreme.Cartesian.Logger;

namespace Extreme.Cartesian.Green
{
	[SuppressUnmanagedCodeSecurity]
	public static unsafe class Giem2gGreenTensor
	{
		

		public static GreenTensor CalcAtoATensor(ForwardSolver solver)
		{
			
			giem2g_set_logger (solver.Logger.WriteStatus);

			var giem2g_ie_op= new giem2g_data();
			var  bkg=new giem2g_background();
			var anomaly=new giem2g_anomaly();

			giem2g_ie_op.comm = solver.Mpi.CommunicatorC2Fortran ();
			int nz = solver.Model.Nz; 
			int nx = solver.Model.Nx; 
			int ny = solver.Model.Ny;

			var model = solver.Model;
			var section = model.Section1D;

			var n = section.NumberOfLayers;




			bkg.nl = n-1;
			var thick = solver.MemoryProvider.AllocateDouble (bkg.nl-1);
			var csigb = solver.MemoryProvider.AllocateComplex (bkg.nl+1);

			for (int i = 0; i < bkg.nl - 1; i++) 
				thick [i] = (double)section [i+1].Thickness;
			for (int i = 0; i < bkg.nl+1; i++) 
				csigb [i] = section [i].Zeta;
			
			bkg.thickness = new IntPtr(thick);
			bkg.csigb = new IntPtr (csigb);

			var z = solver.MemoryProvider.AllocateDouble (nz+1);
			var dz = solver.MemoryProvider.AllocateDouble (nz);

			for (int i = 0; i < nz; i++) {
				z [i] = (double)model.Anomaly.Layers [i].Depth;
				dz [i] =(double) model.Anomaly.Layers [i].Thickness;
			}
			
			z[nz]=z[nz-1]+ dz[nz-1];




			anomaly.nz = nz;
			// ATTENTION ! formal transpose in horizontal dimensions!!
			anomaly.nx = ny;
			anomaly.ny = nx;

			anomaly.z = new IntPtr (z);
			anomaly.dz = new IntPtr (dz);

			anomaly.dx =(double) model.LateralDimensions.CellSizeX;
			anomaly.dy =(double) model.LateralDimensions.CellSizeY;

			giem2g_calc_data_sizes(anomaly,ref giem2g_ie_op);

			var buff=FftBuffersPool.GetBuffer(model);

			if (buff.Plan3Nz.BufferLength == giem2g_ie_op.fft_buffers_length) {
				giem2g_ie_op.fft_buffer_in = buff.Plan3Nz.Buffer1Ptr;
				giem2g_ie_op.fft_buffer_out =buff.Plan3Nz.Buffer2Ptr;
				buff.Plan3Nz.
			} else {
				var len = giem2g_ie_op.fft_buffers_length;
				giem2g_ie_op.fft_buffer_in = solver.MemoryProvider.AllocateComplex (len);
				giem2g_ie_op.fft_buffer_out = solver.MemoryProvider.AllocateComplex (len);
				solver.Logger.WriteError("Allocate additinal memory for FFT inside GIEM2G!!");
			}
				

			var giem2g_ptrs = AllocateGiem2gDataBuffers (solver.MemoryProvider, ref giem2g_ie_op, nz);

			giem2g_prepare_ie_kernel( anomaly, giem2g_ie_op);

			var gt =GreenTensor.CreateGiem2gTensor(solver.MemoryProvider, nx, ny, nz, nz, giem2g_ptrs);


				


			var omega = model.Omega;

			giem2g_calc_ie_kernel( giem2g_ie_op.giem2g_tensor,bkg,  anomaly, omega);

			solver.MemoryProvider.Release(csigb);
			solver.MemoryProvider.Release(thick);
			solver.MemoryProvider.Release(z);
			solver.MemoryProvider.Release(dz);



			return gt;
		}
		public static void CalcFFTofGreenTensor(GreenTensor gt){
			
			IntPtr ie_op = new IntPtr (gt ["giem2g"].Ptr);
			giem2g_calc_fft_of_ie_kernel (ie_op);

		}


		public static void PrepareAnomalyConductivity(GreenTensor gt, Complex* csiga){

			IntPtr ie_op =new  IntPtr (gt ["giem2g"].Ptr);
			giem2g_set_anomaly_conductivity( ie_op,  csiga);
		}

		public static void Apply(GreenTensor gt,Complex* input, Complex* output){

			IntPtr ie_op =new IntPtr (gt ["giem2g"].Ptr);

			giem2g_apply_ie_operator( ie_op,  input, output);

		}

		private static List<IntPtr> AllocateGiem2gDataBuffers (INativeMemoryProvider memoryProvider, ref giem2g_data giem2g_ie_op, int nz)
		{
			var giem2g_ptrs = new List<IntPtr> ();


			//tmp = memoryProvider.AllocateBytes (giem2g_ie_op.tensor_size);
			var tmp1=giem2g_ie_op.giem2g_tensor;
			giem2g_ptrs.Add (tmp1);

		/*	tmp = new IntPtr (memoryProvider.AllocateDouble (nz));
			giem2g_ptrs.Add (tmp);
			giem2g_ie_op.dz = tmp;

			tmp = new IntPtr (memoryProvider.AllocateDouble (nz));
			giem2g_ptrs.Add (tmp);
			giem2g_ie_op.sqsigb = tmp;

			tmp = new IntPtr (memoryProvider.AllocateComplex (nz));
			giem2g_ptrs.Add (tmp);
			giem2g_ie_op.csigb = tmp;*/

			var tmp2 =  (memoryProvider.AllocateComplex (giem2g_ie_op.ie_kernel_buffer_length));
			giem2g_ptrs.Add (new IntPtr(tmp2));
			giem2g_ie_op.kernel_buffer = tmp2;







			return giem2g_ptrs;
		}

		private const string LibName = @"giem2g";


		[StructLayout(LayoutKind.Sequential)]
		private	struct giem2g_data
		{
			public long tensor_size;
			public long ie_kernel_buffer_length;
			public long fft_buffers_length;
			public long comm;
			public IntPtr giem2g_tensor;
			public IntPtr dz;
			public IntPtr sqsigb;
			public IntPtr csigb;
			public Complex* kernel_buffer;
			public Complex* fft_buffer_in;
			public Complex* fft_buffer_out;
		}


		[StructLayout(LayoutKind.Sequential)]
		private	struct giem2g_background
		{
			public int nl;
			public IntPtr csigb;
			public IntPtr thickness;
		}
		[StructLayout(LayoutKind.Sequential)]
		private	struct giem2g_anomaly
		{
			public int nx,ny,nz;
			public double dx,dy;
			public IntPtr z;
			public IntPtr dz;
		}



		private unsafe delegate void giem2g_logger(string str);



		[DllImport(LibName, EntryPoint = "giem2g_calc_data_sizes")]
		private static extern void  giem2g_calc_data_sizes( giem2g_anomaly anomaly,ref  giem2g_data giem2g_op) ;

		[DllImport(LibName, EntryPoint = "giem2g_prepare_ie_kernel")]
		private static extern void giem2g_prepare_ie_kernel( giem2g_anomaly anomaly, giem2g_data giem2g_ie_op);

		[DllImport(LibName, EntryPoint = "giem2g_calc_ie_kernel")]
		private static extern void giem2g_calc_ie_kernel(IntPtr giem2g_ie_op, giem2g_background bkg, giem2g_anomaly anomaly, double omega);

		[DllImport(LibName, EntryPoint = "giem2g_calc_fft_of_ie_kernel")]
		private static extern void giem2g_calc_fft_of_ie_kernel(IntPtr giem2g_ie_op);

		[DllImport(LibName, EntryPoint = "giem2g_set_anomaly_conductivity")]
		private static extern void giem2g_set_anomaly_conductivity( IntPtr giem2g_ie_op,  Complex* siga);

		[DllImport(LibName, EntryPoint = "giem2g_apply_ie_operator")]
		private static extern void giem2g_apply_ie_operator(IntPtr giem2g_ie_op, Complex* input,  Complex* output);

		[DllImport(LibName, EntryPoint = "giem2g_set_logger")]
		private static extern void giem2g_set_logger(giem2g_logger gl);
	}

}
