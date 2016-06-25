//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
using System;
using System.Security;
using System.Runtime.InteropServices;

namespace FftWrap
{
    [SuppressUnmanagedCodeSecurity]
    public static partial class Fftwf
    {
        private const string NativeDllName = "libfftw3f-3";
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_set_planner_hooks")]
        public static extern void SetPlannerHooks(IntPtr before, IntPtr after);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute")]
        public static extern void Execute(IntPtr p);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft")]
        public static extern IntPtr PlanDft(int rank, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_1d")]
        public static extern IntPtr PlanDft1D(int n, IntPtr src, IntPtr dst, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_2d")]
        public static extern IntPtr PlanDft2D(int n0, int n1, IntPtr src, IntPtr dst, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_3d")]
        public static extern IntPtr PlanDft3D(int n0, int n1, int n2, IntPtr src, IntPtr dst, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_many_dft")]
        public static extern IntPtr PlanManyDft(int rank, [In] IntPtr [ ]  n, int howMany, IntPtr src, [In] IntPtr [ ]  inembed, int istride, int idist, IntPtr dst, [In] IntPtr [ ]  onembed, int ostride, int odist, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru_dft")]
        public static extern IntPtr PlanGuruDft(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru_split_dft")]
        public static extern IntPtr PlanGuruSplitDft(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr ri, IntPtr ii, IntPtr ro, IntPtr io, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru64_dft")]
        public static extern IntPtr PlanGuru64Dft(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, int sign, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru64_split_dft")]
        public static extern IntPtr PlanGuru64SplitDft(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr ri, IntPtr ii, IntPtr ro, IntPtr io, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute_dft")]
        public static extern void ExecuteDft(IntPtr p, IntPtr src, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute_split_dft")]
        public static extern void ExecuteSplitDft(IntPtr p, IntPtr ri, IntPtr ii, IntPtr ro, IntPtr io);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_many_dft_r2c")]
        public static extern IntPtr PlanManyDftR2c(int rank, [In] IntPtr [ ]  n, int howMany, IntPtr src, [In] IntPtr [ ]  inembed, int istride, int idist, IntPtr dst, [In] IntPtr [ ]  onembed, int ostride, int odist, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_r2c")]
        public static extern IntPtr PlanDftR2c(int rank, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_r2c_1d")]
        public static extern IntPtr PlanDftR2c1D(int n, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_r2c_2d")]
        public static extern IntPtr PlanDftR2c2D(int n0, int n1, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_r2c_3d")]
        public static extern IntPtr PlanDftR2c3D(int n0, int n1, int n2, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_many_dft_c2r")]
        public static extern IntPtr PlanManyDftC2r(int rank, [In] IntPtr [ ]  n, int howMany, IntPtr src, [In] IntPtr [ ]  inembed, int istride, int idist, IntPtr dst, [In] IntPtr [ ]  onembed, int ostride, int odist, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_c2r")]
        public static extern IntPtr PlanDftC2r(int rank, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_c2r_1d")]
        public static extern IntPtr PlanDftC2r1D(int n, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_c2r_2d")]
        public static extern IntPtr PlanDftC2r2D(int n0, int n1, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_dft_c2r_3d")]
        public static extern IntPtr PlanDftC2r3D(int n0, int n1, int n2, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru_dft_r2c")]
        public static extern IntPtr PlanGuruDftR2c(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru_dft_c2r")]
        public static extern IntPtr PlanGuruDftC2r(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru_split_dft_r2c")]
        public static extern IntPtr PlanGuruSplitDftR2c(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr ro, IntPtr io, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru_split_dft_c2r")]
        public static extern IntPtr PlanGuruSplitDftC2r(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr ri, IntPtr ii, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru64_dft_r2c")]
        public static extern IntPtr PlanGuru64DftR2c(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru64_dft_c2r")]
        public static extern IntPtr PlanGuru64DftC2r(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru64_split_dft_r2c")]
        public static extern IntPtr PlanGuru64SplitDftR2c(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr ro, IntPtr io, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru64_split_dft_c2r")]
        public static extern IntPtr PlanGuru64SplitDftC2r(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr ri, IntPtr ii, IntPtr dst, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute_dft_r2c")]
        public static extern void ExecuteDftR2c(IntPtr p, IntPtr src, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute_dft_c2r")]
        public static extern void ExecuteDftC2r(IntPtr p, IntPtr src, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute_split_dft_r2c")]
        public static extern void ExecuteSplitDftR2c(IntPtr p, IntPtr src, IntPtr ro, IntPtr io);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute_split_dft_c2r")]
        public static extern void ExecuteSplitDftC2r(IntPtr p, IntPtr ri, IntPtr ii, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_many_r2r")]
        public static extern IntPtr PlanManyR2r(int rank, [In] IntPtr [ ]  n, int howMany, IntPtr src, [In] IntPtr [ ]  inembed, int istride, int idist, IntPtr dst, [In] IntPtr [ ]  onembed, int ostride, int odist, [In] IntPtr [ ]  kind, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_r2r")]
        public static extern IntPtr PlanR2r(int rank, [In] IntPtr [ ]  n, IntPtr src, IntPtr dst, [In] IntPtr [ ]  kind, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_r2r_1d")]
        public static extern IntPtr PlanR2r1D(int n, IntPtr src, IntPtr dst, IntPtr kind, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_r2r_2d")]
        public static extern IntPtr PlanR2r2D(int n0, int n1, IntPtr src, IntPtr dst, IntPtr kind0, IntPtr kind1, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_r2r_3d")]
        public static extern IntPtr PlanR2r3D(int n0, int n1, int n2, IntPtr src, IntPtr dst, IntPtr kind0, IntPtr kind1, IntPtr kind2, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru_r2r")]
        public static extern IntPtr PlanGuruR2r(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, [In] IntPtr [ ]  kind, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_guru64_r2r")]
        public static extern IntPtr PlanGuru64R2r(int rank, [In] IntPtr [ ]  dims, int howManyRank, [In] IntPtr [ ]  howManyDims, IntPtr src, IntPtr dst, [In] IntPtr [ ]  kind, uint flags);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_execute_r2r")]
        public static extern void ExecuteR2r(IntPtr p, IntPtr src, IntPtr dst);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_destroy_plan")]
        public static extern void DestroyPlan(IntPtr p);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_forget_wisdom")]
        public static extern void ForgetWisdom();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_cleanup")]
        public static extern void Cleanup();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_set_timelimit")]
        public static extern void SetTimelimit(double t);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_plan_with_nthreads")]
        public static extern void PlanWithNthreads(int nthreads);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_init_threads")]
        public static extern int InitThreads();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_cleanup_threads")]
        public static extern void CleanupThreads();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_export_wisdom_to_filename")]
        public static extern int ExportWisdomToFilename([In] IntPtr [ ]  filename);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_export_wisdom_to_file")]
        public static extern void ExportWisdomToFile(IntPtr outputFile);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_export_wisdom_to_string")]
        public static extern IntPtr ExportWisdomToString();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_export_wisdom")]
        public static extern void ExportWisdom(IntPtr writeChar, IntPtr data);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_import_system_wisdom")]
        public static extern int ImportSystemWisdom();
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_import_wisdom_from_filename")]
        public static extern int ImportWisdomFromFilename([In] IntPtr [ ]  filename);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_import_wisdom_from_file")]
        public static extern int ImportWisdomFromFile(IntPtr inputFile);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_import_wisdom_from_string")]
        public static extern int ImportWisdomFromString([In] IntPtr [ ]  inputString);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_import_wisdom")]
        public static extern int ImportWisdom(IntPtr readChar, IntPtr data);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_fprint_plan")]
        public static extern void FprintPlan(IntPtr p, IntPtr outputFile);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_print_plan")]
        public static extern void PrintPlan(IntPtr p);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_sprint_plan")]
        public static extern IntPtr SprintPlan(IntPtr p);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_malloc")]
        public static extern IntPtr Malloc(IntPtr n);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_alloc_real")]
        public static extern IntPtr AllocReal(IntPtr n);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_alloc_complex")]
        public static extern IntPtr AllocComplex(IntPtr n);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_free")]
        public static extern void Free(IntPtr p);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_flops")]
        public static extern void Flops(IntPtr p, IntPtr add, IntPtr mul, IntPtr fmas);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_estimate_cost")]
        public static extern double EstimateCost(IntPtr p);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_cost")]
        public static extern double Cost(IntPtr p);
        [DllImport(NativeDllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "fftwf_alignment_of")]
        public static extern int AlignmentOf(IntPtr p);
    }
}