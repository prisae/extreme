//#include <mkl_service.h>
//#include <mkl_vml.h>
//#include <mkl.h>

//#include <iostream>
#include <complex>

#include "algebra.h"


#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

using namespace std;


namespace Native
{
	extern "C"
	{

		DllExport void Zscal(const long n, complex16 alpha, complex16* result)
		{
			cblas_zscal((blasint)n,(complex_ptr) &alpha, (complex_ptr) result, one_int);
		}

		DllExport void MultiplyElementwiseAndAddToResult(const long n, complex<double>* m1, complex<double>* m2, complex<double>* result)
		{
//#pragma simd
			for (long i = 0; i < n; i++)
				result[i] += m1[i] * m2[i];
		}

		DllExport void MultiplyElementwiseAndSubtractFromResult(const long n, complex<double>* m1, complex<double>* m2, complex<double>* result)
		{
//#pragma simd
			for (long i = 0; i < n; i++)
				result[i] -= m1[i] * m2[i];
		}


		DllExport void SetAllValuesTo(const long n, complex<double>* m, complex<double> value)
		{
#pragma simd
			for (long i = 0; i < n; i++)
				m[i] = value;
		}

		DllExport void AddToAll(const long n, complex<double>* m, complex<double> value)
		{
#pragma simd
			for (long i = 0; i < n; i++)
				m[i] += value;
		}

		DllExport void Copy(const long n, complex16* src, complex16* dst)
		{
			cblas_zcopy((blasint)n, (complex_ptr)src, one_int,(complex_ptr) dst, one_int);
		}

		DllExport void AddElementwise(const long n, complex16* m1, complex16* m2, complex16* result)
		{
			vzAdd(n, (complex_ptr)m1, (complex_ptr)m2, (complex_ptr)result);
		}

		DllExport void SubtractElementwise(const long n, complex16* m1, complex16* m2, complex16* result)
		{
			vzSub(n, (complex_ptr)m1, (complex_ptr)m2, (complex_ptr)result);
		}

		DllExport void MultiplyElementwise(const long n, complex16* m1, complex16* m2, complex16* result)
		{
			vzMul(n, (complex_ptr)m1, (complex_ptr)m2, (complex_ptr)result);
		}

		DllExport void CalculateAlphaX(const long n, complex16 alpha, complex16* result)
		{
			cblas_zscal((blasint)n,(complex_ptr) &alpha, (complex_ptr)result, one_int);
		}

		DllExport void CalculateDotProductConjugatedLeft(const long n, complex16* m1, complex16* m2, complex16* result)
		{
			cblas_zdotc_sub((blasint)n,(complex_ptr) m1, one_int, (complex_ptr)m2, one_int, (complex_ptr2)result);
		}

		DllExport void CalculateDotProductNotConjugated(const long n, complex16* m1, complex16* m2, complex16* result)
		{
			cblas_zdotu_sub((blasint)n, (complex_ptr)m1, one_int, (complex_ptr)m2, one_int,(complex_ptr2) result);
		}
	}
}
