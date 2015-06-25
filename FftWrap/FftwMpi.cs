using System;
using System.Security;
using System.Runtime.InteropServices;

namespace FftWrap
{
    [SuppressUnmanagedCodeSecurity]
    public static partial class FftwMpi
    {
        private const string NativeDllName = "libfftw3-3";
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_init")]
        public static extern void Init();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_cleanup")]
        public static extern void Cleanup();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_many_transposed")]
        public static extern IntPtr LocalSizeManyTransposed(int rnk, [In] IntPtr [ ]  n, IntPtr howMany, IntPtr block0, IntPtr block1, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start,  out  IntPtr  localN1,  out  IntPtr  local1Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_many")]
        public static extern IntPtr LocalSizeMany(int rnk, [In] IntPtr [ ]  n, IntPtr howMany, IntPtr block0, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_transposed")]
        public static extern IntPtr LocalSizeTransposed(int rnk, [In] IntPtr [ ]  n, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start,  out  IntPtr  localN1,  out  IntPtr  local1Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size")]
        public static extern IntPtr LocalSize(int rnk, [In] IntPtr [ ]  n, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_many_1d")]
        public static extern IntPtr LocalSizeMany1D(IntPtr n0, IntPtr howMany, IntPtr comm, int sign, uint flags,  out  IntPtr  localNi,  out  IntPtr  localIStart,  out  IntPtr  localNo,  out  IntPtr  localOStart);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_1d")]
        public static extern IntPtr LocalSize1D(IntPtr n0, IntPtr comm, int sign, uint flags,  out  IntPtr  localNi,  out  IntPtr  localIStart,  out  IntPtr  localNo,  out  IntPtr  localOStart);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_2d")]
        public static extern IntPtr LocalSize2D(IntPtr n0, IntPtr n1, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_2d_transposed")]
        public static extern IntPtr LocalSize2DTransposed(IntPtr n0, IntPtr n1, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start,  out  IntPtr  localN1,  out  IntPtr  local1Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_3d")]
        public static extern IntPtr LocalSize3D(IntPtr n0, IntPtr n1, IntPtr n2, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_local_size_3d_transposed")]
        public static extern IntPtr LocalSize3DTransposed(IntPtr n0, IntPtr n1, IntPtr n2, IntPtr comm,  out  IntPtr  localN0,  out  IntPtr  local0Start,  out  IntPtr  localN1,  out  IntPtr  local1Start);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_many_transpose")]
        public static extern IntPtr PlanManyTranspose(IntPtr n0, IntPtr n1, IntPtr howMany, IntPtr block0, IntPtr block1, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_transpose")]
        public static extern IntPtr PlanTranspose(IntPtr n0, IntPtr n1, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_many_dft")]
        public static extern IntPtr PlanManyDft(int rnk, [In] IntPtr [ ]  n, IntPtr howMany, IntPtr block, IntPtr tblock, IntPtr src, IntPtr dst, IntPtr comm, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft")]
        public static extern IntPtr PlanDft(int rnk, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, IntPtr comm, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_1d")]
        public static extern IntPtr PlanDft1D(IntPtr n0, IntPtr src, IntPtr dst, IntPtr comm, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_2d")]
        public static extern IntPtr PlanDft2D(IntPtr n0, IntPtr n1, IntPtr src, IntPtr dst, IntPtr comm, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_3d")]
        public static extern IntPtr PlanDft3D(IntPtr n0, IntPtr n1, IntPtr n2, IntPtr src, IntPtr dst, IntPtr comm, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_many_r2r")]
        public static extern IntPtr PlanManyR2r(int rnk, [In] IntPtr [ ]  n, IntPtr howMany, IntPtr iblock, IntPtr oblock, IntPtr src, IntPtr dst, IntPtr comm, [In] IntPtr [ ]  kind, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_r2r")]
        public static extern IntPtr PlanR2r(int rnk, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, IntPtr comm, [In] IntPtr [ ]  kind, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_r2r_2d")]
        public static extern IntPtr PlanR2r2D(IntPtr n0, IntPtr n1, IntPtr src, IntPtr dst, IntPtr comm, IntPtr kind0, IntPtr kind1, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_r2r_3d")]
        public static extern IntPtr PlanR2r3D(IntPtr n0, IntPtr n1, IntPtr n2, IntPtr src, IntPtr dst, IntPtr comm, IntPtr kind0, IntPtr kind1, IntPtr kind2, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_many_dft_r2c")]
        public static extern IntPtr PlanManyDftR2c(int rnk, [In] IntPtr [ ]  n, IntPtr howMany, IntPtr iblock, IntPtr oblock, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_r2c")]
        public static extern IntPtr PlanDftR2c(int rnk, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_r2c_2d")]
        public static extern IntPtr PlanDftR2c2D(IntPtr n0, IntPtr n1, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_r2c_3d")]
        public static extern IntPtr PlanDftR2c3D(IntPtr n0, IntPtr n1, IntPtr n2, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_many_dft_c2r")]
        public static extern IntPtr PlanManyDftC2r(int rnk, [In] IntPtr [ ]  n, IntPtr howMany, IntPtr iblock, IntPtr oblock, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_c2r")]
        public static extern IntPtr PlanDftC2r(int rnk, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_c2r_2d")]
        public static extern IntPtr PlanDftC2r2D(IntPtr n0, IntPtr n1, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_plan_dft_c2r_3d")]
        public static extern IntPtr PlanDftC2r3D(IntPtr n0, IntPtr n1, IntPtr n2, IntPtr src, IntPtr dst, IntPtr comm, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_gather_wisdom")]
        public static extern void GatherWisdom(IntPtr comm);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_broadcast_wisdom")]
        public static extern void BroadcastWisdom(IntPtr comm);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_execute_dft")]
        public static extern void ExecuteDft(IntPtr p, IntPtr src, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_execute_dft_r2c")]
        public static extern void ExecuteDftR2c(IntPtr p, IntPtr src, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_execute_dft_c2r")]
        public static extern void ExecuteDftC2r(IntPtr p, IntPtr src, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftw_mpi_execute_r2r")]
        public static extern void ExecuteR2r(IntPtr p, IntPtr src, IntPtr dst);
    }
}